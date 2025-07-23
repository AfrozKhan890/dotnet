using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using SymphonyLimited.Models;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FAQController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/FAQ/AdminList
        public IActionResult AdminList()
        {
            var faqs = _context.FAQs.ToList();
            return View(faqs);
        }

        // GET: /Admin/FAQ/Manage
        public IActionResult Manage()
        {
            return View(new FAQ());
        }

        // POST: /Admin/FAQ/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Manage(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                _context.FAQs.Add(faq);
                _context.SaveChanges();
                TempData["Success"] = "FAQ added successfully.";
                return RedirectToAction("AdminList");
            }
            return View(faq);
        }

        // GET: /Admin/FAQ/Edit/5
        public IActionResult Edit(int id)
        {
            var faq = _context.FAQs.Find(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        // POST: /Admin/FAQ/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FAQ faq)
        {
            if (ModelState.IsValid)
            {
                _context.FAQs.Update(faq);
                _context.SaveChanges();
                TempData["Success"] = "FAQ updated successfully.";
                return RedirectToAction("AdminList");
            }
            return View(faq);
        }

        // POST: /Admin/FAQ/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var faq = _context.FAQs.Find(id);
            if (faq != null)
            {
                _context.FAQs.Remove(faq);
                _context.SaveChanges();
                TempData["Success"] = "FAQ deleted successfully.";
            }
            return RedirectToAction("AdminList");
        }
    }
}
