using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Helpers;
using SIOMS.Models;
using System;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        // ✅ ADDED: single place mapping a currency code to its display symbol,
        // so the symbol always updates automatically and can never drift out of
        // sync with the selected currency.
        private static readonly Dictionary<string, string> CurrencySymbols = new()
        {
            { "PKR", "Rs." },
            { "USD", "$" },
            { "EUR", "€" }
        };

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Settings
        // public async Task<IActionResult> Index()
        // {
        //     var settings = await _context.Settings.FirstOrDefaultAsync();
            
        //     if (settings == null)
        //     {
        //         // Create default settings if none exist
        //         settings = new Settings();
        //         _context.Settings.Add(settings);
        //         await _context.SaveChangesAsync();
        //     }

        //     var settings = await _context.Settings.FirstOrDefaultAsync();
        //     ViewBag.CurrentUsername = settings?.Username ?? "admin";
            
        //     return View(settings);
        // }

public async Task<IActionResult> Index()
{
    var settings = await _context.Settings.FirstOrDefaultAsync();

    if (settings == null)
    {
        settings = new Settings();
        _context.Settings.Add(settings);
        await _context.SaveChangesAsync();
    }

    ViewBag.CurrentUsername = settings.Username ?? "admin";

    return View(settings);
}

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Settings model)
        {
            // Currency/Symbol are derived server-side, not user-typed, so remove any
            // validation state that was computed from the posted values. Username /
            // PasswordHash are managed by the dedicated actions below, not this form.
            ModelState.Remove("CurrencySymbol");
            ModelState.Remove("Username");
            ModelState.Remove("PasswordHash");

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

                    // ✅ FIX: Currency Symbol is now always derived from Currency -
                    // no manual symbol entry, so it can never be wrong/inconsistent.
                    settings.Currency = CurrencySymbols.ContainsKey(model.Currency) ? model.Currency : "PKR";
                    settings.CurrencySymbol = CurrencySymbols[settings.Currency];

                    settings.CompanyName = model.CompanyName;
                    settings.LowStockThreshold = model.LowStockThreshold;
                    settings.TaxRate = model.TaxRate;
                    settings.EnableEmailNotifications = model.EnableEmailNotifications;
                    settings.EnableSMSAlerts = model.EnableSMSAlerts;
                    settings.CompanyAddress = model.CompanyAddress;
                    settings.CompanyPhone = model.CompanyPhone;
                    settings.UpdatedAt = DateTime.Now;
                    
                    await _context.SaveChangesAsync();

                    // ✅ Apply immediately, everywhere, without an app restart.
                    CurrencyFormatter.Symbol = settings.CurrencySymbol;
                    
                    TempData["Success"] = "Settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating settings: {ex.Message}";
                }
            }
            
            var currentSettings = await _context.Settings.FirstOrDefaultAsync();
            ViewBag.CurrentUsername = currentSettings?.Username ?? "admin";
            return View(model);
        }

        // POST: Admin/Settings/ChangeUsername
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUsername(string currentUsername, string newUsername)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();

            if (settings == null || string.IsNullOrWhiteSpace(newUsername)
                || !(currentUsername ?? "").Trim().Equals(settings.Username, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Current username is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            settings.Username = newUsername.Trim();
            settings.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // The session no longer matches the new username - force re-login.
            HttpContext.Session.Clear();

            TempData["Success"] = "Username updated successfully! Please log in again.";
            return RedirectToAction("Login", "Admin");
        }

        // POST: Admin/Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();

            if (settings == null || !PasswordHasher.Verify(currentPassword ?? "", settings.PasswordHash))
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters.";
                return RedirectToAction(nameof(Index));
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Old password is immediately invalidated - the hash is fully replaced.
            settings.PasswordHash = PasswordHasher.Hash(newPassword);
            settings.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Force re-login with the new password.
            HttpContext.Session.Clear();

            TempData["Success"] = "Password updated successfully! Please log in again.";
            return RedirectToAction("Login", "Admin");
        }
    }
}