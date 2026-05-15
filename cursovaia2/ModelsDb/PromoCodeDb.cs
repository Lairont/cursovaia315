using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("promo_codes")]
    public class PromoCodeDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column("discount_id")]
        [Required]
        public int DiscountId { get; set; }

        [Column("usage_limit")]
        public int? UsageLimit { get; set; }

        [Column("times_used")]
        [Range(0, int.MaxValue)]
        public int TimesUsed { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("DiscountId")]
        public virtual DiscountDb? Discount { get; set; }

        public virtual ICollection<OrderDb> Orders { get; set; } = new List<OrderDb>();
    }
}
