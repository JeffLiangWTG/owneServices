using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignContactFilterBusinessObject))]
	class GlbCompanyCampaignContactFilterBusinessObjectSubscriptionTest : GlbCompanyCampaignContactFilterTestCaseWithSubGroupCheckExclusions
	{
		public void TestSubscribedOnlyContacts()
		{
			subscriptionFilter.Property = "SUB";
			subscriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			var collection = GetFilteredCollection();
			AssertEquals(2, collection.Length);
			AssertCollectionContains(subscribedOrganisation.Contacts[0].PK, collection);
			AssertCollectionContains(unsubscribedOrganisation.Contacts[1].PK, collection);
		}

		public void TestUnsubscribedOnlyContacts()
		{
			subscriptionFilter.Property = "UNS";
			subscriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;

			var collection = GetFilteredCollection();
			AssertEquals(2, collection.Length);
			AssertCollectionContains(subscribedOrganisation.Contacts[1].PK, collection);
			AssertCollectionContains(unsubscribedOrganisation.Contacts[0].PK, collection);
		}

		public void TestNotUnsubscribedContacts()
		{
			subscriptionFilter.Property = "UNS";
			subscriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			var collection = GetFilteredCollection();
			AssertEquals(3, collection.Length);
			AssertCollectionContains(organisation.Contacts[0].PK, collection);
			AssertCollectionContains(subscribedOrganisation.Contacts[0].PK, collection);
			AssertCollectionContains(unsubscribedOrganisation.Contacts[1].PK, collection);
		}

		public void TestNotSubscribedContacts()
		{
			subscriptionFilter.Property = "SUB";
			subscriptionFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;

			var collection = GetFilteredCollection();
			AssertEquals(3, collection.Length);
			AssertCollectionContains(organisation.Contacts[0].PK, collection);
			AssertCollectionContains(subscribedOrganisation.Contacts[1].PK, collection);
			AssertCollectionContains(unsubscribedOrganisation.Contacts[0].PK, collection);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GlbCompanyCampaignContactFilterBusinessObject(Factory.NewWithValidTestData<GlbCompanyCampaign>());

		protected override void SetUp()
		{
			base.SetUp();
			CreateCampaign();
			CreateOrg();
			CreateSubscribedOrg();
			CreateUnsubscribedOrg();
			Factory.Save();
		}

		void CreateCampaign()
		{
			campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Category = "CAT";
			campaign.G0_Type = "TYP";
			campaignFilter = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			subscriptionFilter = (ModuleTextFilter)campaignFilter[GlbCompanyCampaignContactFilterBusinessObject.SubscriptionFilterCode];
		}

		void CreateOrg()
		{
			organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = nameof(organisation);
			contact.OC_Email = $"{contact.OC_ContactName}@ema.il";
		}

		void CreateSubscribedOrg()
		{
			subscribedOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var subscription = subscribedOrganisation.Subscriptions.AddNew();
			subscription.GCS_IsSubscribed = true;

			var contact = subscribedOrganisation.Contacts.AddNew();
			contact.OC_ContactName = $"{nameof(subscribedOrganisation)}1";
			contact.OC_Email = $"{contact.OC_ContactName}1@ema.il";

			contact = subscribedOrganisation.Contacts.AddNew();
			contact.OC_ContactName = $"{nameof(subscribedOrganisation)}2";
			contact.OC_Email = $"{contact.OC_ContactName}@ema.il";
			subscription = contact.Subscriptions.AddNew();
			subscription.GCS_IsSubscribed = false;
		}

		void CreateUnsubscribedOrg()
		{
			unsubscribedOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var subscription = unsubscribedOrganisation.Subscriptions.AddNew();
			subscription.GCS_IsSubscribed = false;

			var contact = unsubscribedOrganisation.Contacts.AddNew();
			contact.OC_ContactName = $"{nameof(unsubscribedOrganisation)}1";
			contact.OC_Email = $"{contact.OC_ContactName}1@ema.il";

			contact = unsubscribedOrganisation.Contacts.AddNew();
			contact.OC_ContactName = $"{nameof(unsubscribedOrganisation)}2";
			contact.OC_Email = $"{contact.OC_ContactName}1@ema.il";
			subscription = contact.Subscriptions.AddNew();
			subscription.GCS_IsSubscribed = true;
		}

		ZGuid[] GetFilteredCollection()
		{
			var collection = new GlbCampaignContactCollection(campaign);
			var query = subscriptionFilter.Query;
			collection.Load(query);

			var pks = new HashSet<ZGuid>(new[] { organisation.PK, subscribedOrganisation.PK, unsubscribedOrganisation.PK });
			return collection.Cast<CampaignContact>().Where(contact => pks.Contains(contact.Organisation.PK)).Select(contact => contact.PK).ToArray();
		}

		GlbCompanyCampaign campaign;
		GlbCompanyCampaignContactFilterBusinessObject campaignFilter;
		OrgHeader organisation;
		OrgHeader subscribedOrganisation;
		ModuleTextFilter subscriptionFilter;
		OrgHeader unsubscribedOrganisation;

		#endregion
	}
}
