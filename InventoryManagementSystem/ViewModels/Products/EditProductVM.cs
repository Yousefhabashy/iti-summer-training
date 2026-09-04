using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Products;

public class EditProductVm
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "SKU is required")]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than zero")]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    public List<SelectListItem> Categories { get; set; } = new();
}