using System;

namespace EmailNotification.Framework
{
    public class Clock : IClock
    {
        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
