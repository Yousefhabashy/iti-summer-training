using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Purchases;

public class PurchaseCreateVm
{
    [Required(ErrorMessage = "Supplier is required")]
    public int SupplierId { get; set; }

    public List<SelectListItem> Suppliers { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();

    public List<PurchaseLineItemVm> Items { get; set; } = new();
}