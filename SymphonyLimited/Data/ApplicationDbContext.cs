using Microsoft.EntityFrameworkCore;
using SymphonyLimited.Models;

namespace SymphonyLimited.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        public DbSet<AboutInfo> AboutInfos { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<EntranceExamResult> EntranceExamResults { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<ContactForm> ContactForms { get; set; }
        public DbSet<CenterLocation> CenterLocations { get; set; }
        public DbSet<Admission> Admissions { get; set; }

        
    }
}
