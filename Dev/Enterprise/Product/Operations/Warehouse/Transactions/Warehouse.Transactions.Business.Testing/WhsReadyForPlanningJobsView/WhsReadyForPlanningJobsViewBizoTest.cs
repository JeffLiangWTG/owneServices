using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReadyForPlanningJobsView))]
	class WhsReadyForPlanningJobsViewBizoTest : WhsBusinessObjectTestCase
	{
		public void TestIsSavedByFactory()
		{
			AssertEquals("Bizo generated from a view.", false, Factory.New<WhsReadyForPlanningJobsView>().IsSavedByFactory);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Bizo generated from a view.", true);
		}

		public override void TestFetchForLoad()
		{
			Assert("Bizo generated from a view.", true);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Bizo generated from a view.", true);
		}
	}
}
