using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using SymphonyLimited.Models;
using System.Linq;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EntranceExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EntranceExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Manage()
        {
            var results = _context.EntranceExamResults.ToList();
            return View(results);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(EntranceExamResult model)
        {
            if (ModelState.IsValid)
            {
                _context.EntranceExamResults.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Result added successfully!";
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var result = _context.EntranceExamResults.Find(id);
            if (result == null) return NotFound();
            return View(result);
        }

        [HttpPost]
        public IActionResult Edit(EntranceExamResult model)
        {
            if (ModelState.IsValid)
            {
                _context.EntranceExamResults.Update(model);
                _context.SaveChanges();
                TempData["Success"] = "Result updated!";
                return RedirectToAction("Manage");
            }
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var result = _context.EntranceExamResults.Find(id);
            if (result != null)
            {
                _context.EntranceExamResults.Remove(result);
                _context.SaveChanges();
                TempData["Success"] = "Deleted successfully!";
            }
            return RedirectToAction("Manage");
        }
    }
}
