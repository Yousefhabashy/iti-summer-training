using InventoryManagementSystem.ViewModels.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Basic statistics
            var totalProducts = await _context.Products
                .CountAsync(p => p.IsActive);

            var totalCategories = await _context.Categories
                .CountAsync();

            var totalSuppliers = await _context.Suppliers
                .CountAsync();

            var totalStockQuantity = await _context.Products
                .Where(p => p.IsActive)
                .SumAsync(p => p.StockQuantity);

            var lowStockCount = await _context.Products
                .CountAsync(p =>
                    p.IsActive &&
                    p.StockQuantity <= p.LowStockThreshold);
            var lowStockProducts = await _context.Products
                .Where(p =>
            p.IsActive &&
            p.StockQuantity <= p.LowStockThreshold)
               .OrderBy(p => p.StockQuantity)
               .ToListAsync();

            // Purchase statistics
            var totalPurchasesCount = await _context.Purchases
                .CountAsync();

            var totalPurchasesValue = await _context.Purchases
                .SumAsync(p => p.TotalAmount);

            // Sales statistics
            var totalSalesCount = await _context.Sales
                .CountAsync();

            var totalSalesRevenue = await _context.Sales
                .SumAsync(s => s.TotalAmount);
            var recentPurchases = await _context.Purchases
              .OrderByDescending(p => p.PurchaseDate)
              .Take(10)
              .Select(p => new ActivityItem
              {
             Type = "Purchase",
             Description = $"Purchase #{p.PurchaseId}",
             Date = p.PurchaseDate
              })
             .ToListAsync();

            var recentSales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .Take(10)
                .Select(s => new ActivityItem
                {
                    Type = "Sale",
                    Description = $"Sale #{s.SaleId}",
                    Date = s.SaleDate
                })
                .ToListAsync();

            var recentActivity = recentPurchases
                .Concat(recentSales)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToList();

            // Most sold products
            var mostSoldProducts = await _context.SaleItems
                .GroupBy(si => new
                {
                    si.ProductId,
                    si.Product.ProductName
                })
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(si => si.Quantity),
                    Revenue = g.Sum(si => si.Quantity * si.UnitPrice)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToListAsync();

            // Build Dashboard ViewModel
            var vm = new DashboardVm
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalSuppliers = totalSuppliers,
                TotalStockQuantity = totalStockQuantity,
                LowStockCount = lowStockCount,
                LowStockProducts = lowStockProducts,

                TotalPurchasesCount = totalPurchasesCount,
                TotalPurchasesValue = totalPurchasesValue,

                TotalSalesCount = totalSalesCount,
                TotalSalesRevenue = totalSalesRevenue,

                RecentActivity = recentActivity,
                MostSoldProducts = mostSoldProducts
            };

            return View(vm);
        }
    }
}