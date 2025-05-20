using ControlDeGastosMVC.API.Models;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;

namespace ControlDeGastosMVC.API.Services
{
    /* public class EmailService : IEmailService
     {
         private readonly IConfiguration _config;
         public EmailService(IConfiguration config)
         {
             _config = config;
         }

         public void SendEmail(EmailDTO request)
         {
             var email = new MimeMessage();
             email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
             email.To.Add(MailboxAddress.Parse(request.Para));
             email.Subject = request.Asunto;
             email.Body = new TextPart(TextFormat.Html)
             {
                 Text = request.Contenido
             };

             using var smtp = new SmtpClient();
             smtp.Connect(
                 _config.GetSection("Email:Host").Value,
                 Convert.ToInt32(_config.GetSection("Email:Port").Value),
                 SecureSocketOptions.StartTls
             );

             smtp.Authenticate(
                 _config.GetSection("Email:UserName").Value,
                 _config.GetSection("Email:Password").Value
             );  
             smtp.Send(email);
             smtp.Disconnect(true);
         }
     }*/

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly string _host;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _host = _config["Email:Host"];
            _port = int.Parse(_config["Email:Port"]);
            _userName = _config["Email:UserName"];
            _password = _config["Email:Password"];
        }

        public void SendEmail(EmailDTO request)
        {
            var message = CreateEmailMessage(request);

            using var smtp = new SmtpClient();
            try
            {
                smtp.Connect(_host, _port, SecureSocketOptions.StartTls);
                smtp.Authenticate(_userName, _password);
                smtp.Send(message);
            }
            finally
            {
                smtp.Disconnect(true);
            }
        }

        private MimeMessage CreateEmailMessage(EmailDTO request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_userName));
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Contenido };
            return email;
        }
    }

}
