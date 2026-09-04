using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.ServerSentEvents;

namespace InventoryManagementSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "SKU Required")]
        [MaxLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Name Required")]
        [MaxLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category Required")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price Must Be Greater Than Zero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity Cannot Be Negative")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue)]
        public int LowStockThreshold { get; set; } = 5;

        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
