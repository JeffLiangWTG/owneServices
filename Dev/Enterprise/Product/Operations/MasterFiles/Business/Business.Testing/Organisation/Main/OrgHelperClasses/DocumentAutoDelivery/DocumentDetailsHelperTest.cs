using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocumentDetailsHelperTest : TestCaseWithFactory
	{
		public void TestPopulateAutoDeliveryContactsForJob()
		{
			var menuItem = Factory.New<IStmMenuItem>();
			var jobDocumentDelivery1 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			var shipment1 = Factory.New<Forwarding.IForwardingShipment>() as BusinessObject;
			jobDocumentDelivery1.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery1.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			jobDocumentDelivery1.JDC_ParentID = shipment1.PK;
			jobDocumentDelivery1.JDC_ContactName = "Just1n Alpha";

			var jobDocumentDelivery2 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery2.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery2.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery2.JDC_ParentID = shipment1.PK;
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "Just1n Beta";
			jobDocumentDelivery2.JDC_OC_Contact = orgContact.PK;
			jobDocumentDelivery2.EmailToRecipients.Value = "i@just1n.net";

			var shipment2 = Factory.New<Forwarding.IForwardingShipment>() as BusinessObject;
			var jobDocumentDelivery3 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery3.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery3.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery3.JDC_ParentID = shipment2.PK;
			jobDocumentDelivery3.EmailToRecipients.Value = "what@ever.com";
			jobDocumentDelivery3.JDC_ContactName = "Just1n Gamma";

			var jobDocumentDelivery4 = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery4.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery4.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.DoNotDeliver;
			jobDocumentDelivery4.JDC_ParentID = shipment1.PK;
			jobDocumentDelivery4.JDC_ContactName = "Just1n Delta";

			var contacts = new DocDeliveryContactCollection(Factory);
			AssertEquals(0, contacts.Count);

			contacts.PopulateAutoDeliveryContactsForJob(menuItem, shipment1);
			AssertEquals(2, contacts.Count);

			var docDeliveryContacts = contacts.OfType<DocDeliveryContact>();
			var just1nAlpha = docDeliveryContacts.Single(d => d.Name == "Just1n Alpha");
			var just1nBeta = docDeliveryContacts.Single(d => d.Name == "Just1n Beta");

			AssertEquals(Core.Constants.ContactNotifyModes.Print, just1nAlpha.DeliveryMethod);
			AssertEquals("i@just1n.net", just1nBeta.EmailToRecipients.Value);
			AssertEquals(Core.Constants.ContactNotifyModes.Email, just1nBeta.DeliveryMethod);
		}

		public void TestPopulateToCcAndBccRecipients()
		{
			var contact = new DocDeliveryContact(Factory);
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_EmailToRecipientsAsString = "i@just1n.net";
			jobDocumentDelivery.JDC_CarbonCopyRecipientsAsString = "cc1@cc.com, cc2@cc.com";
			jobDocumentDelivery.JDC_BlindCarbonCopyRecipientsAsString = "bcc1@bcc.com, bcc2@bcc.com, bcc3@bcc.com";

			AssertEquals(0, contact.EmailToRecipients.Count);
			AssertEquals(0, contact.EmailCarbonCopyRecipients.Count);
			AssertEquals(0, contact.EmailBlindCarbonCopyRecipients.Count);

			contact.PopulateToCcAndBccRecipients(jobDocumentDelivery);

			AssertEquals(1, contact.EmailToRecipients.Count);
			AssertEquals(2, contact.EmailCarbonCopyRecipients.Count);
			AssertEquals(3, contact.EmailBlindCarbonCopyRecipients.Count);
		}

		public void TestPopulateAddressDetailsFromOrganisation()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.Initialise(menuItem, null);
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			AssertEquals("deliveryContact.Address1", "*** NO ORGANIZATION DETAILS FOUND ***", deliveryContact.Address1);

			OrgHeader organization = Factory.New<OrgHeader>();
			organization.OH_Language = Constants.Languages.ChineseTraditional;
			organization.OH_RL_NKClosestPort = "AUSYD";

			OrgAddress englishAddress = organization.Addresses.AddNew();
			englishAddress.OA_Language = Constants.Languages.English;
			englishAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			englishAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			englishAddress.OA_Address1 = "ARM ENG MAIN";
			englishAddress.OA_RL_NKRelatedPortCode = "GBLON";
			englishAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL";

			OrgAddress chineseAddress = organization.Addresses.AddNew();
			chineseAddress.OA_Language = Constants.Languages.ChineseTraditional;
			chineseAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables.Code);
			chineseAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables.Code);
			chineseAddress.OA_Address1 = "ARM CHT MAIN";

			OrgContact orgContact = organization.Contacts.AddNew();

			deliveryContact.OrgHeaderPK = organization.PK;

			orgContact.WorkingAddressPK = englishAddress.PK;
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Constants.Languages.English;
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			AssertEquals("deliveryContact.Address1", "ARM ENG MAIN", deliveryContact.Address1);
			AssertEquals("deliveryContact.UNLOCO", "GBLON", deliveryContact.UNLOCO.RL_Code);
			AssertEquals("deliveryContact.AdditionalAddressInformation", "ADDITIONAL", deliveryContact.AdditionalAddress);

			orgContact.WorkingAddressPK = chineseAddress.PK;
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Constants.Languages.ChineseTraditional;
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			AssertEquals("deliveryContact.Address1", "ARM CHT MAIN", deliveryContact.Address1);
			AssertEquals("deliveryContact.UNLOCO", "AUSYD", deliveryContact.UNLOCO.RL_Code);
		}

		public void TestCompanyNameMaxLength()
		{
			var docDeliveryContact = new DocDeliveryContact(Factory);
			AssertEquals(100, docDeliveryContact.CompanyNameInfo.MaxLength);
		}

		public void TestUpdateCompanyName()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader organization = Factory.New<OrgHeader>();
			organization.OH_Language = Constants.Languages.EnglishBritish;
			organization.OH_RL_NKClosestPort = "CNSHA";
			organization.OH_FullName = "AStrangeCompanyWithAVeryLongLongLongLongLongLongLongLongLongLongLongLongName";

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.OrgHeaderPK = organization.PK;
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			deliveryContact.UpdateCompanyName();
			AssertEquals("Full Name", organization.OH_FullName, deliveryContact.CompanyName);

			deliveryContact.OrgAddress.OA_CompanyNameOverride = "Overridden Name";
			deliveryContact.UpdateCompanyName();
			AssertEquals("Full Name", deliveryContact.OrgAddress.OA_CompanyNameOverride, deliveryContact.CompanyName);

			deliveryContact.OrgAddressPK = ZGuid.Empty;
			deliveryContact.UpdateCompanyName();
			AssertEquals("CompanyName should be cleared", "", deliveryContact.CompanyName);

			deliveryContact.OrgHeaderPK = organization.PK;
			deliveryContact.UpdateCompanyName();
			AssertEquals("Full Name", deliveryContact.OrgAddress.OA_CompanyNameOverride, deliveryContact.CompanyName);

			deliveryContact.OrgHeaderPK = ZGuid.Empty;
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			deliveryContact.UpdateCompanyName();
			AssertEquals("CompanyName should be cleared", "", deliveryContact.CompanyName);
		}

		public void TestUpdateEmailFaxPhoneDetails()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader organization = Factory.New<OrgHeader>();
			organization.OH_Language = Constants.Languages.EnglishBritish;
			organization.OH_RL_NKClosestPort = "CNSHA";

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.Name = "Test Contact";
			deliveryContact.OrgHeaderPK = organization.PK;
			deliveryContact.PopulateAddressDetailsFromOrganisation();
			OrgContact contact = deliveryContact.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Phone = "025-12345678";
			contact.OC_Email = "test@abc.com";
			contact.OC_Fax = "12345678";
			deliveryContact.OrgAddress.OA_Phone = "025-87654321";
			deliveryContact.OrgAddress.OA_Email = "test@def.com";
			deliveryContact.OrgAddress.OA_Fax = "87654321";

			deliveryContact.UpdatePhoneDetails();
			deliveryContact.UpdateEmailDetails();
			deliveryContact.UpdateFaxDetails();

			AssertEquals("Phone", "025-12345678", deliveryContact.Phone);
			AssertEquals("Email", "test@abc.com", deliveryContact.Email);
			AssertEquals("Fax", "12345678", deliveryContact.Fax);

			deliveryContact.Contacts[0].OC_Phone = "";
			deliveryContact.Contacts[0].OC_Email = "";
			deliveryContact.Contacts[0].OC_Fax = "";

			deliveryContact.UpdatePhoneDetails();
			deliveryContact.UpdateEmailDetails();
			deliveryContact.UpdateFaxDetails();

			AssertEquals("Phone", "025-87654321", deliveryContact.Phone);
			AssertEquals("Email", "test@def.com", deliveryContact.Email);
			AssertEquals("Fax", "87654321", deliveryContact.Fax);

			deliveryContact.OrgHeaderPK = ZGuid.Empty;

			AssertEquals("Phone", "025-87654321", deliveryContact.Phone);
			AssertEquals("Email", "test@def.com", deliveryContact.Email);
			AssertEquals("Fax", "87654321", deliveryContact.Fax);

			deliveryContact.OrgAddressPK = ZGuid.Empty;

			deliveryContact.UpdatePhoneDetails();
			deliveryContact.UpdateEmailDetails();
			deliveryContact.UpdateFaxDetails();

			AssertEquals("Phone", "025-87654321", deliveryContact.Phone);
			AssertEquals("Email", "test@def.com", deliveryContact.Email);
			AssertEquals("Fax", "87654321", deliveryContact.Fax);
		}

		public void TestUpdateCcAndBccEmailDetails()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var contact = organization.Contacts.AddNew();
			contact.OC_ContactName = "Name";
			contact.OC_Email = "email@email.com";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.Code;
			document.OD_CarbonCopyRecipientsAsString = "cc@cc.com";
			document.OD_BlindCarbonCopyRecipientsAsString = "bcc@bcc.com";
			document.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document.OD_AttachmentType = OrgConstants.AttachmentType.PDF;

			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;

			Factory.Save();

			var documentSupporter = new AutoDeliveryBizO(organization);
			var docAutoDelivery = new DocAutoDelivery();
			var docDeliveryContacts = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);

			AssertEquals("docDeliveryContacts.Count", 1, docDeliveryContacts.Count);
			docDeliveryContacts[0].UpdateCcAndBccEmailDetails();
			AssertEquals("Cc emails of document should be combined", docDeliveryContacts[0].EmailCarbonCopyRecipientsAsString, "cc@cc.com");
			AssertEquals("Bcc emails of document should be combined", docDeliveryContacts[0].EmailBlindCarbonCopyRecipientsAsString, "bcc@bcc.com");

			document.OD_DocumentGroup = ContactType.Consignee.Code;
			Factory.Save();
			var newDocDeliveryContact = docDeliveryContacts.AddNew();
			AssertDeliveryContactCCAndBCCEmailAddressAreEmpty(newDocDeliveryContact, "Name", organization.PK);

			document.OD_DocumentGroup = ContactType.All.Code;
			document.OD_AttachmentType = OrgConstants.AttachmentType.XLSX;
			Factory.Save();
			AssertDeliveryContactCCAndBCCEmailAddressAreEmpty(newDocDeliveryContact, "Name", organization.PK);

			contact = organization.Contacts.AddNew();
			contact.OC_ContactName = "New Name";
			contact.OC_Email = "newemail@email.com";
			Factory.Save();

			newDocDeliveryContact = docDeliveryContacts.AddNew();
			AssertDeliveryContactCCAndBCCEmailAddressAreEmpty(newDocDeliveryContact, "New Name", organization.PK);
		}

		void AssertDeliveryContactCCAndBCCEmailAddressAreEmpty(DocDeliveryContact contact, string name, ZGuid orgHeaderPK)
		{
			contact.OrgHeaderPK = orgHeaderPK;
			contact.Name = name;
			contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			contact.AttachmentType = OrgConstants.AttachmentType.PDF;
			AssertNullOrEmpty(contact.EmailCarbonCopyRecipientsAsString);
			AssertNullOrEmpty(contact.EmailBlindCarbonCopyRecipientsAsString);
		}

		public void TestClearCcAndBccRecipients()
		{
			var docDeliveryContact = new DocDeliveryContact(Factory);
			docDeliveryContact.EmailCarbonCopyRecipients.AddNew();
			docDeliveryContact.EmailBlindCarbonCopyRecipients.AddNew();

			AssertEquals(1, docDeliveryContact.EmailCarbonCopyRecipients.Count);
			AssertEquals(1, docDeliveryContact.EmailBlindCarbonCopyRecipients.Count);

			docDeliveryContact.ClearCcAndBccRecipients();

			AssertEquals(0, docDeliveryContact.EmailCarbonCopyRecipients.Count);
			AssertEquals(0, docDeliveryContact.EmailBlindCarbonCopyRecipients.Count);
		}

		public void TestClearToCcAndBccRecipients()
		{
			var docDeliveryContact = new DocDeliveryContact(Factory);
			docDeliveryContact.EmailCarbonCopyRecipients.AddNew();
			docDeliveryContact.EmailBlindCarbonCopyRecipients.AddNew();
			docDeliveryContact.EmailToRecipients.AddNew();

			AssertEquals(1, docDeliveryContact.EmailCarbonCopyRecipients.Count);
			AssertEquals(1, docDeliveryContact.EmailBlindCarbonCopyRecipients.Count);
			AssertEquals(1, docDeliveryContact.EmailToRecipients.Count);

			docDeliveryContact.ClearToCcAndBccRecipients();

			AssertEquals(0, docDeliveryContact.EmailCarbonCopyRecipients.Count);
			AssertEquals(0, docDeliveryContact.EmailBlindCarbonCopyRecipients.Count);
			AssertEquals(0, docDeliveryContact.EmailToRecipients.Count);
		}
	}
}
