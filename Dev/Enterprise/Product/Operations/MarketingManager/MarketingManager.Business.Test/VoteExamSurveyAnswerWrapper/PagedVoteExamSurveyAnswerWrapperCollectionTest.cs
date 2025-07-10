using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(PagedVoteExamSurveyAnswerWrapperCollection))]
	sealed class PagedVoteExamSurveyAnswerWrapperCollectionTest : BusinessObjectCollectionViewTestCase<PagedVoteExamSurveyAnswerWrapperCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		public void TestElementsWithHeaders()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 3;

			SetupTestQuestions(campaign, true);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();

			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 1",
				"Question 1",
				"Question 2",
				"Question 3");

			answerSet.CurrentPage = 4;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 3",
				"Question 10",
				"Question 11",
				"Question 12");

			answerSet.CurrentPage = 2;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 1 (...continued)",
				"Question 4",
				"Header 2",
				"Question 5",
				"Question 6");

			answerSet.CurrentPage = 3;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 2 (...continued)",
				"Question 7",
				"Question 8",
				"Question 9");
		}

		public void TestElementsWithoutHeaders()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 4;

			SetupTestQuestions(campaign, false);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();

			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 1",
				"Question 2",
				"Question 3",
				"Question 4");

			answerSet.CurrentPage = 3;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 9",
				"Question 10",
				"Question 11",
				"Question 12");

			answerSet.CurrentPage = 2;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 5",
				"Question 6",
				"Question 7",
				"Question 8");
		}

		public void TestRandomisedElementsWithHeader()
		{
			CampaignWithRandomisedQuestionsForTest campaign = Factory.New<CampaignWithRandomisedQuestionsForTest>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 4;

			SetupTestQuestions(campaign, true);

			CampaignItemWithRandomisedQuestionsForTest campaignItem = Factory.New<CampaignItemWithRandomisedQuestionsForTest>();
			campaignItem.G8_G0 = campaign.PK;
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();

			answerSet.CurrentPage = 1;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 1",
				"Question 3",
				"Question 1",
				"Question 4",
				"Question 2");

			answerSet.CurrentPage = 3;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 2 (...continued)",
				"Question 5",
				"Header 3",
				"Question 10",
				"Question 11",
				"Question 12");

			answerSet.CurrentPage = 2;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Header 2",
				"Question 8",
				"Question 6",
				"Question 7",
				"Question 9");
		}

		public void TestRandomisedElementsWithoutHeader()
		{
			CampaignWithRandomisedQuestionsForTest campaign = Factory.New<CampaignWithRandomisedQuestionsForTest>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 5;

			SetupTestQuestions(campaign, false);

			CampaignItemWithRandomisedQuestionsForTest campaignItem = Factory.New<CampaignItemWithRandomisedQuestionsForTest>();
			campaignItem.G8_G0 = campaign.PK;
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();

			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 10",
				"Question 3",
				"Question 8",
				"Question 1",
				"Question 11");

			answerSet.CurrentPage = 3;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 2",
				"Question 5");

			answerSet.CurrentPage = 2;
			AssertPagedAnswerWrappers(campaignItem, answerSet,
				"Question 6",
				"Question 12",
				"Question 7",
				"Question 9",
				"Question 4");
		}

		public override void TestTypedAddNew()
		{
			Assert(true);
		}

		public void TestLazyLoadAnswerCapturersOnConstruction()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				campaign.G0_QuestionsPerWebPage = 4;

				SetupTestQuestions(campaign, false);

				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				campaignItem.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;
				campaignItem.G8_GS_NKFollowedUpBy = "E";
				campaignItem.G8_RecipientTableCode = "OC";
				campaignItem.G8_TrackingStatus = "UNV";
				VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
				((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();

				object lazyLoadAnswerWrappers = answerSet.PagedAnswerWrappers;
				answerSet.RunPreSaveValidation();
				AssertEquals(12, answerSet.NotificationsIncludingChildren.Count());
				AssertEquals(1, answerSet.NotificationsIncludingChildren.GetUniqueMessageList().Length);
				AssertContains("You need to select 1 option(s). You have selected 0 option(s).", answerSet.NotificationsIncludingChildren.GetUniqueMessageList()[0]);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestRebuilt()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			campaign.G0_QuestionsPerWebPage = 4;

			SetupTestQuestions(campaign, false);

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();
			bool rebuiltFired = false;
			answerSet.PagedAnswerWrappers.Rebuilt += delegate
			{ rebuiltFired = true; };
			answerSet.CurrentPage = 2;
			Assert(rebuiltFired);
		}

		void AssertPagedAnswerWrappers(GlbCompanyCampaignItem campaignItem, IVoteExamSurveyAnswerSet answerSet, params string[] questionTexts)
		{
			StringBuilder actualTextBuilder = new StringBuilder();
			foreach (VoteExamSurveyAnswerWrapper wrapper in answerSet.PagedAnswerWrappers)
			{
				actualTextBuilder.AppendLine(wrapper.QuestionTextForWeb);
			}
			string actualText = actualTextBuilder.ToString().Trim();
			string expectedText = string.Join("\r\n", questionTexts);

			string errorMessage = string.Format("Expected (Full):\r\n{0}\r\n\r\nActual (Full):\r\n{1}\r\n\r\n", expectedText, actualText);
			AssertEquals(errorMessage, questionTexts.Length, answerSet.PagedAnswerWrappers.Count);
			AssertMultilineEquals(errorMessage, expectedText, actualText, '\n');
		}

		void SetupTestQuestions(GlbCompanyCampaign campaign, bool includeHeaders)
		{
			if (includeHeaders)
			{
				AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.Header, "Header 1");
			}

			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 1");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 2");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 3");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 4");
			if (includeHeaders)
			{
				AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.Header, "Header 2");
			}

			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 5");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 6");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 7");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 8");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 9");
			if (includeHeaders)
			{
				AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.Header, "Header 3");
			}

			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 10");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 11");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 12");
		}

		void AddNewQuestion(GlbCompanyCampaign campaign, string answerType, string questionText)
		{
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_AnswerType = answerType;
			question.HY_Question = questionText;
			question.HY_IsOptional = false;
		}

		protected override PagedVoteExamSurveyAnswerWrapperCollection GetCollectionToTest()
		{
			return new PagedVoteExamSurveyAnswerWrapperCollection(ExamAnswerSet);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			return new VoteExamSurveyAnswerWrapper(question, ExamAnswerSet, CampaignItem, 3);
		}

		GlbCompanyCampaignItem campaignItem;
		GlbCompanyCampaignItem CampaignItem
		{
			get
			{
				if (campaignItem == null)
				{
					GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
					campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
					campaign.G0_QuestionsPerWebPage = 3;
					campaignItem = campaign.CampaignsItemsSent.AddNew();
				}
				return campaignItem;
			}
		}

		VoteExamSurveyAnswerSet examAnswerSet;
		VoteExamSurveyAnswerSet ExamAnswerSet
		{
			get
			{
				return examAnswerSet ?? (examAnswerSet = new VoteExamSurveyAnswerSet(Factory, CampaignItem));
			}
		}
	}
}
