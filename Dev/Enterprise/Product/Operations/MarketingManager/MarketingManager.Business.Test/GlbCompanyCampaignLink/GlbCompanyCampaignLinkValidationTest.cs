using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGCL_Context()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var collection = new SimpleGlbCompanyCampaignLinkCollection(campaign, Factory);
			var link1 = collection.AddNew();
			var link2 = collection.AddNew();
			link1.HasTrackingMacro = true;
			link2.HasTrackingMacro = false;
			link1.GCL_Context = "A";
			link2.GCL_Context = "A";
			collection.RunPreSaveValidation();
			AssertNoErrors("duplicate OK if not tracked", link1.GCL_ContextInfo);
			AssertNoErrors("duplicate OK if not tracked", link2.GCL_ContextInfo);

			link2.HasTrackingMacro = true;
			collection.RunPreSaveValidation();
			AssertHasError(link1.GCL_ContextInfo, "Duplicate value exists. Please choose a unique value for each tracked link.");
			AssertHasError(link2.GCL_ContextInfo, "Duplicate value exists. Please choose a unique value for each tracked link.");

			link2.GCL_Context = "B";
			collection.RunPreSaveValidation();
			AssertNoErrors("unique context", link1.GCL_ContextInfo);
			AssertNoErrors("unique context", link2.GCL_ContextInfo);
		}

		public void TestGCL_URL()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var collection = new SimpleGlbCompanyCampaignLinkCollection(campaign, Factory);
			var link = collection.AddNew();
			link.Validation.ValidateGCL_URL();
			AssertHasErrors(link.GCL_URLInfo);

			link.GCL_URL = "google.com";
			link.Validation.ValidateGCL_URL();
			AssertHasErrors(link.GCL_URLInfo);

			link.GCL_URL = "ftp://google.com";
			link.Validation.ValidateGCL_URL();
			AssertHasErrors(link.GCL_URLInfo);

			link.GCL_URL = "http:\\google.com";
			link.Validation.ValidateGCL_URL();
			AssertHasErrors(link.GCL_URLInfo);

			link.GCL_URL = "http://google.com";
			link.Validation.ValidateGCL_URL();
			AssertNoErrors(link.GCL_URLInfo);

			link.GCL_URL = "https://gmail.google.com";
			link.Validation.ValidateGCL_URL();
			AssertNoErrors(link.GCL_URLInfo);
		}
	}
}
