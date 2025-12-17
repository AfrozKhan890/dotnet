using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        
        [StringLength(1000)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        
        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Required]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
        
        [Required]
        [Display(Name = "Minimum Stock Limit")]
        public int MinStockLimit { get; set; }
        
        [StringLength(50)]
        public string SKU { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }
        
        public virtual ICollection<StockMovement> StockMovements { get; set; }
    }
}