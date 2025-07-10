using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class RunSheetDayExtensionsTest : TestCase
	{
		#region TestGetDate

		[TestDate(2013, 3, 8)]
		public void TestGetDate()
		{
			var today = ZDate.Today; // today is a Friday

			AssertEquals(ZDate.Today.AddDays(-1), RunSheetDay.Yesterday.GetDate());
			AssertEquals(ZDate.Today.AddDays(0), RunSheetDay.Today.GetDate());
			AssertEquals(ZDate.Today.AddDays(1), RunSheetDay.Tomorrow.GetDate());

			AssertEquals(ZDate.Today.AddDays(2), RunSheetDay.Sunday.GetDate());
			AssertEquals(ZDate.Today.AddDays(3), RunSheetDay.Monday.GetDate());
			AssertEquals(ZDate.Today.AddDays(4), RunSheetDay.Tuesday.GetDate());
			AssertEquals(ZDate.Today.AddDays(5), RunSheetDay.Wednesday.GetDate());
			AssertEquals(ZDate.Today.AddDays(6), RunSheetDay.Thursday.GetDate());
			AssertEquals(ZDate.Today.AddDays(7), RunSheetDay.Friday.GetDate());
			AssertEquals(ZDate.Today.AddDays(8), RunSheetDay.Saturday.GetDate());

			AssertExceptionThrown(typeof(ArgumentException), "GetDate() should not be called on CustomDate.", () => RunSheetDay.CustomDate.GetDate());
		}

		#endregion

		#region TestGetDescription

		public void TestGetDescription()
		{
			AssertEquals("Yesterday", RunSheetDay.Yesterday.GetDescription().Caption);
			AssertEquals("Today", RunSheetDay.Today.GetDescription().Caption);
			AssertEquals("Tomorrow", RunSheetDay.Tomorrow.GetDescription().Caption);

			AssertEquals("Sunday", RunSheetDay.Sunday.GetDescription().Caption);
			AssertEquals("Monday", RunSheetDay.Monday.GetDescription().Caption);
			AssertEquals("Tuesday", RunSheetDay.Tuesday.GetDescription().Caption);
			AssertEquals("Wednesday", RunSheetDay.Wednesday.GetDescription().Caption);
			AssertEquals("Thursday", RunSheetDay.Thursday.GetDescription().Caption);
			AssertEquals("Friday", RunSheetDay.Friday.GetDescription().Caption);
			AssertEquals("Saturday", RunSheetDay.Saturday.GetDescription().Caption);

			AssertEquals("Custom Date", RunSheetDay.CustomDate.GetDescription().Caption);
		}

		#endregion

		#region TestGetNextSevenDaysAfterTomorrow

		[TestDate(2013, 3, 8)]
		public void TestGetNextSevenDaysAfterTomorrow()
		{
			// today is a Friday
			AssertArrayEqualsByElements(new[]
			{
				RunSheetDay.Sunday,
				RunSheetDay.Monday,
				RunSheetDay.Tuesday,
				RunSheetDay.Wednesday,
				RunSheetDay.Thursday,
				RunSheetDay.Friday,
				RunSheetDay.Saturday
			},
			RunSheetDayExtensions.GetNextSevenDaysAfterTomorrow());
		}

		#endregion

		#region TestGetYesterdayTodayTomorrow

		public void TestGetYesterdayTodayTomorrow()
		{
			AssertArrayEqualsByElements(new[]
			{
				RunSheetDay.Yesterday,
				RunSheetDay.Today,
				RunSheetDay.Tomorrow
			},
			RunSheetDayExtensions.GetYesterdayTodayTomorrow());
		}

		#endregion

		#region TestIsCustomDate

		public void TestIsCustomDate()
		{
			foreach (RunSheetDay day in Enum.GetValues(typeof(RunSheetDay)))
			{
				AssertEquals(day == RunSheetDay.CustomDate, day.IsCustomDate());
			}
		}

		#endregion
	}
}
