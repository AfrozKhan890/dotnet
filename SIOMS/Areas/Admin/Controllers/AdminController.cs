using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Helpers;
using SIOMS.Models;
using SIOMS.ViewModels;
using System;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminController(ILogger<AdminController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ✅ ADDED: seeds default admin/admin123 credentials onto the existing
        // Settings row the first time the app runs against a fresh database, so
        // login keeps working out of the box while still being changeable from
        // Settings afterwards. Does not introduce any new table/module.
        private async Task<Settings> GetOrCreateSettingsAsync()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new Settings
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.Hash("admin123")
                };
                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }
            else if (string.IsNullOrEmpty(settings.PasswordHash))
            {
                // Settings row existed before this feature - seed credentials once.
                settings.Username = "admin";
                settings.PasswordHash = PasswordHasher.Hash("admin123");
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        public IActionResult Login()
        {
            _logger.LogInformation("Login page accessed");
            
            // Check if already logged in
            var user = HttpContext.Session.GetString("AdminUser");
            if (!string.IsNullOrEmpty(user))
            {
                _logger.LogInformation($"User {user} already logged in, redirecting to dashboard");
                return RedirectToAction("Index", "Dashboard");
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            _logger.LogInformation($"Login attempt - Username: {model?.Username}");
            
            if (model == null)
            {
                _logger.LogWarning("Login model is null");
                ModelState.AddModelError("", "Please enter username and password");
                return View();
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                _logger.LogWarning("Empty username or password");
                ModelState.AddModelError("", "Username and password are required");
                return View(model);
            }

            // ✅ FIX: Check credentials against the existing Settings row (so
            // changing username/password from Settings actually takes effect)
            // instead of the old hardcoded "admin" / "admin123" check. Login
            // architecture (session-based) is unchanged.
            var settings = await GetOrCreateSettingsAsync();

            if (model.Username.Trim().Equals(settings.Username, StringComparison.OrdinalIgnoreCase)
                && PasswordHasher.Verify(model.Password, settings.PasswordHash))
            {
                try
                {
                    // Set session
                    HttpContext.Session.SetString("AdminUser", model.Username);
                    HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    HttpContext.Session.SetInt32("UserId", 1);
                    
                    // Test if session is working
                    var sessionTest = HttpContext.Session.GetString("AdminUser");
                    _logger.LogInformation($"Session set successfully: {sessionTest}");
                    
                    // Set TempData for success message
                    TempData["Success"] = $"Welcome back, {model.Username}!";
                    
                    // Log successful login
                    _logger.LogInformation($"User {model.Username} logged in successfully");
                    
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Session error during login");
                    ModelState.AddModelError("", "Session error. Please try again.");
                    return View(model);
                }
            }
            else
            {
                _logger.LogWarning($"Invalid login attempt: {model.Username}");
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("AdminUser");
            
            // Clear session
            HttpContext.Session.Clear();
            
            // Expire session cookie
            Response.Cookies.Delete("SIOMS.Session");
            
            _logger.LogInformation($"User {username} logged out");
            TempData["Success"] = "You have been logged out successfully.";
            
            return RedirectToAction("Login", "Admin");
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            var user = HttpContext.Session.GetString("AdminUser");
            var loginTime = HttpContext.Session.GetString("LoginTime");
            var sessionId = HttpContext.Session.Id;
            
            return Json(new
            {
                isAuthenticated = !string.IsNullOrEmpty(user),
                username = user,
                loginTime = loginTime,
                sessionId = sessionId,
                sessionKeys = HttpContext.Session.Keys
            });
        }

        [HttpGet]
        public IActionResult SetTestSession()
        {
            // Test endpoint to manually set session
            HttpContext.Session.SetString("AdminUser", "testadmin");
            HttpContext.Session.SetString("TestTime", DateTime.Now.ToString());
            
            return Content("Test session set. Username: testadmin");
        }
    }
}