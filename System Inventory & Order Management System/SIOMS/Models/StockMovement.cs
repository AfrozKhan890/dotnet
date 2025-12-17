using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } // Positive = IN, Negative = OUT
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Movement Type")]
        public string MovementType { get; set; } // Purchase, Sale, Adjustment, Return
        
        [StringLength(50)]
        [Display(Name = "Reference Number")]
        public string ReferenceNumber { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        [Required]
        [Display(Name = "Movement Date")]
        public DateTime MovementDate { get; set; } = DateTime.Now;
        
        // Navigation
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}