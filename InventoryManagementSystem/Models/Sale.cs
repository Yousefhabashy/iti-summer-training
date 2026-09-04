using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.ServerSentEvents;

namespace InventoryManagementSystem.Models
{
    public class Sale
    {
        public int SaleId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(150)]
        public string? CustomerInfo { get; set; }

        [MinLength(1, ErrorMessage = "Sale must include at least one item")]
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}
