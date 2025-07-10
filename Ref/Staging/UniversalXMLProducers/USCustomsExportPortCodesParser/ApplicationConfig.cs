using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser
{
	public static class ApplicationConfig
	{
		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonConfigFileName = "CargoWise.RefDbRepo.UniversalXMLProducers.USCustomsExportPortCodesParser.config.json";

		public static string XmlFileOutputPath => Configuration[nameof(XmlFileOutputPath)];

		public static string WebSiteUrl => Configuration[nameof(WebSiteUrl)];

		public static string DownloadPath => Configuration[nameof(DownloadPath)];
	}
}
