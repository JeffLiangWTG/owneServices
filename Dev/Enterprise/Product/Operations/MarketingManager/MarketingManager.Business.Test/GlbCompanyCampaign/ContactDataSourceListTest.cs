using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ContactDataSourceListTest : TransactionedTestCase
	{
		public void TestContactDataSourceList_Normal()
		{
			var list = new ContactDataSourceList(new BusinessObjectFactory().NewWithValidTestData<GlbCompanyCampaign>());
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, list.GetDescriptionFromCode(ContactDataSourceList.Codes.CampaignTracking));
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.ClientIntelligence, list.GetDescriptionFromCode(ContactDataSourceList.Codes.ClientIntelligence));
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.Inquiries, list.GetDescriptionFromCode(ContactDataSourceList.Codes.Inquiries));
		}

		public void TestContactDataSourceList_Master()
		{
			var master = new BusinessObjectFactory().NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var list = new ContactDataSourceList(master);
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, list.GetDescriptionFromCode(ContactDataSourceList.Codes.CampaignTracking));
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.ClientIntelligence, list.GetDescriptionFromCode(ContactDataSourceList.Codes.ClientIntelligence));
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.Inquiries, list.GetDescriptionFromCode(ContactDataSourceList.Codes.Inquiries));
		}

		public void TestContactDataSourceList_Touch()
		{
			var master = new BusinessObjectFactory().NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = master.AllTouches.AddNew();

			var list = new ContactDataSourceList(touch);
			AssertEquals(1, list.Count);
			AssertEquals(ContactDataSourceList.FilterModuleIDSuffixes.CampaignTracking, list.GetDescriptionFromCode(ContactDataSourceList.Codes.CampaignTracking));
		}
	}
}
