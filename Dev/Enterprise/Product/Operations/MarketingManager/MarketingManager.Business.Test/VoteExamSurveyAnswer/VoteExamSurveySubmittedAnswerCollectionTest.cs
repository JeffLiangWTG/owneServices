using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveySubmittedAnswerCollection))]
	sealed class VoteExamSurveySubmittedAnswerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<VoteExamSurveySubmittedAnswerCollection>
	{
		#region TestLoad_RankedVote

		public void TestLoad_RankedVote()
		{
			SetupCampaignForTestLoad_RankedVote();
			AssertEquals("Pre-condition", 0, Collection.Count);
			Collection.Load();
			AssertEquals(3, Collection.Count);
			AssertEquals("Item 1", Collection[0].Question.HY_Question);
			AssertEquals("Item 4", Collection[1].Question.HY_Question);
			AssertEquals("Item 2", Collection[2].Question.HY_Question);
		}

		void SetupCampaignForTestLoad_RankedVote()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			Campaign.VoteHeader.HY_Min = 3;
			Campaign.VoteHeader.HY_Max = 3;

			VoteExamSurveyQuestion voteItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem1.HY_Question = "Item 1";
			VoteExamSurveyQuestion voteItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem2.HY_Question = "Item 2";
			VoteExamSurveyQuestion voteItem3 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem3.HY_Question = "Item 3";
			VoteExamSurveyQuestion voteItem4 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem4.HY_Question = "Item 4";
			VoteExamSurveyQuestion header = Campaign.VoteHeader.SubQuestions.AddNew();
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			header.HY_Question = "Header";

			VoteExamSurveyAnswer votedItem1 = CampaignItem.PersistedAnswers.LoadOrCreateNew(voteItem1);
			votedItem1.AnswerAsInt = 1;
			VoteExamSurveyAnswer votedItem2 = CampaignItem.PersistedAnswers.LoadOrCreateNew(voteItem2);
			votedItem2.AnswerAsInt = 3;
			VoteExamSurveyAnswer votedItem3 = CampaignItem.PersistedAnswers.LoadOrCreateNew(voteItem4);
			votedItem3.AnswerAsInt = 2;
		}

		#endregion

		#region TestLoad_UnrankedVote

		public void TestLoad_UnrankedVote()
		{
			SetupCampaignForTestLoad_UnrankedVote();
			AssertEquals("Pre-condition", 0, Collection.Count);
			Collection.Load();
			AssertEquals(2, Collection.Count);
			AssertEquals(1, Collection.Count(s => ((VoteExamSurveySubmittedAnswer)s).Question.HY_Question == "Item 1"));
			AssertEquals(1, Collection.Count(s => ((VoteExamSurveySubmittedAnswer)s).Question.HY_Question == "Item 3"));
		}

		void SetupCampaignForTestLoad_UnrankedVote()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;
			Campaign.VoteHeader.HY_Min = 2;
			Campaign.VoteHeader.HY_Max = 2;

			VoteExamSurveyQuestion voteItem1 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem1.HY_Question = "Item 1";
			VoteExamSurveyQuestion voteItem2 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem2.HY_Question = "Item 2";
			VoteExamSurveyQuestion voteItem3 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem3.HY_Question = "Item 3";
			VoteExamSurveyQuestion voteItem4 = Campaign.VoteHeader.SubQuestions.AddNew();
			voteItem4.HY_Question = "Item 4";
			VoteExamSurveyQuestion header = Campaign.VoteHeader.SubQuestions.AddNew();
			header.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;
			header.HY_Question = "Header";

			VoteExamSurveyAnswer votedItem1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(voteItem3, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			votedItem1.AnswerAsBool = true;
			VoteExamSurveyAnswer votedItem2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(voteItem1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			votedItem2.AnswerAsBool = true;
		}

		#endregion

		#region Test Elements

		public void TestLoad_ExamSurveyAnswers()
		{
			SetupCampaignForTestLoad_ExamSurveyAnswers();
			AssertEquals("Pre-condition", 0, Collection.Count);
			Collection.Load();
			AssertEquals(5, Collection.Count);
			AssertEquals("Question1", Collection[0].Question.HY_Question);
			AssertEquals("Answer 1", Collection[0].Answer);
			AssertEquals("Question2", Collection[1].Question.HY_Question);
			AssertEquals("Disagree", Collection[1].Answer);
			AssertEquals("Question5", Collection[2].Question.HY_Question);
			AssertEquals(75, Collection[2].AnswerAsInt);
			AssertEquals("Question6", Collection[3].Question.HY_Question);
			AssertEquals("Yes", Collection[3].Answer);
			AssertEquals("Question8", Collection[4].Question.HY_Question);
			AssertEquals("1) Option 1\r\n4) Option 4\r\n", Collection[4].Answer);
		}

		public void TestLoadedElements()
		{
			SetupSurveyCampaignForElementTests();
			Collection.Load();
			AssertEquals(4, Collection.Count);
			AssertEquals(0, Collection.PopulatedAnswers.Count());

			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Collection[0].Question, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).HZ_Answer = "2";
			AssertEquals(4, Collection.Count);
			AssertEquals(1, Collection.PopulatedAnswers.Count());

			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Collection[1].Question, VoteExamSurveyAnswerSet.ExamCampaignItems[0]).HZ_Answer = "2";
			AssertEquals(4, Collection.Count);
			AssertEquals(2, Collection.PopulatedAnswers.Count());

			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Collection[2].Question.SubQuestions[0], VoteExamSurveyAnswerSet.ExamCampaignItems[0]).AnswerAsBool = true;
			AssertEquals(4, Collection.Count);
			AssertEquals(3, Collection.PopulatedAnswers.Count());

			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(Collection[3].Question.SubQuestions[0], VoteExamSurveyAnswerSet.ExamCampaignItems[0]).AnswerAsBool = true;
			AssertEquals(4, Collection.Count);
			AssertEquals(4, Collection.PopulatedAnswers.Count());
		}

		void SetupSurveyCampaignForElementTests()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion trueFalseQuestion1 = Campaign.Questions.AddNew();
			trueFalseQuestion1.HY_Question = "Question1";
			trueFalseQuestion1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(trueFalseQuestion1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			VoteExamSurveyQuestion trueFalseQuestion2 = Campaign.Questions.AddNew();
			trueFalseQuestion2.HY_Question = "Question2";
			trueFalseQuestion2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;
			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(trueFalseQuestion2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			VoteExamSurveyQuestion multipleChoiceQuestion1 = Campaign.Questions.AddNew();
			multipleChoiceQuestion1.HY_Question = "Question3";
			multipleChoiceQuestion1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion q3option1 = multipleChoiceQuestion1.SubQuestions.AddNew();
			VoteExamSurveyQuestion q3option2 = multipleChoiceQuestion1.SubQuestions.AddNew();
			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(multipleChoiceQuestion1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);

			VoteExamSurveyQuestion multipleChoiceQuestion2 = Campaign.Questions.AddNew();
			multipleChoiceQuestion2.HY_Question = "Question4";
			multipleChoiceQuestion2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			VoteExamSurveyQuestion q4option1 = multipleChoiceQuestion2.SubQuestions.AddNew();
			VoteExamSurveyQuestion q4option2 = multipleChoiceQuestion2.SubQuestions.AddNew();
			VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(multipleChoiceQuestion2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
		}

		void SetupCampaignForTestLoad_ExamSurveyAnswers()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion question1 = Campaign.Questions.AddNew();
			question1.HY_Question = "Question1";
			question1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.FreeText;
			VoteExamSurveyAnswer answer1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer1.HZ_AnswerComment = "Answer 1";

			VoteExamSurveyQuestion question2 = Campaign.Questions.AddNew();
			question2.HY_Question = "Question2";
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.LikertScale;
			VoteExamSurveyAnswer answer2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer2.AnswerAsInt = 2;

			VoteExamSurveyQuestion question3 = Campaign.Questions.AddNew();
			question3.HY_Question = "Question3";
			question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Header;

			VoteExamSurveyQuestion question4 = Campaign.Questions.AddNew();
			question4.HY_Question = "Question4";
			question4.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.NumericScale;

			VoteExamSurveyQuestion question5 = Campaign.Questions.AddNew();
			question5.HY_Question = "Question5";
			question5.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.Percentage;
			VoteExamSurveyAnswer answer5 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question5, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer5.AnswerAsInt = 75;

			VoteExamSurveyQuestion question6 = Campaign.Questions.AddNew();
			question6.HY_Question = "Question6";
			question6.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.YesNo;
			VoteExamSurveyAnswer answer6 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question6, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer6.AnswerAsInt = 1;

			VoteExamSurveyQuestion question7 = Campaign.Questions.AddNew();
			question7.HY_Question = "Question7";
			question7.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;

			VoteExamSurveyQuestion question8 = Campaign.Questions.AddNew();
			question8.HY_Question = "Question8";
			question8.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
			question8.HY_Min = 2;
			question8.HY_Max = 2;
			VoteExamSurveyQuestion option1 = question8.SubQuestions.AddNew();
			option1.HY_Question = "Option 1";
			VoteExamSurveyQuestion option2 = question8.SubQuestions.AddNew();
			option2.HY_Question = "Option 2";
			VoteExamSurveyQuestion option3 = question8.SubQuestions.AddNew();
			option3.HY_Question = "Option 3";
			VoteExamSurveyQuestion option4 = question8.SubQuestions.AddNew();
			option4.HY_Question = "Option 4";
			VoteExamSurveyAnswer answerOption1 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answerOption1.AnswerAsBool = true;
			VoteExamSurveyAnswer answerOption2 = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(option4, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answerOption2.AnswerAsBool = true;
		}

		#endregion

		public void TestShouldRemoveElementsBeforeLoad()
		{
			SetupCampaignForTestLoad_RankedVote();
			Collection.Add(Factory.New<VoteExamSurveyQuestion>());
			AssertEquals("Pre-condition", 1, Collection.Count);

			Collection.Load();
			AssertEquals(3, Collection.Count);
			AssertEquals("Item 1", Collection[0].Question.HY_Question);
			AssertEquals("Item 4", Collection[1].Question.HY_Question);
			AssertEquals("Item 2", Collection[2].Question.HY_Question);
		}

		public void TestAllowSort()
		{
			Assert(((IBindingList)Collection).SupportsSorting);
		}

		public void TestIsLoaded()
		{
			Assert("Pre-condition", !Collection.IsLoaded);

			Collection.Load();
			Assert(Collection.IsLoaded);
		}

		[ExpectNoExceptions]
		public void TestDoesNotBlowUpWhenCampaignIsNull()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			VoteExamSurveySubmittedAnswerCollection collection1 = new VoteExamSurveySubmittedAnswerCollection(campaignItem);
			collection1.Load();
			AssertEquals(0, collection1.PopulatedAnswers.Count());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
			Assert(true);
		}

		public void TestPopulatedAnswersCountIsCached()
		{
			Campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			VoteExamSurveyQuestion trueFalseQuestion1 = Campaign.Questions.AddNew();
			trueFalseQuestion1.HY_Question = "Question1";
			trueFalseQuestion1.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.TrueFalse;

			VoteExamSurveyAnswer answer = VoteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(trueFalseQuestion1, VoteExamSurveyAnswerSet.ExamCampaignItems[0]);
			answer.HZ_Answer = "2";
			Collection.Load();
			AssertEquals(1, Collection.PopulatedAnswersCount);

			answer.HZ_Answer = "";
			AssertEquals("Cached", 1, Collection.PopulatedAnswersCount);
			AssertEquals(0, Collection.PopulatedAnswers.Count());
		}

		#region Implementation

		protected override VoteExamSurveySubmittedAnswerCollection GetCollectionToTest()
		{
			return new VoteExamSurveySubmittedAnswerCollection(CampaignItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			VoteExamSurveyQuestion question = Campaign.Questions.AddNew();
			return new VoteExamSurveySubmittedAnswer(CampaignItem, question);
		}

		new VoteExamSurveySubmittedAnswerCollection Collection
		{
			get { return base.Collection; }
		}

		VoteExamSurveyAnswerSet VoteExamSurveyAnswerSet
		{
			get
			{
				if (fVoteExamSurveyAnswerSet == null)
				{
					fVoteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, CampaignItem);
				}
				return fVoteExamSurveyAnswerSet;
			}
		}

		GlbCompanyCampaignItem CampaignItem
		{
			get
			{
				if (fCampaignItem == null)
				{
					fCampaignItem = Campaign.CampaignsItemsSent.AddNew();
				}
				return fCampaignItem;
			}
		}

		GlbCompanyCampaign Campaign
		{
			get
			{
				if (fCampaign == null)
				{
					fCampaign = Factory.New<GlbCompanyCampaign>();
				}
				return fCampaign;
			}
		}

		GlbCompanyCampaignItem fCampaignItem;
		VoteExamSurveyAnswerSet fVoteExamSurveyAnswerSet;
		GlbCompanyCampaign fCampaign;

		#endregion
	}
}
