using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSalesRelationModel))]
	sealed class GlbCompanyCampaignSalesRelationModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMasterNode()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var model = new GlbCompanyCampaignSalesRelationModel(campaign);
			AssertType(typeof(GlbCompanyCampaignSalesRelationMasterNode), model.MasterNode);

			AssertEquals(campaign, model.MasterNode.BizObj);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			return new GlbCompanyCampaignSalesRelationModel(campaign);
		}

		#endregion
	}
}
