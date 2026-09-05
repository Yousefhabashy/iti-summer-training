using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InventoryManagementSystem.Chatbot;

public class GeminiChatbotService
{
    private readonly HttpClient _http;
    private readonly InventoryQueryFunctions _functions;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiChatbotService(HttpClient http, InventoryQueryFunctions functions, IConfiguration config)
    {
        _http = http;
        _functions = functions;
        _apiKey = config["Gemini:ApiKey"]!;
        _model = config["Gemini:Model"] ?? "gemini-3.6-flash";
    }

    public async Task<string> AskAsync(string userMessage)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";

        var requestBody = new JsonObject
        {
            ["contents"] = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["parts"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["text"] = "Answer in plain text only. Do not use markdown, asterisks, bullet points, numbered lists, or tables. " +
                                   "If the answer involves multiple items, put each item on its own separate line as a plain sentence " +
                                   "(e.g. 'Mechanical Keyboard - Electronics - Stock: 3 - Price: $900.00'), with no bold or special symbols. " +
                                   "Keep it short and conversational.\n\nUser question: " + userMessage
                    }
                }
            }
        },
            ["tools"] = new JsonArray
        {
            new JsonObject { ["functionDeclarations"] = GeminiFunctionDeclarations.GetDeclarations() }
        },
            // Forces Gemini 3.x to actually attach the thoughtSignature on functionCall parts
            ["generationConfig"] = new JsonObject
            {
                ["thinkingConfig"] = new JsonObject { ["includeThoughts"] = true }
            }
        };

        var firstResponse = await CallGemini(url, requestBody);

        // Take the FULL content object (all parts), not just parts[0]
        var modelContent = firstResponse?["candidates"]?[0]?["content"] as JsonObject;
        var parts = modelContent?["parts"] as JsonArray;

        // Find the functionCall part (skip any "thought" parts)
        JsonObject? functionCallPart = null;
        foreach (var part in parts ?? new JsonArray())
        {
            if (part is JsonObject po && po.ContainsKey("functionCall"))
            {
                functionCallPart = po;
                break;
            }
        }

        if (functionCallPart == null)
        {
            // No function matched -> return the model's real (non-thought) text reply, or the fallback
            var directTextPart = parts?.FirstOrDefault(p =>
                p is JsonObject po
                && po.ContainsKey("text")
                && po["thought"]?.GetValue<bool>() != true) as JsonObject;

            return directTextPart?["text"]?.ToString()
                ?? "I can't reach that information right now. This feature may be added in the future.";
        }

        string functionName = functionCallPart["functionCall"]!["name"]!.ToString();
        var args = functionCallPart["functionCall"]!["args"] as JsonObject ?? new JsonObject();
        object? result = ExecuteFunction(functionName, args);

        // Resend the ENTIRE model content (all parts, preserving any thought parts + signatures) exactly as received
        requestBody["contents"]!.AsArray().Add(new JsonObject
        {
            ["role"] = "model",
            ["parts"] = modelContent!["parts"]!.DeepClone()
        });

        requestBody["contents"]!.AsArray().Add(new JsonObject
        {
            ["role"] = "user",
            ["parts"] = new JsonArray
        {
            new JsonObject
            {
                ["functionResponse"] = new JsonObject
                {
                    ["name"] = functionName,
                    ["response"] = new JsonObject { ["result"] = JsonSerializer.Serialize(result) }
                }
            }
        }
        });

        var finalResponse = await CallGemini(url, requestBody);
        var finalParts = finalResponse?["candidates"]?[0]?["content"]?["parts"] as JsonArray;

        // Ignore any "thought" part (internal reasoning), return the real reply text only
        var finalTextPart = finalParts?.FirstOrDefault(p =>
            p is JsonObject po
            && po.ContainsKey("text")
            && po["thought"]?.GetValue<bool>() != true) as JsonObject;

        return finalTextPart?["text"]?.ToString()
            ?? "Something went wrong while preparing the response.";
    }

    private async Task<JsonObject?> CallGemini(string url, JsonObject body)
    {
        var content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
        request.Headers.Add("x-goog-api-key", _apiKey);

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Gemini API error: {response.StatusCode} - {json}");

        return JsonNode.Parse(json) as JsonObject;
    }

    private object? ExecuteFunction(string name, JsonObject args)
    {
        string? GetStr(string key) => args.ContainsKey(key) ? args[key]!.ToString() : null;
        int? GetInt(string key) => args.ContainsKey(key) ? int.Parse(args[key]!.ToString()) : null;
        DateTime? GetDate(string key) => args.ContainsKey(key) && DateTime.TryParse(args[key]!.ToString(), out var d) ? d : null;

        return name switch
        {
            "GetLowStockProducts" => _functions.GetLowStockProducts(),
            "GetProductByName" => _functions.GetProductByName(GetStr("productName") ?? ""),
            "GetProductsByCategory" => _functions.GetProductsByCategory(GetStr("categoryName") ?? ""),
            "GetTopSoldProducts" => _functions.GetTopSoldProducts(GetInt("count") ?? 5, GetDate("from"), GetDate("to")),
            "GetTotalSales" => _functions.GetTotalSales(GetDate("from"), GetDate("to")),
            "GetTotalPurchases" => _functions.GetTotalPurchases(GetDate("from"), GetDate("to")),
            "GetSupplierProducts" => _functions.GetSupplierProducts(GetStr("supplierName") ?? ""),
            "GetStockQuantity" => _functions.GetStockQuantity(GetStr("productName") ?? ""),
            "GetRecentActivity" => _functions.GetRecentActivity(GetInt("count") ?? 10),
            "GetTotalStockValue" => _functions.GetTotalStockValue(),
            _ => null
        };
    }
}