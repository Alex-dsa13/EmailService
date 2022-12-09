using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailSendApp
{
    public class EmailService
    {

        private string _emailSender = "quito2813@yandex.ru";
        private string _login;
        private string _password;
        private string _pathToData = "data.json";
        private string _serverResponse = "";


        public string ServerResponse { get { return _serverResponse; } }

        private void getEmailData()
        {
            using (StreamReader filestream = new StreamReader(_pathToData))
            {
                string login = "";
                string password = "";

                string json = filestream.ReadToEnd();
                var js = Newtonsoft.Json.Linq.JArray.Parse(json);

                foreach (var data in js)
                {
                    login = data["Login"]!.ToString();
                    password = data["Password"]!.ToString();
                }

                _login = login;
                _password = password;
            }
        }


        public EmailService()
        {
            getEmailData();
        }
        public async Task SendEmailAsync(string email, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.Subject = "It's a email from EmailService";
            emailMessage.From.Add(new MailboxAddress("My company", _emailSender));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                     await client.ConnectAsync("smtp.yandex.ru", 465, true);
                     await client.AuthenticateAsync(_login, _password);

                     await client.SendAsync(emailMessage);

                    _serverResponse = "Message sended!";
                    
                }
                catch(Exception ex)
                {
                    _serverResponse = ex.Message;
                }
            }
        }
    }
}
