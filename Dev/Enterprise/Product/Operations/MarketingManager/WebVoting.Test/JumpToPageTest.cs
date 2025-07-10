using System;
using System.Reflection;
using System.Web;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class JumpToPageTest : ZPageTestCase
	{
		public void TestCrossThreadAccessException_VoteExamSurveyPreviewBizO()
		{
			var surveyCampaign = Factory.New<GlbCompanyCampaign>();
			surveyCampaign.FillWithValidTestData();
			surveyCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			var surveyItem = surveyCampaign.CampaignsItemsSent.AddNew();
			surveyItem.FillWithValidTestData();
			surveyItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			surveyItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			var campaign = Factory.Load<GlbCompanyCampaign>(surveyItem.G8_G0);
			IVoteExamSurveyAnswerSet dataSource = new VoteExamSurveyPreviewBizOForTest(campaign);
			dataSource.StartVoteExamSurvey();

			var voteExamSurveyPreviewBizO = (VoteExamSurveyPreviewBizOForTest)dataSource;
			var nonSavableLocalFactory = (BusinessObjectFactoryLoadCount)voteExamSurveyPreviewBizO.NonSavableLocalFactory_Exposed;
			var originalLoadCount = nonSavableLocalFactory.LoadCount;

			AssertEquals("Initial Page Count", 1, voteExamSurveyPreviewBizO.PageCount.ToZInt());
			AssertEquals("DummySession", HttpContext.Current.Session.SessionID);

			var index = AddParamsString(GlbCompanyCampaignItemSchema.Constants.PK, surveyItem.PK.ToString());
			HttpContext.Current.Session[index] = dataSource;
			JumpToPageForTest.OnLoadExposed();

			var voteExamSurveyPreviewBizOJumpToPage = (VoteExamSurveyPreviewBizOForTest)JumpToPageForTest.DataSource;
			var nonSavableLocalFactoryJumpToPage = (BusinessObjectFactoryLoadCount)voteExamSurveyPreviewBizOJumpToPage.NonSavableLocalFactory_Exposed;
			var jumpToPageFactoryLoadCount = nonSavableLocalFactoryJumpToPage.LoadCount;

			AssertEquals(originalLoadCount, jumpToPageFactoryLoadCount);
			AssertEquals("DummySession", HttpContext.Current.Session.SessionID);
		}

		public void TestCrossThreadAccessExceptionVoteSurveyExamManager()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			var glbCompanyCampaignItem = campaign.CampaignsItemsSent.AddNew();
			glbCompanyCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			glbCompanyCampaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var factory = new BusinessObjectFactoryLoadCount();
			var manager = new VoteSurveyExamManager();
			IVoteExamSurveyAnswerSet dataSource = manager.StartVoteExamSurvey(factory, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");
			var originalLoadCount = factory.LoadCount;

			var index = AddParamsString(GlbCompanyCampaignItemSchema.Constants.PK, glbCompanyCampaignItem.PK.ToString());
			HttpContext.Current.Session[index] = dataSource;
			JumpToPageForTest.OnLoadExposed();

			var voteExamSurveyAnswerSetJumpToPage = (VoteExamSurveyAnswerSet)JumpToPageForTest.DataSource;
			var jumpToPageFactoryLoadCount = ((BusinessObjectFactoryLoadCount)voteExamSurveyAnswerSetJumpToPage.Factory).LoadCount;
			AssertEquals("Factory load count should be the same as the original count", originalLoadCount, jumpToPageFactoryLoadCount);
		}

		public void TestCrossThreadAccessExceptionAnswerSetPageCount()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 2;

			var glbCompanyCampaignItem = campaign.CampaignsItemsSent.AddNew();
			glbCompanyCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			glbCompanyCampaignItem.G8_RecipientID = contact.PK;

			var surveyQuestion1 = campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";

			var surveyQuestion2 = campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";

			var surveyQuestion3 = campaign.Questions.AddNew();
			surveyQuestion3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;

			var option1 = surveyQuestion3.SubQuestions.AddNew();
			option1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option1.HY_Question = "Option 1";

			var option2 = surveyQuestion3.SubQuestions.AddNew();
			option2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option2.HY_Question = "Option 2";

			var surveyQuestion4 = campaign.Questions.AddNew();
			surveyQuestion4.HY_Question = "survey4";

			var surveyQuestion5 = campaign.Questions.AddNew();
			surveyQuestion5.HY_Question = "survey5";
			Factory.Save();

			var factory = new BusinessObjectFactoryLoadCount();
			var manager = new VoteSurveyExamManager();
			IVoteExamSurveyAnswerSet dataSource = manager.StartVoteExamSurvey(factory, ZGuid.Empty, new GlbCompanyCampaignItem.RecipientInfo(contact.PK, OrgContactSchema.Constants.Prefix), string.Empty, string.Empty, new string[] { campaign.PK.ToString() }, true, "STD");

			var index = AddParamsString(GlbCompanyCampaignItemSchema.Constants.PK, glbCompanyCampaignItem.PK.ToString());
			HttpContext.Current.Session[index] = dataSource;
			JumpToPageForTest.OnLoadExposed();

			var voteExamSurveyAnswerSetJumpToPage = (VoteExamSurveyAnswerSet)JumpToPageForTest.DataSource;
			var answerWrappers = voteExamSurveyAnswerSetJumpToPage.AnswerWrappers;
			answerWrappers.Load();

			var originalLoadCount = factory.LoadCount;
			var pageCount = (short)Math.Ceiling((decimal)answerWrappers.NonHeaderCount / voteExamSurveyAnswerSetJumpToPage.CompanyCampaign.G0_QuestionsPerWebPage);
			AssertEquals("jumpToPageCount should be the same as pageCount", pageCount, voteExamSurveyAnswerSetJumpToPage.PageCount);

			var jumpToPageFactoryLoadCount = ((BusinessObjectFactoryLoadCount)voteExamSurveyAnswerSetJumpToPage.Factory).LoadCount;
			AssertEquals("Factory load count should be the same as the original count", originalLoadCount, jumpToPageFactoryLoadCount);
		}

		string AddParamsString(string key, string value)
		{
			return AddParamsString(JumpToPageForTest, key, value);
		}

		string AddParamsString(JumpToPage page, string key, string value)
		{
			var secureQueryStringData = page.Request[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
			var queryString = new SecureQueryString(secureQueryStringData);

			queryString.Remove(SecureQueryString.TimeStampKey);
			queryString.Add(key, value);

			var isReadOnly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			isReadOnly.SetValue(page.Request.Params, false, null);

			page.Request.Params[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey] = queryString.ToString();
			isReadOnly.SetValue(page.Request.Params, true, null);

			return queryString.ToString();
		}

		JumpToPageForTest JumpToPageForTest => (JumpToPageForTest)Page;

		protected override ZPage GetNewZPage() => new JumpToPageForTest();
	}
}
