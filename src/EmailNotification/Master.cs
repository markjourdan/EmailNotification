using System;
using System.Configuration;
using EmailNotification.Config;
using EmailNotification.Framework;

namespace EmailNotification
{
    public static class Master
    {
        public static Configuration Configure()
        {
            return new Configuration();
        }

        public static Configuration UseAppConfig()
        {
            var section = ConfigurationManager.GetSection("EmailNotificationSettings") as EmailNotificationConfig;

            if (section == null)
                throw new ConfigurationErrorsException(
                    "The EmailNotificationSettings section is missing from the configuration file, please add section and rerun the application.");


            var config = new Configuration
                             {
                                 ServerConfiguration =
                                     new DefaultServerConfiguration
                                         {
                                             SmtpServer = section.ServerSettings.ServerName,
                                             SmtpServerPort = section.ServerSettings.Port,
                                             IsSSLEnabled = section.ServerSettings.IsSslEnabled,
                                             SmtpServerConnectionLimit = section.ServerSettings.ConnectionLimit,
                                             SmtpServerPassword = section.ServerSettings.Password,
                                             SmtpServerRequiredLogin = section.ServerSettings.IsLoginRequired,
                                             SmtpServerUserName = section.ServerSettings.UserName,
                                             UseDefaultCredentials = section.ServerSettings.UseDefaultCredentials
                                         },
                                 Enabled = section.IsEnabled,
                                 FromDefaultEmailAddress = section.DefaultFrom.EmailAddress,
                                 FromDefaultEmailName = section.DefaultFrom.DisplayName,
                                 IsTestEmailAccountsBlocked = section.TestEmailAccounts.IsTestEmailAccountsBlocked,
                                 TestEmailAccounts = section.TestEmailAccounts.Accounts
                             };
            return config;
        }

        public static EmailNotificationResult Execute(Configuration configuration)
        {
            using (var client = new EmailNotificationSmtpClient(configuration))
            {
                return Execute(configuration, client);
            }
        }

        public static EmailNotificationResult Execute(Configuration configuration, ISmtpClient smtpClient)
        {
            if (!configuration.Enabled)
                return new EmailNotificationResult(false, configuration.EmailQueue);

            return SendEmails(configuration, smtpClient);
        }

        private static EmailNotificationResult SendEmails(Configuration configuration, ISmtpClient smtpClient)
        {
            try
            {
                EmailDriver.SendQueuedEmails(configuration, smtpClient);
            }
            catch (Exception)
            {
                return new EmailNotificationResult(false, configuration.EmailQueue);
            }

            return new EmailNotificationResult(true, configuration.EmailQueue);
        }
    }
}
