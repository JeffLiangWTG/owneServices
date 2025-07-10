using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckG8_ClosedDate_NonVotingCampaign()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
			AssertNoErrors("Not a voting campaign, should not have voting nomination error", campaignItem.G8_ClosedDateUtcInfo);
		}

		public void TestCheckG8_ClosedDate_NotInWeb()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
			AssertNoErrors("Not in web environment, should not have any errors", campaignItem.G8_ClosedDateUtcInfo);

			campaign.VoteHeader.HY_Min = 1;
			campaign.VoteHeader.HY_Max = 3;
			VoteExamSurveyQuestion question1 = campaign.VoteHeader.SubQuestions.AddNew();
			var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
			var votedItems = voteExamSurveyAnswerSet.PersistedAnswers.GetCompletedAnswers(campaignItem);
			VoteExamSurveyAnswer answer1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem);
			answer1.HZ_Answer = "2";
			VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
			VoteExamSurveyAnswer answer2 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);
			answer2.HZ_Answer = "4";
			campaignItem.Validation.ValidateG8_ClosedDateUtc();
			AssertNoErrors("Not in web environment, should not have any errors", campaignItem.G8_ClosedDateUtcInfo);
		}

		public void TestCheckG8_ClosedDate()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				AssertNoErrors("Pre-condition", campaignItem.G8_ClosedDateUtcInfo);

				campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
				AssertHasError(campaignItem.G8_ClosedDateUtcInfo, "You need to nominate 10 votes. You have nominated 0 so far");

				campaign.VoteHeader.HY_Min = 1;
				campaign.VoteHeader.HY_Max = 2;
				campaignItem.Validation.ValidateG8_ClosedDateUtc();
				AssertHasError(campaignItem.G8_ClosedDateUtcInfo, "You need to nominate at least 1 and at most 2 votes. You have nominated 0 so far");

				VoteExamSurveyQuestion question = campaign.VoteHeader.SubQuestions.AddNew();
				var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
				VoteExamSurveyAnswer answer = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question, campaignItem);
				answer.HZ_Answer = "1";
				campaignItem.Validation.ValidateG8_ClosedDateUtc();
				AssertNoErrors(campaignItem.G8_ClosedDateUtcInfo);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestCheckG8_ClosedDate_InvalidVotingNominationOrders()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				AssertNoErrors("Pre-condition", campaignItem.G8_ClosedDateUtcInfo);

				campaign.VoteHeader.HY_Min = 1;
				campaign.VoteHeader.HY_Max = 6;
				VoteExamSurveyQuestion question1 = campaign.VoteHeader.SubQuestions.AddNew();
				var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
				VoteExamSurveyAnswer answer1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem);
				answer1.HZ_Answer = "1";
				VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
				VoteExamSurveyAnswer answer2 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);
				answer2.HZ_Answer = "4";
				VoteExamSurveyQuestion question3 = campaign.VoteHeader.SubQuestions.AddNew();
				VoteExamSurveyAnswer answer3 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question3, campaignItem);
				answer3.HZ_Answer = "6";
				campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
				AssertHasError(campaignItem.G8_ClosedDateUtcInfo, "You have nominated 3 votes. Please rank them with value between 1 to 3");

				answer2.HZ_Answer = "2";
				campaignItem.Validation.ValidateG8_ClosedDateUtc();
				AssertHasError(campaignItem.G8_ClosedDateUtcInfo, "You have nominated 3 votes. Please rank them with value between 1 to 3");

				answer3.HZ_Answer = "2";
				campaignItem.Validation.ValidateG8_ClosedDateUtc();
				AssertHasError(campaignItem.G8_ClosedDateUtcInfo, "You have nominated 3 votes. Please rank them with value between 1 to 3");

				answer3.HZ_Answer = "3";
				campaignItem.Validation.ValidateG8_ClosedDateUtc();
				AssertNoErrors(campaignItem.G8_ClosedDateUtcInfo);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestCheckG8_ClosedDate_VotingNominationOrderValidationShouldNotAffectUnrankedVotes()
		{
			bool originalValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				AssertNoErrors("Pre-condition", campaignItem.G8_ClosedDateUtcInfo);

				campaign.VoteHeader.HY_Min = 1;
				campaign.VoteHeader.HY_Max = 6;
				campaign.VoteHeader.HY_AnswerType = VoteExamSurveyAnswerTypeList.Codes.UnrankedVote;

				VoteExamSurveyQuestion question1 = campaign.VoteHeader.SubQuestions.AddNew();
				var voteExamSurveyAnswerSet = new VoteExamSurveyAnswerSet(Factory, campaignItem);
				VoteExamSurveyAnswer answer1 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question1, campaignItem);
				answer1.HZ_Answer = "Y";
				VoteExamSurveyQuestion question2 = campaign.VoteHeader.SubQuestions.AddNew();
				VoteExamSurveyAnswer answer2 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question2, campaignItem);
				answer2.HZ_Answer = "Y";
				VoteExamSurveyQuestion question3 = campaign.VoteHeader.SubQuestions.AddNew();
				VoteExamSurveyAnswer answer3 = voteExamSurveyAnswerSet.PersistedAnswers.LoadOrCreateNew(question3, campaignItem);
				answer3.HZ_Answer = "Y";
				campaignItem.G8_ClosedDateUtc = ZDateTime.UtcNow;
				AssertNoErrors(campaignItem.G8_ClosedDateUtcInfo);
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}
		}

		public void TestCheckG8_GS_NKFollowedUpBy()
		{
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_GS_NKFollowedUpBy = "..";
			AssertHasErrors("Staff code does not exist, should have errors", item.G8_GS_NKFollowedUpByInfo);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			item.G8_GS_NKFollowedUpBy = staff.GS_Code;
			AssertNoErrors("Valid staff code entered, should not have errors", item.G8_GS_NKFollowedUpByInfo);
		}

		public void TestCheckG8_GS_NKSender()
		{
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_GS_NKSender = "..";
			AssertHasErrors("Staff code does not exist, should have errors", item.G8_GS_NKSenderInfo);

			item.G8_GS_NKSender = "";
			AssertNoErrors("Empty Staff staff is valid, should not have errors", item.G8_GS_NKSenderInfo);

			item.G8_GS_NKSender = "..";
			AssertHasErrors("Staff code does not exist, should have errors", item.G8_GS_NKSenderInfo);

			var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
			item.G8_GS_NKSender = glbStaff.GS_Code;
			AssertNoErrors("Valid staff code entered, should not have errors", item.G8_GS_NKSenderInfo);
		}

		public void TestCheckG8_SenderEmailAddress()
		{
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			AssertNoErrors("Empty Email is OK", item.G8_SenderEmailAddressInfo);

			item.G8_SenderEmailAddress = "e";
			AssertHasErrors("Invalid Email", item.G8_SenderEmailAddressInfo);

			item.G8_SenderEmailAddress = ZString.Empty;
			AssertNoErrors("Empty Email is OK", item.G8_SenderEmailAddressInfo);

			item.G8_SenderEmailAddress = "e@ma.il";
			AssertNoErrors("Valid Email", item.G8_SenderEmailAddressInfo);
		}
	}
}
