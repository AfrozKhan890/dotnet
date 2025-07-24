// Controllers/UserAboutController.cs
using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;

namespace SymphonyLimited.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserAbout/Index
        public IActionResult Index()
        {
            var about = _context.AboutInfos.FirstOrDefault();
            return View(about); // Views/UserAbout/Index.cshtml
        }
    }
}
