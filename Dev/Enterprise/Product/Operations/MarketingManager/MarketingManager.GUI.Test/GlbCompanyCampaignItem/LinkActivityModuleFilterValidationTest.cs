using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	sealed class LinkActivityModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			FilterBusinessObject = new GlbCompanyCampaignItemFilterBusinessObject(Campaign);
		}

		public void TestValidateTypeProperty_Context()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			ContextLinkActivityModuleFilter filter = new ContextLinkActivityModuleFilter("Has Context Activity", FilterBusinessObject, Campaign);
			var validation = new LinkActivityModuleFilterValidation(filter);

			filter.TypeProperty = "";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);

			filter.TypeProperty = "XXX";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, true);

			filter.TypeProperty = "Yahoo Plus";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);
		}

		public void TestValidateTypeProperty_DestinationURL()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHCD";

			var link = Campaign.TrackedLinks.AddNew();
			link.GCL_URL = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			link.GCL_Context = "Google Images";
			link.GCL_IsImage = true;

			var link2 = Campaign.TrackedLinks.AddNew();
			link2.GCL_URL = "http://www.yahoomail.com";
			link2.GCL_Context = "Yahoo Plus";
			link2.GCL_IsImage = false;

			var contact = org.Contacts.AddNew();

			var campaignItem = Campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_G8_Recipient = campaignItem.PK;
			campaignClick.GCC_GCL = link.PK;
			campaignClick.GCC_ClickTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			DestinationURLLinkActivityModuleFilter filter = new DestinationURLLinkActivityModuleFilter("Has Destination URL Activity", FilterBusinessObject, Campaign);
			var validation = new LinkActivityModuleFilterValidation(filter);

			filter.TypeProperty = "";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);

			filter.TypeProperty = "XXX";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, true);

			filter.TypeProperty = "https://www.lanl.gov/careers/_assets/images/icon-lego.jpg";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);
		}

		#region LinkActivityModuleFilter

		GlbCompanyCampaignItemFilterBusinessObject FilterBusinessObject;
		GlbCompanyCampaign Campaign;

		#endregion
	}
}
