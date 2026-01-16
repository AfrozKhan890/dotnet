using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Category
        public async Task<IActionResult> Index(string search)
        {
            try
            {
                var query = _context.Categories
                    .Include(c => c.Products)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(c => 
                        c.Name.ToLower().Contains(search) || 
                        (c.Description != null && c.Description.ToLower().Contains(search)));
                }

                var categories = await query
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                ViewBag.SearchFilter = search;
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading categories: {ex.Message}";
                return View(new System.Collections.Generic.List<Category>());
            }
        }

        // GET: /Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid category ID";
                    return RedirectToAction("Index");
                }

                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading category: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: /Admin/Category/Create
        public IActionResult Create()
        {
            return View(new Category()); // Empty category for form
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    ModelState.AddModelError("Name", "Category name is required");
                    return View(category);
                }

                // Trim inputs
                category.Name = category.Name.Trim();
                if (!string.IsNullOrWhiteSpace(category.Description))
                    category.Description = category.Description.Trim();

                // Check for duplicate
                bool exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
                
                if (exists)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists");
                    return View(category);
                }

                // Set timestamps
                category.CreatedAt = DateTime.Now;
                
                // Save
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Category '{category.Name}' created successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating category: {ex.Message}";
                return View(category);
            }
        }

        // GET: /Admin/Category/Edit/5 - FIXED: Shows old data
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid category ID";
                    return RedirectToAction("Index");
                }

                var category = await _context.Categories.FindAsync(id);
                
                if (category == null)
                {
                    TempData["Error"] = "Category not found";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading category: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /Admin/Category/Edit/5 - FIXED: Properly updates
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    TempData["Error"] = "Invalid category ID";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    ModelState.AddModelError("Name", "Category name is required");
                    return View(category);
                }

                // Trim inputs
                category.Name = category.Name.Trim();
                if (!string.IsNullOrWhiteSpace(category.Description))
                    category.Description = category.Description.Trim();

                // Get existing category
                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    TempData["Error"] = "Category not found";
                    return RedirectToAction("Index");
                }

                // Check for duplicate (excluding current)
                if (existingCategory.Name.ToLower() != category.Name.ToLower())
                {
                    bool duplicate = await _context.Categories
                        .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != id);
                    
                    if (duplicate)
                    {
                        ModelState.AddModelError("Name", "A category with this name already exists");
                        return View(category);
                    }
                }

                // Update properties - FIXED: Update existing category, not create new
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.UpdatedAt = DateTime.Now;

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Category '{category.Name}' updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating category: {ex.Message}";
                return View(category);
            }
        }

        // GET: /Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid category ID";
                    return RedirectToAction("Index");
                }

                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found";
                    return RedirectToAction("Index");
                }

                return View(category);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading category: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    TempData["Error"] = "Category not found";
                    return RedirectToAction("Index");
                }

                // Check if category has products
                if (category.Products.Any())
                {
                    TempData["Error"] = $"Cannot delete category '{category.Name}' because it has {category.Products.Count} product(s)";
                    return RedirectToAction("Index");
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Category '{category.Name}' deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting category: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}