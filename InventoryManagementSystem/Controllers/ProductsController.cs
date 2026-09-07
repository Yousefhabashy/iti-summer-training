using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels.Product;
using InventoryManagementSystem.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _context;
    private const int PageSize = 10;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Products
    public async Task<IActionResult> Index(string? search, int? categoryId, string? status, bool showInactive = false, int page = 1)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!showInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.ProductName.Contains(search) || p.Sku.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = status switch
            {
                "low" => query.Where(p => p.StockQuantity < p.LowStockThreshold),
                "out" => query.Where(p => p.StockQuantity == 0),
                "ok" => query.Where(p => p.StockQuantity >= p.LowStockThreshold),
                _ => query
            };
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        if (page < 1) page = 1;

        var products = await query
            .OrderBy(p => p.ProductName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var vm = new ProductListVm()
        {
            Products = products,
            PageNumber = page,
            TotalPages = totalPages,
            SearchTerm = search,
            CategoryFilter = categoryId,
            StatusFilter = status,
            ShowInactive = showInactive
        };

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName", categoryId);

        return View(vm);
    }

    // GET: /Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id.Value);

        if (product == null) return NotFound();

        return View(product);
    }

    // GET: /Products/Create
    public async Task<IActionResult> Create()
    {
        var vm = new CreateProductVm
        {
            Categories = await GetCategorySelectListAsync()
        };
        return View(vm);
    }

    // POST: /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductVm vm)
    {
        if (await _context.Products.AnyAsync(p => p.Sku == vm.Sku))
            ModelState.AddModelError(nameof(vm.Sku), "SKU already exists.");

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync(vm.CategoryId);
            return View(vm);
        }

        var product = new Product
        {
            Sku = vm.Sku,
            ProductName = vm.ProductName,
            CategoryId = vm.CategoryId,
            UnitPrice = vm.UnitPrice,
            StockQuantity = vm.StockQuantity,
            LowStockThreshold = vm.LowStockThreshold
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Product created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products.FindAsync(id.Value);
        if (product == null) return NotFound();

        var vm = new EditProductVm()
        {
            ProductId = product.ProductId,
            Sku = product.Sku,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            UnitPrice = product.UnitPrice,
            StockQuantity = product.StockQuantity,
            LowStockThreshold = product.LowStockThreshold,
            Categories = await GetCategorySelectListAsync(product.CategoryId)
        };

        return View(vm);
    }

    // POST: /Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditProductVm vm)
    {
        if (id != vm.ProductId) return BadRequest();

        if (await _context.Products.AnyAsync(p => p.Sku == vm.Sku && p.ProductId != id))
            ModelState.AddModelError(nameof(vm.Sku), "SKU already exists.");

        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectListAsync(vm.CategoryId);
            return View(vm);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.Sku = vm.Sku;
        product.ProductName = vm.ProductName;
        product.CategoryId = vm.CategoryId;
        product.UnitPrice = vm.UnitPrice;
        product.StockQuantity = vm.StockQuantity;
        product.LowStockThreshold = vm.LowStockThreshold;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Product updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Products/Delete/5 — confirmation page only, does NOT delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == id.Value);

        if (product == null) return NotFound();

        return View(product);
    }

    // POST: /Products/Delete/5 — this is what actually deletes/deactivates
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return RedirectToAction(nameof(Index));

        bool isReferenced = await _context.PurchaseItems.AnyAsync(pi => pi.ProductId == id)
                          || await _context.SaleItems.AnyAsync(si => si.ProductId == id);

        if (isReferenced)
        {
            product.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product has history, so it was marked as inactive instead of being deleted.";
        }
        else
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Products/Reactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product reactivated.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetCategorySelectListAsync(int? selectedId = null)
    {
        return await _context.Categories
            .OrderBy(c => c.CategoryName)
            .Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName,
                Selected = c.CategoryId == selectedId
            })
            .ToListAsync();
    }
}