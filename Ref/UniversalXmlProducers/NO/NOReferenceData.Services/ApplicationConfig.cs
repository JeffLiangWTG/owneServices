using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NOReferenceData.Services
{
	public static class ApplicationConfig
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.NOReferenceData.CmdLine.config.json";

		public static IConfiguration Config
		{
			get
			{
				if (_config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					_config = configurationBuilder.AddJsonFile(JsonConfigFile).Build();
				}
				return _config;
			}
		}
		static IConfiguration _config;

		public static string OutputDirectory => Config[nameof(OutputDirectory)];
		public static string ResourceSearchUrl => Config[nameof(ResourceSearchUrl)];
		public static string ResourceMedlemslandFilename => Config[nameof(ResourceMedlemslandFilename)];
		public static string ResourceExchangeRatesFilename => Config[nameof(ResourceExchangeRatesFilename)];
		public static string ResourceImportReferenceFilename => Config[nameof(ResourceImportReferenceFilename)];
		public static string ResourceExportReferenceFilename => Config[nameof(ResourceExportReferenceFilename)];
		public static string ResourceErrorCodesFilename => Config[nameof(ResourceErrorCodesFilename)];
		public static string ResourceTollsatsFilename => Config[nameof(ResourceTollsatsFilename)];
		public static string ResourceCustomstariffstructureFilename => Config[nameof(ResourceCustomstariffstructureFilename)];
		public static string ResourceTolltariffStrukturFilename => Config[nameof(ResourceTolltariffStrukturFilename)];
		public static string ResourceInnfoerselsavgiftFilename => Config[nameof(ResourceInnfoerselsavgiftFilename)];
		public static string ResourceUtfoerselsavgiftFilename => Config[nameof(ResourceUtfoerselsavgiftFilename)];
		public static string ResourceVarenummerFilename => Config[nameof(ResourceVarenummerFilename)];
		public static string ResourceLandgruppeFilename => Config[nameof(ResourceLandgruppeFilename)];
		public static string ResourceRaavaretollsatsFilename => Config[nameof(ResourceRaavaretollsatsFilename)];
	}
}
