using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier Name Required")]
        [MaxLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [Phone(ErrorMessage = "Phone Number Is Invalid")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Email Address Is Invalid")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
