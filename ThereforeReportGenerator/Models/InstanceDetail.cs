namespace ThereforeReportGenerator.Models
{
    // See https://aka.ms/new-console-template for more information
    public class InstanceDetail
    {
        public int InstanceNo { get; set; }
        public int ProcessNo { get; set; }
        public string? ProcessName { get; set; } = string.Empty;
        public string? TaskName { get; set; } = string.Empty;
        public DateTime TaskStart { get; set; }
        public DateTime? TaskDue { get; set; }
        public DateTime ProcessStartDate { get; set; }
        public int? UserId { get; set; }
        public string? UserDisplayName { get; set; } = string.Empty;
        public string? UserSMTP { get; set; } = string.Empty;
        public List<UserDetail> AssignedToUsers { get; set; } = new List<UserDetail>();
        public List<string> GroupsAssignedTo { get; set; } = new List<string>();
        public string? IndexDataString { get; set; } = string.Empty;
        public string TWAUrl { get; set; } = string.Empty;

        public string TWAInstanceUrl
        {
            get
            {
                return $"{TWAUrl}/Viewer.aspx?InstanceNo={InstanceNo}";
            }
        }

        public string ShortIndexDataString
        {
            get
            {
                return (string.IsNullOrEmpty(IndexDataString) ? string.Empty : (IndexDataString.Length >= 50 ? $"{IndexDataString.Substring(0, 46)}..." : IndexDataString));
            }
        }
        public string GetGroupsAssignedTo()
        {
            return string.Join(',', GroupsAssignedTo.Distinct().ToArray());
        }
        public bool IsOverdue
        {
            get
            {
                if (TaskDue > TaskStart && DateTime.Now > TaskDue) return true; return false;
            }
        }
        public override string ToString()
        {
            return $"Process: {ProcessName}, Instance No: {InstanceNo}, Task: {TaskName}, Task Start: {TaskStart}, Task Due: {TaskDue}, Assigned To: {UserDisplayName}, Overdue: {IsOverdue}, Display: {UserDisplayName}, EMail: {UserSMTP}";
        }

        public List<InstanceDetail> GetFlattenedUserList()
        {
            List<InstanceDetail> res = new List<InstanceDetail>();
            AssignedToUsers.ForEach(u => res.Add(new InstanceDetail
            {
                InstanceNo = this.InstanceNo,
                ProcessName = this.ProcessName,
                ProcessNo = this.ProcessNo,
                ProcessStartDate = this.ProcessStartDate,
                TaskDue = this.TaskDue,
                TaskName = this.TaskName,
                TaskStart = this.TaskStart,
                UserDisplayName = u.DisplayName,
                UserId = u.UserId,
                UserSMTP = u.Smtp,
                IndexDataString = this.IndexDataString,
                TWAUrl = this.TWAUrl
            }));
            return res;
        }
    }

}
