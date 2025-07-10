using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TrackingImageLinkTestHelper
	{
		public TrackingImageLinkTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
			SetupStatModelWithRegistryTrackingImageLink();
		}

		void SetupStatModelWithRegistryTrackingImageLink()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			Campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Link1 = Campaign.TrackedLinks.AddNew();
			Link1.GCL_URL = "http://webmail.org";
			Link1.GCL_Context = "A Mail";

			Link2 = Campaign.TrackedLinks.AddNew();
			Link2.GCL_URL = OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value;
			Link2.GCL_Context = CampaignEmailTemplateEditor.TrackingImageContext;
			Link2.GCL_IsImage = true;

			CampaignItem = Campaign.CampaignsItemsSent.AddNew();
			CampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			CampaignItem.G8_RecipientID = ZGuid.NewZGuid();

			var clickTime = ZDateTime.UtcNow;

			Click1 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			Click1.GCC_GCL = Link1.PK;
			Click1.GCC_G8_Recipient = CampaignItem.PK;
			Click1.GCC_ClickTimeUtc = clickTime;

			Click2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			Click2.GCC_GCL = Link1.PK;
			Click2.GCC_G8_Recipient = CampaignItem.PK;
			Click2.GCC_ClickTimeUtc = clickTime;

			Click3 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			Click3.GCC_GCL = Link2.PK;
			Click3.GCC_G8_Recipient = CampaignItem.PK;
			Click3.GCC_ClickTimeUtc = clickTime;

			Factory.Save();

			StatModel = new ClickStatModel(Campaign);
			StatModel.ReportBy = ReportByList.Codes.Context;
			StatModel.LoadClicks();
		}

		public GlbCompanyCampaign Campaign;
		public GlbCompanyCampaignLink Link1;
		public GlbCompanyCampaignLink Link2;
		public GlbCompanyCampaignItem CampaignItem;
		public GlbCompanyCampaignClick Click1;
		public GlbCompanyCampaignClick Click2;
		public GlbCompanyCampaignClick Click3;
		public ClickStatModel StatModel;
		public BusinessObjectFactory Factory;
	}
}
