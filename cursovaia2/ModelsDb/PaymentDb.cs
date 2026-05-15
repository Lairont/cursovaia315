using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("payments")]
    public class PaymentDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        [Required]
        public int OrderId { get; set; }

        [Column("amount")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Column("method_id")]
        public int? MethodId { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual OrderDb? Order { get; set; }

        [ForeignKey("MethodId")]
        public virtual PaymentMethodDb? Method { get; set; }
    }
}
