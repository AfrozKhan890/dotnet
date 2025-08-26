    using System;
    using System.Collections.Generic;

    namespace SymphonyLimited.ViewModels
    {
        public class DashboardViewModel
        {
            public int TotalCourses { get; set; }
            public int NewAdmissionCount { get; set; }
            public int UpcomingExams { get; set; }
            public int NewMessages { get; set; }

            public List<Message> RecentMessages { get; set; } = new();
            public List<AdmissionVM> RecentAdmissions { get; set; } = new();
            public List<CourseViewModel> RecentCourses { get; set; } = new();

            public List<string> CourseNames { get; set; } = new();
            public List<int> StudentsPerCourse { get; set; } = new();
            public List<int> MonthlyAdmissions { get; set; } = new();

            public List<string> PopularCourseNames { get; set; } = new();
            public List<int> CoursePopularity { get; set; } = new();
        }

        public class Message
        {
            public string Name { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }
    }

    public class AdmissionVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string? Message { get; set; }
    }

        public class CourseVM
        {
            public string Name { get; set; } = string.Empty;
            public int TopicsCount { get; set; }
            public DateTime CreatedDate { get; set; }
        }
