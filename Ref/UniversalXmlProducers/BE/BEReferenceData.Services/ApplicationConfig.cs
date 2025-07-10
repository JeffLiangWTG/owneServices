using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonFileName).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonFileName = "CargoWise.RefDbRepo.BEReferenceData.CmdLine.config.json";

		public static string DownloadDir => Config[nameof(DownloadDir)];

		public static string OutputDirectory => Config["OutputPath"];

		public static string TariffUrl => Config["TariffDataDownloadUrl"];

		public static int TariffHistoricalPeriod => int.Parse(Config["TariffHistoricalPeriod"], CultureInfo.CurrentCulture);

		public static string WebSiteUrl => Config[nameof(WebSiteUrl)];

		public static string ExportTariffsDownloadURL => Config[nameof(ExportTariffsDownloadURL)];

		public static string ExportTariffsUXmlFile => Config[nameof(ExportTariffsUXmlFile)];

		public static string EUTariffBaseUrl => Config[nameof(EUTariffBaseUrl)];

		public static string EUNomenclatureBaseUrl => Config[nameof(EUNomenclatureBaseUrl)];

		public static string EUReferenceDataBaseUrl => Config[nameof(EUReferenceDataBaseUrl)];

		public static string SectionDetailsUrl => Config[nameof(SectionDetailsUrl)];

		public static string DownloadsFolder => Config[nameof(DownloadsFolder)];

		public static string NomenclatureUXmlFile => Config[nameof(NomenclatureUXmlFile)];

		public static string DownloadTimeoutInSeconds => Config[nameof(DownloadTimeoutInSeconds)];

		public static string SeleniumServerURL => Config[nameof(SeleniumServerURL)];

		public static string SeleniumTimeOutInSeconds => Config[nameof(SeleniumTimeOutInSeconds)];

		public static string NctsCodesDownloadUrl => Config[nameof(NctsCodesDownloadUrl)];

		public static string UCCCodeListDownloadUrl => Config[nameof(UCCCodeListDownloadUrl)];

		public static string WaitPageLoadingInSeconds => Config[nameof(WaitPageLoadingInSeconds)];

		public static string CircabcDownloadUrlPrefix => Config[nameof(CircabcDownloadUrlPrefix)];

		public static string TariffDataDownloadUrl => Config[nameof(TariffDataDownloadUrl)];

		public static string ClientSettingsProviderServiceUri => Config[nameof(ClientSettingsProviderServiceUri)];

		public static string LocationCodesFolder => Config[nameof(LocationCodesFolder)];

		public static string ServiceDir => Config[nameof(ServiceDir)];

		public static int LoadInterval => int.Parse(Config[nameof(LoadInterval)], CultureInfo.InvariantCulture);

		public static int DownloadIntervalDaily => int.Parse(Config[nameof(DownloadIntervalDaily)], CultureInfo.InvariantCulture);

		public static int DownloadIntervalMonthly => int.Parse(Config[nameof(DownloadIntervalMonthly)], CultureInfo.InvariantCulture);
	}
}
