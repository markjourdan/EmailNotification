namespace EmailNotification
{
    public interface IServerConfiguration
    {
        string SmtpServer { get; }
        int SmtpServerConnectionLimit { get; }
        bool SmtpServerRequiredLogin { get; }
        string SmtpServerUserName { get; }
        string SmtpServerPassword { get; }
        bool IsSSLEnabled { get; }
        int SmtpServerPort { get; }
        bool UseDefaultCredentials { get; }
    }
}
