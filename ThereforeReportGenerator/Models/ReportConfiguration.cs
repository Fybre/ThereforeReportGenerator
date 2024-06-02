using Cronos;

namespace ThereforeReportGenerator.Models
{
    /// <summary>
    /// Configuration details to connect to the tenant
    /// </summary>
    public class ReportConfiguration
    {
        private const string SERVICEBASE = "/theservice/v0001/restun";
        private string _CronSchedule = "";
        private List<int> _WFProcesses = new List<int>();

        public int Id { get; set; }
        public bool Enabled { get; set; } = true;
        public string Reportname { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string TenantUrl { get; set; } = string.Empty;
        public string TenantAuthorisation { get; set; } = string.Empty;
        public string AdminEmail {  get; set; } = string.Empty;
        public bool SendAllToAdmin { get; set; } = false;
        public int MailTemplateId {  get; set; }

        public string WFProcesses
        {
            get
            {
                return string.Join(',', _WFProcesses);
            }
            set
            {
                _WFProcesses.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        value.Split(',', StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries).ToList().ForEach(x => _WFProcesses.Add(int.Parse(x)));
                    }
                    catch { }
                }
            }
        }

        public List<int> GetWFProcessList()
        {
            return _WFProcesses;
        }

        public string CronSchedule { get { return _CronSchedule; } set { _CronSchedule = value; SetNextRun(); } }

        public DateTime? NextRun { get; set; }
        public Dictionary<string, string> TenantHeaders
        {
            get
            {
                return new Dictionary<string, string>() { { "TenantName", TenantName }, { "Authorization", TenantAuthorisation } };
            }
        }

        public DateTime? NextRunLocalTime { get
            {
                return NextRun.HasValue? NextRun.Value.ToLocalTime():null;
            } }
        public string TenantBaseUrl { get { return $"{TenantUrl}{SERVICEBASE}"; } }
        public DateTime? SetNextRun()
        {
            NextRun = null;
            try
            {
                CronExpression expression = CronExpression.Parse(CronSchedule);
                var currentDateUTC = DateTime.UtcNow;
                DateTimeOffset? nextLocal = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
                if (nextLocal.HasValue)
                {
                    NextRun = ((DateTimeOffset)nextLocal).ToUniversalTime().UtcDateTime;
                }
            }
            catch { }
            return NextRun;
        }
    }
}
