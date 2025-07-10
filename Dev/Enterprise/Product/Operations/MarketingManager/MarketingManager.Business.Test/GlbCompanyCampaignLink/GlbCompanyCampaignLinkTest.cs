using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignLink))]
	sealed class GlbCompanyCampaignLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUnicodeUrl()
		{
			var link = (GlbCompanyCampaignLink)base.GetNewBusinessObject();
			string url = "http://www.wisetechglobal.com/default.htm";
			link.UnicodeUrl = url;
			AssertEquals(url, link.UnicodeUrl);
			AssertEquals("http://www.wisetechglobal.com/default.htm", link.GCL_URL);

			url = "http://ko.wikipedia.org/wiki/위키백과:대문";
			link.UnicodeUrl = url;
			AssertEquals(url, link.UnicodeUrl);
			AssertEquals("http://ko.wikipedia.org/wiki/%ec%9c%84%ed%82%a4%eb%b0%b1%ea%b3%bc:%eb%8c%80%eb%ac%b8", link.GCL_URL);

			url = "http://ko.wikipedia.org/wiki/위키백과:대문?&a=%B1";
			link.UnicodeUrl = url;
			AssertEquals("query string not encoded/decoded", url, link.UnicodeUrl);
			AssertEquals("http://ko.wikipedia.org/wiki/%ec%9c%84%ed%82%a4%eb%b0%b1%ea%b3%bc:%eb%8c%80%eb%ac%b8?&a=%B1", link.GCL_URL);
		}

		public void TestTrackedUrl()
		{
			var link = (GlbCompanyCampaignLink)base.GetNewBusinessObject();
			link.GCL_URL = "http://www.wisetechglobal.com/default.htm";
			link.GCL_Context = "WiseTech Global";

			string expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + link.GCL_URL + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N");
			AssertEquals(expected, link.TrackedUrl);

			link.GCL_URL = "http://www.wisetechglobal.com/default.htm#play-video";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://www.wisetechglobal.com/default.htm" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&f=" + "play-video";
			AssertEquals(expected, link.TrackedUrl);

			link.GCL_URL = "http://www.wisetechglobal.com/default.htm?f=123456#play-video";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://www.wisetechglobal.com/default.htm?f=123456" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&f=" + "play-video";
			AssertEquals(expected, link.TrackedUrl);

			link.GCL_URL = "http://www.wisetechglobal.com/default.htm#";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://www.wisetechglobal.com/default.htm" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&f=";
			AssertEquals(expected, link.TrackedUrl);

			link.GCL_URL = "http://www.wisetechglobal.com/default.htm#happy&c=123";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://www.wisetechglobal.com/default.htm" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&f=" + "happy%26c%3D123";
			AssertEquals(expected, link.TrackedUrl);

			link.UnicodeUrl = "http://ko.wikipedia.org/wiki/위키백과:대문";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://ko.wikipedia.org/wiki/%ec%9c%84%ed%82%a4%eb%b0%b1%ea%b3%bc:%eb%8c%80%eb%ac%b8" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N");
			AssertEquals(expected, link.TrackedUrl);

			link.UnicodeUrl = "http://ko.wikipedia.org/wiki/위키백과:대문#도움말";
			expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + "http://ko.wikipedia.org/wiki/%ec%9c%84%ed%82%a4%eb%b0%b1%ea%b3%bc:%eb%8c%80%eb%ac%b8" + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&f=" + "%25eb%258f%2584%25ec%259b%2580%25eb%25a7%2590";
			AssertEquals(expected, link.TrackedUrl);
		}

		public void TestGenerateTrackedUrl()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item = campaign.CampaignsItemsSent.AddNew();

			var link = campaign.TrackedLinks.AddNew();
			link.GCL_URL = "http://www.wisetechglobal.com/default.htm";
			link.GCL_Context = "WiseTech Global";

			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "Head Office";
			link2.GCL_URL = "http://www.wisetechglobal.com/default.htm";

			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_Context = "URL with Macro";
			link3.GCL_URL = "http://www.wisetechglobal.com/(*CampaignID*).html";

			string expected = OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value
				+ "?" + link.GCL_URL + "&c=" + EnvProxy.Instance.CurrentCompany.GetLicenceCode() + "&x=" + link.PK.ToGuid().ToString("N") + "&u=" + item.PK.ToGuid().ToString("N");
			AssertEquals(expected, link.GenerateTrackedUrl(item));
			AssertEquals(expected, link.GenerateTrackedUrl(item.PK));

			AssertEquals(expected, GlbCompanyCampaignLink.GenerateTrackedUrl(item, link.GCL_Context, link.GCL_URL));
			AssertEquals(expected, GlbCompanyCampaignLink.GenerateTrackedUrl(item, "wisetech global", link.GCL_URL));

			AssertEquals(link2.GenerateTrackedUrl(item), GlbCompanyCampaignLink.GenerateTrackedUrl(item, link2.GCL_Context, link2.GCL_URL));
			AssertEquals("context name only", link2.GenerateTrackedUrl(item), GlbCompanyCampaignLink.GenerateTrackedUrl(item, link2.GCL_Context, ""));

			AssertContains("http://www.wisetechglobal.com/AAABBBCCC.html", GlbCompanyCampaignLink.GenerateTrackedUrl(item, link3.GCL_Context, "http://www.wisetechglobal.com/AAABBBCCC.html"));
		}

		public void TestGCL_Context_ReadOnly()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			AssertEquals(true, link.GCL_ContextInfo.ReadOnly);

			link.HasTrackingMacro = true;
			AssertEquals(false, link.GCL_ContextInfo.ReadOnly);
		}

		public void TestUrl_ReadOnly()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			AssertEquals(false, link.UnicodeUrlInfo.ReadOnly);

			link.GCL_IsTracked = true;
			AssertEquals(true, link.GCL_URLInfo.ReadOnly);
			AssertEquals(true, link.UnicodeUrlInfo.ReadOnly);
		}

		public void TestHasTrackingMacro_ReadOnly()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var link = campaign.TrackedLinks.AddNew();
			AssertEquals(false, link.HasTrackingMacroInfo.ReadOnly);

			link.GCL_IsTracked = true;
			AssertEquals(true, link.HasTrackingMacroInfo.ReadOnly);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = (GlbCompanyCampaignLink)base.GetNewBusinessObject();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			bizo.GCL_G0_Campaign = campaign.PK;
			return bizo;
		}

		#endregion
	}
}
