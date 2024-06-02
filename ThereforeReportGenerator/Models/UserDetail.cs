namespace ThereforeReportGenerator.Models
{
    public class UserDetail
    {
        public int? UserId { get; set; }
        public string? DisplayName { get; set; }
        public string? Smtp { get; set; }
        public int? UserType { get; set; }
        public bool Disabled { get; set; } = false;
        public string FromGroup { get; set; } = string.Empty;
    }
}
