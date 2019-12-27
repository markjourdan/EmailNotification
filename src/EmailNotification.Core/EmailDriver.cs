using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using EmailNotification.Core.Framework;

namespace EmailNotification.Core
{
    public class EmailDriver
    {
        internal const string ErrorFromAddressNotConfigured =
            "Email Notification: 'from' property is empty. Make sure you configure the default from email address, or pass it in.";

        internal const string ErrorToAddressNotSet = "Email Notification: 'to' property is empty.";
        internal const string ErrorToAddressInvalid = "Email Notification: 'to' property has invalid email address.";
        internal const string ErrorCcAddressInvalid = "Email Notification: 'CC' property has invalid email address.";
        internal const string ErrorBccAddressInvalid = "Email Notification: 'BCC' property has invalid email address.";

        internal const string ErrorFromAddressInvalid =
            "Email Notification: 'From' property has invalid email address.";

        internal const string EmailNotificationFailedToSendEmailTo = "Email Notification: failed to send email to {0}.";

        internal const string SmtpFailedRecipientsExceptionFailToSendEmailTo =
            "Email Notification: The message could not be delivered to the recipient for email {0}";

        internal const string InvalidOperationExceptionFailedToSendEmailTo =
            "Email Notification: A delivery method configuration issue has occurred. Failed to send email to {0}.";

        internal const string ArgumentNullExceptionFailedToSendEmailTo =
            "Email Notification: Your sender or recipient is null for email {0}.";

        internal const string SmtpExceptionFailedToSendEmailTo =
            "Email Notification: A connection to the SMTP server failed. Failed to send email to {0}.";

        internal const string SentTotalOfEmails = "Sent total of {0} emails.";


        /// <summary>
        ///     Send a collection of Queued Emails
        /// </summary>
        /// <returns>Returns total amount of successfully sent emails</returns>
        public static void SendQueuedEmails(Configuration configuration, ISmtpClient smtpClient)
        {
            var clock = new Clock();

            if (!configuration.EmailQueue.Any()) return;

            configuration.EmailQueue = configuration.EmailQueue.OrderByDescending(e => e.Created).ToList();
            var serverConnectionAttempt = 0;

            foreach (var email in configuration.EmailQueue)
                using (var message = GetMailMessage(configuration, email))
                {
                    email.IsSent = false;
                    if (message == null) continue;

                    try
                    {
                        if (IsTestEmailDetected(configuration, email))
                        {
                            email.IsTestEmail = true;
                            email.IsSent = true;
                            email.Sent = clock.UtcNow;
                            continue;
                        }

                        ++serverConnectionAttempt;
                        smtpClient.Send(message);
                        email.IsSent = true;
                        email.Sent = clock.UtcNow;
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
                            string.Format(SmtpExceptionFailedToSendEmailTo, email.To), ex);

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
                        && serverConnectionAttempt % configuration.ServerConfiguration.SmtpServerConnectionLimit == 0)
                        Thread.Sleep(new TimeSpan(0, 0, 0, 15, 0));
                }

            WriteToLog(EventLogEntryType.Information, configuration.Log,
                string.Format(SentTotalOfEmails, configuration.EmailQueue.Count(e => e.IsSent)));
        }

        private static bool IsTestEmailDetected(Configuration configuration, MessageQueueEntity email)
        {
            if (!configuration.IsTestEmailAccountsBlocked) return false;

            var isTestEmailDetected = email.IsTestEmail;

            if (!isTestEmailDetected)
                isTestEmailDetected = configuration.TestEmailAccounts.Any(testEmailAccount =>
                    email.To.IndexOf(testEmailAccount, StringComparison.OrdinalIgnoreCase) < 0);

            return isTestEmailDetected;
        }

