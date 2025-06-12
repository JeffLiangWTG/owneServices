using System;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class ExchangeWriter : ChannelHolder, IExchangeWriter, IQueueWriter
	{
		static readonly object channelLock = new Object();

		readonly string exchangeName;


		public ExchangeWriter(IConnection connection, string exchangeName)
			: base(connection)
		{
			this.exchangeName = exchangeName;
		}

		public IQueueTransaction NewTransaction()
		{
			return new QueueTransaction(Channel);
		}

		public void Publish(QueueItem item, string routingKey)
		{
			var properties = Channel.CreateBasicProperties().MapFrom(item);

			//Console.WriteLine($"Sending QueueItem to {exchangeName}:{routingKey} with Headers:");
			//foreach (var header in properties.Headers)
			//{
			//	Console.WriteLine($"    {header.Key}: {header.Value ?? "[null]"}");
			//}

			lock (channelLock)
			{
				Channel.BasicPublish(exchangeName, routingKey, properties, null);
			}

			//Console.WriteLine($"Sent queue item {item.MessageId} to {exchangeName}:{routingKey}");
		}

		public virtual void Publish(QueueItem item)
		{
			Publish(item, String.Empty);
		}
	}
}
