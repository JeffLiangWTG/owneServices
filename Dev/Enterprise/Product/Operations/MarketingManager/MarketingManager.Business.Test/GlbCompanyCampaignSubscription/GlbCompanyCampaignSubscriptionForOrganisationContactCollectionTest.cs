using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignSubscriptionForOrganisationContactCollection))]
	sealed class GlbCompanyCampaignSubscriptionForOrganisationContactCollectionTest : GlbCompanyCampaignSubscriptionCollectionTestBase<GlbCompanyCampaignSubscriptionForOrganisationContactCollection>
	{
		protected override GlbCompanyCampaignSubscriptionForOrganisationContactCollection GetCollectionToTest()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "e@ma.il";
			return new GlbCompanyCampaignSubscriptionForOrganisationContactCollection(contact);
		}

		public void TestAddNewSubscriptions()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "";
			AssertEquals("Emails should be the same, shouldn't they?", contact.Email, contact.OC_Email);
			Assert("Empty email doesn't allow to add new", !contact.Subscriptions.AllowNew);

			contact.OC_Email = "asdfg@zxcvb.com";
			AssertEquals("Emails should be the same, shouldn't they?", contact.Email, contact.OC_Email);
			Assert("Allow add new", contact.Subscriptions.AllowNew);

			var subscription1 = contact.Subscriptions.AddNew();
			AssertEquals(contact.Email, subscription1.GCS_Email);
			var subscription2 = contact.Subscriptions.AddNew();
			AssertEquals(contact.Email, subscription2.GCS_Email);
			AssertEquals(2, contact.Subscriptions.Count);
		}

		public override void TestDeleteAll()
		{
			const string email1 = "1@asdfg.com";
			const string email2 = "2@asdfg.com";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			AssertEquals("Emails should be the same, shouldn't they?", contact.Email, contact.OC_Email);

			var subscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription1.GCS_Email = email1;
			subscription1.GCS_MediaCategory = "asd";
			subscription1.GCS_IsSubscribed = true;

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_Email = email1;
			subscription2.GCS_MediaCategory = "zxc";
			subscription2.GCS_IsSubscribed = false;

			var subscription3 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription3.GCS_Email = email2;
			subscription3.GCS_MediaCategory = "asd";
			subscription3.GCS_IsSubscribed = false;

			AssertEquals("Empty email should not have subscriptions", 0, contact.Subscriptions.Count);

			contact.OC_Email = email1;
			AssertEquals(2, contact.Subscriptions.Count);
			AssertWrappedSubscriptionInCollection(subscription1, contact.Subscriptions);
			AssertWrappedSubscriptionInCollection(subscription2, contact.Subscriptions);
			AssertEquals(contact.Email, subscription1.GCS_Email);
			AssertEquals(contact.Email, subscription2.GCS_Email);

			contact.OC_Email = email2;
			AssertEquals(1, contact.Subscriptions.Count);
			AssertWrappedSubscriptionInCollection(subscription3, contact.Subscriptions);
			AssertEquals(contact.Email, subscription3.GCS_Email);

			contact.Subscriptions.DeleteAll();
			AssertEquals(false, subscription1.IsDeleted);
			AssertEquals(false, subscription2.IsDeleted);
			AssertEquals(true, subscription3.IsDeleted);
		}

		static void AssertWrappedSubscriptionInCollection(GlbCompanyCampaignSubscription item, IGlbCompanyCampaignSubscriptionForOrganisationContactCollection subscriptions)
		{
			AssertCollectionContains(item.PK, ((IEnumerable<GlbCompanyCampaignSubscription>)subscriptions).Select(subscription => subscription.PK));
		}

		public void TestDeleteWithChangedEmail()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			AssertEquals("Emails should be the same, shouldn't they?", contact.Email, contact.OC_Email);

			var subscription1 = (GlbCompanyCampaignSubscription)contact.Subscriptions.AddNew();
			AssertEquals(contact.Email, subscription1.GCS_Email);

			contact.OC_Email = "asdfg@zxcvb.com";

			var subscription2 = (GlbCompanyCampaignSubscription)contact.Subscriptions.AddNew();
			AssertEquals(contact.Email, subscription2.GCS_Email);

			contact.Subscriptions.DeleteAll();
			AssertEquals(false, subscription1.IsDeleted);
			AssertEquals(true, subscription2.IsDeleted);
		}

		public void TestRefreshSubscriptionsAfterEmailChanged()
		{
			const string email1 = "1@asdfg.com";
			const string email2 = "2@asdfg.com";

			var subscription1 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription1.GCS_Email = email1;
			subscription1.GCS_MediaCategory = "asd";
			subscription1.GCS_IsSubscribed = true;

			var subscription2 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription2.GCS_Email = email2;
			subscription2.GCS_MediaCategory = "asd";
			subscription2.GCS_IsSubscribed = false;

			var subscription3 = Factory.New<GlbCompanyCampaignSubscription>();
			subscription3.GCS_Email = email2;
			subscription3.GCS_MediaCategory = "zxc";
			subscription3.GCS_IsSubscribed = true;
			Factory.Save();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			AssertEquals("No subscriptions", 0, contact.Subscriptions.Count);

			contact.OC_Email = email1;
			AssertEquals(1, contact.Subscriptions.Count);

			contact.OC_Email = email2;
			AssertEquals(2, contact.Subscriptions.Count);
		}

		public void TestRejectAllChanges()
		{
			const string category = "CAT";
			const string type = "TYP";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "e@ma.il";
			var collection = contact.Subscriptions;
			var subscription1 = collection.AddNew();
			var subscription2 = collection.AddNew();

			subscription1.GCS_MediaCategory = category;
			subscription2.GCS_MediaType = type;

			Factory.Save();

			AssertDefaultValues(category, type, subscription1, subscription2);

			subscription1.GCS_MediaCategory = ZString.Empty;
			subscription2.GCS_MediaCategory = category;
			subscription1.GCS_MediaType = type;
			subscription2.GCS_MediaType = ZString.Empty;

			AssertEquals(ZString.Empty, subscription1.GCS_MediaCategory);
			AssertEquals(category, subscription2.GCS_MediaCategory);
			AssertEquals(type, subscription1.GCS_MediaType);
			AssertEquals(ZString.Empty, subscription2.GCS_MediaType);

			collection.RejectAllChanges();
			AssertDefaultValues(category, type, subscription1, subscription2);
		}

		public void TestFilterForOrganisationSubscriptions()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_Email = "e@ma.il";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_Email = contact1.OC_Email;

			var subscription = org1.Subscriptions.AddNew();
			subscription.GCS_MediaCategory = "asd";

			subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_Email = contact1.OC_Email;
			subscription.GCS_MediaCategory = "asd";
			subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_Email = contact1.OC_Email;
			subscription.GCS_MediaCategory = "zxc";

			Factory.Save();

			AssertEquals("All subscriptions", 3, contact1.Subscriptions.Count);
			AssertEquals("Only contact subscriptions", 2, contact2.Subscriptions.Count);

			org1.Subscriptions.DeleteAll();
			Factory.Save();

			AssertEquals("Only contact subscriptions", 2, contact1.Subscriptions.Count);
			AssertEquals("Only contact subscriptions", 2, contact2.Subscriptions.Count);
		}

		static void AssertDefaultValues(string expectedCategory, string expecedType, IGlbCompanyCampaignSubscription subscription1, IGlbCompanyCampaignSubscription subscription2)
		{
			AssertEquals(expectedCategory, subscription1.GCS_MediaCategory);
			AssertEquals(ZString.Empty, subscription2.GCS_MediaCategory);
			AssertEquals(ZString.Empty, subscription1.GCS_MediaType);
			AssertEquals(expecedType, subscription2.GCS_MediaType);
		}

		protected override Tuple<BusinessObject, BusinessObject, BusinessObject> CreateBusinessObjectsToValidate(GlbCompanyCampaignSubscriptionForOrganisationContactCollection collection)
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
