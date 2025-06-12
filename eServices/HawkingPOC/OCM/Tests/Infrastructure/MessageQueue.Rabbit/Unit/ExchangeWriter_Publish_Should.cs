using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit.Tests.Unit
{
	public class ExchangeWriter_Publish_Should : WriterTests
	{
		[Fact]
		public void WriteToExpectedQueueViaDefaultExchange()
		{
			var exchangeName = Guid.NewGuid().ToString();
			var writer = new ExchangeWriter(Connection, exchangeName);
			var item = new TestQueueItem { Sender = "someone" };
			var expectedHeaders = new Dictionary<string, object> { ["Sender"] = Encoding.UTF8.GetBytes("someone") };
			string routingKey = Guid.NewGuid().ToString();

			writer.Publish(item, routingKey);

			VerifyChannelPublishCall(exchangeName, routingKey, expectedHeaders);
		}
	}
}
