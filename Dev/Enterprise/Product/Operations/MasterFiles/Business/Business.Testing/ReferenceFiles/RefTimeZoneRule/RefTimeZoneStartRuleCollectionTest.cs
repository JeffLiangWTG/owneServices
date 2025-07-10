using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTimeZoneStartRuleCollection))]
	sealed class RefTimeZoneStartRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<RefTimeZoneStartRuleCollection>
	{
		protected override RefTimeZoneStartRuleCollection GetCollectionToTest()
		{
			return new RefTimeZoneStartRuleCollection(Factory.New<DaylightSavingTimeZone>());
		}

		#region AddNew

		public void TestDefaultsForNewChild()
		{
			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.HasDaylightSavings = true;
			RefTimeZoneStartRuleCollection startRules = timeZoneSet.DaylightSavingZone.StartDateRules;
			startRules.AddNew();

			AssertEquals(RefTimeZoneRule.StartRuleCode, startRules[0].R4_StartOrEndRule);
			AssertEquals(timeZoneSet.DaylightSavingZone.PK, startRules[0].R4_R2);
		}

		#endregion

	}
}
