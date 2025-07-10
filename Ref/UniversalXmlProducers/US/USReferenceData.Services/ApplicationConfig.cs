using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.USReferenceData.Services
{
	public sealed class ApplicationConfig
	{
		static string jsonConfigFile = "CargoWise.RefDbRepo.USReferenceData.CmdLine.config.json";
#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
		}

		public static ApplicationConfig InstanceForTest => new ApplicationConfig();
#endif
		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		public ApplicationConfig()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile(jsonConfigFile)
				.Build();

			OutputPath = configuration[nameof(OutputPath)];
			CensusEndPoint = configuration[nameof(CensusEndPoint)];
			CensusTradeDocumentReferenceLibraryEndPoint = configuration[nameof(CensusTradeDocumentReferenceLibraryEndPoint)];
			CargoReleaseConditionCodeURL = configuration[nameof(CargoReleaseConditionCodeURL)];
			InputPath = configuration[nameof(InputPath)];

			ExchangeRateFilePageURL = configuration[nameof(ExchangeRateFilePageURL)];
			EARFilesCreatedByBISURL = configuration[nameof(EARFilesCreatedByBISURL)];
			EARFilesBaseURL = configuration[nameof(EARFilesBaseURL)];
			ExchangeRateFileBaseURL = configuration[nameof(ExchangeRateFileBaseURL)];

			int exchangeRateDelayDays = int.TryParse(configuration[nameof(ExchangeRateDelayDays)], out exchangeRateDelayDays) ? exchangeRateDelayDays : 10;
			ExchangeRateDelayDays = exchangeRateDelayDays;
			int downloadFileRetryTimes = int.TryParse(configuration[nameof(DownloadFileRetryTimes)], out downloadFileRetryTimes) ? downloadFileRetryTimes : 1;
			DownloadFileRetryTimes = downloadFileRetryTimes;

			CustomsBorderProtectionGoverment = configuration[nameof(CustomsBorderProtectionGoverment)];
			UnitedNationsStandardProductAndServiceCodesURLForPDF = configuration[nameof(UnitedNationsStandardProductAndServiceCodesURLForPDF)];
			UnitedNationsStandardProductAndServiceCodesURLForExcel = configuration[nameof(UnitedNationsStandardProductAndServiceCodesURLForExcel)];
			ACEDISImplementationGuideURLForPDF = configuration[nameof(ACEDISImplementationGuideURLForPDF)];
			ACEDDTCITARExemptionCodesURLForPDF = configuration[nameof(ACEDDTCITARExemptionCodesURLForPDF)];
			ACEHTSCodesForPGAURL = configuration[nameof(ACEHTSCodesForPGAURL)];
			ACEHTSCodesForEV1URL = configuration[nameof(ACEHTSCodesForEV1URL)];
			EV1ExcludedTariffs = configuration[nameof(EV1ExcludedTariffs)];

			USIncomingMessageEHubServerAddress = configuration[nameof(USIncomingMessageEHubServerAddress)];
			USIncomingMessageEHubClientID = configuration[nameof(USIncomingMessageEHubClientID)];
			USIncomingMessageEHubClientPassword = configuration[nameof(USIncomingMessageEHubClientPassword)];
			USIncomingMessageQueryMessageRecipientID = configuration[nameof(USIncomingMessageQueryMessageRecipientID)];
			USIncomingMessageQueryMessageApplicationCode = configuration[nameof(USIncomingMessageQueryMessageApplicationCode)];
			USIncomingMessageQueryMessageType = configuration[nameof(USIncomingMessageQueryMessageType)];
			USIncomingMessageQueryFilerCode = configuration[nameof(USIncomingMessageQueryFilerCode)];
			USIncomingMessageQueryPortCode = configuration[nameof(USIncomingMessageQueryPortCode)];
			ScheduleKCsvFileName = configuration[nameof(ScheduleKCsvFileName)];
			int uSIncomingMessageRetryTimes = int.TryParse(configuration[nameof(USIncomingMessageRetryTimes)], out uSIncomingMessageRetryTimes) ? uSIncomingMessageRetryTimes : 5;
			USIncomingMessageRetryTimes = uSIncomingMessageRetryTimes;
			int uSIncomingMessageMillisecondsBetweenRetries = int.TryParse(configuration[nameof(USIncomingMessageMillisecondsBetweenRetries)], out uSIncomingMessageMillisecondsBetweenRetries) ? uSIncomingMessageMillisecondsBetweenRetries : 1000;
			USIncomingMessageMillisecondsBetweenRetries = uSIncomingMessageMillisecondsBetweenRetries;
			CurrencyExchangeRateMessageType = configuration[nameof(CurrencyExchangeRateMessageType)];
			USIncomingMessageResponseMessageType = configuration[nameof(USIncomingMessageResponseMessageType)];

			A99JsonFileName = configuration[nameof(A99JsonFileName)];
			DateTime a99TariffPublicationTime = DateTime.TryParse(configuration[nameof(A99TariffPublicationTime)], out a99TariffPublicationTime) ? a99TariffPublicationTime : DateTime.Today;
			A99TariffPublicationTime = a99TariffPublicationTime;

			LicenseTypeAttributeFileName = configuration[nameof(LicenseTypeAttributeFileName)];
			MEUAttributeFileName = configuration[nameof(MEUAttributeFileName)];

			AESDispostionCodeURL = configuration[nameof(AESDispostionCodeURL)];
		}

		public string OutputPath { get; private set; }
		public string CensusEndPoint { get; private set; }
		public string CensusTradeDocumentReferenceLibraryEndPoint { get; private set; }
		public string CargoReleaseConditionCodeURL { get; private set; }
		public string InputPath { get; private set; }

		public string ExchangeRateFilePageURL { get; private set; }
		public string EARFilesCreatedByBISURL { get; private set; }
		public string EARFilesBaseURL { get; private set; }
		public string ExchangeRateFileBaseURL { get; private set; }

		public int ExchangeRateDelayDays { get; private set; }
		public int DownloadFileRetryTimes { get; private set; }

		public string CustomsBorderProtectionGoverment { get; private set; }
		public string UnitedNationsStandardProductAndServiceCodesURLForPDF { get; private set; }
		public string UnitedNationsStandardProductAndServiceCodesURLForExcel { get; private set; }
		public string ACEDISImplementationGuideURLForPDF { get; private set; }
		public string ACEDDTCITARExemptionCodesURLForPDF { get; private set; }
		public string ACEHTSCodesForPGAURL { get; private set; }
		public string ACEHTSCodesForEV1URL { get; private set; }
		public string EV1ExcludedTariffs { get; private set; }

		public string USIncomingMessageEHubServerAddress { get; private set; }
		public string USIncomingMessageEHubClientID { get; private set; }
		public string USIncomingMessageEHubClientPassword { get; private set; }
		public string USIncomingMessageQueryMessageRecipientID { get; private set; }
		public string USIncomingMessageQueryMessageApplicationCode { get; private set; }
		public string USIncomingMessageQueryMessageType { get; private set; }
		public string USIncomingMessageQueryFilerCode { get; private set; }
		public string USIncomingMessageQueryPortCode { get; private set; }
		public string ScheduleKCsvFileName { get; private set; }
		public int USIncomingMessageRetryTimes { get; private set; }
		public int USIncomingMessageMillisecondsBetweenRetries { get; private set; }
		public string CurrencyExchangeRateMessageType { get; private set; }
		public string USIncomingMessageResponseMessageType { get; private set; }

		public string A99JsonFileName { get; private set; }
		public DateTime A99TariffPublicationTime { get; private set; }

		public string LicenseTypeAttributeFileName { get; private set; }
		public string MEUAttributeFileName {  get; private set; }

		public string AESDispostionCodeURL {  get; private set; }
	}
}
