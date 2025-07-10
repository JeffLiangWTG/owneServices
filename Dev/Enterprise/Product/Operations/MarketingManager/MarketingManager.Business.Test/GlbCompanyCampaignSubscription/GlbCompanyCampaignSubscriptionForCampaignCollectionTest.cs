using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSubscriptionForCampaignCollection))]
	sealed class GlbCompanyCampaignSubscriptionForCampaignCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCompanyCampaignSubscriptionForCampaignCollection>
	{
		protected override GlbCompanyCampaignSubscriptionForCampaignCollection GetCollectionToTest()
		{
			return new GlbCompanyCampaignSubscriptionForCampaignCollection(Factory.NewWithValidTestData<GlbCompanyCampaign>());
		}

		public void TestAddNewSubscriptions()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Assert("Not allowed to add new", !((IBindingList)campaign.UnsubscribedCollection).AllowNew);
		}

		public void TestRemoveAllFromRelationship()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CT";
			campaign.G0_Type = "TP";

			var subscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription1.GCS_Email = $"{nameof(subscription1)}@ema.il";
			subscription1.GCS_MediaCategory = campaign.G0_Category;
			subscription1.GCS_IsSubscribed = false;
			subscription1.GCS_G0 = campaign.PK;

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_Email = $"{nameof(subscription2)}@ema.il";
			subscription2.GCS_MediaType = campaign.G0_Type;
			subscription2.GCS_IsSubscribed = false;
			subscription2.GCS_G0 = campaign.PK;

			var subscription3 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription3.GCS_Email = $"{nameof(subscription3)}@ema.il";
			subscription3.GCS_MediaType = campaign.G0_Type;
			subscription3.GCS_IsSubscribed = false;

			Factory.Save();

			AssertEquals("Count of unsubscribed", 2, campaign.UnsubscribedCollection.Count);
			AssertCollectionContains(subscription1, campaign.UnsubscribedCollection);
			AssertCollectionContains(subscription2, campaign.UnsubscribedCollection);
			AssertEquals(campaign.PK, subscription1.GCS_G0);
			AssertEquals(campaign.PK, subscription2.GCS_G0);

			campaign.UnsubscribedCollection.RemoveAllFromRelationship();

			AssertEquals(false, subscription1.IsDeleted);
			AssertEquals(false, subscription2.IsDeleted);
			AssertEquals(false, subscription3.IsDeleted);

			AssertEquals("Should be empty GUID", ZGuid.Empty, subscription1.GCS_G0);
			AssertEquals("Should be empty GUID", ZGuid.Empty, subscription2.GCS_G0);
		}
	}
}
