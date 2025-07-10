using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignSalesDashboardActivity))]
	sealed class CampaignManagementSalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var parent = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			parent.G8_G0 = campaign.PK;
			parent.G8_RecipientID = contact.PK;
			parent.G8_RecipientTableCode = "OC";
			var activity = Factory.New<CampaignSalesDashboardActivity>();
			activity.VSA_ParentId = parent.PK;
			return activity;
		}
	}
}
