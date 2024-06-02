using System.Drawing;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Controllers
{
    public static class MailMessageGenerator
    {
        const string OVERDUE_STYLE = "background-color: #d9b9b9;";
        const string EMPTY_TABLE_ROW = "<tr><td style='text-align: center; vertical-align: middle;' colspan='42'>No workflow tasks</td>";
        public static MailMessage CreateMessage(string to, string from, List<InstanceDetail> details, string mailSubjectTemplate, string mailBodyTemplate)
        {
            MailMessage msg = new MailMessage(from, to);

            msg.Subject = GenerateSubject(details, mailSubjectTemplate);
            msg.Body = GenerateBody(details, mailBodyTemplate);

            msg.IsBodyHtml = true;
            return msg;
        }

        private static string GenerateSubject(List<InstanceDetail> details, string template)
        {
            string res = template;
            res = ReplaceTags(res, details.First());
            return res;
        }

        private static string GenerateBody(List<InstanceDetail> details, string template)
        {
            //MailTemplate mailTemplate = new MailTemplate { TemplateContent = "<p>hello [username]</p>[tableallinstances]<table border=1 width=100% style='margin-left: auto; margin-right: auto;><thead><tr style='background-Color:#E8E8E8'><th>Link</th><th>Detail</th></tr>[body]<tr><td><a href='[twaurl]'>[taskname]</a> ([processname])</td><td>[indexdatastring]</td></tr>[/body]</table>[/tableallinstances]<p>Do not reply to this email</p>" };
            string res = template;
            List<InstanceDetail> validDetails = new List<InstanceDetail>();
            foreach (var tableType in new[] { "tableallinstances", "tableoverdueinstances", "tablenotoverdueinstances" })
            {
                switch (tableType)
                {
                    case "tableoverdueinstances":
                        validDetails = details.Where(x => x.IsOverdue).ToList(); break;
                    case "tablenotoverdueinstances":
                        validDetails = details.Where(x => !x.IsOverdue).ToList(); break;
                    default:
                        validDetails = details; break;
                }
                // match the table region
                string tableMatch = Regex.Match(res, $@"\[{tableType}\](?'table'.*)\[\/{tableType}\]").Groups["table"].Value;
                // match the body of the table (table rows)
                string bodyMatch = Regex.Match(tableMatch, $@"\[body\](?'body'.*)\[\/body\]").Groups["body"].Value;

                StringBuilder t = new StringBuilder();
                // add the replacement values for each of the tags found - make a row for each
                foreach (var d in validDetails)
                {
                    t.Append(ReplaceTags(bodyMatch, d));
                }
                var newBody = t.Length > 0? t.ToString(): EMPTY_TABLE_ROW;
                var newtable = ReplaceTagContent(tableMatch, newBody, "body");
                res = ReplaceTagContent(res, newtable, tableType);
            }
            res = ReplaceTags(res, details.First());
            return res;
        }

        static string ReplaceTags(string content, InstanceDetail detail)
        {
            string res = content;
            res = res.Replace("[username]", detail.UserDisplayName);
            res = res.Replace("[usersmtp]", detail.UserSMTP);
            res = res.Replace("[processno]", detail.ProcessNo.ToString());
            res = res.Replace("[processname]", detail.ProcessName);
            res = res.Replace("[processstartdate]", detail.ProcessStartDate.ToLocalTime().ToShortDateString());
            res = res.Replace("[taskname]", detail.TaskName);
            res = res.Replace("[taskstart]", detail.TaskStart.ToLocalTime().ToShortDateString());
            res = res.Replace("[instanceno]", detail.InstanceNo.ToString());
            res = res.Replace("[taskdue]", detail.TaskDue?.ToLocalTime().ToShortDateString() ?? "-");
            res = res.Replace("[assignedtousers]", detail.AssignedToUsers.ToString());
            res = res.Replace("[indexdatastring]", detail.IndexDataString);
            res = res.Replace("[time]", DateTime.Now.ToString());
            res = res.Replace("[twaurl]", detail.TWAInstanceUrl);
            res = res.Replace("[overduestyle]", $"{(detail.IsOverdue? OVERDUE_STYLE : string.Empty)}");
            res = res.Replace("[timezone]", @TimeZoneInfo.Local.ToString());
            res = res.Replace("[currentculture]", Thread.CurrentThread.CurrentCulture.DisplayName);
            return res;
        }

        static string ReplaceTagContent(string contentOriginal, string contentToAdd, string tag)
        {
            return Regex.Replace(contentOriginal, @$"\[{tag}\].*\[\/{tag}\]", contentToAdd);
        }
    }
}
