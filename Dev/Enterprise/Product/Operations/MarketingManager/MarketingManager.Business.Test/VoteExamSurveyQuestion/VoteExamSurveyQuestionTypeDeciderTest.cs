using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Recruiter;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class VoteExamSurveyQuestionTypeDeciderTest : TestCaseWithFactory
	{
		public void TestNewAndBinding()
		{
			AssertEquals(typeof(VoteExamSurveyQuestion), VoteExamSurveyQuestion.TypeDecider.GetTypeForNew());
			AssertEquals(typeof(VoteExamSurveyQuestion), VoteExamSurveyQuestion.TypeDecider.GetTypeForBinding());
		}

		public void TestLoad()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			VoteExamSurveyQuestion question = campaign.Questions.AddNew();

			GlbCompanyCampaign learningCentreCampaign = (GlbCompanyCampaign)Factory.New<ILearningCentreCampaign>();
			learningCentreCampaign.FillWithValidTestData();
			VoteExamSurveyQuestion learningCentreQuestion = learningCentreCampaign.Questions.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertEquals(ObjectFactory.GetType<ILearningCentreQuestion>(), newFactory.Load<VoteExamSurveyQuestion>(learningCentreQuestion.PK).GetType());
			AssertEquals(typeof(VoteExamSurveyQuestion), newFactory.Load<VoteExamSurveyQuestion>(question.PK).GetType());
		}
	}
}
