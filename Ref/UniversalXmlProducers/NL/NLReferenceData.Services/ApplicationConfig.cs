using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public static class ApplicationConfig
	{
		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(jsonConfigFile).Build();
				}
				return config;
			}
		}
		static string jsonConfigFile = "CargoWise.RefDbRepo.NLReferenceData.CmdLine.config.json";
		static IConfiguration config;

		public static string OutputPath => Config[nameof(OutputPath)];
		public static string DownloadUrlCodeBook => Config[nameof(DownloadUrlCodeBook)];
		public static string DownloadUrlFiscalExchangeRates => Config[nameof(DownloadUrlFiscalExchangeRates)];
		public static string AdditionalInformationTableNumber => Config[nameof(AdditionalInformationTableNumber)];
		public static string DownloadUrlAdditionalSupplements => Config[nameof(DownloadUrlAdditionalSupplements)];
		public static string DownloadUrlTariff => Config[nameof(DownloadUrlTariff)];
		public static string TariffOutputDirectory => Config[nameof(TariffOutputDirectory)];
		public static string ExchangeRatesSoapServiceUrl => Config[nameof(ExchangeRatesSoapServiceUrl)];
		public static string DownloadUrlCodeBookDwuAangifteBehandeling => Config[nameof(DownloadUrlCodeBookDwuAangifteBehandeling)];
		public static string SupportingDocumentTableNumber => Config[nameof(SupportingDocumentTableNumber)];
		public static string TransportDocumentTableNumber => Config[nameof(TransportDocumentTableNumber)];
		public static string PreviousDocumentTableNumber => Config[nameof(PreviousDocumentTableNumber)];
		public static string AdditionalReferenceTableNumber => Config[nameof(AdditionalReferenceTableNumber)];
		public static string ServiceDir => Config[nameof(ServiceDir)];
		public static int DownloadTimeoutInSeconds => int.Parse(Config[nameof(DownloadTimeoutInSeconds)], CultureInfo.InvariantCulture);
		public static int DownloadMaxRetryAttempts => int.Parse(Config[nameof(DownloadMaxRetryAttempts)], CultureInfo.InvariantCulture);
		public static int DownloadRetryIntervalInSeconds => int.Parse(Config[nameof(DownloadRetryIntervalInSeconds)], CultureInfo.InvariantCulture);
	}
}
