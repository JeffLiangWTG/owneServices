using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	public abstract class VoteExamSurveyAnswerWrapperBaseTest<T> : NonPersistentBusinessObjectTestCase where T : VoteExamSurveyAnswerWrapperBase
	{
		public void TestQuestion()
		{
			AssertEquals("Should be assigned in the constructor", Question, AnswerWrapper.Question);
		}

		public void TestAnswer()
		{
			AssertEquals("Pre-condition", 0, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());

			AssertNotNull("Should be created if cannot be loaded", AnswerWrapper.Answer);
			Assert("Should be registered as EditableChildObject", AnswerWrapper.IsRegisteredEditableChildObject(AnswerWrapper.Answer));
			AssertEquals(Question.PK, AnswerWrapper.Answer.HZ_HY);
			AssertEquals(1, VoteExamSurveyAnswerSet.PersistedAnswers.Count);

			ZGuid answer1PK = AnswerWrapper.Answer.PK;
			AnswerWrapper.Answer.Delete();
			AssertNotEquals("Should create a new one if the previous one was deleted", answer1PK, AnswerWrapper.Answer.PK);

			AnswerWrapper.Answer.Delete();
			VoteExamSurveyAnswer relatedAnswer = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(Question, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			AssertEquals("Should be loaded if already in the factory", relatedAnswer.PK, AnswerWrapper.Answer.PK);
		}

		public void TestQuestionOrder_Randomised()
		{
			fCampaign = Factory.New<CampaignWithRandomisedQuestionsForTest>();
			SetupNewCampaign(fCampaign);
			var campaignItem = Factory.New<CampaignItemWithRandomisedQuestionsForTest>();
			campaignItem.G8_G0 = Campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			VoteExamSurveyQuestion q1 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q2 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q3 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q4 = Campaign.ActualQuestionsForBinding.AddNew();

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			VoteExamSurveyAnswerWrapperBase q1Wrapper = voteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(q1, campaignItem);
			VoteExamSurveyAnswerWrapperBase q2Wrapper = voteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(q2, campaignItem);
			VoteExamSurveyAnswerWrapperBase q3Wrapper = voteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(q3, campaignItem);
			VoteExamSurveyAnswerWrapperBase q4Wrapper = voteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(q4, campaignItem);

			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q1Wrapper) + 1, q1Wrapper.QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q2Wrapper) + 1, q2Wrapper.QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q3Wrapper) + 1, q3Wrapper.QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q4Wrapper) + 1, q4Wrapper.QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q1Wrapper) + 1, q1Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q2Wrapper) + 1, q2Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q3Wrapper) + 1, q3Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(((IList)voteExamSurveyAnswerSet.AnswerWrappers).IndexOf(q4Wrapper) + 1, q4Wrapper.Answer.HZ_QuestionOrder);
		}

		public void TestQuestionOrder_NotRandomised()
		{
			VoteExamSurveyQuestion q1 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q2 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q3 = Campaign.ActualQuestionsForBinding.AddNew();
			VoteExamSurveyQuestion q4 = Campaign.ActualQuestionsForBinding.AddNew();

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();

			T q1Wrapper = VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<T>(q1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			T q2Wrapper = VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<T>(q2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			T q3Wrapper = VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<T>(q3, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			T q4Wrapper = VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<T>(q4, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			AssertEquals(q1.ActualOrder, q1Wrapper.QuestionOrder);
			AssertEquals(q2.ActualOrder, q2Wrapper.QuestionOrder);
			AssertEquals(q3.ActualOrder, q3Wrapper.QuestionOrder);
			AssertEquals(q4.ActualOrder, q4Wrapper.QuestionOrder);
			AssertEquals(q1.ActualOrder, q1Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(q2.ActualOrder, q2Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(q3.ActualOrder, q3Wrapper.Answer.HZ_QuestionOrder);
			AssertEquals(q4.ActualOrder, q4Wrapper.Answer.HZ_QuestionOrder);
		}

		#region Paging Support

		public void TestQuestionTextForWeb()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_QuestionsPerWebPage = 1;

			SetupTestQuestions(campaign);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			AssertEquals("Header 1", voteExamSurveyAnswerSet.AnswerWrappers[0].QuestionTextForWeb);
			AssertEquals("Question 1", voteExamSurveyAnswerSet.AnswerWrappers[1].QuestionTextForWeb);
			AssertEquals("Question 2", voteExamSurveyAnswerSet.AnswerWrappers[2].QuestionTextForWeb);
			AssertEquals("Header 2", voteExamSurveyAnswerSet.AnswerWrappers[3].QuestionTextForWeb);
			AssertEquals("Question 3", voteExamSurveyAnswerSet.AnswerWrappers[4].QuestionTextForWeb);
			AssertEquals("Question 4", voteExamSurveyAnswerSet.AnswerWrappers[5].QuestionTextForWeb);

			voteExamSurveyAnswerSet.CurrentPage = 2;
			AssertEquals("Header 1 (...continued)", voteExamSurveyAnswerSet.AnswerWrappers[0].QuestionTextForWeb);
			AssertEquals("Question 1", voteExamSurveyAnswerSet.AnswerWrappers[1].QuestionTextForWeb);
			AssertEquals("Question 2", voteExamSurveyAnswerSet.AnswerWrappers[2].QuestionTextForWeb);
			AssertEquals("Header 2", voteExamSurveyAnswerSet.AnswerWrappers[3].QuestionTextForWeb);
			AssertEquals("Question 3", voteExamSurveyAnswerSet.AnswerWrappers[4].QuestionTextForWeb);
			AssertEquals("Question 4", voteExamSurveyAnswerSet.AnswerWrappers[5].QuestionTextForWeb);

			voteExamSurveyAnswerSet.CurrentPage = 3;
			AssertEquals("Header 1", voteExamSurveyAnswerSet.AnswerWrappers[0].QuestionTextForWeb);
			AssertEquals("Question 1", voteExamSurveyAnswerSet.AnswerWrappers[1].QuestionTextForWeb);
			AssertEquals("Question 2", voteExamSurveyAnswerSet.AnswerWrappers[2].QuestionTextForWeb);
			AssertEquals("Header 2", voteExamSurveyAnswerSet.AnswerWrappers[3].QuestionTextForWeb);
			AssertEquals("Question 3", voteExamSurveyAnswerSet.AnswerWrappers[4].QuestionTextForWeb);
			AssertEquals("Question 4", voteExamSurveyAnswerSet.AnswerWrappers[5].QuestionTextForWeb);

			voteExamSurveyAnswerSet.CurrentPage = 4;
			AssertEquals("Header 1", voteExamSurveyAnswerSet.AnswerWrappers[0].QuestionTextForWeb);
			AssertEquals("Question 1", voteExamSurveyAnswerSet.AnswerWrappers[1].QuestionTextForWeb);
			AssertEquals("Question 2", voteExamSurveyAnswerSet.AnswerWrappers[2].QuestionTextForWeb);
			AssertEquals("Header 2 (...continued)", voteExamSurveyAnswerSet.AnswerWrappers[3].QuestionTextForWeb);
			AssertEquals("Question 3", voteExamSurveyAnswerSet.AnswerWrappers[4].QuestionTextForWeb);
			AssertEquals("Question 4", voteExamSurveyAnswerSet.AnswerWrappers[5].QuestionTextForWeb);

			campaign.G0_QuestionsPerWebPage = 10;
			AssertEquals("Header 1", voteExamSurveyAnswerSet.AnswerWrappers[0].QuestionTextForWeb);
			AssertEquals("Question 1", voteExamSurveyAnswerSet.AnswerWrappers[1].QuestionTextForWeb);
			AssertEquals("Question 2", voteExamSurveyAnswerSet.AnswerWrappers[2].QuestionTextForWeb);
			AssertEquals("Header 2", voteExamSurveyAnswerSet.AnswerWrappers[3].QuestionTextForWeb);
			AssertEquals("Question 3", voteExamSurveyAnswerSet.AnswerWrappers[4].QuestionTextForWeb);
			AssertEquals("Question 4", voteExamSurveyAnswerSet.AnswerWrappers[5].QuestionTextForWeb);
		}

		public void TestIsContinuedHeaderForPaging()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_QuestionsPerWebPage = 1;

			SetupTestQuestions(campaign);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[0].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[1].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[2].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[3].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[4].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[5].IsContinuedHeaderForPaging);

			voteExamSurveyAnswerSet.CurrentPage = 2;
			Assert(voteExamSurveyAnswerSet.AnswerWrappers[0].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[1].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[2].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[3].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[4].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[5].IsContinuedHeaderForPaging);

			voteExamSurveyAnswerSet.CurrentPage = 3;
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[0].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[1].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[2].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[3].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[4].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[5].IsContinuedHeaderForPaging);

			voteExamSurveyAnswerSet.CurrentPage = 4;
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[0].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[1].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[2].IsContinuedHeaderForPaging);
			Assert(voteExamSurveyAnswerSet.AnswerWrappers[3].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[4].IsContinuedHeaderForPaging);
			Assert(!voteExamSurveyAnswerSet.AnswerWrappers[5].IsContinuedHeaderForPaging);
		}

		public void TestIndexForPaging()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			SetupTestQuestions(campaign);
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			AssertEquals(0, voteExamSurveyAnswerSet.AnswerWrappers[0].IndexForPaging);
			AssertEquals(0, voteExamSurveyAnswerSet.AnswerWrappers[1].IndexForPaging);
			AssertEquals(1, voteExamSurveyAnswerSet.AnswerWrappers[2].IndexForPaging);
			AssertEquals(2, voteExamSurveyAnswerSet.AnswerWrappers[3].IndexForPaging);
			AssertEquals(2, voteExamSurveyAnswerSet.AnswerWrappers[4].IndexForPaging);
			AssertEquals(3, voteExamSurveyAnswerSet.AnswerWrappers[5].IndexForPaging);
		}

		void SetupTestQuestions(GlbCompanyCampaign campaign)
		{
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.Header, "Header 1");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 1");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 2");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.Header, "Header 2");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 3");
			AddNewQuestion(campaign, VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, "Question 4");
		}

		void AddNewQuestion(GlbCompanyCampaign campaign, string answerType, string questionText)
		{
			VoteExamSurveyQuestion question = campaign.ActualQuestionsForBinding.AddNew();
			question.HY_AnswerType = answerType;
			question.HY_Question = questionText;
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return AnswerWrapper;
		}

		protected T AnswerWrapper
		{
			get
			{
				if (fAnswerWrapper == null)
				{
					fAnswerWrapper = GetNewAnswerWrapperForTest(Question, VoteExamSurveyAnswerSet);
				}
				return fAnswerWrapper;
			}
		}

		protected abstract T GetNewAnswerWrapperForTest(VoteExamSurveyQuestion question, IVoteExamSurveyAnswerSet campaignItem);

		protected VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					fQuestion = Campaign.ActualQuestionsForBinding.AddNew();
				}
				return fQuestion;
			}
		}

		protected VoteExamSurveyAnswerSet VoteExamSurveyAnswerSet
		{
			get
			{
				if (fVoteExamSurveyAnswerSet == null)
				{
					fVoteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, Campaign.CampaignsItemsSent.AddNew());
				}
				return fVoteExamSurveyAnswerSet;
			}
		}

		protected GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.New<GlbCompanyCampaign>();
					SetupNewCampaign(fCampaign);
				}
				return fCampaign;
			}
		}

		protected virtual void SetupNewCampaign(GlbCompanyCampaign campaign)
		{
		}

		T fAnswerWrapper;
		VoteExamSurveyQuestion fQuestion;
		VoteExamSurveyAnswerSet fVoteExamSurveyAnswerSet;
		GlbCompanyCampaign fCampaign;
	}
}
