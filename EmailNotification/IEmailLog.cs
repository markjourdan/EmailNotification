using System;
using System.Net.Mail;

namespace EmailNotification
{
    public interface IEmailLog
    {
        void Warn(string formatMessage, Exception exception);
        void Error(string formatMessage, Exception exception);
        void Error(string message);
        void Warn(string message);
        void Info(string message);
        void Info(string message, Exception exception);
    }
}
