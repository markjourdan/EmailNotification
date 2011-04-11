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
            get { return Emails.Where(e => e.IsSent).Count(); }
        }

        public int TestEmails
        {
            get { return Emails.Where(e => e.IsTestEmail).Count(); }
        }

        public bool IsSuccess { get; set; }
    }
}
