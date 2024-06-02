using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Controllers
{
    public class ReportController
    {
        private readonly ILogger<ReportController> _logger;
        private readonly List<MailTemplate> _mailTemplates;
        public event EventHandler<ProgressEventArgs>? OnProgress;

        public ReportController(ILogger<ReportController> logger, List<MailTemplate> mailTemplates )
        {
            _logger = logger;
            _mailTemplates = mailTemplates;
        }

        public async Task ProcessReport(ReportConfiguration report, IMailConfig mailConfig)
        {
            WorkflowDetailProcessor wfp = new WorkflowDetailProcessor(report);
            wfp.OnProgress += Wfp_OnProgress;
            List<int> processes = report.GetWFProcessList();
            var allInstances = await wfp.GetAllInstancesAsync(int.MaxValue, processes.Count > 0 ? processes : null);
            List<InstanceDetail> allValid = allInstances.Where(x => !string.IsNullOrEmpty(x.UserSMTP)).ToList();
            List<InstanceDetail> notValid = allValid.Where(x => string.IsNullOrEmpty(x.UserSMTP)).GroupBy(x => x.ProcessNo).Select(x => x.First()).ToList();

            var allValidGrouped = allValid.GroupBy(x => x.UserSMTP).Select(wfInst => new { SMTP = wfInst.Key ?? "", Value = wfInst.OrderBy(x => x.TaskStart).ToList() }).ToList();
            _logger.LogInformation($"All Instances: {allInstances.Count}, all valid: {allValid.Count}, all not valid: {notValid.Count}");

            IMailMessageDeliveryAgent mailDeliveryAgent = new SMPTMailMessageDeliveryAgent((SMTPMailConfig) mailConfig);

            var count = 0;
            foreach (var detail in allValidGrouped)
            {
                count++;
                OnProgress?.Invoke(this, new ProgressEventArgs
                {
                    CurrentInstanceCount = count,
                    TotalInstanceCount = allValidGrouped.Count,
                    CurrentProcess = ProgressEventArgs.ProcessingType.Emailing
                });
                var email = report.SendAllToAdmin ? report.AdminEmail : detail.SMTP;
                var mailTemplate = _mailTemplates.Find(x => x.Id == report.MailTemplateId);
                if (mailTemplate == null) { throw new Exception($"Invalid mail template specified in report: {report.MailTemplateId}"); }
                var msg = MailMessageGenerator.CreateMessage(email, ((SMTPMailConfig)mailConfig).From, detail.Value, mailTemplate.EmailSubject, mailTemplate.EmailBody);
                await mailDeliveryAgent.SendMessageAsync(msg);
                report.SetNextRun();
            }
        }

        private void Wfp_OnProgress(object sender, ProgressEventArgs e)
        {
            // worflow detail processing progress event
            // bubble this
            OnProgress?.Invoke(sender, e);
        }
    }
}
