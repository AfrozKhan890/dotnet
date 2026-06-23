using Microsoft.AspNetCore.Mvc;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : AdminBaseController
    {
        // GET: Admin/Settings
        public IActionResult Index()
        {
            return View();
        }

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string siteName, string siteEmail, string currency)
        {
            TempData["Success"] = "Settings updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
