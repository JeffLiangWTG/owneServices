using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(BulkCommunicationCollection))]
	sealed class TestBulkCommunicationCollection : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BulkCommunicationCollection(Factory);
		}

		public void TestLoadOrgSalesCall()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsSalesLead = false;
			var contact1 = org.Contacts.AddNew();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org1.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			BulkCommunicationCollectionTest collection = new BulkCommunicationCollectionTest(Factory);
			collection.CreateFromCampaignItems(new GlbCompanyCampaignItem[] { campaignItem1, campaignItem2 });
			Assert("OrgSalesCall in collection should now have errors because a relationship already exists", collection[0].HasErrors);

			Factory.Save();

			collection.CreateFromCampaignItems(new GlbCompanyCampaignItem[] { campaignItem1, campaignItem2 });
			AssertEquals("Collection should contain 2 element of type OrgSalesCall", 2, collection.Count);
			AssertEquals(org.PK, collection[0].OQ_OH);
			AssertEquals(contact1.PK, collection[0].OQ_OC);

			AssertEquals(1, campaignItem1.RelatedChildActivityPivotCollection.Count());
			AssertType(typeof(OrgSalesCall), campaignItem1.RelatedChildActivityPivotCollection.Activities.Single());
			AssertEquals(1, campaignItem2.RelatedChildActivityPivotCollection.Count());
			AssertType(typeof(OrgSalesCall), campaignItem2.RelatedChildActivityPivotCollection.Activities.Single());
		}

		#region Implementation

		public class BulkCommunicationCollectionTest : BulkCommunicationCollection
		{
			public BulkCommunicationCollectionTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion
	}
}
