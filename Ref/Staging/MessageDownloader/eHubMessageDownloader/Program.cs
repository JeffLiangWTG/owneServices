using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.eHubMessageDownloader
{
	static class Program
	{
		public static void Main(string[] args)
		{
			DownloadFromEHub();
		}

		static int DownloadFromEHub()
		{
			var serverAddress = ApplicationConfig.EHubGatewayServerAddress;
			var clientId = ApplicationConfig.EHubGatewayClientId;
			var password = ApplicationConfig.EHubGatewayClientPassword;

			using (var repository = new StagingRepository(ApplicationConfig.ConnectionStrings))
			using (var adapter = new eHubAdapter(serverAddress, clientId, password))
			{
				return new Downloader(repository, adapter).Run();
			}
		}
	}
}
