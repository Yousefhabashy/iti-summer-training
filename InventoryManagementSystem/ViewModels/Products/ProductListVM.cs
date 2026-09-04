namespace InventoryManagementSystem.ViewModels.Products;

public class ProductListVm
{
    public List<Models.Product> Products { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public int? CategoryFilter { get; set; }
    public string? StatusFilter { get; set; }
}