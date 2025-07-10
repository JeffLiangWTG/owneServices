using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using HRCampaignContactFilterCategories = Enterprise.Recruiter.GUI.HRGlbCompanyCampaignContactFilterBusinessObject.HRCampaignContactFilterCategories;

namespace Enterprise.Recruiter.GUI.Testing
{
	public class HRGlbCompanyCampaignModuleFilterCollectionTest : TestCaseWithFactory
	{
		public void TestIsFilterAvailableNonTouchCampaign()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			var collection = new HRGlbCompanyCampaignModuleFilterCollection(campaign);
			var filter1 = collection.AddTextFilter("Org Contact Name", DummyBizoSchema.Z0_VarCharMax);
			filter1.Category = FilterCategories.StatusAndFlags;
			var filter2 = collection.AddTextFilter("InquiryContactName", DummyBizoSchema.Z0_VarCharMax);
			filter2.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.Inquiries;
			var filter3 = collection.AddTextFilter("StaffContactName", DummyBizoSchema.Z0_VarCharMax);
			filter3.Category = HRCampaignContactFilterCategories.Staff;
			var filter4 = collection.AddTextFilter("JobApplicantContactName", DummyBizoSchema.Z0_VarCharMax);
			filter4.Category = HRCampaignContactFilterCategories.JobApplicant;
			var filter5 = collection.AddTextFilter("Job Category", DummyBizoSchema.Z0_VarCharMax);
			filter5.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.ClientIntelligenceAndInquiries;
			var filter6 = collection.AddTextFilter("Tracking Status", DummyBizoSchema.Z0_VarCharMax);
			filter6.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CampaignTracking;
			var filter7 = collection.AddTextFilter("Email Address", DummyBizoSchema.Z0_VarCharMax);
			filter7.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CommonTypes;
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			Assert("Precondition", campaign.IsUsingStaffDataSource);
			AssertEquals(6, collection.Filter_List.Count);
			AssertEquals("Collection should not have filter1", false, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should not have filter2", false, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should have filter3", true, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should not have filter4", false, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should not have filter5", false, collection.Filter_List.ContainsCode(filter5.Description));
			AssertEquals("Collection should not have filter6", false, collection.Filter_List.ContainsCode(filter6.Description));
			AssertEquals("Collection should have filter7", true, collection.Filter_List.ContainsCode(filter7.Description));
			collection.ResetCampaignFilterList();
			campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			Assert("Precondition", campaign.IsUsingJobApplicantDataSource);
			AssertEquals(6, collection.Filter_List.Count);
			AssertEquals("Collection should not have filter1", false, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should not have filter2", false, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should not have filter3", false, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should have filter4", true, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should not have filter5", false, collection.Filter_List.ContainsCode(filter5.Description));
			AssertEquals("Collection should not have filter6", false, collection.Filter_List.ContainsCode(filter6.Description));
			AssertEquals("Collection should have filter7", true, collection.Filter_List.ContainsCode(filter7.Description));
		}

