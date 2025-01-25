using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Interfaces;

namespace UserAuthFunctionality.Application.Implementations
{
    public class EmailService:IEmailService
    {
        public void SendEmail(string body, string email, string title, string subject)
        {
            //string url = Url.Action(nameof(ResetPassword), "Account",
            //new { email = appUser.Email, token }, Request.Scheme, Request.Host.ToString());

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("nihadcoding@gmail.com", title);
                mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;
            SmtpClient smtpClient = new()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("nihadcoding@gmail.com", "gulzclohfwjelppj"),
            };
            smtpClient.Send(mailMessage);
        }
    }
}
