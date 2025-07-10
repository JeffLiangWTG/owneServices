using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSubscriptionForOrganisationCollection))]
	sealed class GlbCompanyCampaignSubscriptionForOrganisationCollectionTest : GlbCompanyCampaignSubscriptionCollectionTestBase<GlbCompanyCampaignSubscriptionForOrganisationCollection>
	{
		protected override GlbCompanyCampaignSubscriptionForOrganisationCollection GetCollectionToTest()
		{
			return new GlbCompanyCampaignSubscriptionForOrganisationCollection(Factory.NewWithValidTestData<OrgHeader>());
		}

		public void TestAddNewSubscriptions()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			Assert("Allow add new", header.Subscriptions.AllowNew);
			var subscription1 = header.Subscriptions.AddNew();
			AssertEquals(header.PK, subscription1.GCS_OH);
			var subscription2 = header.Subscriptions.AddNew();
			AssertEquals(header.PK, subscription2.GCS_OH);
			AssertEquals(2, header.Subscriptions.Count);
		}

		public override void TestDeleteAll()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var subscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription1.GCS_OH = header.PK;
			subscription1.GCS_IsSubscribed = true;

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_OH = header.PK;
			subscription2.GCS_IsSubscribed = false;

			var subscription3 = Factory.New<GlbCompanyCampaignSubscription>();

			var collection = header.Subscriptions;
			AssertEquals(2, collection.Count);
			AssertCollectionContains(subscription1, collection);
			AssertCollectionContains(subscription2, collection);
			AssertEquals(header.PK, subscription1.GCS_OH);
			AssertEquals(header.PK, subscription2.GCS_OH);

			collection.DeleteAll();
			AssertEquals(true, subscription1.IsDeleted);
			AssertEquals(true, subscription2.IsDeleted);
			AssertEquals(false, subscription3.IsDeleted);
		}

		protected override Tuple<BusinessObject, BusinessObject, BusinessObject> CreateBusinessObjectsToValidate(GlbCompanyCampaignSubscriptionForOrganisationCollection collection)
		{
			var subscription1 = collection.AddNew();
			subscription1.GCS_MediaCategory = "asd";
			subscription1.GCS_IsSubscribed = true;

			var subscription2 = collection.AddNew();
			subscription2.GCS_MediaCategory = "zxc";
			subscription2.GCS_IsSubscribed = false;

			var subscription3 = collection.AddNew();
			subscription3.GCS_MediaCategory = "asd";
			subscription3.GCS_IsSubscribed = false;
			return new Tuple<BusinessObject, BusinessObject, BusinessObject>(subscription1, subscription2, subscription3);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var list = new CodeDescriptionBoolCollection
			{
				{ "asd", (NoResString)"Category ASD" },
				{ "zxc", (NoResString)"Category ZXC" }
			};
			OrganisationsDataRegistry.Instance.CampaignCategory1List.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
		}
	}
}
