using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("orders")]
    public class OrderDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("customer_id")]
        [Required]
        public int CustomerId { get; set; }

        [Column("total_price")]
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Column("discount_id")]
        public int? DiscountId { get; set; }

        [Column("promo_code_id")]
        public int? PromoCodeId { get; set; }

        [Column("final_price")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal FinalPrice { get; set; }

        [Column("status")]
        [StringLength(30)]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual CustomerDb? Customer { get; set; }

        [ForeignKey("DiscountId")]
        public virtual DiscountDb? Discount { get; set; }

        [ForeignKey("PromoCodeId")]
        public virtual PromoCodeDb? PromoCode { get; set; }

        public virtual ICollection<OrderItemDb> Items { get; set; } = new List<OrderItemDb>();
        public virtual ICollection<PaymentDb> Payments { get; set; } = new List<PaymentDb>();
        public virtual ICollection<DeliveryDb> Deliveries { get; set; } = new List<DeliveryDb>();
    }
}
