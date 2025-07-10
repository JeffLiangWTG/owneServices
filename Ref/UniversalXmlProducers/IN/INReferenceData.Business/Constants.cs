using System;
using System.Globalization;
using System.Text.Json;
using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class Constants
	{
		public static readonly DateTime DefaultStartDate = DateTime.ParseExact("2024-01-01 00:00:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		public static readonly DateTime DefaultStartDate2025 = DateTime.ParseExact("2025-01-01 00:00:01", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		public static readonly DateTime DefaultEndDate = DateTime.ParseExact("2079-06-06 23:59:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

		public static class ProgramFunctions
		{
			public const string DBKTariff = "DBKTARIFF";
			public const string EDILocation = "EDILOCATION";
			public const string ExchangeRate = "EXCHANGERATE";
			public const string Tariff = "TARIFF";
			public const string ErrorCodes = "ERRORCODES";
			public const string WarehouseCode = "WAREHOUSECODE";
		}

		public static class DataGrouping
		{
			public const string IN = "IN";
		}

		public static class CodeListAttributes
		{
			public const string EmailAddress = "EmailAddress";
			public const string Address = "Address";
		}

		public static class CodeListTypes
		{
			public const string CUSOF = "CUSOF";
			public const string CU1 = "CU1";
			public const string CgmAirErrorCode = "CGAER";
			public const string CgmSeaErrorCode = "CGSER";
			public const string BeFreshErrorCode = "BEFER";
			public const string BeAmendmentErrorCode = "BEAER";
			public const string SbErrorCode = "SBERR";
			public const string WarehouseCode = "WRHCD";
		}

		public static class TariffTypes
		{
			public const string CTH = "CTH";
			public const string DrawbackSchedule = "DBK";
		}

		public static class RateTypes
		{
			public const string DrawbackSchedule = "DBK";
		}

		public static class EDILocation
		{
			public static class FunctionCodes
			{
				public const string _1 = "1";
				public const string _2 = "2";
				public const string _3 = "3";
				public const string _4 = "4";
				public const string _5 = "5";
				public const string _6 = "6";
				public const string _7 = "7";
				public const string B = "B";
			}

			public static class TransportModes
			{
				public const string Air = "AIR";
				public const string Fixed = "FIX";
				public const string Mail = "MAI";
				public const string Rail = "RAI";
				public const string Road = "ROA";
				public const string Sea = "SEA";
			}
		}

		public static class ExchangeRate
		{
			public static class Types
			{
				public const string Customs = "CUS";
				public const string CustomsExport = "CUE";
			}
		}

		public static class Tariff
		{
			public static class Pdf
			{
				public static class Patterns
				{
					public static string TariffItemOptionalRawPattern => $@"^{TariffItemRawPattern}?$";
					public const string TariffItemRawPattern = @"([#*`]{0,2}\d{4}(?:\s\d{2}){0,2})";
				}
			}

			public static class Validation
			{
				public const int DeclarableTariffCodeLength = 8;

				public static class Patterns
				{
					public const string TariffItemCleanPattern = @"^(\d{4}(\d{2}){0,2})?$";
					public const string HyphensPattern = "^[-]*$";
					public const string SymbolsInTariffItemPattern = @"[#*`\s-]";
					public const string StandardRateWithHyphenPattern = @"^(?=.*-)[-\s]+$";
				}
			}

			public static class DataExtraction
			{
				public const float SameLineVerticalTolerance = 5f;
				public const float ChunkSeparationWidth = 5.5f;
				public const float LeftMargin = 15f;
				public const float NearstDistanceTolerance = 35f;
				public const float SuperscriptVerticalTolerance = 3.5f;
				public const float SubscriptVerticalTolerance = 2f;

				public const int PossibleDescriptionStart = 170;
				public const int PossibleDescriptionEnd = 400;

				public static class Patterns
				{
					public const string StartWithHyphenPattern = @"^[-]+.*$";
					public const string TariffAndHyphenPattern = @"^([#*]{0,2}\d{4}(?:\s\d{2}){0,2}|\d{2,4})\s*((?:-\s*)+)\s*(.*?)$";
					public const string DescriptionPattern = "^(?!-).+(?![0-9 ]+$)";
					public const string RateRawPattern = @"Free|100%|[0-9]{1,2}(?:\.[0-9]{1,2})?%";
					public const string PreferentialRateRawPattern = @"(?<!\S)(?:\d{1,3}%|-)(?!\S)";
					public const string HyphenSymbols = "\u2013\u2014_-";
					public const string EndWithHyphensPattern = @"^(.*?)(-{1,2})$";

					public const string DescriptionStartsWithHyphenPattern = @"^(-[\s-]+)+";
					public const string DescriptionTouchesHyphenFromStartPattern = @"^(-[\s-]*)[A-Z][a-z].*$";

					public const string StartWithRomanNumberPattern = @"^(I{1,3}|IV|V|VI{0,3}|IX|X|XI{0,3}|XIV|XV|XVI{0,3}|XIX|XX)\.\s*[\u2013\u2014-]\s*.*$";

					public static string StartWithHyphenOrDashPattern => $@"^(?![\s]+$)\*?[\s{HyphenSymbols}]*[{HyphenSymbols}]+[\s{HyphenSymbols}]*$";
					public static string StartWithTariffItemPattern => $@"^{Pdf.Patterns.TariffItemRawPattern}(\s.*)?$";
					public static string RateWithConditionPattern => $@"^\s*[#*]*\s*({RateRawPattern})\s*(?:.*)?$";
					public static string RatePattern => $@"^\s*[#*]*\s*({RateRawPattern})\s*$";
					public static string PreferentialRatePattern => $@"^{PreferentialRateRawPattern}$";
					public static string TariffDataTogetherPattern => $@"^{Pdf.Patterns.TariffItemRawPattern}\s+([-]+(?:\s*-\s*)*)\s+(.*?)\s+(" + AppConfig.Tariff.WeightUnitsInDescriptionRegEx + $@")\s+({RateRawPattern})\s+({PreferentialRateRawPattern})$";
					public static string DescriptionUnitRatesTogetherPattern => $@"^(.*?)(" + AppConfig.Tariff.WeightUnitsInDescriptionRegEx + $@")\s*({RateRawPattern})\s*({PreferentialRateRawPattern})?$";
					public static string DescriptionEndsWithUnitsPattern => $@"(?<=\S| ){AppConfig.Tariff.WeightUnitsInDescriptionRegEx}$";
					public static string UnitRateTogetherPattern => $@"^\s*({AppConfig.Tariff.WeightUnitsInDescriptionRegEx})\s*([*#]*\s*(?:{RateRawPattern}))\s*$";
				}
			}

			public static class Processing
			{
				public static JsonSerializerOptions JsonSerializerOptions => new JsonSerializerOptions
				{
					WriteIndented = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				};
			}
		}

		public static class ErrorCodes
		{
			public static class DataSubSource
			{
				public const string AirCgm = "Air CGM";
				public const string SeaCgm = "Sea CGM";
				public const string BeFresh = "BE Fresh";
				public const string BeAmendment = "BE Amendment";
				public const string Sb = "SB";
			}

			public static class MessageType
			{
				public const string Fresh = "Fresh";
				public const string Amendment = "Amendment";
			}
		}
	}
}
