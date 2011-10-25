﻿using System;
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
        internal const string ErrorToAddressNotSet = "Email Notification: 'to' property is empty.";
        internal const string EmailNotificationFailedToSendEmailTo = "Email Notification: failed to send email to {0}.";
        internal const string SmtpFailedRecipientsExceptionFailToSendEmailTo =
            "Email Notification: The message could not be delivered to the recipient for email {0}";
        internal const string InvalidOperationExceptionFailedToSendEmailTo =
            "Email Notification: A delivery method configuration issue has occurred. Failed to send email to {0}.";
        internal const string ArgumentNullExceptionFailedToSendEmailTo = "Email Notification: Your sender or recipient is null for email {0}.";
        internal const string SmtpExceptionFaildToSendEmailTo = "Email Notification: A connection to the SMTP server failed. Failed to send email to {0}.";
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

            using (var client = GetEmailClient(configuration))
            {
                foreach (var email in configuration.EmailQueue)
                {
                    using (var message = GetMailMessage(configuration, email))
                    {
                        if (message == null) continue;

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
                                       string.Format(SmtpFailedRecipientsExceptionFailToSendEmailTo, message.To), ex);

                            email.SentException = ex;
                        }
                        catch (ArgumentNullException ex)
                        {
                            WriteToLog(EventLogEntryType.Warning, configuration.Log,
                                       string.Format(ArgumentNullExceptionFailedToSendEmailTo, message.To), ex);

                            email.SentException = ex;                            
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            WriteToLog(EventLogEntryType.Warning, configuration.Log,
                                       string.Format(EmailNotificationFailedToSendEmailTo, message.To), ex);

                            email.SentException = ex;
                        }
                        catch (SmtpException ex)
                        {
                            WriteToLog(EventLogEntryType.Error, configuration.Log,
                                       string.Format(SmtpExceptionFaildToSendEmailTo, email.To), ex);

                            var parameters = GetServerParametersString(configuration.ServerConfiguration);
                            parameters += GetEmailParametersString(email, message.From.Address);
                            configuration.Log.Error(parameters);

                            email.SentException = ex;
                        }
                        catch (InvalidOperationException ex)
                        {
                            WriteToLog(EventLogEntryType.Error, configuration.Log,
                                       string.Format(InvalidOperationExceptionFailedToSendEmailTo, message.To), ex);

                            var parameters = GetServerParametersString(configuration.ServerConfiguration);
                            parameters += GetEmailParametersString(email, message.From.Address);
                            configuration.Log.Error(parameters);

                            email.SentException = ex;
                        }
                        catch (Exception ex)
                        {
                            WriteToLog(EventLogEntryType.Error, configuration.Log,
                                       string.Format(EmailNotificationFailedToSendEmailTo, message.To), ex);

                            var parameters = GetServerParametersString(configuration.ServerConfiguration);
                            parameters += GetEmailParametersString(email, message.From.Address);
                            configuration.Log.Error(parameters);

                            email.SentException = ex;
                        }

                        if (configuration.ServerConfiguration.SmtpServerConnectionLimit > 0
                            &&
                            (serverConnectionAttempt%configuration.ServerConfiguration.SmtpServerConnectionLimit) == 0)
                        {
                            Thread.Sleep(new TimeSpan(0, 0, 0, 15, 0));
                        }
                    }
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
            if (String.IsNullOrWhiteSpace(email.From) && String.IsNullOrWhiteSpace(configuration.FromDefaultEmailAddress))
            {
                WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorFromAddressNotConfigured);
                email.SentException = new Exception(ErrorFromAddressNotConfigured);
                return null;
            }

            if (String.IsNullOrWhiteSpace(email.To))
            {
                WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorFromAddressNotConfigured);
                email.SentException = new Exception(ErrorToAddressNotSet);
                return null;
            }

            var message = new MailMessage
                              {
                                  Priority = email.Priority,
                                  SubjectEncoding = Encoding.ASCII,
                                  Subject = email.Subject,
                                  BodyEncoding = Encoding.ASCII,
                                  Body = email.Body,
                                  IsBodyHtml = email.BodyFormat == BodyFormat.Html,
                                  From =
                                      String.IsNullOrWhiteSpace(email.From)
                                          ? new MailAddress(configuration.FromDefaultEmailAddress, configuration.FromDefaultEmailName)
                                          : new MailAddress(email.From)
                              };

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

            if (email.Attachments != null && email.Attachments.Any())
                GetMailMessageAttachments(email, message);

            return message;
        }

        private static void GetMailMessageAttachments(MessageQueueEntity email, MailMessage message)
        {
            foreach (var messageQueueAttachementEntity in email.Attachments)
            {
                if (!String.IsNullOrWhiteSpace(messageQueueAttachementEntity.FileName))
                {
                    message.Attachments.Add(messageQueueAttachementEntity.ContentType != null
                                                ? new Attachment(messageQueueAttachementEntity.FileName, messageQueueAttachementEntity.ContentType)
                                                : new Attachment(messageQueueAttachementEntity.FileName));
                }
                else if (messageQueueAttachementEntity.ContentStream != null)
                {
                    if(messageQueueAttachementEntity.ContentType == null)
                        throw new Exception("The message content type needs to be set when sending in an attachment Stream.");

                    message.Attachments.Add(new Attachment(messageQueueAttachementEntity.ContentStream, messageQueueAttachementEntity.ContentType));
                }
            }
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
