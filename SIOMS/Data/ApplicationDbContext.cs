// SIOMS/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using SIOMS.Models;

namespace SIOMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<AlertLog> AlertLogs { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrder>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrder>()
                .Property(p => p.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrder>()
                .Property(p => p.GrandTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesOrderItem>()
                .Property(p => p.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrder>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(p => p.TotalPrice)
                .HasPrecision(18, 2);

            // ✅ FIXED: Removed CompanyName from seed data
            // Seed default settings if needed (optional)
            modelBuilder.Entity<Settings>().HasData(
                new Settings
                {
                    Id = 1,
                    SiteName = "SIOMS",
                    SiteEmail = "admin@sioms.com",
                    Currency = "PKR",
                    CurrencySymbol = "Rs.",
                    CompanyName = "SIOMS Inventory System", // ✅ Now exists
                    LowStockThreshold = 10,
                    TaxRate = 16,
                    EnableEmailNotifications = true,
                    EnableSMSAlerts = false,
                    CompanyAddress = "123 Main Street, City",
                    CompanyPhone = "+92-300-1234567",
                    UpdatedAt = DateTime.Now
                }
            );
        }
    }
}