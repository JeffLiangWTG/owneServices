using System;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class SubscribePreferencePageTest : ZPageTestCase
	{
		SubscribePreferenceForTest SubscribePreferencePage => (SubscribePreferenceForTest)Page;
		OrgContact Contact;
		public void TestSubscriptionPreferenceFromCampaign()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidData().ToString());
			SubscribePreferencePage.ForceLoad();
			SubscribePreferencePage.SubmitSubscriptionPreference();
			AssertEquals("You have successfully changed your subscription preferences.", SubscribePreferencePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/success.png", SubscribePreferencePage.ResultImageSrc);
			var emailAddress = GlbEmailAddress.LoadOrNew(Factory, Contact.Email);
			AssertEquals("Contact should be verified.", EmailDeliveryReportStatus.Codes.ValidReport, emailAddress.GI_DeliveryStatus);
		}

		public void TestExpiredSubscriptionPreferenceFromCampaign()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidData(true).ToString());
			SubscribePreferencePage.ForceLoad();

			AssertEquals("This URL is invalid or your session has expired", SubscribePreferencePage.SubscriptionPreferenceLabelText);
		}

		public void TestNullContact()
		{
			var queryString = new SecureQueryString();
			queryString.Add("OFT", "Only For Test");
			queryString.ExpireTime = TimeSpan.FromHours(1);
			Page.Request.QueryString.Add("data", queryString.ToString());
			Page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			SubscribePreferencePage.ForceLoad();

			AssertEquals("This page is unavailable for support login", SubscribePreferencePage.SubscriptionPreferenceLabelText);
		}

		public void TestSubscriptionPreferenceFromPublishedListCode()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidDataForPublishedListCode().ToString());
			SubscribePreferencePage.ForceLoad();
			SubscribePreferencePage.SubmitSubscriptionPreference();
			AssertEquals("You have successfully changed your subscription preferences.", SubscribePreferencePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/success.png", SubscribePreferencePage.ResultImageSrc);
		}

		public void TestExpiredSubscriptionPreferenceFromPublishedListCode()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithValidDataForPublishedListCode(true).ToString());
			SubscribePreferencePage.ForceLoad();

			AssertEquals("This URL is invalid or your session has expired", SubscribePreferencePage.SubscriptionPreferenceLabelText);
		}

		public void TestPreventOldVersionLinkAccess()
		{
			var page = (SubscribePreferenceForTest)Page;
			var queryString = new SecureQueryString();
			AssertEquals("The test query's expire should be equla to 2079,6,6", true, queryString.AbsoluteExpireTime.Equals(new DateTime(2079, 6, 6)));

			page.Request.QueryString.Add("data", queryString.ToString());
			AssertEquals("For old version link, GetNewDataSource should return null", true, page.GetNewDataSourceExposed() == null);

			page.Request.QueryString.Clear();

			var queryString2 = NewQueryStringWithValidData();
			queryString2.ExpireTime = TimeSpan.FromMinutes(20);

			page.Request.QueryString.Add("data", queryString2.ToString());
			AssertEquals("For link which owns valide expire time, GetNewDataSource's return should not be null", false, page.GetNewDataSourceExposed() == null);
		}

		public void TestWebCampaignUrlNeverExpireIfNoExpirationTime()
		{
			var page = (SubscribePreferenceForTest)Page;

			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();
			var queryStringWithValidCampaignItem = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = item.PK.ToString()
			};

			page.Request.QueryString.Add("data", queryStringWithValidCampaignItem.ToString());
			Assert(SecureQueryStringHelper.IsQueryNeverExpire(queryStringWithValidCampaignItem));
			AssertNotNull("Valid WebCampaign should never expire", page.GetNewDataSourceExposed());

			page.Request.QueryString.Clear();

			var invalidPK = ZGuid.NewZGuid();
			var queryStringWithInvalidCampaignItem = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = invalidPK.ToString()
			};

			page.Request.QueryString.Add("data", queryStringWithInvalidCampaignItem.ToString());
			Assert(SecureQueryStringHelper.IsQueryNeverExpire(queryStringWithInvalidCampaignItem));
			AssertNull(Factory.LoadTop1<GlbCompanyCampaignItem>(new ZQuery(GlbCompanyCampaignItemSchema.PK, invalidPK)));
			AssertNull("Invalid WebCampaign should not have data source", (page.GetNewDataSourceExposed() as SubscriptionPreferenceBusinessObjectAdapter).ContactCampaignItem);
		}

		SecureQueryString NewQueryStringWithValidData(bool expired = false)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var subcriptionRules = GetValidRegistryValues();
			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, subcriptionRules);
			campaign.G0_PublishedListCode = OrganisationsDataRegistry.Instance.SubscriptionRules.Value.DefaultCRM.Code;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			Contact = Factory.NewWithValidTestData<OrgContact>();
			Contact.OC_Email = $"{nameof(Contact)}@ema.il";
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Contact.PK;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString()
			};
			if (expired)
			{
				queryString.ExpireTime = TimeSpan.FromMinutes(-1);
			}
			Factory.Save();
			return queryString;
		}

		SecureQueryString NewQueryStringWithValidDataForPublishedListCode(bool expired = false)
		{
			var subcriptionRules = GetValidRegistryValues();
			OrganisationsDataRegistry.Instance.SubscriptionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, subcriptionRules);
			ZString publishedListCode = OrganisationsDataRegistry.Instance.SubscriptionRules.Value.DefaultCRM.Code;
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = $"{nameof(contact)}@ema.il";
			var queryString = new SecureQueryString
			{
				[OrgContactSchema.Constants.PK] = contact.PK.ToString(),
				[UnsubscribeUrlHelper.PublishedListCodeKey] = publishedListCode
			};
			if (expired)
			{
				queryString.ExpireTime = TimeSpan.FromMinutes(-1);
			}
			else
			{
				queryString.ExpireTime = TimeSpan.FromMinutes(20);
			}
			Factory.Save();
			return queryString;
		}

		protected SubscriptionRuleCollection GetValidRegistryValues()
		{
			var rules1 = new SubscriptionRuleCollection();
			var rule1 = rules1.AddNewRule("CD1", (NoResString)"Default Pubished List AAA", false, false, new string[] { "PRINT;SLT30;DESC1;SUM1", "RADIO;PREAP;DESC2;SUM2" });
			rule1.IsDefault = true;
			return rules1;
		}

		public void TestErrorResult()
		{
			Page.Request.QueryString.Add("data", NewQueryStringWithInvalidData().ToString());
			SubscribePreferencePage.ForceLoad();

			AssertEquals("A subscription preference list could not be found.", SubscribePreferencePage.ResultMessageLabelText);
			AssertEquals("UnsubscribeImages/error.png", SubscribePreferencePage.ResultImageSrc);
		}

		SecureQueryString NewQueryStringWithInvalidData()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_PublishedListCode = "XXX";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = $"{nameof(contact)}@ema.il";
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString()
			};

			Factory.Save();
			return queryString;
		}

		public void TestValidQueryStringForSubscriptionPreference()
		{
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			var queryString = new SecureQueryString
			{
				[GlbCompanyCampaignItemSchema.Constants.PK] = item.PK.ToString()
			};
			SubscribePreferencePage.Request.QueryString.Add("data", queryString.ToString());
			AssertNotNull(SubscribePreferencePage.GetNewDataSourceExposed());
			Assert(string.IsNullOrEmpty(SubscribePreferencePage.SubscriptionPreferenceLabelText));

			SubscribePreferencePage.Request.QueryString.Clear();
			SubscribePreferencePage.Request.QueryString.Add("data", "invalidString");
			SubscribePreferencePage.ForceLoad();

			AssertNull(SubscribePreferencePage.GetNewDataSourceExposed());
			AssertEquals("This URL is invalid or your session has expired", SubscribePreferencePage.SubscriptionPreferenceLabelText);
		}

		protected override ZPage GetNewZPage() => new SubscribePreferenceForTest();

		sealed class SubscribePreferenceForTest : SubscribePreference
		{
			public SubscribePreferenceForTest()
			{
				ResultMessageLabel = new ZTextLabel { BindTo = "ResultMessage" };
				ResultImage = new HtmlImage();
				SubscriptionPreferenceDescLabel = new ZTextLabelNoEncode();
				SubscriptionPreferenceLabel = new ZTextLabel();
				SubscriptionResultContainer = new HtmlGenericControl();
				SubscriptionListContainer = new HtmlGenericControl();
				SubscriptionPreferencesRepeater = new ZRepeater();
				SubmitButton = new ZButton();
				Controls.Add(ResultMessageLabel);
				Controls.Add(ResultImage);
				Controls.Add(SubscriptionPreferenceDescLabel);
				Controls.Add(SubscriptionPreferenceLabel);
				Controls.Add(SubscriptionResultContainer);
				Controls.Add(SubscriptionListContainer);
				Controls.Add(SubscriptionPreferencesRepeater);
				Controls.Add(SubmitButton);
			}

			public string SubscriptionPreferenceLabelText => SubscriptionPreferenceLabel.Text;
			public string ResultMessageLabelText => ResultMessageLabel.Text;
			public string ResultImageSrc => ResultImage.Src;
			internal void ForceLoad()
			{
				OnLoad(EventArgs.Empty);
			}
			public void SubmitSubscriptionPreference()
			{
				foreach (SubscriptionProperties sub in DataSourceGlbCompanyCampaignUnsubscribe.SubscriptionList)
				{
					sub.IsSubscribed = false;
					DataSourceGlbCompanyCampaignUnsubscribe.Process(sub);
				}
				ShowResult();
			}

			protected override void ShowResult()
			{
				ResultImage.Src = DataSourceGlbCompanyCampaignUnsubscribe.IsErrorResult
					? ErrorImagePath
					: SuccessImagePath;
				ResultImage.Visible = true;
				ResultMessageLabel.Text = DataSourceGlbCompanyCampaignUnsubscribe.ResultMessage;
			}

			public BusinessObject GetNewDataSourceExposed()
			{
				return GetNewDataSource();
			}
		}
	}
}