		public void TestIsFilterAvailableTouchCampaign()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			var touch = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			campaign.AllTouches.Add(touch);
			touch.ContactDataSource = HRContactDataSourceList.Codes.CampaignTracking;
			var collection = new HRGlbCompanyCampaignModuleFilterCollection(touch);
			var filter1 = collection.AddTextFilter("Org Contact Name", DummyBizoSchema.Z0_VarCharMax);
			filter1.Category = FilterCategories.StatusAndFlags;
			var filter2 = collection.AddTextFilter("InquiryContactName", DummyBizoSchema.Z0_VarCharMax);
			filter2.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.Inquiries;
			var filter3 = collection.AddTextFilter("StaffContactName", DummyBizoSchema.Z0_VarCharMax);
			filter3.Category = HRCampaignContactFilterCategories.Staff;
			var filter4 = collection.AddTextFilter("JobApplicantContactName", DummyBizoSchema.Z0_VarCharMax);
			filter4.Category = HRCampaignContactFilterCategories.JobApplicant;
			var filter5 = collection.AddTextFilter("Job Category", DummyBizoSchema.Z0_VarCharMax);
			filter5.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.ClientIntelligenceAndInquiries;
			var filter6 = collection.AddTextFilter("Tracking Status", DummyBizoSchema.Z0_VarCharMax);
			filter6.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CampaignTracking;
			var filter7 = collection.AddTextFilter("Email Address", DummyBizoSchema.Z0_VarCharMax);
			filter7.Category = MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CommonTypes;
			Assert("Precondition", touch.IsUsingCampaignTrackingDataSource);
			AssertEquals("Collection should have filter1", true, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should have filter2", true, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should have filter3", true, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should have filter4", true, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should have filter5", true, collection.Filter_List.ContainsCode(filter5.Description));
			AssertEquals("Collection should have filter6", true, collection.Filter_List.ContainsCode(filter6.Description));
			AssertEquals("Collection should have filter7", true, collection.Filter_List.ContainsCode(filter7.Description));
		}

		public void TestBMSFiltersVisibility()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			var bufferManagementDescription = (NoResString)"Buffer Management";
			var collection = new HRGlbCompanyCampaignModuleFilterCollection(campaign);
			var filter1 = collection.AddTextFilter("Tag", DummyBizoSchema.Z0_VarCharMax);
			filter1.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			var filter2 = collection.AddTextFilter("Tag Group", DummyBizoSchema.Z0_VarCharMax);
			filter2.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			var filter3 = collection.AddTextFilter("Buffer Management Component", DummyBizoSchema.Z0_VarCharMax);
			filter3.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			var filter4 = collection.AddTextFilter("Schedule Type", DummyBizoSchema.Z0_VarCharMax);
			filter4.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			var filter5 = collection.AddTextFilter("Approved Scheduled Start Time", DummyBizoSchema.Z0_VarCharMax);
			filter5.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			var filter6 = collection.AddTextFilter("Approved Scheduled Finish Time", DummyBizoSchema.Z0_VarCharMax);
			filter6.Category = FilterCategories.GetOrCreateFilterCategory(bufferManagementDescription);
			campaign.ContactDataSource = HRContactDataSourceList.Codes.Staff;
			Assert("Precondition", campaign.IsUsingStaffDataSource);
			AssertEquals(8, collection.Filter_List.Count);
			Assert("Collection should have filter1", collection.Filter_List.ContainsCode(filter1.Description));
			Assert("Collection should have filter2", collection.Filter_List.ContainsCode(filter2.Description));
			Assert("Collection should have filter3", collection.Filter_List.ContainsCode(filter3.Description));
			Assert("Collection should have filter4", collection.Filter_List.ContainsCode(filter4.Description));
			Assert("Collection should have filter5", collection.Filter_List.ContainsCode(filter5.Description));
			Assert("Collection should have filter6", collection.Filter_List.ContainsCode(filter6.Description));
			collection.ResetCampaignFilterList();
			campaign.ContactDataSource = HRContactDataSourceList.Codes.JobApplicant;
			Assert("Precondition", campaign.IsUsingJobApplicantDataSource);
			AssertEquals(8, collection.Filter_List.Count);
			Assert("Collection should have filter1", collection.Filter_List.ContainsCode(filter1.Description));
			Assert("Collection should have filter2", collection.Filter_List.ContainsCode(filter2.Description));
			Assert("Collection should have filter3", collection.Filter_List.ContainsCode(filter3.Description));
			Assert("Collection should have filter4", collection.Filter_List.ContainsCode(filter4.Description));
			Assert("Collection should have filter5", collection.Filter_List.ContainsCode(filter5.Description));
			Assert("Collection should have filter6", collection.Filter_List.ContainsCode(filter6.Description));
		}
	}
}
