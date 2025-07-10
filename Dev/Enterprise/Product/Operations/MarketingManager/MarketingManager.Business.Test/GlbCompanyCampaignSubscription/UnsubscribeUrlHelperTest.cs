using System;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class UnsubscribeUrlHelperTest : TestCaseWithFactory
	{
		public void TestGetUriFormatErrorMessage()
		{
			AssertEquals("Can't create Web Campaign link. Please verify the value of the registry item 'Web and Visibility -> Web Component URLs -> WebCampaign URL' for the Company Empty Company.", UnsubscribeUrlHelper.GetUriFormatErrorMessage(companyWithNoUrl));
			AssertEquals("Can't create Web Campaign link. Please verify the value of the registry item 'Web and Visibility -> Web Component URLs -> WebCampaign URL' for the Company .", UnsubscribeUrlHelper.GetUriFormatErrorMessage(null));
		}

		public void TestGetUnsubscribeUrlString_EmptyUrl()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = companyWithNoUrl.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			foreach (UnsubscribeType value in Enum.GetValues(typeof(UnsubscribeType)))
			{
				AssertExceptionThrown(typeof(UriFormatException), delegate
				{
					campaignItem.GetUnsubscribeUrlString(value);
				});
			}
		}

		public void TestGetUnsubscribeUrl_EmptyUrl()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = companyWithNoUrl.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			foreach (UnsubscribeType value in Enum.GetValues(typeof(UnsubscribeType)))
			{
				AssertExceptionThrown<UriFormatException>(delegate
				{ campaignItem.GetUnsubscribeUrl(value.ToString("G")); });
				AssertExceptionThrown<UriFormatException>(delegate
				{ campaignItem.GetUnsubscribeUrl(value.ToString("G"), isResubscribe: false); });
			}
		}

		public void TestGetUnsubscribeUrl()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = companyWithUrl.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			foreach (UnsubscribeType value in Enum.GetValues(typeof(UnsubscribeType)))
			{
				var queryString = new SecureQueryString
				{
					[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString(),
					[UnsubscribeUrlHelper.UnsubscribeTypeKey] = value.ToString("G"),
					[UnsubscribeUrlHelper.ResubscribeKey] = false.ToString()
				};
				var expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/unsubscribe.aspx?{0}={1}",
					VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));

				AssertEquals("Expected URL for Unsubscribe", expectedUrl, campaignItem.GetUnsubscribeUrl(value.ToString("G")));
				AssertEquals("Expected URL for Unsubscribe", expectedUrl, campaignItem.GetUnsubscribeUrl(value.ToString("G"), isResubscribe: false));
			}
		}

		public void TestGetUnsubscribeUrl_IsResubscribe()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = companyWithUrl.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			foreach (UnsubscribeType value in Enum.GetValues(typeof(UnsubscribeType)))
			{
				var queryString = new SecureQueryString
				{
					[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString(),
					[UnsubscribeUrlHelper.UnsubscribeTypeKey] = value.ToString("G"),
					[UnsubscribeUrlHelper.ResubscribeKey] = true.ToString()
				};
				var expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/unsubscribe.aspx?{0}={1}",
					VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));

				AssertEquals("Expected URL for Unsubscribe", expectedUrl, campaignItem.GetUnsubscribeUrl(value.ToString("G"), isResubscribe: true));
			}
		}

		public void TestGetUnsubscribeUrl_WithoutCampaignItem()
		{
			foreach (UnsubscribeType value in Enum.GetValues(typeof(UnsubscribeType)))
			{
				var queryString = new SecureQueryString
				{
					[GlbCompanyCampaignItemSchema.Constants.PK] = Guid.Empty.ToString(),
					[UnsubscribeUrlHelper.UnsubscribeTypeKey] = value.ToString("G"),
					[UnsubscribeUrlHelper.ResubscribeKey] = false.ToString()
				};
				var expectedUrl = string.Format("http://mehmehserver2.webvoting.com.au/unsubscribe.aspx?{0}={1}",
					VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));

				AssertEquals("Expected URL for Unsubscribe", expectedUrl, UnsubscribeUrlHelper.GetUnsubscribeUrl(null, value.ToString("G")));
			}
		}

		public void TestGetSubscriptionPreferenceUrl()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = companyWithUrl.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.org";

			SecureQueryString queryString1 = new SecureQueryString();
			queryString1[GlbCompanyCampaignItemSchema.PK.Name] = campaignItem.PK.ToString();
			var expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/subscribepreference.aspx?{0}={1}", UnsubscribeUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString1.ToString()));
			var url = campaignItem.GetSubscriptionPreferenceUrl();
			AssertEquals(expectedUrl, url);

			SecureQueryString queryString2 = new SecureQueryString();
			queryString2[OrgContactSchema.Constants.PK] = contact.PK.ToString();
			queryString2[UnsubscribeUrlHelper.PublishedListCodeKey] = "ABC";
			queryString2.ExpireTime = TimeSpan.FromMinutes(5);
			expectedUrl = string.Format("http://mehmehserver2.webvoting.com.au/subscribepreference.aspx?{0}={1}", UnsubscribeUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString2.ToString()));

			var url2 = UnsubscribeUrlHelper.GetSubscriptionPreferenceUrlString(contact.PK.ToGuid(), "ABC", 5);
			AssertEquals(expectedUrl, url2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyWithNoUrl = Factory.New<GlbCompany>();
			companyWithUrl = Factory.New<GlbCompany>();
			companyWithNoUrl.CompanyName = "Empty Company";
			companyWithUrl.CompanyName = "Test Company with URL";
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(companyWithNoUrl.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(companyWithUrl.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://mehmehserver.webvoting.com.au/");
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmehserver2.webvoting.com.au/");
		}

		GlbCompany companyWithNoUrl;
		GlbCompany companyWithUrl;
	}
}
