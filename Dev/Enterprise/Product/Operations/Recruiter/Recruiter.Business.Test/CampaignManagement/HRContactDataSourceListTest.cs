using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRContactDataSourceListTest : TransactionedTestCase
	{
		public void TestNonTouchCampaign()
		{
			var factory = new BusinessObjectFactory();
			var campaign = factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			var list = new HRContactDataSourceList(campaign);
			AssertEquals(HRContactDataSourceList.FilterModuleIDSuffixes.Staff, list.GetDescriptionFromCode(HRContactDataSourceList.Codes.Staff));
			AssertEquals(HRContactDataSourceList.FilterModuleIDSuffixes.JobApplicant, list.GetDescriptionFromCode(HRContactDataSourceList.Codes.JobApplicant));
			AssertEquals(HRContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, list.GetDescriptionFromCode(HRContactDataSourceList.Codes.CampaignTracking));
			AssertEquals(3, list.Count);
		}

		public void TestTouchCampaign()
		{
			var factory = new BusinessObjectFactory();

			var master = factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();

			AssertEquals(HRContactDataSourceList.Codes.CampaignTracking, touch.ContactDataSource);

			var list = new HRContactDataSourceList(touch);
			AssertEquals(HRContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, list.GetDescriptionFromCode(HRContactDataSourceList.Codes.CampaignTracking));
			AssertEquals(1, list.Count);
		}
	}
}
