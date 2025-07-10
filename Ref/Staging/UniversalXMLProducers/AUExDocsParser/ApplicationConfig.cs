using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser
{
	public static class ApplicationConfig
	{
		static IConfiguration Configuration
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(jsonConfigFile)
						.Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		static string jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.AUExDocsParser.config.json";

		public static string OutputFileFolderPath => Configuration[nameof(OutputFileFolderPath)];

		public static string PublicationTime => Configuration[nameof(PublicationTime)];

		public static string FileName(string fileCode) => Configuration[fileCode];

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
