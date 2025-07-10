using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneRuleValidationTest : BusinessObjectValidationTestCase
	{
		#region DayWeekDateSelectionValidation

		public void TestDayWeekDateSelectionValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;
			AssertNoErrors(Rule.R4_DaylightSavingDayWeekDateInfo);

			Rule.R4_DaylightSavingDayWeekDate = "";
			AssertHasErrors(Rule.R4_DaylightSavingDayWeekDateInfo);

			Rule.R4_DaylightSavingDayWeekDate = "AAA";
			AssertHasErrors(Rule.R4_DaylightSavingDayWeekDateInfo);
		}

		#endregion

		#region DayCountValidation

		public void TestDayCountValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Rule.DayNumber = "3";
			AssertNoErrors(Rule.DayNumberInfo);

			Rule.DayNumber = "0";
			AssertHasErrors(Rule.DayNumberInfo);

			Rule.DayNumber = "F";
			AssertHasErrors(Rule.DayNumberInfo);
		}

		#endregion

		#region DayNameValidation

		public void TestDayNameValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Rule.R4_DaylightSavingDayName = "MON";
			AssertNoErrors(Rule.R4_DaylightSavingDayNameInfo);

			Rule.R4_DaylightSavingDayName = "AAA";
			AssertHasErrors(Rule.R4_DaylightSavingDayNameInfo);
		}

		#endregion

		#region MonthValidation

		public void TestMonthValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			Rule.R4_DaylightSavingMonth = "MAR";
			AssertNoErrors(Rule.R4_DaylightSavingMonthInfo);

			Rule.R4_DaylightSavingMonth = "AAA";
			AssertHasErrors(Rule.R4_DaylightSavingMonthInfo);
		}

		#endregion

		#region DateDayNumberValidation

		[TestDate(2004, 07, 07, 12, 25, 36)]
		public void TestDateDayNumberValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DateDayNumber = "3";
			AssertNoErrors(Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "29";
			AssertNoErrors(Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "0";
			AssertHasErrors(Rule.DateDayNumberInfo);

			Rule.DateDayNumber = "F";
			AssertHasErrors(Rule.DateDayNumberInfo);

			Rule.R4_DaylightSavingDate = new ZDateTime(2004, 07, 07, 12, 25, 36);
			Rule.DateDayNumber = "3";
			AssertNoErrors(Rule.DateDayNumberInfo);

			Rule.R4_DaylightSavingDate = new ZDateTime(2004, 09, 07, 12, 25, 36);
			Rule.DateDayNumber = "31";
			AssertHasErrors(Rule.DateDayNumberInfo);

			Rule.DateMonth = "FEB";
			Rule.DateDayNumber = "29";
			AssertHasWarning(Rule.DateDayNumberInfo, "Note: February 29th is only a valid date in a leap year. If this date is not manually changed to be a valid date in a non-leap year, daylight saving will not work correctly.");
		}

		#endregion

		#region DateMonthValidation

		[TestDate(2004, 07, 07, 12, 25, 36)]
		public void TestDateMonthValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DateMonth = "";
			AssertHasErrors(Rule.DateMonthInfo);

			Rule.DateMonth = "ABC";
			AssertHasErrors(Rule.DateMonthInfo);

			Rule.R4_DaylightSavingDate = new ZDateTime(2004, 07, 07, 12, 25, 36);
			Rule.DateMonth = "MAR";
			AssertNoErrors(Rule.DateMonthInfo);

			Rule.DateDayNumber = "29";
			Rule.DateMonth = "FEB";
			AssertHasWarning(Rule.DateMonthInfo, "Note: February 29th is only a valid date in a leap year. If this date is not manually changed to be a valid date in a non-leap year, daylight saving will not work correctly.");
		}

		#endregion

		#region DaylightSavingChangeTimeValidation

		public void TestDaylightSavingChangeTimeValidation()
		{
			Rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;

			Rule.DaylightSavingChangeTime = ZDateTime.Empty;
			AssertHasErrors(Rule.DaylightSavingChangeTimeInfo);

			Rule.DaylightSavingChangeTime = new ZDateTime(2004, 07, 07, 12, 25, 36);
			AssertNoErrors(Rule.DaylightSavingChangeTimeInfo);
		}

		#endregion

		#region FromYearValidation

		public void TestFromYearValidation()
		{
			Rule.R4_FromYear = 0;
			AssertHasErrors(Rule.R4_FromYearInfo);

			Rule.R4_FromYear = 2000;
			AssertNoErrors(Rule.R4_FromYearInfo);

			Rule.R4_FromYear = 500;
			AssertHasError(Rule.R4_FromYearInfo, "The first year when this rule applies must be greater than 1900.");

			Rule.R4_ToYear = 2005;
			Rule.R4_FromYear = 2020;
			AssertHasError(Rule.R4_FromYearInfo, "The first year when this rule applies must be before or in the same year as when the rule ends.");

			Rule.R4_ToYear = 0;
			Rule.R4_FromYear = 2020;
			AssertNoErrors(Rule.R4_FromYearInfo);
		}

		#endregion

		#region ToYearValidation

		public void TestToYearValidation()
		{
			Rule.R4_ToYear = 2030;
			AssertNoErrors(Rule.R4_ToYearInfo);

			Rule.R4_ToYear = 1800;
			AssertHasErrors(Rule.R4_ToYearInfo);

			Rule.R4_ToYear = 0;
			AssertNoErrors(Rule.R4_ToYearInfo);

			Rule.R4_FromYear = 2020;
			Rule.R4_ToYear = 2005;
			AssertHasError(Rule.R4_ToYearInfo, "The first year when this rule applies must be before or in the same year as when the rule ends.");
		}

		#endregion

		#region TypeOfTimeValidation

		public void TypeOfTimeValidation()
		{
			Rule.R4_TypeOfTime = "";
			AssertHasErrors(Rule.R4_TypeOfTimeInfo);

			Rule.R4_TypeOfTime = "ABC";
			AssertHasErrors(Rule.R4_TypeOfTimeInfo);

			Rule.R4_TypeOfTime = "UTC";
			AssertNoErrors(Rule.R4_TypeOfTimeInfo);

			Rule.R4_TypeOfTime = "LOC";
			AssertNoErrors(Rule.R4_TypeOfTimeInfo);

			Rule.R4_TypeOfTime = "STD";
			AssertNoErrors(Rule.R4_TypeOfTimeInfo);
		}

		#endregion

		#region Implementation

		DaylightSavingTimeZone DaylightSavingZone;
		RefTimeZoneRule Rule;

		protected override void SetUp()
		{
			base.SetUp();

			DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>();
			Rule = DaylightSavingZone.StartDateRules.AddNew();
		}

		#endregion
	}
}
