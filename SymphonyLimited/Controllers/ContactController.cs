using Microsoft.AspNetCore.Mvc;
using SymphonyLimited.Data;
using SymphonyLimited.ViewModels;
using SymphonyLimited.Models;

namespace SymphonyLimited.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new ContactPageViewModel
            {
                Centers = _context.CenterLocations.ToList(),
                ContactForm = new ContactForm()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactPageViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.ContactForm.SubmissionDate = DateTime.Now;
                _context.ContactForms.Add(model.ContactForm);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your message has been submitted!";
                return RedirectToAction("Index");
            }

            model.Centers = _context.CenterLocations.ToList();
            return View(model);
        }
    }
}