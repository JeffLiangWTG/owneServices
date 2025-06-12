using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Infrastructure.MessageRepository.Mongo;
using OcmPoc.Infrastructure.MessageTracking.Kafka;

namespace OcmPoc.Mapping.Mapper
{
	class CompositionRoot
	{
		readonly Configuration configuration;

		public CompositionRoot(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public MessageHandler CreateMessageHandler()
		{
			var queueClient = CreateQueueClient();

			return
				new MessageHandler(
					queueClient.GetQueueListener<QueueItem>(),
					queueClient.GetQueueWriter(),
					CreateMessageMapper(),
					CreateEventLogger());
		}

		private IMessageMapper CreateMessageMapper()
		{
			switch (configuration.MappingMode)
			{
				case MappingMode.Receive:
					return new ReceivedMessageMapper(CreateMessageRepository(), new MappingProvider());

				case MappingMode.Send:
					return new MessageForSendMapper(CreateMessageRepository(), new MappingProvider());

				default:
					throw new System.Exception("Mapping mode is unspecified.   Set the 'MappingMode' env var to 'Send' or 'Receive'.");
			}
		}

		public virtual IQueueClient CreateQueueClient()
		{
			return new QueueClientFactory(configuration.RabbitMq).CreateQueueClient();
		}

		public virtual IMessageRepository CreateMessageRepository()
		{
			return new MongoMessageRepositoryFactory(configuration.MongoDb).CreateRepository();
		}

		public virtual IEventLogger CreateEventLogger()
		{
			return new EventLogger(configuration.Kafka);
		}
	}
}
