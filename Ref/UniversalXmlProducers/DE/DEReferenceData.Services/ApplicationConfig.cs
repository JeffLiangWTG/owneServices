using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.DEReferenceData.Services
{
	public static class ApplicationConfig
	{
		public static string OutputPath => Configuration[nameof(OutputPath)];

		public static string CustomsBaseURL => Configuration[nameof(CustomsBaseURL)];

		public static string ImportCodeListsDownloadUrlPrimary => Configuration[nameof(ImportCodeListsDownloadUrlPrimary)];

		public static string ImportCodeListsDownloadUrlSecondary => Configuration[nameof(ImportCodeListsDownloadUrlSecondary)];

		public static string ExportCodeListsDownloadUrlPrimary => Configuration[nameof(ExportCodeListsDownloadUrlPrimary)];
		public static string ExportCodeListsDownloadUrlSecondary => Configuration[nameof(ExportCodeListsDownloadUrlSecondary)];

		public static string NctsCodeListsDownloadUrlPrimary => Configuration[nameof(NctsCodeListsDownloadUrlPrimary)];
		public static string NctsCodeListsDownloadUrlSecondary => Configuration[nameof(NctsCodeListsDownloadUrlSecondary)];

		public static string EmcsCodeListsDownloadPageUrl => Configuration[nameof(EmcsCodeListsDownloadPageUrl)];

		public static string ExchangeRatesStartURL => Configuration[nameof(ExchangeRatesStartURL)];

		public static string DeTariffsBaselineIndexURL => Configuration[nameof(DeTariffsBaselineIndexURL)];

		public static string DeTariffsUpdateIndexURL => Configuration[nameof(DeTariffsUpdateIndexURL)];

		public static string DeTariffsClientId => Configuration[nameof(DeTariffsClientId)];

		public static string DeTariffsSecret => Configuration[nameof(DeTariffsSecret)];

		public static string DeTariffsArchiveCachePath => Configuration[nameof(DeTariffsArchiveCachePath)];

		public static string DeTariffsExtractedCachePath => Configuration[nameof(DeTariffsExtractedCachePath)];

		public static int MinDelayInMilliSecondsBetweenAttemptsInCaseOfDownloadError
		{
			get
			{
				if (int.TryParse(Configuration[nameof(MinDelayInMilliSecondsBetweenAttemptsInCaseOfDownloadError)], out int delay))
				{
					return delay;
				}
				return 0;
			}
		}
		public static int MaxAttemptsCountInCaseOfDownloadError
		{
			get
			{
				if (int.TryParse(Configuration[nameof(MaxAttemptsCountInCaseOfDownloadError)], out int delay))
				{
					return delay;
				}
				return 1;
			}
		}

		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonFileName)
						.AddJsonFile(LocalJsonFileName, true).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonFileName = "CargoWise.RefDbRepo.DEReferenceData.CmdLine.config.json";

		const string LocalJsonFileName = "CargoWise.RefDbRepo.DEReferenceData.CmdLine.config.local.json";
	}
}

