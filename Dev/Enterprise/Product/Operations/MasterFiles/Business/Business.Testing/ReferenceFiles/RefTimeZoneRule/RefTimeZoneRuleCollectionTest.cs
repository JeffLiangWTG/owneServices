using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RefTimeZoneRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<RefTimeZoneRuleCollection>
	{
		#region CreateRelationshipFilter

		public void TestCreateRelationshipFilter()
		{
			DaylightSavingTimeZone timeZone = Factory.New<DaylightSavingTimeZone>();
			DaylightSavingTimeZone timeZone2 = Factory.New<DaylightSavingTimeZone>();

			RefTimeZoneRule rule1 = timeZone.StartDateRules.AddNew();
			RefTimeZoneRule rule2 = timeZone.EndDateRules.AddNew();

			RefTimeZoneEndRuleCollection newEnds = new RefTimeZoneEndRuleCollection(timeZone);
			AssertEquals(1, newEnds.Count);

			RefTimeZoneStartRuleCollection newStarts = new RefTimeZoneStartRuleCollection(timeZone);
			AssertEquals(1, newStarts.Count);

			RefTimeZoneStartRuleCollection otherStarts = new RefTimeZoneStartRuleCollection(timeZone2);
			AssertEquals(0, otherStarts.Count);

			RefTimeZoneEndRuleCollection otherEnds = new RefTimeZoneEndRuleCollection(timeZone2);
			AssertEquals(0, otherEnds.Count);
		}

		#endregion
	}
}
