using System.Reflection.Metadata.Ecma335;

namespace ThereforeReportGenerator.Models
{
    public class ProgressEventArgs : EventArgs
    {
        public enum ProcessingType { Querying, Emailing };

        public ProcessingType CurrentProcess { get; set; } = ProcessingType.Querying;
        public int TotalInstanceCount { get; set; }
        public int CurrentInstanceCount { get; set; }
        public string? TaskName { get; set; } = string.Empty;
        public string? ProcessName { get; set; } = string.Empty;
        public int InstanceNo { get; set; }
        public string IndexDataString { get; set; } = string.Empty;

        public string ProgressSummary { get => $"{CurrentProcess}: {((double)CurrentInstanceCount / (double)TotalInstanceCount):P0} ({CurrentInstanceCount}/{TotalInstanceCount}) {(CurrentProcess==ProcessingType.Querying ? $"{ProcessName} - {TaskName}" : "")}" ; }
        public override string ToString()
        {
            return $"{CurrentInstanceCount}/{TotalInstanceCount} - {((double)CurrentInstanceCount / (double)TotalInstanceCount):P0} ({ProcessName} - {TaskName}), {IndexDataString}";
        }
    }
}