        private static MailMessage GetMailMessage(Configuration configuration, MessageQueueEntity email)
        {
            if (string.IsNullOrWhiteSpace(email.From) &&
                string.IsNullOrWhiteSpace(configuration.FromDefaultEmailAddress))
            {
                WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorFromAddressNotConfigured);
                email.SentException = new Exception(ErrorFromAddressNotConfigured);
                return null;
            }

            if (string.IsNullOrWhiteSpace(email.To))
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
                IsBodyHtml = email.BodyFormat == BodyFormat.Html
            };


            try
            {
                message.From = string.IsNullOrWhiteSpace(email.From)
                    ? new MailAddress(configuration.FromDefaultEmailAddress, configuration.FromDefaultEmailName)
                    : new MailAddress(email.From);
            }
            catch (Exception)
            {
                WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorFromAddressInvalid);
                email.SentException = new Exception(ErrorFromAddressInvalid);
                return null;
            }

            foreach (var emailTo in email.To.Split(';', ','))
                try
                {
                    if (!string.IsNullOrWhiteSpace(emailTo))
                        message.To.Add(new MailAddress(emailTo));
                }
                catch (Exception)
                {
                    WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorToAddressInvalid);
                    email.SentException = new Exception(ErrorToAddressInvalid);
                    return null;
                }

            if (!string.IsNullOrWhiteSpace(email.Cc))
                foreach (var emailCc in email.Cc.Split(';', ','))
                    try
                    {
                        message.CC.Add(new MailAddress(emailCc));
                    }
                    catch (Exception)
                    {
                        WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorCcAddressInvalid);
                        email.SentException = new Exception(ErrorCcAddressInvalid);
                        return null;
                    }

            if (!string.IsNullOrWhiteSpace(email.Bcc))
                foreach (var emailBcc in email.Bcc.Split(';', ','))
                    try
                    {
                        message.Bcc.Add(new MailAddress(emailBcc));
                    }
                    catch (Exception)
                    {
                        WriteToLog(EventLogEntryType.Warning, configuration.Log, ErrorBccAddressInvalid);
                        email.SentException = new Exception(ErrorBccAddressInvalid);
                        return null;
                    }

            if (email.Attachments != null && email.Attachments.Any())
                GetMailMessageAttachments(email, message);

            return message;
        }

        private static void GetMailMessageAttachments(MessageQueueEntity email, MailMessage message)
        {
            foreach (var messageQueueAttachmentEntity in email.Attachments)
                if (!string.IsNullOrWhiteSpace(messageQueueAttachmentEntity.FileName))
                {
                    message.Attachments.Add(messageQueueAttachmentEntity.ContentType != null
                        ? new Attachment(messageQueueAttachmentEntity.FileName,
                            messageQueueAttachmentEntity.ContentType)
                        : new Attachment(messageQueueAttachmentEntity.FileName));
                }
                else if (messageQueueAttachmentEntity.ContentStream != null)
                {
                    if (messageQueueAttachmentEntity.ContentType == null)
                        throw new Exception(
                            "The message content type needs to be set when sending in an attachment Stream.");

                    message.Attachments.Add(new Attachment(messageQueueAttachmentEntity.ContentStream,
                        messageQueueAttachmentEntity.ContentType));
                }
        }

        private static string GetServerParametersString(IServerConfiguration serverConfiguration)
        {
            return $"Settings: Host: {serverConfiguration.SmtpServer}\nSSL: {serverConfiguration.IsSSLEnabled}\nRequire Auth: {serverConfiguration.SmtpServerRequiredLogin}\nUsername: {serverConfiguration.SmtpServerUserName}\nPassword: {serverConfiguration.SmtpServerPassword}";
        }

        private static string GetEmailParametersString(MessageQueueEntity email, string fromEmailAddress)
        {
            return $"\n\nParameters: To: {email.To}\nCC: {email.Cc}\nBCC: {email.Bcc}\nSubject: {email.Subject}\nBody: {email.Body}\nBody Format: {email.BodyFormat}\nFrom: {fromEmailAddress}";
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