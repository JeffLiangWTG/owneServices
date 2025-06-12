using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddMessageTrackingServices(this IServiceCollection services, KafkaConfig config)
		{
			return services
				.AddScoped<IEventLogger>(sp => new EventLogger(config))
				.AddScoped<IEventListener>(sp => new EventListener(config));
		}
	}
}
