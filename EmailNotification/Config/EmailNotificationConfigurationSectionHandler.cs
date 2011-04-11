using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EmailNotification.Config
{
    /// <summary>
    /// Class to register for the EmailNotificationSettings section of the configuration file
    /// </summary>
    /// <remarks>
    /// The EmailNotificationSettings section of the configuration file needs to have a section
    /// handler registered. This is the section handler used. It simply returns
    /// the XML element that is the root of the section.
    /// </remarks>
    /// <example>
    /// Example of registering the EmailNotificationSettings section handler :
    /// <code lang="XML" escaped="true">
    /// <configuration>
    ///		<configSections>
    ///         <section name="EmailNotificationSettings" type="EmailNotification.EmailNotificationConfiguration, EmailNotification" allowLocation="false" allowDefinition="Everywhere"/>
    ///		</configSections>
    ///		<EmailNotificationSettings isEnabled="true">
    ///         <ServerSettings smtpServer="smtp.gmail.com" isSSLEnabled="true" smtpServerConnectionLimit="4" smtpServerPassword="password" 
    ///                smtpServerUser="test@gmail.com" smtpServerRequiredLogin="true"/>
    ///         <DefaultFrom emailAddress="store@gmail.com" displayName="Store"></DefaultFrom>
    ///         <TestEmailAccounts isTestEmailAccountsBlocked="true">
    ///           <add account="mailinator.com"></add>
    ///         </TestEmailAccounts>
    ///     </EmailNotificationSettings>
    /// </configuration>
    /// </code>
    /// </example>
    public class EmailNotificationConfigurationSectionHandler : IConfigurationSectionHandler
    {
		#region Implementation of IConfigurationSectionHandler

		/// <summary>
		/// Parses the configuration section.
		/// </summary>
		/// <param name="parent">The configuration settings in a corresponding parent configuration section.</param>
		/// <param name="configContext">The configuration context when called from the ASP.NET configuration system. Otherwise, this parameter is reserved and is a null reference.</param>
		/// <param name="section">The <see cref="XmlNode" /> for the log4net section.</param>
        /// <returns>The <see cref="XmlNode" /> for the EmailNotificationSettings section.</returns>
		/// <remarks>
		/// <para>
		/// Returns the <see cref="XmlNode"/> containing the configuration data,
		/// </para>
		/// </remarks>
		public object Create(object parent, object configContext, XmlNode section)
		{
		    var xml = section.OuterXml;
            if (string.IsNullOrEmpty(xml))
            {
                throw new ConfigurationErrorsException("Missing configuration");
            }
            var settings = XElement.Parse(xml);
            var settingAttributes = settings.Attributes();
		    var settingElements = settings.Elements();
            if (Equals(settingAttributes.Count(), 0) && Equals(settingElements.Count(), 0))
            {
                throw new ConfigurationErrorsException("There were no settings configured for the Email Notifications");
            }

            var isEnabled = settings.Attribute("isEnabled");

		    var config = new EmailNotificationConfig
		                     {
		                         IsEnabled = isEnabled == null ? false : Convert.ToBoolean(isEnabled.Value)
		                     };

            foreach (var setting in settingElements)
            {
                if (string.Equals(setting.Name.LocalName, "ServerSettings", StringComparison.OrdinalIgnoreCase))
                {
                    GetServerSettings(setting, config);
                }

                if (string.Equals(setting.Name.LocalName, "DefaultFrom", StringComparison.OrdinalIgnoreCase))
                {
                    GetDefaultFrom(setting, config);
                }

                if (string.Equals(setting.Name.LocalName, "TestEmailAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    GetTestEmailAccounts(setting, config);
                }
            }

            return config;
		}
        
        #endregion Implementation of IConfigurationSectionHandler

        private static void GetTestEmailAccounts(XElement setting, EmailNotificationConfig config)
        {
            var isBlocked = setting.Attribute("isTestEmailAccountsBlocked");
            if (isBlocked != null)
                config.TestEmailAccounts.IsTestEmailAccountsBlocked = Convert.ToBoolean(isBlocked.Value);

            foreach (var settingElement in setting.Elements())
            {
                if (string.Equals(settingElement.Name.LocalName, "Add", StringComparison.OrdinalIgnoreCase))
                {
                    var account = settingElement.Attribute("account");
                    if (account != null)
                        config.TestEmailAccounts.Accounts.Add(account.Value);
                }
            }
        }

        private static void GetDefaultFrom(XElement setting, EmailNotificationConfig config)
        {
            var emailAddress = setting.Attribute("emailAddress");
            if (emailAddress != null)
                config.DefaultFrom.EmailAddress = emailAddress.Value;

            var displayName = setting.Attribute("displayName");
            if (displayName != null)
                config.DefaultFrom.DisplayName = displayName.Value;
        }

        private static void GetServerSettings(XElement setting, EmailNotificationConfig config)
        {
            var smtpServer = setting.Attribute("smtpServer");
            if (smtpServer != null)
                config.ServerSettings.ServerName = smtpServer.Value;

            var smtpServerUser = setting.Attribute("smtpServerUser");
            if (smtpServerUser != null)
                config.ServerSettings.UserName = smtpServerUser.Value;

            var smtpServerPassword = setting.Attribute("smtpServerPassword");
            if (smtpServerPassword != null)
                config.ServerSettings.Password = smtpServerPassword.Value;

            var smtpServerRequiredLogin = setting.Attribute("smtpServerRequiredLogin");
            if (smtpServerRequiredLogin != null)
                config.ServerSettings.IsLoginRequired = Convert.ToBoolean(smtpServerRequiredLogin.Value);

            var isSSLEnabled = setting.Attribute("isSSLEnabled");
            if (isSSLEnabled != null)
                config.ServerSettings.IsSslEnabled = Convert.ToBoolean(isSSLEnabled.Value);

            var smtpServerConnectionLimit = setting.Attribute("smtpServerConnectionLimit");
            if (smtpServerConnectionLimit != null)
                config.ServerSettings.ConnectionLimit = Convert.ToInt32(smtpServerConnectionLimit.Value);

            var useDefaultCredentials = setting.Attribute("useDefaultCredentials");
            if (useDefaultCredentials != null)
                config.ServerSettings.UseDefaultCredentials = Convert.ToBoolean(useDefaultCredentials.Value);
        }
    }

    internal class EmailNotificationConfig
    {
        internal EmailNotificationConfig()
        {
            ServerSettings = new ServerSettingsConfig();
            DefaultFrom = new DefaultFromConfig();
            TestEmailAccounts = new TestEmailAccountsConfig();
        }

        internal bool IsEnabled { get; set; }
        internal ServerSettingsConfig ServerSettings { get; set; }
        internal DefaultFromConfig DefaultFrom { get; set; }
        internal TestEmailAccountsConfig TestEmailAccounts { get; set; }
    }

    internal class ServerSettingsConfig
    {
        internal string ServerName { get; set; }
        internal string UserName { get; set; }
        internal string Password { get; set; }
        internal int ConnectionLimit { get; set; }
        internal bool IsSslEnabled { get; set; }
        internal bool IsLoginRequired { get; set; }
        internal bool UseDefaultCredentials { get; set; }
    }

    internal class DefaultFromConfig
    {
        internal string EmailAddress { get; set; }
        internal string DisplayName { get; set; }
    }

    internal class TestEmailAccountsConfig
    {
        internal TestEmailAccountsConfig()
        {
            Accounts = new List<string>();
        }

        internal bool IsTestEmailAccountsBlocked { get; set; }
        internal List<string> Accounts { get; set; }
    }
}
