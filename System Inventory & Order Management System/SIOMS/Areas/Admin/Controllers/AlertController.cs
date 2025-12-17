using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AlertController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AlertController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Alerts/Index
        public async Task<IActionResult> Index(bool? resolved, string alertType)
        {
            var alerts = _context.AlertLogs
                .Include(a => a.Product)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if (resolved.HasValue)
            {
                alerts = alerts.Where(a => a.IsResolved == resolved.Value);
            }

            if (!string.IsNullOrEmpty(alertType))
            {
                alerts = alerts.Where(a => a.AlertType == alertType);
            }

            ViewBag.ResolvedFilter = resolved;
            ViewBag.AlertTypeFilter = alertType;

            return View(await alerts.OrderByDescending(a => a.AlertDate).ToListAsync());
        }

        // GET: Alerts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var alert = await _context.AlertLogs
                .Include(a => a.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (alert == null)
                return NotFound();

            return View(alert);
        }

        // POST: Alerts/Resolve/5
        [HttpPost]
        public async Task<IActionResult> Resolve(int id, string resolutionNotes)
        {
            var alert = await _context.AlertLogs.FindAsync(id);
            if (alert == null)
                return Json(new { success = false, message = "Alert not found" });

            alert.IsResolved = true;
            alert.ResolvedDate = DateTime.Now;
            alert.ResolutionNotes = resolutionNotes;

            _context.Update(alert);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Alert resolved successfully!" 
            });
        }

        // POST: Alerts/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var alert = await _context.AlertLogs.FindAsync(id);
            if (alert == null)
                return Json(new { success = false, message = "Alert not found" });

            _context.AlertLogs.Remove(alert);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Alert deleted successfully!" 
            });
        }

        // POST: Alerts/CheckLowStock
        [HttpPost]
        public async Task<IActionResult> CheckLowStock()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.MinStockLimit && p.StockQuantity > 0)
                .ToListAsync();

            int newAlerts = 0;
            foreach (var product in lowStockProducts)
            {
                // Check if alert already exists
                var existingAlert = await _context.AlertLogs
                    .Where(a => a.ProductId == product.Id && 
                               a.AlertType == "LowStock" && 
                               !a.IsResolved)
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var alert = new AlertLog
                    {
                        ProductId = product.Id,
                        Message = $"Low stock alert for {product.Name}. Current stock: {product.StockQuantity}, Minimum: {product.MinStockLimit}",
                        AlertType = "LowStock"
                    };
                    _context.AlertLogs.Add(alert);
                    newAlerts++;
                }
            }

            // Check for out of stock
            var outOfStockProducts = await _context.Products
                .Where(p => p.StockQuantity == 0)
                .ToListAsync();

            foreach (var product in outOfStockProducts)
            {
                var existingAlert = await _context.AlertLogs
                    .Where(a => a.ProductId == product.Id && 
                               a.AlertType == "OutOfStock" && 
                               !a.IsResolved)
                    .FirstOrDefaultAsync();

                if (existingAlert == null)
                {
                    var alert = new AlertLog
                    {
                        ProductId = product.Id,
                        Message = $"Out of stock alert for {product.Name}",
                        AlertType = "OutOfStock"
                    };
                    _context.AlertLogs.Add(alert);
                    newAlerts++;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Stock check completed. {newAlerts} new alerts created." 
            });
        }

        // GET: Alerts/GetUnresolvedCount
        [HttpGet]
        public async Task<IActionResult> GetUnresolvedCount()
        {
            var count = await _context.AlertLogs
                .Where(a => !a.IsResolved)
                .CountAsync();

            return Json(new { count = count });
        }
    }
}