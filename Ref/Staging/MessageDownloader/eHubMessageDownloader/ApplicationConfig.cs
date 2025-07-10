using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	public static class ApplicationConfig
	{
		public static string RetryTimes => Config["retryTimes"];
		public static string MillisecondsBetweenRetries => Config["millisecondsBetweenRetries"];
		public static string EHubGatewayServerAddress => Config["eHubGatewayServerAddress"];
		public static string EHubGatewayClientId => Config["eHubGatewayClientId"];
		public static string EHubGatewayClientPassword => Config["eHubGatewayClientPassword"];

		public static string ConnectionStrings => DbConnectionStringManager.StagingConnectionString;

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Staging.eHubMessageDownloader.config.json").Build();
	}
}
