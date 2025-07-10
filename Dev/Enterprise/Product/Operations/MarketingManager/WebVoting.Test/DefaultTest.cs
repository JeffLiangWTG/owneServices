using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting
{
	class DefaultTest : ZPageTestCase
	{
		public void TestQueryStringRetainsAllExtraParameters()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);

			DefaultPage.Request.QueryString["xxx"] = "more";
			Assert(DefaultPage.GetSubmissionRedirectParametersInternal().Contains("&xxx=more"));

			DefaultPage.Request.QueryString["yyy"] = "evenmore";
			Assert(DefaultPage.GetSubmissionRedirectParametersInternal().Contains("&xxx=more&yyy=evenmore"));
		}

		public void TestPageLoad_NoDataSource()
		{
			AssertNull("Pre-condition", DefaultPage.Response.RedirectLocation);

			DefaultPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("Should be redirected to the login page if DataSource is null", DefaultPage.AppInstance.LoginPage, DefaultPage.Response.RedirectLocation);
		}

		public void TestPageLoad_SuppressErrorDialog()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			Factory.Save();
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			Assert("Should be suppressed onLoad", DefaultPage.SupressErrorDialogInternal);
		}

		public void TestPageLoad_AddVoteUserControlToContentPlaceHolder()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			Factory.Save();
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			AssertEquals("Pre-condition", 0, DefaultPage.ContentPlaceHolderInternal.Controls.Count);

			DefaultPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals(1, DefaultPage.ContentPlaceHolderInternal.Controls.Count);
			AssertEquals(typeof(VoteControl), DefaultPage.ContentPlaceHolderInternal.Controls[0].GetType());
			AssertEquals("VoteExamSurveyUserControl", DefaultPage.ContentPlaceHolderInternal.Controls[0].ID);
		}

		public void TestPageLoad_AddSurveyUserControlToContentPlaceHolder()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Survey);
			Factory.Save();
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			AssertEquals("Pre-condition", 0, DefaultPage.ContentPlaceHolderInternal.Controls.Count);

			DefaultPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals(1, DefaultPage.ContentPlaceHolderInternal.Controls.Count);
			AssertEquals(typeof(ExamSurveyUserControl), DefaultPage.ContentPlaceHolderInternal.Controls[0].GetType());
			AssertEquals(OrganisationsDataRegistry.Instance.AutoSaveVoteSurveyExamAnswers.Value, ((ExamSurveyUserControl)DefaultPage.ContentPlaceHolderInternal.Controls[0]).AutoPostBack);
			AssertEquals("VoteExamSurveyUserControl", DefaultPage.ContentPlaceHolderInternal.Controls[0].ID);
		}

		public void TestPageLoad_SubmissionConfirmation()
		{
			GlbCompanyCampaignItem surveyItem = CreateCampaignItem(CampaignTypeList.Codes.Survey);
			Factory.Save();
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, surveyItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			AssertEquals("javascript: return window.confirm('Once submitted, you will not be able to modify your answers. Are you sure?');", DefaultPage.SubmitButtonInternal.OnClientClick);
		}

		[TestDate(2006, 1, 1)]
		public void TestSubmitButtonClick()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";

			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";

			DefaultPage.SubmitButton_ClickInternal(DefaultPage.SubmitButtonInternal, EventArgs.Empty);
			Assert("Error Dialog should be shown", !DefaultPage.SupressErrorDialogInternal);
			AssertEquals(ZDateTime.UtcNow, DefaultPage.DataSource.ExamCampaignItems[0].G8_ClosedDateUtc);
			Assert("Should be saved", !DefaultPage.DataSource.ExamCampaignItems[0].HasChanges);
		}

		public void TestSubmitButtonClick_NoDuplicateKey() // JNG: See WI00036790 for defect. Functionality has moved to another class&method but test case has stayed here. Finish other functionality before moving back to this.
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			Factory.Save();

			campaign.VoteHeader.HY_Min = 1;
			campaign.VoteHeader.HY_Max = 1;
			var votingItem = campaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaign.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.ExamSettingsStringKey, "STD");
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";
			DefaultPage.DataSource.AnswerWrappers[0].Answer.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var campaignItemInAnotherFactory = newFactory.Load<GlbCompanyCampaignItem>(campaignItem.PK);
			var wrapper = new VoteExamSurveyAnswerSet(newFactory, campaignItemInAnotherFactory);
			wrapper.AnswerWrappers[0].Answer.HZ_Answer = "A";
			newFactory.Save();
			wrapper.AnswerWrappers[0].Answer.Factory.Save();
			DefaultPage.SubmitButton_ClickInternal(DefaultPage.SubmitButtonInternal, EventArgs.Empty);
			var newDataSource = DefaultPage.DataSource;
			AssertEquals("No duplicate key saving error", 0, newDataSource.ExamCampaignItems[0].RowErrors.Count());
			AssertEquals("Last answer wins", "1", newDataSource.AnswerWrappers[0].Answer.HZ_Answer);
		}

		[TestDate(2012, 8, 24, 9, 30, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSubmitButtonClick_ConcurrentSave()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/");

			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var campaignItemInAnotherFactory = newFactory.Load<GlbCompanyCampaignItem>(campaignItem.PK);
			campaignItemInAnotherFactory.G8_ClosedDateUtc = new ZDateTime(2012, 8, 24, 9, 29, 0);
			newFactory.Save();

			DefaultPage.SubmitButton_ClickInternal(DefaultPage.SubmitButtonInternal, EventArgs.Empty);
			var dataSource = DefaultPage.DataSource as GlbCompanyCampaignItem;

			campaignItem = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem.PK);
			AssertEquals("First submit wins", new ZDateTime(2012, 8, 24, 9, 29, 0), campaignItem.G8_ClosedDateUtc);
		}

		public void TestOnPreRender()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());

			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";

			DefaultPage.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("Voting Campaign", DefaultPage.Title);
		}

		[ExpectNoExceptions]
		public void TestOnPreRenderNullRef()
		{
			AssertNull(DefaultPage.DataSource);
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.OnPreRenderInternal(EventArgs.Empty);
			DefaultPage.OnPreRenderCompleteInternal(EventArgs.Empty);
			AssertEquals("", DefaultPage.Title);
		}

		[TestDate(2006, 1, 1)]
		public void TestOnPreRender_AnswersHaveBeenSubmitted()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";
			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());

			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";
			DefaultPage.SubmitButton_ClickInternal(DefaultPage.SubmitButtonInternal, EventArgs.Empty);

			DefaultPage.OnPreRenderInternal(EventArgs.Empty);
			AssertEquals("Should not be cleared if answers were submitted", ZDateTime.UtcNow, DefaultPage.DataSource.ExamCampaignItems.FirstOrDefault().G8_ClosedDateUtc);
		}

		public void TestGetNewDataSource_FromCampaignItem()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			AddQueryString(RefCountrySchema.Constants.Prefix, "AU");
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);

			var dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			AssertEquals(campaignItem.PK, dataSource.ExamCampaignItems[0].PK);
			AssertEquals("AU", dataSource.CurrentQuestionsCountryCode);

			var factory2 = new BusinessObjectFactory();
			var loadedCampaignItem = factory2.Load<GlbCompanyCampaignItem>(dataSource.ExamCampaignItems[0].PK);
			AssertEquals("Should be marked as verified", TrackingStatusCodes.Codes.VER, loadedCampaignItem.G8_TrackingStatus);
		}

		public void TestGetNewDataSource_FromCampaign()
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			Factory.Save();

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaign.PK.ToString());
			var dataSource = DefaultPage.GetNewDataSourceInternal() as VoteExamSurveyPreviewBizO;
			AssertNotNull(dataSource);
			AssertEquals(campaign.PK, dataSource.CompanyCampaign.PK);
		}

		public void TestGetNewDataSource_CreateNewCampaignItem()
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			AddQueryString(VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey, campaign.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);

			var dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should be saved", dataSource.ExamCampaignItems.All(x => x.IsInDatabase));
			AssertEquals(campaign.PK, dataSource.CompanyCampaign.PK);
			AssertEquals(OrgContactSchema.Constants.Prefix, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientTableCode).FirstOrDefault());
			AssertEquals(contact.PK, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientID).FirstOrDefault());

			dataSource.ExamCampaignItems.ForEach(x => x.G8_ClosedDateUtc = ZDateTime.UtcNow);
			var newDataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should be saved", newDataSource.ExamCampaignItems.All(x => x.IsInDatabase));
			AssertEquals(campaign.PK, newDataSource.CompanyCampaign.PK);
			AssertEquals(OrgContactSchema.Constants.Prefix, newDataSource.ExamCampaignItems.Select(x => x.G8_RecipientTableCode).FirstOrDefault());
			AssertEquals(contact.PK, newDataSource.ExamCampaignItems.Select(x => x.G8_RecipientID).FirstOrDefault());
		}

		public void TestGetNewDataSource_CreateApplicantFromContact()
		{
			var campaign = CreateCampaign("LCT");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "tester@abc.org";
			var settings = Factory.New<ExamSetting>();
			settings.EXS_G0 = campaign.PK;
			settings.EXS_ExamVersion = "ZZZ";
			Factory.Save();

			AddQueryString(VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey, campaign.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);

			var dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should be saved", dataSource.ExamCampaignItems.All(x => x.IsInDatabase));
			AssertEquals(campaign.PK, dataSource.CompanyCampaign.PK);
			AssertEquals(HRJobApplicantSchema.Constants.Prefix, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientTableCode).FirstOrDefault());
			var applicant = Factory.Load(HRJobApplicantSchema.Constants.Prefix, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientID).FirstOrDefault());
			AssertNotNull(applicant);
			AssertEquals("tester@abc.org", applicant[HRJobApplicantSchema.Constants.HA_EmailAddress]);

			dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			AssertEquals(HRJobApplicantSchema.Constants.Prefix, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientTableCode).FirstOrDefault());
			AssertEquals(applicant.PK, dataSource.ExamCampaignItems.Select(x => x.G8_RecipientID).FirstOrDefault());
		}

		public void TestShowLoginStatus_FromCampaignItem()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			Assert(DefaultPage.ShowLoginStatusInternal);
		}

		public void TestShowLoginStatus_FromCampaign()
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			Factory.Save();

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaign.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			Assert("Should not show login status when previewing", !DefaultPage.ShowLoginStatusInternal);
		}

		public void TestShowLogOffLinkButton()
		{
			Assert("Always false", !DefaultPage.ShowLogOffLinkButtonInternal);
		}

		public void TestIsPersistDataSourceBetweenPostbacks()
		{
			Assert("Always true", DefaultPage.IsPersistDataSourceBetweenPostbacksInternal);
		}

		[TestDate(2006, 1, 1)]
		public void TestRaisePostBackEvent_AutoSubmission()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";

			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);

			var postBackEventHandler = DefaultPage as IPostBackEventHandler;
			postBackEventHandler.RaisePostBackEvent(Default.AutoSubmission);

			var redirectLocation = DefaultPage.Response.RedirectLocation;
			AssertEquals("/webapp/Error.aspx?data=gJ1pQTUIIOB3hHwtxt3piHIuw9DJJDmvZMYcJEKUpoTcRm7J7gsVEe5vF9iHh8nMu85llA5C1stODdS12XHsxXmhXcBQ5QUKp1yj5Op6ts747lcBncuGcpHnTfft8cv9iVFu9u77OzF%2B4f63X0Ms%2FFhUJe6BnlpIc3NTIrC6K%2F5XUezP4I%2FoepwVqqrblm5rNE3FnW8l1oslTnYHu1VZLL7MMMPddE5E", redirectLocation);
		}

		[TestDate(2006, 1, 1)]
		public void TestRaisePostBackEvent_ManualSubmission()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";

			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";

			var postBackEventHandler = DefaultPage as IPostBackEventHandler;
			postBackEventHandler.RaisePostBackEvent(Default.ManualSubmission);

			var redirectLocation = DefaultPage.Response.RedirectLocation;
			AssertEquals(null, redirectLocation);

			AssertEquals(ZDateTime.UtcNow, DefaultPage.DataSource.ExamCampaignItems[0].G8_ClosedDateUtc);
			Assert("Should be saved", !DefaultPage.DataSource.ExamCampaignItems[0].HasChanges);
		}

		[TestDate(2006, 1, 1)]
		public void TestRaisePostBackEvent_CancelSubmission()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";

			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.DataSource.AnswerWrappers[0].Answer.HZ_Answer = "1";

			var postBackEventHandler = DefaultPage as IPostBackEventHandler;
			postBackEventHandler.RaisePostBackEvent(Default.CancelSubmission);

			var redirectLocation = DefaultPage.Response.RedirectLocation;
			AssertEquals("/webapp/Error.aspx?data=gJ1pQTUIIOB3hHwtxt3piEnrfF0caNnzy4ZtQSqrJZ6VEh%2BoJGovfb8LNwIo2KgodPctgCbi6o0ZBgXy0UQ46FSxldJ8hWbF0Avhi%2BAKT%2FfM%2FfWWUNAKScVGjXss1A99VCB9%2BnLS8YMeFCA1LrkmGk%2B8dqGXMpHR", redirectLocation);
		}

		[TestDate(2006, 1, 1)]
		public void TestRaisePostBackEvent_ManualSubmissionButTimeExpired()
		{
			var campaignItem = CreateCampaignItem(CampaignTypeList.Codes.Voting);
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";

			campaignItem.CompanyCampaign.VoteHeader.HY_Min = 1;
			campaignItem.CompanyCampaign.VoteHeader.HY_Max = 1;
			var votingItem = campaignItem.CompanyCampaign.VoteHeader.SubQuestions.AddNew();
			votingItem.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Factory.Save();

			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);
			DefaultPage.SetDataSourceTimeExpired(true);

			var postBackEventHandler = DefaultPage as IPostBackEventHandler;
			postBackEventHandler.RaisePostBackEvent(Default.ManualSubmission);

			var redirectLocation = DefaultPage.Response.RedirectLocation;
			AssertEquals("/webapp/Error.aspx?data=gJ1pQTUIIOB3hHwtxt3piEnrfF0caNnzy4ZtQSqrJZ6VEh%2BoJGovfb8LNwIo2KgodPctgCbi6o0ZBgXy0UQ46FSxldJ8hWbF0Avhi%2BAKT%2FfM%2FfWWUNAKScVGjXss1A99VCB9%2BnLS8YMeFCA1LrkmGk%2B8dqGXMpHR", redirectLocation);
		}

		public void TestGetNewDataSource_GroupedExam_AfterPagePostBack()
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			AddQueryString(VoteExamSurveyUrlHelper.GroupedExamCampaignsStringKey, campaign.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);

			var dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should be saved", dataSource.ExamCampaignItems.All(x => x.IsInDatabase));

			DefaultPage.Session.Clear();
			DefaultPage.IsPostBack = true;
			var newDataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should not be null.", newDataSource != null);
			AssertEquals(campaign.PK, newDataSource.CompanyCampaign.PK);
		}

		public void TestGetNewDataSource_IndividualExam_AfterPagePostBack()
		{
			var campaign = CreateCampaign(CampaignTypeList.Codes.Voting);
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();

			AddQueryString(GlbCompanyCampaignSchema.Constants.PK, campaign.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientIDQueryStringKey, contact.PK.ToString());
			AddQueryString(VoteExamSurveyUrlHelper.RecipientTableCodeQueryStringKey, OrgContactSchema.Constants.Prefix);

			var dataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should be saved", dataSource.ExamCampaignItems.All(x => x.IsInDatabase));

			DefaultPage.Session.Clear();
			DefaultPage.IsPostBack = true;
			var newDataSource = (IVoteExamSurveyAnswerSet)DefaultPage.GetNewDataSourceInternal();
			Assert("Should not be null.", newDataSource != null);
			AssertEquals(campaign.PK, newDataSource.CompanyCampaign.PK);
		}

		[TestDate(2006, 1, 1)]
		public void TestRaisePostBackEvent_ManualSubmission_InvalidData()
		{
			var campaignItem = CreateCampaignItem("LCT");
			campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
			campaignItem.G8_GS_NKFollowedUpBy = "E";
			campaignItem.G8_RecipientTableCode = "OC";
			campaignItem.G8_TrackingStatus = "UNV";
			Factory.Save();

			AssertEquals(true, campaignItem.CompanyCampaign.IsExamCampaign);
			AddQueryString(GlbCompanyCampaignItemSchema.Constants.PK, campaignItem.PK.ToString());
			DefaultPage.OnLoadInternal(EventArgs.Empty);

			var postBackEventHandler = DefaultPage as IPostBackEventHandler;
			postBackEventHandler.RaisePostBackEvent(Default.ManualSubmission);

			var redirectLocation = DefaultPage.Response.RedirectLocation;
			AssertStartsWith("ErrorPage", "/webapp/Error.aspx?data=", redirectLocation);
		}

		public void TestReportError()
		{
			Default.ReportError("someting bad...");
			AssertEquals("someting bad...", ErrorReporter.LastMessageReported);
			AssertEquals("Enterprise.MarketingManager.WebVoting.ReportError", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		void AddQueryString(string key, string value)
		{
			AddQueryString(DefaultPage, key, value);
		}

		void AddQueryString(Default page, string key, string value)
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

		DefaultPageForTest DefaultPage
		{
			get { return (DefaultPageForTest)Page; }
		}

		protected override ZPage GetNewZPage()
		{
			var result = new DefaultPageForTest();
			result.SetSubmitButtonInternal(new ZButton());
			result.SetContentPlaceHolderInternal(new PlaceHolder());
			result.SetVoteTitleLabelInternal(new ZTextLabel());
			return result;
		}
	}
}
