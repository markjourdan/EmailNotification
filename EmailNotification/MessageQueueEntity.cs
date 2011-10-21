using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace EmailNotification
{
    public class MessageQueueEntity
    {
        public object Identifier { get; set; }

        public MailPriority Priority { get; set; }

        public DateTime Created { get; set; }

        public DateTime Sent { get; set; }

        public BodyFormat BodyFormat { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Cc { get; set; }

        public string Bcc { get; set; }

        public bool IsSent { get; set; }

        public bool IsTestEmail { get; set; }

        public IEnumerable<MessageQueueAttachementEntity> Attachments { get; set; }
    }

    public enum BodyFormat
    {
        Html,
        PlainText
    }
}