namespace cursovaia2.Models
{
    public class ProfileViewModel
    {
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string RoleName { get; set; } = "";
        public DateTime RegisteredAt { get; set; }
        public decimal BonusBalance { get; set; }
    }
}
