using System.Collections.Generic;
using System.Text;
using Moq;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class WriterTests
	{
		Mock<IModel> mockChannel;

		public WriterTests()
		{
			mockChannel = new Mock<IModel>();
			mockChannel.Setup(m => m.CreateBasicProperties()).Returns(() => new TestProperties());
			
			var mockConnection = new Mock<IConnection>();
			mockConnection.Setup(m => m.CreateModel()).Returns(mockChannel.Object);
			Connection = mockConnection.Object;
		}

		protected IConnection Connection { get; }

		protected void VerifyChannelPublishCall(string expectedExchange, string expectedRoutingKey, Dictionary<string, object> expectedHeaders)
		{
			mockChannel.Verify(m => m.BasicPublish(It.Is<string>(exchange => exchange == expectedExchange),
												   It.Is<string>(routingKey => routingKey == expectedRoutingKey),
												   It.Is<bool>(mandatory => mandatory == false),
												   It.Is<IBasicProperties>(properties => properties.Headers.ContainsKey("Sender") &&
																						 Encoding.UTF8.GetString((byte[])properties.Headers["Sender"]) == "someone"),
												   It.Is<byte[]>(body => body == null)),
							   Times.Once);
		}
	}
}
