using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class AppConfig
	{
		public static class Nomenclature
		{
			public static class DownloadUrl
			{
				public static string HsSectionsAndChapters => Config["nomenclature:downloadUrl:hsSectionsAndChapters"];
				public static string HsNomenclatureDescription => Config["nomenclature:downloadUrl:hsNomenclatureDescription"];
				public static string HsNomenclatureEnglishDescription => Config["nomenclature:downloadUrl:hsNomenclatureEnglishDescription"];
			}
		}

		public static class TradeGroup
		{
			public static class DownloadUrl
			{
				public static string WtoFtaCountries => Config["tradeGroup:downloadUrl:wtoFtaCountries"];
				public static string Countries => Config["tradeGroup:downloadUrl:countries"];
			}
		}

		public static class Tariff
		{
			public static class DownloadUrl
			{
				public static string TariffColumn1And3 => Config["tariff:downloadUrl:tariffColumn1And3"];
				public static string TariffColumn2 => Config["tariff:downloadUrl:tariffColumn2"];
				public static string TariffChineseDescription => Config["tariff:downloadUrl:tariffChineseDescription"];
				public static string TariffEnglishDescription => Config["tariff:downloadUrl:tariffEnglishDescription"];
				public static string EnvironmentalProtectionTariffs => Config["tariff:downloadUrl:environmentalProtectionTariffs"];
				public static string PublicationDateTime => Config["tariff:downloadUrl:publicationDateTime"];
			}

			public static string ExciseTaxesFileName => Config["exciseTaxesFileName"];
			public static string ExciseTaxesFileNamePublicationTime => Config["exciseTaxesFileNamePublicationTime"];
			public static string TWTariffF5ValidationFileName => Config["twTariffF5ValidationFileName"];
		}

		public static class ExchangeRate
		{
			public static class DownloadUrl
			{
				public static string ExchangeRateText => Config["exchangeRate:downloadUrl:exchangeRateText"];
				public static string ExchangeRateJSON => Config["exchangeRate:downloadUrl:exchangeRateJSON"];
			}
		}

		public static class CodeLists
		{
			public static class Shared
			{
				public static string BaseUrl => Config["codeLists:shared:baseUrl"];
				public static string PublicationDateRegex => Config["codeLists:shared:publicationDateRegex"];
				public static string PublicationDateRegexTest => Config["codeLists:shared:publicationDateRegexTest"];
			}

			public static class TaiwanPackingHouse
			{
				public static class Url
				{
					public static string BaseUrl => Config["codeLists:taiwanPackingHouse:url:baseUrl"];
					public static string DownloadUrl => Config["codeLists:taiwanPackingHouse:url:downloadUrl"];
				}
				public static class Regex
				{
					public static string DownloadFileNameRegex => Config["codeLists:taiwanPackingHouse:regex:downloadFileNameRegex"];
					public static string DownloadFileNameRegexTest => Config["codeLists:taiwanPackingHouse:regex:downloadFileNameRegexTest"];
				}
			}

			public static class TaiwanUnitsOfMeasurement
			{
				public static string DownloadUrl => Config["codeLists:taiwanUnitsOfMeasurement:downloadUrl"];

				public static class Regex
				{
					public static string CodeRegex => Config["codeLists:taiwanUnitsOfMeasurement:regex:codeRegex"];
					public static string DownloadFileNameRegex => Config["codeLists:taiwanUnitsOfMeasurement:regex:downloadFileNameRegex"];
					public static string DownloadFileNameRegexTest => Config["codeLists:taiwanUnitsOfMeasurement:regex:downloadFileNameRegexTest"];
				}
			}

			public static class TaiwanSCECA
			{
				public static string DownloadUrl => Config["codeLists:taiwanSCECA:downloadUrl"];
			}

			public static class TaiwanControllingAgency
			{
				public static string DownloadUrl => Config["codeLists:taiwanControllingAgency:downloadUrl"];
			}

			public static class TaiwanAircraftPartCAACodeCategory
			{
				public static string DownloadUrl => Config["codeLists:taiwanAircraftPartCAACodeCategory:downloadUrl"];
			}

			public static class TaiwanAircraftPartCAACCode
			{
				public static string DownloadUrl => Config["codeLists:taiwanAircraftPartCAACCode:downloadUrl"];
			}

			public static class TaiwanCustomsOffices
			{
				public static string DownloadUrl => Config["codeLists:taiwanCustomsOffices:downloadUrl"];
			}

			public static class FacilityCode
			{
				public static class DownloadUrl
				{
					public static string DischargingStoringKeelung => Config["codeLists:facilityCode:downloadUrl:dischargingStoringKeelung"];
					public static string DischargingStoringTaipei => Config["codeLists:facilityCode:downloadUrl:dischargingStoringTaipei"];
					public static string DischargingStoringTaichung => Config["codeLists:facilityCode:downloadUrl:dischargingStoringTaichung"];
					public static string DischargingStoringKaohsiung => Config["codeLists:facilityCode:downloadUrl:dischargingStoringKaohsiung"];
				}
			}

			public static class TaiwanCommodityInspection
			{
				public static class DownloadUrl
				{
					public static string ExamingZoneKeelung => Config["codeLists:taiwanCommodityInspection:downloadUrl:examingZoneKeelung"];
					public static string ExamingZoneTaipei => Config["codeLists:taiwanCommodityInspection:downloadUrl:examingZoneTaipei"];
					public static string ExamingZoneTaichung => Config["codeLists:taiwanCommodityInspection:downloadUrl:examingZoneTaichung"];
					public static string ExamingZoneKaohsiung => Config["codeLists:taiwanCommodityInspection:downloadUrl:examingZoneKaohsiung"];
				}
			}

			public static class TaiwanImportRegulations
			{
				public static string DownloadUrl => Config["codeLists:taiwanImportRegulations:downloadUrl"];
			}

			public static class TaiwanExportRegulations
			{
				public static string DownloadUrl => Config["codeLists:taiwanExportRegulations:downloadUrl"];
			}

			public static class TaiwanCPT016RejectionReason
			{
				public static string DownloadUrl => Config["codeLists:taiwanCPT016RejectionReason:downloadUrl"];
				public static string PublicationDateTime => Config["codeLists:taiwanCPT016RejectionReason:publicationDateTime"];
			}

			public static class TWCPT_017_Error_DocumentOrRequiredFormalities
			{
				public static string DownloadUrl => Config["codeLists:twCPT_017_Error_DocumentOrRequiredFormalities:downloadUrl"];
				public static string PublicationDateTime => Config["codeLists:twCPT_017_Error_DocumentOrRequiredFormalities:publicationDateTime"];
			}

			public static class TaiwanTWCPT_018_ResponseToWarehouse
			{
				public static string DownloadUrl => Config["codeLists:taiwanTWCPT_018_ResponseToWarehouse:downloadUrl"];
				public static string PublicationDateTime => Config["codeLists:taiwanTWCPT_018_ResponseToWarehouse:publicationDateTime"];
			}
		}

		const string JsonConfigFileName = "CargoWise.RefDbRepo.TaiwanReferenceData.config.json";
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
	}
}
