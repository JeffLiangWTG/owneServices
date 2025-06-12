using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Core.Services;
using OcmPoc.Infrastructure.MessageHistory.SqlServer;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Infrastructure.MessageRepository.Mongo;
using OcmPoc.Infrastructure.MessageTracking.Kafka;

namespace OcmPoc.FrontEnd.Bootstrapper
{
	public static class IServiceCollectionExtensions
	{
		public static void AddApplicationServices(this IServiceCollection services, Configuration config)
		{
			services.AddMessageRepositoryServices(config.MongoDb);

			services.AddMessageHistoryServices(config.SqlServer);

			services.AddMessageQueueServices(config.RabbitMq);

			services.AddScoped(sp => sp.GetService<IQueueClient>().GetQueueWriter());

			services.AddScoped(sp => sp.GetService<IQueueClient>().GetQueueReader<QueueItem>());

			services.AddMessageTrackingServices(config.Kafka);

			services.AddScoped<IMessageFlowService, MessageFlowService>();
		}
	}
}
