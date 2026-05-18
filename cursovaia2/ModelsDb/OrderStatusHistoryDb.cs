using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cursovaia2.ModelsDb
{
    [Table("order_status_history")]
    public class OrderStatusHistoryDb
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        [Required]
        public int OrderId { get; set; }

        [Column("old_status")]
        [StringLength(30)]
        public string? OldStatus { get; set; }

        [Column("new_status")]
        [Required]
        [StringLength(30)]
        public string NewStatus { get; set; } = string.Empty;

        [Column("changed_by_user_id")]
        public int? ChangedByUserId { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Column("comment")]
        public string? Comment { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderDb? Order { get; set; }

        [ForeignKey("ChangedByUserId")]
        public virtual UserDb? ChangedByUser { get; set; }
    }
}
