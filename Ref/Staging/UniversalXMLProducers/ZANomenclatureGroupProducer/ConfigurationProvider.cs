using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer.config.json");
			}
			set
			{
				jsonConfigFile = value;
			}
		}
		static string jsonConfigFile;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFile).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string PDFDownloadUrl => Configuration["PDFDownloadUrl"];
		public static string OutputFilePath => Configuration["OutputFilePath"];
	}
}

