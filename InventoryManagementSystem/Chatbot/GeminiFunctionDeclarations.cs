using System.Text.Json.Nodes;

namespace InventoryManagementSystem.Chatbot;

public static class GeminiFunctionDeclarations
{
    public static JsonArray GetDeclarations()
    {
        return new JsonArray
        {
            Declare(
                "GetLowStockProducts",
                "Returns all products whose stock quantity is below their low-stock threshold.",
                new JsonObject()),

            Declare(
                "GetProductByName", 
                "Returns price, stock quantity and category for a specific product by name.",
                new JsonObject
                {
                    ["productName"] = Param("The name (or part of the name) of the product.")
                }),

            Declare(
                "GetProductsByCategory",
                "Returns all products that belong to a given category name.",
                new JsonObject
                {
                    ["categoryName"] = Param("The category name, e.g. Electronics, Groceries.") 
                }),

            Declare(
                "GetTopSoldProducts",
                "Returns the best-selling products ranked by quantity sold, optionally within a date range.",
                new JsonObject
                {
                    ["count"] = Param("How many top products to return. Default 5.", "integer"),
                    ["from"] = Param("Start date (YYYY-MM-DD), optional.", "string"),
                    ["to"] = Param("End date (YYYY-MM-DD), optional.", "string")
                }),

            Declare(
                "GetTotalSales", 
                "Returns the total number of sales and total sales revenue, optionally within a date range.",
                new JsonObject
                {
                    ["from"] = Param("Start date (YYYY-MM-DD), optional.", "string"),
                    ["to"] = Param("End date (YYYY-MM-DD), optional.", "string")
                }),

            Declare(
                "GetTotalPurchases", 
                "Returns the total number of purchases and total purchase cost, optionally within a date range.",
                new JsonObject
                {
                    ["from"] = Param("Start date (YYYY-MM-DD), optional.", "string"),
                    ["to"] = Param("End date (YYYY-MM-DD), optional.", "string")
                }),

            Declare(
                "GetSupplierProducts",
                "Returns all products that have been supplied by a given supplier name.",
                new JsonObject
                {
                    ["supplierName"] = Param("The supplier's name.") 
                }),

            Declare("GetStockQuantity", "Returns the current stock quantity of a specific product by name.",
                new JsonObject
                {
                    ["productName"] = Param("The name of the product.") 
                }),

            Declare(
                "GetRecentActivity", 
                "Returns the most recent sales and purchases combined, most recent first.",
                new JsonObject
                {
                    ["count"] = Param("How many recent activities to return. Default 10.", "integer") 
                }),

            Declare(
                "GetTotalStockValue", 
                "Returns the total monetary value of all current stock (price x quantity summed).",
                new JsonObject()
                )
        };
    }

    private static JsonObject Declare(string name, string description, JsonObject properties)
    {
        return new JsonObject
        {
            ["name"] = name,
            ["description"] = description,
            ["parameters"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties
            }
        };
    }

    private static JsonObject Param(string description, string type = "string")
        => new JsonObject { ["type"] = type, ["description"] = description };
}