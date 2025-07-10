using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder
{
	public static class ApplicationConfig
	{
		static IConfiguration Config => new ConfigurationBuilder()
			.AddJsonFile("CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffEndDateFinder.config.json")
			.Build();

		public static string WorkingFolder => Config[nameof(WorkingFolder)];
		public static string OutputFilePath => Config[nameof(OutputFilePath)];
		public static bool CheckCodesInSafeDb => bool.Parse(Config[nameof(CheckCodesInSafeDb)]);
		public static string SafeUpdateServiceUri => Config[nameof(SafeUpdateServiceUri)];
	}
}
