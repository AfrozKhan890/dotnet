using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SymphonyLimited.Data;
using SymphonyLimited.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SymphonyLimited.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Session Check
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUser")))
            {
                return RedirectToAction("Login", "Admin");
            }

            try
            {
                var dashboardData = await GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading dashboard data";
                return RedirectToAction("Login", "Admin");
            }
        }


        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            return new DashboardViewModel
            {
                TotalCourses = await _context.Courses.CountAsync(),
                NewAdmissionCount = await _context.Admissions
                    .Where(a => a.AdmissionDate >= DateTime.Today.AddDays(-7))
                    .CountAsync(),
                NewMessages = await _context.ContactForms.CountAsync(c => !c.IsRead),
                RecentMessages = await GetRecentMessagesAsync(),
                RecentAdmissions = await GetRecentAdmissionsAsync(),
                RecentCourses = await GetRecentCoursesAsync()
            };
        }

        private async Task<List<Message>> GetRecentMessagesAsync()
        {
            return await _context.ContactForms
                .OrderByDescending(c => c.SubmissionDate)
                .Take(5)
                .Select(c => new Message
                {
                    Name = c.FullName,
                    Subject = c.Subject,
                    Date = c.SubmissionDate
                })
                .ToListAsync();
        }

        private async Task<List<AdmissionVM>> GetRecentAdmissionsAsync()
        {
            return await _context.Admissions
                .OrderByDescending(a => a.AdmissionDate)
                .Take(5)
                .Select(a => new AdmissionVM
                {
                    StudentName = a.StudentName,
                    Email = a.Email,
                    Course = a.Course,
                    AdmissionDate = a.AdmissionDate,
                    Message = a.Message ?? "N/A"
                })
                .ToListAsync();
        }

        private async Task<List<CourseViewModel>> GetRecentCoursesAsync()
        {
            return await _context.Courses
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .Select(c => new CourseViewModel
                {
                    Name = c.CourseName,
                    TopicsCount = !string.IsNullOrWhiteSpace(c.TopicsCovered)
                        ? c.TopicsCovered.Split(',', StringSplitOptions.RemoveEmptyEntries).Length
                        : 0,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }
    }
}
