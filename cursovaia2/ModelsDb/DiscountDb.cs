using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("discounts")]
    public class DiscountDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "percent"; // percent / fixed

        [Column("value")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Value { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        // Navigation properties
        public virtual ICollection<OrderDb> Orders { get; set; } = new List<OrderDb>();
        public virtual ICollection<ProductDiscountDb> ProductDiscounts { get; set; } = new List<ProductDiscountDb>();
    }
}
