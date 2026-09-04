using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name Required")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}