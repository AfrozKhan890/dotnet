using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using Microsoft.EntityFrameworkCore;

namespace SymphonyLimited.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var about = _context.AboutInfos.FirstOrDefault();
            var courses = _context.Courses.ToList();
            var newCourses = courses.Where(c => c.IsNew).ToList();

            ViewBag.About = about;
            ViewBag.Courses = courses;
            ViewBag.NewCourses = newCourses;

            return View();
        }
    }
}
