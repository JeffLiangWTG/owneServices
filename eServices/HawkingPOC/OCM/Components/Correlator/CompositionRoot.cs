using System;
using OcmPoc.Core.Services;
using OcmPoc.Infrastructure.MessageHistory.SqlServer;
using OcmPoc.Infrastructure.MessageHistory.SqlServer.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Infrastructure.MessageTracking.Kafka;
using Polly;

namespace OcmPoc.Components.Correlator
{
	class CompositionRoot
	{
		readonly Configuration configuration;

		public CompositionRoot(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public Correlator CreateCorrelator()
		{
			var queueClient = CreateQueueClient();
			var eventLogger = CreateEventLogger();
			var messageFlowCorrelator = CreateMessageFlowCorrelator(CreateMessageFlowRepository(), eventLogger);

			return
				new Correlator(
					queueClient.GetQueueListener<QueueItem>(),
					queueClient.GetQueueWriter(),
					CreateMessageFlowCorrelator(CreateMessageFlowRepository(), CreateEventLogger()));
		}

		public virtual IMessageFlowRepository CreateMessageFlowRepository()
		{
			return new RetryingMessageFlowRespository(
				Policy.Handle<Exception>()
					  .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(250) }),
				new MessageFlowRepository(
					new SqlContextFactory(configuration.SqlServer)));
		}

		public virtual IMessageFlowCorrelator CreateMessageFlowCorrelator(IMessageFlowRepository messageFlowRepository, IEventLogger eventLogger)
		{
			return new MessageFlowCorrelator(messageFlowRepository, eventLogger);
		}

		public virtual IQueueClient CreateQueueClient()
		{
			return new QueueClientFactory(configuration.RabbitMQ).CreateQueueClient();
		}

		public virtual IEventLogger CreateEventLogger()
		{
			return new EventLogger(configuration.Kafka);
		}
	}
}
