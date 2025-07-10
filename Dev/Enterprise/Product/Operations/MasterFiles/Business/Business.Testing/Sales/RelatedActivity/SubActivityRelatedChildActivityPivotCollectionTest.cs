using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SubActivityRelatedChildActivityPivotCollection))]
	sealed class SubActivityRelatedChildActivityPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<SubActivityRelatedChildActivityPivotCollection>
	{
		public void TestAddActivity()
		{
			var superActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var subActivity1 = superActivity.AddNewSubRelatableActivity();
			var subActivity2 = superActivity.AddNewSubRelatableActivity();

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			superActivity.StubMatchingSubActivityForTesting(opportunity1, subActivity1);
			superActivity.StubMatchingSubActivityForTesting(opportunity2, subActivity2);
			Factory.Save();

			var collection1 = new SubActivityRelatedChildActivityPivotCollection(subActivity1);
			var addResult1 = collection1.AddActivity(communication);
			AssertEquals(true, addResult1.Success);
			AssertCollectionNotContains("Should have added communication to superActivity instead of subActivity1", communication, collection1.Activities);
			AssertCollectionContains("Should have added communication to superActivity instead of subActivity1", communication, superActivity.RelatedChildActivityPivotCollection.Activities);

			var addResult2 = collection1.AddActivity(opportunity1);
			AssertEquals(true, addResult2.Success);
			AssertCollectionContains("Should have added opportunity1 to subActivity1 because they match", opportunity1, collection1.Activities);

			var addResult3 = collection1.AddActivity(opportunity2);
			AssertEquals(true, addResult3.Success);
			AssertCollectionNotContains("Should have added opportunity2 to subActivity2 instead of subActivity1", opportunity2, collection1.Activities);
			AssertCollectionContains("Should have added opportunity2 to subActivity2 instead of subActivity1", opportunity2, subActivity2.RelatedChildActivityPivotCollection.Activities);
		}

		#region Implementation

		protected override SubActivityRelatedChildActivityPivotCollection GetCollectionToTest()
		{
			var subActivity = Factory.NewWithValidTestData<DummySubRelatableActivity>();
			return new SubActivityRelatedChildActivityPivotCollection(subActivity);
		}

		#endregion
	}
}
