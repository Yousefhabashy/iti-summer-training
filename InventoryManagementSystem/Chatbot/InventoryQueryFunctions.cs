using InventoryManagementSystem.Chatbot.Dtos;

namespace InventoryManagementSystem.Chatbot;

using Microsoft.EntityFrameworkCore;

public class InventoryQueryFunctions
{
    private readonly AppDbContext _context;

    public InventoryQueryFunctions(AppDbContext context)
    {
        _context = context;
    }

    public List<ProductDto> GetLowStockProducts()
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.StockQuantity < p.LowStockThreshold)
            .Select(p => new ProductDto
            {
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.CategoryName
            })
            .ToList();
    }

    public ProductDto? GetProductByName(string productName)
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.ProductName.Contains(productName))
            .Select(p => new ProductDto
            {
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.CategoryName
            })
            .FirstOrDefault();
    }

    public List<ProductDto> GetProductsByCategory(string categoryName)
    {
        return _context.Products
            .Include(p => p.Category)
            .Where(p => p.Category.CategoryName.Contains(categoryName))
            .Select(p => new ProductDto
            {
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category.CategoryName
            })
            .ToList();
    }

    public List<TopProductDto> GetTopSoldProducts(int count = 5, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.SaleItems.Include(si => si.Sale).AsQueryable();

        if (from.HasValue) query = query.Where(si => si.Sale.SaleDate >= from.Value);
        if (to.HasValue) query = query.Where(si => si.Sale.SaleDate <= to.Value);

        return query
            .GroupBy(si => si.Product.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .ToList();
    }

    public SalesSummaryDto GetTotalSales(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Sales.AsQueryable();
        if (from.HasValue) query = query.Where(s => s.SaleDate >= from.Value);
        if (to.HasValue) query = query.Where(s => s.SaleDate <= to.Value);

        return new SalesSummaryDto
        {
            Count = query.Count(),
            TotalAmount = query.Sum(s => (decimal?)s.TotalAmount) ?? 0
        };
    }

    public PurchasesSummaryDto GetTotalPurchases(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Purchases.AsQueryable();
        if (from.HasValue) query = query.Where(p => p.PurchaseDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PurchaseDate <= to.Value);

        return new PurchasesSummaryDto
        {
            Count = query.Count(),
            TotalAmount = query.Sum(p => (decimal?)p.TotalAmount) ?? 0
        };
    }

    public List<ProductDto> GetSupplierProducts(string supplierName)
    {
        return _context.PurchaseItems
            .Include(pi => pi.Purchase).ThenInclude(p => p.Supplier)
            .Include(pi => pi.Product).ThenInclude(p => p.Category)
            .Where(pi => pi.Purchase.Supplier.SupplierName.Contains(supplierName))
            .Select(pi => new ProductDto
            {
                ProductName = pi.Product.ProductName,
                UnitPrice = pi.Product.UnitPrice,
                StockQuantity = pi.Product.StockQuantity,
                CategoryName = pi.Product.Category.CategoryName
            })
            .Distinct()
            .ToList();
    }

    public int? GetStockQuantity(string productName)
    {
        return _context.Products
            .Where(p => p.ProductName.Contains(productName))
            .Select(p => (int?)p.StockQuantity)
            .FirstOrDefault();
    }

    public List<ActivityDto> GetRecentActivity(int count = 10)
    {
        var sales = _context.Sales
            .OrderByDescending(s => s.SaleDate)
            .Take(count)
            .Select(s => new ActivityDto
            {
                Type = "Sale",
                Description = $"Sale #{s.SaleId} - {s.TotalAmount:C}",
                Date = s.SaleDate
            })
            .ToList(); 

        var purchases = _context.Purchases
            .OrderByDescending(p => p.PurchaseDate)
            .Take(count)
            .Select(p => new ActivityDto
            {
                Type = "Purchase",
                Description = $"Purchase #{p.PurchaseId} - {p.TotalAmount:C}",
                Date = p.PurchaseDate
            })
            .ToList(); 

        return sales.Concat(purchases)  
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToList();
    }

    public decimal GetTotalStockValue()
    {
        return _context.Products.Sum(p => (decimal?)(p.UnitPrice * p.StockQuantity)) ?? 0;
    }
}