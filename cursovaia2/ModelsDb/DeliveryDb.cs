using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("deliveries")]
    public class DeliveryDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        [Required]
        public int OrderId { get; set; }

        [Column("address")]
        [Required]
        public string Address { get; set; } = string.Empty;

        [Column("delivery_method_id")]
        public int? DeliveryMethodId { get; set; }

        [Column("status")]
        [StringLength(30)]
        public string Status { get; set; } = "pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual OrderDb? Order { get; set; }

        [ForeignKey("DeliveryMethodId")]
        public virtual DeliveryMethodDb? DeliveryMethod { get; set; }
    }
}
