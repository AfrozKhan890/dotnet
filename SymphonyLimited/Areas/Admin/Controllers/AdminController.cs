using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SymphonyLimited.ViewModels;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == "admin" && model.Password == "admin123")
                {
                    HttpContext.Session.SetString("AdminUser", model.Username);
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                ModelState.AddModelError("", "Invalid username or password");
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Admin");
        }
    }
}
