using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Sales;

public class SaleLineItemVm
{
    [Required(ErrorMessage = "Product is required")]
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than zero")]
    public decimal UnitPrice { get; set; }
}