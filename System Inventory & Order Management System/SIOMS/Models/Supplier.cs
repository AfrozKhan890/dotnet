// SIOMS/Models/Supplier.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIOMS.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(200)]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [StringLength(500)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(10)]
        public string PostalCode { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        
        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}