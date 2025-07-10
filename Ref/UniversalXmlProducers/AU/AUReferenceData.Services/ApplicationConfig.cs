using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class ApplicationConfig
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json";
		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		public static string NexDocReferenceDataUsername => Config[nameof(NexDocReferenceDataUsername)];
		public static string NexDocReferenceDataPassword => Config[nameof(NexDocReferenceDataPassword)];
		public static string NexDocRESTReferenceDataEndPoint => Config[nameof(NexDocRESTReferenceDataEndPoint)];
		public static string NexDocRESTReferenceDataMaxAttempts => Config[nameof(NexDocRESTReferenceDataMaxAttempts)];
		public static string NexDocRESTReferenceDataRetryDelay => Config[nameof(NexDocRESTReferenceDataRetryDelay)];
		public static string OutputDirectory => OutputPath;

		static string exchangeRateRetryDelay;
		public static string ExchangeRateRetryDelay
		{
			get
			{
				if (exchangeRateRetryDelay == null)
				{
					exchangeRateRetryDelay = Config[nameof(ExchangeRateRetryDelay)];
				}
				return exchangeRateRetryDelay;
			}
			private set => exchangeRateRetryDelay = value;
		}

		private static string exchangeRateMaxAttempts;
		public static string ExchangeRateMaxAttempts
		{
			get
			{
				if (exchangeRateMaxAttempts == null)
				{
					exchangeRateMaxAttempts = Config[nameof(ExchangeRateMaxAttempts)];
				}
				return exchangeRateMaxAttempts;
			}
			private set => exchangeRateMaxAttempts = value;
		}

		public static string ExchangeRateUrl => Config[nameof(ExchangeRateUrl)];
		public static string ExchangeRateFileNamePrefix => Config[nameof(ExchangeRateFileNamePrefix)];

		public static string LCTThresholdsUrl => Config[nameof(LCTThresholdsUrl)];

		public static string RefDbServiceURI => Config[nameof(RefDbServiceURI)];
		public static bool IsRefDbServiceSecure => ReadBool(nameof(IsRefDbServiceSecure));
		public static string AHECCFilePrefix => Config[nameof(AHECCFilePrefix)];
		public static string AHECCPartialFilePrefix => Config[nameof(AHECCPartialFilePrefix)];
		public static string AHECCTestFilePrefix => Config[nameof(AHECCTestFilePrefix)];
		public static string AHECCPartialTestFilePrefix => Config[nameof(AHECCPartialTestFilePrefix)];
		public static string AQISCommodityCodesFilePrefix => Config[nameof(AQISCommodityCodesFilePrefix)];
		public static bool AQISCommodityCodesParserIsActive => ReadBool(nameof(AQISCommodityCodesParserIsActive));
		public static string AQISCommodityCode => Config[nameof(AQISCommodityCode)];
		public static string AQISCommodityStatisticalClassificationFilePrefix => Config[nameof(AQISCommodityStatisticalClassificationFilePrefix)];

		public static bool AQISCommodityStatisticalClassificationParserIsActive => ReadBool(nameof(AQISCommodityStatisticalClassificationParserIsActive));
		public static string AQISCommodityStatisticalClassificationTestFilePrefix => Config[nameof(AQISCommodityStatisticalClassificationTestFilePrefix)];
		public static string AQISConcernCodesFilePrefix => Config[nameof(AQISConcernCodesFilePrefix)];
		public static bool AQISConcernCodesParserIsActive => ReadBool(nameof(AQISConcernCodesParserIsActive));
		public static string AQISDocumentTypesFilePrefix => Config[nameof(AQISDocumentTypesFilePrefix)];
		public static bool AQISDocumentTypesParserIsActive => ReadBool(nameof(AQISDocumentTypesParserIsActive));
		public static string AQISEntityCodesFilePrefix => Config[nameof(AQISEntityCodesFilePrefix)];
		public static bool AQISEntityCodesParserIsActive=> ReadBool(nameof(AQISEntityCodesParserIsActive));
		public static string AQISPremisesCodesFilePrefix => Config[nameof(AQISPremisesCodesFilePrefix)];
		public static bool AQISPremisesCodesParserIsActive => ReadBool(nameof(AQISPremisesCodesParserIsActive));
		public static string AQISPremisesCodesPartialFilePrefix => Config[nameof(AQISPremisesCodesPartialFilePrefix)];
		public static bool AQISPremisesCodesPartialParserIsActive => ReadBool(nameof(AQISPremisesCodesPartialParserIsActive));
		public static string AQISProcessingTypeFilePrefix => Config[nameof(AQISProcessingTypeFilePrefix)];
		public static bool AQISProcessingTypeParserIsActive => ReadBool(nameof(AQISProcessingTypeParserIsActive));
		public static string AQISProducerCodesFilePrefix => Config[nameof(AQISProducerCodesFilePrefix)];
		public static bool AQISProducerCodesParserIsActive => ReadBool(nameof(AQISProducerCodesParserIsActive));
		public static string AQISProducerCodesPartialFilePrefix => Config[nameof(AQISProducerCodesPartialFilePrefix)];
		public static bool AQISProducerCodesPartialParserIsActive => ReadBool(nameof(AQISProducerCodesPartialParserIsActive));
		public static string BerthCodesFilePrefix => Config[nameof(BerthCodesFilePrefix)];
		public static bool BerthCodesParserIsActive=> ReadBool(nameof(BerthCodesParserIsActive));
		public static string CharacteristicCodesFilePrefix => Config[nameof(CharacteristicCodesFilePrefix)];
		public static bool CharacteristicCodesParserIsActive => ReadBool(nameof(CharacteristicCodesParserIsActive));
		public static string CMRSeaImpendingArrivalsFilePrefix => Config[nameof(CMRSeaImpendingArrivalsFilePrefix)];
		public static bool CMRSeaImpendingArrivalsParserIsActive => ReadBool(nameof(CMRSeaImpendingArrivalsParserIsActive));
		public static string CMRSeaImpendingArrivalsPartialFilePrefix => Config[nameof(CMRSeaImpendingArrivalsPartialFilePrefix)];
		public static bool CMRSeaImpendingArrivalsPartialParserIsActive => ReadBool(nameof(CMRSeaImpendingArrivalsPartialParserIsActive));
		public static string CMRSeaImpendingArrivalsTestFilePrefix => Config[nameof(CMRSeaImpendingArrivalsTestFilePrefix)];
		public static bool CMRSeaImpendingArrivalsTestParserIsActive => ReadBool(nameof(CMRSeaImpendingArrivalsTestParserIsActive));
		public static string EstablishmentCodesFilePrefix => Config[nameof(EstablishmentCodesFilePrefix)];
		public static bool EstablishmentCodesParserIsActive => ReadBool(nameof(EstablishmentCodesParserIsActive));
		public static string PreferenceRulesFilePrefix => Config[nameof(PreferenceRulesFilePrefix)];
		public static bool PreferenceRulesParserIsActive => ReadBool(nameof(PreferenceRulesParserIsActive));
		public static string PreferenceRuleSchemesFilePrefix => Config[nameof(PreferenceRuleSchemesFilePrefix)];
		public static string PreferenceRulesTestFilePrefix => Config[nameof(PreferenceRulesTestFilePrefix)];
		public static bool PreferenceRulesTestParserIsActive => ReadBool(nameof(PreferenceRulesTestParserIsActive));
		public static string PreferenceRuleSchemesTestFilePrefix => Config[nameof(PreferenceRuleSchemesTestFilePrefix)];
		public static string RefCusTradeGroupCountryFilePrefix => Config[nameof(RefCusTradeGroupCountryFilePrefix)];
		public static bool RefCusTradeGroupCountryParserIsActive => ReadBool(nameof(RefCusTradeGroupCountryParserIsActive));
		public static string RefCusTradeGroupFilePrefix => Config[nameof(RefCusTradeGroupFilePrefix)];
		public static bool RefCusTradeGroupParserIsActive => ReadBool(nameof(RefCusTradeGroupParserIsActive));
		public static string RefundReasonCodesFilePrefix => Config[nameof(RefundReasonCodesFilePrefix)];
		public static bool RefundReasonCodesParserIsActive => ReadBool(nameof(RefundReasonCodesParserIsActive));
		public static string StatisticalClassificationPeriodCharacteristicFilePrefix => Config[nameof(StatisticalClassificationPeriodCharacteristicFilePrefix)];
		public static bool StatisticalClassificationPeriodCharacteristicParserIsActive => ReadBool(nameof(StatisticalClassificationPeriodCharacteristicParserIsActive));
		public static string StatisticalClassificationPeriodCharacteristicTestFilePrefix => Config[nameof(StatisticalClassificationPeriodCharacteristicTestFilePrefix)];
		public static string StatisticalClassificationPeriodSnapshotFilePrefix => Config[nameof(StatisticalClassificationPeriodSnapshotFilePrefix)];
		public static bool StatisticalClassificationPeriodSnapshotParserIsActive => ReadBool(nameof(StatisticalClassificationPeriodSnapshotParserIsActive));
		public static string StatisticalClassificationPeriodSnapshotTestFilePrefix => Config[nameof(StatisticalClassificationPeriodSnapshotTestFilePrefix)];
		public static string TariffClassificationCharacteristicFilePrefix => Config[nameof(TariffClassificationCharacteristicFilePrefix)];
		public static bool TariffClassificationCharacteristicParserIsActive => ReadBool(nameof(TariffClassificationCharacteristicParserIsActive));
		public static string TariffClassificationCharacteristicTestFilePrefix => Config[nameof(TariffClassificationCharacteristicTestFilePrefix)];
		public static string NomenclatureRetryTimes => Config[nameof(NomenclatureRetryTimes)];
		public static string NomenclatureRetryWaitSeconds => Config[nameof(NomenclatureRetryWaitSeconds)];
		private static bool? auNomenclatureForceDownloaded;
		public static bool AUNomenclatureForceDownloaded
		{
			get
			{
				if (auNomenclatureForceDownloaded == null )
				{
					_ = bool.TryParse(Config[nameof(AUNomenclatureForceDownloaded)], out var parsedValue);
					auNomenclatureForceDownloaded = parsedValue;
				}
				return auNomenclatureForceDownloaded.GetValueOrDefault();
			}
			private set => auNomenclatureForceDownloaded = value;
		}
		public static string AUNomenclatureWebsiteMaxAttempts => Config[nameof(AUNomenclatureWebsiteMaxAttempts)];
		public static string EmailSender => Config[nameof(EmailSender)];
		public static string EmailSmtpServer => Config[nameof(EmailSmtpServer)];
		public static string EmailUsername => Config[nameof(EmailUsername)];
		public static string EmailCredentialsPassword => Config[nameof(EmailCredentialsPassword)];
		public static string EmailSmtpPort => Config[nameof(EmailSmtpPort)];
		private static string emailGroup;
		public static string EmailGroup
		{
			get
			{
				if (emailGroup == null)
				{
					emailGroup = Config[nameof(EmailGroup)];
				}
				return emailGroup;
			}
			private set => emailGroup = value;
		}
		public static string OutputPath => Config[nameof(OutputPath)];
		public static string AUTariffsOutputPath => Config[nameof(AUTariffsOutputPath)];
		private static string nomenclatureBasePath;
		public static string NomenclatureBasePath
		{
			get
			{
				if (nomenclatureBasePath == null)
				{
					nomenclatureBasePath = Config[nameof(NomenclatureBasePath)];
				}
				return nomenclatureBasePath;
			}
			private set => nomenclatureBasePath = value;
		}

		private static string aUReferenceFilesDirectory;
		public static string AUReferenceFilesDirectory
		{
			get
			{
				if (aUReferenceFilesDirectory == null)
				{
					aUReferenceFilesDirectory = Config[nameof(AUReferenceFilesDirectory)];
				}
				return aUReferenceFilesDirectory;
			}
			private set => aUReferenceFilesDirectory = value;
		}

		private static string aUReferenceTestFilesDirectory;
		public static string AUReferenceTestFilesDirectory
		{
			get
			{
				if (aUReferenceTestFilesDirectory == null)
				{
					aUReferenceTestFilesDirectory = Config[nameof(AUReferenceTestFilesDirectory)];
				}
				return aUReferenceTestFilesDirectory;
			}
			private set => aUReferenceTestFilesDirectory = value;
		}

		private static string aUReferenceFilesPartialDirectory;
		public static string AUReferenceFilesPartialDirectory
		{
			get
			{
				if (aUReferenceFilesPartialDirectory == null)
				{
					aUReferenceFilesPartialDirectory = Config[nameof(AUReferenceFilesPartialDirectory)];
				}
				return aUReferenceFilesPartialDirectory;
			}
			private set => aUReferenceFilesPartialDirectory = value;
		}

		private static string aUReferenceTestFilesPartialDirectory;
		public static string AUReferenceTestFilesPartialDirectory
		{
			get
			{
				if (aUReferenceTestFilesPartialDirectory == null)
				{
					aUReferenceTestFilesPartialDirectory = Config[nameof(AUReferenceTestFilesPartialDirectory)];
				}
				return aUReferenceTestFilesPartialDirectory;
			}
			private set => aUReferenceTestFilesPartialDirectory = value;
		}

		public static bool ReadBool(string propertyName)
		{
			_ = bool.TryParse(Config[propertyName], out var parsedValue);
			return parsedValue;
		}
	}
}
