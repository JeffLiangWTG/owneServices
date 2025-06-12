using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueWriter : ExchangeWriter, IQueueWriter
	{
		const string defaultExchange = "";

		readonly string queueName;

		public QueueWriter(IConnection connection, string queueName) 
			: base(connection, defaultExchange)
		{
			this.queueName = queueName;
		}

		public override void Publish(QueueItem item)
		{
			Publish(item, queueName);
		}
	}
}
