// Controllers/AdminAboutController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SymphonyLimited.Data;
using SymphonyLimited.Models;
using System.IO;
using System.Threading.Tasks;

namespace SymphonyLimited.Areas.Admin.Controllers
{   
    
    [Area("Admin")] 
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /About/Manage
        [HttpGet]
        public IActionResult Manage()
        {
            var aboutInfo = _context.AboutInfos.FirstOrDefault();
            if (aboutInfo == null)
                aboutInfo = new AboutInfo();

            return View("Manage", aboutInfo); // Views/AdminAbout/Manage.cshtml
        }

        // POST: /AdminAbout/Manage
        [HttpPost]
        public async Task<IActionResult> Manage(AboutInfo model, IFormFile InstitutionImage)
        {
            var aboutInfo = _context.AboutInfos.FirstOrDefault();

            if (aboutInfo == null)
            {
                if (InstitutionImage != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(InstitutionImage.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/institution", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await InstitutionImage.CopyToAsync(stream);
                    }
                    model.InstitutionImagePath = "/images/institution/" + fileName;
                }

                _context.AboutInfos.Add(model);
            }
            else
            {
                aboutInfo.InstitutionName = model.InstitutionName;
                aboutInfo.Vision = model.Vision;
                aboutInfo.Mission = model.Mission;
                aboutInfo.CoreValues = model.CoreValues;
                aboutInfo.FounderMessage = model.FounderMessage;
                aboutInfo.FounderName = model.FounderName;
                aboutInfo.History = model.History;

                if (InstitutionImage != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(InstitutionImage.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/institution", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await InstitutionImage.CopyToAsync(stream);
                    }
                    aboutInfo.InstitutionImagePath = "/images/institution/" + fileName;
                }

                _context.AboutInfos.Update(aboutInfo);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "About information updated successfully!";
            return RedirectToAction("Manage");
        }
    }
}
