using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OcmPoc.Utils;

namespace OcmPoc.Mapping.Mapper
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configuration = BindConfiguration();
			var messageHandler = new CompositionRoot(configuration).CreateMessageHandler();

			using (var cancelHandler = new ConsoleCancelHandler())
			{
				await messageHandler.RunAsync(cancelHandler.Token);
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
	}
}
