using System.IO;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer.config.json");
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

		public static string AirlineFilePath => Configuration["AirlineFilePath"];
		public static string OutputPath => Configuration["OutputFilePath"];

		public static string AirlineWithMultipleKeysOutputPath => Path.Combine(OutputPath, "RefAirline_MultipleKeys.xml");
		public static string AirlineName1OutputPath => Path.Combine(OutputPath, "RefAirline_AirlineName1.xml");
	}
}
