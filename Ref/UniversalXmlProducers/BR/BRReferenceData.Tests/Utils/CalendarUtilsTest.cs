using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BRReferenceData.Tests
{
	[TestFixture]
	class CalendarUtilsTest
	{
		[Test]
		public void TestHolidaysCount()
		{
			var holidays = CalendarUtils.GetHolidays(year);
			Assert.AreEqual(holidaysOf2021.Count, holidays.Count);
		}

		[Test]
		public void TestHolidaysDates()
		{
			var holidays = CalendarUtils.GetHolidays(year);
			for (int count = 0; count < holidaysOf2021.Count; count++)
			{
				Assert.AreEqual(holidaysOf2021[count], holidays[count]);
			}
		}

		[Test]
		public void CheckLastButOneWorkDay()
		{
			var expectedDate = new DateTime(2021, 7, 23);
			var outDate = CalendarUtils.GetBRLastButOneWorkDay(new DateTime(2021, 7, 26));

			Assert.AreEqual(expectedDate, outDate);
		}

		[Test]
		public void CheckLastButOneWorkDayPassingThroughHoliday()
		{
			var expectedDate = new DateTime(2020, 12, 31);
			var outDate = CalendarUtils.GetBRLastButOneWorkDay(new DateTime(2021, 1, 2));

			Assert.AreEqual(expectedDate, outDate);
		}

		[Test]
		public void CheckLastButOneWorkDayStartingFromHoliday()
		{
			var expectedDate = new DateTime(2021, 2, 12);
			var outDate = CalendarUtils.GetBRLastButOneWorkDay(new DateTime(2021, 2, 16));

			Assert.AreEqual(expectedDate, outDate);
		}

		[TestCaseSource(typeof(DateCases), nameof(DateCases.NextWorkDates))]
		public void TestGetBRNextWorkDay(DateTime inputDate, DateTime expectedDate)
		{
			Assert.AreEqual(expectedDate, CalendarUtils.GetBRNextWorkDay(inputDate));
		}

		[SetUp]
		public void SetUp()
		{
			holidaysOf2021 = new List<DateTime>()
			{
				new DateTime(year, 1, 1),
				new DateTime(year, 2, 15),
				new DateTime(year, 2, 16),
				new DateTime(year, 4, 21),
				new DateTime(year, 5, 1),
				new DateTime(year, 6, 3),
				new DateTime(year, 9, 7),
				new DateTime(year, 10, 12),
				new DateTime(year, 11, 2),
				new DateTime(year, 11, 15),
				new DateTime(year, 11, 20),
				new DateTime(year, 12, 25)
			};
		}

		List<DateTime> holidaysOf2021;
		readonly int year = 2021;

		class DateCases
		{
			public static object[] NextWorkDates =
			{
				new object[] { new DateTime(2022, 6, 8), new DateTime(2022, 6, 9) },//Next day during normal week
				new object[] { new DateTime(2022, 6, 10), new DateTime(2022, 6, 13) },//Next day after weekend
				new object[] { new DateTime(2022, 9, 6), new DateTime(2022, 9, 8) },//Next day after holiday in the middle of week
				new object[] { new DateTime(2020, 12, 31), new DateTime(2021, 1, 4) },//Next day after holiday and then weekend
				new object[] { new DateTime(2021, 11, 12), new DateTime(2021, 11, 16) }//Next day after weekend and then holiday
			};
		}
	}
}
