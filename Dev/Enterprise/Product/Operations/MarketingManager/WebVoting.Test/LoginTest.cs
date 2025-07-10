using System;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting
{
	class LoginTest : ZPageTestCase
	{
		public void TestRedirectIfExamHasMigrated_CampaignItem_NoRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertNotEquals("Should not be redirected to ContentMoved.aspx as there's no registry setting", LoginPage.ContentMovedPageInternal, LoginPage.Response.RedirectLocation);
		}

		public void TestRedirectIfExamHasMigrated_Campaign_NoRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaignItem.G8_G0.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Should not be redirected to ContentMoved.aspx as there's no registry setting", null, LoginPage.Response.RedirectLocation);
		}

		public void TestRedirectIfExamHasMigrated_CampaignItem_FalseRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			var collection = new CodeDescriptionBoolCollection(50);
			collection.Add(campaignItem.CompanyCampaign.G0_CampaignID, (NoResString)"test exam", false);
			RecruiterDataRegistry.Instance.ExamMigrationStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertNotEquals("Should not be redirected as the registry is false", LoginPage.ContentMovedPageInternal, LoginPage.Response.RedirectLocation);
		}

		public void TestRedirectIfExamHasMigrated_Campaign_FalseRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			var collection = new CodeDescriptionBoolCollection(50);
			collection.Add(campaignItem.CompanyCampaign.G0_CampaignID, (NoResString)"test exam", false);
			RecruiterDataRegistry.Instance.ExamMigrationStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaignItem.G8_G0.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Should not be redirected as the registry is false", null, LoginPage.Response.RedirectLocation);
		}

		public void TestRedirectIfExamHasMigrated_CampaignItem_TrueRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			var collection = new CodeDescriptionBoolCollection(50);
			collection.Add(campaignItem.CompanyCampaign.G0_CampaignID, (NoResString)"test exam", true);
			RecruiterDataRegistry.Instance.ExamMigrationStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Should be redirected to the ContentMovedPage", LoginPage.ContentMovedPageInternal, LoginPage.Response.RedirectLocation);
		}

		public void TestRedirectIfExamHasMigrated_Campaign_TrueRegistry()
		{
			var campaignItem = CreateCampaignItem("LCT");
			Factory.Save();

			var collection = new CodeDescriptionBoolCollection(50);
			collection.Add(campaignItem.CompanyCampaign.G0_CampaignID, (NoResString)"test exam", true);
			RecruiterDataRegistry.Instance.ExamMigrationStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignSchema.Constants.PK, campaignItem.PK.ToString());
			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaignItem.G8_G0.ToString());

			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Should be redirected to the ContentMovedPage", LoginPage.ContentMovedPageInternal, LoginPage.Response.RedirectLocation);
		}

		void AddQueryString(string key, string value)
		{
			AddQueryString(LoginPage, key, value);
		}

		void AddQueryString(Login page, string key, string value)
		{
			var secureQueryStringData = page.Request[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
			var queryString = new SecureQueryString(secureQueryStringData);
			queryString.Remove(SecureQueryString.TimeStampKey);
			queryString.Add(key, value);
			page.Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey] = queryString.ToString();
		}

		GlbCompanyCampaignItem CreateCampaignItem(ZString campaignType)
		{
			var campaign = CreateCampaign(campaignType);
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.FillWithValidTestData();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			return campaignItem;
		}

		GlbCompanyCampaign CreateCampaign(ZString campaignType)
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.FillWithValidTestData();
			campaign.G0_BroadcastVoteSurveyExam = campaignType;
			return campaign;
		}

		public void TestAuthenticateCampaignParticipantOnPageLoad()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.FillWithValidTestData();
			OrgContact participant = Factory.NewWithValidTestData<OrgContact>();
			participant.OC_Email = "testemail@emailtest.com.au";
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = participant.PK;
			Factory.Save();

			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("", LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(true, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		public void TestAuthenticateCampaignParticipant_NoCampaignItemPKInQueryString()
		{
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Either your session has expired or the campaign URL has been manually altered.", LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(false, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		public void TestAuthenticateCampaignParticipant_NoCampaignItem()
		{
			SecureQueryString secureQueryString = new SecureQueryString();
			ZGuid testGuid = ZGuid.NewZGuid();
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, testGuid.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Either your session has expired or the campaign URL has been manually altered.", LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(false, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		public void TestAuthenticateCampaignParticipant_CampaignIsAlreadyClosed()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_ActualCompletedDate = new ZDateTime(2006, 1, 9);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.FillWithValidTestData();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem.RecipientAsOrgContact.OC_Email = "testemail@emailtest.com.au";
			Factory.Save();

			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("This campaign has ended.", LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(false, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		[TestDate(2006, 1, 10)]
		public void TestAuthenticateCampaignParticipant_CampaignItemHasAlreadyBeenSubmittedPreviously()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.FillWithValidTestData();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem.RecipientAsOrgContact.OC_Email = "testemail@emailtest.com.au";
			campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
			Factory.Save();

			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("You have previously submitted your answers. Only one submission is allowed per participant.", LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(false, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		public void TestAuthenticateForPreview()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			SecureQueryString secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignSchema.Constants.PK, campaign.PK.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals(string.Empty, LoginPage.ErrorMessageLabelInternal.Text);
			AssertEquals(true, LoginPage.CampaignDetailRepeaterInternal.Visible);
		}

		public void TestExamLandingPageFooterText()
		{
			RecruiterDataRegistry.Instance.ExamLandingPageFooterText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "This is a test...");

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = "LCT";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.FillWithValidTestData();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem.RecipientAsOrgContact.OC_Email = "testemail@emailtest.com.au";
			Factory.Save();

			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			Page.Request.QueryString.Add("data", secureQueryString.ToString());
			if (LoginPage.FooterTextForExamLiteralInternal == null)
			{
				LoginPage.SetFooterTextForExamLiteralInternal(new Literal());
			}
			LoginPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("This is a test...", LoginPage.FooterTextForExamLiteralInternal.Text);
		}

		LoginPageForTest LoginPage
		{
			get { return (LoginPageForTest)Page; }
		}

		protected override ZPage GetNewZPage()
		{
			LoginPageForTest result = new LoginPageForTest();
			result.SetGroupedExamCampaignSummaryInternal(new Panel());
			result.SetGroupedExamPanelInternal(new Panel());
			result.Setform1Internal(new System.Web.UI.HtmlControls.HtmlForm());
			result.SetErrorMessageLabelInternal(new ZTextLabel());
			result.ErrorMessageLabelInternal.BindTo = "ErrorMessage";
			result.Controls.Add(result.ErrorMessageLabelInternal);
			result.SetCampaignDetailRepeaterInternal(new ZRepeater());
			result.CampaignDetailRepeaterInternal.BindTo = "VoteExamSurveyDetails";
			result.Controls.Add(result.CampaignDetailRepeaterInternal);
			return result;
		}

		class LoginPageForTest : Login
		{
			internal Literal FooterTextForExamLiteralInternal => FooterTextForExamLiteral;
			internal void SetFooterTextForExamLiteralInternal(Literal literal) => FooterTextForExamLiteral = literal;
			internal ZTextLabel ErrorMessageLabelInternal => ErrorMessageLabel;
			internal void SetErrorMessageLabelInternal(ZTextLabel label) => ErrorMessageLabel = label;
			internal ZRepeater CampaignDetailRepeaterInternal => CampaignDetailRepeater;
			internal void SetCampaignDetailRepeaterInternal(ZRepeater zrepeater) => CampaignDetailRepeater = zrepeater;
			internal void SetGroupedExamCampaignSummaryInternal(Panel panel) => GroupedExamCampaignSummary = panel;
			internal void SetGroupedExamPanelInternal(Panel panel) => GroupedExamPanel = panel;
			internal void Setform1Internal(System.Web.UI.HtmlControls.HtmlForm form) => form1 = form;
			internal string ContentMovedPageInternal => ContentMovedPage;
			internal void OnLoadInternal(EventArgs e) => OnLoad(e);
		}
	}
}
