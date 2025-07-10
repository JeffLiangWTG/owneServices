using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ContactsWithNonDeliveryReportsCollection))]
	sealed class ContactsWithNonDeliveryReportsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ContactsWithNonDeliveryReportsCollection(Factory);
		}

		public void TestGetContactsWithNonDeliveryReportStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsSalesLead = false;
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Anthony";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			ContactsWithNonDeliveryReportsCollection collection = new ContactsWithNonDeliveryReportsCollection(Factory);
			collection.AddContacts(new[] { campaignItem1, campaignItem2 });
			AssertEquals("Collection should contain 2 element of type CampaignContact", 2, collection.Count);
			AssertEquals(org.PK, collection[0].VCC_OH);
			AssertEquals(contact1.PK, collection[0].PK);
			AssertEquals(org1.PK, collection[1].VCC_OH);
			AssertEquals(contact2.PK, collection[1].PK);

			ContactsWithNonDeliveryReportsCollection collectionOnlyContacts = new ContactsWithNonDeliveryReportsCollection(Factory);
			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			var campaignContact1 = campaignContactCollection[0];
			var campaignContact2 = campaignContactCollection[1];
			collectionOnlyContacts.AddContacts(new[] { campaignContact1, campaignContact2 });
			AssertEquals("Collection should contain 2 element of type CampaignContact", 2, collectionOnlyContacts.Count);
		}
	}
}
