// Controllers/UserCoursesController.cs
using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;

namespace SymphonyLimited.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // User Site: Course List
        public IActionResult Index()
        {
            var courses = _context.Courses.ToList();
            return View(courses); 
        }

        // User Site: Course Detail
        public IActionResult Details(int id)
{
    var course = _context.Courses.FirstOrDefault(c => c.Id == id);
    if (course == null)
    {
        return NotFound();
    }

    return View(course);
}

    }
}
