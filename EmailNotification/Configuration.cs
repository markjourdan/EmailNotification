using System.Collections.Generic;
using System.Net.Mail;

namespace EmailNotification
{
    public class Configuration
    {
        internal Configuration()
        {
            ServerConfiguration = new DefaultServerConfiguration();
            Enabled = false;
            IsTestEmailAccountsBlocked = false;
        }

        public Configuration WithServerConfiguration(IServerConfiguration serverConfiguration)
        {
            ServerConfiguration = serverConfiguration;
            return this;
        }

        public Configuration IsEnabled(bool isEnabled)
        {
            Enabled = isEnabled;
            return this;
        }

        public Configuration WithDefaultFromEmail(string emailAddress, string displayName)
        {
            FromDefaultEmailAddress = emailAddress;
            FromDefaultEmailName = displayName;
            return this;
        }

        public Configuration WithEmails(IEnumerable<MessageQueueEntity> emails)
        {
            EmailQueue = emails;
            return this;
        }

        public Configuration IgnoreTestAccounts(bool isTestEmailAccountsBlocked, IEnumerable<string> testEmailAccounts)
        {
            IsTestEmailAccountsBlocked = isTestEmailAccountsBlocked;
            TestEmailAccounts = testEmailAccounts;
            return this;
        }

        public Configuration WithLogger(IEmailLog logger)
        {
            Log = logger;
            return this;
        }

        public Configuration WithSendCompletedEvent(SendCompletedEventHandler sendCompletedEventHandler)
        {
            SendCompletedEventHandler = sendCompletedEventHandler;
            return this;
        }

        internal IServerConfiguration ServerConfiguration { get; set; }
        internal bool Enabled { get; set; }
        internal string FromDefaultEmailAddress { get; set; }
        internal string FromDefaultEmailName { get; set; }
        internal IEnumerable<MessageQueueEntity> EmailQueue { get; set; }
        internal bool IsTestEmailAccountsBlocked { get; set; }
        internal IEnumerable<string> TestEmailAccounts { get; set; }
        internal IEmailLog Log { get; set; }
        internal SendCompletedEventHandler SendCompletedEventHandler { get; set; }
    }

    internal class DefaultServerConfiguration : IServerConfiguration
    {
        internal DefaultServerConfiguration()
        {
            SmtpServer = null;
            SmtpServerPort = 25;
            SmtpServerConnectionLimit = 4;
            SmtpServerPassword = null;
            SmtpServerRequiredLogin = false;
            SmtpServerUserName = null;
            IsSSLEnabled = false;
            UseDefaultCredentials = false;
            Timeout = null;
        }

        public string SmtpServer { get; internal set; }
        public int SmtpServerConnectionLimit { get; internal set; }
        public bool SmtpServerRequiredLogin { get; internal set; }
        public string SmtpServerUserName { get; internal set; }
        public string SmtpServerPassword { get; internal set; }
        public bool IsSSLEnabled { get; internal set; }
        public int SmtpServerPort { get; internal set; }
        public bool UseDefaultCredentials { get; internal set; }
        public int? Timeout { get; internal set; }
    }
}
