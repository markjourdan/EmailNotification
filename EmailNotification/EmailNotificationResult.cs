using System.Collections.Generic;
using System.Linq;

namespace EmailNotification
{
    public class EmailNotificationResult
    {
        public EmailNotificationResult(bool isSuccess, IEnumerable<MessageQueueEntity> emails)
        {
            Emails = emails;
            IsSuccess = isSuccess;
        }

        public IEnumerable<MessageQueueEntity> Emails { get; set; }

        public int EmailSent
        {
            get { return Emails.Count(e => e.IsSent); }
        }

        public int TestEmails
        {
            get { return Emails.Count(e => e.IsTestEmail); }
        }

        public bool IsSuccess { get; set; }
    }
}
