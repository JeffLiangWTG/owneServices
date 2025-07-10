using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRCampaignProcessTasksCollection))]
	sealed class CampaignProcessTasksCollectionTest : ProcessTaskCollectionTest<HRCampaignProcessTasksCollection>
	{
		protected override HRCampaignProcessTasksCollection GetCollectionToTestCore()
		{
			return new HRCampaignProcessTasksCollection(Campaign);
		}

		HRGlbCompanyCampaign Campaign
		{
			get { return campaign ?? (campaign = Factory.New<HRGlbCompanyCampaign>()); }
		}
		HRGlbCompanyCampaign campaign;
	}
}
