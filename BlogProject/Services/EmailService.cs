using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PersonalBlog.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalBlog.Services
{
    public class EmailService : IBlogEmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        /// <summary>
        /// Sending an email to me via the Contact form
        /// </summary>
        /// <param name="emailFrom"></param>
        /// <param name="emailName"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMsg"></param>
        /// <returns></returns>
        public async Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMsg)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(_mailSettings.Mail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = $"<b>{name}</b> has sent you an email and can be reached at: <b>{emailFrom}</b><br/><br/>{htmlMsg}";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }

        /// <summary>
        /// Someone getting an email from the app
        /// </summary>
        /// <param name="emailTo"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMsg"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMsg)
        {
            //MIME - multipurpose internet mail extensions protocol
            var email = new MimeMessage();

            //email sender
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);

            //email recipient
            email.To.Add(MailboxAddress.Parse(emailTo));

            email.Subject = subject;

            //var builder = new BodyBuilder();
            //builder.HtmlBody = htmlMessage;
            var builder = new BodyBuilder()
            {
                HtmlBody = htmlMsg
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

            //authenticate
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
