using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OcmPoc.Utils;

namespace OcmPoc.Components.Correlator
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configuration = BindConfiguration();
			var transceiver = new CompositionRoot(configuration).CreateCorrelator();

			using (var cancelHandler = new ConsoleCancelHandler())
			{
				await transceiver.RunAsync(cancelHandler.Token);
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
