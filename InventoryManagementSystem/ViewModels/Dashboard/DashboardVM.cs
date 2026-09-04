namespace InventoryManagementSystem.ViewModels.Dashboard;

public class DashboardVm
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalStockQuantity { get; set; }
    public int LowStockCount { get; set; }
    public int TotalPurchasesCount { get; set; }
    public decimal TotalPurchasesValue { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal TotalSalesRevenue { get; set; }
    public List<ActivityItem> RecentActivity { get; set; } = new();
    public List<TopProductItem> MostSoldProducts { get; set; } = new();
}