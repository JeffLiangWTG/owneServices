using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignItemClickStatModel))]
	sealed class CampaignItemClickStatModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadClicks()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODDD";
			var contact = org.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";

			var clickTime = ZDateTime.UtcNow;

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;
			click11.GCC_G8_Recipient = campaignItem.PK;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link1.PK;
			click12.GCC_ClickTimeUtc = clickTime;
			click12.GCC_G8_Recipient = campaignItem.PK;

			Factory.Save();

			CampaignItemClickStatModel statModel = new CampaignItemClickStatModel(campaignItem);
			statModel.ReportBy = ReportByList.Codes.Context;

			AssertEquals(1, statModel.LinkClicks.Count);

			AssertEquals("AAA", statModel.LinkClicks[0].Context);
			AssertEquals(2, statModel.LinkClicks[0].Clicks);
		}

		public void TestLinkClicks_ShouldNotIncludeRegistryTrackingImageLink()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODDD";
			var contact = org.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = CampaignEmailTemplateEditor.TrackingImageContext;
			link2.GCL_URL = OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value;
			link2.GCL_IsImage = true;

			var clickTime = ZDateTime.UtcNow;

			var click1 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click1.GCC_GCL = link1.PK;
			click1.GCC_ClickTimeUtc = clickTime;
			click1.GCC_G8_Recipient = campaignItem.PK;

			var click2 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click2.GCC_GCL = link2.PK;
			click2.GCC_ClickTimeUtc = clickTime;
			click2.GCC_G8_Recipient = campaignItem.PK;

			var click3 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click3.GCC_GCL = link1.PK;
			click3.GCC_ClickTimeUtc = clickTime;
			click3.GCC_G8_Recipient = campaignItem.PK;

			Factory.Save();

			var statModel = new CampaignItemClickStatModel(campaignItem);
			statModel.ReportBy = ReportByList.Codes.Context;

			AssertEquals(1, statModel.LinkClicks.Count);

			AssertEquals("AAA", statModel.LinkClicks[0].Context);
			AssertEquals(2, statModel.LinkClicks[0].Clicks);
		}

		public void TestTotalUniqueOpens()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AACCE";
			org.OH_RL_NKClosestPort = "AUBNE";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@email.com";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_Email = "inquiry@gmail.com";
			inquiry.O1_PortOrCountry = "";

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = "OC";

			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientID = inquiry.PK;
			campaignItem2.G8_RecipientTableCode = "O1";

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";

			var clickTime = new ZDateTime(2015, 5, 21, 12, 12, 4);

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;
			click11.GCC_G8_Recipient = campaignItem.PK;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link1.PK;
			click12.GCC_ClickTimeUtc = clickTime.AddHours(4);
			click12.GCC_G8_Recipient = campaignItem.PK;

			var click13 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click13.GCC_GCL = link2.PK;
			click13.GCC_ClickTimeUtc = clickTime;
			click13.GCC_G8_Recipient = campaignItem2.PK;

			Factory.Save();

			CampaignItemClickStatModel statModel = new CampaignItemClickStatModel(campaignItem);
			statModel.ReportBy = ReportByList.Codes.Context;
			AssertEquals("2 unique day(s)", statModel.TotalUniqueOpensText);

			click12.GCC_ClickTimeUtc = clickTime.AddHours(1);
			Factory.Save();
			Factory.ClearCachedValue<ZInt>("CampaignItemClickModel.TotalUniqueOpens:" + campaignItem.PK);
			statModel = new CampaignItemClickStatModel(campaignItem);
			statModel.ReportBy = ReportByList.Codes.Context;
			AssertEquals("1 unique day(s)", statModel.TotalUniqueOpensText);

			statModel = new CampaignItemClickStatModel(campaignItem2);
			statModel.ReportBy = ReportByList.Codes.Context;
			ZDateTime dateReturned = statModel.GetDateTimeFromTimeZone(click13.GCC_ClickTimeUtc);
			AssertEquals(clickTime.AddHours(10), dateReturned);

			inquiry.O1_PortOrCountry = "USA2D";
			Factory.Save();
			Factory.ClearCachedValue<ZInt>();
			dateReturned = statModel.GetDateTimeFromTimeZone(click13.GCC_ClickTimeUtc);
			AssertEquals(clickTime.AddHours(-4), dateReturned);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			return new CampaignItemClickStatModel(campaignItem);
		}
	}
}
