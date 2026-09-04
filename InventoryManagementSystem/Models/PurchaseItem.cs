using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models
{
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }

        public int PurchaseId { get; set; }
        [ForeignKey(nameof(PurchaseId))]
        public Purchase Purchase { get; set; } = null!;

        [Required(ErrorMessage = "Product Required")]
        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity Must Be Greater Than Zero")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Cost Must Be Greater Than Zero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }
    }
}
