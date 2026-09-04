using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Sales;

public class SaleCreateVm
{
    [MaxLength(150)]
    public string? CustomerInfo { get; set; }
    public List<SelectListItem> Products { get; set; } = new();
    public List<SaleLineItemVm> Items { get; set; } = new();
}