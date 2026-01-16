using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SIOMS.ViewModels;
using System;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
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
        public IActionResult Login(AdminLoginViewModel model)
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

            // Check credentials
            if (model.Username.Trim().ToLower() == "admin" && model.Password == "admin123")
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
                ModelState.AddModelError("", "Invalid username or password. Try: admin / admin123");
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