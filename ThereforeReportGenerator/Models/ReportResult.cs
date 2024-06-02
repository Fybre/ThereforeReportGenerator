namespace ThereforeReportGenerator.Models
{
    public class ReportResult
    {
        public int AllInstances { get; set; }
        public int ValidInstances { get; set; }
        public int InValidInstances { get; set; }   
        public string StatusResult { get; set; } = string.Empty;

    }
}
