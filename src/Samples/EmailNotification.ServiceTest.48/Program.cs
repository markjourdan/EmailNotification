using System;
using System.Collections.Generic;

namespace EmailNotification.ServiceTest48
{
    public class Program
    {
        static void Main(string[] args)
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
