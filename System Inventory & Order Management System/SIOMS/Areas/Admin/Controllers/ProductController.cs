using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products/Index
        public async Task<IActionResult> Index(string search, int? categoryId, string sortOrder)
        {
            ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSort"] = sortOrder == "price" ? "price_desc" : "price";
            ViewData["StockSort"] = sortOrder == "stock" ? "stock_desc" : "stock";
            ViewData["CategoryFilter"] = categoryId;
            ViewData["SearchFilter"] = search;

            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => 
                    p.Name.Contains(search) || 
                    p.Description.Contains(search) ||
                    p.SKU.Contains(search));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "stock":
                    products = products.OrderBy(p => p.StockQuantity);
                    break;
                case "stock_desc":
                    products = products.OrderByDescending(p => p.StockQuantity);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.StockMovements)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                // Generate SKU if not provided
                if (string.IsNullOrEmpty(product.SKU))
                {
                    product.SKU = "PROD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                }

                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();

                // Create initial stock movement
                if (product.StockQuantity > 0)
                {
                    var movement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = product.StockQuantity,
                        MovementType = "Initial",
                        ReferenceNumber = "INITIAL",
                        Notes = "Initial stock"
                    };
                    _context.StockMovements.Add(movement);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products.FindAsync(id);
                    
                    // Track stock change
                    int stockChange = product.StockQuantity - existingProduct.StockQuantity;
                    
                    // Update properties
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.SupplierId = product.SupplierId;
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.MinStockLimit = product.MinStockLimit;
                    existingProduct.SKU = product.SKU;
                    existingProduct.UpdatedAt = DateTime.Now;

                    _context.Update(existingProduct);

                    // Record stock adjustment if quantity changed
                    if (stockChange != 0)
                    {
                        var movement = new StockMovement
                        {
                            ProductId = existingProduct.Id,
                            Quantity = stockChange,
                            MovementType = stockChange > 0 ? "Adjustment IN" : "Adjustment OUT",
                            ReferenceNumber = "ADJUST",
                            Notes = $"Manual adjustment by admin. Previous: {existingProduct.StockQuantity - stockChange}, New: {product.StockQuantity}"
                        };
                        _context.StockMovements.Add(movement);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.Suppliers = _context.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            
            // Check if product has related records
            var hasSales = await _context.SalesOrderItems.AnyAsync(si => si.ProductId == id);
            var hasPurchases = await _context.PurchaseOrderItems.AnyAsync(pi => pi.ProductId == id);
            
            if (hasSales || hasPurchases)
            {
                TempData["Error"] = "Cannot delete product because it has related sales or purchase records.";
                return RedirectToAction(nameof(Index));
            }

            // Delete stock movements first
            var movements = _context.StockMovements.Where(sm => sm.ProductId == id);
            _context.StockMovements.RemoveRange(movements);

            // Delete alert logs
            var alerts = _context.AlertLogs.Where(a => a.ProductId == id);
            _context.AlertLogs.RemoveRange(alerts);

            // Delete product
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Products/UpdateStock/5
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int adjustment, string notes)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            int oldStock = product.StockQuantity;
            product.StockQuantity += adjustment;
            product.UpdatedAt = DateTime.Now;

            // Record stock movement
            var movement = new StockMovement
            {
                ProductId = id,
                Quantity = adjustment,
                MovementType = adjustment > 0 ? "Manual IN" : "Manual OUT",
                ReferenceNumber = "MANUAL",
                Notes = notes ?? $"Manual adjustment. Old: {oldStock}, New: {product.StockQuantity}"
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

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}