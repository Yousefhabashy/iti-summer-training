namespace InventoryManagementSystem.ViewModels.Dashboard;

public class ActivityItem
{
    public string Type { get; set; } = string.Empty; // "Sale" or "Purchase"
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}