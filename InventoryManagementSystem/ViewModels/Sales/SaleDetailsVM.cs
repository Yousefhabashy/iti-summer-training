namespace InventoryManagementSystem.ViewModels.Sales;

public class SaleDetailsVm
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CustomerInfo { get; set; }
    public List<SaleLineItemVm> Items { get; set; } = new();

}