namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	internal static class Constants
	{
		internal const string DateFormatCCYYMMDD = "yyyyMMdd";

		internal static class DutyTaxFeeTypeCodes
		{
			internal const string CIFAndCValue = "CIF";
			internal const string TransactionValue = "TRN";
			internal const string TotalDutiesDue = "TDD";
			internal const string TotalVATDue = "TVD";
			internal const string OverpaidExcise = "AOP";
			internal const string UnpaidExcise = "AUP";
			internal const string CustomsValue = "CUS";
		}

		internal static class RelatedIndicator
		{
			internal const string Related = "R";
		}
		internal static class TradeStatisticsIndicator
		{
			internal const string Yes = "1";
			internal const string No = "2";
		}
	}
}
