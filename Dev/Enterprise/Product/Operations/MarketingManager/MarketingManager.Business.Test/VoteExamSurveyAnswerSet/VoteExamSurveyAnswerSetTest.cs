using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyAnswerSet))]
	public class VoteExamSurveyAnswerSetTest : NonPersistentBusinessObjectTestCase
	{
		#region IVoteExamSurveyAnswerSet

		public void TestAnswerWrappers()
		{
			SetupBusinessObject();
			AssertEquals(typeof(VoteExamSurveyAnswerWrapperCollection), AnswerSet.AnswerWrappers.GetType());
			Assert("Should be loaded", AnswerSet.AnswerWrappers.IsLoaded);
			Assert("Should be registered as EditableChildObject", AnswerSet.IsRegisteredEditableChildObject(AnswerSet.AnswerWrappers));
		}

		public void TestHasLoadedAnswerWrappers()
		{
			SetupBusinessObject();
			AssertEquals(false, AnswerSet.HasLoadedAnswerWrappers);
			AssertNotNull(AnswerSet.AnswerWrappers);
			AssertEquals(true, AnswerSet.HasLoadedAnswerWrappers);
		}

		public virtual void TestStartVoteExamSurvey()
		{
			SetupBusinessObject();
			AssertEquals("Pre-condition", "", CampaignItem.G8_Stage);
			((IVoteExamSurveyAnswerSet)AnswerSet).StartVoteExamSurvey();
			AssertEquals(GlbCompanyCampaignItemLookups.StagesConstants.Taken, CampaignItem.G8_Stage);
		}

		public void TestStartVoteExamSurvey_InitAnswerWrappers()
		{
			var survey = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var q1 = survey.Questions.AddNew();
			q1.HY_Question = "Q1";

			var q2 = survey.Questions.AddNew();
			q2.HY_Question = "Q2";

			var q3 = survey.Questions.AddNew();
			q3.HY_Question = "Q3";
			q3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var q3_option1 = q3.SubQuestions.AddNew();
			q3_option1.HY_Question = "Q3_1";
			var q3_option2 = q3.SubQuestions.AddNew();
			q3_option2.HY_Question = "Q3_2";
			var q3_option3 = q3.SubQuestions.AddNew();
			q3_option3.HY_Question = "Q3_3";
			var q3_option4 = q3.SubQuestions.AddNew();
			q3_option4.HY_Question = "Q3_4";

			var q4 = survey.Questions.AddNew();
			q4.HY_Question = "Q4";
			q4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var q4_option1 = q4.SubQuestions.AddNew();
			q4_option1.HY_Question = "Q4_1";
			var q4_option2 = q4.SubQuestions.AddNew();
			q4_option2.HY_Question = "Q4_2";

			Factory.Save();

			var campaignItem = survey.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();
			Factory.Save();

			var campaignItemInOtherFactory = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem.PK);
			var answers = campaignItemInOtherFactory.PersistedAnswers.Cast<VoteExamSurveyAnswer>().OrderBy(x => x.HZ_QuestionOrder).ThenBy(x => x.HZ_SubQuestionOrder).ToArray();

			AssertEquals(10, answers.Length);
			AssertEquals("Q1", answers[0].Question.HY_Question);
			AssertEquals("Q2", answers[1].Question.HY_Question);
			AssertEquals("Q3", answers[2].Question.HY_Question);
			AssertEquals("Q3_1", answers[3].Question.HY_Question);
			AssertEquals("Q3_2", answers[4].Question.HY_Question);
			AssertEquals("Q3_3", answers[5].Question.HY_Question);
			AssertEquals("Q3_4", answers[6].Question.HY_Question);
			AssertEquals("Q4", answers[7].Question.HY_Question);
			AssertEquals("Q4_1", answers[8].Question.HY_Question);
			AssertEquals("Q4_2", answers[9].Question.HY_Question);
		}

		public void TestStartVoteExamSurvey_AnswerOrderWhenNotRandomisedAndUsesQuestionOrder()
		{
			var survey = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			SetUpQuestion(survey, "Q1", VoteExamSurveyAnswerTypeList.Codes.TrueFalse, 1);
			SetUpQuestion(survey, "Header for Q2 and Q3", VoteExamSurveyAnswerTypeList.Codes.Header, 2);
			SetUpQuestion(survey, "Q2", VoteExamSurveyAnswerTypeList.Codes.MultipleChoice, 3, true);
			SetUpQuestion(survey, "Q3", VoteExamSurveyAnswerTypeList.Codes.FreeText, 4);
			SetUpQuestion(survey, "Header at the end", VoteExamSurveyAnswerTypeList.Codes.Header, 5);

			Factory.Save();

			var campaignItem = survey.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();
			Factory.Save();

			var campaignItemInOtherFactory = new BusinessObjectFactory().Load<GlbCompanyCampaignItem>(campaignItem.PK);
			var answers = campaignItemInOtherFactory.PersistedAnswers.Cast<VoteExamSurveyAnswer>().OrderBy(x => x.HZ_QuestionOrder)
																								 .ThenBy(x => x.Question.HY_AnswerType == VoteExamSurveyAnswerTypeList.Codes.Header ? 1 : 0)
																								 .ThenBy(x => x.HZ_SubQuestionOrder).ToArray();

			AssertEquals(9, answers.Length);
			AssertEquals("Q1", answers[0].Question.HY_Question);
			AssertEquals((short)1, answers[0].HZ_QuestionOrder);
			AssertEquals("Header for Q2 and Q3", answers[1].Question.HY_Question);
			AssertEquals((short)1, answers[1].HZ_QuestionOrder);
			AssertEquals("Q2", answers[2].Question.HY_Question);
			AssertEquals((short)2, answers[2].HZ_QuestionOrder);
			AssertEquals("Q2_1", answers[3].Question.HY_Question);
			AssertEquals((short)2, answers[3].HZ_QuestionOrder);
			AssertEquals((short)1, answers[3].HZ_SubQuestionOrder);
			AssertEquals("Q2_2", answers[4].Question.HY_Question);
			AssertEquals((short)2, answers[4].HZ_QuestionOrder);
			AssertEquals((short)2, answers[4].HZ_SubQuestionOrder);
			AssertEquals("Q2_3", answers[5].Question.HY_Question);
			AssertEquals((short)2, answers[5].HZ_QuestionOrder);
			AssertEquals((short)3, answers[5].HZ_SubQuestionOrder);
			AssertEquals("Q2_4", answers[6].Question.HY_Question);
			AssertEquals((short)2, answers[6].HZ_QuestionOrder);
			AssertEquals((short)4, answers[6].HZ_SubQuestionOrder);
			AssertEquals("Q3", answers[7].Question.HY_Question);
			AssertEquals((short)3, answers[7].HZ_QuestionOrder);
			AssertEquals("Header at the end", answers[8].Question.HY_Question);
			AssertEquals((short)4, answers[8].HZ_QuestionOrder);
		}

		public void TestStartVoteExamSurvey_AnswerOrderWhenRandomisedAndUsesSubQuestionOrder()
		{
			var votingCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			votingCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			votingCampaign.G0_RandomizeWithinHeader = true;

			Assert("Pre-Condition", votingCampaign.RandomiseQuestionAndMultipleChoiceOrder);

			SetUpQuestion(votingCampaign, "Header 1", VoteExamSurveyAnswerTypeList.Codes.Header, 1, false, true);
			SetUpQuestion(votingCampaign, "Q1", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 2, false, true);
			SetUpQuestion(votingCampaign, "Q2", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 3, false, true);
			SetUpQuestion(votingCampaign, "Q3", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 4, false, true);
			SetUpQuestion(votingCampaign, "Header 2", VoteExamSurveyAnswerTypeList.Codes.Header, 5, false, true);
			SetUpQuestion(votingCampaign, "Q4", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 6, false, true);

			Factory.Save();

			var campaignItem = votingCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();
			Factory.Save();

			var answers = campaignItem.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().OrderBy(x => x.Answer.HZ_QuestionOrder).ThenBy(x => x.Answer.HZ_SubQuestionOrder).ToArray();

			AssertEquals(6, answers.Length);

			AssertEquals("Header 1", answers[0].Question.HY_Question);
			AssertEquals((short)0, answers[0].Answer.HZ_QuestionOrder);

			AssertEquals("Q2", answers[1].Question.HY_Question);
			AssertEquals((short)1, answers[1].Answer.HZ_QuestionOrder);

			AssertEquals("Q1", answers[2].Question.HY_Question);
			AssertEquals((short)2, answers[2].Answer.HZ_QuestionOrder);

			AssertEquals("Q3", answers[3].Question.HY_Question);
			AssertEquals((short)3, answers[3].Answer.HZ_QuestionOrder);

			AssertEquals("Header 2", answers[4].Question.HY_Question);
			AssertEquals((short)3, answers[4].Answer.HZ_QuestionOrder);

			AssertEquals("Q4", answers[5].Question.HY_Question);
			AssertEquals((short)4, answers[5].Answer.HZ_QuestionOrder);
		}

		public void TestStartVoteExamSurvey_AnswerOrderWhenNotRandomisedAndUsesSubQuestionOrder()
		{
			var votingCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			votingCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			votingCampaign.G0_RandomizeWithinHeader = false;

			AssertEquals("Pre-Condition", false, votingCampaign.RandomiseQuestionAndMultipleChoiceOrder);

			SetUpQuestion(votingCampaign, "Header for Q1 and Q2", VoteExamSurveyAnswerTypeList.Codes.Header, 1, false, true);
			SetUpQuestion(votingCampaign, "Q1", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 2, false, true);
			SetUpQuestion(votingCampaign, "Q2", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 3, false, true);
			SetUpQuestion(votingCampaign, "Q3", VoteExamSurveyAnswerTypeList.Codes.VotingItem, 4, false, true);
			SetUpQuestion(votingCampaign, "Header at the end", VoteExamSurveyAnswerTypeList.Codes.Header, 5, false, true);

			Factory.Save();

			var campaignItem = votingCampaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			VoteExamSurveyAnswerSet answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)answerSet).StartVoteExamSurvey();
			Factory.Save();

			var answers = campaignItem.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().OrderBy(x => x.Answer.HZ_QuestionOrder).ThenBy(x => x.Answer.HZ_SubQuestionOrder).ToArray();

			AssertEquals(5, answers.Length);

			AssertEquals("Header for Q1 and Q2", answers[0].Question.HY_Question);
			AssertEquals((short)0, answers[0].Answer.HZ_QuestionOrder);

			AssertEquals("Q1", answers[1].Question.HY_Question);
			AssertEquals((short)1, answers[1].Answer.HZ_QuestionOrder);

			AssertEquals("Q2", answers[2].Question.HY_Question);
			AssertEquals((short)2, answers[2].Answer.HZ_QuestionOrder);

			AssertEquals("Q3", answers[3].Question.HY_Question);
			AssertEquals((short)3, answers[3].Answer.HZ_QuestionOrder);

			AssertEquals("Header at the end", answers[4].Question.HY_Question);
			AssertEquals((short)4, answers[4].Answer.HZ_QuestionOrder);
		}

		void SetUpQuestion(GlbCompanyCampaign campaign, string questionText, string answerType, ZShort questionOrder, bool hasSubQuestions = false, bool useSubQuestionOrdering = false)
		{
			VoteExamSurveyQuestion question = null;

			if (useSubQuestionOrdering)
			{
				question = campaign.VoteHeader.SubQuestions.AddNew();
			}
			else
			{
				question = campaign.Questions.AddNew();
			}

			question.HY_Question = questionText;
			question.HY_AnswerType = answerType;

			if (useSubQuestionOrdering)
			{
				question.HY_SubQuestionOrder = questionOrder;
			}
			else
			{
				question.HY_QuestionOrder = questionOrder;
			}

			if (hasSubQuestions)
			{
				var option1 = question.SubQuestions.AddNew();
				option1.HY_Question = questionText + "_1";
				option1.HY_QuestionOrder = questionOrder;
				option1.HY_SubQuestionOrder = 1;
				var option2 = question.SubQuestions.AddNew();
				option2.HY_Question = questionText + "_2";
				option2.HY_QuestionOrder = questionOrder;
				option2.HY_SubQuestionOrder = 2;
				var option3 = question.SubQuestions.AddNew();
				option3.HY_Question = questionText + "_3";
				option3.HY_QuestionOrder = questionOrder;
				option3.HY_SubQuestionOrder = 3;
				var option4 = question.SubQuestions.AddNew();
				option4.HY_Question = questionText + "_4";
				option4.HY_QuestionOrder = questionOrder;
				option4.HY_SubQuestionOrder = 4;
			}
		}

		[TestDate(2006, 1, 1)]
		public void TestSubmitAnswerSet()
		{
			SetupBusinessObject();
			AssertEquals("Pre-condition", ZDateTime.Empty, CampaignItem.G8_ClosedDateUtc);
			AssertEquals("Pre-condition", "", CampaignItem.G8_Stage);
			((IVoteExamSurveyAnswerSet)AnswerSet).SubmitAnswerSet();
			AssertEquals(ZDateTime.UtcNow, CampaignItem.G8_ClosedDateUtc);
			AssertEquals(GlbCompanyCampaignItemLookups.StagesConstants.Submitted, CampaignItem.G8_Stage);
		}

		[TestDate(2006, 1, 1)]
		public virtual void TestClearSubmissionDate()
		{
			// See VoteSurveyExamManager.SaveAnswersOnSubmitButtonClickCore or VoteSurveyExamManager.SaveAnswersOnTimeOutCore
			// It submits the answer set and then tries to save. If the save fails, Default.HandleSubmissionResult() will invoke ClearSubmissionDate
			SetupBusinessObject();
			CampaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
			CampaignItem.G8_Stage = GlbCompanyCampaignItemLookups.StagesConstants.Submitted;
			((IVoteExamSurveyAnswerSet)AnswerSet).ClearSubmissionDate();
			AssertEquals(ZDateTime.Empty, CampaignItem.G8_ClosedDateUtc);
			AssertEquals("Reverted", GlbCompanyCampaignItemLookups.StagesConstants.Taken, CampaignItem.G8_Stage);
		}

		public virtual void TestSubmissionConfirmationMessage()
		{
			SetupBusinessObject();
			AssertEquals("Once submitted, you will not be able to modify your answers. Are you sure?",
				((IVoteExamSurveyAnswerSet)AnswerSet).SubmissionConfirmationMessage);
		}

		public virtual void TestHasPreviousSessionEnded()
		{
			SetupBusinessObject();
			Assert(!((IVoteExamSurveyAnswerSet)AnswerSet).HasPreviousSessionEnded);

			CampaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
			Assert(((IVoteExamSurveyAnswerSet)AnswerSet).HasPreviousSessionEnded);
		}

		public virtual void TestCanAutoStartVoteExamSurvey()
		{
			SetupBusinessObject();
			Assert(!((IVoteExamSurveyAnswerSet)AnswerSet).CanAutoStartVoteExamSurvey);
		}

		public virtual void TestAutoSaveAnswers()
		{
			SetupBusinessObject();
			Assert(!((IVoteExamSurveyAnswerSet)AnswerSet).AutoSaveAnswers);
		}

		public virtual void TestRemainingDuration()
		{
			SetupBusinessObject();
			AssertEquals(TimeSpan.Zero, ((IVoteExamSurveyAnswerSet)AnswerSet).RemainingDuration);
		}

		public virtual void TestIsContinuingPreviousAttempt()
		{
			SetupBusinessObject();
			Assert(!((IVoteExamSurveyAnswerSet)AnswerSet).IsContinuingPreviousAttempt);
		}

		#endregion
		#region Paging Support
		public virtual void TestHtmlSubmissionCompletedMessage()
		{
			var campaign1 = (GlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			AssertEquals("Your answers have been successfully submitted to our server. Thank you for participating.", voteExamSurveyAnswerSet.HtmlSubmissionCompletedMessage);
		}

		public void TestVoteExamSurveyCampaignURL()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://meh");
			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			string expectedURL = VoteExamSurveyUrlHelper.GetCampaignUrl(campaignItem);
			AssertEquals(expectedURL, voteExamSurveyAnswerSet.VoteExamSurveyCampaignURL);
		}

		public void TestSubmittedAnswers()
		{
			SetupBusinessObject();
			AssertEquals(ExpectedSubmittedAnswersType, CampaignItem.SubmittedAnswers.GetType());
			AssertEquals("Should be assigned in the constructor", CampaignItem, AnswerSet.SubmittedAnswers.campaignItems.First());
			Assert("Should be lazy loaded", AnswerSet.SubmittedAnswers.IsLoaded);
		}

		public void TestPersistedAnswers()
		{
			SetupBusinessObject();
			AssertEquals(typeof(VoteExamSurveyAnswerCollectionDictionary), AnswerSet.PersistedAnswers.GetType());
			AssertEquals(typeof(VoteExamSurveyAnswerCollection), AnswerSet.PersistedAnswers.GetCampaignItemAnswers(CampaignItem).GetType());
			AssertEquals("Should be assigned in the constructor", AnswerSet, AnswerSet.PersistedAnswers.Master);
		}

		public void TestMergeAnswers()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			VoteExamSurveyQuestion surveyQuestion1 = campaign.Questions.AddNew();
			surveyQuestion1.HY_Question = "survey1";
			VoteExamSurveyQuestion surveyQuestion2 = campaign.Questions.AddNew();
			surveyQuestion2.HY_Question = "survey2";
			VoteExamSurveyQuestion surveyQuestion3 = campaign.Questions.AddNew();
			surveyQuestion3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion option1 = surveyQuestion3.SubQuestions.AddNew();
			option1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option1.HY_Question = "Option 1";
			VoteExamSurveyQuestion option2 = surveyQuestion3.SubQuestions.AddNew();
			option2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option2.HY_Question = "Option 2";
			VoteExamSurveyQuestion option3 = surveyQuestion3.SubQuestions.AddNew();
			option3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option3.HY_Question = "Option 3";
			VoteExamSurveyQuestion option4 = surveyQuestion3.SubQuestions.AddNew();
			option4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoiceOption;
			option4.HY_Question = "Option 4";
			Factory.Save();

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(surveyQuestion1, campaignItem);
			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(surveyQuestion2, campaignItem);
			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option1, campaignItem);
			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option2, campaignItem);
			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option3, campaignItem);
			voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option4, campaignItem);

			AssertEquals(1, voteExamSurveyAnswerSet.PersistedAnswers.Count);
			AssertEquals(6, voteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());
			voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion1, campaignItem).HZ_Answer = "1";
			voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion2, campaignItem).HZ_Answer = "2";
			voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option3, campaignItem).HZ_Answer = "Y";

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			GlbCompanyCampaignItem campaignItemInAnotherFactory = newFactory.Load<GlbCompanyCampaignItem>(campaignItem.PK);
			var voteExamSurveyAnswerSetInAnotherFactory = new VoteExamSurveyAnswerSet(newFactory, campaignItemInAnotherFactory);
			voteExamSurveyAnswerSetInAnotherFactory.PersistedAnswers.LoadOrCreateNew(surveyQuestion1, campaignItemInAnotherFactory);
			voteExamSurveyAnswerSetInAnotherFactory.PersistedAnswers.LoadOrCreateNew(option2, campaignItemInAnotherFactory);
			voteExamSurveyAnswerSetInAnotherFactory.PersistedAnswers.FindByQuestion(surveyQuestion1, campaignItemInAnotherFactory).HZ_Answer = "A";
			voteExamSurveyAnswerSetInAnotherFactory.PersistedAnswers.FindByQuestion(option2, campaignItemInAnotherFactory).HZ_Answer = "Y";
			newFactory.Save();

			voteExamSurveyAnswerSet.MergeAnswers();
			AssertEquals(1, voteExamSurveyAnswerSet.PersistedAnswers.Count);
			AssertEquals(6, voteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());
			AssertEquals("Q1 answer is merged", "1", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion1, campaignItem).HZ_Answer);
			AssertEquals("Q2 answer has no changes", "2", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion2, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 1 answer has no changes", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option1, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 2 answer is merged", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option2, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 3 answer has no changes", "Y", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option3, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 4 answer has no changes", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option4, campaignItem).HZ_Answer);

			AssertEquals(false, voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option1, campaignItem).IsInDatabase);
			AssertEquals(true, voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option2, campaignItem).IsInDatabase);
			AssertEquals(false, voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option3, campaignItem).IsInDatabase);
			AssertEquals(false, voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option4, campaignItem).IsInDatabase);

			Factory.Save();

			voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option3, campaignItem).HZ_Answer = "";
			voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option4, campaignItem).HZ_Answer = "Y";
			voteExamSurveyAnswerSet.MergeAnswers();
			AssertEquals(1, voteExamSurveyAnswerSet.PersistedAnswers.Count);
			AssertEquals(6, voteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());
			AssertEquals("Q1 answer has no changes", "1", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion1, campaignItem).HZ_Answer);
			AssertEquals("Q2 answer has no changes", "2", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(surveyQuestion2, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 1 answer has no changes", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option1, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 2 answer has no changes", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option2, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 3 answer is changed", "", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option3, campaignItem).HZ_Answer);
			AssertEquals("Q3 option 4 answer is changed", "Y", voteExamSurveyAnswerSet.PersistedAnswers.FindByQuestion(option4, campaignItem).HZ_Answer);
		}

		#endregion

		#region Implementation

		public GlbCompanyCampaignItem CampaignItem { get; set; }

		public VoteExamSurveyAnswerSet AnswerSet { get; set; }

		protected virtual void SetupBusinessObject()
		{
			CampaignItem = Factory.New<GlbCompanyCampaignItem>();
			CampaignItem.G8_G0 = Factory.NewWithValidTestData<GlbCompanyCampaign>().PK;
			AnswerSet = new VoteExamSurveyAnswerSet(Factory, CampaignItem);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var campaignItem = Factory.New<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = Factory.NewWithValidTestData<GlbCompanyCampaign>().PK;
			var answerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			return answerSet;
		}

		protected virtual Type ExpectedSubmittedAnswersType
		{
			get { return typeof(VoteExamSurveySubmittedAnswerCollection); }
		}

		#endregion
	}
}
