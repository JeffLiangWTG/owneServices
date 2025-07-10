using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	internal class OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsVariableTImetableTest : OpeningHoursAndPreparationTimeAtCFSDDDCalculationStepsTest
	{
		public void TestDefault_Pickup_WorkingDay()
		{
			var defaultCollection = CreateTimetableSetting(120, MondayToFriday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Monday1.AddHours(7);
				var expected = Monday1.AddHours(10);
				var expectedLog = @"01-Sep-25 07:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 01-Sep-25 08:00:00
Processing time at PIC CFS for Monday: 2 Hours
Adding processing time step for PIC: 01-Sep-25 08:00:00 adjusted to 01-Sep-25 10:00:00 based on time table of PIC CFS
";
				ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_WorkingDay()
		{
			var defaultCollection = CreateTimetableSetting(120, MondayToFriday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Monday1.AddHours(7);
				var expected = Monday1.AddHours(10);
				var expectedLog = @"01-Sep-25 07:00:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 01-Sep-25 08:00:00
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 01-Sep-25 08:00:00 adjusted to 01-Sep-25 10:00:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_OnWeekend()
		{
			var defaultCollection = CreateTimetableSetting(240, MondayToFriday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Saturday6.AddHours(14).AddMinutes(30);
				var expected = Monday8.AddHours(12);
				var expectedLog = @"[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
06-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 08:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 08-Sep-25 08:00:00 adjusted to 08-Sep-25 12:00:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_OverWeekend()
		{
			var defaultCollection = CreateTimetableSetting(240, MondayToFriday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Friday5.AddHours(12).AddMinutes(30);
				var expected = Monday8.AddHours(10).AddMinutes(30);
				var expectedLog = @"Processing time at DLV CFS for Friday: 4 Hours
[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
Adding processing time step for DLV: 05-Sep-25 12:30:00 adjusted to 08-Sep-25 10:30:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_OverWeekend_WithPublicHoliday()
		{
			var defaultCollection = CreateTimetableSetting(120, MondayToFriday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				AddCountryHoliday(Friday5);
				var orgAddress = CreateAddressDefault();
				var arrival = Friday5.AddHours(12).AddMinutes(30);
				var expected = Monday8.AddHours(10);
				var expectedLog = @"[Header #1 YY] non working days: Friday 05-Sep-25; Saturday 06-Sep-25; Sunday 07-Sep-25; 
05-Sep-25 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 08:00:00
Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 08-Sep-25 08:00:00 adjusted to 08-Sep-25 10:00:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_OnWeekend_MondayTuesday()
		{
			var defaultCollection = CreateTimetableSetting(240, WednesdayToSunday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Monday1.AddHours(12).AddMinutes(30);
				var expected = Wednesday3.AddHours(12);
				var expectedLog = @"[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
01-Sep-25 12:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 03-Sep-25 08:00:00
Processing time at DLV CFS for Wednesday: 4 Hours
Adding processing time step for DLV: 03-Sep-25 08:00:00 adjusted to 03-Sep-25 12:00:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
			}
		}

		public void TestDefault_Delivery_OverWeekend_MondayTuesday()
		{
			var defaultCollection = CreateTimetableSetting(240, WednesdayToSunday);
			using (OrganisationRegistry.Instance.DefaultOrgTimetable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultCollection))
			{
				var orgAddress = CreateAddressDefault();
				var arrival = Sunday0.AddHours(12).AddMinutes(30);
				var expected = Wednesday3.AddHours(10).AddMinutes(30);
				var expectedLog = @"Processing time at DLV CFS for Sunday: 4 Hours
[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
Adding processing time step for DLV: 31-Aug-25 12:30:00 adjusted to 03-Sep-25 10:30:00 based on time table of DLV CFS
";
				ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
			}
		}

		public void TestWeekday_Pickup_WorkingDay()
		{
			AddCountryWeekend("SAT");
			AddCountryWeekend("SUN");

			var orgAddress = CreateAddressWeekday(120);
			var arrival = Monday1.AddHours(10);
			var expected = Monday1.AddHours(12);
			var expectedLog = @"Processing time at PIC CFS for Monday: 2 Hours
Adding processing time step for PIC: 01-Sep-25 10:00:00 adjusted to 01-Sep-25 12:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_WorkingDay()
		{
			AddCountryWeekend("SAT");
			AddCountryWeekend("SUN");

			var orgAddress = CreateAddressWeekday(120);
			var arrival = Monday1.AddHours(10);
			var expected = Monday1.AddHours(12);
			var expectedLog = @"Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 01-Sep-25 10:00:00 adjusted to 01-Sep-25 12:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_OnWeekend()
		{
			AddCountryWeekend("SAT");
			AddCountryWeekend("SUN");

			var orgAddress = CreateAddressWeekday(240);
			var arrival = Saturday6.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(14);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
06-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 10:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 08-Sep-25 10:00:00 adjusted to 08-Sep-25 14:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_OverWeekend()
		{
			AddCountryWeekend("SAT");
			AddCountryWeekend("SUN");

			var orgAddress = CreateAddressWeekday(240);
			var arrival = Friday5.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(12).AddMinutes(30);
			var expectedLog = @"Processing time at DLV CFS for Friday: 4 Hours
[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
Adding processing time step for DLV: 05-Sep-25 14:30:00 adjusted to 08-Sep-25 12:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_OverWeekend_WithPublicHoliday()
		{
			AddCountryWeekend("SAT");
			AddCountryWeekend("SUN");
			AddCountryHoliday(Friday5);

			var orgAddress = CreateAddressWeekday(240);
			var arrival = Friday5.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(14);
			var expectedLog = @"[Header #1 YY] non working days: Friday 05-Sep-25; Saturday 06-Sep-25; Sunday 07-Sep-25; 
05-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 10:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 08-Sep-25 10:00:00 adjusted to 08-Sep-25 14:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_OnWeekend_MondayTuesday()
		{
			AddCountryWeekend("MON");
			AddCountryWeekend("TUE");

			var orgAddress = CreateAddressWeekday(240);
			var arrival = Monday1.AddHours(14).AddMinutes(30);
			var expected = Wednesday3.AddHours(14);
			var expectedLog = @"[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
01-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 03-Sep-25 10:00:00
Processing time at DLV CFS for Wednesday: 4 Hours
Adding processing time step for DLV: 03-Sep-25 10:00:00 adjusted to 03-Sep-25 14:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestWeekday_Delivery_OverWeekend_MondayTuesday()
		{
			AddCountryWeekend("MON");
			AddCountryWeekend("TUE");

			var orgAddress = CreateAddressWeekday(240);
			var arrival = Sunday0.AddHours(14).AddMinutes(30);
			var expected = Wednesday3.AddHours(12).AddMinutes(30);
			var expectedLog = @"Processing time at DLV CFS for Sunday: 4 Hours
[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
Adding processing time step for DLV: 31-Aug-25 14:30:00 adjusted to 03-Sep-25 12:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestAdvanced_Pickup_WorkingDay()
		{
			var orgAddress = CreateAddressAdvanced(120, MondayToFriday);
			var arrival = Monday1.AddHours(10);
			var expected = Monday1.AddHours(12);
			var expectedLog = @"Processing time at PIC CFS for Monday: 2 Hours
Adding processing time step for PIC: 01-Sep-25 10:00:00 adjusted to 01-Sep-25 12:00:00 based on time table of PIC CFS
";
			ExecutePickupTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_WorkingDay()
		{
			var orgAddress = CreateAddressAdvanced(120, MondayToFriday);
			var arrival = Monday1.AddHours(10);
			var expected = Monday1.AddHours(12);
			var expectedLog = @"Processing time at DLV CFS for Monday: 2 Hours
Adding processing time step for DLV: 01-Sep-25 10:00:00 adjusted to 01-Sep-25 12:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_OnWeekend()
		{
			var orgAddress = CreateAddressAdvanced(240, MondayToFriday);
			var arrival = Saturday6.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(13);
			var expectedLog = @"[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
06-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 08-Sep-25 09:00:00 adjusted to 08-Sep-25 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_OverWeekend()
		{
			var orgAddress = CreateAddressAdvanced(240, MondayToFriday);
			var arrival = Friday5.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(10).AddMinutes(30);
			var expectedLog = @"Processing time at DLV CFS for Friday: 4 Hours
[Header #1 YY] non working days: Saturday 06-Sep-25; Sunday 07-Sep-25; 
Adding processing time step for DLV: 05-Sep-25 14:30:00 adjusted to 08-Sep-25 10:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_OverWeekend_WithPublicHoliday()
		{
			AddCountryHoliday(Friday5);
			var orgAddress = CreateAddressAdvanced(240, MondayToFriday);
			var arrival = Friday5.AddHours(14).AddMinutes(30);
			var expected = Monday8.AddHours(13);
			var expectedLog = @"[Header #1 YY] non working days: Friday 05-Sep-25; Saturday 06-Sep-25; Sunday 07-Sep-25; 
05-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 08-Sep-25 09:00:00
Processing time at DLV CFS for Monday: 4 Hours
Adding processing time step for DLV: 08-Sep-25 09:00:00 adjusted to 08-Sep-25 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_OnWeekend_MondayTuesday()
		{
			var orgAddress = CreateAddressAdvanced(240, WednesdayToSunday);
			var arrival = Monday1.AddHours(14).AddMinutes(30);
			var expected = Wednesday3.AddHours(13);
			var expectedLog = @"[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
01-Sep-25 14:30:00 wasn't matched with opening hours of [Header #1 YY]. Closest opening hour was: 03-Sep-25 09:00:00
Processing time at DLV CFS for Wednesday: 4 Hours
Adding processing time step for DLV: 03-Sep-25 09:00:00 adjusted to 03-Sep-25 13:00:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		public void TestAdvanced_Delivery_OverWeekend_MondayTuesday()
		{
			var orgAddress = CreateAddressAdvanced(240, WednesdayToSunday);
			var arrival = Sunday0.AddHours(14).AddMinutes(30);
			var expected = Wednesday3.AddHours(10).AddMinutes(30);
			var expectedLog = @"Processing time at DLV CFS for Sunday: 4 Hours
[Header #1 YY] non working days: Monday 01-Sep-25; Tuesday 02-Sep-25; 
Adding processing time step for DLV: 31-Aug-25 14:30:00 adjusted to 03-Sep-25 10:30:00 based on time table of DLV CFS
";
			ExecuteDeliveryTest(orgAddress, arrival, expected, isXtoCFS: false, expectedLog: expectedLog);
		}

		OrgAddress CreateAddressDefault()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			orgAddress.OA_RN_NKCountryCode = CountryCode1;
			orgAddress.State = Country1StateCode1;

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Default);

			Factory.Save();

			return orgAddress;
		}

		/// <summary>
		/// Opening times from 0800 to 1400
		/// </summary>
		DefaultOrgTimetableSettingsCollection CreateTimetableSetting(int processingTime, params string[] workingDaysOfWeek)
		{
			var defaultCollection = new DefaultOrgTimetableSettingsCollection();
			defaultCollection.AddNew();

			var defaultSetting = defaultCollection.AddNew();

			defaultSetting.CountryCode = CountryCode1;

			var openingTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 8, 0, 0);
			var closingTime = new ZDateTime(ZDateTime.Now.Year, 1, 1, 14, 0, 0);

			var types = new string[] { OrgTimetableType.Codes.Deliver, OrgTimetableType.Codes.Pickup };

			foreach (var day in workingDaysOfWeek)
			{
				foreach (var type in types)
				{
					var newItem = defaultSetting.Timetables.AddNew();
					newItem.Type = type;
					newItem.From = openingTime;
					newItem.To = closingTime;
					newItem.Day = day;
					newItem.ProcessingTimeInHours = new ZDateTime(ZDateTime.Now.Year, 1, 1, processingTime / 60, processingTime % 60, 0);
				}
			}

			Factory.Save();

			return defaultCollection;
		}

		/// <summary>
		/// Opening times from 1000 to 1600
		/// </summary>
		OrgAddress CreateAddressWeekday(int processingTime)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			orgAddress.OA_RN_NKCountryCode = CountryCode1;
			orgAddress.State = Country1StateCode1;

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();
			orgAddress.Timetables.allowDeleteLastTimeTable = false;

			orgAddress.Timetables.Add(CreateWeekdayTimeTable(orgAddress, OrgTimetableType.Codes.Deliver, processingTime));
			orgAddress.Timetables.Add(CreateWeekdayTimeTable(orgAddress, OrgTimetableType.Codes.Pickup, processingTime));
			orgAddress.Timetables.ReInitializeRangeType();

			Factory.Save();

			return orgAddress;
		}
		OrgTimetable CreateWeekdayTimeTable(OrgAddress orgAddress, string type, int processingTime)
		{
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = orgAddress.PK;
			t1.OTT_Type = type;
			t1.OTT_IsForAllWeekDays = true;
			t1.OTT_TimeFrom = new DateTime(1900, 1, 1, 10, 0, 0);
			t1.OTT_TimeTo = new DateTime(1900, 1, 1, 16, 0, 0);
			t1.OTT_ProcessingTimeInMinutes = processingTime;
			return t1;
		}

		/// <summary>
		/// Opening times from 0900 to 1700
		/// </summary>
		OrgAddress CreateAddressAdvanced(int processingTime, params string[] days)
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			// This must be set before the timetables are deleted
			orgAddress.OA_RN_NKCountryCode = CountryCode1;
			orgAddress.State = Country1StateCode1;

			orgAddress.Timetables.allowDeleteLastTimeTable = true;
			orgAddress.Timetables.DeleteAll();
			orgAddress.Timetables.allowDeleteLastTimeTable = false;

			orgAddress.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);

			foreach (var day in days)
			{
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Deliver, 9, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: processingTime);
				DeliveryDueDateCalculationTestHelper.AddTimetable(orgAddress, OrgTimetableType.Codes.Pickup, 9, 0, 17, 0, isAdvanced: true, advancedDay: day, processingTime: processingTime);
			}

			orgAddress.Timetables.ReInitializeRangeType();

			Factory.Save();

			return orgAddress;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Country1 = Factory.NewWithValidTestData<RefCountry>();
			Country1.RN_Code = CountryCode1;
			Country1State1 = Factory.NewWithValidTestData<RefCountryStates>();
			Country1State1.RW_Code = Country1StateCode1;
			Country1State1.RW_RN_NKCountryCode = CountryCode1;

			Factory.Save();
		}

		void AddCountryWeekend(string dayOfWeek, bool isWorkingDay = false, bool active = true) => AddWeekend(Country1.PK, RefCountrySchema.Constants.Prefix, dayOfWeek, active);

		//void AddStateWeekend(string dayOfWeek, bool isWorkingDay = false, bool active = true) => AddWeekend(Country1State1.PK, RefCountryStatesSchema.Constants.Prefix, dayOfWeek, active);

		void AddWeekend(ZGuid stateOrCountryPK, string tableCode, string dayOfWeek, bool active)
		{
			var weekend = Factory.NewWithValidTestData<GlbHoliday>();
			weekend.GH_IsWorkingDay = false;
			weekend.GH_Recurring = true;
			weekend.GH_ParentID = stateOrCountryPK;
			weekend.GH_ParentTableCode = tableCode;
			weekend.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Weekly;
			weekend.GH_RecurrDay = dayOfWeek;
			weekend.GH_IsActive = active;

			Factory.Save();
		}

		void AddCountryHoliday(ZDate date, bool isWorkingDay = false, bool recurring = true, bool active = true) => AddHoliday(Country1.PK, RefCountrySchema.Constants.Prefix, date, isWorkingDay, recurring, active);

		//void AddStateHoliday(ZDate date, bool isWorkingDay = false, bool recurring = true, bool active = true) => AddHoliday(Country1State1.PK, RefCountryStatesSchema.Constants.Prefix, date, isWorkingDay, recurring, active);

		void AddHoliday(ZGuid stateOrCountryPK, string tableCode, ZDate date, bool isWorkingDay, bool recurring, bool active)
		{
			var glbHoliday = Factory.NewWithValidTestData<GlbHoliday>();
			glbHoliday.GH_Date = date.ToDateTime();
			glbHoliday.GH_IsWorkingDay = isWorkingDay;
			glbHoliday.GH_Recurring = recurring;
			glbHoliday.GH_ParentID = stateOrCountryPK;
			glbHoliday.GH_ParentTableCode = tableCode;
			glbHoliday.GH_RecurrType = GlbHolidayRecurTypeCodeList.Codes.Date;
			glbHoliday.GH_IsActive = active;

			Factory.Save();
		}

		RefCountry Country1;
		RefCountryStates Country1State1;

		const string CountryCode1 = "XX";
		const string Country1StateCode1 = "YY";
		static readonly string[] MondayToFriday = { "MON", "TUE", "WED", "THU", "FRI" };
		static readonly string[] WednesdayToSunday = { "WED", "THU", "FRI", "SAT", "SUN" };

		static readonly ZDate Sunday0 = new ZDate(2025, 8, 31);
		static readonly ZDate Monday1 = new ZDate(2025, 9, 1);
		static readonly ZDate Wednesday3 = new ZDate(2025, 9, 3);
		static readonly ZDate Friday5 = new ZDate(2025, 9, 5);
		static readonly ZDate Saturday6 = new ZDate(2025, 9, 6);
		static readonly ZDate Monday8 = new ZDate(2025, 9, 8);
	}
}
