using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Areas.Admin.Models;
using SymphonyLimited.Data;
using Microsoft.AspNetCore.Authorization;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Administrator")]
    public class ContactController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            var messages = _context.ContactForms
                .OrderByDescending(x => x.SubmissionDate)
                .ToList();
            return View(messages);
        }

        public IActionResult Details(int id)
        {
            var message = _context.ContactForms.FirstOrDefault(x => x.Id == id);
            if (message == null) return NotFound();
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var message = _context.ContactForms.Find(id);
            if (message != null)
            {
                _context.ContactForms.Remove(message);
                _context.SaveChanges();
                TempData["Success"] = "Message deleted successfully.";
            }
            return RedirectToAction("List");
        }
    }
}