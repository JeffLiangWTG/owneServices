using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneRuleFactoryTest : TestCaseWithFactory
	{
		[TestDate(2006, 07, 05)]
		public void TestGetDaylightSavingDateInYear()
		{
			RefTimeZoneRule rule = StartRules[0];
			rule.R4_FromYear = 2000;
			rule.R4_ToYear = 2008;
			rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			rule.R4_DaylightSavingDate = new ZDateTime(2004, 07, 05);

			ZDateTime expected = new ZDateTime(2002, rule.R4_DaylightSavingDate.Month, rule.R4_DaylightSavingDate.Day, rule.R4_DaylightSavingDate.Hour, rule.R4_DaylightSavingDate.Minute, rule.R4_DaylightSavingDate.Second);

			AssertEquals("If R4_DaylightSavingDayWeekDate is set to Date, the returned date should return R4_DaylightSavingDate with the year being the year requested", expected, rule.GetDaylightSavingDateTimeInYear(2002));

			rule.R4_DaylightSavingDayWeekDate = "XXX";
			AssertEquals("If no daylight saving details exist, an empty string should be returned", ZDateTime.Empty, rule.GetDaylightSavingDateTimeInYear(2002));

			rule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleWeekday;

			rule.R4_DaylightSavingMonth = "JAN";
			rule.R4_DaylightSavingDayName = "TUE";
			rule.R4_DaylightSavingDayCount = 3;
			rule.R4_DaylightSavingDate = new ZDateTime(2006, 07, 05, 16, 15, 25);

			expected = new ZDateTime(2006, 01, 17, 16, 15, 25);

			AssertEquals("The returned date should include hours, minutes and seconds if R4_DaylightSavingDate is set", expected, rule.GetDaylightSavingDateTimeInYear(2006));

			rule.R4_DaylightSavingMonth = "JAN";
			rule.R4_DaylightSavingDayName = "TUE";
			rule.R4_DaylightSavingDayCount = 3;
			rule.R4_DaylightSavingDate = ZDateTime.Empty;

			AssertEquals("If R4_DaylightSavingDate is not set, ZDateTime.Empty should be returned", ZDateTime.Empty, rule.GetDaylightSavingDateTimeInYear(2006));

			rule.R4_DaylightSavingMonth = "JAN";
			rule.R4_DaylightSavingDayName = "WED";
			rule.R4_DaylightSavingDayCount = 6;
			rule.R4_DaylightSavingDate = new ZDateTime(2006, 07, 05, 16, 15, 25);

			expected = ZDateTime.Empty;

			AssertEquals("If the recurring date specified does not exist in the current year, an empty date should be returned", expected, rule.GetDaylightSavingDateTimeInYear(2006));
		}

		#region Convert Methods

		public void TestGetMonthAsString()
		{
			RefTimeZoneRule rule = StartRules[0];

			ZString result = rule.GetMonthAsString(1);
			AssertEquals("Month as String doesn't correspond to Month as Int", "JAN", result);

			result = rule.GetMonthAsString(2);
			AssertEquals("Month as String doesn't correspond to Month as Int", "FEB", result);

			result = rule.GetMonthAsString(3);
			AssertEquals("Month as String doesn't correspond to Month as Int", "MAR", result);

			result = rule.GetMonthAsString(4);
			AssertEquals("Month as String doesn't correspond to Month as Int", "APR", result);

			result = rule.GetMonthAsString(5);
			AssertEquals("Month as String doesn't correspond to Month as Int", "MAY", result);

			result = rule.GetMonthAsString(6);
			AssertEquals("Month as String doesn't correspond to Month as Int", "JUN", result);

			result = rule.GetMonthAsString(7);
			AssertEquals("Month as String doesn't correspond to Month as Int", "JUL", result);

			result = rule.GetMonthAsString(8);
			AssertEquals("Month as String doesn't correspond to Month as Int", "AUG", result);

			result = rule.GetMonthAsString(9);
			AssertEquals("Month as String doesn't correspond to Month as Int", "SEP", result);

			result = rule.GetMonthAsString(10);
			AssertEquals("Month as String doesn't correspond to Month as Int", "OCT", result);

			result = rule.GetMonthAsString(11);
			AssertEquals("Month as String doesn't correspond to Month as Int", "NOV", result);

			result = rule.GetMonthAsString(12);
			AssertEquals("Month as String doesn't correspond to Month as Int", "DEC", result);

			result = rule.GetMonthAsString(87);
			AssertEquals("Invalid int input should return empty string", "", result);
		}

		#endregion

		#region Implementation

		DaylightSavingTimeZone DaylightSavingZone;
		RefTimeZoneRuleCollection StartRules;
		RefTimeZoneRuleCollection EndRules;

		protected override void SetUp()
		{
			base.SetUp();

			DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>();
			StartRules = DaylightSavingZone.StartDateRules;
			StartRules.AddNew();
			EndRules = DaylightSavingZone.EndDateRules;
			EndRules.AddNew();
		}

		#endregion
	}
}
