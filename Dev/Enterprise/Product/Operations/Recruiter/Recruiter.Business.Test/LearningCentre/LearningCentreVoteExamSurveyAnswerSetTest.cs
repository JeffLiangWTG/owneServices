using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreVoteExamSurveyAnswerSet))]
	sealed class LearningCentreVoteExamSurveyAnswerSetTest : VoteExamSurveyAnswerSetTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var campaignItem = Factory.New<LearningCentreCampaignItem>();
			campaignItem.G8_G0 = Factory.NewWithValidTestData<LearningCentreCampaign>().PK;
			var answerSet = new LearningCentreVoteExamSurveyAnswerSet(Factory, campaignItem);
			return answerSet;
		}
	}
}
