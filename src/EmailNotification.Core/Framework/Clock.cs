using System;

namespace EmailNotification.Core.Framework
{
    public class Clock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
