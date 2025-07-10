
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ClickStatData))]
	sealed class ClickStatDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestViewInChart()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			var click = new ClickStatData(link);
			AssertEquals(true, click.ViewInChart_ReadOnly);

			click.Clicks = 1;
			AssertEquals(false, click.ViewInChart_ReadOnly);
		}

		public void TestClicks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			var click = new ClickStatData(link);
			AssertEquals(true, click.Clicks_ReadOnly);
		}

		public void TestUniqueClicks()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			var uniqueClick = new ClickStatData(link);
			AssertEquals(true, uniqueClick.UniqueClicks_ReadOnly);
		}

		public void TestCampaignRecipients()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "ZZZ";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "A";
			contact1.OC_Title = "Mr";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "B";
			contact2.OC_Title = "MA";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Invalid Name";
			contact3.OC_Email = "x@abc.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			Factory.Save();
			var clickStatData = new ClickStatData(campaign);
			AssertEquals(1, clickStatData.CampaignRecipients);

			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			var item3 = campaign.CampaignsItemsSent.AddNew();
			item3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3.G8_RecipientID = contact3.PK;
			item3.G8_TrackingStatus = TrackingStatusCodes.Codes.NDR;
			Factory.Save();
			var clickStatData2 = new ClickStatData(campaign);
			AssertEquals("CTR should exclude Bounced mails", 2, clickStatData2.CampaignRecipients);
		}

		public void TestContext()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_Context = "AAA";
			link.GCL_URL = "http://abc.net";

			var click1 = new ClickStatData(link);
			AssertEquals("AAA", click1.Context);
			click1.Context = "A11";
			AssertEquals("A11", link.GCL_Context);

			var click2 = new ClickStatData(campaign);
			AssertEquals("", click2.Context);
			click2.Context = "BBB";
			AssertEquals(2, campaign.TrackedLinks.Count);
			AssertEquals("BBB", campaign.TrackedLinks[1].GCL_Context);

			campaign.TrackedLinks[1].GCL_URL = "http://abc.net";

			AssertEquals(true, click1.Context_ReadOnly);
			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_GCL = link.PK;
			Factory.Save();
			AssertEquals(true, click1.Context_ReadOnly);
		}

		public void TestURL()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.net";

			var click1 = new ClickStatData(link);
			AssertEquals("http://webtest.net", click1.URL);
			click1.URL = "http://webtest.net.au";
			AssertEquals("http://webtest.net.au", link.GCL_URL);

			var click2 = new ClickStatData(campaign);
			AssertEquals("", click2.URL);
			click2.URL = "http://webtest.com";
			AssertEquals(2, campaign.TrackedLinks.Count);
			AssertEquals("http://webtest.com", campaign.TrackedLinks[1].GCL_URL);

			AssertEquals(true, click1.URL_ReadOnly);
			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_GCL = link.PK;
			Factory.Save();
			AssertEquals(true, click1.URL_ReadOnly);
		}

		public void TestTrackedUrl()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.net";

			var click1 = new ClickStatData(link);
			AssertEquals("", click1.TrackedUrl);

			Factory.Save();
			AssertEquals(link.TrackedUrl, click1.TrackedUrl);

			link.GCL_URL = "http:\\webtest.net";
			AssertEquals("", click1.TrackedUrl);
		}

		public void TestCanDelete()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://abc.net";
			Factory.Save();

			var click = new ClickStatData(link);
			AssertEquals(true, click.CanDelete);

			var campaignClick = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			campaignClick.GCC_GCL = link.PK;
			Factory.Save();

			AssertEquals(false, click.CanDelete);
		}

		public void TestDelete()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			var click = new ClickStatData(link);

			click.Delete();
			AssertEquals(true, link.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			var click = new ClickStatData(link);
			return click;
		}
	}
}
