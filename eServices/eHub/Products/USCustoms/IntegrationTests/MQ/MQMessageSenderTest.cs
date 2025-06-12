using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using CargoWise.eServices.USCustoms.OutboundProcessingService;
using Common.Logging;
using Common.Logging.Simple;
using IBM.WMQ;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.IntegrationTests.MQ
{
	[TestFixture]
	public class MQMessageSenderTest : MQBaseTest
	{
		[Test]
		public void TestSend_Exception()
		{
			var logerMock = new Mock<ILog>();
			logerMock.Setup(loger => loger.Error(It.IsAny<string>())).Callback((object errorMessage) =>
			{
				Assert.IsTrue(errorMessage.ToString().Contains("ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770"), "ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770");
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			var cancellationTokenSource = new CancellationTokenSource();
			var senderMock = new Mock<MQMessageSender>(logerMock.Object, managerProviderMock.Object, cancellationTokenSource.Token);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true, "").Throws(new Exception("Test Exception"));
			senderMock.Setup(x => x.MessageLog).Returns(new NoOpLogger());

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream = new MemoryStream();

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.IsTrue(responseStream.ReadToEnd().StartsWith(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""-1"" ErrorDescription=""System.Exception: Test Exception"));
		}

		/// <summary>
		/// Test MQMessageSend see if it will retry when MQ comms goes down outbound messages fail with "CompCode: 2, Reason: 2538".
		/// </summary>
		[Test]
		public void TestSendWithMQ2538Exception()
		{
			var loggerMock = new Mock<ILog>();
			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);
				Assert.IsTrue(messageBody.Trim('\0').StartsWith("A3910SV9CAREDI05171101                                               00000057021"));
				Assert.AreEqual(1213, message.MessageLength);
				Assert.AreEqual(MQC.MQPMO_SYNCPOINT, options.Options);
			});

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();

			int count = 0;
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(() =>
			{
				if (count < 3)
				{
					count++;
					throw new MQException(2, 2538);
				}
				return queueWrapperMock.Object;
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Returns(mqConfigMock.Object);
			senderMock.Protected().Setup<IMQQueueManagerProvider>("NewMQQueueManagerProvider").Returns(managerProviderMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream;

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.AreEqual(3, count);
			Assert.AreEqual(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""2"" ErrorDescription="""" />", responseStream.ReadToEnd());

			loggerMock.Verify(log => log.Info(It.IsAny<string>()), Times.Exactly(1));
			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
		}

		[Test]
		public void TestSendWithWin32Exception_Retry()
		{
			var loggerMock = new Mock<ILog>();
			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();

			int count = 0;
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(() =>
			{
				if (count < 3)
				{
					count++;
					throw new Win32Exception(5);
				}
				return queueWrapperMock.Object;
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Returns(mqConfigMock.Object);
			senderMock.Protected().Setup<IMQQueueManagerProvider>("NewMQQueueManagerProvider").Returns(managerProviderMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream;

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.AreEqual(3, count);
			Assert.AreEqual(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""2"" ErrorDescription="""" />", responseStream.ReadToEnd());

			loggerMock.Verify(log => log.Warn(It.IsAny<string>()), Times.Exactly(3));
			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
		}

		[Test]
		public void TestSend()
		{
			var loggerMock = new Mock<ILog>();
			loggerMock.Setup(_ => _.IsTraceEnabled).Returns(true);

			loggerMock.Setup(_ => _.Trace(It.IsAny<string>()));

			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);
				Assert.IsTrue(messageBody.Trim('\0').StartsWith("A3910SV9CAREDI05171101                                               00000057021"));
				Assert.AreEqual(1213, message.MessageLength);
				Assert.AreEqual(MQC.MQPMO_SYNCPOINT, options.Options);
			});

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			outboundMessageLoggerMock.Setup(_ => _.IsTraceEnabled).Returns(false);
			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true, "").Returns(mqConfigMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream;
			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.AreEqual(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""2"" ErrorDescription="""" />", responseStream.ReadToEnd());

			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
			outboundMessageLoggerMock.VerifyAll();
			var expectedMQMessageProperties = @"MessageType:MQMT_DATAGRAM;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:437;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:1213;Encoding:546;Expiry:-1;Feedback:0;Format:MQSTR   ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:1213;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:1;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			loggerMock.Verify((m => m.Trace(expectedMQMessageProperties)), Times.Once());
		}

		[Test]
		public void TestSendLarge()
		{
			var loggerMock = new Mock<ILog>();
			loggerMock.Setup(_ => _.IsTraceEnabled).Returns(true);
			var expectedDatagramMQMessageProperties = @"MessageType:MQMT_DATAGRAM;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:437;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:0;Encoding:546;Expiry:-1;Feedback:0;Format:MQSTR   ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:0;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:1;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			loggerMock.Setup(_ => _.Trace(expectedDatagramMQMessageProperties));
			var expectedFirstMQMessageProperties = @"MessageType:MQMT_APPL_FIRST;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:437;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:4194240;Encoding:546;Expiry:-1;Feedback:0;Format:MQSTR   ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:4194240;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:1;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			loggerMock.Setup(_ => _.Trace(expectedFirstMQMessageProperties));
			loggerMock.Setup(_ => _.IsTraceEnabled).Returns(true);
			var expectedLastMQMessageProperties = @"MessageType:MQMT_APPL_LAST;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:437;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:0;DataOffset:305471;Encoding:546;Expiry:-1;Feedback:0;Format:MQSTR   ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:305471;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:1;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:1;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;";
			loggerMock.Setup(_ => _.Trace(expectedLastMQMessageProperties));

			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);

				Assert.IsTrue(message.MessageType == MQC.MQMT_APPL_FIRST || message.MessageType == MQC.MQMT_APPL_LAST);

				switch (message.MessageType)
				{
					case MQC.MQMT_APPL_FIRST:
						Assert.AreEqual(messageBody.Trim('\0').Substring(0, 80), "A3910SV9CAREDI10141101                                               00000114368");
						Assert.AreEqual(4194240, message.MessageLength);
						break;
					case MQC.MQMT_APPL_LAST:
						Assert.AreEqual(messageBody.Trim('\0').Trim().Substring(0, 32), "FD05SEMBRITISHFOOD@MINDSPRING.CO");
						Assert.AreEqual(305471, message.MessageLength);
						break;
				}
			});

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			outboundMessageLoggerMock.Setup(_ => _.IsTraceEnabled).Returns(false);
			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Returns(mqConfigMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFileLarge.xml");
			Stream responseStream;

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.AreEqual(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""2"" ErrorDescription="""" />", responseStream.ReadToEnd());

			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
			outboundMessageLoggerMock.VerifyAll();
			loggerMock.VerifyAll();
		}

		[Test]
		public void TestSend_MQException_RetriedAndSucceed()
		{
			var loggerMock = new Mock<ILog>();
			loggerMock.Setup(loger => loger.Info("Processing message with InboxPK 42c996e2-c3c8-4e31-b4f0-6ebf36170770 and MQ Config: Queue1, , , 0"));
			loggerMock.Setup(loger => loger.Warn("Failed sending outbound MQ message(ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770, RetryCount: 0)\r\nCompCode: 2, Reason: 2009\r\n"));
			loggerMock.Setup(loger => loger.Warn("Failed sending outbound MQ message(ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770, RetryCount: 1)\r\nCompCode: 2, Reason: 2009\r\n"));
			loggerMock.Setup(loger => loger.Warn("Failed sending outbound MQ message(ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770, RetryCount: 2)\r\nCompCode: 2, Reason: 2009\r\n"));

			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);
				Assert.IsTrue(messageBody.Trim('\0').StartsWith("A3910SV9CAREDI05171101                                               00000057021"));
				Assert.AreEqual(1213, message.MessageLength);
				Assert.AreEqual(MQC.MQPMO_SYNCPOINT, options.Options);
			});

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();

			int count = 0;
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(() =>
			{
				if (count < 3)
				{
					count++;
					throw new MQException(2, 2009);
				}
				return queueWrapperMock.Object;
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true, "").Returns(mqConfigMock.Object);
			senderMock.Protected().Setup<IMQQueueManagerProvider>("NewMQQueueManagerProvider").Returns(managerProviderMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream;

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.AreEqual(3, count);
			Assert.AreEqual(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""2"" ErrorDescription="""" />", responseStream.ReadToEnd(), responseStream.ReadToEnd());

			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
			managerWrapperMock.Verify(manager => manager.Commit(), Times.Once());
		}

		[Test]
		public void TestSend_ProdServiceAccessFromNonProdMessageException()
		{
			var logerMock = new Mock<ILog>();
			logerMock.Setup(logger => logger.Error(It.IsAny<string>())).Callback((object errorMessage) =>
			{
				Assert.IsTrue(errorMessage.ToString().Contains("ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770"), "ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770");
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			var senderMock = new Mock<MQMessageSender>(logerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Throws(new Exception("Test Exception"));
			senderMock.Setup(x => x.MessageLog).Returns(new NoOpLogger());

			var messageStream = GetEmbeddedResource("TestFiles.TestFileNonProd.xml");
			Stream responseStream = new MemoryStream();

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
			Assert.IsTrue(responseStream.ReadToEnd().StartsWith(@"<SendResponse ClientId=""BLAH"" IsProduction=""False"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""-1"" ErrorDescription=""System.Exception: Test message cannot be sent via a Production service."), "Test message in production error was expected.");
		}

		[Test]
		public void TestSend_NonProdServiceAccessFromProdMessageException()
		{
			var logerMock = new Mock<ILog>();
			logerMock.Setup(loger => loger.Error(It.IsAny<string>())).Callback((object errorMessage) =>
			{
				Assert.IsTrue(errorMessage.ToString().Contains("ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770"), "ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770");
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			var senderMock = new Mock<MQMessageSender>(logerMock.Object, managerProviderMock.Object, CancellationToken.None);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Throws(new Exception("Test Exception"));
			senderMock.Setup(x => x.MessageLog).Returns(new NoOpLogger());

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");
			Stream responseStream = new MemoryStream();

			senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream, false);
			Assert.IsTrue(responseStream.ReadToEnd().StartsWith(@"<SendResponse ClientId=""BLAH"" IsProduction=""True"" MessageTrackingId=""42c996e2-c3c8-4e31-b4f0-6ebf36170770"" ResponseStatus=""-1"" ErrorDescription=""System.Exception: Production message cannot be sent via a Test service."), "Prod message in test service error was expected.");
		}

		[Test]
		public void TestSend_RetriesCanBeStopped()
		{
			var loggerMock = new Mock<ILog>();
			loggerMock.Setup(loger => loger.Info("Processing message with InboxPK 42c996e2-c3c8-4e31-b4f0-6ebf36170770 and MQ Config: Queue1, , , 0"));
			for (var i = 0; i < 50; i++)
			{
				loggerMock.Setup(loger => loger.Warn($@"Failed sending outbound MQ message(ClientId: BLAH, InboxPK: 42c996e2-c3c8-4e31-b4f0-6ebf36170770, RetryCount: {i})\r\nCompCode: 2, Reason: 2009\r\n"));
			}

			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>())).Callback((MQMessage message, MQPutMessageOptions options) =>
			{
				message.DataOffset = 0;
				string messageBody = message.ReadString(message.MessageLength);
				Assert.IsTrue(messageBody.Trim('\0').StartsWith("A3910SV9CAREDI05171101                                               00000057021"));
				Assert.AreEqual(1213, message.MessageLength);
				Assert.AreEqual(MQC.MQPMO_SYNCPOINT, options.Options);
			});

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();

			int count = 0;
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(() =>
			{
				if (count < 50)
				{
					count++;
					throw new MQException(2, 2009);
				}
				return queueWrapperMock.Object;
			});

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(mqConfigMock.Object)).Returns(managerWrapperMock.Object);

			var outboundMessageLoggerMock = new Mock<ILog>();
			var cancellationTokenSource = new CancellationTokenSource();

			var senderMock = new Mock<MQMessageSender>(loggerMock.Object, managerProviderMock.Object, cancellationTokenSource.Token);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", "AMS", "EI", true,"").Returns(mqConfigMock.Object);
			senderMock.Protected().Setup<IMQQueueManagerProvider>("NewMQQueueManagerProvider").Returns(managerProviderMock.Object);
			senderMock.Setup(_ => _.MessageLog).Returns(outboundMessageLoggerMock.Object);

			var messageStream = GetEmbeddedResource("TestFiles.TestFile.xml");

			Stream responseStream = Stream.Null;
			var sendMessage = Task.Run(() =>
		   {
			   senderMock.Object.Send(new USCustomsOutboundMessage(messageStream), out responseStream);
		   });
			Thread.Sleep(4 * 1000);
			cancellationTokenSource.Cancel();
			var errorMessage = "";
			try
			{
				sendMessage.Wait();
			}
			catch (Exception e)
			{
				errorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
			}

			Assert.AreEqual(@"The operation was canceled.", errorMessage);
			Assert.IsTrue(count > 1 && count < 50);
		}
	}

}

