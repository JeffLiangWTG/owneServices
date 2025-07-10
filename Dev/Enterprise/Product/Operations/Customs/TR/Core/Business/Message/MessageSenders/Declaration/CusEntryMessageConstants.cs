namespace Enterprise.Customs.TR.Business
{
	static class CusEntryMessageConstants
	{
		public static class TransportModes
		{
			public const string Waterway = "3";
			public const string Road = "4";
			public const string Air = "5";
			public const string Rail = "6";
			public const string Other = "_";
		}

		public static class TurkishAnswers
		{
			public const string Yes = "EVET";
			public const string No = "HAYIR";
			public const string ThereIsRelationShip = "VAR";
			public const string NoRelationShip = "YOK";
		}

		public static class CountryConstants
		{
			public const string CountryMapType = "CNTRY";
			public const string PaymentType = "PESIN";
			public const string VATCode = "KDV";
			public const string PackTypeContainer = "KN";
			public const string PackTypeQuantity = "BI";
			public const string TRNCTS4ErrorCodes = "NCTER";
		}

		public static class DateFormat
		{
			public const string DayMonthYear = "dd/MM/yyyy";
		}

		public static class ScopeOfTransaction
		{
			public const string NotIncludeAllItems = "3";
			public const string IncludeAllItems = "2";
		}

		public static class OrganizationRelationship
		{
			public const string NotRelated = "6";
			public const string Related = "0";
		}

		public static class ProcedureCodes
		{
			public const string _7200 = "7200";
			public const string _7241 = "7241";
			public const string _7272 = "7272";
			public const string _8000 = "8000";
			public const string _8100 = "8100";
			public const string _8200 = "8200";
		}

		public static class VehicleInfo
		{
			public const string BrandType = "1";
			public const string NonBrandType = "0";
			public const string EmptyModelYear = "0000";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
		public static class ExportUnionConstants
		{
			public const string SoftwareHouseCode = "SKDWL20AO1";

			public static class Answers
			{
				public const string YesAbbreviation = "E";
				public const string NoAbbreviation = "H";
				public const string YesValue = "1";
				public const string NoValue = "0";
			}

			public static class DeclarationTypes
			{
				public const string ExportType = "EX";
				public const string MainActivity = "1";
			}

			public static class TradeTypes
			{
				public const string ECommerce = "1";
				public const string NotECommerce = "2";
			}

			public static class CompanyTypes
			{
				public const string Exporter = "Exporter";
				public const string Importer = "Importer";
				public const string DeclarationOwner = "DeclarationOwner";
				public const string FinancialConsultant = "FinancialConsultant";
				public const string Responsible = "Responsible";
				public const string Producer = "Producer";
			}
		}
	}
}
