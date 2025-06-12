using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueReader<TQueueItem> : ChannelHolder, IQueueReader<TQueueItem>
		where TQueueItem : QueueItem, new()
	{
		readonly string queueName;

		public QueueReader(IConnection connection, string queueName)
			: base(connection)
		{
			this.queueName = queueName;
		}

		public IEnumerable<TQueueItem> GetMessages(int maxFetchSize)
		{
			while (maxFetchSize > 0)
			{
				var result = Channel.BasicGet(queueName, false);

				if (result == null) { yield break; }

				yield return result.MapTo<TQueueItem>();

				maxFetchSize--;
			}
		}

		public void Ack(QueueItemDelivery delivery)
		{
			//Console.WriteLine($"Acknowleging {delivery.Exchange}:{delivery.RoutingKey}:{delivery.DeliveryTag}");
			Channel.BasicAck(delivery.DeliveryTag, false);
		}

		public void Nack(QueueItemDelivery delivery, bool requeue = false)
		{
			//Console.WriteLine($"Rejecting {delivery.Exchange}:{delivery.RoutingKey}:{delivery.DeliveryTag}");
			Channel.BasicReject(delivery.DeliveryTag, requeue);
		}
	}
}
