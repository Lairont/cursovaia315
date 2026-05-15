using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("delivery_methods")]
    public class DeliveryMethodDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("price")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } = 0;

        // Navigation property
        public virtual ICollection<DeliveryDb> Deliveries { get; set; } = new List<DeliveryDb>();
    }
}
