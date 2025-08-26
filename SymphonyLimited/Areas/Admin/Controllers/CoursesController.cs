using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SymphonyLimited.Data;
using SymphonyLimited.Models;
using System.IO;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    
[Area("Admin")]
    public class AdminCoursesController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminCoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin: Manage View
        public IActionResult Manage()
        {
            var courses = _context.Courses.ToList();
            return View(courses); 
        }

        // Admin: Create GET
        public IActionResult Create()
        {
            return View();
        }

        // Admin: Create POST
        [HttpPost]
        public async Task<IActionResult> Create(Course model, IFormFile ImageFile)
        {
            if (ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                model.ImageUrl = "/images/courses/" + fileName;
            }

            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "New course added!";
            return RedirectToAction("Manage");
        }

        // Admin: Edit GET
        public IActionResult Edit(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        // Admin: Edit POST
        [HttpPost]
        public async Task<IActionResult> Edit(Course model, IFormFile ImageFile)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == model.Id);
            if (course == null) return NotFound();

            course.CourseName = model.CourseName;
            course.Description = model.Description;
            course.Duration = model.Duration;
            course.TopicsCovered = model.TopicsCovered;
            course.IsNew = model.IsNew;

            if (ImageFile != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/courses", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                course.ImageUrl = "/images/courses/" + fileName;
            }

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course updated!";
            return RedirectToAction("Manage");
        }

        // Admin: Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            if (!string.IsNullOrEmpty(course.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course deleted successfully!";
            return RedirectToAction("Manage");
        }
    }
}
