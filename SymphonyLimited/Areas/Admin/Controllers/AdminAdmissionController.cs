using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using System.Linq;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAdmissionController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminAdmissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Manage()
        {
            var admissions = _context.Admissions
                .OrderByDescending(a => a.AdmissionDate)
                .ToList();

            return View(admissions);
        }

        public IActionResult Delete(int id)
        {
            var admission = _context.Admissions.Find(id);
            if (admission == null)
                return NotFound();

            _context.Admissions.Remove(admission);
            _context.SaveChanges();
            return RedirectToAction(nameof(Manage));
        }
    }
}
