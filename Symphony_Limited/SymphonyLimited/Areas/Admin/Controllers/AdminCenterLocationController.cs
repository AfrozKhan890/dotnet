using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using SymphonyLimited.Models; 
using System.Linq;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCenterLocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCenterLocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var centers = _context.CenterLocations.ToList();
            return View(centers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CenterLocation center)
        {
            if (ModelState.IsValid)
            {
                _context.CenterLocations.Add(center);
                _context.SaveChanges();
                TempData["Success"] = "Center added successfully.";
                return RedirectToAction("Index");
            }
            return View(center);
        }

        public IActionResult Edit(int id)
        {
            var center = _context.CenterLocations.Find(id);
            if (center == null)
                return NotFound();
            return View(center);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CenterLocation center)
        {
            if (ModelState.IsValid)
            {
                _context.CenterLocations.Update(center);
                _context.SaveChanges();
                TempData["Success"] = "Center updated successfully.";
                return RedirectToAction("Index");
            }
            return View(center);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var center = _context.CenterLocations.Find(id);
            if (center != null)
            {
                _context.CenterLocations.Remove(center);
                _context.SaveChanges();
                TempData["Success"] = "Center deleted successfully.";
            }
            return RedirectToAction("Index");
        }
    }
}
