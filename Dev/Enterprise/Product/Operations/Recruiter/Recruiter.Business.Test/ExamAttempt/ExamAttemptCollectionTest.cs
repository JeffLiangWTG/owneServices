using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ExamAttemptCollection))]
	sealed class ExamAttemptCollectionTest : ActiveBusinessObjectCollectionTestCase<ExamAttemptCollection>
	{
		protected override ExamAttemptCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			campaign.G0_CampaignName = "Exam AAA";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new ExamAttemptCollection(campaignItem);
		}
	}
}
