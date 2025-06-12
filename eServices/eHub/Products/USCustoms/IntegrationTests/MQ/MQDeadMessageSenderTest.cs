using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;
using IBM.WMQ;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.eServices.USCustoms.IntegrationTests.MQ
{
	[TestFixture]
	public class MQDeadMessageSenderTest : MQBaseTest
	{
		[Test]
		public void TestSend()
		{
			var mqConfigMock = new Mock<IMQConfiguration>();
			mqConfigMock.Setup(config => config.QueueName).Returns("Queue1");

			var queueWrapperMock = new Mock<IMQQueueWrapper>();
			queueWrapperMock.Setup(queueWrapper => queueWrapper.Put(It.IsAny<MQMessage>(), It.IsAny<MQPutMessageOptions>()));

			var managerWrapperMock = new Mock<IMQQueueManagerWrapper>();
			managerWrapperMock.Setup(manager => manager.AccessQueue("Queue1", MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING)).Returns(queueWrapperMock.Object);

			var managerProviderMock = new Mock<IMQQueueManagerProvider>();
			managerProviderMock.Setup(managerProvider => managerProvider.GetOrCreateNewQueueManager(It.IsAny<IMQConfiguration>())).Returns(managerWrapperMock.Object);

			var senderMock = new Mock<MQDeadMessageSender>();
			senderMock.Protected().Setup<IMQQueueManagerProvider>("GetMQQueueManagerProvider").Returns(managerProviderMock.Object);
			senderMock.Protected().Setup<IMQConfiguration>("GetMQConfiguration", true, true).Returns(mqConfigMock.Object);

			var mqMessages = new[]
			{
				new MQMessage {Format = MQC.MQFMT_NONE},
				new MQMessage {Format = MQC.MQFMT_STRING}
			};

			senderMock.Object.Send(mqMessages, true);

			queueWrapperMock.Verify(queue => queue.Close(), Times.Once());
		}
	}
}
