using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProducer.config.json");
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

		public static string AirlineCommodityCodesCsvFilePath => Configuration["IATACommodityCodesCsvFilePath"];
	}
}
