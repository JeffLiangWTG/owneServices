using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(ScaledTestResultByCategoryCollection))]
	sealed class ScaledTestResultByCategoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ScaledTestResultByCategoryCollection>
	{
		public void TestLoad()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory newCategory = campaign.QuestionCategories.AddNew("MEH", "new cat");
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			AssertEquals(2, campaignItem.ScaledTestResults.Count);
			AssertEquals(campaign.QuestionCategories[0], campaignItem.ScaledTestResults[0].Category);
			AssertEquals(newCategory, campaignItem.ScaledTestResults[1].Category);
		}

		public void TestAllowNew()
		{
			Assert(!((IBindingList)Collection).AllowNew);
		}

		protected override ScaledTestResultByCategoryCollection GetCollectionToTest()
		{
			LearningCentreCampaignItem campaignItem = Factory.New<LearningCentreCampaignItem>();
			return new ScaledTestResultByCategoryCollection(campaignItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			QuestionCategory newCategory = campaign.QuestionCategories.AddNew("MEH", "new cat");
			LearningCentreCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			return new ScaledTestResultByCategory(campaignItem, newCategory);
		}
	}
}
