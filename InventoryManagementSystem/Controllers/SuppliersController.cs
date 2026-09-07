using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return View(suppliers);
        }

        // GET: /Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == id);

            if (supplier == null)
            {
                return NotFound();
            }

            // Supplier -> Purchase -> PurchaseItem -> Product
            // Distinct() here is translated by EF Core into SQL SELECT DISTINCT
            // over the Product columns being selected, so the same product
            // supplied across multiple purchases only appears once.
            var suppliedProducts = await _context.PurchaseItems
                .Where(pi => pi.Purchase.SupplierId == id)
                .Select(pi => pi.Product)
                .Distinct()
                .ToListAsync();

            var vm = new SupplierVm
            {
                SupplierId = supplier.SupplierId,
                SupplierName = supplier.SupplierName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                SuppliedProducts = suppliedProducts
            };

            return View(vm);
        }

        // GET: /Suppliers/Create
        public IActionResult Create()
        {
            return View(new SupplierVm());
        }

        // POST: /Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierVm vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var supplier = new Supplier
            {
                SupplierName = vm.SupplierName,
                ContactName = vm.ContactName,
                Phone = vm.Phone,
                Email = vm.Email,
                Address = vm.Address
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Suppliers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            var vm = new SupplierVm
            {
                SupplierId = supplier.SupplierId,
                SupplierName = supplier.SupplierName,
                ContactName = supplier.ContactName,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address
            };

            return View(vm);
        }

        // POST: /Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierVm vm)
        {
            if (id != vm.SupplierId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            supplier.SupplierName = vm.SupplierName;
            supplier.ContactName = vm.ContactName;
            supplier.Phone = vm.Phone;
            supplier.Email = vm.Email;
            supplier.Address = vm.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Suppliers.AnyAsync(s => s.SupplierId == id))
                {
                    return NotFound();
                }
                throw;
            }

            TempData["SuccessMessage"] = "Supplier updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Suppliers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            bool hasPurchases = await _context.Purchases.AnyAsync(p => p.SupplierId == id);

            if (hasPurchases)
            {
                TempData["ErrorMessage"] = "Cannot delete this supplier because they have existing purchase records.";
                return RedirectToAction(nameof(Index));
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
