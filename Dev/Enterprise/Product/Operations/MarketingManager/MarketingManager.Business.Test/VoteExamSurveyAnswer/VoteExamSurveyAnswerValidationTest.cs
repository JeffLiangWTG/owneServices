using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyAnswerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateHZ_Answer_NotInWebEnvironment()
		{
			SetupBizOsForVotingTest(VoteExamSurveyAnswerTypeList.Codes.RankedVote, 3);
			Answer.Validation.ValidateHZ_Answer();
			AssertNoErrors("Should not have any errors when not filled in", Answer.HZ_AnswerInfo);

			Answer.AnswerAsInt = 4;
			AssertNoErrors(Answer.HZ_AnswerInfo);
		}

		public void TestValidateHZ_Answer()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				SetupBizOsForVotingTest(VoteExamSurveyAnswerTypeList.Codes.RankedVote, 3);
				Answer.Validation.ValidateHZ_Answer();
				AssertNoErrors("Should not have any errors when not filled in", Answer.HZ_AnswerInfo);

				Answer.AnswerAsInt = 3;
				AssertNoErrors("Valid Answer", Answer.HZ_AnswerInfo);

				Answer.AnswerAsInt = 4;
				AssertHasErrors(Answer.HZ_AnswerInfo);

				Answer.AnswerAsInt = 1;
				AssertNoErrors("Valid Answer", Answer.HZ_AnswerInfo);

				VoteExamSurveyQuestion anotherQuestion = Campaign.VoteHeader.SubQuestions.AddNew();
				var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, CampaignItem);
				VoteExamSurveyAnswer anotherAnswer = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(anotherQuestion, CampaignItem);
				anotherAnswer.AnswerAsInt = 1;
				AssertHasError(Answer.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");
				AssertHasError(anotherAnswer.HZ_AnswerInfo, "Rank number 1 is nominated to more than one voting item");
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestCheckHZ_AnswerCommentIsWesternEuropean()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				SetupBizOsForVotingTest(VoteExamSurveyAnswerTypeList.Codes.RankedVote, 3);
				Answer.Validation.ValidateHZ_Answer();
				AssertNoErrors("Should not have any errors when not filled in", Answer.HZ_AnswerInfo);

				Answer.HZ_AnswerComment = "爱德华";
				Answer.Validation.ValidateHZ_Answer();
				AssertNoErrors("There should be no errors", Answer.HZ_AnswerCommentInfo);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		void SetupBizOsForVotingTest(string campaignType, string votingType, ZByte maxItemsVoted)
		{
			Campaign.G0_BroadcastVoteSurveyExam = campaignType;
			Campaign.VoteHeader.HY_AnswerType = votingType;
			Campaign.VoteHeader.HY_Max = maxItemsVoted;
			Campaign.VoteHeader.SubQuestions.Add(Question);
			Question.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.VotingItem;
			Question.HY_SubQuestionOrder = 1;
		}

		void SetupBizOsForVotingTest(string votingType, ZByte maxItemsVoted)
		{
			SetupBizOsForVotingTest(CampaignTypeList.Codes.Voting, votingType, maxItemsVoted);
		}

		VoteExamSurveyAnswer Answer
		{
			get
			{
				if (fAnswer == null)
				{
					fAnswer = Factory.New<VoteExamSurveyAnswer>();
					fAnswer.HZ_G8 = CampaignItem.PK;
					fAnswer.HZ_HY = Question.PK;
				}
				return fAnswer;
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

		VoteExamSurveyQuestion Question
		{
			get
			{
				if (fQuestion == null)
				{
					fQuestion = Factory.New<VoteExamSurveyQuestion>();
					fQuestion.HY_G0 = Campaign.PK;
				}
				return fQuestion;
			}
		}

		VoteExamSurveyAnswer fAnswer;
		GlbCompanyCampaign fCampaign;
		GlbCompanyCampaignItem fCampaignItem;
		VoteExamSurveyQuestion fQuestion;
	}
}
