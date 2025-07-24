using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using System.Linq;

namespace SymphonyLimited.Controllers
{
    public class EntranceExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntranceExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show input form for roll number
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Result(string rollNumber)
        {
            var result = _context.EntranceExamResults.FirstOrDefault(r => r.RollNumber == rollNumber);
            if (result != null)
                return View(result);

            ViewBag.Message = "No such roll number found.";
            return View("Index");
        }
    }
}
