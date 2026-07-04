// SIOMS/Models/Settings.cs
using System.ComponentModel.DataAnnotations;

namespace SIOMS.Models
{
    public class Settings
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Site Name")]
        public string SiteName { get; set; } = "SIOMS";
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Site Email")]
        public string SiteEmail { get; set; } = "admin@sioms.com";
        
        [Required]
        [StringLength(10)]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "PKR";
        
        [StringLength(20)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "Rs.";
        
        // ✅ ADDED: CompanyName property
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = "SIOMS Inventory System";
        
        [Display(Name = "Low Stock Alert Threshold")]
        [Range(1, 100)]
        public int LowStockThreshold { get; set; } = 10;
        
        [Display(Name = "Tax Rate %")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 16;
        
        [Display(Name = "Enable Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;
        
        [Display(Name = "Enable SMS Alerts")]
        public bool EnableSMSAlerts { get; set; } = false;
        
        [StringLength(500)]
        [Display(Name = "Company Address")]
        public string CompanyAddress { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Display(Name = "Company Phone")]
        public string CompanyPhone { get; set; } = string.Empty;
        
        // ✅ ADDED: admin login credentials, kept on the existing Settings row
        // (rather than a new module/table) so Change Username / Change Password
        // in Settings can update them directly.
        [StringLength(100)]
        public string Username { get; set; } = "admin";

        // Hashed (never stored in plain text) - see Helpers/PasswordHasher.cs
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}