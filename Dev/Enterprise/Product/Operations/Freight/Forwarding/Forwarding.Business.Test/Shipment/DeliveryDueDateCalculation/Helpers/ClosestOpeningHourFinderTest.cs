using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ClosestOpeningHourFinderTest : TestCaseWithFactory
	{
		public void TestGetClosestOpeningHour_Default()
		{
			var monday10Am = new ZDateTime(2022, 09, 26, 10, 0, 0);
			var monday9Pm = new ZDateTime(2022, 09, 26, 21, 0, 0);
			var monday930Pm = new ZDateTime(2022, 09, 26, 21, 30, 0);
			var sunday3Pm = new ZDateTime(2022, 10, 02, 15, 0, 0);

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Default);
			AssertEquals("Monday 10AM is in opening hours", monday10Am,
				openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour);
			AssertEquals("Monday 9PM is not in opening hours, closest opening hour is 9AM next Tuesday", new ZDateTime(2022, 09, 27, 9, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(monday9Pm).ClosestOpeningHour);
			AssertEquals("Sunday 3PM is not in opening hours, closest opening hour is 9AM next Monday", new ZDateTime(2022, 10, 03, 9, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(sunday3Pm).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_Advanced()
		{
			var monday10Am = new ZDateTime(2022, 09, 26, 10, 0, 0);
			var monday9Pm = new ZDateTime(2022, 09, 26, 21, 0, 0);
			var monday930Pm = new ZDateTime(2022, 09, 26, 21, 30, 0);
			var sunday3Pm = new ZDateTime(2022, 10, 02, 15, 0, 0);

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 20, 0, 22, 0, true, "MON");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 14, 0, 16, 0, true, "SUN");

			AssertEquals("Monday 10AM is not in opening hours, closest opening hour is 8PM on the same day", monday10Am.AddHours(10),
				openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour);
			AssertEquals("Monday 9PM is in opening hours", monday9Pm,
				openingHoursHelper.GetClosestOpeningHour(monday9Pm).ClosestOpeningHour);
			AssertEquals("Sunday 3PM is in opening hours", sunday3Pm,
				openingHoursHelper.GetClosestOpeningHour(sunday3Pm).ClosestOpeningHour);

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 21, 30, 22, 0, true, "MON");
			AssertEquals("Monday 9PM is not in opening hours, closest opening hour is 9:30PM on the same day", monday9Pm.AddMinutes(30),
				openingHoursHelper.GetClosestOpeningHour(monday9Pm).ClosestOpeningHour);
			AssertEquals("Monday 9:30PM is in opening hours", monday930Pm,
				openingHoursHelper.GetClosestOpeningHour(monday930Pm).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_NotApplicable()
		{
			var monday10Am = new ZDateTime(2022, 09, 26, 10, 0, 0);
			var monday9Pm = new ZDateTime(2022, 09, 26, 21, 0, 0);
			var sunday3Pm = new ZDateTime(2022, 10, 02, 15, 0, 0);

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(new List<DateTime>());
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.NotApplicable);
			AssertEquals("Monday 10AM is in opening hours", monday10Am,
				openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour);
			AssertEquals("Monday 9PM is in opening hours", monday9Pm,
				openingHoursHelper.GetClosestOpeningHour(monday9Pm).ClosestOpeningHour);
			AssertEquals("Sunday 3PM is in opening hours", sunday3Pm,
				openingHoursHelper.GetClosestOpeningHour(sunday3Pm).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_ShouldConsiderNonWorkingDays()
		{
			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 12, 25));
			holidays.Add(new DateTime(2022, 12, 26));

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);

			#region Setup Dates

			var monday10Am = new ZDateTime(2022, 12, 23, 10, 0, 0);
			var monday9Pm = new ZDateTime(2022, 12, 23, 21, 0, 0);
			var sunday3Pm = new ZDateTime(2022, 12, 25, 10, 0, 0);

			#endregion

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgAddress.OA_State = "NSW";

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "MON");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "TUE");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "WED");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "THU");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "FRI");

			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			AssertEquals("Friday 10AM is in opening hours", monday10Am,
				openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour);
			AssertEquals("Friday 09PM is not in opening hours, closest opening hour is 9AM next Tuesday (Monday is Boxing day in Australia)", new ZDateTime(2022, 12, 27, 9, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(monday9Pm).ClosestOpeningHour);
			AssertEquals("Sunday 10AM is not in opening hours, closest opening hour is 9AM next Tuesday (Monday is Boxing day in Australia)", new ZDateTime(2022, 12, 27, 9, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(sunday3Pm).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_ShouldConsiderNonWorkingDays_WhenThereAreMoreThanOneWeekHolidayForTimeTable()
		{
			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 12, 26));
			holidays.Add(new DateTime(2022, 12, 27));
			holidays.Add(new DateTime(2023, 01, 02));

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);

			var sunday10Am = new ZDateTime(2022, 12, 25, 10, 0, 0);

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgAddress.OA_State = "NSW";

			#region Organizing TimeTable

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			orgAddress.Timetables.DeleteAll();
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "MON");
			DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 09, 0, 18, 0, true, "TUE");

			#endregion

			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			AssertEquals("Sunday 10AM is not in opening hours, closest opening hour is 9AM next next Tuesday (Monday, Tuesday and next Monday are all holidays)", new ZDateTime(2023, 01, 03, 9, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(sunday10Am).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_ShouldFallbackToNonWorkingDays_WhenThereIsNotAnyTimeTableForAddress()
		{
			var holidays = new List<DateTime>();
			holidays.Add(new DateTime(2022, 12, 26));
			holidays.Add(new DateTime(2022, 12, 27));
			holidays.Add(new DateTime(2023, 01, 02));

			var calendarDayTypeProvider = new CalendarDayTypeProviderTest(holidays);

			var friday10Am = new ZDateTime(2022, 12, 23, 10, 0, 0);
			var sunday10Am = new ZDateTime(2022, 12, 25, 10, 0, 0);
			var monday10Am = new ZDateTime(2022, 12, 26, 10, 0, 0);

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			orgAddress.OA_State = "NSW";
			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.NotApplicable);

			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			AssertEquals("Monday 10AM is not in opening hours, closest opening hour is 0AM next Wednesday (Monday and Tuesday are holiday in Australia)",
				new ZDateTime(2022, 12, 28, 0, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour);

			AssertEquals("Sunday 10AM is in opening hours",
				new ZDateTime(2022, 12, 25, 10, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(sunday10Am).ClosestOpeningHour);

			AssertEquals("Friday 10AM is in opening hours",
				new ZDateTime(2022, 12, 23, 10, 0, 0),
				openingHoursHelper.GetClosestOpeningHour(friday10Am).ClosestOpeningHour);
		}

		public void TestGetClosestOpeningHour_WithWeekdayTimetable()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Weekday);
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var country = new RefCountry.Loader(Factory).LoadForCountry(orgAddress.OA_RN_NKCountryCode);
			country.IsFridayNonWorkingDay = true;
			orgAddress.Timetables.DeleteAll();
			var timetable = orgAddress.Timetables.AddNew();
			timetable.OTT_IsForAllWeekDays = true;
			timetable.OTT_Type = OrgTimetableType.Codes.Pickup;
			timetable.OTT_TimeFrom = new DateTime(1900, 1, 1, 9, 0, 0);
			timetable.OTT_TimeTo = new DateTime(1900, 1, 1, 12, 0, 0);

			var calendarDayTypeProvider = new CalendarDayTypeProvider();
			var openingHoursHelper = new ClosestOpeningHourFinder(orgAddress, OrgTimetableType.Codes.Pickup, calendarDayTypeProvider, new ZStringBuilder());

			var monday10Am = new ZDateTime(2024, 08, 12, 10, 0, 0);
			var closestOpeningHour = openingHoursHelper.GetClosestOpeningHour(monday10Am).ClosestOpeningHour;
			AssertEquals("Monday 10AM is in opening hours", monday10Am,closestOpeningHour);
			var friday10Am = new ZDateTime(2024, 08, 16, 10, 0, 0);
			closestOpeningHour = openingHoursHelper.GetClosestOpeningHour(friday10Am).ClosestOpeningHour;
			AssertEquals("Friday 10AM is not in opening hours, closest opening hour is 9AM next Saturday", new ZDateTime(2024, 08, 17, 9, 0, 0), closestOpeningHour);
		}
	}
}
