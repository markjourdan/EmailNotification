﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using EmailNotification.Framework;
using NUnit.Framework;
using Rhino.Mocks;

namespace EmailNotification.Tests.EmailDriver
{
    [TestFixture]
    public class SendQueuedEmailsTests
    {
        [SetUp]
        public void SetUp()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void SuccessfulEmail()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

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

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Emails.First().IsSent);
            Assert.AreEqual(1, result.EmailSent);
        }

        [Test]
        public void FailedNoToEmail()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

            var emails = new List<MessageQueueEntity>
                             {
                                 new MessageQueueEntity
                                     {
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

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Emails.First().IsSent);
            Assert.AreEqual(0, result.EmailSent);
        }

        [Test]
        public void FailedBadToEmailAddress()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

            var emails = new List<MessageQueueEntity>
                             {
                                 new MessageQueueEntity
                                     {
                                         To = "bad-email",
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

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Emails.First().IsSent);
            Assert.AreEqual(0, result.EmailSent);
            Assert.IsTrue(result.Emails.First().SentException.Message.Contains("'to' property"));
        }

        [Test]
        public void FailedNoFromEmailAddress()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

            var emails = new List<MessageQueueEntity>
                             {
                                 new MessageQueueEntity
                                     {
                                         To = "test@test.com",
                                         Body = "Test Email",
                                         BodyFormat = BodyFormat.PlainText,
                                         Created = DateTime.Now
                                     }
                             };


            var configuration = Master.Configure()
                .WithServerConfiguration(serverConfig)
                .IsEnabled(true)
                .WithEmails(emails);

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Emails.First().IsSent);
            Assert.AreEqual(0, result.EmailSent);
            Assert.IsTrue(result.Emails.First().SentException.Message.Contains("'from' property is empty"));
        }

        [Test]
        public void FailedBadCcEmailAddress()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

            var emails = new List<MessageQueueEntity>
            {
                new MessageQueueEntity
                {
                    To = "good@test.com",
                    Cc = "bad-email",
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

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Emails.First().IsSent);
            Assert.AreEqual(0, result.EmailSent);
            Assert.IsTrue(result.Emails.First().SentException.Message.Contains("'CC' property"));
        }

        [Test]
        public void FailedBadBccEmailAddress()
        {
            var serverConfig = MockRepository.GenerateMock<IServerConfiguration>();

            var smtpClient = MockRepository.GenerateMock<ISmtpClient>();
            smtpClient.Expect(s => s.Send(new MailMessage())).IgnoreArguments();

            var emails = new List<MessageQueueEntity>
            {
                new MessageQueueEntity
                {
                    To = "good@test.com",
                    Bcc = "bad-email",
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

            var result = Master.Execute(configuration, smtpClient);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Emails.First().IsSent);
            Assert.AreEqual(0, result.EmailSent);
            Assert.IsTrue(result.Emails.First().SentException.Message.Contains("'BCC' property"));
        }
    }
}