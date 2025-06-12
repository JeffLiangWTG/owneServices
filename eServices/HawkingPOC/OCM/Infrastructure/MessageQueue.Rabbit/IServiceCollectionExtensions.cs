using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils.Config;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageQueueServices(this IServiceCollection services, RabbitMqConfig config)
        {
			return services
				.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
				{
					HostName = config.Host,
					UserName = config.Username,
					Password = config.Password
				})
				.AddSingleton<IQueueClient>(sp => new QueueClient(config, sp.GetRequiredService<IConnectionFactory>()));
        }
    }
}
