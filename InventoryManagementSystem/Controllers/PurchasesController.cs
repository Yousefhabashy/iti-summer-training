using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels.Purchases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly AppDbContext _context;

        public PurchasesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Purchases
        public async Task<IActionResult> Index()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // GET: /Purchases/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);

            if (purchase == null)
            {
                return NotFound();
            }

            var vm = new PurchaseDetailsVm
            {
                PurchaseId = purchase.PurchaseId,
                SupplierName = purchase.Supplier.SupplierName,
                PurchaseDate = purchase.PurchaseDate,
                TotalAmount = purchase.TotalAmount,
                Items = purchase.PurchaseItems.Select(pi => new PurchaseLineItemVm
                {
                    ProductId = pi.ProductId,
                    ProductName = pi.Product.ProductName,
                    Quantity = pi.Quantity,
                    UnitCost = pi.UnitCost
                }).ToList()
            };

            return View(vm);
        }

        // GET: /Purchases/Create
        public async Task<IActionResult> Create()
        {
            var vm = new PurchaseCreateVm
            {
                Suppliers = await GetSupplierOptions(),
                Products = await GetProductOptions(),
                Items = new List<PurchaseLineItemVm> { new PurchaseLineItemVm() }
            };

            return View(vm);
        }

        // POST: /Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseCreateVm vm)
        {
            // Remove empty rows the user didn't fill in (e.g. left-over blank template row)
            vm.Items = vm.Items?.Where(i => i.ProductId != 0 && i.Quantity > 0).ToList()
                       ?? new List<PurchaseLineItemVm>();

            if (vm.Items.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Purchase must include at least one item.");
            }

            if (!ModelState.IsValid)
            {
                vm.Suppliers = await GetSupplierOptions();
                vm.Products = await GetProductOptions();
                return View(vm);
            }

            // Load the actual Product entities so we can increase their stock.
            var productIds = vm.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            // Guard against a stale/invalid ProductId sent from the form.
            var missingProductId = productIds.FirstOrDefault(id => !products.ContainsKey(id));
            if (missingProductId != 0)
            {
                ModelState.AddModelError(string.Empty, "One of the selected products no longer exists.");
                vm.Suppliers = await GetSupplierOptions();
                vm.Products = await GetProductOptions();
                return View(vm);
            }

            var purchase = new Purchase
            {
                SupplierId = vm.SupplierId,
                PurchaseDate = DateTime.Now,
                TotalAmount = vm.Items.Sum(i => i.Quantity * i.UnitCost),
                PurchaseItems = vm.Items.Select(i => new PurchaseItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost
                }).ToList()
            };

            // Wrap purchase creation + stock updates in a single transaction:
            // if anything fails, nothing is saved, so stock counts never get out of sync.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Purchases.Add(purchase);

                foreach (var item in vm.Items)
                {
                    products[item.ProductId].StockQuantity += item.Quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            TempData["SuccessMessage"] = "Purchase created successfully.";
            return RedirectToAction(nameof(Details), new { id = purchase.PurchaseId });
        }

        private async Task<List<SelectListItem>> GetSupplierOptions()
        {
            return await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetProductOptions()
        {
            return await _context.Products
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
