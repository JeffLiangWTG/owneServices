using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(VoteExamSurveyAnswer))]
	sealed class VoteExamSurveyAnswerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAnswerAsBool()
		{
			AssertEquals(ZBool.False, Answer.AnswerAsBool);

			Answer.HZ_Answer = "Y";
			AssertEquals(ZBool.True, Answer.AnswerAsBool);

			Answer.HZ_Answer = "n";
			AssertEquals(ZBool.False, Answer.AnswerAsBool);

			Answer.AnswerAsBool = true;
			AssertEquals("Y", Answer.HZ_Answer);

			Answer.AnswerAsBool = false;
			AssertEquals("", Answer.HZ_Answer);

			Answer.HZ_Answer = "MEH";
			AssertEquals(ZBool.False, Answer.AnswerAsBool);
		}

		public void TestAnswerAsBoolInfo()
		{
			ZWrappedPropertyInfo propertyInfo = (ZWrappedPropertyInfo)Answer.AnswerAsBoolInfo;
			AssertEquals(Answer.HZ_AnswerInfo, propertyInfo.InnerInfo);
		}

		public void TestAnswerAsInt()
		{
			AssertEquals(ZInt.Zero, Answer.AnswerAsInt);

			Answer.HZ_Answer = "12";
			AssertEquals(12, Answer.AnswerAsInt);

			Answer.HZ_Answer = "999";
			AssertEquals(999, Answer.AnswerAsInt);

			Answer.AnswerAsInt = 44;
			AssertEquals("44", Answer.HZ_Answer);

			Answer.AnswerAsInt = 0;
			AssertEquals("0", Answer.HZ_Answer);

			Answer.HZ_Answer = "MEH";
			AssertEquals(ZInt.Zero, Answer.AnswerAsInt);
		}

		public void TestAnswerAsIntInfo()
		{
			ZWrappedPropertyInfo propertyInfo = (ZWrappedPropertyInfo)Answer.AnswerAsIntInfo;
			AssertEquals(Answer.HZ_AnswerInfo, propertyInfo.InnerInfo);
		}

		public void TestHZ_Answer_ShouldNotRunExtraWebValidationIfNotInWebEnvironment()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
			VoteExamSurveyQuestion question = campaign.VoteHeader.SubQuestions.AddNew();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Answer.HZ_HY = question.PK;
			Answer.HZ_G8 = campaignItem.PK;

			VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
			question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			VoteExamSurveyAnswer answer2 = campaignItem.PersistedAnswers.LoadOrCreateNew(question2);
			answer2.HZ_Answer = "1";
			AssertNoErrors(answer2.HZ_AnswerInfo);

			Answer.HZ_Answer = "1";
			AssertNoErrors("not in web environment, should not be validated", answer2.HZ_AnswerInfo);
		}

		public void TestHZ_Answer_ShouldValidateVotingItemInWebEnvironment()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
				VoteExamSurveyQuestion question = campaign.VoteHeader.SubQuestions.AddNew();
				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
				Answer.HZ_HY = question.PK;
				Answer.HZ_G8 = campaignItem.PK;

				VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
				VoteExamSurveyAnswer answer2 = campaignItem.PersistedAnswers.LoadOrCreateNew(question2);
				answer2.HZ_Answer = "1";
				AssertNoErrors(answer2.HZ_AnswerInfo);

				Answer.HZ_Answer = "1";
				AssertHasError(answer2.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");
				AssertHasError(Answer.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");

				Answer.HZ_Answer = "2";
				AssertNoErrors(answer2.HZ_AnswerInfo);
				AssertNoErrors(Answer.HZ_AnswerInfo);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestShouldNotRunExtraWebValidationIfSuspended()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				//Multiple choice question validation
				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				VoteExamSurveyQuestion question = campaign.Questions.AddNew();
				question.HY_IsOptional = false;
				VoteExamSurveyAnswer parentAnswer = campaignItem.PersistedAnswers.LoadOrCreateNew(question);
				question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;
				question.HY_Min = question.HY_Max = 2;
				VoteExamSurveyQuestion option1 = question.SubQuestions.AddNew();
				VoteExamSurveyQuestion option2 = question.SubQuestions.AddNew();
				VoteExamSurveyQuestion option3 = question.SubQuestions.AddNew();
				parentAnswer.AnswerAsBool = true;

				VoteExamSurveyAnswer parentAnswer2 = campaignItem.PersistedAnswers.CreateNew(question);
				using (parentAnswer2.GetValidationSuspender())
				{
					parentAnswer2.AnswerAsBool = true;
					parentAnswer2.AnswerAsBool = false;
				}
				parentAnswer2.AnswerAsBool = true;

				//Voting item  validation
				GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
				campaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				campaign2.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.RankedVote;
				VoteExamSurveyQuestion question2 = campaign2.VoteHeader.SubQuestions.AddNew();
				GlbCompanyCampaignItem campaignItem2 = campaign2.CampaignsItemsSent.AddNew();
				question2.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
				VoteExamSurveyAnswer answer2 = campaignItem2.PersistedAnswers.LoadOrCreateNew(question2);

				VoteExamSurveyQuestion question3 = campaign2.VoteHeader.SubQuestions.AddNew();
				question3.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
				VoteExamSurveyAnswer answer3 = campaignItem2.PersistedAnswers.LoadOrCreateNew(question3);

				answer2.HZ_Answer = "1";
				AssertNoErrors(answer2.HZ_AnswerInfo);

				answer3.HZ_Answer = "1";
				AssertHasError(answer3.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");

				answer3.HZ_Answer = "2";
				AssertNoErrors("Pre-condition", answer3.HZ_AnswerInfo);
				using (answer3.GetValidationSuspender())
				{
					answer3.HZ_Answer = "1";
					AssertNoErrors("Validation suspended - expected no errors", answer3.HZ_AnswerInfo);

					answer3.HZ_Answer = "2";
					AssertNoErrors("Pre-condition", answer3.HZ_AnswerInfo);
				}
				answer3.HZ_Answer = "1";
				AssertHasError("Validation resumed - expected errors", answer3.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestHtmlEncodedMultipleChoiceAnswer()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			var question = campaign.Questions.AddNew();
			question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.MultipleChoice;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			var answer = campaignItem.PersistedAnswers.LoadOrCreateNew(question);
			answer.Question.HY_Question = @"Answer		Html Encoded Text
With Tabs and	Newlines and JS script <img src=x onerror=alert(document.domain)>";

			AssertEquals("Answer&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Html Encoded Text<br/>With Tabs and&nbsp;&nbsp;&nbsp;&nbsp;Newlines and JS script &lt;img src=x onerror=alert(document.domain)&gt;", answer.Text);
		}

		public void TestGetOtherVotesWithRanking()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			Answer.HZ_G8 = campaignItem.PK;
			AssertEquals("Pre-condition", 0, Answer.GetOtherVotesWithRanking("1").Length);

			Answer.HZ_Answer = "1";
			AssertEquals("Should not include itself", 0, Answer.GetOtherVotesWithRanking("1").Length);

			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			VoteExamSurveyAnswer answer2 = campaignItem.PersistedAnswers.LoadOrCreateNew(question2);
			answer2.HZ_Answer = "1";
			VoteExamSurveyAnswer[] otherAnswers = Answer.GetOtherVotesWithRanking("1");
			AssertEquals(1, otherAnswers.Length);
			Assert(((IList)otherAnswers).Contains(answer2));

			VoteExamSurveyAnswer answerNotFromThisCampaignItem = Factory.New<VoteExamSurveyAnswer>();
			answerNotFromThisCampaignItem.HZ_Answer = "1";
			AssertEquals(1, Answer.GetOtherVotesWithRanking("1").Length);
		}

		public void TestGetOtherVotesWithRanking_ShouldNotIncludeInactiveAnswers()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			Answer.HZ_G8 = campaignItem.PK;
			Answer.HZ_Answer = "1";
			VoteExamSurveyQuestion question2 = campaign.Questions.AddNew();
			VoteExamSurveyAnswer answer2 = campaignItem.PersistedAnswers.LoadOrCreateNew(question2);
			answer2.HZ_Answer = "1";
			VoteExamSurveyQuestion question3 = campaign.Questions.AddNew();
			VoteExamSurveyAnswer answer3 = campaignItem.PersistedAnswers.LoadOrCreateNew(question3);
			answer3.HZ_Answer = "1";

			VoteExamSurveyAnswer[] otherAnswers = Answer.GetOtherVotesWithRanking("1");
			AssertEquals(2, otherAnswers.Length);

			question3.HY_IsActive = false;
			otherAnswers = Answer.GetOtherVotesWithRanking("1");
			AssertEquals(1, otherAnswers.Length);
			AssertEquals(answer2, otherAnswers[0]);
		}

		[ExpectNoExceptions]
		public void TestHZ_Answer_NullQuestion()
		{
			Answer.HZ_Answer = "1";
		}

		[ExpectNoExceptions]
		public void TestGetOtherVotesWithRanking_NullCampaignItem()
		{
			Answer.GetOtherVotesWithRanking("1");
		}

		public void TestCampaignItem()
		{
			AssertNull(Answer.CampaignItem);

			GlbCompanyCampaignItem campaignItem = Factory.New<GlbCompanyCampaignItem>();
			Answer.HZ_G8 = campaignItem.PK;
			AssertEquals(campaignItem.PK, Answer.CampaignItem.PK);
		}

		public void TestQuestion()
		{
			AssertNull(Answer.Question);

			VoteExamSurveyQuestion question = Factory.New<VoteExamSurveyQuestion>();
			Answer.HZ_HY = question.PK;
			AssertEquals(question.PK, Answer.Question.PK);
		}

		#region IBindableBooleanItem tests

		public void TestBoolValue()
		{
			IBindableBooleanItem bindableBoolean = Answer;
			AssertEquals(false, bindableBoolean.BoolValue);

			Answer.AnswerAsBool = true;
			AssertEquals(true, bindableBoolean.BoolValue);

			bindableBoolean.BoolValue = false;
			AssertEquals(false, bindableBoolean.BoolValue);
		}

		public void TestBoolValueInfo()
		{
			ZWrappedPropertyInfo wrappedProperty = (ZWrappedPropertyInfo)Answer.BoolValueInfo;
			AssertEquals(Answer.HZ_AnswerInfo, wrappedProperty.InnerInfo);
		}

		public void TestText()
		{
			AssertEquals("", Answer.Text);

			Answer.HZ_HY = Factory.New<VoteExamSurveyQuestion>().PK;
			Answer.Question.HY_Question = "MEH MEH";
			AssertEquals("MEH MEH", Answer.Text);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			VoteExamSurveyAnswer result = factory.New<VoteExamSurveyAnswer>();
			result.FillWithValidTestData();
			result.HZ_Answer = "1";
			result.CampaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			result.CampaignItem.G8_RecipientID = ZGuid.NewZGuid();
			return result;
		}

		VoteExamSurveyAnswer Answer
		{
			get
			{
				if (fAnswer == null)
				{
					fAnswer = Factory.New<VoteExamSurveyAnswer>();
				}
				return fAnswer;
			}
		}

		VoteExamSurveyAnswer fAnswer;
	}
}
