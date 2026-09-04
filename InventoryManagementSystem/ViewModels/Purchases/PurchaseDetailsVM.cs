namespace InventoryManagementSystem.ViewModels.Purchases;

public class PurchaseDetailsVm
{
    public int PurchaseId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseLineItemVm> Items { get; set; } = new();
}