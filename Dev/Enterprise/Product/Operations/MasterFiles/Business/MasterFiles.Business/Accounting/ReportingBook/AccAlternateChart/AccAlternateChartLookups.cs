using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateChartLookups : AutoAccAlternateChartLookups
	{
		public AccAlternateChartLookups(AutoAccAlternateChart parent) : base(parent)
		{
		}

		public static class BalanceSheetStyleCode
		{
			public const string EAL = "EAL";
			public const string ELA = "ELA";
		}

		public static class ReportOrderCode
		{
			public const string BTP = "BTP";
			public const string PTB = "PTB";
		}

		public CodeDescriptionPairList BaseBalanceSheetStyleList => GetBaseBalanceSheetStyleList();

		public static CodeDescriptionPairList GetBaseBalanceSheetStyleList()
		{
			var balanceSheetStyleList = new CodeDescriptionPairList();
			balanceSheetStyleList.AddPair(BalanceSheetStyleCode.EAL, ResString.GetMultilingualString("F9FC90E4-E1FD-4009-8434-E03BEF956C52", "Assets - Liabilities = Shareholder Equity"));
			balanceSheetStyleList.AddPair(BalanceSheetStyleCode.ELA, ResString.GetMultilingualString("3C089DF8-D884-4A03-A765-E8B877C0D492", "Shareholder Equity + Liabilities = Assets"));

			return balanceSheetStyleList;
		}

		public CodeDescriptionPairList ReportOrderList => GetReportOrderList();

		public static CodeDescriptionPairList GetReportOrderList()
		{
				var reportOrderList = new CodeDescriptionPairList();
				reportOrderList.AddPair(ReportOrderCode.BTP, ResString.GetMultilingualString("8DFF2389-7CA0-46DE-8BF3-EF0F92FBB4BB", "Balance Sheet followed by Profit and Loss"));
				reportOrderList.AddPair(ReportOrderCode.PTB, ResString.GetMultilingualString("E9040198-0A4F-4E99-9264-C33BF70E4E07", "Profit and Loss followed by Balance Sheet"));
				return reportOrderList;
		}
	}
}
