using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EmailNotification.Tests
{
    public partial class EmailNotificationResultTests
    {
        [Test]
        public void IsSentCountValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>
                                                         {
                                                             new MessageQueueEntity {IsSent = true},
                                                             new MessageQueueEntity {IsSent = true},
                                                             new MessageQueueEntity {IsSent = false}
                                                         });

            Assert.AreEqual(2, result.EmailSent);
            Assert.AreEqual(3, result.Emails.Count());
        }

        [Test]
        public void NoSentEmailsCountIsValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>
                                                         {
                                                             new MessageQueueEntity {IsSent = false},
                                                             new MessageQueueEntity {IsSent = false},
                                                             new MessageQueueEntity {IsSent = false}
                                                         });

            Assert.AreEqual(0, result.TestEmails);
            Assert.AreEqual(3, result.Emails.Count());
        }

        [Test]
        public void NoEmailsSentCountIsValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>());

            Assert.AreEqual(0, result.TestEmails);
            Assert.AreEqual(0, result.Emails.Count());
        }
    }
}
