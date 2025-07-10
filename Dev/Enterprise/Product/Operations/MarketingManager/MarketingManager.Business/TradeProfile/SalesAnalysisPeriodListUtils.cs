using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public static class SalesAnalysisPeriodListUtils
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZDate GetEarliestStartDateForPeriod(ZDate firstDayOfCurrentCalendarMonth, ZString period)
		{
			switch (period)
			{
				case SalesAnalysisPeriodList.Codes.CurrentMonth:
					return firstDayOfCurrentCalendarMonth;

				case SalesAnalysisPeriodList.Codes.Trailing1Month:
					return firstDayOfCurrentCalendarMonth.AddMonths(-1);

				case SalesAnalysisPeriodList.Codes.Trailing3Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(-3);

				case SalesAnalysisPeriodList.Codes.Trailing6Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(-6);

				case SalesAnalysisPeriodList.Codes.Trailing9Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(-9);

				case SalesAnalysisPeriodList.Codes.Trailing12Months:
				case SalesAnalysisPeriodList.Codes.Last12Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(-12);

				case SalesAnalysisPeriodList.Codes.NextMonth:
				case SalesAnalysisPeriodList.Codes.Next3Months:
				case SalesAnalysisPeriodList.Codes.Next6Months:
				case SalesAnalysisPeriodList.Codes.Next12Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(1);

				case SalesAnalysisPeriodList.Codes.LastFinancialYear:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth).AddMonths(-12);

				case SalesAnalysisPeriodList.Codes.CurrentFinancialYear:
				case SalesAnalysisPeriodList.Codes.FinancialYearToDate:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth);

				case SalesAnalysisPeriodList.Codes.NextFinancialYear:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth).AddMonths(12);

				case SalesAnalysisPeriodList.Codes.TotalTradingLifetime:
					return ZDate.Empty;

				default:
					return firstDayOfCurrentCalendarMonth;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZDate GetEndDateBoundForPeriod(ZDate firstDayOfCurrentCalendarMonth, ZString period)
		{
			switch (period)
			{
				case SalesAnalysisPeriodList.Codes.CurrentMonth:
				case SalesAnalysisPeriodList.Codes.Last12Months:
				case SalesAnalysisPeriodList.Codes.FinancialYearToDate:
					return firstDayOfCurrentCalendarMonth.AddMonths(1);

				case SalesAnalysisPeriodList.Codes.NextMonth:
					return firstDayOfCurrentCalendarMonth.AddMonths(2);

				case SalesAnalysisPeriodList.Codes.Next3Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(4);

				case SalesAnalysisPeriodList.Codes.Next6Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(7);

				case SalesAnalysisPeriodList.Codes.Next12Months:
					return firstDayOfCurrentCalendarMonth.AddMonths(13);

				case SalesAnalysisPeriodList.Codes.LastFinancialYear:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth);

				case SalesAnalysisPeriodList.Codes.CurrentFinancialYear:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth).AddMonths(12);

				case SalesAnalysisPeriodList.Codes.NextFinancialYear:
					return GetFinancialYearStartDate(firstDayOfCurrentCalendarMonth).AddMonths(24);

				case SalesAnalysisPeriodList.Codes.TotalTradingLifetime:
					return ZDate.Empty;

				default:
					return firstDayOfCurrentCalendarMonth;
			}
		}

		public static CodeDescriptionPairList GetPeriodDescriptionList(SalesAnalysisPeriodList periodList)
		{
			var result = new CodeDescriptionPairList();

			var today = ZDate.Today;
			var firstDayOfCurrentCalendarMonth = new ZDate(today.Year, today.Month, 1);

			foreach (CodeDescriptionPair pair in periodList)
			{
				var periodStartDate = GetEarliestStartDateForPeriod(firstDayOfCurrentCalendarMonth, pair.Code);
				var periodEndDate = GetEndDateBoundForPeriod(firstDayOfCurrentCalendarMonth, pair.Code);

				string periodDateText;
				if (!periodStartDate.IsEmpty && !periodEndDate.IsEmpty)
				{
					var periodStartDateText = periodStartDate.ToShortDateString();
					var periodEndDateText = periodEndDate.AddDays(-1).ToShortDateString();
					periodDateText = Res.GetString("aaf631a7-e6ec-440e-9e07-cea733ab2ee2", "{0} to {1}", periodStartDateText, periodEndDateText);
				}
				else
				{
					periodDateText = Res.GetString("44f71e63-f07f-446d-8a98-931709712a6f", "Any date");
				}

				result.AddPair(pair.MultilingualDescription, periodDateText);
			}

			return result;
		}

		static ZDate GetFinancialYearStartDate(ZDate firstDayOfCurrentCalendarMonth)
		{
			var result = ZDate.Empty;
			var periodCalculator = new AccountingPeriodCalculator(new BusinessObjectFactory());
			var currentFinancialPeriod = periodCalculator.GetPeriodFromDate(firstDayOfCurrentCalendarMonth);
			if (currentFinancialPeriod > 0)
			{
				result = firstDayOfCurrentCalendarMonth.AddMonths(1 - currentFinancialPeriod % 100);
			}
			else
			{
				result = firstDayOfCurrentCalendarMonth;
			}
			return result;
		}
	}
}
