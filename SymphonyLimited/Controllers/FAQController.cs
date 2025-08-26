// Controllers/UserFAQController.cs
using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;

namespace SymphonyLimited.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserFAQ
        public IActionResult Index()
        {
            var faqs = _context.FAQs.ToList();
            return View(faqs); 
        }
    }
}
