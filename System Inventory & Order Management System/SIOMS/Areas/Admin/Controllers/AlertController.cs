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

        // GET: Admin/Alert
        public async Task<IActionResult> Index(bool? resolved)
        {
            var alerts = _context.AlertLogs
                .Include(a => a.Product)
                .AsQueryable();

            if (resolved.HasValue)
            {
                alerts = alerts.Where(a => a.IsResolved == resolved.Value);
            }

            return View(await alerts.OrderByDescending(a => a.AlertDate).ToListAsync());
        }

        // POST: Admin/Alert/Resolve/5
        [HttpPost]
        public async Task<IActionResult> Resolve(int id, string resolutionNotes)
        {
            var alert = await _context.AlertLogs.FindAsync(id);
            if (alert == null) return NotFound();

            alert.IsResolved = true;
            alert.ResolvedDate = DateTime.Now;
            alert.ResolutionNotes = resolutionNotes;

            _context.Update(alert);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Alert resolved successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Alert/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var alert = await _context.AlertLogs.FindAsync(id);
            if (alert == null) return NotFound();

            _context.AlertLogs.Remove(alert);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Alert deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}