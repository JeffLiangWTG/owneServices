using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRRecruitmentJobCampaignDocManagerInfo))]
	sealed class HRRecruitmentJobCampaignDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestRelatedObjects()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.New<HRRecruitmentJobCampaign>();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			AssertEquals(5, jobCampaign.DocManagerInfo.RelatedObjects.Length);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<HRRecruitmentJobCampaign>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			HRRecruitmentJobCampaign jobCampaign = Factory.New<HRRecruitmentJobCampaign>();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			jobCampaign.Applications.AddNew();
			return jobCampaign;
		}
	}
}
