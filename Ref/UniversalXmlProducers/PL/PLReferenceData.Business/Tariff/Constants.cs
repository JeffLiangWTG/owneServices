using System;
using System.Collections.Generic;
using System.Globalization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	public static class Constants
	{
		public const int ExpiredYears = 5;
		public static DateTime ConstantEndDate => Convert.ToDateTime("2079-06-06T23:59:00", CultureInfo.InvariantCulture);
		public const string EuropeanUnionCode = "EUN";
		public const char NationalMeasureStartingLetter = 'p';

		public const string TariffUniversalReferenceDataXmlFilename = "PLTariffData.xml";
		public const string CW1ValueForDuty = "VFD";

		public static class MeasureType
		{
			public const string MeasureVatVATApplicabilityTypeId = "305";
			public const string MeasureRateTypeId = "306";
			public const string P01 = "p01";
			public const string P02 = "p02";
			public const string P03 = "p03";
			public const string P04 = "p04";
			public const string P05 = "p05";
			public const string P06 = "p06";
			public const string P07 = "p07";
			public const string P09 = "p09";
			public const string P10 = "p10";
			public const string P11 = "p11";
		}

		public static class DutyCode
		{
			public const string Ese = "ESE";
			public const string Spe = "SPE";
			public const string Min = "MIN";
			public const string Rid = "RID";
			public const string Ord = "ORD";
		}

		public static class DutyExpressionId
		{
			public const string ExpressionId_01 = "01";
			public const string ExpressionId_04 = "04";
			public const string ExpressionId_15 = "15";
			public const string ExpressionId_17 = "17";
			public const string ExpressionId_35 = "35";
			public const string ExpressionId_37 = "37";
			public const string NotApplicable_95 = "95";
			public const string NotApplicable_96 = "96";
			public const string NotApplicable_AN = "AN";
			public const string NotApplicable_AZ = "AZ";
		}

		public static class PTypeMeasureComment
		{
			public const string P01 = "p01-Graniczna kontrola sanitarna";
			public const string P02 = "p02-Dozór techniczny";
			public const string P03 = "p03-Ograniczenie wywozu";
			public const string P04 = "p04-Zakaz przywozu";
			public const string P05 = "p05-Zakaz przywozu (jeśli wyrób zawiera azbest)";
			public const string P06 = "p06-Kontrola jakości handlowej";
			public const string P07 = "p07-Ograniczenia/zakazy";
			public const string P09 = "p09-Ograniczenie przywozu";
			public const string P10 = "p10-Graniczna kontrola fitosanitarna";
			public const string P11 = "p11-Jakość paliw stałych";
			public const string MeasureRateTypeId = "Excise tax";
		}

		public static Dictionary<string, string> PTypeMeasureCommentMeasureTypeDictionary => typeMeasureCommentMeasureTypeDictionary;
		static readonly Dictionary<string, string> typeMeasureCommentMeasureTypeDictionary = new Dictionary<string, string>()
		{
			{ MeasureType.P01, PTypeMeasureComment.P01 },
			{ MeasureType.P02, PTypeMeasureComment.P02 },
			{ MeasureType.P03, PTypeMeasureComment.P03 },
			{ MeasureType.P04, PTypeMeasureComment.P04 },
			{ MeasureType.P05, PTypeMeasureComment.P05 },
			{ MeasureType.P06, PTypeMeasureComment.P06 },
			{ MeasureType.P07, PTypeMeasureComment.P07 },
			{ MeasureType.P09, PTypeMeasureComment.P09 },
			{ MeasureType.P10, PTypeMeasureComment.P10 },
			{ MeasureType.P11, PTypeMeasureComment.P11 },
			{ MeasureType.MeasureRateTypeId, PTypeMeasureComment.MeasureRateTypeId }
		};
	}
}
