using System;
using CargoWise.eServices.USCustoms.InboundService;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using Common.Logging;
using IBM.WMQ;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.IntegrationTests.MQ
{
	[TestFixture]
	public class MQMessageRetrieverTest : MQBaseTest
	{
		[Test]
		public void TestRetrieve()
		{
			var configMock = new Mock<IMQConfiguration>();
			configMock.Setup(config => config.QueueName).Returns("Queue457");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Get(It.IsAny<MQMessage>(), It.IsAny<MQGetMessageOptions>())).Callback((MQMessage message, MQGetMessageOptions options) =>
			{
				Assert.AreEqual(MQC.MQGMO_SYNCPOINT + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQGMO_CONVERT, options.Options);
				Assert.AreEqual(30000, options.WaitInterval);

				message.Format = MQC.MQFMT_STRING;
				message.WriteString("Test Messsage Text");
				message.DataOffset = 0;
			});

			var queueManagerMock = new Mock<IMQQueueManagerWrapper>();
			queueManagerMock.Setup(queueManager => queueManager.AccessQueue("Queue457", MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(configMock.Object)).Returns(queueManagerMock.Object);

			var loggerMock = new Mock<ILog>();
			loggerMock.SetupGet(_ => _.IsTraceEnabled).Returns(true);
			loggerMock.Setup(_ => _.Trace("MessageType:MQMT_DATAGRAM;MessageId:000000000000000000000000000000000000000000000000;AccountingToken:;ApplicationIdData:;ApplicationOriginData:;BackoutCount:0;CharacterSet:1200;CorrelationId:000000000000000000000000000000000000000000000000;DataLength:36;DataOffset:0;Encoding:546;Expiry:-1;Feedback:0;Format:MQSTR   ;GroupId:000000000000000000000000000000000000000000000000;MessageFlags:0;MessageLength:36;MessageSequenceNumber:1;Offset:0;OriginalLength:-1;Persistence:2;Priority:-1;PropertyValidation:0;PutApplicationName:;PutApplicationType:0;PutDateTime:0001-01-01 00:00:00,000;ReplyToQueueManagerName:;ReplyToQueueName:;Report:0;TotalMessageLength:0;UserId:;Version:2;CompletionCode:0;ReasonCode:0;ReasonName:MQRC_OK;"));

			var messageLoggerMock = new Mock<ILog>();
			messageLoggerMock.SetupGet(_ => _.IsTraceEnabled).Returns(true);
			messageLoggerMock.Setup(_ => _.Trace("Test Messsage Text"));

			var retrieverMock = new Mock<MQMessageRetriever>(configMock.Object, loggerMock.Object, messageLoggerMock.Object, "ABI")
			{
				CallBase = true
			};
			retrieverMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);
			Assert.That(retrieverMock.Object.Retrieve, Throws.TypeOf<ApplicationException>().With.Message.EqualTo("BeginTransaction() method must be called before retrieve."));

			retrieverMock.Object.BeginTransaction();
			var result = retrieverMock.Object.Retrieve();
			Assert.AreEqual(@"<Message IsProduction="""" MessageType=""ABI""><![CDATA[H4sIAAAAAAAEAAtJLS5R8E0tLi5OTE9VCEmtKAEAv+SpQBIAAAA=]]></Message>", result.ReadToEnd());

			loggerMock.VerifyAll();
			messageLoggerMock.VerifyAll();
		}

		[Test]
		public void TestRetrieveMultipartMessage()
		{
			var loggerMock = new Mock<ILog>();
			var configMock = new Mock<IMQConfiguration>();
			configMock.Setup(config => config.QueueName).Returns("Queue457");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();

			int messageGetCounter = 0;
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Get(It.IsAny<MQMessage>(), It.IsAny<MQGetMessageOptions>())).Callback((MQMessage message, MQGetMessageOptions options) =>
			{
				messageGetCounter++;
				if (messageGetCounter == 1)
				{
					Assert.AreEqual(MQC.MQGMO_SYNCPOINT + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQGMO_CONVERT, options.Options);
					message.MessageType = MQC.MQMT_APPL_FIRST;
					message.Format = MQC.MQFMT_STRING;
					message.WriteString("Test Messsage Text 1");
					message.DataOffset = 0;
					Assert.AreEqual(30000, options.WaitInterval);
				}
				else
				{
					Assert.AreEqual(MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQMO_MATCH_MSG_ID + MQC.MQGMO_CONVERT, options.Options);
					message.MessageType = MQC.MQMT_APPL_LAST;
					message.Format = MQC.MQFMT_STRING;
					message.WriteString("Test Messsage Text 2");
					message.DataOffset = 0;
					Assert.AreEqual(120000, options.WaitInterval);
				}
			});

			var queueManagerMock = new Mock<IMQQueueManagerWrapper>();
			queueManagerMock.Setup(queueManager => queueManager.AccessQueue("Queue457", MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(configMock.Object)).Returns(queueManagerMock.Object);

			var messageLoggerMock = new Mock<ILog>();

			var retrieverMock = new Mock<MQMessageRetriever>(configMock.Object, loggerMock.Object, messageLoggerMock.Object, "ABI")
			{
				CallBase = true
			};
			retrieverMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);
			Assert.That(retrieverMock.Object.Retrieve, Throws.TypeOf<ApplicationException>().With.Message.EqualTo("BeginTransaction() method must be called before retrieve."));

			retrieverMock.Object.BeginTransaction();
			var result = retrieverMock.Object.Retrieve();
			Assert.AreEqual(@"<Message IsProduction="""" MessageType=""ABI""><![CDATA[H4sIAAAAAAAEAAtJLS5R8E0tLi5OTE9VCEmtKFEwDMEUMwIAod7ggSgAAAA=]]></Message>", result.ReadToEnd());
		}

		[Test]
		public void TestRetrieve_30SecondTimeoutExpired()
		{
			var loggerMock = new Mock<ILog>();
			var configMock = new Mock<IMQConfiguration>();
			configMock.Setup(config => config.QueueName).Returns("Queue457");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Get(It.IsAny<MQMessage>(), It.IsAny<MQGetMessageOptions>())).Throws(new MQException(MQC.MQRC_NO_MSG_AVAILABLE, MQC.MQRC_NO_MSG_AVAILABLE));

			var queueManagerMock = new Mock<IMQQueueManagerWrapper>();
			queueManagerMock.Setup(queueManager => queueManager.AccessQueue("Queue457", MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(configMock.Object)).Returns(queueManagerMock.Object);

			var messageLoggerMock = new Mock<ILog>();

			var retrieverMock = new Mock<MQMessageRetriever>(configMock.Object, loggerMock.Object, messageLoggerMock.Object, "ABI")
			{
				CallBase = true
			};
			retrieverMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);

			retrieverMock.Object.BeginTransaction();
			Assert.IsNull(retrieverMock.Object.Retrieve());
			retrieverMock.Object.CommitTransaction();
			queueManagerMock.Verify(queueManager => queueManager.Commit(), Times.Once());
		}

		[Test]
		public void TestRetrieve_UnknownFormat()
		{
			var loggerMock = new Mock<ILog>();
			var configMock = new Mock<IMQConfiguration>();
			configMock.Setup(config => config.QueueName).Returns("Queue457");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Get(It.IsAny<MQMessage>(), It.IsAny<MQGetMessageOptions>())).Callback((MQMessage message, MQGetMessageOptions options) =>
			{
				Assert.AreEqual(MQC.MQGMO_SYNCPOINT + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING + MQC.MQGMO_CONVERT, options.Options);
				Assert.AreEqual(30000, options.WaitInterval);

				message.Format = MQC.MQFMT_TRIGGER;
			});

			var queueManagerMock = new Mock<IMQQueueManagerWrapper>();
			queueManagerMock.Setup(queueManager => queueManager.AccessQueue("Queue457", MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(configMock.Object)).Returns(queueManagerMock.Object);

			var messageLoggerMock = new Mock<ILog>();

			var retrieverMock = new Mock<MQMessageRetriever>(configMock.Object, loggerMock.Object, messageLoggerMock.Object, "ABI")
			{
				CallBase = true
			};
			retrieverMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);

			retrieverMock.Object.BeginTransaction();
			try
			{
				retrieverMock.Object.Retrieve();
			}
			catch (FailedMQMessageException exception)
			{
				Assert.AreEqual("A message with an unknown format was received " + MQC.MQFMT_TRIGGER.ToString(), exception.Message);
				Assert.AreEqual(1, exception.MQMessage.Length);
				Assert.AreEqual(MQC.MQFMT_TRIGGER, exception.MQMessage[0].Format);
			}
		}

		[Test]
		public void TestBeginTransaction_CommitTransaction()
		{
			TestBeginTransaction((Mock<MQMessageRetriever> retrieverMock, Mock<IMQQueueWrapper> queueWrapperMock, Mock<IMQQueueManagerWrapper> queueManagerMock) =>
			{
				retrieverMock.Object.CommitTransaction();
				queueWrapperMock.Verify(queueWrapper => queueWrapper.Close(), Times.Once());
				queueManagerMock.Verify(queueManager => queueManager.Commit(), Times.Once());
			});
		}

		[Test]
		public void TestBeginTransaction_RollbackTransaction()
		{
			TestBeginTransaction((Mock<MQMessageRetriever> retrieverMock, Mock<IMQQueueWrapper> queueWrapperMock, Mock<IMQQueueManagerWrapper> queueManagerMock) =>
			{
				retrieverMock.Object.RollbackTransaction();
				queueWrapperMock.Verify(queueWrapper => queueWrapper.Close(), Times.Once());
				queueManagerMock.Verify(queueManager => queueManager.Backout(), Times.Once());
			});
		}

		void TestBeginTransaction(Action<Mock<MQMessageRetriever>, Mock<IMQQueueWrapper>, Mock<IMQQueueManagerWrapper>> action)
		{
			var loggerMock = new Mock<ILog>();
			var configMock = new Mock<IMQConfiguration>();
			configMock.Setup(config => config.QueueName).Returns("Queue457");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();

			var queueManagerMock = new Mock<IMQQueueManagerWrapper>();
			queueManagerMock.Setup(queueManager => queueManager.AccessQueue("Queue457", MQC.MQOO_INPUT_EXCLUSIVE + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(configMock.Object)).Returns(queueManagerMock.Object);

			var messageLoggerMock = new Mock<ILog>();

			var retrieverMock = new Mock<MQMessageRetriever>(configMock.Object, loggerMock.Object, messageLoggerMock.Object, "ABI")
			{
				CallBase = true
			};
			retrieverMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);

			retrieverMock.Object.BeginTransaction();
			queueWrapperMock.Verify(queueWrapper => queueWrapper.SetCloseOptions(MQC.MQCO_NONE), Times.Once());

			action(retrieverMock, queueWrapperMock, queueManagerMock);
		}
	}
}
