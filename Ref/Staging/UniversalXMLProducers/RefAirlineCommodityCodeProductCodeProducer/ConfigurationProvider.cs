using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer.config.json");
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
					configuration = configurationBuilder.AddJsonFile(JsonConfigFile)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string OutputFolderPath => Configuration["OutputFolderPath"];
		public static string OutputCommodityCodesFileName => Configuration["OutputCommodityCodesFileName"];
		public static string OutputProductCodesAndPivotsFileName => Configuration["OutputProductCodesAndPivotsFileName"];
		public static string ProductCommodityCodesFileToProcess => Configuration["ProductCommodityCodesFileToProcess"];
	}
}
