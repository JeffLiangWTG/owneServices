using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTimeZoneRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		#region ZoneTypes

		public void TestZoneTypes()
		{
			AssertEquals("Only 2 types in the list", 2, Rule.Lookups.ZoneTypes.Count);
			Assert("DAT should be in the list", Rule.Lookups.ZoneTypes.ContainsCode(TimeZoneConstants.DstRuleDayOfMonth));
			Assert("MON should be in the list", Rule.Lookups.ZoneTypes.ContainsCode(TimeZoneConstants.DstRuleWeekday));
		}

		#endregion

		#region TypeOfTimeList

		public void TestTypeOfTimeList()
		{
			AssertEquals(3, Rule.Lookups.TypeOfTimeList.Count);
		}

		#endregion

		#region TypeOfTimeCodes

		public void TestTypeOfTimeCodes()
		{
			Assert(!string.IsNullOrEmpty(TimeZoneConstants.DstTimeBaseLocal));
			Assert(!string.IsNullOrEmpty(TimeZoneConstants.DstTimeBaseStandard));
			Assert(!string.IsNullOrEmpty(TimeZoneConstants.DstTimeBaseUtc));
		}

		#endregion

		#region NthDayOfMonthList

		public void TestNthDayOfMonthList()
		{
			AssertEquals(5, Rule.Lookups.NthDayOfMonthList.Count);
		}

		#endregion

		#region ZoneDays

		public void TestZoneDays()
		{
			AssertEquals(7, Rule.Lookups.ZoneDays.Count);
		}

		#endregion

		#region ZoneMonths

		public void TestZoneMonths()
		{
			AssertEquals(12, Rule.Lookups.ZoneMonths.Count);
		}

		#endregion

		#region DaysOfMonthList

		public void TestDaysOfMonthList()
		{
			AssertEquals(31, Rule.Lookups.DaysOfMonthList.Count);
		}

		#endregion

		#region Implementation

		RefTimeZoneRule Rule;

		protected override void SetUp()
		{
			base.SetUp();
			Rule = Factory.New<RefTimeZoneRule>();
		}

		#endregion

	}
}
