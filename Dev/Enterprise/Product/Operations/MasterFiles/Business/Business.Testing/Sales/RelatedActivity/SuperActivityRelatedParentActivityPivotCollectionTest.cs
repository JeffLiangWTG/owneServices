using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SuperActivityRelatedParentActivityPivotCollection))]
	sealed class SuperActivityRelatedParentActivityPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<SuperActivityRelatedParentActivityPivotCollection>
	{
		public void TestAddActivity()
		{
			var superActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			var subActivity = superActivity.AddNewSubRelatableActivity();

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			superActivity.StubMatchingSubActivityForTesting(opportunity2, subActivity);
			Factory.Save();

			var collection = new SuperActivityRelatedParentActivityPivotCollection(superActivity);

			var addResult1 = collection.AddActivity(communication);
			AssertEquals(true, addResult1.Success);
			AssertCollectionContains(communication, collection.Activities);

			var addResult2 = collection.AddActivity(opportunity1);
			AssertEquals(true, addResult2.Success);
			AssertCollectionContains("Should link opportunity to superActivity", opportunity1, collection.Activities);

			var addResult3 = collection.AddActivity(opportunity2);
			AssertEquals(true, addResult3.Success);
			AssertCollectionNotContains("Should link opportunity to subActivity instead of than the superActivity", opportunity2, collection.Activities);
			AssertCollectionContains("Should link opportunity to subActivity", opportunity2, subActivity.RelatedParentActivityPivotCollection.Activities);
		}

		#region Implementation

		protected override SuperActivityRelatedParentActivityPivotCollection GetCollectionToTest()
		{
			var superActivity = Factory.NewWithValidTestData<DummySuperRelatableActivity>();
			return new SuperActivityRelatedParentActivityPivotCollection(superActivity);
		}

		#endregion
	}
}
