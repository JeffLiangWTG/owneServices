using System;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(OnlineSchedulesModuleDateFilter))]
	public class OnlineSchedulesModuleDateFilterTest : ModuleDateFilterTest
	{
		public void TestSwitchToDateRange()
		{
			Filter.PropertySearch = "";
			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(ZDateTime.Today, Filter.Property1.Date);
		}

		public void TestCheckPropertyValues()
		{
			var today = ZDateTime.Today;
			var tomorrow = today.AddDays(1);
			var firstDayOfThisWeek = today.AddDays(-(int)today.DayOfWeek);
			var firstDayOfNextWeek = firstDayOfThisWeek.AddDays(7);
			var firstDayOfNextCalendarMonth = today.AddDays(-today.Day + 1).AddMonths(1);

			AssertPropertyValues(today, today, ModuleDateFilter.DateRangeSearchTexts.Today);
			AssertPropertyValues(tomorrow, tomorrow, ModuleDateFilter.DateRangeSearchTexts.Tomorrow);
			AssertPropertyValues(firstDayOfThisWeek, firstDayOfThisWeek.AddDays(6), ModuleDateFilter.DateRangeSearchTexts.ThisWeek);
			AssertPropertyValues(firstDayOfNextWeek, firstDayOfNextWeek.AddDays(6), ModuleDateFilter.DateRangeSearchTexts.NextWeek);
			AssertPropertyValues(today, today.AddDays(6), ModuleDateFilter.DateRangeSearchTexts.Next7Days);
			AssertPropertyValues(today, today.AddDays(13), ModuleDateFilter.DateRangeSearchTexts.Next14Days);
			AssertPropertyValues(today, today.AddMonths(1), ModuleDateFilter.DateRangeSearchTexts.NextMonth);
			AssertPropertyValues(firstDayOfNextCalendarMonth, firstDayOfNextCalendarMonth.AddMonths(1).AddDays(-1), ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth);
			AssertPropertyValues(today, today.AddMonths(2), ModuleDateFilter.DateRangeSearchTexts.Next2Mths);
			AssertPropertyValues(today, today.AddMonths(3), ModuleDateFilter.DateRangeSearchTexts.Next3Mths);
			AssertPropertyValues(today, today.AddMonths(6), ModuleDateFilter.DateRangeSearchTexts.Next6Mths);
			AssertPropertyValues(today, today.AddMonths(12), ModuleDateFilter.DateRangeSearchTexts.Next12Mths);

			Filter.PropertySearch = ModuleDateFilter.Future;
			AssertEquals(today, Filter.Property1.Date);
			AssertEquals(true, Filter.Property2.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Filter.Property1 = today.AddDays(1);
			Filter.Property2 = today.AddDays(10);
			AssertEquals(today.AddDays(1), Filter.Property1.Date);
			AssertEquals(today.AddDays(10), Filter.Property2.Date);
		}

		void AssertPropertyValues(ZDateTime property1, ZDateTime property2, ZString propertySearch)
		{
			Filter.PropertySearch = propertySearch;
			AssertEquals(property1, Filter.Property1.Date);
			AssertEquals(property2, Filter.Property2.Date);
		}

		#region override

		public override void TestIsEmpty()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.ThisWeek;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextWeek;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next14Days;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextMonth;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next2Mths;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next3Mths;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next6Mths;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next12Mths;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.Future;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.Future;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = "crap data";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(false, Filter.IsEmpty);
		}

		public override void TestFromDate_RemoveTimeComponentWhenIsPropertySearchUsingSpecifiedDateRange()
		{
			var testFromDate = new ZDateTime(2014, 02, 13, 12, 34, 56);
			Filter.Property1 = testFromDate;

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(true, Filter.IsPropertySearchUsingSpecifiedDateRange);
			AssertEquals(false, Filter.IsPropertySearchUsingSpecifiedDateTimeRange);
			AssertEquals(testFromDate.Date, Filter.Property1.Date);
		}

		public override void TestPropertySearchUsesHideFutureDatesFlag()
		{
			Assert(true);
		}

		public override void TestQueryUsingDateTimeRange()
		{
			Assert(true);
		}

		public override void TestQueryUsingDateTimeRangeConvertToUTC()
		{
			Assert(true);
		}

		public override void TestQueryUsingHasDateEntered()
		{
			Assert(true);
		}

		public override void TestQueryUsingHasNoDateEntered()
		{
			Assert(true);
		}

		public override void TestQueryUsingHasNoDateEnteredUsingDataView()
		{
			Assert(true);
		}

		[TestDate(2007, 1, 1)]
		public override void TestQueryWithCommonDates()
		{
			var today = TestDateAttribute.Date;
			var tomorrow = today.AddDays(1);
			var sevenDaysFromNow = today.AddDays(6);
			var fourteenDaysFromNow = today.AddDays(13);
			var oneMonthFromNow = today.AddMonths(1);
			var twoMonthsFromNow = today.AddMonths(2);
			var threeMonthsFromNow = today.AddMonths(3);
			var sixMonthsFromNow = today.AddMonths(6);
			var oneYearFromNow = today.AddYears(1);

			AssertResultsUsingCurrentFilter(today, tomorrow, ModuleDateFilter.DateRangeSearchTexts.Today);
			AssertResultsUsingCurrentFilter(tomorrow, today, ModuleDateFilter.DateRangeSearchTexts.Tomorrow);
			AssertResultsUsingCurrentFilter(sevenDaysFromNow, sevenDaysFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next7Days);
			AssertResultsUsingCurrentFilter(fourteenDaysFromNow, fourteenDaysFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next14Days);
			AssertResultsUsingCurrentFilter(oneMonthFromNow, oneMonthFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.NextMonth);
			AssertResultsUsingCurrentFilter(twoMonthsFromNow, twoMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next2Mths);
			AssertResultsUsingCurrentFilter(threeMonthsFromNow, threeMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next3Mths);
			AssertResultsUsingCurrentFilter(sixMonthsFromNow, sixMonthsFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next6Mths);
			AssertResultsUsingCurrentFilter(oneYearFromNow, oneYearFromNow.AddDays(1), ModuleDateFilter.DateRangeSearchTexts.Next12Mths);
		}

		[TestDate(2007, 1, 1)] // Monday
		public override void TestQueryWithWeekAndMonthDateRanges()
		{
			// this week / month
			var firstDayOfThisWeek = new ZDateTime(2006, 12, 31); // Sunday
			var lastDayOfThisWeek = new ZDateTime(2007, 1, 6); // Saturday

			var firstDayOfNextWeek = new ZDateTime(2007, 1, 7); // Sunday
			var lastDayOfNextWeek = new ZDateTime(2007, 1, 13); // Saturday

			AssertResultsUsingCurrentFilter(firstDayOfThisWeek, firstDayOfThisWeek.AddDays(-1), "This Week");
			AssertResultsUsingCurrentFilter(lastDayOfThisWeek, lastDayOfThisWeek.AddDays(1), "This Week");

			AssertResultsUsingCurrentFilter(firstDayOfNextWeek, firstDayOfNextWeek.AddDays(-1), "Next Week");
			AssertResultsUsingCurrentFilter(lastDayOfNextWeek, lastDayOfNextWeek.AddDays(1), "Next Week");
		}

		[TestDate(2007, 1, 1, 1, 1, 1)]
		public override void TestTodayFilter()
		{
			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Filter.IsActive = true;

			DateTime expectedFrom = new DateTime(2007, 1, 1, 0, 0, 0);
			DateTime expectedTo = new DateTime(2007, 1, 2, 0, 0, 0, 0);

			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFrom) + "# and Z0_Date < #" + ConvertExpectedDate(expectedTo) + "#", Filter.Query.LiteralTextADO);

			Filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Tomorrow;
			Filter.IsActive = true;

			expectedFrom = new DateTime(2007, 1, 2, 0, 0, 0);
			expectedTo = new DateTime(2007, 1, 3, 0, 0, 0, 0);

			AssertEquals("Z0_Date >= #" + ConvertExpectedDate(expectedFrom) + "# and Z0_Date < #" + ConvertExpectedDate(expectedTo) + "#", Filter.Query.LiteralTextADO);
		}

		protected override ImmutableArray<string> ExpectedPropertySearchListCore
		{
			get
			{
				return ImmutableArray.Create<string>(
					ModuleDateFilter.DateRangeSearchTexts.Today,
					ModuleDateFilter.DateRangeSearchTexts.Tomorrow,
					ModuleDateFilter.DateRangeSearchTexts.ThisWeek,
					ModuleDateFilter.DateRangeSearchTexts.NextWeek,
					ModuleDateFilter.DateRangeSearchTexts.Next7Days,
					ModuleDateFilter.DateRangeSearchTexts.Next14Days,
					ModuleDateFilter.DateRangeSearchTexts.NextMonth,
					ModuleDateFilter.DateRangeSearchTexts.NextCalendarMonth,
					ModuleDateFilter.DateRangeSearchTexts.Next2Mths,
					ModuleDateFilter.DateRangeSearchTexts.Next3Mths,
					ModuleDateFilter.DateRangeSearchTexts.Next6Mths,
					ModuleDateFilter.DateRangeSearchTexts.Next12Mths,
					ModuleDateFilter.Future,
					ModuleDateFilter.SpecifiedDateRange
				);
			}
		}

		protected override ModuleDateFilter GetNewModuleFilter()
		{
			return new OnlineSchedulesModuleDateFilter("moo", DummyBizoSchema.Z0_Date);
		}

		#endregion
	}
}
