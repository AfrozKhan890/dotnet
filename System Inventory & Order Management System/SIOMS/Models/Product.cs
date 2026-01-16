// SIOMS/Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class Product : IValidatableObject
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Price must be between $0.01 and $1,000,000")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 1000000, ErrorMessage = "Stock quantity must be between 0 and 1,000,000")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;
        
        [Required(ErrorMessage = "Minimum stock limit is required")]
        [Range(0, 1000000, ErrorMessage = "Minimum stock limit must be between 0 and 1,000,000")]
        [Display(Name = "Minimum Stock Limit")]
        public int MinStockLimit { get; set; } = 10;
        
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; }
        
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }
        
        [NotMapped]
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }
        
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        
        // ✅ ADDED: Custom validation for image
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Image validation: Either file or URL, but not both
            if (ImageFile != null && ImageFile.Length > 0 && !string.IsNullOrWhiteSpace(ImageUrl))
            {
                yield return new ValidationResult(
                    "Please provide either an image file OR an image URL, not both.",
                    new[] { nameof(ImageFile), nameof(ImageUrl) });
            }
            
            // SKU validation: Not required, but if provided, check length
            if (!string.IsNullOrWhiteSpace(SKU) && SKU.Length > 50)
            {
                yield return new ValidationResult(
                    "SKU cannot exceed 50 characters.",
                    new[] { nameof(SKU) });
            }
        }
    }
}