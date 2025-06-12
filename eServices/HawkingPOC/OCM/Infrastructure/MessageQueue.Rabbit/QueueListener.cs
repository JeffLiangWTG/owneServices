using System;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueListener<TQueueItem> : ChannelHolder, IQueueListener<TQueueItem>
		where TQueueItem : QueueItem, new()
	{
		string consumerTag;
		readonly ObservableConsumer<TQueueItem> consumer;
		readonly string queueName;

		public QueueListener(IConnection connection, string queueName, ushort preFetch = 1)
			: base(connection)
		{
			Channel.BasicQos(0, preFetch, false);
			consumer = new ObservableConsumer<TQueueItem>(Channel);
			this.queueName = queueName;
			QueueItems = consumer.Deliveries;
		}

		public IObservable<TQueueItem> QueueItems { get; }

		public void Listen()
		{
			consumerTag = Channel.BasicConsume(queueName, false, consumer);
			Console.WriteLine($"Listening to queue {queueName}");
		}

		public void Stop()
		{
			Channel.BasicCancel(consumerTag);
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

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stop();
			}

			base.Dispose(disposing);
		}
	}
}
