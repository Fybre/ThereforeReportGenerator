namespace ThereforeReportGenerator.Models
{
    public class MailTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string EmailSubject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;

        public string InvalidEmailSubject {  get; set; } = string.Empty;
        public string InvalidEmailBody {  get; set; } = string.Empty;
    }
}
