using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class Constants
	{
		public static class ProgramFunctions
		{
			public const string ExchangeRate = "EXCHANGERATE";
			public const string BanderolTariff = "BANDEROLTARIFF";
			public const string HarmonizedTariff = "HARMONIZEDTARIFF";
			public const string TradeGroup = "TRADEGROUP";
			public const string ExportUnionTariff = "EXPORTUNIONTARIFF";
			public const string ETradeExemptionCodes = "ETRADEEXEMPTIONCODES";
			public const string CodeLists = "CODELISTS";
			public const string MeursingRates = "MEURSINGRATES";
			public const string TRStampDuty = "TRSTAMPDUTY";
			public const string TRDutyCodes = "TRDUTYCODES";
			public const string TRRefCusProcedureCodes = "TRREFCUSPROCEDURECODES";
		}

		public static class DataSources
		{
			public const string ExchangeRate = "TR Exchange Rates";
			public const string TariffAdditionalCodeList = "TR Tariff Additional Code Lists";
			public const string HsnTariff = "TR HSN Tariff";
			public const string WarehouseCodes = "TR Warehouse Codes";
			public const string SupportingDocumentsCodes = "TR Supporting Documents Codes";
			public const string TradeGroupAndCountries = "TR Trade Groups and Countries";
			public const string ExportUnionCountryCodes = "TR Export Union Country Codes";
		}

		public static class XmlFileNames
		{
			public const string TariffAdditionalCodeList = "TRTariffAdditionalCodeLists.xml";
			public const string WarehouseCodes = "TRWarehouseCodes.xml";
			public const string SupportingDocumentsCodes = "TRSupportingDocumentsCodes.xml";
			public const string ExportUnionCountryCodes = "TRExportUnionCountryCodes.xml";
		}

		public static class ExcelFileNames
		{
			public const string TariffAdditionalCodeList = "TariffAdditionalCodesList.xlsx";
			public const string WarehouseCodes = "TRWarehouseCodes.xlsx";
			public const string SupportingDocumentsCodes = "TRSupportingDocumentsCodes.xlsx";
			public const string ExportUnionCountryCodes = "ExportUnionCountryCodes.xlsx";
		}

		public static class ExchangeRateTypes
		{
			public const string Export = "CUE";
			public const string Import = "CUS";
		}

		public static DateTime MinimumDateTime => new DateTime(2020, 01, 01, 00, 00, 00);
		public static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 59);
		public static DateTime MaximumSmallDateTime => new DateTime(2079, 06, 06, 23, 59, 00);
		public static DateTime TariffStartDate => new DateTime(2021, 01, 01, 00, 00, 00);
		public static DateTime HsnTariffStartDate => new DateTime(2025, 01, 01, 00, 00, 00);
		public static DateTime MeursingRateStartDate => new DateTime(2023, 01, 01, 00, 00, 00);
		public static DateTime ExportUnionStartDate => new DateTime(2024, 01, 01, 00, 00, 00);
		public static DateTime HsnTariffRateStartDate => new DateTime(2025, 01, 01, 00, 00, 00);
		public static DateTime HsnTariffPublicationTime => new DateTime(2025, 01, 01, 00, 00, 00);

		public const string CountryCodeTR = "TR";

		public static class TradeGroup
		{
			public const string AllCountries = "All Countries";
			public const string TradeGroupEU = "EU";
			public const string TradeGroupNonEU = "Non-EU";
		}

		public const string Yes = "YES";

		public static class TariffType
		{
			public static class Code
			{
				public const string ETRBN = "ETRBN";
				public const string HSN = "HSN";
				public const string TREUA = "TREUA";
				public const string ETR = "ETR";
				public const string MEU = "MEU";
			}

			public static class Description
			{
				public const string ETRBN = "Turkey eTrade Banderol Tariffs";
				public const string HSN = "Turkey Harmonized Tariff";
				public const string TREUA = "TR Export Union Tariff Codes";
				public const string ETR = "Turkey eTrade Exemption Codes";
				public const string MEU = "Turkey Meursing Rates";
			}
		}

		public static class TariffRateType
		{
			public static class Code
			{
				public const string BAN = "BAN";
				public const string DTY = "DTY";
				public const string SpecialConsumptionDuty = "SCD";
				public const string Excise = "EXC";
			}

			public static class Description
			{
				public const string BAN = "TRT Banderol Duty";
				public const string SpecialConsumptionDuty = "Special Consumption Duty";
			}

			public static class CustomsValueFormula
			{
				public const string CV = "CV";
			}
		}

		public static class TariffRateCode
		{
			public static class Code
			{
				public const string _10 = "10";
				public const string _75 = "75";
				public const string SpecialConsumptionDuty = "10";
				public const string _39 = "39";
			}

			public static class Description
			{
				public const string _75 = "TRT Banderol Duty";
				public const string SpecialConsumptionDuty = "Customs Special Consumption Duty";
			}
		}

		public static class TariffUOM
		{
			public static class Type
			{
				public const string CU1 = "CU1";
				public const string CU2 = "CU2";
				public const string CU3 = "CU3";
				public const string CU4 = "CU4";
				public const string CU5 = "CU5";
			}

			public static class Unit
			{
				public const string KGM = "KGM";
			}
		}

		public static class Preference
		{
			public static class Code
			{
				public const string STD = "STD";

				public const string OtherCountries = "DU";
			}
		}

		public static class TariffAttribute
		{
			public static class Name
			{
				public const string ISETRADEBANDEROL = "ISETRADEBANDEROL";
			}

			public static class Value
			{
				public const string Y = "Y";
			}
		}

		public static class RefCusCodeType
		{
			public const string TariffAdditionalCodeListCode = "ADDCD";
			public const string WarehouseCodes = "TRCWH";
			public const string SupportingDocumentsCodes = "SUPDC";
			public const string ExportUnionCountryCodes = "TREUC";
			public const byte MaxLength = 10;
		}
	}
}
