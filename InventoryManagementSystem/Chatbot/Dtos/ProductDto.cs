namespace InventoryManagementSystem.Chatbot.Dtos;

public class ProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}