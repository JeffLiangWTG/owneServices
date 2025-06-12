using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.ServiceModel;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.PullService.Common;
using CargoWise.eHub.Products.JPCustoms.Tests.MockClasses;
using CargoWise.eHub.Shared.IssueManagerTests;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{

	[TestClass]
	public class JPCustomsPullManagerTests : TestBase
	{
		//This is integration test, pop3 server should be install first. Copy TestFiles/JPCustomsReplyMessage.eml file for user recipient1 after each successful test. 
		//[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_Pull_Integration()
		{
			var auditLogger = new Mock<IAuditLogger>();
			var pullClient = new JPCustomsPullClient(new TestPop3MailClientConfiguration(Logger));
			var eHubClient = new EHubClient(new TestEHubClientConfiguration());
			var issueManager = new Mock<IssueManagerForTesting>();
			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient, eHubClient, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_MailboxLockedByAnotherClientException()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();

			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.Connect()).Throws(new MailboxLockedByAnotherClientException("Lock 1"));

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_MessageNotFound()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(0);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());

			pullClient.Verify(c => c.Delete(), Times.Never());
			pullClient.Verify(c => c.GetNextMessage(), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived(It.IsAny<string>()), Times.Never());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_2NewMessageReceived_WithAuditLogging()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			var issueManager = new Mock<IssueManagerForTesting>();
			pullClient.Setup(c => c.MessageCount).Returns(2);

			var retunValues = new Queue<string>();
			retunValues.Enqueue("Message 1");
			retunValues.Enqueue("Message 2");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues.Dequeue);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, TestIssueManager);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Delete(), Times.Exactly(2));
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());

			eHubClient.Verify(c => c.Send("Message 1"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 2"), Times.Once());

			auditLogger.Verify(a => a.ReplyReceived("Message 1"), Times.Once());
			auditLogger.Verify(a => a.ReplyReceived("Message 2"), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_2NewMessageReceived_OneIsEmpty()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			var issueManager = new Mock<IssueManagerForTesting>();
			pullClient.Setup(c => c.MessageCount).Returns(2);

			var retunValues = new Queue<string>();
			retunValues.Enqueue("");
			retunValues.Enqueue("Message 2");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues.Dequeue);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Delete(), Times.Exactly(2));
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());

			eHubClient.Verify(c => c.Send(""), Times.Never());
			eHubClient.Verify(c => c.Send("Message 2"), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Audit Exception")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_AuditException()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			var issueManager = new Mock<IssueManagerForTesting>();
			pullClient.Setup(c => c.MessageCount).Returns(1);
			pullClient.Setup(c => c.GetNextMessage()).Returns("Message 1");

			auditLogger.Setup(a => a.ReplyReceived("Message 1")).Throws(new Exception("Audit Exception"));

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Delete(), Times.Never());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());

			eHubClient.Verify(c => c.Send("Message 1"), Times.Once());

			auditLogger.Verify(a => a.ReplyReceived("Message 1"), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ErrorToSubmitFirstMessageInEHub_ProcessSecondOne()
		{
			var eHubClient = new Mock<IEHubClient>();
			eHubClient.Setup(c => c.Send("Message 1")).Throws(new Exception("Some error during sending to eHub."));

			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(2);

			var retunValues = new Queue<string>();
			retunValues.Enqueue("Message 1");
			retunValues.Enqueue("Message 2");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues.Dequeue);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();
			Assert.AreEqual("Error - eHubClient.Send: Unable to send message to eHub: Message 1 Exception message: Some error during sending to eHub.\r\nInfo - Message sent to eHub Message 2\r\n", ((TestLogger)Logger).Log);

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Delete(), Times.Once());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());

			eHubClient.Verify(c => c.Send("Message 1"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 2"), Times.Once());

			auditLogger.Verify(a => a.ReplyReceived("Message 1"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 2"), Times.Once());

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ResetTimer_WhenMessageSubmittedToEHubSucessfully()
		{
			var eHubClient = new Mock<IEHubClient>();
			eHubClient.Setup(c => c.Send("Message 1")).Throws(new EndpointNotFoundException("Some error during sending to eHub."));
			eHubClient.Setup(c => c.Send("Message 2")).Throws(new EndpointNotFoundException("Some error during sending to eHub."));

			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(2);

			var retunValues1 = new Queue<string>();
			retunValues1.Enqueue("Message 1");
			retunValues1.Enqueue("Message 2");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues1.Dequeue);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();
			Assert.AreEqual("Error - eHubClient.Send: Unable to send message to eHub: Message 1 Exception message: Some error during sending to eHub.\r\nError - eHubClient.Send: Unable to send message to eHub: Message 2 Exception message: Some error during sending to eHub.\r\n", ((TestLogger)Logger).Log);

			Assert.IsNotNull(manager.IssueStartTime);

			var retunValues2 = new Queue<string>();
			retunValues2.Enqueue("Message 3");
			retunValues2.Enqueue("Message 4");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues2.Dequeue);

			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Connect(), Times.Exactly(2));
			pullClient.Verify(c => c.Delete(), Times.Exactly(2));
			pullClient.Verify(c => c.Quit(), Times.Exactly(2));
			pullClient.Verify(c => c.Disconnect(), Times.Exactly(2));

			eHubClient.Verify(c => c.Send("Message 1"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 2"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 3"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 4"), Times.Once());

			auditLogger.Verify(a => a.ReplyReceived("Message 1"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 2"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 3"), Times.Once());
			auditLogger.Verify(a => a.ReplyReceived("Message 4"), Times.Once());

			Assert.IsNull(manager.IssueStartTime);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ReportToIssueManagerAfterTimeElapsed()
		{
			var eHubClient = new Mock<IEHubClient>();
			eHubClient.Setup(c => c.Send("Message 1")).Throws(new EndpointNotFoundException("Endpoint cannot be found or reached."));
			eHubClient.Setup(c => c.Send("Message 2")).Throws(new EndpointNotFoundException("Endpoint cannot be found or reached."));

			eHubClient.Setup(c => c.Send("Message 3")).Throws(new EndpointNotFoundException("Endpoint cannot be found or reached."));
			eHubClient.Setup(c => c.Send("Message 4")).Throws(new EndpointNotFoundException("Endpoint cannot be found or reached."));

			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(2);

			var retunValues1 = new Queue<string>();
			retunValues1.Enqueue("Message 1");
			retunValues1.Enqueue("Message 2");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues1.Dequeue);

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			Assert.IsNotNull(manager.IssueStartTime);

			var retunValues2 = new Queue<string>();
			retunValues2.Enqueue("Message 3");
			retunValues2.Enqueue("Message 4");
			pullClient.Setup(c => c.GetNextMessage()).Returns(retunValues2.Dequeue);

			manager.PullCustomsForNewMessageAndPushToEHub();

			Assert.IsNotNull(manager.IssueStartTime);

			pullClient.Verify(c => c.Connect(), Times.Exactly(2));
			pullClient.Verify(c => c.Quit(), Times.Exactly(2));
			pullClient.Verify(c => c.Disconnect(), Times.Exactly(2));

			eHubClient.Verify(c => c.Send("Message 1"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 2"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 3"), Times.Once());
			eHubClient.Verify(c => c.Send("Message 4"), Times.Once());

			auditLogger.Verify(a => a.ReplyReceived("Message 1"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 2"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 3"), Times.Never());
			auditLogger.Verify(a => a.ReplyReceived("Message 4"), Times.Never());

			issueManager.Verify(x => x.ReportToIssueManager("JPCustoms Pull Manager - eHubClient.Send: Unable to send message to eHub after retrying for 0 minutes.", It.IsAny<Exception>(), It.IsAny<ILog>(), It.IsAny<NameValueCollection>(), true), Times.Exactly(3));

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Some error during Connect")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ExceptionConnect()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.Connect()).Throws(new Exception("Some error during Connect"));

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();


			pullClient.Verify(c => c.Delete(), Times.Never());
			pullClient.Verify(c => c.GetNextMessage(), Times.Never());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Some error during MessageCount")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ExceptionMessageCount()
		{
			var eHubClient = new Mock<IEHubClient>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Throws(new Exception("Some error during MessageCount"));

			var auditLogger = new Mock<IAuditLogger>();

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object,issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Delete(), Times.Never());
			pullClient.Verify(c => c.GetNextMessage(), Times.Never());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Some error during GetNextMessage")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ExceptionGetNextMessage()
		{
			var eHubClient = new Mock<IEHubClient>();
			var issueManager = new Mock<IssueManagerForTesting>();
			var auditLogger = new Mock<IAuditLogger>();

			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(1);
			pullClient.Setup(c => c.GetNextMessage()).Throws(new Exception("Some error during GetNextMessage"));

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Delete(), Times.Never());
			pullClient.Verify(c => c.GetNextMessage(), Times.Once());
			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Some error during Delete")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ExceptionDelete()
		{
			var eHubClient = new Mock<IEHubClient>();
			eHubClient.Setup(c => c.Send(It.IsAny<String>()));
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.MessageCount).Returns(1);
			pullClient.Setup(c => c.GetNextMessage()).Returns("Message 1");
			pullClient.Setup(c => c.Delete()).Throws(new Exception("Some error during Delete"));
			var auditLogger = new Mock<IAuditLogger>();
			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(Exception), "Some error during Disconnect")]
		public void JPCustomsPullManager_PullCustomsForNewMessageAndPushToEHub_ExceptionDisconnect()
		{
			var eHubClient = new Mock<IEHubClient>();
			eHubClient.Setup(c => c.Send(It.IsAny<String>()));
			var issueManager = new Mock<IssueManagerForTesting>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.Connect());
			pullClient.Setup(c => c.MessageCount).Returns(1);
			pullClient.Setup(c => c.GetNextMessage()).Returns("Message 1");
			pullClient.Setup(c => c.Delete());
			pullClient.Setup(c => c.Disconnect()).Throws(new Exception("Some error during Disconnect"));
			var auditLogger = new Mock<IAuditLogger>();
			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, issueManager.Object);
			manager.PullCustomsForNewMessageAndPushToEHub();

			pullClient.Verify(c => c.Quit(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void JPCustomsPullManager_ErrorMailboxLocked_ReportToIssueManager()
		{
			var eHubClient = new Mock<IEHubClient>();
			var auditLogger = new Mock<IAuditLogger>();
			var pullClient = new Mock<IJPCustomsPullClient>();
			pullClient.Setup(c => c.Connect()).Throws(new MailboxLockedByAnotherClientException("Lock 1"));

			var manager = new JPCustomsPullManager(Logger, auditLogger.Object, pullClient.Object, eHubClient.Object, TestIssueManager);
			
			manager.PullCustomsForNewMessageAndPushToEHub();

			string expectedKey;
			string expectedEHubStackTrace;
			string expectedException;
			string expectedSubject;
			string allText;
			using (var memStream = new MemoryStream())
			{
				var task = ((IssueManagerForTesting)manager.IssueManager).EnterpriseErrorReportBuilder.WriteToAsync(memStream);
				task.ConfigureAwait(false);
				task.Wait();
				memStream.Seek(0, SeekOrigin.Begin);
				using (var reader = new StreamReader(memStream))
				{
					var t2 = reader.ReadToEndAsync();
					t2.ConfigureAwait(false);
					t2.Wait();
					allText = t2.Result;
				}

				expectedException = "<ExceptionType>CargoWise.eHub.Products.JPCustoms.Common.MailboxLockedByAnotherClientException</ExceptionType>";
				expectedSubject = "JPCustoms Pull Manager - PullCustomsForNewMessageAndPushToEHub completed. Mailbox was locked but another client.";
				expectedKey = "<Key>JP Customs Pull Service exception: Lock 1</Key>";
				expectedEHubStackTrace = "at CargoWise.eHub.Shared.IssueManagerTests.IssueManagerForTesting.ReportToIssueManager";
			}

			StringAssert.Contains(allText, expectedException);
			StringAssert.Contains(allText, expectedSubject);
			StringAssert.Contains(allText, expectedKey);
			StringAssert.Contains(allText, expectedEHubStackTrace);

			pullClient.Verify(c => c.Connect(), Times.Once());
			pullClient.Verify(c => c.Disconnect(), Times.Once());
		}
	}
}
