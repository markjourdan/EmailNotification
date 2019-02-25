using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace EmailNotification.Tests
{
    public partial class EmailNotificationResultTests
    {
        [Test]
        public void CountIsValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>
                                                         {
                                                             new MessageQueueEntity {IsTestEmail = true},
                                                             new MessageQueueEntity {IsTestEmail = true},
                                                             new MessageQueueEntity {IsTestEmail = false}
                                                         });

            Assert.AreEqual(2, result.TestEmails);
            Assert.AreEqual(3, result.Emails.Count());
        }

        [Test]
        public void NoTestEmailsCountIsValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>
                                                         {
                                                             new MessageQueueEntity {IsTestEmail = false},
                                                             new MessageQueueEntity {IsTestEmail = false},
                                                             new MessageQueueEntity {IsTestEmail = false}
                                                         });

            Assert.AreEqual(0, result.TestEmails);
            Assert.AreEqual(3, result.Emails.Count());
        }

        [Test]
        public void NoEmailsCountIsValid()
        {
            var result = new EmailNotificationResult(true,
                                                     new List<MessageQueueEntity>());

            Assert.AreEqual(0, result.TestEmails);
            Assert.AreEqual(0, result.Emails.Count());
        }
    }
}
