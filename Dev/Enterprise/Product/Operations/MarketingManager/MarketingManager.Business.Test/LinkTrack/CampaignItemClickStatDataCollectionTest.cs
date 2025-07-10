using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CampaignItemClickStatDataCollection))]
	sealed class CampaignItemClickStatDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CampaignItemClickStatDataCollection>
	{
		#region Implementation

		protected override CampaignItemClickStatDataCollection GetCollectionToTest()
		{
			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			return new CampaignItemClickStatDataCollection(campaignItem);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CampaignItemClickStatData(Factory, "", "");
		}

		#endregion
	}
}
