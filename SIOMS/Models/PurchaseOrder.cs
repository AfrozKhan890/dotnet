using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIOMS.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        
        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }
        
        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Expected Delivery")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending";
        
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // ✅ ADDED: tracks whether stock has already been increased for this order.
        // Prevents stock from being increased more than once no matter how many times
        // the order is edited or its status is re-saved.
        public bool StockUpdated { get; set; } = false;

        // Navigation
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }
        
        public virtual ICollection<PurchaseOrderItem>? Items { get; set; }
    }
}