namespace InventoryManagementSystem.ViewModels.Dashboard;

public class TopProductItem
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}