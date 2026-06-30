using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new Settings();
                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }
            
            return View(settings);
        }

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Settings model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var settings = await _context.Settings.FirstOrDefaultAsync();
                    
                    if (settings == null)
                    {
                        settings = new Settings();
                        _context.Settings.Add(settings);
                    }
                    
                    // Update properties
                    settings.SiteName = model.SiteName;
                    settings.SiteEmail = model.SiteEmail;
                    settings.Currency = model.Currency;
                    settings.CurrencySymbol = model.CurrencySymbol;
                    settings.CompanyName = model.CompanyName; // ✅ Now exists
                    settings.LowStockThreshold = model.LowStockThreshold;
                    settings.TaxRate = model.TaxRate;
                    settings.EnableEmailNotifications = model.EnableEmailNotifications;
                    settings.EnableSMSAlerts = model.EnableSMSAlerts;
                    settings.CompanyAddress = model.CompanyAddress;
                    settings.CompanyPhone = model.CompanyPhone;
                    settings.UpdatedAt = DateTime.Now;
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating settings: {ex.Message}";
                }
            }
            
            return View(model);
        }
    }
}