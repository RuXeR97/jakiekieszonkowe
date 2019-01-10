using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace jakiekieszonkowe_api.Other
{
    public static class Email
    {
        public static void SendEmail(string email, string bodyMessage, string bodySubject, string password)
        {
            MailMessage mail = new MailMessage("jakiekieszonkowe@gmail.com", email);
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("jakiekieszonkowe@gmail.com", "asdopQlzV3uQkg-!@hgQQA");
            client.Host = "smtp.gmail.com";
            mail.Subject = bodySubject;
            mail.Body = bodyMessage;
            client.Send(mail);
        }
    }
}
