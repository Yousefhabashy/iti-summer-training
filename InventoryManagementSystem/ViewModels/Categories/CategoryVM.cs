using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.ViewModels.Categories;

public class CategoryVm
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name is required")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    public int ProductCount { get; set; }
}