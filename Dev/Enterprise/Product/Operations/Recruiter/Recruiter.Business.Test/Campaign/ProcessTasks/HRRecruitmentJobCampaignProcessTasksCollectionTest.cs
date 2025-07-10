using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaignProcessTaskCollection))]
	sealed class HRRecruitmentJobCampaignProcessTasksCollectionTest : ProcessTaskCollectionTest<HRRecruitmentJobCampaignProcessTaskCollection>
	{
		protected override HRRecruitmentJobCampaignProcessTaskCollection GetCollectionToTestCore()
			=> new HRRecruitmentJobCampaignProcessTaskCollection(Factory.New<HRRecruitmentJobCampaign>());
	}
}
