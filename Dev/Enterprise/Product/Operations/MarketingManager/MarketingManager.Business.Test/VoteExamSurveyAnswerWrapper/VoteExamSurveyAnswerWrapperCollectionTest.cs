using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyAnswerWrapperCollection))]
	sealed class VoteExamSurveyAnswerWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VoteExamSurveyAnswerWrapperCollection>
	{
		public void TestLoad()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion votingItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion votingItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem2.HY_RN_NKCountryCode = "HK";

			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			AssertEquals(2, voteExamSurveyAnswerSet.AnswerWrappers.Count);
			AssertEquals(votingItem1.PK, voteExamSurveyAnswerSet.AnswerWrappers[0].Question.PK);
			AssertEquals(votingItem2.PK, voteExamSurveyAnswerSet.AnswerWrappers[1].Question.PK);

			Campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion = Campaign.Questions.AddNew();
			surveyQuestion.HY_RN_NKCountryCode = "ID";

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			voteExamSurveyAnswerSet.AnswerWrappers.Load();
			AssertEquals(1, voteExamSurveyAnswerSet.AnswerWrappers.Count);
			AssertEquals(surveyQuestion.PK, voteExamSurveyAnswerSet.AnswerWrappers[0].Question.PK);
		}

		public void TestLoad_CountrySpecificQuestions()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			VoteExamSurveyQuestion votingItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem1.HY_RN_NKCountryCode = "AU";
			VoteExamSurveyQuestion votingItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			votingItem2.HY_RN_NKCountryCode = "HK";

			var item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CurrentQuestionsCountryCode = "FJ";
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			AssertEquals(0, voteExamSurveyAnswerSet.AnswerWrappers.Count);

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CurrentQuestionsCountryCode = "AU";
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			AssertEquals(1, voteExamSurveyAnswerSet.AnswerWrappers.Count);
			AssertEquals(votingItem1.PK, voteExamSurveyAnswerSet.AnswerWrappers[0].Question.PK);

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CurrentQuestionsCountryCode = "HK";
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			AssertEquals(1, voteExamSurveyAnswerSet.AnswerWrappers.Count);
			AssertEquals(votingItem2.PK, voteExamSurveyAnswerSet.AnswerWrappers[0].Question.PK);

			Campaign.CampaignsItemsSent.RemoveAndDeleteAll();
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion = Campaign.Questions.AddNew();
			surveyQuestion.HY_RN_NKCountryCode = "ID";

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CurrentQuestionsCountryCode = "HK";
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			voteExamSurveyAnswerSet.AnswerWrappers.Load();
			AssertEquals(0, voteExamSurveyAnswerSet.AnswerWrappers.Count);

			item = Factory.New<GlbCompanyCampaignItem>();
			item.G8_G0 = Campaign.PK;
			voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, item);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).CurrentQuestionsCountryCode = "ID";
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();
			AssertEquals(1, voteExamSurveyAnswerSet.AnswerWrappers.Count);
			AssertEquals(surveyQuestion.PK, voteExamSurveyAnswerSet.AnswerWrappers[0].Question.PK);
		}

		public void TestLoad_CorrectElementType()
		{
			GlbCompanyCampaign surveyCampaign = Factory.New<GlbCompanyCampaign>();
			surveyCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			VoteExamSurveyQuestion surveyQuestion = surveyCampaign.Questions.AddNew();
			GlbCompanyCampaignItem surveyCampaignItem = surveyCampaign.CampaignsItemsSent.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, surveyCampaignItem);

			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			AssertEquals(typeof(VoteExamSurveyAnswerWrapper), voteExamSurveyAnswerSet.AnswerWrappers[0].GetType());
			AssertEquals(surveyQuestion, voteExamSurveyAnswerSet.AnswerWrappers[0].Question);
		}

		public void TestFindAnswers()
		{
			VoteExamSurveyQuestion includedQuestion1 = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyQuestion includedQuestion2 = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyQuestion excludedQuestion = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer includedAnswer1 = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(includedQuestion1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			includedAnswer1.HZ_Answer = "FOO";
			VoteExamSurveyAnswer includedAnswer2 = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(includedQuestion2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			includedAnswer2.HZ_Answer = "MEH";
			VoteExamSurveyAnswer excludedAnswer = Factory.New<VoteExamSurveyAnswer>();
			excludedAnswer.HZ_HY = excludedQuestion.PK;
			excludedAnswer.HZ_Answer = "ME";

			ZQuery query = new ZQuery(VoteExamSurveyAnswerSchema.HZ_Answer, SQLComparisonOperator.StartsWith, "ME");
			var answersFound = fCampaignItem.PersistedAnswers.Where(x => x.HZ_Answer.StartsWith("ME"));
			AssertEquals(1, answersFound.Count());
			AssertEquals(includedAnswer2.PK, answersFound.FirstOrDefault().PK);
		}

		public void TestGetCompletedAnswers()
		{
			VoteExamSurveyQuestion question1 = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer1 = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(question1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer1.HZ_Answer = "1";

			VoteExamSurveyQuestion question2 = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer2 = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(question2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer2.HZ_AnswerComment = "2";

			VoteExamSurveyQuestion question3 = Factory.New<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer3 = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(question3, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			var completedAnswers = VoteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(fCampaignItem);
			AssertEquals(2, completedAnswers.Count());
			Assert(completedAnswers.Contains(answer1));
			Assert(completedAnswers.Contains(answer2));
		}

		public void TestLoadOrCreateNewAnswer()
		{
			CampaignItem.FillWithValidTestData();
			AssertEquals(0, CampaignItem.PersistedAnswers.Find(new ZQuery()).Length);

			VoteExamSurveyQuestion question = Factory.NewWithValidTestData<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = CampaignItem.PersistedAnswers.LoadOrCreateNew(question);
			answer.HZ_Answer = "MEH";
			VoteExamSurveyAnswer[] answersFound = CampaignItem.PersistedAnswers.Find(new ZQuery());
			AssertEquals(1, answersFound.Length);
			AssertEquals(answer.PK, answersFound[0].PK);
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompanyCampaignItem campaignItemInNewFactory = newFactory.Load<GlbCompanyCampaignItem>(CampaignItem.PK);
			VoteExamSurveyAnswer answerInNewFactory = campaignItemInNewFactory.PersistedAnswers.LoadOrCreateNew(question);
			VoteExamSurveyAnswer[] answersFoundInNewFactory = campaignItemInNewFactory.PersistedAnswers.Find(new ZQuery());
			AssertEquals("Should not create a new one", 1, answersFoundInNewFactory.Length);
			AssertEquals(answerInNewFactory.PK, answersFoundInNewFactory[0].PK);
			AssertEquals(answer.PK, answerInNewFactory.PK);
		}

		public void TestCreateNewAnswer()
		{
			AssertEquals(0, VoteExamSurveyAnswerSet.PersistedAnswers.Find(new ZQuery(), fCampaignItem).Count());

			VoteExamSurveyQuestion question = Factory.NewWithValidTestData<VoteExamSurveyQuestion>();
			VoteExamSurveyAnswer answer = VoteExamSurveyAnswerSet.PersistedAnswers.CreateNew(question, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			var answersFound = VoteExamSurveyAnswerSet.PersistedAnswers.GetCampaignItemAnswers(fCampaignItem);
			AssertEquals(1, answersFound.Count);
			AssertEquals(answer.PK, answersFound[0].PK);
		}

		public void TestFindByQuestion()
		{
			VoteExamSurveyQuestion question1 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			question1.FillWithValidTestData();
			VoteExamSurveyQuestion question2 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			question2.FillWithValidTestData();
			Factory.Save();

			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();
			Factory.Save();

			AssertEquals(question1, VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(question1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).Question);
			AssertEquals(question2, VoteExamSurveyAnswerSet.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(question2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).Question);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompanyCampaignItem campaignItemInNewFactory = newFactory.Load<GlbCompanyCampaignItem>(VoteExamSurveyAnswerSet.ExamCampaignItems[0].PK);
			var wrapperInNewFactory = new VoteExamSurveyAnswerSet(newFactory, campaignItemInNewFactory);
			VoteExamSurveyQuestion question1InNewFactory = newFactory.Load<VoteExamSurveyQuestion>(question1.PK);
			VoteExamSurveyQuestion question2InNewFactory = newFactory.Load<VoteExamSurveyQuestion>(question2.PK);
			AssertEquals(question1InNewFactory, wrapperInNewFactory.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(question1InNewFactory, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).Question);
			AssertEquals(question2InNewFactory, wrapperInNewFactory.AnswerWrappers.FindByQuestion<VoteExamSurveyAnswerWrapperBase>(question2InNewFactory, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).Question);
		}

		public void TestFindByPage()
		{
			VoteExamSurveyQuestion question1 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			question1.FillWithValidTestData();
			VoteExamSurveyQuestion question2 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			question2.FillWithValidTestData();
			VoteExamSurveyQuestion question3 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			question2.FillWithValidTestData();
			Factory.Save();

			VoteExamSurveyAnswerSet.CompanyCampaign.G0_QuestionsPerWebPage = 2;
			((IVoteExamSurveyAnswerSet)VoteExamSurveyAnswerSet).StartVoteExamSurvey();

			IEnumerable<VoteExamSurveyAnswerWrapper> page1 = VoteExamSurveyAnswerSet.AnswerWrappers.FindByPage<VoteExamSurveyAnswerWrapper>(1, true);
			AssertEquals("Page 1 should contain 2 wrappers", 2, page1.Count());
			Assert("Should contain wrapper1", page1.Contains((VoteExamSurveyAnswerWrapper)VoteExamSurveyAnswerSet.AnswerWrappers[0]));
			Assert("Should contain wrapper2", page1.Contains((VoteExamSurveyAnswerWrapper)VoteExamSurveyAnswerSet.AnswerWrappers[1]));
			IEnumerable<VoteExamSurveyAnswerWrapper> page2 = VoteExamSurveyAnswerSet.AnswerWrappers.FindByPage<VoteExamSurveyAnswerWrapper>(2, true);
			AssertEquals("Page 2 Should contain 1 wrapper", 1, page2.Count());
			Assert("Should contain wrapper3", page2.Contains((VoteExamSurveyAnswerWrapper)VoteExamSurveyAnswerSet.AnswerWrappers[2]));
		}

		public void TestDeleteEmptyPersistedAnswers()
		{
			var q1 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			q1.HY_Question = "Q1";

			var q2 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			q2.HY_Question = "Q2";

			var q3 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			q3.HY_Question = "Q3";
			q3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var q3_option1 = q3.SubQuestions.AddNew();
			q3_option1.HY_Question = "Q3_1";
			var q3_option2 = q3.SubQuestions.AddNew();
			q3_option2.HY_Question = "Q3_2";

			var q4 = VoteExamSurveyAnswerSet.CompanyCampaign.Questions.AddNew();
			q4.HY_Question = "Q4";
			q4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			var q4_option1 = q4.SubQuestions.AddNew();
			q4_option1.HY_Question = "Q4_1";
			var q4_option2 = q4.SubQuestions.AddNew();
			q4_option2.HY_Question = "Q4_2";

			Factory.Save();

			var a1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			a1.HZ_AnswerComment = "Answer for Q1";

			var a2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			var a3 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q3, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			var a3_option1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q3_option1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			a3_option1.HZ_Answer = "Y";
			var a3_option2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q3_option2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			var a4 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q4, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			var a4_option1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q4_option1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			var a4_option2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(q4_option2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			AssertEquals(8, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());

			VoteExamSurveyAnswerSet.AnswerWrappers.DeleteEmptyPersistedAnswers();
			AssertEquals(4, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers.Count());
			AssertCollectionContains(a1, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers);
			AssertCollectionContains(a2, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers);
			AssertCollectionContains(a3_option1, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers);
			AssertCollectionContains(a4, VoteExamSurveyAnswerSet.PersistedAnswers.AllPersistedAnswers);
		}

		[ExpectNoExceptions]
		public void TestLoad_NullCampaign()
		{
			GlbCompanyCampaignItem campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			voteExamSurveyAnswerSet.AnswerWrappers.Load();
		}

		public void TestIsLoaded()
		{
			Assert("Pre-condition", !Collection.IsLoaded);

			Collection.Load();
			Assert(Collection.IsLoaded);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
			Assert(true);
		}

		#region Randomise Questions

		public void TestRandomiseQuestion_IncludingQuestionHeaders()
		{
			var item = CreateTestCampaignItemForRandomTest(true, true, 0);
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Section",
			  "Question 3",
			  "Question 1",
			  "Question 2",
			  "Section",
			  "Question 6",
			  "Question 4",
			  "Question 5",
			  "Section",
			  "Question 7",
			  "Question 8",
			  "Question 9");
		}

		public void TestRandomiseQuestions_NoQuestionHeader()
		{
			var item = CreateTestCampaignItemForRandomTest(false, true, 0);
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Question 6",
			  "Question 4",
			  "Question 3",
			  "Question 1",
			  "Question 5",
			  "Question 7",
			  "Question 2",
			  "Question 8",
			  "Question 9");
		}

		public void TestNonRandomisedQuestions_IncludingQuestionHeader()
		{
			var item = CreateTestCampaignItemForRandomTest(true, false, 0);
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Section",
			  "Question 1",
			  "Question 2",
			  "Question 3",
			  "Section",
			  "Question 4",
			  "Question 5",
			  "Question 6",
			  "Section",
			  "Question 7",
			  "Question 8",
			  "Question 9");
		}

		public void TestNonRandomisedQuestions_NoQuestionHeader()
		{
			var item = CreateTestCampaignItemForRandomTest(false, false, 0);
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Question 1",
			  "Question 2",
			  "Question 3",
			  "Question 4",
			  "Question 5",
			  "Question 6",
			  "Question 7",
			  "Question 8",
			  "Question 9");
		}

		public void TestRandomiseQuestion_CountrySpecificIncludingQuestionHeaderWithMaxCount()
		{
			var item = CreateTestCampaignItemForRandomTest(true, true, 6);
			((IVoteExamSurveyAnswerSet)item).CurrentQuestionsCountryCode = "NZ";
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Section",
			  "Question 2",
			  "Question 3",
			  "Section",
			  "Question 6",
			  "Question 5",
			  "Question 4",
			  "Section",
			  "Question 7");
		}

		public void TestNonRandomisedQuestions_CountrySpecific()
		{
			var item = CreateTestCampaignItemForRandomTest(false, false, 20);
			((IVoteExamSurveyAnswerSet)item).CurrentQuestionsCountryCode = "AU";
			((IVoteExamSurveyAnswerSet)item).StartVoteExamSurvey();
			AssertQuestions(item,
			  "Question 1",
			  "Question 2",
			  "Question 6",
			  "Question 7",
			  "Question 8",
			  "Question 9");
		}

		int codeIndex;

		void AddSettings(short maxQuestions, GlbCompanyCampaign campaign)
		{
			var settings = Factory.New<IExamSetting>();
			settings.EXS_Code = "~settings" + codeIndex;
			settings.EXS_MaximumAskedQuestionsPerExam = maxQuestions;
			settings.EXS_G0 = campaign.PK;
			settings.EXS_ExamVersion = "STD";
			codeIndex++;
		}

		VoteExamSurveyAnswerSet CreateTestCampaignItemForRandomTest(bool addQuestionHeaders, bool randomiseQuestions, short maxCount)
		{
			GlbCompanyCampaignItemForRandomTest campaignItem = Factory.New<GlbCompanyCampaignItemForRandomTest>();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			GlbCompanyCampaignForRandomTest campaign = Factory.NewWithValidTestData<GlbCompanyCampaignForRandomTest>();

			campaign.SetCurrentSettings(new GlbCompanyCampaign.CampaignSettings("STD"));
			campaign.RandomiseQuestionAndMultipleChoiceOrderForTest = randomiseQuestions;
			AddSettings(maxCount, campaign);

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);

			if (addQuestionHeaders)
			{
				AddQuestionForRandomTest(campaign, "Section", VoteExamSurveyAnswerTypeList.Codes.Header);
			}
			AddQuestionForRandomTest(campaign, "Question 1", VoteExamSurveyAnswerTypeList.Codes.NumericScale, "AU");
			AddQuestionForRandomTest(campaign, "Question 2", VoteExamSurveyAnswerTypeList.Codes.TrueFalse);
			AddQuestionForRandomTest(campaign, "Question 3", VoteExamSurveyAnswerTypeList.Codes.Percentage, "NZ");

			if (addQuestionHeaders)
			{
				AddQuestionForRandomTest(campaign, "Section", VoteExamSurveyAnswerTypeList.Codes.Header);
			}
			AddQuestionForRandomTest(campaign, "Question 4", VoteExamSurveyAnswerTypeList.Codes.NumericScale, "NZ");
			AddQuestionForRandomTest(campaign, "Question 5", VoteExamSurveyAnswerTypeList.Codes.NumericScale, "NZ");
			AddQuestionForRandomTest(campaign, "Question 6", VoteExamSurveyAnswerTypeList.Codes.NumericScale);

			if (addQuestionHeaders)
			{
				AddQuestionForRandomTest(campaign, "Section", VoteExamSurveyAnswerTypeList.Codes.Header);
			}
			AddQuestionForRandomTest(campaign, "Question 7", VoteExamSurveyAnswerTypeList.Codes.TrueFalse);
			AddQuestionForRandomTest(campaign, "Question 8", VoteExamSurveyAnswerTypeList.Codes.TrueFalse, "AU");
			AddQuestionForRandomTest(campaign, "Question 9", VoteExamSurveyAnswerTypeList.Codes.Percentage, "AU");

			campaignItem.G8_G0 = campaign.PK;

			Factory.Save();

			return voteExamSurveyAnswerSet;
		}

		void AssertQuestions(VoteExamSurveyAnswerSet item, params string[] questionTexts)
		{
			AssertQuestionsCore(item, questionTexts);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var campaignItemInNewFactory = newFactory.Load<GlbCompanyCampaignItemForRandomTest>(item.ExamCampaignItems[0].PK);
			((GlbCompanyCampaignForRandomTest)campaignItemInNewFactory.CompanyCampaign).RandomiseQuestionAndMultipleChoiceOrderForTest = item.CompanyCampaign.RandomiseQuestionAndMultipleChoiceOrder;
			var newWrapperWithReloadedItem = new VoteExamSurveyAnswerSet(newFactory, campaignItemInNewFactory);
			CombineAssertions("AssertQuestions after reloading from another factory", () => AssertQuestionsCore(newWrapperWithReloadedItem, questionTexts));
		}

		void AssertQuestionsCore(VoteExamSurveyAnswerSet item, params string[] questionTexts)
		{
			var isRandomised = item.CompanyCampaign.RandomiseQuestionAndMultipleChoiceOrder;
			AssertEquals(questionTexts.Length, item.AnswerWrappers.Count);
			AssertEquals("G8_Stage", true, item.IsStageTaken);
			StringBuilder builder = new StringBuilder();
			ZShort expectedQuestionOrder = 0;
			for (int index = 0; index < item.AnswerWrappers.Count; index++)
			{
				var wrapper = item.AnswerWrappers[index];
				if (!wrapper.Question.IsHeader || index == item.AnswerWrappers.Count - 1)
				{
					expectedQuestionOrder++;
				}
				AssertEquals("HZ_QuestionOrder", expectedQuestionOrder, wrapper.Answer.HZ_QuestionOrder);
				builder.Append(wrapper.Question.HY_Question);
				builder.Append("\r\n");
			}
			string expected = string.Join("\r\n", questionTexts);
			string actual = builder.ToString().Trim();

			string failureMessage = string.Format("Incorrect Order.\r\nExpected (full):\r\n{0}\r\n\r\nActual (full):\r\n{1}\r\n\r\n", expected, actual);
			AssertMultilineEquals(failureMessage, expected, actual, '\n');
		}

		void AddQuestionForRandomTest(GlbCompanyCampaign campaign, string questionText, string answerType)
		{
			AddQuestionForRandomTest(campaign, questionText, answerType, "");
		}

		void AddQuestionForRandomTest(GlbCompanyCampaign campaign, string questionText, string answerType, string countryCode)
		{
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();
			question.HY_Question = questionText;
			question.HY_AnswerType = answerType;
			question.HY_RN_NKCountryCode = countryCode;
			question.HY_QuestionOrder = (short)campaign.Questions.Count;
		}

		class GlbCompanyCampaignItemForRandomTest : GlbCompanyCampaignItem
		{
			public GlbCompanyCampaignItemForRandomTest(BusinessObjectFactory factory, DataRow row)
			  : base(factory, row)
			{
			}

			public override GlbCompanyCampaign CompanyCampaign
			{
				get { return Factory.Load<GlbCompanyCampaignForRandomTest>(G8_G0); }
			}
		}

		class GlbCompanyCampaignForRandomTest : GlbCompanyCampaign
		{
			public GlbCompanyCampaignForRandomTest(BusinessObjectFactory factory, DataRow row)
			  : base(factory, row)
			{
			}

			public bool RandomiseQuestionAndMultipleChoiceOrderForTest
			{
				get;
				set;
			}

			public override bool RandomiseQuestionAndMultipleChoiceOrder
			{
				get { return RandomiseQuestionAndMultipleChoiceOrderForTest; }
			}
		}

		#endregion

		#region Voting

		public void TestQuestionsOrderVotingCampaign()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			var votingItem1 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem2 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem3 = campaign.VoteHeader.SubQuestions.AddNew();

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			var answersList = campaignItem.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().ToList();
			AssertEquals(3, answersList.Count);

			var answer1 = answersList.First(a => a.Question.PK == votingItem1.PK).Answer;
			AssertEquals(answer1.Question.ActualOrder, votingItem1.ActualOrder);

			var answer2 = answersList.First(a => a.Question.PK == votingItem2.PK).Answer;
			AssertEquals(answer2.Question.ActualOrder, votingItem2.ActualOrder);

			var answer3 = answersList.First(a => a.Question.PK == votingItem3.PK).Answer;
			AssertEquals(answer3.Question.ActualOrder, votingItem3.ActualOrder);
		}

		public void TestQuestionsOrderVotingCampaign_IncludingQuestionHeaders()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			var header1 = campaign.VoteHeader.SubQuestions.AddNew();
			header1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;

			var votingItem1 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem2 = campaign.VoteHeader.SubQuestions.AddNew();

			var header2 = campaign.VoteHeader.SubQuestions.AddNew();
			header2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;

			var votingItem3 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem4 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem5 = campaign.VoteHeader.SubQuestions.AddNew();

			var header3 = campaign.VoteHeader.SubQuestions.AddNew();
			header3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;

			var votingItem6 = campaign.VoteHeader.SubQuestions.AddNew();
			var votingItem7 = campaign.VoteHeader.SubQuestions.AddNew();

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_G0 = campaign.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = ZGuid.NewZGuid();
			Factory.Save();

			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			((IVoteExamSurveyAnswerSet)voteExamSurveyAnswerSet).StartVoteExamSurvey();

			var answersList = campaignItem.AnswerWrappers.OfType<VoteExamSurveyAnswerWrapper>().ToList();
			AssertEquals(10, answersList.Count);

			var headerAnswer1 = answersList.First(a => a.Question.PK == header1.PK).Answer;
			AssertEquals(headerAnswer1.HZ_QuestionOrder, header1.ActualOrder - 1);

			var answer1 = answersList.First(a => a.Question.PK == votingItem1.PK).Answer;
			AssertEquals(answer1.Question.ActualOrder, votingItem1.ActualOrder);

			var answer2 = answersList.First(a => a.Question.PK == votingItem2.PK).Answer;
			AssertEquals(answer2.Question.ActualOrder, votingItem2.ActualOrder);

			var headerAnswer2 = answersList.First(a => a.Question.PK == header2.PK).Answer;
			AssertEquals(headerAnswer2.HZ_QuestionOrder, header2.ActualOrder - 1);

			var answer3 = answersList.First(a => a.Question.PK == votingItem3.PK).Answer;
			AssertEquals(answer3.Question.ActualOrder, votingItem3.ActualOrder);

			var answer4 = answersList.First(a => a.Question.PK == votingItem4.PK).Answer;
			AssertEquals(answer4.Question.ActualOrder, votingItem4.ActualOrder);

			var answer5 = answersList.First(a => a.Question.PK == votingItem5.PK).Answer;
			AssertEquals(answer5.Question.ActualOrder, votingItem5.ActualOrder);

			var headerAnswer3 = answersList.First(a => a.Question.PK == header3.PK).Answer;
			AssertEquals(headerAnswer3.HZ_QuestionOrder, header3.ActualOrder - 1);

			var answer6 = answersList.First(a => a.Question.PK == votingItem6.PK).Answer;
			AssertEquals(answer6.Question.ActualOrder, votingItem6.ActualOrder);

			var answer7 = answersList.First(a => a.Question.PK == votingItem7.PK).Answer;
			AssertEquals(answer7.Question.ActualOrder, votingItem7.ActualOrder);
		}

		#endregion

		protected override VoteExamSurveyAnswerWrapperCollection GetCollectionToTest()
		{
			return new VoteExamSurveyAnswerWrapperCollection(VoteExamSurveyAnswerSet, VoteExamSurveyAnswerSet.ExamCampaignItems[0], Campaign.G0_QuestionsPerWebPage);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			return new VoteExamSurveyAnswerWrapper(question, VoteExamSurveyAnswerSet, VoteExamSurveyAnswerSet.ExamCampaignItems[0], Campaign.G0_QuestionsPerWebPage);
		}

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
				}
				return fCampaign;
			}
		}

		GlbCompanyCampaignItem CampaignItem
		{
			get
			{
				if (fCampaignItem == null || fCampaignItem.IsDeleted)
				{
					fCampaignItem = Campaign.CampaignsItemsSent.AddNew();
					fCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
					fCampaignItem.G8_RecipientID = ZGuid.NewZGuid();
				}
				return fCampaignItem;
			}
		}

		VoteExamSurveyAnswerSet VoteExamSurveyAnswerSet
		{
			get
			{
				if (fVoteExamSurveyAnswerSet == null)
				{
					fCampaignItem = Campaign.CampaignsItemsSent.AddNew();
					fCampaignItem.FillWithValidTestData();
					fCampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
					fCampaignItem.G8_RecipientID = ZGuid.NewZGuid();
					fVoteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, fCampaignItem);
				}
				return fVoteExamSurveyAnswerSet;
			}
		}

		GlbCompanyCampaign fCampaign;
		VoteExamSurveyAnswerSet fVoteExamSurveyAnswerSet;
		GlbCompanyCampaignItem fCampaignItem;
	}
}
