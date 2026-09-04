using InventoryManagementSystem.Models;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PurchaseItem>()
                .Property(p => p.UnitCost).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>()
                .Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
