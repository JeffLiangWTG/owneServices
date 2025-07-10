using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(CampaignItemContactCrossReferences))]
	class CampaignItemContactCrossReferencesTest : NonPersistentBusinessObjectCollectionTestCase<CampaignItemContactCrossReferences>
	{
		public void TestLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var differentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "contact1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "contact2";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "contact3";
			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "contact4";
			var contact5 = org.Contacts.AddNew();
			contact5.OC_ContactName = "contact5";
			var contactForDifferentOrg = differentOrg.Contacts.AddNew();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact1.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignItemWithSameOrg = campaign.CampaignsItemsSent.AddNew();
			campaignItemWithSameOrg.G8_RecipientID = contact2.PK;
			campaignItemWithSameOrg.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var campaignItemWithDifferentOrg = campaign.CampaignsItemsSent.AddNew();
			campaignItemWithDifferentOrg.G8_RecipientID = contactForDifferentOrg.PK;
			campaignItemWithDifferentOrg.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			Factory.Save();

			var inquiryLinkedToSameOrg = Factory.New<SalesEnquiry>();
			inquiryLinkedToSameOrg.O1_OC_LinkedContact = contact5.PK;
			inquiryLinkedToSameOrg.O1_OH_ConvertedToQualifiedLead = org.PK;

			var campaignItemForInquiryWithSameOrg = campaign.CampaignsItemsSent.AddNew();
			campaignItemForInquiryWithSameOrg.G8_RecipientID = inquiryLinkedToSameOrg.PK;
			campaignItemForInquiryWithSameOrg.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;

			Factory.Save();

			var opportunityWithSameOrg = Factory.New<OrgOpportunity>();
			opportunityWithSameOrg.P8_OH = org.PK;
			opportunityWithSameOrg.P8_OC = contact3.PK;
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(opportunityWithSameOrg);
			var communicationWithSameOrgButNoContact = Factory.New<OrgSalesCall>();
			communicationWithSameOrgButNoContact.OQ_OH = org.PK;
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communicationWithSameOrgButNoContact);
			var inquiryWithDifferentOrg = Factory.New<SalesEnquiry>();
			inquiryWithDifferentOrg.O1_OH_ConvertedToQualifiedLead = differentOrg.PK;
			inquiryWithDifferentOrg.O1_OC_LinkedContact = contactForDifferentOrg.PK;
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(inquiryWithDifferentOrg);

			var communicationWithSameOrgAndContactButLinkedToHeaderDirectly = Factory.New<OrgSalesCall>();
			communicationWithSameOrgAndContactButLinkedToHeaderDirectly.OQ_OH = org.PK;
			communicationWithSameOrgAndContactButLinkedToHeaderDirectly.OQ_OC = contact2.PK;
			ViewRelatedActivityPivot.Create(Factory, campaign, communicationWithSameOrgAndContactButLinkedToHeaderDirectly);

			var crossReferenceCollection = new CampaignItemContactCrossReferences();
			crossReferenceCollection.Load(campaignItem);

			var contact2CrossReference = crossReferenceCollection.Cast<OrgContactCampaignReferences>().First(x => x.Contact == contact2);
			AssertEquals(campaignItemWithSameOrg, contact2CrossReference.CampaignItem);
			AssertContainsExactElementsInAnyOrder("Should have communicationWithSameOrgAndContactButLinkedToHeaderDirectly", new[] { communicationWithSameOrgAndContactButLinkedToHeaderDirectly }, contact2CrossReference.CampaignChildren);

			var contact3CrossReference = crossReferenceCollection.Cast<OrgContactCampaignReferences>().First(x => x.Contact == contact3);
			AssertNull(contact3CrossReference.CampaignItem);
			AssertContainsExactElementsInAnyOrder("Opportunity is the only campaign child that should be cross referenced", new[] { opportunityWithSameOrg }, contact3CrossReference.CampaignChildren);

			var contact5CrossReference = crossReferenceCollection.Cast<OrgContactCampaignReferences>().Last(x => x.Contact == contact5);
			AssertNotNull(contact5CrossReference.CampaignItem);

			var contact4CrossReference = crossReferenceCollection.Cast<OrgContactCampaignReferences>().FirstOrDefault(x => x.Contact == contact4);
			AssertNull("Should not include contacts that don't have cross referenced campaign children nor campaignitem", contact4CrossReference);

			AssertEquals("Only 3 contact cross references", 3, crossReferenceCollection.Count);
		}

		public void TestLoadWhenTwoCampaignItemsSentToInquiriesWithSameContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contactA = org.Contacts.AddNew();
			contactA.OC_ContactName = "contactA";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItemForContactA = campaign.CampaignsItemsSent.AddNew();
			campaignItemForContactA.G8_RecipientID = contactA.PK;
			campaignItemForContactA.G8_RecipientTableCode = contactA.TablePrefix;

			var contactB = org.Contacts.AddNew();
			contactB.OC_ContactName = "contactB";

			var inquiry1 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry1.O1_OC_LinkedContact = contactB.PK;
			var campaignItemForInquiry1 = campaign.CampaignsItemsSent.AddNew();
			campaignItemForInquiry1.G8_RecipientID = inquiry1.PK;
			campaignItemForInquiry1.G8_RecipientTableCode = inquiry1.TablePrefix;

			var inquiry2 = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry2.O1_OC_LinkedContact = contactB.PK;
			var campaignItemForInquiry2 = campaign.CampaignsItemsSent.AddNew();
			campaignItemForInquiry2.G8_RecipientID = inquiry2.PK;
			campaignItemForInquiry2.G8_RecipientTableCode = inquiry2.TablePrefix;

			Factory.Save();

			var crossReferenceCollection = new CampaignItemContactCrossReferences();
			crossReferenceCollection.Load(campaignItemForContactA);

			AssertContainsExactElementsInAnyOrder("There should not be separate cross reference for each inquiry campaign item",
				new[]
				{
					campaignItemForInquiry1,
					campaignItemForInquiry2,
				},
				crossReferenceCollection.Cast<OrgContactCampaignReferences>().Select(x => x.CampaignItem));
		}

		public void TestLoadWhenNoOrgIsAssignedToInquiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			var inquiry = Factory.New<SalesEnquiry>();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = inquiry.PK;
			campaignItem.G8_RecipientTableCode = OrgColdCallRegisterSchema.Constants.Prefix;
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientID = contact.PK;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			Factory.Save();

			var crossReferenceCollection = new CampaignItemContactCrossReferences();
			crossReferenceCollection.Load(campaignItem);

			AssertEquals("There should not be a cross reference with an inquiry with no organisation set", 0, crossReferenceCollection.Count);
		}

		#region Implementation

		protected override CampaignItemContactCrossReferences GetCollectionToTest()
		{
			return new CampaignItemContactCrossReferences();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgContactCampaignReferences(Factory.New<OrgContact>());
		}

		#endregion
	}
}
