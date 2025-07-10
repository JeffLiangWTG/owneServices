using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesAnalysisPeriodListUtilsTest : TestCaseWithFactory
	{
		[TestDate(2002, 12, 5)]
		public void TestDatesBoundForAnalysisPeriod()
		{
			var currentMonth = new ZDate(2002, 12, 1);

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2003);

			Factory.Save();

			AssertEquals(new ZDate(2002, 12, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.CurrentMonth));
			AssertEquals(new ZDate(2002, 9, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing3Months));
			AssertEquals(new ZDate(2002, 6, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing6Months));
			AssertEquals(new ZDate(2002, 3, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing9Months));
			AssertEquals(new ZDate(2001, 12, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing12Months));
			AssertEquals(new ZDate(2001, 12, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Last12Months));
			AssertEquals(ZDate.Empty, SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.TotalTradingLifetime));
			AssertEquals(new ZDate(2002, 7, 1), SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.FinancialYearToDate));

			AssertEquals(new ZDate(2003, 1, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.CurrentMonth));
			AssertEquals(new ZDate(2002, 12, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing3Months));
			AssertEquals(new ZDate(2002, 12, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing6Months));
			AssertEquals(new ZDate(2002, 12, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing9Months));
			AssertEquals(new ZDate(2002, 12, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Trailing12Months));
			AssertEquals(new ZDate(2003, 1, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.Last12Months));
			AssertEquals(ZDate.Empty, SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.TotalTradingLifetime));
			AssertEquals(new ZDate(2003, 1, 1), SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(currentMonth, SalesAnalysisPeriodList.Codes.FinancialYearToDate));
		}
	}
}
