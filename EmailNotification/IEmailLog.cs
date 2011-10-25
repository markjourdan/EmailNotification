using System;

namespace EmailNotification
{
    public interface IEmailLog
    {
        void Error(string message);
        void Error(string formatMessage, Exception exception);
        void Warn(string message);
        void Warn(string formatMessage, Exception exception);
        void Info(string message);
        void Info(string message, Exception exception);
    }
}
