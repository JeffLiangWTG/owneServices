using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using OcmPoc.Core.Services;
using OcmPoc.Infrastructure.MessageHistory.SqlServer;
using OcmPoc.Infrastructure.MessageHistory.SqlServer.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Infrastructure.MessageRepository.Mongo;
using OcmPoc.Infrastructure.MessageTracking.Kafka;
using OcmPoc.Utils;

namespace OcmPoc.Transceivers
{
	public class TransceiverBuilder
	{
		readonly Configuration configuration;
		readonly IConnectionBuilder connectionBuilder;

		public TransceiverBuilder(IConnectionBuilder connectionBuilder)
		{
			configuration = new Configuration();
			this.connectionBuilder = connectionBuilder;
		}

		public virtual IRunnable Build()
		{
			BindConfiguration();

			var messageRepository = CreateMessageRepository();
			var eventLogger = CreateEventLogger();
			var messageFlowService = CreateMessageFlowService(messageRepository, CreateMessageFlowRepository(), CreateQueueClient(), eventLogger, CreateMapper());

			return
				new Transceiver(
					new Sender(connectionBuilder.BuildSendingConnection(), CreateQueueListener<QueueItem>(), messageRepository, eventLogger),
					new Receiver(connectionBuilder.BuildReceivingConnection(), CreateMessageBuilder(), messageFlowService));
		}

		private void BindConfiguration()
		{
			var configurationRoot = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();

			configurationRoot.Bind(configuration);

			connectionBuilder.BindConfiguration(configurationRoot);

			var baseQueueName = new QueueName(configuration.RabbitMQ.NamePrefix);

			if (String.IsNullOrWhiteSpace(configuration.RabbitMQ.Queue.Input))
			{
				configuration.RabbitMQ.Queue.Input = baseQueueName.Combine(configuration.Provider.Name.ToLower(), "send");
			}

			if (String.IsNullOrWhiteSpace(configuration.RabbitMQ.Queue.Output))
			{
				configuration.RabbitMQ.Queue.Output = baseQueueName.Combine("receive");
			}
		}

		private IMapper CreateMapper()
		{
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<MessageFlow, QueueItem>()
					.ForMember(qi => qi.MessageFlowId, opts => opts.MapFrom(mc => mc.Id))
					.ForMember(qi => qi.MessageId, opts => opts.MapFrom(mc => mc.InitialMessageId));
			});

			return mapperConfig.CreateMapper();
		}

		protected virtual IMessageFlowRepository CreateMessageFlowRepository()
		{
			return new MessageFlowRepository(new SqlContextFactory(configuration.SqlServer));
		}

		protected virtual IMessageFlowService CreateMessageFlowService(IMessageRepository messageRepository, IMessageFlowRepository messageFlowRepository, IQueueClient queueClient, IEventLogger eventLogger, IMapper mapper)
		{
			return new MessageFlowService(messageRepository, messageFlowRepository, queueClient, eventLogger, mapper);
		}

		protected virtual IMessageBuilder CreateMessageBuilder()
		{
			return new MessageBuilder(configuration.Provider.Name);
		}

		protected virtual IQueueListener<T> CreateQueueListener<T>()
			where T : QueueItem, new()
		{
			return CreateQueueClient().GetQueueListener<T>();
		}

		protected virtual IQueueWriter CreateQueueWriter()
		{
			return CreateQueueClient().GetQueueWriter();
		}

		protected virtual IQueueClient CreateQueueClient()
		{
			return new QueueClientFactory(configuration.RabbitMQ).CreateQueueClient();
		}

		protected virtual IMessageRepository CreateMessageRepository()
		{
			return new MongoMessageRepositoryFactory(configuration.MongoDB).CreateRepository();
		}

		protected virtual IEventLogger CreateEventLogger()
		{
			return new EventLogger(configuration.Kafka);
		}
	}
}
