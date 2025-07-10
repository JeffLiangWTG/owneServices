using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreCampaignCollection))]
	sealed class LearningCentreCampaignCollectionTest : ActiveBusinessObjectCollectionTestCase<LearningCentreCampaignCollection>
	{
		public void TestRelationshipFilter()
		{
			LearningCentreCampaign examCampaign = Factory.New<LearningCentreCampaign>();
			examCampaign.G0_Type = LearningCentreTestTypes.Codes.Exam;
			LearningCentreCampaign scaledTestCampaign = Factory.New<LearningCentreCampaign>();
			scaledTestCampaign.G0_Type = LearningCentreTestTypes.Codes.Scaled;
			LearningCentreCampaign otherCampaign = Factory.New<LearningCentreCampaign>();
			otherCampaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;

			LearningCentreCampaignCollection collection = new LearningCentreCampaignCollection(Factory);
			AssertEquals(2, collection.Count);
			AssertCollectionContains(examCampaign, collection);
			AssertCollectionContains(scaledTestCampaign, collection);
		}
	}
}
