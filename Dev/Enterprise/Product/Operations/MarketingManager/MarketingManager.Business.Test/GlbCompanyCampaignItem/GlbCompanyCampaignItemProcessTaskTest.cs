using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemProcessTask))]
	sealed class GlbCompanyCampaignItemProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var processTask = Factory.New<GlbCompanyCampaignItemProcessTask>();

			AssertEquals(typeof(GlbCompanyCampaignItemProcessTask), processTask.GetType());
			AssertEquals(ControllerIDs.GlbCompanyCampaignItem, processTask.ParentControllerID);
		}

		public void TestSubclassOfCRMProcessTask()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(CRMProcessTask)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<GlbCompanyCampaignItemProcessTask>();
		}

		#endregion
	}
}
