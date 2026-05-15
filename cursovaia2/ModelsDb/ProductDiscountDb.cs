using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("product_discounts")]
    public class ProductDiscountDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        [Required]
        public int ProductId { get; set; }

        [Column("discount_id")]
        [Required]
        public int DiscountId { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual ProductDb? Product { get; set; }

        [ForeignKey("DiscountId")]
        public virtual DiscountDb? Discount { get; set; }
    }
}
