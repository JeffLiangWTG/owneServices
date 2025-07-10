using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTimeZoneEndRuleCollection))]
	sealed class RefTimeZoneEndRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<RefTimeZoneEndRuleCollection>
	{
		protected override RefTimeZoneEndRuleCollection GetCollectionToTest()
		{
			return new RefTimeZoneEndRuleCollection(Factory.New<DaylightSavingTimeZone>());
		}

		public void TestDefaultsForNewChild()
		{
			base.TestAddNew();

			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			RefTimeZoneEndRuleCollection endRules = timeZoneSet.DaylightSavingZone.EndDateRules;
			endRules.AddNew();

			AssertEquals(RefTimeZoneRule.EndRuleCode, endRules[0].R4_StartOrEndRule);
			AssertEquals(timeZoneSet.DaylightSavingZone.PK, endRules[0].R4_R2);
		}
	}
}
