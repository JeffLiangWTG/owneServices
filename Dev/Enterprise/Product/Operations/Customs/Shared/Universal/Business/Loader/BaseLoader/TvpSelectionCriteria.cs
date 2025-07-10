namespace Enterprise.Customs.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "This class is designed to be inherited by other classes, allowing shared functionality without enforcing static behavior.")]
	public class TvpSelectionCriteria
	{
		public const string QualifiedName = "TvpSelectionCriteria";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL column name")]
		public class Columns
		{
			public const string CriteriaId = "CriteriaId";
			public const string TariffPK = "TariffPK";
			public const string EffectiveDate = "EffectiveDate";
			public const string TradeGroupCountry = "TradeGroupCountry";
			public const string DataGrouping = "DataGrouping";
			public const string Preference = "Preference";
			public const string OrderNumber = "OrderNumber";
		}
	}

	public static class TvpAdditionalCodes
	{
		public const string QualifiedName = "dbo.TVP_AdditionalCodes";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL column name")]
		public static class Columns
		{
			public const string Id = "Id";
			public const string CriteriaId = "CriteriaId";
			public const string AdditionalCode = "AdditionalCode";
		}
	}

	public static class TvpSecondTradeGroup
	{
		public const string QualifiedName = "dbo.TVP_SecondTradeGroup";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL column name")]
		public static class Columns
		{
			public const string Id = "Id";
			public const string CriteriaId = "CriteriaId";
			public const string SecondTradeGroup = "SecondTradeGroup";
		}
	}
}
