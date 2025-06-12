using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageHistory.SqlServer;
using OcmPoc.Infrastructure.MessageTracking.Kafka;
using OcmPoc.Utils;

namespace OcmPoc.Components.EventProcessor
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configuration = BindConfiguration();
			var serviceProvider = ConfigureServices(configuration).BuildServiceProvider();

			using (var cancelHandler = serviceProvider.GetRequiredService<IConsoleCancelHandler>())
			{
				await serviceProvider.GetRequiredService<HistoryBuilder>()
									 .RunAsync(configuration, cancelHandler.Token);
			}
		}

		static Configuration BindConfiguration()
		{
			var configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();

			var appConfig = new Configuration();
			configuration.Bind(appConfig);

			return appConfig;
		}

		internal static IServiceCollection ConfigureServices(Configuration config)
		{
			return new ServiceCollection()
				.AddTransient<IConsoleCancelHandler, ConsoleCancelHandler>()
				.AddSingleton<HistoryBuilder>()
				.AddMessageTrackingServices(config.Kafka)
				.AddMessageHistoryServices(config.SqlServer);
		}
	}
}
