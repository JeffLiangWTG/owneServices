using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.CmdLine
{
	public static class ApplicationConfig
	{
		private const string JsonFileName = "CargoWise.RefDbRepo.ESReferenceData.CmdLine.config.json";

		public static IConfiguration Config
		{
			get
			{
				if (_config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					_config = configurationBuilder.AddJsonFile(JsonFileName).Build();
				}
				return _config;
			}
		}
		static IConfiguration _config;

		public static string OutputPath => Config[nameof(OutputPath)];
		public static string ExchangeRatesURL => Config[nameof(ExchangeRatesURL)];
		public static string MeasuresURL => Config[nameof(MeasuresURL)];
		public static string MeasuresCodesURL => Config[nameof(MeasuresCodesURL)];
		public static string FootnotesURL => Config[nameof(FootnotesURL)];
		public static string CanaryIslandMeasuresURL => Config[nameof(CanaryIslandMeasuresURL)];
		public static string CanaryIslandFootnotesURL => Config[nameof(CanaryIslandFootnotesURL)];
		public static string CanaryIslandCodesURL => Config[nameof(CanaryIslandCodesURL)];
		public static string CanaryIslandExciseURL => Config[nameof(CanaryIslandExciseURL)];
		public static string C44DocumentsURL => Config[nameof(C44DocumentsURL)];
		public static string ElementoQueryAeatURL => Config[nameof(ElementoQueryAeatURL)];
		public static string LocationsURL => Config[nameof(LocationsURL)];
		public static string RefDbServiceURI => Config[nameof(RefDbServiceURI)];
		public static string TariffOneURL => Config[nameof(TariffOneURL)];
	}
}
