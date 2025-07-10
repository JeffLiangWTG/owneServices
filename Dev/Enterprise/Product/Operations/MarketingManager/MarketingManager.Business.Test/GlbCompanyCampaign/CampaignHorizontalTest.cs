using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class CampaignHorizontalTest : TestCaseWithFactory
	{
		public void TestAddCampaign()
		{
			var horizontal = new CampaignHorizontal(5);

			AssertNotNull(horizontal.Campaigns);
			AssertEquals(0, horizontal.Campaigns.Count);

			bool collectionChanged = false;
			horizontal.Campaigns.CollectionChanged += delegate
			{
				collectionChanged = true;
			};

			horizontal.AddCampaign(Factory.New<GlbCompanyCampaign>());
			AssertEquals(1, horizontal.Campaigns.Count);
			Assert(collectionChanged);
		}
	}
}
