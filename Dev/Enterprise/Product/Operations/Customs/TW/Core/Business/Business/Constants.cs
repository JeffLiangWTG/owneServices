namespace Enterprise.Customs.TW.Business
{
	public static class Constants
	{
		public const string DocumentNo = "DOC";
		public const string TradersRemarksNote = "TradersRemarksNote";
		public const string CEI_StyleDescription = "CEI_StyleDescription";
		public const string APNReferenceType = "APN";
		public const string PackType = "PT";
		public const string CCPPrefix = "FFF";
		public const string Figure = "FIG";
		public const string NIL = "NIL";
		public const string Z99 = "Z99";
		public const string Number = "NO";

		public static class ImportExportRegulationCodes
		{
			public const string RegulationsCode581 = "581";
			public const string RegulationsCode541 = "541";
			public const string RegulationsCode602 = "602";
			public const string RegulationsCodeF01 = "F01";
			public const string RegulationsCodeF02 = "F02";
			public const string RegulationsCodeMP1 = "MP1";
			public const string RegulationsCodeMW0 = "MW0";
		}

		public static class RangeTypes
		{
			public const string A1 = "A1";
		}

		public static class CDIStatus
		{
			public const string B = "B";
			public const string F = "F";
		}

		public static class UnitOfQuantityCodes
		{
			public const string Kilograms = "KGM";
			public const string KLT = "KLT";
			public const string LOT = "LOT";
			public const string LTR = "LTR";
			public const string Tonnes = "TNE";
			public const string Yards = "YDS";
			public const string Dozen = "DOZ";
			public const string Pieces = "PCS";
			public const string Centimeter = "CMT";
			public const string Meter = "MTR";
			public const string MilliMeter = "MMT";
			public const string Inch = "INH";
			public const string Foot = "FOT";
			public const string Yard = "YRD";
			public const string Gram = "GRM";
			public const string Pound = "LBR";
		}

		public static class GenAddOnColumnFieldName
		{
			public const string BoxNumber = "TW_BoxNumber";
			public const string BulkApplicationID = "BulkApplicationID";
			public const string BulkPaymentID = "BulkPaymentID";
			public const string TotalDutyTaxFeeAmount = "TotalDutyTaxFeeAmount";
			public const string OtherChargeDeductionAmount = "OtherChargeDeductionAmount";
		}

		public static class PreferenceCodes
		{
			public const string Standard = "STD";
			public const string Preference = "PRE";
			public const string Preference1 = "PR1";
			public const string Preference2 = "PR2";
			public const string ProvisionalPreference1 = "PT1";
			public const string ProvisionalPreference2 = "PT2";
			public const string ProvisionalPreference3 = "PT3";
		}

		public static class CusInBondBill
		{
			public static class ShipmentType
			{
				public const string Import = "IMP";

				public const string Export = "EXP";
			}
		}

		public static class ProcedureCodes
		{
			public const string _01 = "01";
			public const string _1A = "1A";
			public const string _8A = "8A";
			public const string _8D = "8D";
			public const string _02 = "02";
			public const string _04 = "04";
			public const string _31 = "31";
			public const string _35 = "35";
			public const string _37 = "37";
			public const string _38 = "38";
			public const string _39 = "39";
			public const string _3E = "3E";
			public const string _3F = "3F";
			public const string _50 = "50";
			public const string _92 = "92";
			public const string _94 = "94";
			public const string _97 = "97";
			public const string _98 = "98";
			public const string _99 = "99";
			public const string _9T = "9T";
			public const string _9U = "9U";
			public const string _51 = "51";
			public const string _65 = "65";
			public const string _67 = "67";
			public const string _69 = "69";
			public const string _56 = "56";
			public const string _5Y = "5Y";
			public const string _58 = "58";
			public const string _5C = "5C";
			public const string EF = "EF";
		}

		public static class DeclarationTypes
		{
			public static class Import
			{
				public const string B6 = "B6";
				public const string D2 = "D2";
				public const string D7 = "D7";
				public const string D8 = "D8";
				public const string G1 = "G1";
				public const string G2 = "G2";
				public const string G7 = "G7";
				public const string F1 = "F1";
				public const string F2 = "F2";
				public const string F3 = "F3";
				public const string L1 = "L1";
			}

			public static class Export
			{
				public const string B1 = "B1";
				public const string B2 = "B2";
				public const string B8 = "B8";
				public const string B9 = "B9";
				public const string D1 = "D1";
				public const string D5 = "D5";
				public const string G3 = "G3";
				public const string G5 = "G5";
				public const string F4 = "F4";
				public const string F5 = "F5";
			}
		}

		#region UniversalReferenceConstants
		public static class UniversalReferenceConstants
		{
			public static class CusTariffAttributeName
			{
				public const string CustomsRequirements = "CustomsRequirements";
				public const string ImportRegulations = "ImportRegulations";
				public const string ExportRegulations = "ExportRegulations";
				public const string EnvironmentalProtectionTariff = "EnvironmentalProtectionTariff";
				public const string F5FTZDestination = "F5FTZDestination";
			}

			public static class CusTariffAttributeValue
			{
				public const string B = "B";
				public const string T = "T";
				public const string Z = "Z";
				public const string BPartially = "B*";
				public const string TPartially = "T*";
				public const string C = "C";
				public const string LPartially = "L*";
				public const string SPartially = "S*";
			}

			public static class RefCusRateTypes
			{
				public const string CT = "CT";
				public const string AT = "AT";
				public const string TT = "TT";
				public const string SS = "SS";
			}
		}
		#endregion

		public static class GoodsTypeListCacheType
		{
			public const string DNOnly = "DNONLY";
			public const string IFOrDH = "IFDHONLY";
			public const string All = "ALL";
		}

		public static class ConcessionOrder
		{
			public const string Quota = "QUOTA";
		}

		public static class TWNCATKClient
		{
			public const string EHubClientStatusOK = "OK";
			public const string ConfigNameForeService = "TWCustomsNCATK";
		}

		public static class CustomsUnitOfMeasureList
		{
			public const string SquareMetre = "MTK";
		}

		public static class OrgCusCodeType
		{
			public const string CustomCode = "ZZZ";
		}

		public static class Volume
		{
			public const string CubicMeter = "CBM";
		}

		public static class FreePaymentMethodDescriptions
		{
			public const string DutyFree = "Duty-Free";
			public const string TaxFree = "Tax-Free";
		}

		public static class CustomsOffice
		{
			public const string BA = "BA";
			public const string BC = "BC";
			public const string BD = "BD";
			public const string BE = "BE";
			public const string BF = "BF";
			public const string BJ = "BJ";
		}

		public static class RefDocTypes
		{
			public const string MiscellaneousCustomsDocument = "MCD";
		}

		public static class TransportModeTypeCodes
		{
			public const string Sea = "1";
			public const string Air = "4";
		}

		public static class DocumentWrapper
		{
			public const string Tel = "TEL:";
			public const string Fax = "FAX:";
			public const string Email = "EMAIL:";
		}

		public static class CustomsManifestStatus
		{
			public const string AK = "AK";
			public const string AP = "AP";
			public const string EX = "EX";
			public const string RE = "RE";
		}
	}
}
