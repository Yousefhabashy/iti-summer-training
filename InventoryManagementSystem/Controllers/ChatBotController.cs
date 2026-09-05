using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Chatbot;

namespace InventoryManagementSystem.Controllers;

public class ChatbotController : Controller
{
    private readonly GeminiChatbotService _geminiService;

    public ChatbotController(GeminiChatbotService geminiService)
    {
        _geminiService = geminiService;
    }

    // GET /Chatbot  -> the chat page directly
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Ask([FromForm] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return Json(new { reply = "Please type a question." });

        try
        {
            var reply = await _geminiService.AskAsync(message);
            return Json(new { reply });
        }
        catch (Exception ex)
        {
            return Json(new { reply = $"ERROR: {ex.Message}" });
        }
    }
}