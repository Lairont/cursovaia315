using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("product_images")]
    public class ProductImageDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        [Required]
        public int ProductId { get; set; }

        [Column("url")]
        [Required]
        public string Url { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual ProductDb? Product { get; set; }
    }
}
