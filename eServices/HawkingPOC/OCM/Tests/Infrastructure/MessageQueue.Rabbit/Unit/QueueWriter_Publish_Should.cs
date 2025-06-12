using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class QueueWriter_Publish_Should : WriterTests
	{
		public QueueWriter_Publish_Should()
		{
			var mockChannel = new Mock<IModel>();
			mockChannel.Setup(m => m.CreateBasicProperties()).Returns(() => new TestProperties());
			
			var mockConnection = new Mock<IConnection>();
			mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
		}

		[Fact]
		public void WriteToExpectedQueueViaDefaultExchange()
		{
			var queueName = Guid.NewGuid().ToString();
			var writer = new QueueWriter(Connection, queueName);
			var item = new TestQueueItem { Sender = "someone" };
			var expectedHeaders = new Dictionary<string, object> { ["Sender"] = Encoding.UTF8.GetBytes("someone") };

			writer.Publish(item);

			VerifyChannelPublishCall(string.Empty, queueName, expectedHeaders);
		}
	}
}
