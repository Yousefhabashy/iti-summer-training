using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Sales
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            return View(sales);
        }

        // GET: /Sales/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .FirstOrDefaultAsync(s => s.SaleId == id);

            if (sale == null)
            {
                return NotFound();
            }

            var vm = new SaleDetailsVm
            {
                SaleId = sale.SaleId,
                SaleDate = sale.SaleDate,
                TotalAmount = sale.TotalAmount,
                CustomerInfo = sale.CustomerInfo,

                Items = sale.SaleItems.Select(si => new SaleLineItemVm
                {
                    ProductId = si.ProductId,
                    ProductName = si.Product.ProductName,
                    Quantity = si.Quantity,
                    UnitPrice = si.UnitPrice
                }).ToList()
            };

            return View(vm);
        }

        // GET: /Sales/Create
        public async Task<IActionResult> Create()
        {
            var vm = new SaleCreateVm
            {
                Products = await GetProductOptions(),

                Items = new List<SaleLineItemVm>
                {
                    new SaleLineItemVm()
                }
            };

            return View(vm);
        }

        // POST: /Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateVm vm)
        {
            // Remove empty rows
            vm.Items = vm.Items?
                .Where(i => i.ProductId != 0 && i.Quantity > 0)
                .ToList()
                ?? new List<SaleLineItemVm>();

            // Sale must contain at least one item
            if (vm.Items.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sale must include at least one item."
                );
            }

            if (!ModelState.IsValid)
            {
                vm.Products = await GetProductOptions();
                return View(vm);
            }

            // Get selected Product IDs
            var productIds = vm.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            // Load actual products from database
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            // Check that all products still exist
            var missingProductId = productIds
                .FirstOrDefault(id => !products.ContainsKey(id));

            if (missingProductId != 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "One of the selected products no longer exists."
                );

                vm.Products = await GetProductOptions();
                return View(vm);
            }

            // Check stock availability
            // Group items by product
            var groupedItems = vm.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .ToList();

            // Check stock availability
            foreach (var item in groupedItems)
            {
                var product = products[item.ProductId];

                if (item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Not enough stock for {product.ProductName}. " +
                        $"Available quantity: {product.StockQuantity}."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                vm.Products = await GetProductOptions();
                return View(vm);
            }

            // Create Sale
            var saleItems = groupedItems.Select(item =>
            {
                var product = products[item.ProductId];

                return new SaleItem
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.UnitPrice
                };
            }).ToList();

            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                CustomerInfo = vm.CustomerInfo,

                TotalAmount = saleItems.Sum(
                    item => item.Quantity * item.UnitPrice
                ),

                SaleItems = saleItems
            };

            // Save sale + update stock in one transaction
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Sales.Add(sale);

                foreach (var item in groupedItems)
                {
                    products[item.ProductId].StockQuantity -= item.Quantity;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            TempData["SuccessMessage"] =
                "Sale created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = sale.SaleId }
            );
        }

        private async Task<List<SelectListItem>> GetProductOptions()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    Text = p.ProductName
                })
                .ToListAsync();
        }
    }
}