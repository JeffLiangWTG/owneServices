using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CRMCampaignProcessTasksCollection))]
	sealed class CampaignProcessTasksCollectionTest : ProcessTaskCollectionTest<CRMCampaignProcessTasksCollection>
	{
		protected override CRMCampaignProcessTasksCollection GetCollectionToTestCore()
		{
			return new CRMCampaignProcessTasksCollection(Campaign);
		}

		GlbCompanyCampaign Campaign
		{
			get { return campaign ?? (campaign = Factory.New<GlbCompanyCampaign>()); }
		}
		GlbCompanyCampaign campaign;
	}
}
