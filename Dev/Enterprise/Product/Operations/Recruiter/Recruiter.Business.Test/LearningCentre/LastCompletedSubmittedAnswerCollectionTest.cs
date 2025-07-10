using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LastCompletedSubmittedAnswerCollection))]
	sealed class LastCompletedSubmittedAnswerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LastCompletedSubmittedAnswerCollection>
	{
		protected override LastCompletedSubmittedAnswerCollection GetCollectionToTest()
		{
			var campaignItem = Factory.New<LearningCentreCampaignItem>();
			return new LastCompletedSubmittedAnswerCollection(campaignItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var campaignItem = Factory.New<LearningCentreCampaignItem>();
			var question = Factory.New<LearningCentreQuestion>();
			return new LastCompletedSubmittedAnswer(campaignItem, question);
		}
	}
}
