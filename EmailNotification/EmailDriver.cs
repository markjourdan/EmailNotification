using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace EmailNotification
{
    public class EmailDriver
    {
        internal const string ErrorFromAddressNotConfigured = "Email Notification: 'from' property is empty. Make sure you configure store email.";
        internal const string EmailNotificationFailedToSendEmailTo = "Email Notification: failed to send email to {0}.";
        internal const string SentTotalOfEmails = "Sent total of {0} emails.";
        
        /// <summary>
        /// Send a collection of Queued Emails
        /// </summary>
        /// <returns>Returns total amount of successfully sent emails</returns>
        public static void SendQueuedEmails(Configuration configuration)
        {
            if (configuration.EmailQueue.Count() == 0) return;
            
            configuration.EmailQueue = configuration.EmailQueue.OrderByDescending(e => e.Created);
            var serverConnectionAttempt = 0;
            var client = GetEmailClient(configuration.ServerConfiguration);

            foreach (var email in configuration.EmailQueue)
            {
                var message = GetMailMessage(configuration, email);

                try
                {
                    if (IsTestEmailDetected(configuration, email))
                    {
                        email.IsTestEmail = true;
                        continue;
                    }
                    ++serverConnectionAttempt;
                    client.Send(message);
                    email.IsSent = true;
                    email.Sent = DateTime.UtcNow;
                }
                catch (SmtpFailedRecipientsException ex)
                {
                    WriteToLog(EventLogEntryType.Warning, configuration.Log,
                               String.Format(
                                   EmailNotificationFailedToSendEmailTo,
                                   message.To), ex);
                }
                catch (ArgumentNullException ex)
                {
                    WriteToLog(EventLogEntryType.Warning, configuration.Log,
                               String.Format(
                                   EmailNotificationFailedToSendEmailTo,
                                   message.To), ex);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    WriteToLog(EventLogEntryType.Warning, configuration.Log,
                               String.Format(
                                   EmailNotificationFailedToSendEmailTo,
                                   message.To), ex);
                }
                catch (SmtpException ex)
                {
                    WriteToLog(EventLogEntryType.Error, configuration.Log, string.Format(EmailNotificationFailedToSendEmailTo, email.To), ex);

                    var parameters = GetServerParametersString(configuration.ServerConfiguration);
                    parameters += GetEmailParametersString(email, message.From.Address);
                    configuration.Log.Error(parameters);
                }
                catch (InvalidOperationException ex)
                {
                    WriteToLog(EventLogEntryType.Error, configuration.Log,
                               String.Format(EmailNotificationFailedToSendEmailTo, message.To), ex);

                    var parameters = GetServerParametersString(configuration.ServerConfiguration);
                    parameters += GetEmailParametersString(email, message.From.Address);
                    configuration.Log.Error(parameters);
                }
                catch (Exception ex)
                {
                    WriteToLog(EventLogEntryType.Error, configuration.Log,
                               String.Format(EmailNotificationFailedToSendEmailTo, message.To), ex);

                    var parameters = GetServerParametersString(configuration.ServerConfiguration);
                    parameters += GetEmailParametersString(email, message.From.Address);
                    configuration.Log.Error(parameters);
                }

                if (configuration.ServerConfiguration.SmtpServerConnectionLimit > 0
                    && (serverConnectionAttempt % configuration.ServerConfiguration.SmtpServerConnectionLimit) == 0)
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 15, 0));
                }
            }

            WriteToLog(EventLogEntryType.Information, configuration.Log, String.Format(SentTotalOfEmails, configuration.EmailQueue.Where(e => e.IsSent).Count()));
        }

        private static bool IsTestEmailDetected(Configuration configuration, MessageQueueEntity email)
        {
            if (!configuration.IsTestEmailAccountsBlocked) return false;

            var isTestEmailDetected = email.IsTestEmail;
            
            if (!isTestEmailDetected)
            {
                isTestEmailDetected = configuration.TestEmailAccounts.Where(testEmailAccount =>
                                                                            email.To.IndexOf(testEmailAccount,
                                                                                             StringComparison.
                                                                                                 OrdinalIgnoreCase) < 0)
                    .Any();
            }

            return isTestEmailDetected;
        }

        private static MailMessage GetMailMessage(Configuration configuration, MessageQueueEntity email)
        {
            var message = new MailMessage
                              {
                                  Priority = email.Priority,
                                  SubjectEncoding = Encoding.ASCII,
                                  Subject = email.Subject,
                                  BodyEncoding = Encoding.ASCII,
                                  Body = email.Body,
                                  IsBodyHtml = email.BodyFormat == BodyFormat.Html,
                                  From =
                                      email.From == String.Empty
                                          ? new MailAddress(configuration.FromDefaultEmailAddress, configuration.FromDefaultEmailName)
                                          : new MailAddress(email.From)
                              };

            if (String.IsNullOrWhiteSpace(email.From))
            {
                WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorFromAddressNotConfigured);
                return message;
            }

            foreach (var emailTo in email.To.Split(new[] { ';', ',' }))
            {
                message.To.Add(new MailAddress(emailTo));
            }

            if (!String.IsNullOrWhiteSpace(email.Cc))
            {
                foreach (var emailCc in email.Cc.Split(new[] { ';', ',' }))
                {
                    message.CC.Add(new MailAddress(emailCc));                                    
                }
            }

            if (!String.IsNullOrWhiteSpace(email.Bcc) && String.Equals(email.Bcc, email.To, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var emailBcc in email.Bcc.Split(new[] { ';', ',' }))
                {
                    message.Bcc.Add(new MailAddress(emailBcc));
                }
            }
            return message;
        }

        private static SmtpClient GetEmailClient(IServerConfiguration serverConfiguration)
        {
            var client = new SmtpClient
                             {
                                 Host = serverConfiguration.SmtpServer,
                                 EnableSsl = serverConfiguration.IsSSLEnabled,
                                 DeliveryMethod = SmtpDeliveryMethod.Network,
                                 Port = serverConfiguration.SmtpServerPort,
                                 UseDefaultCredentials = serverConfiguration.UseDefaultCredentials
                             };

            if (serverConfiguration.Timeout.HasValue) client.Timeout = serverConfiguration.Timeout.GetValueOrDefault();

            if (serverConfiguration.SmtpServerRequiredLogin)
                client.Credentials = new NetworkCredential(serverConfiguration.SmtpServerUserName, serverConfiguration.SmtpServerPassword);
            return client;
        }

        private static string GetServerParametersString(IServerConfiguration serverConfiguration)
        {
            return string.Format("Settings: Host: {0}\nSSL: {1}\nRequire Auth: {2}\nUsername: {3}\nPassword: {4}",
                                 serverConfiguration.SmtpServer, serverConfiguration.IsSSLEnabled,
                                 serverConfiguration.SmtpServerRequiredLogin,
                                 serverConfiguration.SmtpServerUserName, serverConfiguration.SmtpServerPassword);
        }

        private static string GetEmailParametersString(MessageQueueEntity email, string fromEmailAddress)
        {
            return string.Format(
                            "\n\nParameters: To: {0}\nCC: {1}\nBCC: {2}\nSubject: {3}\nBody: {4}\nBody Format: {5}\nFrom: {6}",
                            email.To, email.Cc, email.Bcc, email.Subject, email.Body, email.BodyFormat,
                            fromEmailAddress);
        }

        private static void WriteToLog(EventLogEntryType type, IEmailLog log, string message)
        {
            if (log == null) return;

            switch (type)
            {
                case EventLogEntryType.Error:
                    log.Error(type.GetType().Name + message);
                    break;
                case EventLogEntryType.Warning:
                    log.Warn(type.GetType().Name + message);
                    break;
                default:
                    log.Info(type.GetType().Name + message);
                    break;
            }
        }

        private static void WriteToLog(EventLogEntryType type, IEmailLog log, string message, Exception ex)
        {
            if (log == null) return;

            switch (type)
            {
                case EventLogEntryType.Error:
                    log.Error(type.GetType().Name + message, ex);
                    break;
                case EventLogEntryType.Warning:
                    log.Warn(type.GetType().Name + message, ex);
                    break;
                default:
                    log.Info(type.GetType().Name + message, ex);
                    break;
            }
        }
    }
}
