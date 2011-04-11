using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmailNotification.Tests
{
    public class Class1
    {
        [Test]
        public void Test1()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();
            serverConfig.Expect(s => s.IsSSLEnabled).Return(true);
            serverConfig.Expect(s => s.SmtpServer).Return("Server");
            serverConfig.Expect(s => s.SmtpServerRequiredLogin).Return(false);


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


            var configuration = Master.Configure()
                .WithServerConfiguration(serverConfig)
                .IsEnabled(true)
                .WithEmails(emails);

            var result = Master.Execute(configuration);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.EmailSent);
        }
    }
}
