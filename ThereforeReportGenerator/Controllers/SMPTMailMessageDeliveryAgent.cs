using System.Net.Mail;
using System.Net;
using System.Text;
using ThereforeReportGenerator.Models;

namespace ThereforeReportGenerator.Controllers
{

    public interface IMailMessageDeliveryAgent
    {
        public Task SendMessageAsync(MailMessage msg);
    }

    class SMPTMailMessageDeliveryAgent : IMailMessageDeliveryAgent
    {
        private SMTPMailConfig _config;
        private SmtpClient _smtpClient;

        public SMPTMailMessageDeliveryAgent(SMTPMailConfig config)
        {
            _config = config;
            _smtpClient = new SmtpClient(_config.MailServer)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_config.UserName, _config.Password)
            };
        }

        public async Task SendMessageAsync(MailMessage msg)
        {
            await _smtpClient.SendMailAsync(msg);
        }

    }

}
