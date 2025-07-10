using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRGlbCompanyCampaignLookupsTest : GlbCompanyCampaignLookupsTest
	{
		public void TestLookups()
		{
			var category1List = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Type.MaxLength);
			var category2List = new CodeDescriptionBoolCollection(GlbCompanyCampaignSchema.G0_Type.MaxLength);

			category1List.Add("CYANE", (NoResString)"Bananas");
			category2List.Add("CYABI", (NoResString)"Sananab");

			OrganisationsDataRegistry.Instance.HRCampaignCategory1List.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, category1List);
			OrganisationsDataRegistry.Instance.HRCampaignCategory2List.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, category2List);

			var campaign = Factory.New<HRGlbCompanyCampaign>();
			Assert(campaign.Lookups.ActiveMediaCategoryList.ContainsCode("CYANE"));
			Assert(campaign.Lookups.ActiveMediaTypesList.ContainsCode("CYABI"));
			Assert(campaign.Lookups.MediaCategoryList.ContainsCode("CYANE"));
			Assert(campaign.Lookups.MediaTypesList.ContainsCode("CYABI"));
		}

		public override void TestMediaLabels()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, (NoResString)"Test Category Label");
			OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, (NoResString)"Test Type Label");
			AssertEquals("Test Category Label", campaign.Lookups.MediaCategoryLabel);
			AssertEquals("Test Type Label", campaign.Lookups.MediaTypeLabel);
		}

		public override void TestEmailSenderOptionList()
		{
			var campaign = Factory.New<HRGlbCompanyCampaign>();
			Assert("ORG - Staff Assignment should be removed for HR Campaigns", campaign.Lookups.EmailSenderOptionList.ContainsOnly(EmailSenderOptionCodeDescriptionList.Codes.COR,
				EmailSenderOptionCodeDescriptionList.Codes.EML,
				EmailSenderOptionCodeDescriptionList.Codes.SPS));
		}
	}
}
