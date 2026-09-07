using InventoryManagementSystem.Models;
using InventoryManagementSystem.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int pageNumber = 1, int pageSize = 6)
        {
            var query = _context.Categories.AsQueryable();

            int totalCategories = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);
            var categories = query.OrderBy(c => c.CategoryId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .Select(c => new CategoryVm 
                    { 
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        Description = c.Description, 
                        ProductCount = _context.Products.Count(p => p.CategoryId == c.CategoryId)
                    }).ToList();

            ViewBag.TotalPage = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(categories);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var category = _context.Categories. FirstOrDefault(c => c.CategoryId == id.Value);

            if (category == null) 
                return NotFound();

            var viewModel = new CategoryVm()
            {
                CategoryId = category.CategoryId, 
                CategoryName = category.CategoryName, 
                Description = category.Description, 
                ProductCount = _context.Products.Count(p => p.CategoryId == category.CategoryId)
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = _context.Categories.FirstOrDefault(c => c.CategoryName == category.CategoryName);

                if (existingCategory != null)
                {
                    TempData["ErrorMessage"] = "A category with this name already exists!";
                    return View(category);
                }

                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) 
                return NotFound();

            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id.Value);

            if (category == null) 
                return NotFound();

            var viewModel = new CategoryVm() 
                { 
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description
                };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(CategoryVm categoryVm)
        {
            if (ModelState.IsValid)
            {
                var category = _context.Categories.FirstOrDefault(c => c.CategoryId == categoryVm.CategoryId);

                if (category == null)
                    return NotFound();
                
                var existingCategory = _context.Categories
                    .FirstOrDefault(c => c.CategoryName == categoryVm.CategoryName
                                         && c.CategoryId != categoryVm.CategoryId);

                if (existingCategory != null)
                {
                    TempData["ErrorMessage"] = "A category with this name already exists!";
                    return View(categoryVm);
                }

                category.CategoryName = categoryVm.CategoryName;
                category.Description = categoryVm.Description;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }

            return View(categoryVm);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) 
                return NotFound();

            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id.Value);

            if (category == null) 
                return NotFound();

            int productCount = _context.Products.Count(p => p.CategoryId == id.Value);

            var viewModel = new CategoryVm()
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description, 
                ProductCount = productCount, 
                HasProducts = productCount>0
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found!";
                return RedirectToAction("Index");
            }

            int productCount = _context.Products.Count(p => p.CategoryId == id);

            if (productCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete category! It contains {productCount} product.\n Please remove or reassign products first.";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
    }
}