using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PurchaseOrderId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }
        
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }
        
        // Navigation
        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}