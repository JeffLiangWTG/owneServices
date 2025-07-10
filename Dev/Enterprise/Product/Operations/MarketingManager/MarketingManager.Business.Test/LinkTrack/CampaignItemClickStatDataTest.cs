using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignItemClickStatData))]
	sealed class CampaignItemClickStatDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContext()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			var link = campaign.TrackedLinks.AddNew();
			link.GCL_Context = "AAA";
			link.GCL_URL = "http://abc.net";

			var click1 = new CampaignItemClickStatData(Factory, link.GCL_Context, link.GCL_URL);
			AssertEquals("AAA", click1.Context);
			link.GCL_Context = "A11";
			AssertEquals("A11", link.GCL_Context);

			var click2 = new CampaignItemClickStatData(Factory, "", "http://abc.net");
			AssertEquals("", click2.Context);
			link.GCL_Context = "BBB";
			AssertEquals("", click2.Context);
			AssertEquals(1, campaign.TrackedLinks.Count);
			AssertEquals("BBB", campaign.TrackedLinks[0].GCL_Context);

			campaign.TrackedLinks[0].GCL_URL = "http://abc.net";
		}

		public void TestURL()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.net";

			var click1 = new CampaignItemClickStatData(Factory, link.GCL_Context, link.GCL_URL);
			AssertEquals("http://webtest.net", click1.URL);
			link.GCL_URL = "http://webtest.net.au";
			AssertEquals("http://webtest.net.au", link.GCL_URL);

			var click2 = new CampaignItemClickStatData(Factory, link.GCL_Context, "http://abc.net");
			AssertEquals("http://abc.net", click2.URL);
			link.GCL_URL = "http://webtest.com";
			AssertEquals("http://abc.net", click2.URL);
			AssertEquals(1, campaign.TrackedLinks.Count);
			AssertEquals("http://webtest.com", campaign.TrackedLinks[0].GCL_URL);
		}

		[TestDate(2015, 5, 11)]
		public void TestFirstClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.net";

			var click1 = new CampaignItemClickStatData(Factory, link.GCL_Context, link.GCL_URL);
			click1.FirstClick = ZDateTime.UtcNow;
			AssertEquals(new ZDateTime(2015, 5, 11), click1.FirstClick);
		}

		public void TestLastClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://webtest.net";

			var click1 = new CampaignItemClickStatData(Factory, link.GCL_Context, link.GCL_URL);
			click1.LastClick = "3 day(s) ago";
			AssertEquals("3 day(s) ago", click1.LastClick);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var link = campaign.TrackedLinks.AddNew();
			var click = new CampaignItemClickStatData(Factory, link.GCL_Context, link.GCL_URL);
			return click;
		}
	}
}
