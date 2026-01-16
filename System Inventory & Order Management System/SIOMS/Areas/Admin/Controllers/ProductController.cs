// SIOMS/Areas/Admin/Controllers/ProductController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /Admin/Product
        public async Task<IActionResult> Index(string search, int? categoryId)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p => 
                        p.Name.ToLower().Contains(search) || 
                        (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                        (p.Description != null && p.Description.ToLower().Contains(search)));
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                ViewBag.SearchFilter = search;
                ViewBag.CategoryFilter = categoryId;
                ViewBag.Categories = await _context.Categories.ToListAsync();

                var products = await query
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading products: {ex.Message}";
                return View(new System.Collections.Generic.List<Product>());
            }
        }

        // GET: /Admin/Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .Include(p => p.StockMovements)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: /Admin/Product/Create
       public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(new Product());
        }

        // POST: /Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                // ✅ FIX: Remove these fields from ModelState BEFORE checking ModelState.IsValid
                ModelState.Remove("ImageFile");
                ModelState.Remove("ImageUrl");
                ModelState.Remove("SKU");
                ModelState.Remove("Category"); // Remove navigation property
                ModelState.Remove("Supplier"); // Remove navigation property
                
                // ✅ FIX: Check for null values instead of relying on ModelState
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    ModelState.AddModelError("Name", "Product name is required");
                }

                if (product.CategoryId <= 0)
                {
                    ModelState.AddModelError("CategoryId", "Please select a category");
                }

                if (product.SupplierId <= 0)
                {
                    ModelState.AddModelError("SupplierId", "Please select a supplier");
                }

                if (product.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Price must be greater than 0");
                }

                if (product.StockQuantity < 0)
                {
                    ModelState.AddModelError("StockQuantity", "Stock quantity cannot be negative");
                }

                if (product.MinStockLimit < 0)
                {
                    ModelState.AddModelError("MinStockLimit", "Minimum stock limit cannot be negative");
                }

                // ✅ Image validation
                bool hasImageFile = product.ImageFile != null && product.ImageFile.Length > 0;
                bool hasImageUrl = !string.IsNullOrWhiteSpace(product.ImageUrl);
                
                if (hasImageFile && hasImageUrl)
                {
                    ModelState.AddModelError("ImageFile", "Please choose only one: upload file OR enter URL, not both");
                    ModelState.AddModelError("ImageUrl", "Please choose only one: upload file OR enter URL, not both");
                }
                else if (hasImageFile)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var fileExtension = Path.GetExtension(product.ImageFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (JPG, PNG, GIF, BMP, WEBP) are allowed");
                    }

                    if (product.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Image file size cannot exceed 5MB");
                    }
                }
                else if (hasImageUrl)
                {
                    if (!Uri.IsWellFormedUriString(product.ImageUrl, UriKind.Absolute))
                    {
                        ModelState.AddModelError("ImageUrl", "Please enter a valid URL");
                    }
                }

                // ✅ FIX: Check if ModelState has any errors (excluding the removed ones)
                bool hasErrors = ModelState.Keys
                    .Where(key => key != "ImageFile" && key != "ImageUrl" && key != "SKU" && key != "Category" && key != "Supplier")
                    .Any(key => ModelState[key].Errors.Any());

                if (hasErrors)
                {
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
                    return View(product);
                }

                // Generate SKU if empty
                if (string.IsNullOrWhiteSpace(product.SKU))
                {
                    product.SKU = $"PROD-{DateTime.Now:yyyyMMddHHmmss}";
                }

                // Handle image upload
                if (hasImageFile)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
                    
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = $"/uploads/products/{uniqueFileName}";
                }
                else if (hasImageUrl)
                {
                    product.ImageUrl = product.ImageUrl.Trim();
                }

                // Set timestamps
                product.CreatedAt = DateTime.Now;
                
                // Save product
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Create initial stock movement
                if (product.StockQuantity > 0)
                {
                    var movement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = product.StockQuantity,
                        MovementType = "Initial Stock",
                        ReferenceNumber = "INITIAL",
                        Notes = "Initial stock entry",
                        MovementDate = DateTime.Now
                    };
                    _context.StockMovements.Add(movement);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Product '{product.Name}' created successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating product: {ex.Message}";
                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
                return View(product);
            }
        }
        // GET: /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = await _context.Categories.ToListAsync();
                ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /Admin/Product/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Product product)
{
    try
    {
        if (id != product.Id)
        {
            TempData["Error"] = "Invalid product ID";
            return RedirectToAction("Index");
        }

        // ✅ FIX: Remove these fields from ModelState BEFORE checking validation
        ModelState.Remove("ImageFile");
        ModelState.Remove("ImageUrl");
        ModelState.Remove("SKU");
        ModelState.Remove("Category"); // Remove navigation property
        ModelState.Remove("Supplier"); // Remove navigation property
        ModelState.Remove("StockQuantity"); // Stock can't be updated here
        
        // ✅ FIX: Manual validation like Create method
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            ModelState.AddModelError("Name", "Product name is required");
        }

        if (product.CategoryId <= 0)
        {
            ModelState.AddModelError("CategoryId", "Please select a category");
        }

        if (product.SupplierId <= 0)
        {
            ModelState.AddModelError("SupplierId", "Please select a supplier");
        }

        if (product.Price <= 0)
        {
            ModelState.AddModelError("Price", "Price must be greater than 0");
        }

        if (product.MinStockLimit < 0)
        {
            ModelState.AddModelError("MinStockLimit", "Minimum stock limit cannot be negative");
        }

        // ✅ Image validation: If both file and URL are provided, show error
        bool hasImageFile = product.ImageFile != null && product.ImageFile.Length > 0;
        bool hasImageUrl = !string.IsNullOrWhiteSpace(product.ImageUrl);
        
        if (hasImageFile && hasImageUrl)
        {
            ModelState.AddModelError("ImageFile", "Please choose only one: upload file OR enter URL, not both");
            ModelState.AddModelError("ImageUrl", "Please choose only one: upload file OR enter URL, not both");
        }
        else if (hasImageFile)
        {
            // Validate image file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var fileExtension = Path.GetExtension(product.ImageFile.FileName).ToLower();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ImageFile", "Only image files (JPG, PNG, GIF, BMP, WEBP) are allowed");
            }

            // Check file size (5MB max)
            if (product.ImageFile.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ImageFile", "Image file size cannot exceed 5MB");
            }
        }
        else if (hasImageUrl)
        {
            // Validate URL format
            if (!Uri.IsWellFormedUriString(product.ImageUrl, UriKind.Absolute))
            {
                ModelState.AddModelError("ImageUrl", "Please enter a valid URL");
            }
        }

        // ✅ FIX: Check if ModelState has any errors (excluding the removed ones)
        bool hasErrors = ModelState.Keys
            .Where(key => key != "ImageFile" && key != "ImageUrl" && key != "SKU" && 
                         key != "Category" && key != "Supplier" && key != "StockQuantity")
            .Any(key => ModelState[key].Errors.Any());

        if (hasErrors)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
            return View(product);
        }

        // Get existing product
        var existingProduct = await _context.Products.FindAsync(id);
        if (existingProduct == null)
        {
            TempData["Error"] = "Product not found";
            return RedirectToAction("Index");
        }

        // Handle image upload
        if (hasImageFile)
        {
            // Delete old image if exists (only if it's a local file)
            if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && 
                existingProduct.ImageUrl.StartsWith("/uploads/products/"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            // Save new image
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await product.ImageFile.CopyToAsync(fileStream);
            }

            existingProduct.ImageUrl = $"/uploads/products/{uniqueFileName}";
        }
        // If user provided ImageUrl (but didn't upload file)
        else if (hasImageUrl)
        {
            // If existing product had uploaded image, delete it
            if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && 
                existingProduct.ImageUrl.StartsWith("/uploads/products/"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }
            
            existingProduct.ImageUrl = product.ImageUrl.Trim();
        }
        // If user wants to remove image (both fields empty and we have existing image)
        else if (string.IsNullOrEmpty(product.ImageUrl) && 
                 !string.IsNullOrEmpty(existingProduct.ImageUrl) &&
                 !hasImageFile)
        {
            // Delete the existing image file if it's local
            if (existingProduct.ImageUrl.StartsWith("/uploads/products/"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }
            existingProduct.ImageUrl = null;
        }
        // Otherwise keep existing image

        // Update properties
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.SupplierId = product.SupplierId;
        existingProduct.Price = product.Price;
        existingProduct.MinStockLimit = product.MinStockLimit;
        existingProduct.SKU = product.SKU;
        existingProduct.UpdatedAt = DateTime.Now;

        _context.Update(existingProduct);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Product '{product.Name}' updated successfully";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"Error updating product: {ex.Message}";
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Suppliers = await _context.Suppliers.Where(s => s.IsActive).ToListAsync();
        return View(product);
    }
}
        // GET: /Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Supplier)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.StockMovements)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index");
                }

                // Delete image file if exists (only if it's a local file)
                if (!string.IsNullOrEmpty(product.ImageUrl) && 
                    product.ImageUrl.StartsWith("/uploads/products/"))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                        System.IO.File.Delete(imagePath);
                }

                // Delete stock movements first
                if (product.StockMovements.Any())
                {
                    _context.StockMovements.RemoveRange(product.StockMovements);
                }

                // Delete product
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Product '{product.Name}' deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting product: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: /Admin/Product/UpdateStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        
public async Task<IActionResult> UpdateStock(int id, [FromBody] StockUpdateModel model)
{
    try
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return Json(new { success = false, message = "Product not found" });

        // Validate new stock
        if (model.NewStock < 0)
        {
            return Json(new { 
                success = false, 
                message = "Stock quantity cannot be negative" 
            });
        }

        int oldStock = product.StockQuantity;
        product.StockQuantity = model.NewStock;
        product.UpdatedAt = DateTime.Now;

        // Create stock movement record
        var movement = new StockMovement
        {
            ProductId = product.Id,
            Quantity = model.NewStock - oldStock,
            MovementType = "Manual Adjustment",
            ReferenceNumber = $"MANUAL-{DateTime.Now:yyyyMMddHHmmss}",
            Notes = model.Notes,
            MovementDate = DateTime.Now
        };

        _context.StockMovements.Add(movement);
        _context.Update(product);
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            message = "Stock updated successfully",
            newStock = product.StockQuantity 
        });
    }
    catch (Exception ex)
    {
        return Json(new { 
            success = false, 
            message = $"Error: {ex.Message}"
        });
    }
}
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }

    public class StockUpdateModel
    {
        public int NewStock { get; set; }
        public string Notes { get; set; }
    }
}