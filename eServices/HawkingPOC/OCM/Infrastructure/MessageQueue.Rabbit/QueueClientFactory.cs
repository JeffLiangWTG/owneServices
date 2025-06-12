using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils.Config;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueClientFactory : IQueueClientFactory
	{
		readonly IConnectionFactory connectionFactory;
		readonly RabbitMqConfig config;

		public QueueClientFactory(RabbitMqConfig config)
		{
			this.config = config;

			connectionFactory = new ConnectionFactory
			{
				HostName = config.Host,
				UserName = config.Username,
				Password = config.Password
			};
		}

		public IQueueClient CreateQueueClient()
		{
			return new QueueClient(config, connectionFactory);
		}
	}
}
