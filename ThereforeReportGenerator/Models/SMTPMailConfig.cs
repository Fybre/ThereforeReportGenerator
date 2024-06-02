using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ThereforeReportGenerator.Models
{

    public interface IMailConfig
    {
        public bool IsValid();
    }

    public class SMTPMailConfig :IMailConfig
    {
        public int Id { get; set; }
        [Required]
        public string MailServer { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string From { get; set; } = string.Empty;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(MailServer);
        }

    }
}
