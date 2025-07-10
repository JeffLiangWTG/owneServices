using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CalendarArithmeticDataSourceTest : TestCaseWithFactory
	{
		#region GetBranchHolidayDatesByYear

		public void TestGetBranchHolidayDatesByYear()
		{
			CreateHolidays();
			WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, new DateTimeRange(new DateTime(2000, 1, 1), new DateTime(2020, 2, 2)));

			var actualHolidays = CalendarDataSource.GetBranchHolidayDatesByYear(2017);
			AssertContainsExactElementsInAnyOrder("Should have only the 2017 holidays", Holidays2017, actualHolidays);

			actualHolidays = CalendarDataSource.GetBranchHolidayDatesByYear(2016);
			AssertContainsExactElementsInAnyOrder("Should have only the 2016 holidays", Holidays2016, actualHolidays);

			actualHolidays = CalendarDataSource.GetBranchHolidayDatesByYear(2000);
			AssertContainsExactElementsInAnyOrder("Should have only the 2000 holidays", Holidays2000, actualHolidays);
		}

		static IEnumerable<DateTime> Holidays2017 => new[]
		{
			new DateTime(2017, 01, 01), // NYD
			new DateTime(2017, 04, 14), // Good Friday
			new DateTime(2017, 04, 17), // Easter Monday
			new DateTime(2017, 10, 02), // Labour Day
		};

		static IEnumerable<DateTime> Holidays2016 => new[]
		{
			new DateTime(2016, 01, 01), // NYD
			new DateTime(2016, 03, 25), // Good Friday
			new DateTime(2016, 03, 28), // Easter Monday
			new DateTime(2016, 10, 03), // Labour Day
		};

		static IEnumerable<DateTime> Holidays2000 => new[]
		{
			new DateTime(2000, 01, 01), // NYD
			new DateTime(2000, 01, 02), // Public holiday
			new DateTime(2000, 04, 21), // Good Friday
			new DateTime(2000, 04, 24), // Easter Monday
			new DateTime(2000, 10, 02), // Labour Day
		};

		#endregion

		#region GetStaffHolidaysFromRange

		public void TestGetStaffHolidaysFromRange_HolidayFullyWithinPeriod_ShouldGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 11), new DateTime(2017, 07, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range).ToArray();

			AssertEquals(holidayRange.Start, actualHolidays.Single().Start);
			AssertEquals(holidayRange.End, actualHolidays.Single().End);
		}

		public void TestGetStaffHolidaysFromRange_HolidayPartiallyWithinPeriod_ShouldGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 01), new DateTime(2017, 07, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range).ToArray();

			AssertEquals(holidayRange.Start, actualHolidays.Single().Start);
			AssertEquals(holidayRange.End, actualHolidays.Single().End);
		}

		public void TestGetStaffHolidaysFromRange_OutsidePeriod_ShouldNotGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 08, 01), new DateTime(2017, 08, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range);

			AssertEquals(0, actualHolidays.Count());
		}

		public void TestGetStaffHolidaysFromRange_HolidayNotApproved_ShouldNotGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 14), new DateTime(2017, 08, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange, approvalCode: StaffHolidayApprovalCodes.Requested);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range);

			AssertEquals(0, actualHolidays.Count());
		}

		public void TestGetStaffHolidaysFromRange_AvailabilityPercentageNot0_ShouldNotGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 14), new DateTime(2017, 08, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange, availabilityPercentage: 50);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range);

			AssertEquals(0, actualHolidays.Count());
		}

		public void TestGetStaffHolidaysFromRange_WorkingAway_ShouldNotGetHoliday()
		{
			var range = new DateTimeRange(new DateTime(2017, 07, 10), new DateTime(2017, 07, 23));
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 14), new DateTime(2017, 08, 12));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange, workingAway: true);

			var actualHolidays = CalendarDataSource.GetStaffHolidaysFromRange(range);

			AssertEquals(0, actualHolidays.Count());
		}

		#endregion

		#region GetStaffHolidayForDateTime

		public void TestGetStaffHolidayForDateTime()
		{
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 24, 14, 30, 00), new DateTime(2017, 07, 26, 10, 00, 00));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var dateTimeToCheck = new DateTime(2017, 07, 24, 14, 30, 00);
			var actualHoliday = CalendarDataSource.GetStaffHolidayForDateTime(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range", holidayRange.End, actualHoliday.End);

			dateTimeToCheck = new DateTime(2017, 07, 26, 10, 00, 00);
			actualHoliday = CalendarDataSource.GetStaffHolidayForDateTime(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range, on the end edge", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range, on the end edge", holidayRange.End, actualHoliday.End);

			dateTimeToCheck = new DateTime(2017, 07, 26, 10, 01, 00);
			actualHoliday = CalendarDataSource.GetStaffHolidayForDateTime(dateTimeToCheck);
			AssertEquals("Should not get holiday as time is after the holiday end", null, actualHoliday);
		}

		public void TestGetStaffHolidayForDateTime_InvalidStaffPK()
		{
			var dateTimeToCheck = new DateTime(2017, 07, 24, 14, 30, 00);
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 24, 14, 30, 00), new DateTime(2017, 07, 26, 10, 00, 00));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var actualHoliday = CalendarDataSource_NoStaff.GetStaffHolidayForDateTime(dateTimeToCheck);
			AssertEquals("Should not get anything as no staff was set", null, actualHoliday);

			actualHoliday = CalendarDataSource.GetStaffHolidayForDateTime(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range", holidayRange.End, actualHoliday.End);
		}

		#endregion

		#region GetStaffHolidayForDay

		public void TestGetStaffHolidayForDay()
		{
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 24), new DateTime(2017, 07, 26));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var dateTimeToCheck = new DateTime(2017, 07, 24);
			var actualHoliday = CalendarDataSource.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range", holidayRange.End, actualHoliday.End);

			dateTimeToCheck = new DateTime(2017, 07, 25);
			actualHoliday = CalendarDataSource.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range, on the end edge", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range, on the end edge", holidayRange.End, actualHoliday.End);

			dateTimeToCheck = new DateTime(2017, 07, 26);
			actualHoliday = CalendarDataSource.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should not get holiday as time is on the holiday end edge", null, actualHoliday);

			dateTimeToCheck = new DateTime(2017, 07, 30);
			actualHoliday = CalendarDataSource.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should not get holiday as time is after the holiday end", null, actualHoliday);
		}

		public void TestGetStaffHolidayForDay_InvalidStaffPK()
		{
			var dateTimeToCheck = new DateTime(2017, 07, 24);
			var holidayRange = new DateTimeRange(new DateTime(2017, 07, 24), new DateTime(2017, 07, 26));
			var staffHoliday = WorkingDaysTestHelper.CreateStaffHoliday(Factory, staffPK, holidayRange);

			var actualHoliday = CalendarDataSource_NoStaff.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should not get anything as no staff was set", null, actualHoliday);

			actualHoliday = CalendarDataSource.GetStaffHolidayForDay(dateTimeToCheck);
			AssertEquals("Should get holiday start, when dateTime within Holiday Range", holidayRange.Start, actualHoliday.Start);
			AssertEquals("Should get holiday end, when dateTime within Holiday Range", holidayRange.End, actualHoliday.End);
		}

		#endregion

		#region StaffWorkTimeWeek

		public void TestStaffWorkTimeWeek_NormalWeek()
		{
			var workTimeWeekToUse = WorkingDaysTestHelper.GetDefaultWorkWeek();
			WorkingDaysTestHelper.CreateWorkTime(Factory, staffPK, GlbStaffSchema.Constants.Prefix, workTimeWeekToUse);
			var expectedWeek = new WorkTimeWeek(workTimeWeekToUse);

			var actualWeek = CalendarDataSource.StaffWorkTimeWeek;
			AssertWorkTimeWeekEquality(actualWeek, expectedWeek);
		}

		public void TestStaffWorkTimeWeek_WedSunWeek()
		{
			var workTimeWeekToUse = WorkingDaysTestHelper.GetWedSunWorkWeek();
			WorkingDaysTestHelper.CreateWorkTime(Factory, staffPK, GlbStaffSchema.Constants.Prefix, workTimeWeekToUse);
			var expectedWeek = new WorkTimeWeek(workTimeWeekToUse);

			var actualWeek = CalendarDataSource.StaffWorkTimeWeek;
			AssertWorkTimeWeekEquality(actualWeek, expectedWeek);
		}

		#endregion

		#region DepartmentWorkTimeWeek

		public void TestDepartmentWorkTimeWeek_NormalWeek()
		{
			var workTimeWeekToUse = WorkingDaysTestHelper.GetDefaultWorkWeek();
			WorkingDaysTestHelper.CreateWorkTime(Factory, deptPK, GlbDepartmentSchema.Constants.Prefix, workTimeWeekToUse);
			var expectedWeek = new WorkTimeWeek(workTimeWeekToUse);

			var actualWeek = CalendarDataSource.DepartmentWorkTimeWeek;
			AssertWorkTimeWeekEquality(actualWeek, expectedWeek);
		}

		public void TestDepartmentWorkTimeWeek_WedSunWeek()
		{
			var workTimeWeekToUse = WorkingDaysTestHelper.GetWedSunWorkWeek();
			WorkingDaysTestHelper.CreateWorkTime(Factory, deptPK, GlbDepartmentSchema.Constants.Prefix, workTimeWeekToUse);
			var expectedWeek = new WorkTimeWeek(workTimeWeekToUse);

			var actualWeek = CalendarDataSource.DepartmentWorkTimeWeek;
			AssertWorkTimeWeekEquality(actualWeek, expectedWeek);
		}

		#endregion

		#region Implementation

		ZGuid deptPK;
		ZGuid staffPK;
		ZGuid branchPK;

		protected override void SetUp()
		{
			base.SetUp();
			deptPK = EnvProxy.Instance.CurrentDepartment.PK;
			branchPK = EnvProxy.Instance.CurrentBranch.PK;
			staffPK = EnvProxy.Instance.CurrentUser.PK;
		}

		void CreateHolidays()
		{
			WorkingDaysTestHelper.CreateHoliday(Factory, branchPK, new DateTime(2017, 1, 1), "New Years Day 2017");
			WorkingDaysTestHelper.CreateHoliday(Factory, branchPK, new DateTime(2016, 1, 1), "New Years Day 2016");
			WorkingDaysTestHelper.SetBranchHolidays(Factory, branchPK);
		}

		ICalendarDataSource CalendarDataSource => new CalendarArithmeticDataSource(Factory, deptPK, branchPK, staffPK);

		ICalendarDataSource CalendarDataSource_NoStaff => new CalendarArithmeticDataSource(Factory, deptPK, branchPK);

		void AssertWorkTimeWeekEquality(WorkTimeWeek actualWeek, WorkTimeWeek expectedWeek)
		{
			foreach (var day in Enum.GetValues(typeof(DayOfWeek)).OfType<DayOfWeek>())
			{
				var actualDay = actualWeek.GetDay(day);
				var expectedDay = expectedWeek.GetDay(day);

				AssertEquals("Hours in day should equal", expectedDay.Hours.Length, actualDay.Hours.Length);
				for (var i = 0; i < actualDay.Hours.Length; ++i)
				{
					AssertEquals($"Actual hour at interval {i} should equal in both arrays", expectedDay.Hours[i], actualDay.Hours[i]);
				}
			}
		}

		#endregion
	}
}
