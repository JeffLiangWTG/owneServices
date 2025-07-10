using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTimeZoneRule))]
	sealed class RefTimeZoneRuleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Rule.R4_DaylightSavingDate = ZDateTime.Now;
			return Rule;
		}

		#region Properties

		#region DayNumber

		public void TestDayNumber()
		{
			Rule.R4_DaylightSavingDayCount = ZByte.Zero;
			Rule.DayNumber = "3";
			AssertEquals((ZByte)3, Rule.R4_DaylightSavingDayCount);

			Rule.R4_DaylightSavingDayCount = ZByte.Zero;
			Rule.DayNumber = "DF";
			AssertEquals(ZByte.Zero, Rule.R4_DaylightSavingDayCount);
		}

		#endregion

		#region DateDayNumber

		public void TestDayNumberInvalidDayInput()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DateDayNumber = "-2";
			AssertHasErrors("DateDayNumber must be between 0 and 32", Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "36";
			AssertHasErrors("DateDayNumber must be between 0 and 32", Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "1";
			AssertNoErrors("If month is not set, any date should be able to be selected", Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "31";
			AssertNoErrors("If month is not set, any date should be able to be selected", Rule.DateDayNumberInfo);
		}

		public void TestDayNumberWhenDatabaseDateFieldEmptyOrInvalid()
		{
			Rule.R4_DaylightSavingDate = ZDateTime.Empty;

			Rule.DateDayNumber = "3";
			AssertEquals("Year value for R4_DaylightSavingDate should be set to the current year", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Month value for R4_DaylightSavingDate should be 1", 1, Rule.R4_DaylightSavingDate.Month);
			AssertEquals("Hour value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Hour);
			AssertEquals("Minute value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Minute);
			AssertEquals("Second value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Second);

			Rule.R4_DaylightSavingDate = new ZDateTime(2006, 07, 07, 12, 12, 12);
			Rule.DateDayNumber = "20";
			AssertEquals("Year value for R4_DaylightSavingDate should not have changed when the day was changed", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Month value for R4_DaylightSavingDate should not have changed when the day was changed", 7, Rule.R4_DaylightSavingDate.Month);
			AssertEquals("Hour value for R4_DaylightSavingDate should not have changed when the day was changed", 12, Rule.R4_DaylightSavingDate.Hour);
			AssertEquals("Minute value for R4_DaylightSavingDate not have changed when the day was changed", 12, Rule.R4_DaylightSavingDate.Minute);
			AssertEquals("Second value for R4_DaylightSavingDate not have changed when the day was changed", 12, Rule.R4_DaylightSavingDate.Second);
		}

		#endregion

		#region DateMonth

		public void TestDateMonthInvalidInput()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DateMonth = "ABC";
			AssertHasErrors("DateMonth must be a code for a month", Rule.DateMonthInfo);

			Rule.DateMonth = "195";
			AssertHasErrors("DateMonth must be a code for a month", Rule.DateMonthInfo);

			Rule.DateMonth = "JAN";
			AssertNoErrors("JAN is the month code for January and is valid", Rule.DateMonthInfo);
		}

		public void TestDateMonthWhenDatabaseDateFieldEmptyOrInvalid()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.R4_DaylightSavingDate = ZDateTime.Empty;
			Rule.DateMonth = "JAN";
			AssertEquals("Year value for R4_DaylightSavingDate should be set to the current year", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Day value for R4_DaylightSavingDate should be 1", 1, Rule.R4_DaylightSavingDate.Day);
			AssertEquals("Hour value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Hour);
			AssertEquals("Minute value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Minute);
			AssertEquals("Second value for R4_DaylightSavingDate should be 0", 0, Rule.R4_DaylightSavingDate.Second);

			Rule.R4_DaylightSavingDate = new ZDateTime(2006, 07, 07, 12, 12, 12);
			Rule.DateMonth = "JUL";
			AssertEquals("Year value for R4_DaylightSavingDate should not have changed when the month was changed", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Day value for R4_DaylightSavingDate should not have changed when the month was changed", 7, Rule.R4_DaylightSavingDate.Day);
			AssertEquals("Hour value for R4_DaylightSavingDate should not have changed when the month was changed", 12, Rule.R4_DaylightSavingDate.Hour);
			AssertEquals("Minute value for R4_DaylightSavingDate not have changed when the day month changed", 12, Rule.R4_DaylightSavingDate.Minute);
			AssertEquals("Second value for R4_DaylightSavingDate not have changed when the day month changed", 12, Rule.R4_DaylightSavingDate.Second);
		}

		#endregion

		#region DaylightSavingChangeTime

		public void TestDaylightSavingChangeTimeInvalidInput()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DaylightSavingChangeTime = ZDateTime.Invalid;
			AssertHasErrors("DaylightSavingChangeTime should check for an invalid ZDateTime as the input", Rule.DaylightSavingChangeTimeInfo);
		}

		[TestDate(2006, 12, 25)]
		public void TestDaylightSavingChangeTimeWhenDatabaseDateFieldEmptyOrInvalid()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.R4_DaylightSavingDate = ZDateTime.Empty;
			Rule.DaylightSavingChangeTime = new ZDateTime(2006, 1, 1, 20, 15, 19);
			AssertEquals("Year value for R4_DaylightSavingDate should be set to the current year", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Day value for R4_DaylightSavingDate should be 1", 1, Rule.R4_DaylightSavingDate.Day);
			AssertEquals("Hour value for R4_DaylightSavingDate should be 20", 20, Rule.R4_DaylightSavingDate.Hour);
			AssertEquals("Minute value for R4_DaylightSavingDate should be 15", 15, Rule.R4_DaylightSavingDate.Minute);
			AssertEquals("Second value for R4_DaylightSavingDate should be 19", 19, Rule.R4_DaylightSavingDate.Second);

			Rule.R4_DaylightSavingDate = new ZDateTime(2006, 07, 07, 0, 0, 0);
			Rule.DaylightSavingChangeTime = new ZDateTime(2006, 07, 07, 20, 15, 19);
			AssertEquals("Year value for R4_DaylightSavingDate should not have changed when the day was changed", ZDateTime.Now.Year, Rule.R4_DaylightSavingDate.Year);
			AssertEquals("Month value for R4_DaylightSavingDate should not have changed when the day was changed", 7, Rule.R4_DaylightSavingDate.Month);
			AssertEquals("Day value for R4_DaylightSavingDate should not have changed when the day was changed", 7, Rule.R4_DaylightSavingDate.Day);
		}

		#endregion

		#region R4_DaylightSavingDayWeekDate

		public void TestR4_DaylightSavingDayWeekDate()
		{
			//check Date fields empty when DayWeekDate field changed to Standard
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			Rule.R4_DaylightSavingDate = new ZDateTime(2005, 2, 2);
			Rule.R4_DaylightSavingDayWeekDate = "...";
			Assert("Should be cleared", Rule.R4_DaylightSavingDate.IsEmpty);

			//check Date fields empty when DayWeekDate field changed to Recurring
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			Rule.R4_DaylightSavingDate = new ZDateTime(2005, 2, 2);
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;
			Assert("Should be cleared", Rule.R4_DaylightSavingDate.IsEmpty);

			//check Day fields empty when DayWeekDate field changed to Standard

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Rule.R4_DaylightSavingDayCount = 1;
			Rule.R4_DaylightSavingDayName = "MON";
			Rule.R4_DaylightSavingMonth = "JAN";

			Rule.R4_DaylightSavingDayWeekDate = "***";

			Assert("Should be cleared", Rule.DayNumber.Equals("0"));
			Assert("Should be cleared", Rule.R4_DaylightSavingDayName.IsEmpty);
			Assert("Should be cleared", Rule.R4_DaylightSavingMonth.IsEmpty);

			//check Day fields empty when DayWeekDate field changed to Date

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Rule.R4_DaylightSavingDayCount = 1;
			Rule.R4_DaylightSavingDayName = "MON";
			Rule.R4_DaylightSavingMonth = "JAN";

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Assert("Should be cleared", Rule.DayNumber.Equals("0"));
			Assert("Should be cleared", Rule.R4_DaylightSavingDayName.IsEmpty);
			Assert("Should be cleared", Rule.R4_DaylightSavingMonth.IsEmpty);
		}

		#endregion

		#endregion

		#region CheckReadOnlyFields

		public void TestCheckReadOnlyFields()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Assert("Should NOT be readonly", !Rule.DateDayNumberInfo.ReadOnly);
			Assert("Should NOT be readonly", !Rule.DateMonthInfo.ReadOnly);

			Assert("Should be readonly", Rule.DayNumberInfo.ReadOnly);
			Assert("Should be readonly", Rule.R4_DaylightSavingDayNameInfo.ReadOnly);
			Assert("Should be readonly", Rule.R4_DaylightSavingMonthInfo.ReadOnly);

			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Assert("Should be readonly", Rule.DateDayNumberInfo.ReadOnly);
			Assert("Should be readonly", Rule.DateMonthInfo.ReadOnly);

			Assert("Should NOT be readonly", !Rule.DayNumberInfo.ReadOnly);
			Assert("Should NOT be readonly", !Rule.R4_DaylightSavingDayNameInfo.ReadOnly);
			Assert("Should NOT be readonly", !Rule.R4_DaylightSavingMonthInfo.ReadOnly);
		}

		#endregion

		#region IsDateValid

		public void TestIsDateValid()
		{
			bool success = Rule.IsDateValid(2006, 07, 31, 0, 0, 0);
			Assert("The input date should be valid", success);

			success = Rule.IsDateValid(2006, 02, 28, 0, 0, 0);
			Assert("The input date should be valid", success);

			success = Rule.IsDateValid(2004, 02, 29, 0, 0, 0);
			Assert("The input date should be valid", success);

			bool fail = Rule.IsDateValid(2004, 02, 30, 0, 0, 0);
			Assert("The input date should NOT be valid", !fail);

			fail = Rule.IsDateValid(2006, 9, 31, 0, 0, 0);
			Assert("The input date should NOT be valid", !fail);
		}

		#endregion

		#region Implementation

		RefTimeZoneSet TimeZoneSet;
		DaylightSavingTimeZone TimeZone;
		RefTimeZoneRule Rule;

		protected override void SetUp()
		{
			base.SetUp();
			TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			TimeZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>();
			TimeZoneSet.R3_R2_DaylightSavingZone = TimeZone.PK;
			Rule = TimeZone.StartDateRules.AddNew();
		}

		#endregion
	}
}
