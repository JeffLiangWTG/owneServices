using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefCalendarDayTypeHelperTest : TestCaseWithFactory
	{
		static readonly CalendarDayType WorkingDay = CalendarDayType.WorkingDay;
		static readonly CalendarDayType Weekend = CalendarDayType.Weekend;
		static readonly CalendarDayType PublicHoliday = CalendarDayType.PublicHoliday;
		static readonly CalendarDayType WeekendAndPublicHoliday = CalendarDayType.WeekendAndPublicHoliday;

		#region CalendarDayType

		public void TestCalendarDayTypeWorkingDay()
		{
			var calendarDayType = WorkingDay;

			AssertEquals("IsWorkingDay", true, calendarDayType.IsWorkingDay());
			AssertEquals("IsNonWorkingDay", false, calendarDayType.IsNonWorkingDay());
			AssertEquals("IsWeekend", false, calendarDayType.IsWeekend());
			AssertEquals("IsPublicHoliday", false, calendarDayType.IsPublicHoliday());

			AssertEquals("Name", "WorkingDay", calendarDayType.Name);
		}

		public void TestCalendarDayTypeWeekend()
		{
			var calendarDayType = Weekend;

			AssertEquals("IsWorkingDay", false, calendarDayType.IsWorkingDay());
			AssertEquals("IsNonWorkingDay", true, calendarDayType.IsNonWorkingDay());
			AssertEquals("IsWeekend", true, calendarDayType.IsWeekend());
			AssertEquals("IsPublicHoliday", false, calendarDayType.IsPublicHoliday());

			AssertEquals("Name", "Weekend", calendarDayType.Name);
		}

		public void TestCalendarDayTypePublicHoliday()
		{
			var calendarDayType = PublicHoliday;

			AssertEquals("IsWorkingDay", false, calendarDayType.IsWorkingDay());
			AssertEquals("IsNonWorkingDay", true, calendarDayType.IsNonWorkingDay());
			AssertEquals("IsWeekend", false, calendarDayType.IsWeekend());
			AssertEquals("IsPublicHoliday", true, calendarDayType.IsPublicHoliday());

			AssertEquals("Name", "PublicHoliday", calendarDayType.Name);
		}

		public void TestCalendarDayTypeWeendEndAndPublicHoliday()
		{
			var calendarDayType = WeekendAndPublicHoliday;

			AssertEquals("IsWorkingDay", false, calendarDayType.IsWorkingDay());
			AssertEquals("IsNonWorkingDay", true, calendarDayType.IsNonWorkingDay());
			AssertEquals("IsWeekend", true, calendarDayType.IsWeekend());
			AssertEquals("IsPublicHoliday", true, calendarDayType.IsPublicHoliday());

			AssertEquals("Name", "WeekendAndPublicHoliday", calendarDayType.Name);
		}

		public void TestCalendarDayTypeGet()
		{
			AssertEquals("WorkingDay", WorkingDay, CalendarDayType.Get(false, false));
			AssertEquals("Weekend", Weekend, CalendarDayType.Get(true, false));
			AssertEquals("Public Holiday", PublicHoliday, CalendarDayType.Get(false, true));
			AssertEquals("Weekend and Public Holiday", WeekendAndPublicHoliday, CalendarDayType.Get(true, true));
		}

		#endregion

		#region GetDatesWithCalendarDayTypes

		public void TestGetDatesWithCalendarDayTypesFromStateHolidays()
		{
			var (_, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2000, 10, 30), false, true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday);
		}

		public void TestGetDatesWithCalendarDayTypesFromCountryHolidays()
		{
			var (country, _, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(country, new DateTime(2000, 10, 30), false, true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday);
		}

		public void TestGetDatesWithCalendarDayTypesFromCountryWeekendsWithWeekdayRange()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			address.Timetables.WeekdayRange = true;

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, Weekend, Weekend);

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Friday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);

			Factory.Save();

			result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, Weekend, Weekend, WorkingDay);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekend()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekendAndHolidayCombination()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddHoliday(state, new DateTime(2022, 10, 26), false, false);
			AddHoliday(country, new DateTime(2020, 10, 29), false, true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay, WeekendAndPublicHoliday, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekendAndHolidayCombinationIgnoreInactiveWeekend()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday, active: false);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddHoliday(state, new DateTime(2022, 10, 26), false, false);
			AddHoliday(country, new DateTime(2020, 10, 29), false, true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay, PublicHoliday, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekendAndHolidayCombinationIgnoreInactiveHoliday()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddHoliday(state, new DateTime(2022, 10, 26), false, false);
			AddHoliday(country, new DateTime(2020, 10, 29), false, true, active: false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay, Weekend, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekendRangeNotContainWeekend()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(2));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay);
		}

		public void TestGetDatesWithCalendarDayTypesFromWorkingDayException()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2000, 10, 30), false, true);

			AddHoliday(country, new DateTime(2022, 10, 30), true, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday);
		}

		public void TestGetDatesWithCalendarDayTypesFromHolidayException()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2000, 10, 30), true, true);

			AddHoliday(country, new DateTime(2022, 10, 30), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay);
		}

		public void TestGetDatesWithCalendarDayTypesFromHolidayExceptionWithStateNotApplicable()
		{
			var (country, state, _) = CreateCountryStateAndOrgAddress();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			address.OA_RN_NKCountryCode = CountryCode1;
			address.State = Country1StateCode2;

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2000, 10, 30), true, true);

			AddHoliday(country, new DateTime(2022, 10, 30), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday);
		}

		public void TestGetDatesWithCalendarDayTypesFromHolidayException_JobDocAddressWithoutOrgAddress()
		{
			var (country, state, _) = CreateCountryStateAndOrgAddress();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_RN_NKCountryCode = CountryCode1;
			jobDocAddress.E2_State = Country1StateCode1;

			AddHoliday(state, new DateTime(2000, 10, 30), true, true);

			AddHoliday(country, new DateTime(2022, 10, 30), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(jobDocAddress, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay);
		}

		public void TestGetDatesWithCalendarDayTypesFromHolidayException_JobDocAddressWithoutOrgAddressAndStateNotApplicable()
		{
			var (country, state, _) = CreateCountryStateAndOrgAddress();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_RN_NKCountryCode = CountryCode1;
			jobDocAddress.E2_State = Country1StateCode2;

			AddHoliday(state, new DateTime(2000, 10, 30), true, true);
			AddHoliday(country, new DateTime(2022, 10, 30), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(jobDocAddress, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday);
		}

		#endregion

		#region GetDatesWithCalendarDayTypes - only Holidays or only Weekends

		public void TestGetDatesWithCalendarDayTypesWithExclusion()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddHoliday(country, new DateTime(2000, 10, 29), false, true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday, Weekend);
		}

		#endregion

		#region Registry Weekend Cases

		public void TestGetDatesWithCalendarDayTypesFromRegistryDefaultNoHolidays()
		{
			var (_, _, address) = CreateCountryStateAndOrgAddress();

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, Weekend, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromRegistryDefaultWithHolidays()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			AddHoliday(state, new DateTime(2020, 10, 26), false, true);
			AddHoliday(country, new DateTime(2022, 10, 29), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay, WeekendAndPublicHoliday, Weekend);
		}

		public void TestGetDatesWithCalendarDayTypesFromRegistryCountryDefaultNoHolidays()
		{
			var (country, _, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, Weekend, Weekend, Weekend, WorkingDay, WorkingDay, WorkingDay);
			}
		}

		public void TestGetDatesWithCalendarDayTypesFromRegistryCountryDefaultWithHolidays()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			AddHoliday(state, new DateTime(2020, 10, 26), false, true);
			AddHoliday(country, new DateTime(2022, 10, 29), false, false);

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, Weekend, WeekendAndPublicHoliday, Weekend, WorkingDay, PublicHoliday, WorkingDay);
			}
		}

		public void TestGetDatesWithCalendarDayTypesCountryWeekendsOverrideRegistryCountryDefaultNoHolidays()
		{
			var (country, _, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgTimeTableCollection = new OrgTimetableCollection(address);
				orgTimeTableCollection.NotApplicable = true;

				AddWeekend(country, "TUE");
				AddWeekend(country, "SAT");

				Factory.Save();

				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, Weekend, WorkingDay, WorkingDay, WorkingDay, Weekend, WorkingDay);
			}
		}

		public void TestGetDatesWithCalendarDayTypesCountryWeekendsOverrideRegistryCountryDefaultWithHolidays()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgTimeTableCollection = new OrgTimetableCollection(address);
				orgTimeTableCollection.NotApplicable = true;

				AddHoliday(state, new DateTime(2020, 10, 26), false, true);
				AddHoliday(country, new DateTime(2022, 10, 29), false, false);

				AddWeekend(country, "TUE");
				AddWeekend(country, "SAT");

				Factory.Save();

				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, Weekend, PublicHoliday, WorkingDay, WorkingDay, WeekendAndPublicHoliday, WorkingDay);
			}
		}

		public void TestGetDatesWithCalendarDayTypesStateWeekendsOverrideRegistryCountryDefaultNoHolidays()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgTimeTableCollection = new OrgTimetableCollection(address);
				orgTimeTableCollection.NotApplicable = true;

				AddWeekend(country, "TUE");
				AddWeekend(country, "SAT");

				AddWeekend(state, "WED");
				AddWeekend(state, "SUN");

				Factory.Save();

				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, Weekend, WorkingDay, WorkingDay, WorkingDay, Weekend);
			}
		}

		public void TestGetDatesWithCalendarDayTypesStateWeekendsOverrideRegistryCountryDefaultWithHolidays()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();
			CreateTimetableSetting(defaultCollection, CountryCode1, "MON", "FRI", "SAT", "SUN");

			Factory.Save();

			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgTimeTableCollection = new OrgTimetableCollection(address);
				orgTimeTableCollection.NotApplicable = true;

				AddHoliday(state, new DateTime(2020, 10, 26), false, true);
				AddHoliday(country, new DateTime(2022, 10, 29), false, false);

				AddWeekend(country, "TUE");
				AddWeekend(country, "SAT");

				AddWeekend(state, "WED");
				AddWeekend(state, "SUN");

				Factory.Save();

				var helper = new RefCalendarDayTypeHelper();

				var fromDate = new DateTime(2022, 10, 24);
				var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

				CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WeekendAndPublicHoliday, WorkingDay, WorkingDay, PublicHoliday, Weekend);
			}
		}

		#endregion

		#region Edge Cases

		public void TestGetDatesWithCalendarDayTypesFromWeekendAndHolidayStrangeCombination()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Wednesday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Thursday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Friday);

			AddHoliday(state, new DateTime(2020, 10, 26), false, true);
			AddHoliday(country, new DateTime(2022, 10, 29), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WeekendAndPublicHoliday, Weekend, Weekend, PublicHoliday, WorkingDay);
		}

		public void TestGetDatesWithCalendarDayTypesFromWeekendAndHolidayStrangeCombinationAndNoAddress()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = ZGuid.Empty;
			jobDocAddress.E2_RN_NKCountryCode = CountryCode1;
			jobDocAddress.E2_State = Country1StateCode1;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Wednesday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Thursday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Friday);

			AddHoliday(state, new DateTime(2020, 10, 26), false, true);
			AddHoliday(country, new DateTime(2022, 10, 29), false, false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(jobDocAddress, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WeekendAndPublicHoliday, Weekend, Weekend, PublicHoliday, WorkingDay);
		}

		#endregion

		#region State/Country Holiday Testing

		public void TestStateWeekendsOverrideCountryWeekends()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddWeekend(state, AutoDayOfWeekCodeList.Codes.Tuesday);
			AddWeekend(state, AutoDayOfWeekCodeList.Codes.Friday);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, Weekend, WorkingDay, WorkingDay, Weekend, WorkingDay, WorkingDay);
		}

		public void TestEmptyStateWeekendsOverrideCountryWeekends()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddWeekend(state, AutoDayOfWeekCodeList.Codes.Tuesday, isWorkingDay: true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay);
		}

		public void TestWorkingPublicHolidaysIgnored()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Saturday);
			AddWeekend(country, AutoDayOfWeekCodeList.Codes.Sunday);

			AddHoliday(country, new DateTime(2022, 10, 26), isWorkingDay: true, recurring: false);
			AddHoliday(state, new DateTime(2022, 10, 28), isWorkingDay: true, recurring: true);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, WorkingDay, Weekend, Weekend);
		}

		#endregion

		#region Working Holidays

		public void TestCancelRecurringCountryHoliday()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();
			var state2 = CreateAlternateState();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(country, new DateTime(2020, 10, 26), isWorkingDay: false, recurring: true);
			AddHoliday(country, new DateTime(2020, 10, 27), isWorkingDay: false, recurring: true);
			AddHoliday(country, new DateTime(2020, 10, 28), isWorkingDay: false, recurring: true);

			AddHoliday(country, new DateTime(2022, 10, 26), isWorkingDay: true, recurring: false);
			AddHoliday(state, new DateTime(2022, 10, 27), isWorkingDay: true, recurring: false);
			AddHoliday(state2, new DateTime(2022, 10, 28), isWorkingDay: true, recurring: false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay); // state cancels country public holiday
		}

		public void TestCancelRecurringStateHoliday()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();
			var state2 = CreateAlternateState();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2020, 10, 26), isWorkingDay: false, recurring: true);
			AddHoliday(state, new DateTime(2020, 10, 27), isWorkingDay: false, recurring: true);
			AddHoliday(state, new DateTime(2020, 10, 28), isWorkingDay: false, recurring: true);

			AddHoliday(state, new DateTime(2022, 10, 26), isWorkingDay: true, recurring: false);
			AddHoliday(country, new DateTime(2022, 10, 27), isWorkingDay: true, recurring: false);
			AddHoliday(state2, new DateTime(2022, 10, 28), isWorkingDay: true, recurring: false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, PublicHoliday, PublicHoliday, WorkingDay, WorkingDay);
		}

		public void TestCancelNonRecurringCountryHoliday()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();
			var state2 = CreateAlternateState();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(country, new DateTime(2022, 10, 26), isWorkingDay: false, recurring: false);
			AddHoliday(country, new DateTime(2022, 10, 27), isWorkingDay: false, recurring: false);
			AddHoliday(country, new DateTime(2022, 10, 28), isWorkingDay: false, recurring: false);

			AddHoliday(country, new DateTime(2022, 10, 26), isWorkingDay: true, recurring: false);
			AddHoliday(state, new DateTime(2022, 10, 27), isWorkingDay: true, recurring: false);
			AddHoliday(state2, new DateTime(2022, 10, 28), isWorkingDay: true, recurring: false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, WorkingDay, PublicHoliday, WorkingDay, WorkingDay); // state cancels country public holiday
		}

		public void TestCancelNonRecurringStateHoliday()
		{
			var (country, state, address) = CreateCountryStateAndOrgAddress();
			var state2 = CreateAlternateState();

			var orgTimeTableCollection = new OrgTimetableCollection(address);
			orgTimeTableCollection.NotApplicable = true;

			AddHoliday(state, new DateTime(2022, 10, 26), isWorkingDay: false, recurring: false);
			AddHoliday(state, new DateTime(2022, 10, 27), isWorkingDay: false, recurring: false);
			AddHoliday(state, new DateTime(2022, 10, 28), isWorkingDay: false, recurring: false);

			AddHoliday(state, new DateTime(2022, 10, 26), isWorkingDay: true, recurring: false);
			AddHoliday(country, new DateTime(2022, 10, 27), isWorkingDay: true, recurring: false);
			AddHoliday(state2, new DateTime(2022, 10, 28), isWorkingDay: true, recurring: false);

			Factory.Save();

			var helper = new RefCalendarDayTypeHelper();

			var fromDate = new DateTime(2022, 10, 24);
			var result = helper.GetDatesWithCalendarDayTypes(address, OrgTimetableType.Codes.Pickup, fromDate, fromDate.AddDays(6));

			CheckCalendarDayTypes(fromDate, result, WorkingDay, WorkingDay, WorkingDay, PublicHoliday, PublicHoliday, WorkingDay, WorkingDay);
		}

		#endregion

		#region Implementation

		const string CountryCode1 = "XX";
		const string Country1StateCode1 = "YY";
		const string Country1StateCode2 = "ZZ";

		(RefCountry, RefCountryStates, OrgAddress) CreateCountryStateAndOrgAddress()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = CountryCode1;
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = Country1StateCode1;
			state.RW_RN_NKCountryCode = CountryCode1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			address.OA_RN_NKCountryCode = CountryCode1;
			address.State = Country1StateCode1;

			return (country, state, address);
		}

		RefCountryStates CreateAlternateState()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = Country1StateCode2;
			state.RW_RN_NKCountryCode = CountryCode1;

			return state;
		}

		void AddWeekend(RefCountry country, string dayOfWeek, bool isWorkingDay = false, bool active = true) => AddWeekend(country.PK, RefCountrySchema.Constants.Prefix, dayOfWeek, isWorkingDay, active);

		void AddWeekend(RefCountryStates state, string dayOfWeek, bool isWorkingDay = false, bool active = true) => AddWeekend(state.PK, RefCountryStatesSchema.Constants.Prefix, dayOfWeek, isWorkingDay, active);

		void AddWeekend(ZGuid stateOrCountryPK, string tableCode, string dayOfWeek, bool isWorkingDay, bool active)
		{
			var weekend = Factory.NewWithValidTestData<GlbHoliday>();
			weekend.GH_IsWorkingDay = isWorkingDay;
			weekend.GH_Recurring = true;
			weekend.GH_ParentID = stateOrCountryPK;
			weekend.GH_ParentTableCode = tableCode;
			weekend.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			weekend.GH_RecurrDay = dayOfWeek;
			weekend.GH_IsActive = active;
		}

		void AddHoliday(RefCountry country, DateTime date, bool isWorkingDay, bool recurring, bool active = true) => AddHoliday(country.PK, RefCountrySchema.Constants.Prefix, date, isWorkingDay, recurring, active);

		void AddHoliday(RefCountryStates state, DateTime date, bool isWorkingDay, bool recurring, bool active = true) => AddHoliday(state.PK, RefCountryStatesSchema.Constants.Prefix, date, isWorkingDay, recurring, active);

		void AddHoliday(ZGuid stateOrCountryPK, string tableCode, DateTime date, bool isWorkingDay, bool recurring, bool active)
		{
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = date;
			glbHoliday.GH_IsWorkingDay = isWorkingDay;
			glbHoliday.GH_Recurring = recurring;
			glbHoliday.GH_ParentID = stateOrCountryPK;
			glbHoliday.GH_ParentTableCode = tableCode;
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = active;
		}

		void CheckCalendarDayTypes(DateTime fromDate, DateWithCalendarDayType[] result, params CalendarDayType[] expectedCalendarDayTypes)
		{
			AssertEquals("Calendar day types length", expectedCalendarDayTypes.Length, result.Length);
			for (var i = 0; i < expectedCalendarDayTypes.Length; i++)
			{
				var expectedDate = fromDate.AddDays(i);
				var expectedCalendarDayType = expectedCalendarDayTypes[i];
				AssertEquals("Unexpected date", expectedDate, result[i].Date);
				if (expectedCalendarDayType != null)
				{
					AssertEquals($"Calendar day type for {expectedDate}", expectedCalendarDayType.Name, result[i].CalendarDayType.Name);
				}
			}
		}

		DefaultOrgTimetableSettings CreateTimetableSetting(DefaultOrgTimetableSettingsCollection defaultCollection, ZString countryCode, params string[] workingDaysOfWeek)
		{
			var defaultSetting = defaultCollection.AddNew();

			defaultSetting.CountryCode = countryCode;

			var openingTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 10, 0, 0);
			var closingTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0);

			var types = new string[] { OrgTimetableType.Codes.Deliver , OrgTimetableType.Codes.Pickup };

			foreach (var day in workingDaysOfWeek)
			{
				foreach (var type in types)
				{
					var newItem = defaultSetting.Timetables.AddNew();
					newItem.Type = type;
					newItem.From = openingTime;
					newItem.To = closingTime;
					newItem.Day = day;
				}
			}

			return defaultSetting;
		}

		#endregion
	}
}
