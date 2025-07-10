using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.EdmxGen
{
	public static class Application
	{
		static IConfiguration config;
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.EdmxGen.config.json").Build();
				}
				return config;
			}
		}

		public static IGeneratorConfiguration LoadSafeConfig(string tempFilePath, XDocument currentEdmx)
		{
			Argument.NotNullOrEmpty(tempFilePath, nameof(tempFilePath));
			Argument.NotNull(currentEdmx, nameof(currentEdmx));
			return new GeneratorConfiguration(tempFilePath, currentEdmx,
				Config["AppSettings:SafeNamespace"],
				Config["AppSettings:SafeEntityContainer"],
				SafeConnectionString,
				new string[] { "/pl" });
		}

		public static IGeneratorConfiguration LoadStagingConfig(string tempFilePath, XDocument currentEdmx)
		{
			Argument.NotNullOrEmpty(tempFilePath, nameof(tempFilePath));
			Argument.NotNull(currentEdmx, nameof(currentEdmx));
			return new GeneratorConfiguration(tempFilePath, currentEdmx,
				Config["AppSettings:StagingNamespace"],
				Config["AppSettings:StagingEntityContainer"],
				StagingConnectionString,
				new string[] { "/pl" });
		}


		public static string SafeConnectionString { get; } = Config.GetConnectionString("Safe");
		public static string StagingConnectionString { get; } = Config.GetConnectionString("Staging");
		public static string SafeDbTriggersPath { get; } = Config["AppSettings:SafeDbTriggersPath"];
	}
}
