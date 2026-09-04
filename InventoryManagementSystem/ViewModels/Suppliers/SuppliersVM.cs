using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Suppliers;

public class SupplierVm
{
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Supplier Name is required")]
    [MaxLength(150)]
    public string SupplierName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [Phone(ErrorMessage = "Phone number is invalid")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email address is invalid")]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    public List<Models.Product> SuppliedProducts { get; set; } = new();
}