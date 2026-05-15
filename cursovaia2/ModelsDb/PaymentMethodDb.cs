using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("payment_methods")]
    public class PaymentMethodDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<PaymentDb> Payments { get; set; } = new List<PaymentDb>();
    }
}
