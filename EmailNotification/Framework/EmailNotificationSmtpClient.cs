using System;
using System.Net;
using System.Net.Mail;

namespace EmailNotification.Framework
{
    public class EmailNotificationSmtpClient : ISmtpClient, IDisposable
    {
        private SmtpClient _client;

        public EmailNotificationSmtpClient(Configuration configuration)
        {
            _client = GetEmailClient(configuration);
        }

        private static SmtpClient GetEmailClient(Configuration configuration)
        {
            var client = new SmtpClient
            {
                Host = configuration.ServerConfiguration.SmtpServer,
                EnableSsl = configuration.ServerConfiguration.IsSSLEnabled,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Port = configuration.ServerConfiguration.SmtpServerPort,
                UseDefaultCredentials = configuration.ServerConfiguration.UseDefaultCredentials
            };

            if (configuration.ServerConfiguration.Timeout.HasValue) client.Timeout = configuration.ServerConfiguration.Timeout.GetValueOrDefault();
            if (configuration.SendCompletedEventHandler != null) client.SendCompleted += configuration.SendCompletedEventHandler;

            if (configuration.ServerConfiguration.SmtpServerRequiredLogin)
                client.Credentials = new NetworkCredential(configuration.ServerConfiguration.SmtpServerUserName, configuration.ServerConfiguration.SmtpServerPassword);
            return client;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void Send(MailMessage message)
        {
            _client.Send(message);
        }
    }

    public interface ISmtpClient
    {
        void Send(MailMessage message);
    }
}
