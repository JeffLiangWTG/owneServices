using System;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyUrlHelperTest : TestCaseWithFactory
	{
		public void TestGetCampaignPreviewUrl()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			string expectedURL = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedURL, VoteExamSurveyUrlHelper.GetCampaignPreviewUrl(campaign));
		}

		public void TestGetCampaignUrl_ExistingCampaignItem()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString();
			string expectedURL = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedURL, VoteExamSurveyUrlHelper.GetCampaignUrl(campaignItem));

			campaignItem.CurrentQuestionsCountryCode = "AU";
			queryString[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "AU";
			expectedURL = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedURL, VoteExamSurveyUrlHelper.GetCampaignUrl(campaignItem));
		}

		public void TestGetCampaignUrl_CompanySpecific()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_GC = company1.PK;
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignItemSchema.Constants.PK] = campaignItem.PK.ToString();

			var expectedURL = string.Format("http://company1.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedURL, VoteExamSurveyUrlHelper.GetCampaignUrl(campaignItem));

			campaign.G0_GC = company2.PK;
			expectedURL = string.Format("http://company2.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedURL, VoteExamSurveyUrlHelper.GetCampaignUrl(campaignItem));
		}

		public void TestGetCampaignUrl_NewCampaignItem()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			OrgContact contact = Factory.New<OrgContact>();

			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "ID";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			string expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, VoteExamSurveyUrlHelper.GetCampaignUrl(campaign, "ID", new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix)));
		}

		public void TestGetCampaignUrl_TestExpiry()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			OrgContact contact = Factory.New<OrgContact>();

			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "ID";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString[VoteExamSurveyUrlHelper.TestExpiryDateStringKey] = new ZDate(2009, 10, 30).ToJulianDateString();
			string expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, VoteExamSurveyUrlHelper.GetCampaignUrl(campaign, "ID", new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), new ZDate(2009, 10, 30)));
		}

		public void TestGetCampaignUrl_JobSkillCode()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			OrgContact contact = Factory.New<OrgContact>();

			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "ID";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString[VoteExamSurveyUrlHelper.TestExpiryDateStringKey] = new ZDate(2009, 10, 30).ToJulianDateString();
			queryString[VoteExamSurveyUrlHelper.JobSkillCodeStringKey] = "LOL";
			queryString[VoteExamSurveyUrlHelper.LanguageStringKey] = "ENG";
			string expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, VoteExamSurveyUrlHelper.GetCampaignUrl(campaign, "ID", new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), new ZDate(2009, 10, 30), "LOL", "ENG"));
		}

		public void TestGetCampaignUrl_ExamSettingsCode()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			OrgContact contact = Factory.New<OrgContact>();

			SecureQueryString queryString = new SecureQueryString();
			queryString[GlbCompanyCampaignSchema.Constants.PK] = campaign.PK.ToString();
			queryString[RefCountrySchema.Constants.Prefix] = "ID";
			queryString[VoteExamSurveyUrlHelper.RecipientIDQueryStringKey] = contact.PK.ToString();
			queryString[VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey] = OrgContactSchema.Constants.Prefix;
			queryString[VoteExamSurveyUrlHelper.TestExpiryDateStringKey] = new ZDate(2009, 10, 30).ToJulianDateString();
			queryString[VoteExamSurveyUrlHelper.JobSkillCodeStringKey] = "LOL";
			queryString[VoteExamSurveyUrlHelper.LanguageStringKey] = "ENG";
			queryString[VoteExamSurveyUrlHelper.ExamSettingsStringKey] = "WOW";
			string expectedUrl = string.Format("http://mehmehserver.webvoting.com.au/login.aspx?{0}={1}", VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			AssertEquals(expectedUrl, VoteExamSurveyUrlHelper.GetCampaignUrl(campaign, "ID", new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), new ZDate(2009, 10, 30), "LOL", "ENG", "WOW"));
		}

		public void TestNoWebCampaignUrl()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, string.Empty);
			var campaign = Factory.New<GlbCompanyCampaign>();
			var contact = Factory.New<OrgContact>();

			AssertExceptionThrown(
				"Fail uri message",
				typeof(UriFormatException),
				"Can't create Web Campaign link. Please verify the value of the registry item 'Web and Visibility -> Web Component URLs -> WebCampaign URL' for the Company Eagle Datamation International.",
				() => VoteExamSurveyUrlHelper.GetCampaignUrl(campaign, "ID", new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), new ZDate(2009, 10, 30), "LOL", string.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();

			company1 = Factory.New<GlbCompany>();
			company2 = Factory.New<GlbCompany>();

			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmehserver.webvoting.com.au/");
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://company1.webvoting.com.au/");
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://company2.webvoting.com.au/");
		}

		GlbCompany company1;
		GlbCompany company2;
	}
}
