using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class GlbCompanyCampaignModuleFilterCollectionTest : TestCaseWithFactory
	{
		public void TestIsFilterAvailable()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var collection = new GlbCompanyCampaignModuleFilterCollection(campaign);

			ModuleFilter filter1 = collection.AddTextFilter("Org Contact Name", DummyBizoSchema.Z0_VarCharMax);
			filter1.Category = FilterCategories.StatusAndFlags;

			ModuleFilter filter2 = collection.AddTextFilter("InquiryContactName", DummyBizoSchema.Z0_VarCharMax);
			filter2.Category = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.Inquiries;

			ModuleFilter filter3 = collection.AddTextFilter("Job Category", DummyBizoSchema.Z0_VarCharMax);
			filter3.Category = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.ClientIntelligenceAndInquiries;

			ModuleFilter filter4 = collection.AddTextFilter("Tracking Status", DummyBizoSchema.Z0_VarCharMax);
			filter4.Category = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CampaignTracking;

			ModuleFilter filter5 = collection.AddTextFilter("Email Address", DummyBizoSchema.Z0_VarCharMax);
			filter5.Category = Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject.CampaignContactFilterCategories.CommonTypes;

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			Assert("Precondition", campaign.IsUsingClientIntelligenceDataSource);
			AssertEquals(9, collection.Filter_List.Count);
			AssertEquals("Collection should have filter1", true, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should have filter2", false, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should have filter3", true, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should have filter4", false, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should have filter5", true, collection.Filter_List.ContainsCode(filter5.Description));

			collection.ResetCampaignFilterList();

			campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			Assert("Precondition", campaign.IsUsingInquiryDataSource);
			AssertEquals(9, collection.Filter_List.Count);
			AssertEquals("Collection should have filter1", false, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should have filter2", true, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should have filter3", true, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should have filter4", false, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should have filter5", true, collection.Filter_List.ContainsCode(filter5.Description));

			collection.ResetCampaignFilterList();

			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Assert("Precondition", campaign.IsUsingCampaignTrackingDataSource);
			AssertEquals(15, collection.Filter_List.Count);
			AssertEquals("Collection should have filter1", true, collection.Filter_List.ContainsCode(filter1.Description));
			AssertEquals("Collection should have filter2", true, collection.Filter_List.ContainsCode(filter2.Description));
			AssertEquals("Collection should have filter3", true, collection.Filter_List.ContainsCode(filter3.Description));
			AssertEquals("Collection should have filter4", true, collection.Filter_List.ContainsCode(filter4.Description));
			AssertEquals("Collection should have filter5", true, collection.Filter_List.ContainsCode(filter5.Description));
		}

		public void TestBMSFiltersVisibility()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var bufferManagementDescription = (NoResString)"Buffer Management";
			var collection = new GlbCompanyCampaignModuleFilterCollection(campaign);

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

			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			Assert("Precondition", campaign.IsUsingCampaignTrackingDataSource);
			AssertEquals(8, collection.Filter_List.Count);
			Assert("Collection should have filter1", collection.Filter_List.ContainsCode(filter1.Description));
			Assert("Collection should have filter2", collection.Filter_List.ContainsCode(filter2.Description));
			Assert("Collection should have filter3", collection.Filter_List.ContainsCode(filter3.Description));
			Assert("Collection should have filter4", collection.Filter_List.ContainsCode(filter4.Description));
			Assert("Collection should have filter5", collection.Filter_List.ContainsCode(filter5.Description));
			Assert("Collection should have filter6", collection.Filter_List.ContainsCode(filter6.Description));

			collection.ResetCampaignFilterList();

			campaign.ContactDataSource = ContactDataSourceList.Codes.ClientIntelligence;
			Assert("Precondition", campaign.IsUsingClientIntelligenceDataSource);
			AssertEquals(8, collection.Filter_List.Count);
			Assert("Collection should have filter1", collection.Filter_List.ContainsCode(filter1.Description));
			Assert("Collection should have filter2", collection.Filter_List.ContainsCode(filter2.Description));
			Assert("Collection should have filter3", collection.Filter_List.ContainsCode(filter3.Description));
			Assert("Collection should have filter4", collection.Filter_List.ContainsCode(filter4.Description));
			Assert("Collection should have filter5", collection.Filter_List.ContainsCode(filter5.Description));
			Assert("Collection should have filter6", collection.Filter_List.ContainsCode(filter6.Description));

			collection.ResetCampaignFilterList();

			campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
			Assert("Precondition", campaign.IsUsingInquiryDataSource);
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
