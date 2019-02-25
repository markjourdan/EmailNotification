using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace EmailNotification.ServiceTest
{
    public class Class1
    {
        [Test]
        public void Main()
        {
            var emails = new List<MessageQueueEntity>
                             {
                                 new MessageQueueEntity
                                     {
                                         To = "test@test.com",
                                         From = "from@test.com",
                                         Body = "Test Email",
                                         BodyFormat = BodyFormat.PlainText,
                                         Created = DateTime.Now
                                     }
                             };


            var configuration = Master.UseAppConfig().WithEmails(emails);

            Master.Execute(configuration);
        }
    }

}
