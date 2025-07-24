using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using SymphonyLimited.Models;
using System.Threading.Tasks;

namespace SymphonyLimited.Controllers
{
    public class AdmissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdmissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Admission admission)
        {
            if (ModelState.IsValid)
            {
                _context.Add(admission);
                await _context.SaveChangesAsync();
                ViewBag.Message = "Your admission has been submitted successfully!";
                return View();
            }
            return View(admission);
        }

    }
}
