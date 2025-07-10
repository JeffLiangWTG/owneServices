using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreCampaignItemCollection))]
	sealed class LearningCentreCampaignItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexerAndAddNew()
		{
			LearningCentreCampaignItemCollection collection = (LearningCentreCampaignItemCollection)Collection;
			LearningCentreCampaignItem item = collection.AddNew();
			AssertEquals(item, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			LearningCentreCampaign campaign = Factory.New<LearningCentreCampaign>();
			return new LearningCentreCampaignItemCollection(campaign);
		}
	}
}
