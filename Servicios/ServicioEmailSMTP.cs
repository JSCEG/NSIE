
using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace NSIE.Servicios
{

    public interface IServicioEmailSMTP
    {
        Task EnviarCorreo(string destinatario, string asunto, string cuerpo);
    }

    public class ServicioEmailSmtp : IServicioEmailSMTP
    {
        private readonly IConfiguration _configuration;

        public ServicioEmailSmtp(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarCorreo(string destinatario, string asunto, string cuerpo)
        {
            var client = new SmtpClient("smtp.office365.com", 587)
            {
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:Username"]),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(destinatario);

            await client.SendMailAsync(mailMessage);
        }
    }

}
