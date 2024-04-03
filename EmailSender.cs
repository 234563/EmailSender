using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{

        public interface IEmailSender
        {
            Task SendEmailAsync(MailMessage mailMessage);
        }

        public class EmailSender : IEmailSender
        {
            private  SmtpClient smtp;
            private ILogger Logger;
            public EmailSender(SmtpClient smtpClient , ILogger _logger)
            {
                smtp = smtpClient;
                Logger = _logger;
            }

            public async Task SendEmailAsync(MailMessage mailMessage)
            {
                try
                {
                  
                    smtp.Port = 587; 
                    smtp.Credentials = new NetworkCredential("youremail@gmail.com", "Yourapppasswrod");
                    smtp.EnableSsl = true; 

                    // Send Email
                    await smtp.SendMailAsync(mailMessage);
                
                    Console.WriteLine("Email sent successfully.");
          
           
                 }
                catch (Exception ex)
                {
                    string message = "SendEmailAsync Exception: " + ex.Message + ",DateAndTime:" + DateTime.Now;
                    Console.WriteLine(message);
                    Logger.Log(message);
                }
                finally
                {
                    // Dispose of resources
                    mailMessage.Dispose();
                    smtp.Dispose();
                }

        }
    }
}



