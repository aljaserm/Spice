using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Service
{
    public class EmailSender : IEmailSender
    {
        public EmailOptions emailOptions { get; set; }
        public EmailSender(IOptions<EmailOptions> options)
        {
            emailOptions = options.Value;
        }
        public Task SendEmailAsync(string email, string subject, string Message)
        {
            return execcute(emailOptions.SendGridKey,subject,Message,email);
        }

        private Task execcute(string sendGridKey, string subject, string message, string email)
        {
            var client = new SendGridClient(sendGridKey);
            var msg = new SendGridMessage()
            {
                From=new EmailAddress("Admin@spice.com","Spice"),
                Subject=subject,
                PlainTextContent=message,
                HtmlContent=message
            };
            msg.AddTo(new EmailAddress(email));
            try
            {
                return client.SendEmailAsync(msg);
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
