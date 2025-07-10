using CargoWise.EntityFramework.Testing;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAlternateChartLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetBaseBalanceSheetStyleList()
		{
			var baseBalanceSheetStyleList = AccAlternateChartLookups.GetBaseBalanceSheetStyleList();

			AssertEquals("BalanceSheetStyleList.Count", 2, baseBalanceSheetStyleList.Count);

			AssertEquals("0th Element (Code)", BalanceSheetStyleCode.EAL, baseBalanceSheetStyleList[0].Code);
			AssertEquals("0th Element (Description)", "Assets - Liabilities = Shareholder Equity", baseBalanceSheetStyleList[0].Description);

			AssertEquals("1th Element (Code)", BalanceSheetStyleCode.ELA, baseBalanceSheetStyleList[1].Code);
			AssertEquals("1th Element (Description)", "Shareholder Equity + Liabilities = Assets", baseBalanceSheetStyleList[1].Description);
		}

		public void TestReportOrderList()
		{
			var reportOrderList = AccAlternateChartLookups.GetReportOrderList();

			AssertEquals("reportOrderList.Count", 2, reportOrderList.Count);

			AssertEquals("0th Element (Code)", ReportOrderCode.BTP, reportOrderList[0].Code);
			AssertEquals("0th Element (Description)", "Balance Sheet followed by Profit and Loss", reportOrderList[0].Description);

			AssertEquals("1th Element (Code)", ReportOrderCode.PTB, reportOrderList[1].Code);
			AssertEquals("1th Element (Description)", "Profit and Loss followed by Balance Sheet", reportOrderList[1].Description);
		}
	}
}
