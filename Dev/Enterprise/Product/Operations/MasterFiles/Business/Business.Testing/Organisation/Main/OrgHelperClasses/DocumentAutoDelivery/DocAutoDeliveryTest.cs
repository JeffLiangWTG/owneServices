using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class DocAutoDeliveryTest : TestCaseWithFactory
	{
		public void TestGetDeliveryDetailsForContactShouldIgnoreOverriddenOrganisation()
		{
			var contact = PrepareContactWithOverriddenOrg();
			var docDeliveryContact = new DocAutoDelivery().GetDeliveryDetailsForContact(contact);

			AssertEquals("Just1n", docDeliveryContact.Name);
			AssertEquals("just1n@email.com", docDeliveryContact.DeliveryAddress);
		}

		public void TestGetAutoDeliveryContactsShouldIgnoreOverriddenOrganisation()
		{
			var contact = PrepareContactWithOverriddenOrg();

			var menuItem = new Mock<IStmMenuItem>();

			menuItem.Setup(m => m.SU_ContactType).Returns(ContactType.Receivables.Code);
			menuItem.Setup(m => m.SU_MenuName).Returns("Test Document");
			menuItem.Setup(m => m.AttachmentTypes).Returns(new CodeDescriptionPairList());

			var documentSupporter = new OrgHeaderDocumentSupporter(contact.Header);

			var docAutoDelivery = new DocAutoDelivery();
			var contacts = docAutoDelivery.GetDeliveryContacts(menuItem.Object, documentSupporter);

			AssertEquals("There should be 1 contact.", 1, contacts.Count);
			AssertEquals("Just1n", contacts[0].Name);
			AssertEquals("just1n@email.com", contacts[0].DeliveryAddress);

			menuItem.Verify(m => m.SU_ContactType, Times.AtLeastOnce);
			menuItem.Verify(m => m.SU_MenuName, Times.AtLeastOnce);
			menuItem.Verify(m => m.AttachmentTypes, Times.AtLeastOnce);
		}

		OrgContact PrepareContactWithOverriddenOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgOverridden = Factory.NewWithValidTestData<OrgHeader>();
			var addressOverridden = orgOverridden.Addresses.AddNew();
			addressOverridden.OA_Address1 = "A Fantastic Place";

			var contact = org.Contacts.AddNew();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Just1n";
			contact.OC_Email = "just1n@email.com";
			contact.OC_OH_AddressOverride = orgOverridden.PK;
			contact.OC_OA_OrgAddress = addressOverridden.PK;

			var document = contact.Documents.AddNew();
			document.OD_OC = contact.PK;
			document.OD_DocumentGroup = ContactType.Receivables.Code;

			Factory.Save();

			return contact;
		}

		public void TestSystemDefaultContactIsIgnoredWhenHavingJobSpecificRecipients()
		{
			var org = Factory.NewWithValidTestData<OrgHeaderThatSupportJobDocumentRecipient>();
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);
			var documentSupporter = new OrgHeaderDocumentSupporter(org);

			var deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem, documentSupporter, ContactType.Payables.Code, menuItem);
			AssertEquals(1, deliveryContacts.Count);

			var systemDefaultContact = deliveryContacts.OfType<DocDeliveryContact>().Single(d => d.IsSystemDefaultContact);
			AssertNotNull(systemDefaultContact);

			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			jobDocumentDelivery.JDC_ParentID = org.PK;
			jobDocumentDelivery.JDC_ParentTableCode = org.TablePrefix;
			jobDocumentDelivery.JDC_SU_MenuItem = menuItem.PK;
			jobDocumentDelivery.JDC_ContactName = "Just1n Job";
			jobDocumentDelivery.JDC_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			jobDocumentDelivery.JDC_AttachmentType = OrgConstants.AttachmentType.PDF;
			jobDocumentDelivery.JDC_EmailSubjectMacro = "Email Subject Macro";
			Factory.Save();

			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem, documentSupporter, ContactType.Payables.Code, menuItem);
			AssertEquals(1, deliveryContacts.Count);

			var jobSpecificContact = deliveryContacts.OfType<DocDeliveryContact>().Single(d => !d.IsSystemDefaultContact && d.Name == "Just1n Job");
			AssertNotNull(jobSpecificContact);
		}

		public void TestGetOrgDocumentsWithSomeSuppressed()
		{
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);
			var addressForOrg = CreateNewDeliveryAddress("Company", "USBTV");
			var orgHeader = Factory.NewWithValidTestData<OrgHeaderThatSupportJobDocumentRecipient>();
			AddAddressDetailsToOrg(orgHeader, addressForOrg);
			Factory.Save();

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_IsActive = true;
			contact.OC_ContactName = "Just1n";
			contact.OC_Email = "i@just1n.net";
			var orgDocument1 = contact.Documents.AddNew();
			orgDocument1.OD_DocumentGroup = ContactType.Warehouse.Code;
			orgDocument1.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			var orgDocument2 = contact.Documents.AddNew();
			orgDocument2.OD_DocumentGroup = ContactType.Warehouse.Code;
			orgDocument2.OD_DeliverBy = Constants.ContactNotifyModes.Print;

			var deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem, new AutoDeliveryBizO(orgHeader));
			var docAutoDelivery = deliveryContacts[0].DocAutoDelivery;

			var filteredOrgDocuments = docAutoDelivery.FilterOrgDocuments(contact, docAutoDelivery.DocumentSupporter);
			AssertEquals("No OrgDocuments suppressed by specific job, so all OrgDocuments should be found.", 2, filteredOrgDocuments.Count);

			var jobDocumentExclusion = Factory.New<JobDocumentExclusion>();
			jobDocumentExclusion.JDE_ParentID = orgHeader.PK;
			jobDocumentExclusion.JDE_ParentTableCode = orgHeader.TablePrefix;
			jobDocumentExclusion.JDE_OD_Document = orgDocument2.PK;
			Factory.Save();

			filteredOrgDocuments = docAutoDelivery.FilterOrgDocuments(contact, docAutoDelivery.DocumentSupporter);
			AssertEquals("orgDocument2 was suppressed by OrgHeader, so only orgDocument1 will be returned.", 1, filteredOrgDocuments.Count);
			AssertEquals(orgDocument1, filteredOrgDocuments[0]);
		}

		public void TestDocDeliveryContactsContainsAutoDeliverySystemContactsOnly_WithResString()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			var documentSupporter = new OrgHeaderDocumentSupporter(org);
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);

			AutoDelivery.SetDeliveryLanguage(Core.SharedConstants.Languages.ChineseSimplified);
			var deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem, documentSupporter, null, null);
			AssertEquals("1 default contact added", 1, deliveryContacts[0].Contacts.Count);
			AssertEquals("DocDeliveryContact shows system contact's name in delivered language.", ContactType.Warehouse.DefaultName.ToString(Core.SharedConstants.Languages.ChineseSimplified), deliveryContacts[0].Contacts[0].OC_ContactName);

			var defaultContact1 = new DefaultContactFinder(org).DefaultContact(ContactType.Consignee, Enterprise.Core.Constants.TransportModes.Air);
			var defaultContact2 = new DefaultContactFinder(org).DefaultContact(ContactType.Consignor, Enterprise.Core.Constants.TransportModes.Air);
			AssertEquals("no contact added into the contacts selection", 1, deliveryContacts[0].Contacts.Count);
		}

		public void TestGetAutoDeliveryContactsIncludesAdditionalContact()
		{
			var menuQuery = new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice");
			menuQuery.AddToFilter(StmMenuItemSchema.SU_PreventAutoDelivery, ZBool.False);
			StmMenuItem docMenu = Factory.LoadTop1<StmMenuItem>(menuQuery);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contactDetails = CreateNewDocDeliveryContact("NewContact", org);
			OrgContact contact = CreateNewContactInOrganisation(org, contactDetails);
			contact.Documents.AddNew();
			contact.Documents[0].OD_SU_MenuItem = docMenu.PK;

			DocDeliveryContact contactDetails2 = CreateNewDocDeliveryContact("NewContact2", org);
			OrgContact contact2 = CreateNewContactInOrganisation(org, contactDetails2);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_SU_MenuItem = docMenu.PK;

			DocDeliveryContact contactDetails3 = CreateNewDocDeliveryContact("NewContact3", org);
			OrgContact contact3 = CreateNewContactInOrganisation(org, contactDetails3);

			AccTransactionHeader testInvoicingBase = Factory.NewWithValidTestData<AccTransactionHeader>();
			testInvoicingBase.AH_TransactionType = TransactionTypes.Invoice;
			testInvoicingBase.AH_Ledger = LedgerTypes.AccountsReceivable;
			testInvoicingBase.AH_OH = org.PK;
			testInvoicingBase.AH_GB = GlbBranch.CurrentBranch.PK;
			testInvoicingBase.AH_ConsolidatedInvoiceRef = "1234567";

			Factory.Save();

			DocAutoDelivery docDelivery = new DocAutoDelivery();
			DocumentSupporter documentSupporter = new OrgHeaderDocumentSupporter(org);
			DocDeliveryContactCollection returnedDetails = docDelivery.GetDeliveryContacts(docMenu, documentSupporter);
			AssertEquals("expect 2 contacts", 2, returnedDetails.Count);

			var mockDocumentSupporter = new Mock<DocumentSupporter>(Factory.New<DummyBusinessObject>());
			mockDocumentSupporter.Setup(m => m.GetAdditionalDeliveryContact()).Returns(contact3);
			documentSupporter = mockDocumentSupporter.Object;
			docDelivery = new DocAutoDelivery();
			docDelivery.SetOrganisationForTestOnly(org);
			returnedDetails = docDelivery.GetDeliveryContacts(docMenu, documentSupporter);
			AssertEquals("expect 3 contacts because document supporter returns 1 additional contact", 3, returnedDetails.Count);

			mockDocumentSupporter.VerifyAll();
		}

		public void TestGetAutoDeliveryContactsWhenAdditionalContactIsDeactive()
		{
			var menuQuery = new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice");
			menuQuery.AddToFilter(StmMenuItemSchema.SU_PreventAutoDelivery, ZBool.False);
			StmMenuItem docMenu = Factory.LoadTop1<StmMenuItem>(menuQuery);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contactDetails = CreateNewDocDeliveryContact("NewContact", org);
			OrgContact contact = CreateNewContactInOrganisation(org, contactDetails);
			contact.Documents.AddNew();
			contact.Documents[0].OD_SU_MenuItem = docMenu.PK;

			DocDeliveryContact contactDetails2 = CreateNewDocDeliveryContact("NewContact2", org);
			OrgContact contact2 = CreateNewContactInOrganisation(org, contactDetails2);
			contact2.OC_IsActive = false;
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_SU_MenuItem = docMenu.PK;

			DocDeliveryContact contactDetails3 = CreateNewDocDeliveryContact("NewContact3", org);
			OrgContact contact3 = CreateNewContactInOrganisation(org, contactDetails3);
			contact3.OC_IsActive = false;

			AccTransactionHeader testInvoicingBase = Factory.NewWithValidTestData<AccTransactionHeader>();
			testInvoicingBase.AH_TransactionType = TransactionTypes.Invoice;
			testInvoicingBase.AH_Ledger = LedgerTypes.AccountsReceivable;
			testInvoicingBase.AH_OH = org.PK;
			testInvoicingBase.AH_GB = GlbBranch.CurrentBranch.PK;
			testInvoicingBase.AH_ConsolidatedInvoiceRef = "1234567";

			Factory.Save();

			DocAutoDelivery docDelivery = new DocAutoDelivery();
			DocumentSupporter documentSupporter = new OrgHeaderDocumentSupporter(org);
			DocDeliveryContactCollection returnedDetails = docDelivery.GetDeliveryContacts(docMenu, documentSupporter);
			AssertEquals("expect only 1 contact, Boz contact2 is Inactive", 1, returnedDetails.Count);
			AssertEquals("NewContact", returnedDetails.Cast<DocDeliveryContact>().FirstOrDefault()?.Name);

			var mockDocumentSupporter = new Mock<DocumentSupporter>(Factory.New<DummyBusinessObject>());
			mockDocumentSupporter.Setup(m => m.GetAdditionalDeliveryContact()).Returns(contact3);
			documentSupporter = mockDocumentSupporter.Object;
			docDelivery = new DocAutoDelivery();
			docDelivery.SetOrganisationForTestOnly(org);
			returnedDetails = docDelivery.GetDeliveryContacts(docMenu, documentSupporter);
			AssertEquals("expect 1 contact because document supporter no returns additional contact, Boz contact 2&3 are Inactive", 1, returnedDetails.Count);
			AssertEquals("NewContact", returnedDetails.Cast<DocDeliveryContact>().FirstOrDefault()?.Name);

			mockDocumentSupporter.VerifyAll();
		}

		public void TestGetDeliveryContactsWithFilterDirectionIsNotCaseSensitive()
		{
			var organization = Factory.New<OrgHeader>();
			organization.OH_Code = "UNITTEST";

			var contact = organization.Contacts.AddNew();
			contact.OC_OH = organization.PK;
			contact.OC_ContactName = "Unit Tester";
			contact.OC_Email = "unit.tester@cargowise.com";

			var document = contact.Documents.AddNew();
			document.OD_OC = contact.PK;
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_FilterDirection = "All";

			Factory.Save();

			var attachmentTypes = new CodeDescriptionPairList();
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
			attachmentTypes.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);

			var menuItem = new Mock<IStmMenuItem>();

			menuItem.Setup(m => m.SU_ContactType).Returns(ContactType.Receivables.Code);
			menuItem.Setup(m => m.SU_MenuName).Returns("Test Document");
			menuItem.Setup(m => m.AttachmentTypes).Returns(attachmentTypes);

			var documentSupporter = new OrgHeaderDocumentSupporter(organization);

			var autoDelivery = new DocAutoDelivery();
			var contacts = autoDelivery.GetDeliveryContacts(menuItem.Object, documentSupporter);

			AssertEquals("There should be 1 contact.", 1, contacts.Count);
			AssertEquals("Unit Tester", contacts[0].Name);

			menuItem.Verify(m => m.SU_ContactType, Times.AtLeastOnce);
			menuItem.Verify(m => m.SU_MenuName, Times.AtLeastOnce);
			menuItem.Verify(m => m.AttachmentTypes, Times.AtLeastOnce);
		}

		public void TestNewAndLegacyDocumentsAreTreatedTheSameDuringAutoDelivery()
		{
			var deliveryAddress = CreateNewDeliveryAddress("Microsoft", "USNYC");
			var org = CreateNewCompany("Microsoft", deliveryAddress);

			var legacyMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			legacyMenuItem.SU_MenuName = "My Document";
			legacyMenuItem.SU_MenuPath = "Legacy Documents/Test/";

			var docDeliveryContact1 = CreateNewDocDeliveryContact("Contact 1", org);
			var contact1 = CreateNewContactInOrganisation(org, docDeliveryContact1);
			var document1 = contact1.Documents.AddNew();
			document1.OD_SU_MenuItem = legacyMenuItem.PK;

			var newMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			newMenuItem.SU_MenuName = "My Document";
			newMenuItem.SU_MenuPath = "Test/";

			var docDeliveryContact2 = CreateNewDocDeliveryContact("Contact 2", org);
			var contact2 = CreateNewContactInOrganisation(org, docDeliveryContact2);
			var document2 = contact2.Documents.AddNew();
			document2.OD_SU_MenuItem = newMenuItem.PK;

			var documentSupporter = new OrgHeaderDocumentSupporter(org);
			var docAutoDelivery = new DocAutoDelivery();

			var deliveryContacts = docAutoDelivery.GetDeliveryContacts(legacyMenuItem, documentSupporter);
			AssertEquals("deliveryContacts.Count", 2, deliveryContacts.Count);
			AssertEquals("deliveryContacts[0].Name", "Contact 1", deliveryContacts[0].Name);
			AssertEquals("deliveryContacts[1].Name", "Contact 2", deliveryContacts[1].Name);

			deliveryContacts = docAutoDelivery.GetDeliveryContacts(newMenuItem, documentSupporter);
			AssertEquals("deliveryContacts.Count", 2, deliveryContacts.Count);
			AssertEquals("deliveryContacts[0].Name", "Contact 1", deliveryContacts[0].Name);
			AssertEquals("deliveryContacts[1].Name", "Contact 2", deliveryContacts[1].Name);
		}

		public void TestSuppressedDocumentWorksForDocumentsWithSameDocumentID()
		{
			var deliveryAddress = CreateNewDeliveryAddress("Microsoft", "USNYC");
			var org = CreateNewCompany("Microsoft", deliveryAddress);

			var legacyMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			legacyMenuItem.SU_MenuName = "My Document";
			legacyMenuItem.SU_MenuPath = "Legacy Documents/Test/";

			var document1 = org.SuppressedDocuments.AddNew();
			document1.OD_SU_MenuItem = legacyMenuItem.PK;
			document1.OD_DeliverBy = "DND";

			var newMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			newMenuItem.SU_MenuName = "My Document";
			newMenuItem.SU_MenuPath = "Test/";

			var documentSupporter = new OrgHeaderDocumentSupporter(org);
			var docAutoDelivery = new DocAutoDelivery();

			var deliveryContacts = docAutoDelivery.GetDeliveryContacts(legacyMenuItem, documentSupporter);
			AssertEquals("deliveryContacts.Count", 0, deliveryContacts.Count);

			deliveryContacts = docAutoDelivery.GetDeliveryContacts(newMenuItem, documentSupporter);
			AssertEquals("deliveryContacts.Count", 0, deliveryContacts.Count);
		}

		public void TestGlobalOrganizationShouldOnlyGetContactsMatchingUNLOCO()
		{
			var testHelper = new MasterFilesTestHelper(Factory);
			var orgHeader = testHelper.CreateOrganisation("TEST WORLDWIDE CARRIER", "AUSYD", "TEST STREET", "SYDNEY", "0298123654");
			orgHeader.OH_IsGlobalAccount = ZBool.True;
			orgHeader.OH_IsShippingProvider = ZBool.True;

			var sydneyAddress = orgHeader.MainAddress;

			var auklandAddress = orgHeader.Addresses.AddNew(OrgAddressType.Office, false);
			auklandAddress.OA_Address1 = "2 TEST STREET";
			auklandAddress.OA_City = "AUKLAND";
			auklandAddress.OA_PostCode = "1111";
			auklandAddress.OA_State = "AKL";
			auklandAddress.OA_RL_NKRelatedPortCode = "NZAKL";

			var melbourneAddress = orgHeader.Addresses.AddNew(OrgAddressType.Office, false);
			melbourneAddress.OA_Address1 = "3 TEST STREET";
			melbourneAddress.OA_City = "MELBOURNE";
			melbourneAddress.OA_PostCode = "3333";
			melbourneAddress.OA_State = "VIC";
			melbourneAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			var sydneyContact = orgHeader.Contacts.AddNew();
			sydneyContact.OC_ContactName = "BOB - SYD";
			sydneyContact.OC_Title = "BOB in SYDNEY";
			sydneyContact.OC_OA_OrgAddress = sydneyAddress.PK;

			var auklandContact = orgHeader.Contacts.AddNew();
			auklandContact.OC_ContactName = "BILL - AKL";
			auklandContact.OC_Title = "BILL IN AKL";
			auklandContact.OC_OA_OrgAddress = auklandAddress.PK;

			var melbourneContact = orgHeader.Contacts.AddNew();
			melbourneContact.OC_ContactName = "BRUNO - MEL";
			melbourneContact.OC_Title = "BRUNO IN MEL";
			melbourneContact.OC_OA_OrgAddress = melbourneAddress.PK;

			var sydneyDocument = sydneyContact.Documents.AddNew();
			sydneyDocument.OD_DocumentGroup = ContactType.All.Code;
			var auklandDocument = auklandContact.Documents.AddNew();
			auklandDocument.OD_DocumentGroup = ContactType.All.Code;
			var melbourneDocument = melbourneContact.Documents.AddNew();
			melbourneDocument.OD_DocumentGroup = ContactType.All.Code;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.ShippingLine.Code;
			var documentSupporter = new OrgHeaderDocumentSupporter(orgHeader);
			var docAutoDelivery = new DocAutoDelivery();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USCHI";
			var recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should only get contacts from the branch's closest port.", 3, recipients.Count);
			AssertEquals("contact.OC_ContactName", sydneyContact.OC_ContactName, recipients[0].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", sydneyContact.OC_Title, recipients[0].Contact.OC_Title);
			AssertEquals("contact.OC_ContactName", auklandContact.OC_ContactName, recipients[1].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", auklandContact.OC_Title, recipients[1].Contact.OC_Title);
			AssertEquals("contact.OC_ContactName", melbourneContact.OC_ContactName, recipients[2].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", melbourneContact.OC_Title, recipients[2].Contact.OC_Title);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";
			recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should only get contacts from the branch's closest port.", 1, recipients.Count);
			AssertEquals("contact.OC_ContactName", melbourneContact.OC_ContactName, recipients[0].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", melbourneContact.OC_Title, recipients[0].Contact.OC_Title);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZWEL";
			recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should only get contacts from the branch's closest port.", 1, recipients.Count);
			AssertEquals("contact.OC_ContactName", auklandContact.OC_ContactName, recipients[0].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", auklandContact.OC_Title, recipients[0].Contact.OC_Title);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should only get contacts from the branch's closest port.", 2, recipients.Count);
			AssertEquals("contact.OC_ContactName", sydneyContact.OC_ContactName, recipients[0].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", sydneyContact.OC_Title, recipients[0].Contact.OC_Title);
			AssertEquals("contact.OC_ContactName", melbourneContact.OC_ContactName, recipients[1].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", melbourneContact.OC_Title, recipients[1].Contact.OC_Title);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "NZAKL";
			recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should only get contacts from the branch's closest port.", 1, recipients.Count);
			AssertEquals("contact.OC_ContactName", auklandContact.OC_ContactName, recipients[0].Contact.OC_ContactName);
			AssertEquals("contact.OC_Title", auklandContact.OC_Title, recipients[0].Contact.OC_Title);
		}

		#region GetDeliveryMethodWhenNoContactSpecified Fallback Tests

		public void TestGetDeliveryMethodWhenNoContactSpecifiedIsEmailFaxThenPrint()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailFaxThenPrint);
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);
			OrgHeader organization = Factory.New<OrgHeader>();
			DocumentSupporter documentSupporter = new AutoDeliveryBizO(organization);

			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = "Fax Number";
			AssertCorrectDeliveryMethod("", AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Fax);

			organization.MainAddress.OA_Email = "dummy@email.com";
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Email);

			organization.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Email);

			organization.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);
		}

		public void TestGetDeliveryMethodWhenNoContactSpecifiedIsFaxEmailThenPrint()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.FaxEmailThenPrint);
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);
			OrgHeader organization = Factory.New<OrgHeader>();
			DocumentSupporter documentSupporter = new AutoDeliveryBizO(organization);

			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = "Fax Number";
			AssertCorrectDeliveryMethod("", AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Fax);

			organization.MainAddress.OA_Email = "dummy@email.com";
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Fax);

			organization.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Email);

			organization.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);
		}

		public void TestGetDeliveryMethodWhenNoContactSpecifiedIsEmailThenPrint()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailThenPrint);
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);
			OrgHeader organization = Factory.New<OrgHeader>();
			DocumentSupporter documentSupporter = new AutoDeliveryBizO(organization);

			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = "Fax Number";
			AssertCorrectDeliveryMethod("", AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Email = "dummy@email.com";
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Email);

			organization.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Email);

			organization.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);
		}

		public void TestGetDeliveryMethodWhenNoContactSpecifiedIsFaxThenPrint()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.FaxThenPrint);
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);
			OrgHeader organization = Factory.New<OrgHeader>();
			DocumentSupporter documentSupporter = new AutoDeliveryBizO(organization);

			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = "Fax Number";
			AssertCorrectDeliveryMethod("", AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Fax);

			organization.MainAddress.OA_Email = "dummy@email.com";
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Fax);

			organization.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);
		}

		public void TestGetDeliveryMethodWhenNoContactSpecifiedIsPrintOnly()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.PrintOnly);
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);
			OrgHeader organization = Factory.New<OrgHeader>();
			DocumentSupporter documentSupporter = new AutoDeliveryBizO(organization);

			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = "Fax Number";
			AssertCorrectDeliveryMethod("", AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Email = "dummy@email.com";
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);

			organization.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod(AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter), Constants.ContactNotifyModes.Print);
		}

		#endregion

		#region General auto-delivery tests

		public void TestAttachmentTypeForMenuItemHasDefaultType()
		{
			StmMenuItem menuItem = CreateNewMenuItem(ContactType.All);

			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			company.MainAddress.OA_Address1 = "	Test Address";
			company.MainAddress.OA_Email = "123@123.com";

			var deliveryContact = AutoDelivery.GetDeliveryDetailsForSystemDefaultContact(company, menuItem);
			AssertEquals("PDF", deliveryContact.AttachmentType);

			menuItem.SU_DefaultAttachmentType = OrgConstants.AttachmentType.HTMF;
			deliveryContact = AutoDelivery.GetDeliveryDetailsForSystemDefaultContact(company, menuItem);
			AssertEquals("HTMF", deliveryContact.AttachmentType);
		}

		public void TestDeliveryContactTypesForAirAndSea()
		{
			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Freight Manager");
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Freight Manager");
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Freight Manager");
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Freight Manager");

			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Air Freight Manager", Constants.TransportModes.Air, "");
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Air Freight Manager", Constants.TransportModes.Air, "");
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Air Freight Manager", Constants.TransportModes.Air, "");
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Air Freight Manager", Constants.TransportModes.Air, "");

			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Air Freight Manager", Constants.TransportModes.Air, Constants.ContainerModes.Loose);
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Air Freight Manager", Constants.TransportModes.Air, Constants.ContainerModes.Loose);
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Air Freight Manager", Constants.TransportModes.Air, Constants.ContainerModes.Loose);
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Air Freight Manager", Constants.TransportModes.Air, Constants.ContainerModes.Loose);

			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Sea Freight Manager", Constants.TransportModes.Sea, "");
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Sea Freight Manager", Constants.TransportModes.Sea, "");
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Sea Freight Manager", Constants.TransportModes.Sea, "");
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Sea Freight Manager", Constants.TransportModes.Sea, "");

			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			AssertSpecificDeliveryContactType(ContactType.ExportFreightAgent, "The Export Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertSpecificDeliveryContactType(ContactType.ImportFreightAgent, "The Import Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertSpecificDeliveryContactType(ContactType.ExportDepot, "The Export Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			AssertSpecificDeliveryContactType(ContactType.ImportDepot, "The Import Sea Freight Manager", Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertSpecificDeliveryContactType(ContactType.ImportSeaDepot, "");
		}

		void AssertSpecificDeliveryContactType(ContactType typeOfContactToDeliverTo, string expectedName, string transportMode = "", string containerMode = "")
		{
			var menuItem = CreateNewMenuItem(typeOfContactToDeliverTo);
			var company = Factory.New<OrgHeader>();
			company.MainAddress.OA_Address1 = "Main Address";
			company.MainAddress.OA_Fax = "234234";
			company.MainAddress.OA_Email = "blah@blah.com";

			var documentSupporter = new AutoDeliveryBizO(company, transportMode, containerMode);

			AssertEquals(expectedName, AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter)[0].Name);
			AssertEquals(company.PK, AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter)[0].OrgHeaderPK);
		}

		public void TestOverriddenDeliveryDetails()
		{
			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Receivables);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			JobDocAddressParentForTesting parent = new JobDocAddressParentForTesting(Factory);
			JobDocAddress overriddenDeliveryDetails = parent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			overriddenDeliveryDetails.ContactPK = ZGuid.Empty;
			overriddenDeliveryDetails.E2_OA_Address = ZGuid.Empty;
			overriddenDeliveryDetails.E2_CompanyName = "Overridden Company Name";
			overriddenDeliveryDetails.E2_Contact = "New Contact";
			overriddenDeliveryDetails.E2_Email = "som@som.com";
			overriddenDeliveryDetails.E2_Fax = "22222222";
			overriddenDeliveryDetails.E2_Phone = "444444444";
			overriddenDeliveryDetails.E2_Address1 = "addy1";
			overriddenDeliveryDetails.E2_Address2 = "addy2";
			overriddenDeliveryDetails.E2_City = "seeety";
			overriddenDeliveryDetails.E2_State = "staaaaaate";
			overriddenDeliveryDetails.E2_Postcode = "postycodie";

			AutoDeliveryBizO supporter = new AutoDeliveryBizO(Factory.New<OrgHeader>(), "AIR", "", "AUSYD", "USLAX", false, null, overriddenDeliveryDetails);

			DocDeliveryContactCollection contacts = AutoDelivery.GetDeliveryContacts(recMenuItem, supporter);
			AssertEquals("Only 1 contact should be present - the overridden contact", 1, contacts.Count);
			AssertNotNull("Contacts[0].DocAutoDelivery", contacts[0].DocAutoDelivery);
			AssertEquals(AutoDelivery.GetHashCode(), contacts[0].DocAutoDelivery.GetHashCode());

			DocDeliveryContact deliveryContact = contacts[0];
			AssertEquals(recMenuItem, deliveryContact.MenuItem);
			AssertEquals("Overridden Company Name", deliveryContact.CompanyName);
			AssertEquals("New Contact", deliveryContact.Name);
			AssertEquals("som@som.com", deliveryContact.Email);
			AssertEquals("22222222", deliveryContact.Fax);
			AssertEquals("444444444", deliveryContact.Phone);
			AssertEquals("EML", deliveryContact.DeliveryMethod);
			AssertEquals("PDF", deliveryContact.AttachmentType);
			AssertEquals("addy1", deliveryContact.Address1);
			AssertEquals("addy2", deliveryContact.Address2);
			AssertEquals("seeety", deliveryContact.City);
			AssertEquals("staaaaaate", deliveryContact.State);
			AssertEquals("postycodie", deliveryContact.PostCode);

			OrgHeader header = Factory.New<OrgHeader>();
			deliveryContact.OrgHeaderPK = header.PK;
			OrgAddress newAdr = header.Addresses.AddNew();
			newAdr.OA_Address1 = "new adr";
			overriddenDeliveryDetails.E2_OA_Address = newAdr.PK;
			header.MainAddress.OA_Address1 = "adr1";
			deliveryContact.DeliveryMethod = "FAX";
			AssertEquals("new adr", deliveryContact.Address1);
		}

		public void TestOverriddenDeliveryDetails_WithOrgAddress()
		{
			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Receivables);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address = header.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_Address1 = "Overridden Address1";
			address.OA_Address2 = "addy2";
			address.OA_City = "seeety";
			address.OA_State = "staaaaaate";
			address.OA_PostCode = "postycodie";

			AutoDeliveryBizO supporter = new AutoDeliveryBizO(Factory.New<OrgHeader>(), "AIR", "", "AUSYD", "USLAX", false, null, address);

			DocDeliveryContactCollection contacts = AutoDelivery.GetDeliveryContacts(recMenuItem, supporter);
			AssertEquals("Only 1 contact should be present - the overridden contact", 1, contacts.Count);

			DocDeliveryContact deliveryContact = contacts[0];
			AssertEquals(recMenuItem, deliveryContact.MenuItem);
			AssertEquals("Overridden Address1", deliveryContact.Address1);
			AssertEquals("addy2", deliveryContact.Address2);
			AssertEquals("seeety", deliveryContact.City);
			AssertEquals("staaaaaate", deliveryContact.State);
			AssertEquals("postycodie", deliveryContact.PostCode);
		}

		public void TestSystemDefaultContact()
		{
			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Receivables);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader company = Factory.New<OrgHeader>();
			company.MainAddress.OA_Address1 = "Main Address";
			company.MainAddress.OA_Fax = "234234";
			company.MainAddress.OA_Email = "blah@blah.com";

			OrgAddress aRMAddress = company.Addresses.AddNew();
			aRMAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);
			aRMAddress.OA_Address1 = "ARM Address";
			aRMAddress.OA_Fax = ZString.Empty;
			aRMAddress.OA_Email = ZString.Empty;

			AssertCorrectDeliveryMethod("Email and fax not entered, delivery method should be print", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Print);

			aRMAddress.OA_Fax = "1234567";
			AssertCorrectDeliveryMethod("Valid fax entered, delivery method should be fax", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Fax);

			aRMAddress.OA_Email = "blobs@blobs.com";
			AssertCorrectDeliveryMethod("Email entered, delivery method should be email", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Email);

			aRMAddress.Delete();
			AssertCorrectDeliveryMethod("Email entered on Main Address, delivery method should be email", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Email);

			company.MainAddress.OA_Email = ZString.Empty;
			AssertCorrectDeliveryMethod("Fax entered, no email address, delivery method should be fax", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Fax);

			company.MainAddress.OA_Fax = ZString.Empty;
			AssertCorrectDeliveryMethod("Fax not entered, no email address, delivery method should be print", AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company)), Constants.ContactNotifyModes.Print);
		}

		public void TestSystemDefaultContactDoesntCreateDuplicates_WithPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertSystemDefaultContactDoesntCreateDuplicates();
		}

		public void TestSystemDefaultContactDoesntCreateDuplicates_WithoutPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertSystemDefaultContactDoesntCreateDuplicates();
		}

		void AssertSystemDefaultContactDoesntCreateDuplicates()
		{
			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Consignor);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Office;

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			AssertEquals(0, organisation.Contacts.Count);

			var contacts = AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(organisation));
			//Let's call GetDeliveryContacts a few more times and check it doesn't create duplicated contacts on the organisation
			AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(organisation));
			AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(organisation));
			AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(organisation));

			AssertEquals(1, contacts.Count);
			AssertEquals(1, contacts[0].Contact.ParentOrg.Contacts.Count);
		}

		public void TestSystemDefaultContactSalutation()
		{
			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Receivables);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DocDeliveryContactCollection contacts = AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company));

			AssertEquals(1, contacts.Count);
			AssertEquals(true, contacts[0].Contact.IsSystemDefaultContact);
			AssertEquals(ContactType.Receivables.DefaultName, contacts[0].Contact.SystemDefaultContactName);
			AssertEquals(DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.EnglishAmerican, string.Empty).Replace(Core.Constants.SalutationMacros.Name, contacts[0].Name), contacts[0].Contact.OC_Salutation);
		}

		public void TestSystemDefaultContactSalutationWhenPrintSystemCreatedContactWhenNoRealContactFoundIsNo()
		{
			bool oldValue = DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.Value;
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.Value);

			StmMenuItem recMenuItem = CreateNewMenuItem(ContactType.Receivables);
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DocDeliveryContactCollection contacts = AutoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company));

			AssertEquals(1, contacts.Count);
			AssertEquals(true, contacts[0].Contact.IsSystemDefaultContact);
			AssertNotNull(contacts[0].Contact.SystemDefaultContactName);
			AssertEquals((NoResString)"", contacts[0].Contact.SystemDefaultContactName);

			contacts[0].Contact.Factory.Save();
			AssertNull("Temporary contact should not be saved to database", new BusinessObjectFactory().Load<OrgContact>(contacts[0].Contact.PK));
			AssertEquals("Incorrect saving action is logged.", "SavingReadOnlyFactory", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);
		}

		public void TestPlainDocGroupFilter()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);

			DocDeliveryContact expectedContact = contact1Details;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(expectedContact, deliveryContacts));
		}

		public void TestPlainDocGroupFilterWithSameContactTwoDeliveries()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactIsInList(contact1Details, deliveryContacts));
			Assert(ContactIsInList(contact2Details, deliveryContacts));
		}

		public void TestPlainDocGroupFilterWithInactiveContact()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			contact1.OC_IsActive = false;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count - inactive contact is excluded", 1, deliveryContacts.Count);
			Assert(ContactIsInList(contact2Details, deliveryContacts));
		}

		public void TestSpecificDocFilter()
		{
			StmMenuItem tRNMenuItem = CreateNewMenuItem(ContactType.LocalTransport);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			OrgDocument contact1Doc1 = contact1.Documents.AddNew();
			contact1Doc1.OD_DocumentGroup = ContactType.TransportServices.Code;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			OrgDocument contact2Doc1 = contact2.Documents.AddNew();
			contact2Doc1.OD_SU_MenuItem = tRNMenuItem.PK;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.All.Code;
			contact3.Documents.AddNew();
			contact3.Documents[1].OD_DocumentGroup = ContactType.LocalTransport.Code;
			contact3.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_SU_MenuItem = tRNMenuItem.PK;
			contact4.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			contact4.Documents.AddNew();
			contact4.Documents[1].OD_DocumentGroup = ContactType.LocalTransport.Code;

			#endregion

			// Exptected Contact:
			// Contact1 does not have the correct document group for the menu item
			// Contact2 is the expected recipient.
			// Contact3 is to receive ALL, but has TRN docs suppressed
			// Contact4 is to receive all TRN docs, but has this specific doc suppressed
			DocDeliveryContact expectedContact = contact2Details;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(tRNMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(expectedContact, deliveryContacts));
		}

		public void TestSpecificDocFilterWithDifferentDocSuppressed()
		{
			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			StmMenuItem sALMenuItem1 = CreateNewMenuItem(ContactType.Sales);
			sALMenuItem1.SU_MenuName = "Document 1";
			StmMenuItem sALMenuItem2 = CreateNewMenuItem(ContactType.Sales);
			sALMenuItem2.SU_MenuName = "Document 2";

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Sales.Code;
			contact1.Documents.AddNew();
			contact1.Documents[1].OD_SU_MenuItem = sALMenuItem2.PK;
			contact1.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			// Contact1 has doc group SAL, and different doc suppressed so should receive doc
			DocDeliveryContact expectedContact = contact1Details;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(sALMenuItem1, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(expectedContact, deliveryContacts));
		}

		public void TestMatchOnParentMenuCommand()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Dexter Morgan";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			StmMenuItem menuItem1 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"));
			contact1Document.OD_SU_MenuItem = menuItem1.PK;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Debra Morgan";
			StmMenuItem menuItem2 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice"));

			StmMenuItem menuItem3 = CreateNewMenuItem(ContactType.Warehouse);

			var documentSupporter = new OrgHeaderDocumentSupporter(org);

			menuItem1.SU_BusinessContext = nameof(BusinessContext.WhsPeriodicBilling);
			menuItem2.SU_BusinessContext = nameof(BusinessContext.CFSContainerRego);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem2, documentSupporter, menuItem1.SU_ContactType, menuItem1);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem1, documentSupporter, menuItem1.SU_ContactType, menuItem2);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem3, documentSupporter, menuItem1.SU_ContactType, menuItem1);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem1, documentSupporter, menuItem1.SU_ContactType, menuItem3);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem3, documentSupporter, menuItem1.SU_ContactType, menuItem3);
			AssertEquals("The Accounts Payable Manager", deliveryContacts[0].Contact.OC_ContactName);

			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_SU_MenuItem = menuItem2.PK;
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem2, documentSupporter, menuItem1.SU_ContactType, menuItem1);
			AssertEquals(2, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem3, documentSupporter, menuItem1.SU_ContactType, menuItem1);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem2, documentSupporter, menuItem1.SU_ContactType, menuItem3);
			AssertEquals(1, deliveryContacts.Count);
			deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem3, documentSupporter, menuItem1.SU_ContactType, menuItem3);
			AssertEquals("The Accounts Payable Manager", deliveryContacts[0].Contact.OC_ContactName);
		}

		public void TestDocBuilderInvoice_IsBusinessContextAware()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Dexter Morgan";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			StmMenuItem menuItem1 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"));
			contact1Document.OD_SU_MenuItem = menuItem1.PK;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Debra Morgan";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			StmMenuItem menuItem2 = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice"));
			contact2Document.OD_SU_MenuItem = menuItem2.PK;

			var documentSupporter = new OrgHeaderDocumentSupporter(org);

			menuItem1.SU_BusinessContext = nameof(BusinessContext.WhsPeriodicBilling);
			menuItem2.SU_BusinessContext = nameof(BusinessContext.WhsPeriodicBilling);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem2, documentSupporter);
			AssertEquals("Same BusinessContext => Two delivery contacts expected", 2, deliveryContacts.Count);

			menuItem2.SU_BusinessContext = nameof(BusinessContext.CFSContainerRego);
			deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem2, documentSupporter);
			AssertEquals("Different BusinessContexts => One delivery contact expected", 1, deliveryContacts.Count);
			AssertEquals("Debra Morgan should be the delivery contact", "Debra Morgan", deliveryContacts[0].Contact.OC_ContactName);
		}

		public void TestLegacyAndDocBuilderInvoiceShareTheSameContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			StmMenuItem legacyMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice"));
			legacyMenuItem.SU_BusinessContext = nameof(BusinessContext.ARInvoice);
			contact1Document.OD_SU_MenuItem = legacyMenuItem.PK;

			var documentSupporter = new OrgHeaderDocumentSupporter(org);
			StmMenuItem docBuilderMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"));
			docBuilderMenuItem.SU_BusinessContext = nameof(BusinessContext.ARInvoice);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(docBuilderMenuItem, documentSupporter);
			AssertEquals("1 delivery contact added for document requiring DocBuilder Invoice", 1, deliveryContacts.Count);
			AssertEquals("Fred Bloggs is the only delivery contact", "Fred Bloggs", deliveryContacts[0].Name);

			org.Contacts.RemoveAll();
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Peter Adams";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_SU_MenuItem = docBuilderMenuItem.PK;
			deliveryContacts = AutoDelivery.GetDeliveryContacts(legacyMenuItem, documentSupporter);
			AssertEquals("1 delivery contact added for document requiring Legacy Invoice", 1, deliveryContacts.Count);
			AssertEquals("Peter Adams is the only delivery contact", "Peter Adams", deliveryContacts[0].Name);
		}

		public void TestPaymentVoucherOfTwoBusinessContextShareTheSameContact()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			ZQuery query = new ZQuery(StmMenuItemSchema.SU_MenuName, "Payment Voucher");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "PaymentApproval");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "");
			StmMenuItem paymentVoucherMenuItemOfPaymentApproval = Factory.LoadTop1<StmMenuItem>(query);
			contact1Document.OD_SU_MenuItem = paymentVoucherMenuItemOfPaymentApproval.PK;

			OrgHeaderDocumentSupporter documentSupporter = new OrgHeaderDocumentSupporter(org);
			query = new ZQuery(StmMenuItemSchema.SU_MenuName, "Payment Voucher");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "APTransaction");
			query.AddToFilter(StmMenuItemSchema.SU_MenuPath, "");
			StmMenuItem paymentVoucherMenuItemOfAPTransaction = Factory.LoadTop1<StmMenuItem>(query);

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(paymentVoucherMenuItemOfAPTransaction, documentSupporter);
			AssertEquals("1 delivery contact added for document requiring Payment Voucher of APTransaction", 1, deliveryContacts.Count);
			AssertEquals("Fred Bloggs is the only delivery contact", "Fred Bloggs", deliveryContacts[0].Name);

			org.Contacts.RemoveAll();
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Peter Adams";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_SU_MenuItem = paymentVoucherMenuItemOfAPTransaction.PK;
			deliveryContacts = AutoDelivery.GetDeliveryContacts(paymentVoucherMenuItemOfPaymentApproval, documentSupporter);
			AssertEquals("1 delivery contact added for document requiring Payment Voucher of PaymentApproval", 1, deliveryContacts.Count);
			AssertEquals("Peter Adams is the only delivery contact", "Peter Adams", deliveryContacts[0].Name);
		}

		public void TestAdditionalExcludeFilter()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Consignor.Code;

			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Fred Bloggs";
			OrgDocument contact1Document = contact1.Documents.AddNew();
			contact1Document.OD_SU_MenuItem = menuItem.PK;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "John Doe";
			OrgDocument contact2Document = contact2.Documents.AddNew();
			contact2Document.OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "A. N. Other";
			OrgDocument contact3Document = contact3.Documents.AddNew();
			contact3Document.OD_DocumentGroup = ContactType.Warehouse.Code;

			menuItem.SU_MenuName = "Menu requiring specific document for recipient";

			OrgHeaderDocumentSupporter documentSupporter = new OrgHeaderDocumentSupporter(org);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);

			AssertEquals("1 delivery contact added for document requiring specific document for recipient", 1, deliveryContacts.Count);
			AssertEquals("Fred Bloggs is the only delivery contact", "Fred Bloggs", deliveryContacts[0].Name);

			menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = "A menu valid for WHS group";
			menuItem.SU_ContactType = ContactType.Warehouse.Code;

			documentSupporter = new OrgHeaderDocumentSupporter(org);
			deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);

			AssertEquals("1 delivery contact added for a Warehouse document if RequireSpecificDocumentRecipient is not set", 1, deliveryContacts.Count);
			AssertEquals("A. N. Other is the only delivery contact", "A. N. Other", deliveryContacts[0].Name);
		}

		public void TestFallbackToSystemDefault()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			// No contacts in org, so should receive system default contact
			DocDeliveryContact systemDefault = CreateNewDocDeliveryContact(ContactType.Warehouse.DefaultName, org);

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(systemDefault, deliveryContacts));
		}

		public void TestUseDifferentDeliveryMethod()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			contact1.Documents[0].OD_DeliverBy = "FAX";
			contact1.Documents[0].OD_AttachmentType = "";
			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);

			DocDeliveryContact expectedContact = contact1Details;
			expectedContact.DeliveryMethod = "FAX";
			expectedContact.AttachmentType = "";

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(expectedContact, deliveryContacts));
		}

		public void TestAutoDeliveryContactAutoDeliveryIsNotNull()
		{
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);
			var addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			var org = CreateNewCompany("Company1", addressForOrg);

			var contactDetail = CreateNewDocDeliveryContact("Contact1", org);

			var contact = CreateNewContactInOrganisation(org, contactDetail);
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			contact.Documents[0].OD_DeliverBy = "FAX";
			contact.Documents[0].OD_AttachmentType = "";

			var documentSupporter = new AutoDeliveryBizO(org);
			var deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals(documentSupporter.GetHashCode(), AutoDelivery.DocumentSupporter.GetHashCode());

			AssertEquals("deliveryContacts.Count", 1, deliveryContacts.Count);
			AssertNotNull("deliveryContacts[0].DocAutoDelivery", deliveryContacts[0].DocAutoDelivery);
			AssertEquals(deliveryContacts[0].DocAutoDelivery.GetHashCode(), AutoDelivery.GetHashCode());

			var newDeliveryContact = deliveryContacts.AddNew();
			AssertNotNull("newDeliveryContact.DocAutoDelivery", newDeliveryContact.DocAutoDelivery);
			AssertEquals(newDeliveryContact.DocAutoDelivery.GetHashCode(), AutoDelivery.GetHashCode());
		}

		public void TestFilterOrgDocumentWithDocumentSupporterNull()
		{
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);
			var addressForOrg = CreateNewDeliveryAddress("Company", "USBTV");
			var orgHeader = CreateNewCompany("Company", addressForOrg);

			var deliveryContacts = AutoDelivery.GetDeliveryContacts(menuItem, new AutoDeliveryBizO(orgHeader));
			var docAutoDelivery = deliveryContacts[0].DocAutoDelivery;

			var contact = deliveryContacts[0].Contact;
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;
			contact.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.Email;

			var orgDocuments = docAutoDelivery.FilterOrgDocuments(contact, docAutoDelivery.DocumentSupporter);
			AssertEquals("Documents list should not be empty.", true, orgDocuments.Any());

			var orgDocumentsEmpty = docAutoDelivery.FilterOrgDocuments(contact, null);
			AssertEquals("Documents list should be empty.", false, orgDocumentsEmpty.Any());
		}

		public void TestGetDeliveryContactsForDocPack_WillNotGetAnEmailWithoutRecipient_WhenMainAddressChangedToAnotherOne()
		{
			var jobFactory = new BusinessObjectFactory();
			var org = jobFactory.NewWithValidTestData<OrgHeaderThatSupportJobDocumentRecipient>();
			var menuItem = CreateNewMenuItem(ContactType.Warehouse);
			var documentSupporter = new OrgHeaderDocumentSupporter(org);
			org.Addresses[0].OA_Email = "test@test.com";
			jobFactory.Save();

			var orgLoadedByAnotherFactory = Factory.Load<OrgHeader>(org.PK);
			var newMainAddress = orgLoadedByAnotherFactory.Addresses.AddNew();
			newMainAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			newMainAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			newMainAddress.OA_Email = string.Empty;
			Factory.Save();

			var deliveryContacts = AutoDelivery.GetDeliveryContactsForDocPack(menuItem, documentSupporter, ContactType.Payables.Code, menuItem);

			AssertEquals("Delivery Contacts Count", 1, deliveryContacts.Count);
			AssertEquals("Delivery method should be PRN if email is empty.", "PRN", deliveryContacts[0].DeliveryMethod);
		}

		public void TestAutoDelivery_WillUpdateDeliveryMethodWhenOrganizationAccountReceivableMailingAddressIsNull()
		{
			var testHelper = new MasterFilesTestHelper(Factory);
			var orgHeader = testHelper.CreateOrganisation("TEST WORLDWIDE CARRIER", "AUSYD", "TEST STREET", "SYDNEY", "0298123654");
			orgHeader.OH_IsGlobalAccount = ZBool.True;
			orgHeader.OH_IsShippingProvider = ZBool.True;
			orgHeader.MainAddress.OA_Email = "123@qq.com";

			var melbourneAddress = orgHeader.Addresses.AddNew(OrgAddressType.Receivables, true);
			melbourneAddress.OA_Address1 = "3 TEST STREET";
			melbourneAddress.OA_City = "MELBOURNE";
			melbourneAddress.OA_PostCode = "3333";
			melbourneAddress.OA_State = "VIC";
			melbourneAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			melbourneAddress.OA_Fax = "+61 2 9694 8099";

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			menuItem.SU_PreventAutoDelivery = true;
			menuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;
			var documentSupporter = new OrgHeaderDocumentSupporter(orgHeader);
			var docAutoDelivery = new DocAutoDelivery();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USCHI";
			var recipients = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should generate system default contact.", 1, recipients.Count);
			var systemDefaultContact = recipients[0];
			AssertEquals("shoud set delivery method to Fax", "FAX", systemDefaultContact.DeliveryMethod);

			melbourneAddress.OA_Email = "123@qq.com";
			var recipients1 = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);
			AssertEquals("Global Organization should generate system default contact.", 1, recipients.Count);
			var systemDefaultContact1 = recipients1[0];
			AssertEquals("shoud set delivery method to E-Mail", "EML", systemDefaultContact1.DeliveryMethod);
		}

		#endregion

		#region Aggregate contact types (eg Freight docs)

		public void TestGetPreciseContactTypeByTransportMode()
		{
			var contactType = ContactType.ExportFreightAgent;
			var preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "SEA");
			AssertEquals(preciseContactType, ContactType.ExportSeaFreightAgent);

			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "AIR");
			AssertEquals(preciseContactType, ContactType.ExportAirFreightAgent);

			contactType = ContactType.ImportFreightAgent;
			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "SEA");
			AssertEquals(preciseContactType, ContactType.ImportSeaFreightAgent);

			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "AIR");
			AssertEquals(preciseContactType, ContactType.ImportAirFreightAgent);

			contactType = ContactType.ImportDepot;
			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "SEA");
			AssertEquals(preciseContactType, ContactType.ImportSeaDepot);

			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "AIR");
			AssertEquals(preciseContactType, ContactType.ImportAirDepot);

			contactType = ContactType.ExportDepot;
			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "SEA");
			AssertEquals(preciseContactType, ContactType.ExportSeaDepot);

			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "AIR");
			AssertEquals(preciseContactType, ContactType.ExportAirDepot);

			preciseContactType = DocAutoDelivery.GetPreciseContactTypeByTransportMode(contactType, "");
			AssertEquals(preciseContactType, ContactType.ExportDepot);
		}

		public void TestExportFreightDocs()
		{
			StmMenuItem fWEMenuItem = CreateNewMenuItem(ContactType.ExportFreightAgent);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.ExportFreightAgent.Code;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.ExportAirFreightAgent.Code;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.ExportSeaFreightAgent.Code;

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.FreightAgent.Code;

			OrgContact contact5 = CreateNewContactInOrganisation(org, contact5Details);
			contact5.Documents.AddNew();
			contact5.Documents[0].OD_DocumentGroup = ContactType.ExportAirFreightAgent.Code;
			contact5.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			contact5.Documents.AddNew();
			contact5.Documents[1].OD_DocumentGroup = ContactType.FreightAgent.Code;

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR");
			// Exptected Contacts:
			// Contact1 has FWE as doc group so will receive doc
			// Contact2 has FEA as doc group, so will receive doc as transport mode is AIR
			// Contact3 has FES as doc group, but will not receive doc as transport mode is AIR
			// Contact4 has FWD as doc group, so will receive doc
			// Contact5 has FWD as doc group, but also has FEA doc group suppressed so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact4Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(fWEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestImportFreightDocs()
		{
			StmMenuItem fWEMenuItem = CreateNewMenuItem(ContactType.ImportFreightAgent);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.ImportFreightAgent.Code;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.ImportAirFreightAgent.Code;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.ImportSeaFreightAgent.Code;

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.FreightAgent.Code;

			OrgContact contact5 = CreateNewContactInOrganisation(org, contact5Details);
			contact5.Documents.AddNew();
			contact5.Documents[0].OD_DocumentGroup = ContactType.ImportSeaFreightAgent.Code;
			contact5.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			contact5.Documents.AddNew();
			contact5.Documents[1].OD_DocumentGroup = ContactType.FreightAgent.Code;

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "SEA");
			// Exptected Contacts:
			// Contact1 has FWI as doc group so will receive doc
			// Contact2 has FIA as doc group, but will not receive doc as transport mode is SEA
			// Contact3 has FIS as doc group, so will receive doc as transport mode is SEA
			// Contact4 has FWD as doc group, so will receive doc
			// Contact5 has FWD as doc group, but also has FIS doc group suppressed so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact3Details, contact4Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(fWEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		#endregion

		#region Local Port, Foreign Port, Direction, Related Party

		public void TestContactForGroup_ButDNDOnSpecificDocument_WithLegacyVersion()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);
			StmMenuItem cNRMenuItemLegacy = CreateNewMenuItem(ContactType.Consignor);
			cNRMenuItem.SU_MenuPath = "Departure";
			cNRMenuItemLegacy.SU_MenuPath = "Legacy Documents/Departure";

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_SU_MenuItem = cNRMenuItemLegacy.PK;
			contact1.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			contact1.Documents.AddNew();
			contact1.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_SU_MenuItem = cNRMenuItemLegacy.PK;
			contact2.Documents.AddNew();
			contact2.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_SU_MenuItem = cNRMenuItemLegacy.PK;

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			//Contact1 has DND on specific document but accepts the document group it's in. Still, it should not receive the document.
			//Contact2 has DND on group but accepts the specific document. We're fine with it accepting the document.
			//Contacts 3 and 4 should receive, having no DND.

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", true);

			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact2Details, contact3Details, contact4Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnDirection()
		{
			var cnrMenuItem = CreateNewMenuItem(ContactType.Consignor);

			var addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			var org = CreateNewDummyCompany("Company1", addressForOrg);

			#region Setup contacts for test

			var contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			var contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			var contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			var contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			var contact5Details = CreateNewDocDeliveryContact("Contact5", org);
			var contact6Details = CreateNewDocDeliveryContact("Contact6", org);
			var contact7Details = CreateNewDocDeliveryContact("Contact7", org);
			var contact8Details = CreateNewDocDeliveryContact("Contact8", org);

			var contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Import;

			var contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Export;

			var contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;

			var contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			contact4.Documents[0].OD_FilterShipmentMode = "AIR";

			var contact5 = CreateNewContactInOrganisation(org, contact5Details);
			contact5.Documents.AddNew();
			contact5.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact5.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Import;
			contact5.Documents[0].OD_FilterShipmentMode = "SEA";

			var contact6 = CreateNewContactInOrganisation(org, contact6Details);
			contact6.Documents.AddNew();
			contact6.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact6.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			contact6.Documents[0].OD_FilterShipmentMode = "AIR";
			contact6.Documents[0].OD_FilterLocalPort = "AUSYD";

			var contact7 = CreateNewContactInOrganisation(org, contact7Details);
			contact7.Documents.AddNew();
			contact7.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact7.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
			contact7.Documents[0].OD_FilterShipmentMode = "AIR";
			contact7.Documents[0].OD_FilterLocalPort = "AUBNE";

			var contact8 = CreateNewContactInOrganisation(org, contact8Details);
			contact8.Documents.AddNew();
			contact8.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact8.Documents[0].OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.CrossTrade;
			contact8.Documents[0].OD_FilterShipmentMode = "AIR";
			contact8.Documents[0].OD_FilterLocalPort = "AUBNE";

			#endregion

			var deliveryFilter = new DummyAutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", true, Directions.Import);
			// Exptected Contacts:
			// Contact1 is IMPORT so will receive doc
			// Contact2 is EXPORT so will not receive doc
			// Contact3 is BLANK so will receive doc
			// Contact4 is IMPORT AIR so will receive doc
			// Contact5 is IMPORT SEA so will not receive doc
			// Contact6 is BLANK AIR with local port AUSYD so will receive doc
			// Contact7 is DOMESTIC so will not receive doc
			// Contact8 is CROSSTRADE so will not receive doc
			var expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact3Details, contact4Details, contact6Details });

			var deliveryContacts = AutoDelivery.GetDeliveryContacts(cnrMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 4, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new DummyAutoDeliveryBizO(org, "AIR", "", "AUBNE", "USLAX", false, Directions.Export);
			// Exptected Contacts:
			// Contact1 is IMPORT so will not receive doc
			// Contact2 is EXPORT so will receive doc
			// Contact3 is BLANK so will receive doc
			// Contact4 is IMPORT AIR so will not receive doc
			// Contact5 is IMPORT SEA so will not receive doc
			// Contact6 is BLANK AIR with local port AUSYD so will not receive doc
			// Contact7 is DOMESTIC so will not receive doc
			// Contact8 is CROSSTRADE so will not receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact2Details, contact3Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cnrMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new DummyAutoDeliveryBizO(org, "AIR", "", "AUBNE", "USLAX", false, Directions.Domestic);
			// Exptected Contacts:
			// Contact1 is IMPORT so will not receive doc
			// Contact2 is EXPORT so will not receive doc
			// Contact3 is BLANK so will receive doc
			// Contact4 is IMPORT AIR so will not receive doc
			// Contact5 is IMPORT SEA so will not receive doc
			// Contact6 is BLANK AIR with local port AUSYD so will not receive doc
			// Contact7 is DOMESTIC so will receive doc
			// Contact8 is CROSSTRADE so will not receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact3Details, contact7Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cnrMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new DummyAutoDeliveryBizO(org, "AIR", "", "AUBNE", "USLAX", false, Directions.CrossTrade);
			// Exptected Contacts:
			// Contact1 is IMPORT so will not receive doc
			// Contact2 is EXPORT so will not receive doc
			// Contact3 is BLANK so will receive doc
			// Contact4 is IMPORT AIR so will not receive doc
			// Contact5 is IMPORT SEA so will not receive doc
			// Contact6 is BLANK AIR with local port AUSYD so will not receive doc
			// Contact7 is DOMESTIC so will not receive doc
			// Contact8 is CROSSTRADE so will receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact3Details, contact8Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cnrMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnLocalPort()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_FilterLocalPort = "AUSYD";

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_FilterLocalPort = "AU";

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_FilterLocalPort = "AUBNE";

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact4.Documents[0].OD_FilterShipmentMode = "SEA";

			OrgContact contact5 = CreateNewContactInOrganisation(org, contact5Details);
			contact5.Documents.AddNew();
			contact5.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			#endregion

			var deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX");

			// Exptected Contacts:
			// Contact1 has local port AUSYD so will receive doc
			// Contact2 has local port AU so will receive doc
			// Contact3 has local port AUBNE so will not receive doc
			// Contact4 has local port AUSYD, but shipment mode SEA so will not receive doc
			// Contact5 does not have local port so will receive doc
			var expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact5Details });

			var deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "", "USLAX");
			// Exptected Contacts:
			// Contact1 has local port AUSYD and the filtering condition for LocalPort is empty so will receive doc
			// Contact2 has local port AU and the filtering condition for LocalPort so will receive doc
			// Contact3 has local port AUBNE and the filtering condition for LocalPort so will receive doc
			// Contact4 has local port AUSYD and the filtering condition for LocalPort, but shipment mode SEA so will not receive doc
			// Contact5 does not have local port and the filtering condition for LocalPort, so will receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact3Details, contact5Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 4, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnForeignPort()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_FilterForeignPort = "USLAX";

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_FilterForeignPort = "US";

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_FilterForeignPort = "USSFO";

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[0].OD_FilterForeignPort = "USLAX";
			contact4.Documents[0].OD_FilterShipmentMode = "SEA";

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX");

			// Exptected Contacts:
			// Contact1 has foreign port USLAX so will receive doc
			// Contact2 has foreign port US so will receive doc
			// Contact3 has foreign port USSFO so will not receive doc
			// Contact4 has foreign port USLAX, but shipment mode SEA so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnLocalAndForeignPort()
		{
			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);
			DocDeliveryContact contact6Details = CreateNewDocDeliveryContact("Contact6", org);
			DocDeliveryContact contact7Details = CreateNewDocDeliveryContact("Contact7", org);
			DocDeliveryContact contact8Details = CreateNewDocDeliveryContact("Contact7", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact1.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact1.Documents[0].OD_FilterForeignPort = "USLAX";

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact2.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact2.Documents[0].OD_FilterForeignPort = "US";

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact3.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact3.Documents[0].OD_FilterForeignPort = "USSFO";

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact4.Documents[0].OD_FilterLocalPort = "AU";
			contact4.Documents[0].OD_FilterForeignPort = "USLAX";

			OrgContact contact5 = CreateNewContactInOrganisation(org, contact5Details);
			contact5.Documents.AddNew();
			contact5.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact5.Documents[0].OD_FilterLocalPort = "AU";
			contact5.Documents[0].OD_FilterForeignPort = "USSFO";

			OrgContact contact6 = CreateNewContactInOrganisation(org, contact6Details);
			contact6.Documents.AddNew();
			contact6.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact6.Documents[0].OD_FilterLocalPort = "AUBNE";
			contact6.Documents[0].OD_FilterForeignPort = "USSFO";

			OrgContact contact7 = CreateNewContactInOrganisation(org, contact7Details);
			contact7.Documents.AddNew();
			contact7.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact7.Documents[0].OD_FilterLocalPort = "AU";
			contact7.Documents[0].OD_FilterForeignPort = "GB";

			OrgContact contact8 = CreateNewContactInOrganisation(org, contact7Details);
			contact8.Documents.AddNew();
			contact8.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			contact8.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact8.Documents[0].OD_FilterForeignPort = "USLAX";
			contact8.Documents[0].OD_FilterShipmentMode = "SEA";

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX");

			// Exptected Contacts:
			// Contact1 has local port AUSYD and foreign port USLAX so will receive doc
			// Contact2 has local port AUSYD and foreign port US so will receive doc
			// Contact3 has local port AUSYD and foreign port USSFO so will not receive doc
			// Contact4 has local port AU and foreign port USLAX, so will receive doc
			// Contact5 has local port AU and foreign port USSFO, so will not receive doc
			// Contact6 has local port AUBNE and foreign port USSFO, so will not receive doc
			// Contact7 has local port AU and foreign port GB, so will not receive doc
			// Contact8 has local port AUSYD and foreign port USLAX, but shipment mode SEA so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact4Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnRelatedParty()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			OrgHeader relatedOrg = Factory.New<OrgHeader>();

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;
			contact2.Documents[0].OD_FilterForeignPort = "GBLON";

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;
			contact3.Documents.AddNew();
			contact3.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);
			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;
			contact4.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			contact4.Documents.AddNew();
			contact4.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", relatedOrg);

			// Exptected Contacts:
			// Contact1 has related party so will receive doc
			// Contact2 has related party and foreign port GBLON so will not receive doc
			// Contact3 has doc group suppressed but also has related party so will receive doc
			// Contact4 has doc group but also has related party suppressed so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact3Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnShipmentMode()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_FilterShipmentMode = "ALL";

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_FilterShipmentMode = "AIR";

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_FilterShipmentMode = "SEA";

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "SEA", "FCL", "AUSYD", "USLAX", true);
			// Exptected Contacts:
			// Contact1 is ALL so will receive doc
			// Contact2 is AIR so will not receive doc
			// Contact3 is SEA so will receive doc
			// Contact4 is FCL so will receive doc
			// Contact5 is LCL so will not receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact3Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new AutoDeliveryBizO(org, "SEA", "LCL", "AUSYD", "USLAX", false);
			// Exptected Contacts:
			// Contact1 is ALL so will receive doc
			// Contact2 is AIR so will not receive doc
			// Contact3 is SEA so will receive doc
			// Contact4 is FCL so will not receive doc
			// Contact5 is LCL so will receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact3Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));

			deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", false);
			// Exptected Contacts:
			// Contact1 is ALL so will receive doc
			// Contact2 is AIR so will receive doc
			// Contact3 is SEA so will not receive doc
			// Contact4 is FCL so will not receive doc
			// Contact5 is LCL so will not receive doc
			expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact1Details, contact2Details });

			deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestFilterOnTransportMode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.RemoveAndDeleteAll();

			var seaContactDetails = CreateNewDocDeliveryContact("SeaContact", org);
			var seaContact = CreateNewContactInOrganisation(org, seaContactDetails);
			var seaContactDocument = seaContact.Documents.AddNew();
			seaContactDocument.OD_DocumentGroup = ContactType.Consignor.Code;
			seaContactDocument.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			seaContactDocument.OD_FilterShipmentMode = Constants.TransportModes.Sea;

			var airContactDetails = CreateNewDocDeliveryContact("AirContact", org);
			var airContact = CreateNewContactInOrganisation(org, airContactDetails);
			var airContactDocument = airContact.Documents.AddNew();
			airContactDocument.OD_DocumentGroup = ContactType.Consignor.Code;
			airContactDocument.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			airContactDocument.OD_FilterShipmentMode = Constants.TransportModes.Air;

			var allContactDetails = CreateNewDocDeliveryContact("AllContact", org);
			var allContact = CreateNewContactInOrganisation(org, allContactDetails);
			var allContactDocument = allContact.Documents.AddNew();
			allContactDocument.OD_DocumentGroup = ContactType.Consignor.Code;
			allContactDocument.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			allContactDocument.OD_FilterShipmentMode = Constants.TransportModes.All;

			var consignorMenu = CreateNewMenuItem(ContactType.Consignor);

			void Assert(string mode, ZString[] expectedOrgContacts, string message)
			{
				var supporter = new OrgHeaderDocumentSupporter(org);
				supporter.transportModeExposed = mode;

				var actualOrgContacts = AutoDelivery
					.GetDeliveryContacts(consignorMenu, supporter)
					.Cast<DocDeliveryContact>()
					.Select(dc => dc.Contact.Name)
					.ToArray();

				AssertContainsExactElementsInAnyOrder(message, expectedOrgContacts, actualOrgContacts);
			}

			var expected = new ZString[] { "AirContact", "AllContact" };
			Assert(Constants.TransportModes.Air, expected, "Should contain only the AIR and ALL mode");

			expected = new ZString[] { "SeaContact", "AllContact" };
			Assert(Constants.TransportModes.Sea, expected, "Should contain only the SEA and ALL mode");

			expected = new ZString[] { "AllContact" };
			Assert(Constants.TransportModes.All, expected, "Should only include the ALL mode contact");

			expected = new ZString[] { "AirContact", "SeaContact", "AllContact" };
			Assert("", expected, "Should contain only the AIR and ALL mode as we don't filter if there is no mode.");
		}

		public void TestGetLocalAndForeignCountryWithNullPort()
		{
			var docDelivery = new DocAutoDelivery();
			docDelivery.SetLocalAndForeignPortNullForTesting();

			AssertEquals(string.Empty, docDelivery.GetLocalCountryForTesting());
			AssertEquals(string.Empty, docDelivery.GetForeignCountryForTesting());
		}

		public void TestFilterOnRelatedBranch()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Org1", "USBTV");
			OrgHeader org = CreateNewCompany("Org1", addressForOrg);

			#region Setup contacts for test

			var newBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch1.GB_Code = "GB1";
			var newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_Code = "GB2";

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_GB_FilterBranch = newBranch1.PK;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_GB_FilterBranch = newBranch2.PK;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_GB_FilterBranch = ZGuid.Empty;

			#endregion

			AutoDeliveryBizO deliveryFilter1 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "GB1", "", "");
			AutoDeliveryBizO deliveryFilter2 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "GB2", "", "");
			AutoDeliveryBizO deliveryFilter3 = new AutoDeliveryBizO(org);

			DocDeliveryContactCollection expectedContacts1 = new DocDeliveryContactCollection(Factory);
			expectedContacts1.AddRange(new BusinessObject[] { contact1Details, contact3Details });
			DocDeliveryContactCollection expectedContacts2 = new DocDeliveryContactCollection(Factory);
			expectedContacts2.AddRange(new BusinessObject[] { contact2Details, contact3Details });
			DocDeliveryContactCollection expectedContacts3 = new DocDeliveryContactCollection(Factory);
			expectedContacts3.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact3Details });

			DocDeliveryContactCollection deliveryContacts1 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter1);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts1.Count);
			Assert(ContactCollectionsMatch(expectedContacts1, deliveryContacts1));

			DocDeliveryContactCollection deliveryContacts2 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter2);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts2.Count);
			Assert(ContactCollectionsMatch(expectedContacts2, deliveryContacts2));

			DocDeliveryContactCollection deliveryContacts3 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter3);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts3.Count);
			Assert(ContactCollectionsMatch(expectedContacts3, deliveryContacts3));
		}

		public void TestFilterOnRelatedCompany()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			var newCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			newCompany1.GC_Code = "GC1";
			var newCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			newCompany2.GC_Code = "GC2";

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_GC_FilterCompany = newCompany1.PK;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_GC_FilterCompany = newCompany2.PK;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_GC_FilterCompany = ZGuid.Empty;

			#endregion

			AutoDeliveryBizO deliveryFilter1 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "", "GC1", "");
			AutoDeliveryBizO deliveryFilter2 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "", "GC2", "");
			AutoDeliveryBizO deliveryFilter3 = new AutoDeliveryBizO(org);

			DocDeliveryContactCollection expectedContacts1 = new DocDeliveryContactCollection(Factory);
			expectedContacts1.AddRange(new BusinessObject[] { contact1Details, contact3Details });
			DocDeliveryContactCollection expectedContacts2 = new DocDeliveryContactCollection(Factory);
			expectedContacts2.AddRange(new BusinessObject[] { contact2Details, contact3Details });
			DocDeliveryContactCollection expectedContacts3 = new DocDeliveryContactCollection(Factory);
			expectedContacts3.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact3Details });

			DocDeliveryContactCollection deliveryContacts1 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter1);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts1.Count);
			Assert(ContactCollectionsMatch(expectedContacts1, deliveryContacts1));

			DocDeliveryContactCollection deliveryContacts2 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter2);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts2.Count);
			Assert(ContactCollectionsMatch(expectedContacts2, deliveryContacts2));

			DocDeliveryContactCollection deliveryContacts3 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter3);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts3.Count);
			Assert(ContactCollectionsMatch(expectedContacts3, deliveryContacts3));
		}

		public void TestFilterOnRelatedDepartment()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			#region Setup contacts for test

			var newDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			newDepartment1.GE_Code = "GE1";
			var newDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			newDepartment2.GE_Code = "GE2";

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[0].OD_GE_FilterDepartment = newDepartment1.PK;

			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[0].OD_GE_FilterDepartment = newDepartment2.PK;

			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact3.Documents[0].OD_GE_FilterDepartment = ZGuid.Empty;

			#endregion

			AutoDeliveryBizO deliveryFilter1 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "", "", "GE1");
			AutoDeliveryBizO deliveryFilter2 = new AutoDeliveryBizO(org, "", "", "", "", false, null, null, "", "", "GE2");
			AutoDeliveryBizO deliveryFilter3 = new AutoDeliveryBizO(org);

			DocDeliveryContactCollection expectedContacts1 = new DocDeliveryContactCollection(Factory);
			expectedContacts1.AddRange(new BusinessObject[] { contact1Details, contact3Details });
			DocDeliveryContactCollection expectedContacts2 = new DocDeliveryContactCollection(Factory);
			expectedContacts2.AddRange(new BusinessObject[] { contact2Details, contact3Details });
			DocDeliveryContactCollection expectedContacts3 = new DocDeliveryContactCollection(Factory);
			expectedContacts3.AddRange(new BusinessObject[] { contact1Details, contact2Details, contact3Details });

			DocDeliveryContactCollection deliveryContacts1 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter1);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts1.Count);
			Assert(ContactCollectionsMatch(expectedContacts1, deliveryContacts1));

			DocDeliveryContactCollection deliveryContacts2 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter2);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts2.Count);
			Assert(ContactCollectionsMatch(expectedContacts2, deliveryContacts2));

			DocDeliveryContactCollection deliveryContacts3 = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter3);
			AssertEquals("DeliveryContacts.Count", 3, deliveryContacts3.Count);
			Assert(ContactCollectionsMatch(expectedContacts3, deliveryContacts3));
		}

		class DummyAutoDeliveryBizO : AutoDeliveryBizO
		{
			public DummyAutoDeliveryBizO(DummyOrgHeader organisation, string transportMode, string containerMode, string localPort,
				string foreignPort, bool isImport, Directions jobDirection)
				: base(organisation, transportMode, containerMode, localPort, foreignPort, isImport)
			{
				organisation.JobDirection = jobDirection;
			}
		}

		public class DummyOrgHeader : OrgHeader, IImportExport
		{
			public DummyOrgHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Directions JobDirection { get; set; }
		}

		DummyOrgHeader CreateNewDummyCompany(ZString name, DeliveryAddress addressDetails)
		{
			var org = Factory.New<DummyOrgHeader>();
			org.OH_FullName = name;
			AddAddressDetailsToOrg(org, addressDetails);
			Factory.Save();

			return org;
		}

		#endregion

		#region Import docs to Importer and/or Broker

		[ExpectNoExceptions]
		public void TestSendDocsToBothWhenBrokerIsNull()
		{
			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			AssertNull("No Consignee Air Customs Broker", org.DeliveryAirCustomsBroker);

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
		}

		public void TestSendImportDocToImportBroker()
		{
			DeliveryAddress brokerAddress = CreateNewDeliveryAddress("Broker", "USNYC");
			DeliveryAddress brokerAddressCHI = CreateNewDeliveryAddress("BrokerCHI", "USCHI");
			OrgHeader broker = CreateNewCompany("Broker", brokerAddress);
			OrgHeader brokerCHI = CreateNewCompany("CHIBroker", brokerAddressCHI);

			DocDeliveryContact brokerContactDetails = CreateNewDocDeliveryContact("BrokerContact", broker);
			OrgContact brokerContact = CreateNewContactInOrganisation(broker, brokerContactDetails);
			brokerContact.Documents.AddNew();
			brokerContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DocDeliveryContact brokerContactDetailsCHI = CreateNewDocDeliveryContact("BrokerContactCHI", brokerCHI);
			OrgContact brokerContactCHI = CreateNewContactInOrganisation(brokerCHI, brokerContactDetailsCHI);
			brokerContactCHI.Documents.AddNew();
			brokerContactCHI.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			org.SetRelatedParty(broker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			org.SetRelatedParty(brokerCHI, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty, "USCHI");

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(brokerContactDetails, deliveryContacts));

			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR", "", "USCHI", "GBLHR"));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("CHI contact is in list", ContactIsInList(brokerContactDetailsCHI, deliveryContacts));
		}

		public void TestSendImportDocToImporter()
		{
			DeliveryAddress brokerAddress = CreateNewDeliveryAddress("Broker", "USNYC");
			OrgHeader broker = CreateNewCompany("Broker", brokerAddress);

			DocDeliveryContact brokerContactDetails = CreateNewDocDeliveryContact("BrokerContact", broker);
			OrgContact brokerContact = CreateNewContactInOrganisation(broker, brokerContactDetails);
			brokerContact.Documents.AddNew();
			brokerContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Importer;
			org.SetRelatedParty(broker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(companyContactDetails, deliveryContacts));
		}

		public void TestSendImportDocToImporterAndBroker()
		{
			DeliveryAddress brokerAddress = CreateNewDeliveryAddress("Broker", "USNYC");
			OrgHeader broker = CreateNewCompany("Broker", brokerAddress);

			DocDeliveryContact brokerContactDetails = CreateNewDocDeliveryContact("BrokerContact", broker);
			OrgContact brokerContact = CreateNewContactInOrganisation(broker, brokerContactDetails);
			brokerContact.Documents.AddNew();
			brokerContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(broker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { brokerContactDetails, companyContactDetails });
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert("Contact is in list", ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestSendImportDocToBothWhenImporterAndBrokerSame()
		{
			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(org, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(companyContactDetails, deliveryContacts));
		}

		#region SupplierBuyer links

		public void TestGetContactFromBrokerWithOrgSupplierBuyerLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.All;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Air, "", "", "", consignor);

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(linkContactDetails, deliveryContacts));
		}

		public void TestGetFCLContactFromBrokerWithOrgSupplierBuyerLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.Sea;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Sea, "", "", "", consignor);

			StmMenuItem cneMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cneMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(linkContactDetails, deliveryContacts));
		}

		public void TestGetContactFromImporterWithOrgSupplierBuyerLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Importer;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.All;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Air, "", "", "", consignor);

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(companyContactDetails, deliveryContacts));
		}

		public void TestGetContactFromBothWithOrgSupplierBuyerLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);
			DocDeliveryContact mainBrokerContactDetails = CreateNewDocDeliveryContact("MainBroker", mainBroker);
			CreateNewContactInOrganisation(mainBroker, mainBrokerContactDetails);
			mainBroker.Contacts[0].Documents.AddNew();
			mainBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Both;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.All;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Air, "", "", "", consignor);
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { companyContactDetails, linkContactDetails });

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 2, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestGetContactFromBrokerWithOrgSupplierBuyerLinkButBrokerNotInLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);
			DocDeliveryContact mainBrokerContactDetails = CreateNewDocDeliveryContact("MainBroker", mainBroker);
			CreateNewContactInOrganisation(mainBroker, mainBrokerContactDetails);
			mainBroker.Contacts[0].Documents.AddNew();
			mainBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.All;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Air, "", "", "", consignor);
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { companyContactDetails, linkContactDetails });

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(mainBrokerContactDetails, deliveryContacts));
		}

		public void TestGetContactFromBrokerWithOrgSupplierBuyerLinkButSendDocsNotInLink()
		{
			#region Setup and save Orgs to DB

			DeliveryAddress consignorAddress = CreateNewDeliveryAddress("Consignor", "USLAX");
			OrgHeader consignor = CreateNewCompany("Consignor", consignorAddress);

			DeliveryAddress mainBrokerAddress = CreateNewDeliveryAddress("MainBroker", "USNYC");
			OrgHeader mainBroker = CreateNewCompany("MainBroker", mainBrokerAddress);
			DocDeliveryContact mainBrokerContactDetails = CreateNewDocDeliveryContact("MainBroker", mainBroker);
			CreateNewContactInOrganisation(mainBroker, mainBrokerContactDetails);
			mainBroker.Contacts[0].Documents.AddNew();
			mainBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			DeliveryAddress linkBrokerAddress = CreateNewDeliveryAddress("LinkBroker", "USNYC");
			OrgHeader linkBroker = CreateNewCompany("LinkBroker", linkBrokerAddress);
			DocDeliveryContact linkContactDetails = CreateNewDocDeliveryContact("LinkContact", linkBroker);
			CreateNewContactInOrganisation(linkBroker, linkContactDetails);
			linkBroker.Contacts[0].Documents.AddNew();
			linkBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			#endregion

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Broker;
			org.SetRelatedParty(mainBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			OrgSupplierBuyerLink link = org.SupplierLinks.AddNew();
			link.OL_OH_Supplier = consignor.PK;
			link.OL_OH_ImportBroker = linkBroker.PK;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = Constants.TransportModes.All;

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, Constants.TransportModes.Air, "", "", "", consignor);

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert("Contact is in list", ContactIsInList(linkContactDetails, deliveryContacts));
		}

		public void TestGetContactFromBrokerWithOrgSupplierBuyerLink_Comprehensive()
		{
			#region Setup and save Orgs to DB

			var fclBrokerAddress = CreateNewDeliveryAddress("FCLBROKER", "AUBNE");
			var fclBroker = CreateNewCompany("FCLBROKER", fclBrokerAddress);
			var fclBrokerContact = CreateNewDocDeliveryContact("FCLBROKER", fclBroker);
			CreateNewContactInOrganisation(fclBroker, fclBrokerContact);
			fclBroker.Contacts[0].Documents.AddNew();
			fclBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var lclBrokerAddress = CreateNewDeliveryAddress("LCLBROKER", "AUBNE");
			var lclBroker = CreateNewCompany("LCLBROKER", lclBrokerAddress);
			var lclBrokerContact = CreateNewDocDeliveryContact("LCLBROKER", lclBroker);
			CreateNewContactInOrganisation(lclBroker, lclBrokerContact);
			lclBroker.Contacts[0].Documents.AddNew();
			lclBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var seaBrokerAddress = CreateNewDeliveryAddress("SEABROKER", "AUBNE");
			var seaBroker = CreateNewCompany("SEABROKER", seaBrokerAddress);
			var seaBrokerContact = CreateNewDocDeliveryContact("SEABROKER", seaBroker);
			CreateNewContactInOrganisation(seaBroker, seaBrokerContact);
			seaBroker.Contacts[0].Documents.AddNew();
			seaBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var airBrokerAddress = CreateNewDeliveryAddress("AIRBROKER", "AUBNE");
			var airBroker = CreateNewCompany("AIRBROKER", airBrokerAddress);
			var airBrokerContact = CreateNewDocDeliveryContact("AIRBROKER", airBroker);
			CreateNewContactInOrganisation(airBroker, airBrokerContact);
			airBroker.Contacts[0].Documents.AddNew();
			airBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var consignorAddress = CreateNewDeliveryAddress("Consignor", "AUSYD");
			var consignor = CreateNewCompany("Consignor", consignorAddress);

			var consigneeAddress = CreateNewDeliveryAddress("Consignee", "AUBNE");
			var consignee = CreateNewCompany("Consignee", consigneeAddress);
			var consigneeContact = CreateNewDocDeliveryContact("CONSIGNEE", consignee);
			CreateNewContactInOrganisation(consignee, consigneeContact);
			consignee.Contacts[0].Documents.AddNew();
			consignee.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			consignee.OH_IsConsignee = true;
			consignee.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			consignee.SetRelatedParty(airBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Air, ZString.Empty);
			consignee.SetRelatedParty(seaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, ZString.Empty);
			consignee.SetRelatedParty(fclBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(lclBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			Factory.Save();

			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.Air);
			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.Sea);
			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);

			#endregion

			var cneMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			var deliveryFilter = new AutoDeliveryBizO(consignee, "SEA", "", "", "", consignor);
			var deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("Mode = SEA, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("Mode = SEA, should have 'SEA' Broker contact", ContactIsInList(seaBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);

			deliveryFilter = new AutoDeliveryBizO(consignee, "AIR", "", "", "", consignor);
			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("Mode = AIR, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("Mode = AIR, should have 'AIR' Broker contact", ContactIsInList(airBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);

			deliveryFilter = new AutoDeliveryBizO(consignee, "SEA", "FCL", "", "", consignor);
			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("TransportMode = SEA, ContainerMode = FCL, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("TransportMode = SEA, ContainerMode = FCL, should have 'FCL' Broker contact", ContactIsInList(fclBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);

			deliveryFilter = new AutoDeliveryBizO(consignee, "SEA", "LCL", "", "", consignor);
			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("TransportMode = SEA, ContainerMode = LCL, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("TransportMode = SEA, ContainerMode = LCL, should have 'LCL' Broker contact", ContactIsInList(lclBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);
		}

		public void TestGetContactFromBrokerWithOrgSupplierBuyerLink_FreightModeIsALL()
		{
			#region Setup and save Orgs to DB

			var fclBrokerAddress = CreateNewDeliveryAddress("FCLBROKER", "AUBNE");
			var fclBroker = CreateNewCompany("FCLBROKER", fclBrokerAddress);
			var fclBrokerContact = CreateNewDocDeliveryContact("FCLBROKER", fclBroker);
			CreateNewContactInOrganisation(fclBroker, fclBrokerContact);
			fclBroker.Contacts[0].Documents.AddNew();
			fclBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var allBrokerAddress = CreateNewDeliveryAddress("ALLBROKER", "AUBNE");
			var allBroker = CreateNewCompany("ALLBROKER", allBrokerAddress);
			var allBrokerContact = CreateNewDocDeliveryContact("ALLBROKER", allBroker);
			CreateNewContactInOrganisation(allBroker, allBrokerContact);
			allBroker.Contacts[0].Documents.AddNew();
			allBroker.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var consignorAddress = CreateNewDeliveryAddress("Consignor", "AUSYD");
			var consignor = CreateNewCompany("Consignor", consignorAddress);

			var consigneeAddress = CreateNewDeliveryAddress("Consignee", "AUBNE");
			var consignee = CreateNewCompany("Consignee", consigneeAddress);
			var consigneeContact = CreateNewDocDeliveryContact("CONSIGNEE", consignee);
			CreateNewContactInOrganisation(consignee, consigneeContact);
			consignee.Contacts[0].Documents.AddNew();
			consignee.Contacts[0].Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			consignee.OH_IsConsignee = true;
			consignee.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			consignee.SetRelatedParty(allBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.All, ZString.Empty);
			consignee.SetRelatedParty(fclBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);

			Factory.Save();

			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			CreateNewSupplierLinks(consignee, consignor, Constants.TransportModes.All);

			#endregion

			var cneMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			var deliveryFilter = new AutoDeliveryBizO(consignee, "AIR", "", "", "", consignor);
			var deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("TransportMode = AIR, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("TransportMode = AIR, should have 'ALL' Broker contact", ContactIsInList(allBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);

			deliveryFilter = new AutoDeliveryBizO(consignee, "", "", "", "", consignor);
			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("TransportMode = blank, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("TransportMode = blank, should have 'ALL' Broker contact", ContactIsInList(allBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);

			deliveryFilter = new AutoDeliveryBizO(consignee, "SEA", "FCL", "", "", consignor);
			deliveryContacts = new DocAutoDelivery().GetDeliveryContacts(cneMenuItem, deliveryFilter);
			Assert("TransportMode = SEA, ContainerMode = FCL, should have Consignee contact", ContactIsInList(consigneeContact, deliveryContacts));
			Assert("TransportMode = SEA, ContainerMode = FCL, should have 'FCL' Broker contact", ContactIsInList(fclBrokerContact, deliveryContacts));
			AssertEquals("Delivery Contacts count", 2, deliveryContacts.Count);
		}

		#endregion

		#endregion

		#region Fallback to correct filter for same contact

		public void TestFallbackToCorrectFilterForSameContact()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			OrgHeader relatedOrg = Factory.New<OrgHeader>();

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);
			DocDeliveryContact contact2Details = CreateNewDocDeliveryContact("Contact2", org);
			DocDeliveryContact contact3Details = CreateNewDocDeliveryContact("Contact3", org);
			DocDeliveryContact contact4Details = CreateNewDocDeliveryContact("Contact4", org);
			DocDeliveryContact contact5Details = CreateNewDocDeliveryContact("Contact5", org);
			DocDeliveryContact contact6Details = CreateNewDocDeliveryContact("Contact6", org);
			DocDeliveryContact contact7Details = CreateNewDocDeliveryContact("Contact7", org);

			// Contact 1
			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);

			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.All.Code;

			contact1.Documents.AddNew();
			contact1.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;
			contact1.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			// Contact 2
			OrgContact contact2 = CreateNewContactInOrganisation(org, contact2Details);

			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.All.Code;

			contact2.Documents.AddNew();
			contact2.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;
			contact2.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			contact2.Documents.AddNew();
			contact2.Documents[2].OD_SU_MenuItem = cNRMenuItem.PK;

			// Contact 3
			OrgContact contact3 = CreateNewContactInOrganisation(org, contact3Details);

			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			contact2.Documents.AddNew();
			contact2.Documents[1].OD_SU_MenuItem = cNRMenuItem.PK;
			contact2.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			// Contact 4
			OrgContact contact4 = CreateNewContactInOrganisation(org, contact4Details);

			contact4.Documents.AddNew();
			contact4.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;
			contact4.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			contact4.Documents.AddNew();
			contact4.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			contact4.Documents.AddNew();
			contact4.Documents[2].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[2].OD_FilterLocalPort = "AUSYD";
			contact4.Documents[2].OD_FilterForeignPort = "USLAX";

			contact4.Documents.AddNew();
			contact4.Documents[3].OD_DocumentGroup = ContactType.Consignor.Code;
			contact4.Documents[3].OD_FilterLocalPort = "AUSYD";
			contact4.Documents[3].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			// Contact 5
			OrgContact contact5 = CreateNewContactInOrganisation(org, contact5Details);

			contact5.Documents.AddNew();
			contact5.Documents[0].OD_SU_MenuItem = cNRMenuItem.PK;

			contact5.Documents.AddNew();
			contact5.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			contact5.Documents.AddNew();
			contact5.Documents[2].OD_DocumentGroup = ContactType.Consignor.Code;
			contact5.Documents[2].OD_FilterLocalPort = "AUSYD";
			contact5.Documents[2].OD_FilterForeignPort = "USLAX";
			contact5.Documents[2].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			// Contact 6
			OrgContact contact6 = CreateNewContactInOrganisation(org, contact6Details);

			contact6.Documents.AddNew();
			contact6.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact6.Documents[0].OD_FilterLocalPort = "AUSYD";
			contact6.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			contact6.Documents.AddNew();
			contact6.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			contact6.Documents.AddNew();
			contact6.Documents[2].OD_DocumentGroup = ContactType.Consignor.Code;
			contact6.Documents[2].OD_FilterLocalPort = "AUSYD";
			contact6.Documents[2].OD_FilterForeignPort = "USLAX";

			contact6.Documents.AddNew();

			// Contact 7
			OrgContact contact7 = CreateNewContactInOrganisation(org, contact7Details);

			contact7.Documents.AddNew();
			contact7.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;
			contact7.Documents[0].OD_OH_RelatedFilterByParty = relatedOrg.PK;
			contact7.Documents[0].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			contact7.Documents.AddNew();
			contact7.Documents[1].OD_SU_MenuItem = cNRMenuItem.PK;
			contact7.Documents[1].OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;

			contact7.Documents.AddNew();
			contact7.Documents[2].OD_SU_MenuItem = cNRMenuItem.PK;
			contact7.Documents[2].OD_FilterLocalPort = "AUSYD";
			contact7.Documents[2].OD_FilterForeignPort = "USLAX";

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", relatedOrg);

			// Exptected Contacts:
			// Contact1 has ALL doc group and CNR doc group suppressed so will not receive doc
			// Contact2 has ALL doc group, CNR doc group suppressed and specific doc so will receive doc
			// Contact3 has CNR doc group and specific doc suppressed so will not receive doc
			// Contact4 has most specific filter as related party with doc suppressed so will not receive doc
			// Contact5 has doc group suppressed with ports filter, but also has specific doc, so will receive doc
			// Contact6 has local port filter suppressed, but also has filter with both ports so will receive doc
			// Contact7 has doc suppressed for doc group CNR with related org and also for specific doc, but has specific doc for ports, so will receive doc
			DocDeliveryContactCollection expectedContacts = new DocDeliveryContactCollection(Factory);
			expectedContacts.AddRange(new BusinessObject[] { contact2Details, contact5Details, contact6Details, contact7Details });

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("DeliveryContacts.Count", 4, deliveryContacts.Count);
			Assert(ContactCollectionsMatch(expectedContacts, deliveryContacts));
		}

		public void TestAddSameFilterToHashtabhe()
		{
			StmMenuItem cNRMenuItem = CreateNewMenuItem(ContactType.Consignor);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			OrgHeader relatedOrg = Factory.New<OrgHeader>();

			#region Setup contacts for test

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);

			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignor.Code;

			contact1.Documents.AddNew();
			contact1.Documents[1].OD_DocumentGroup = ContactType.Consignor.Code;

			#endregion

			AutoDeliveryBizO deliveryFilter = new AutoDeliveryBizO(org, "AIR", "", "AUSYD", "USLAX", relatedOrg);

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNRMenuItem, deliveryFilter);
			AssertEquals("Contact is setup to receive the same doc twice, so two contacts should appear", 2, deliveryContacts.Count);
			Assert(ContactIsInList(contact1Details, deliveryContacts));
		}

		#endregion

		#region Suppress document for organisation

		public void TestSuppressDocumentForOrganisation()
		{
			StmMenuItem wHSMenuItem = CreateNewMenuItem(ContactType.Warehouse);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contact1Details = CreateNewDocDeliveryContact("Contact1", org);

			OrgContact contact1 = CreateNewContactInOrganisation(org, contact1Details);
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.Warehouse.Code;

			var document = org.SuppressedDocuments.AddNew();
			document.OD_DocumentGroup = ContactType.Warehouse.Code;

			// Org has doc suppressed so no contacts for delivery
			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(wHSMenuItem, new AutoDeliveryBizO(org));
			AssertEquals("DeliveryContacts.Count", 0, deliveryContacts.Count);
		}

		public void TestSuppressDocumentForBrokerOrganisation()
		{
			DeliveryAddress brokerAddress = CreateNewDeliveryAddress("Broker", "USNYC");
			OrgHeader broker = CreateNewCompany("Broker", brokerAddress);

			var document = broker.SuppressedDocuments.AddNew();
			document.OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			StmMenuItem cNEMenuItem = CreateNewMenuItem(ContactType.Consignee.Code);

			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendImportDocsTo = OrgConstants.SendDocsTo.Both;
			org.SetRelatedParty(broker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);

			DocDeliveryContact companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			OrgContact companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			// Broker has doc suppressed, so only deliver to consignee
			DocDeliveryContact expectedContact = companyContactDetails;

			DocDeliveryContactCollection deliveryContacts = AutoDelivery.GetDeliveryContacts(cNEMenuItem, new AutoDeliveryBizO(org, "AIR"));
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			Assert(ContactIsInList(expectedContact, deliveryContacts));
		}

		#endregion

		#region GetDeliveryDetailsForContact

		public void TestGettingContactsAsContactCollection()
		{
			StmMenuItem doc = CreateNewMenuItem(ContactType.CustomerService.Code);
			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contactDetails = CreateNewDocDeliveryContact("NewContact", org);
			OrgContact contact = CreateNewContactInOrganisation(org, contactDetails);
			contact.Documents.AddNew();
			contact.Documents[0].OD_SU_MenuItem = doc.PK;

			DocDeliveryContact contactDetails2 = CreateNewDocDeliveryContact("NewContact2", org);
			OrgContact contact2 = CreateNewContactInOrganisation(org, contactDetails2);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_SU_MenuItem = doc.PK;

			DocDeliveryContact contactDetails3 = CreateNewDocDeliveryContact("NewContact3", org);
			OrgContact contact3 = CreateNewContactInOrganisation(org, contactDetails3);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_SU_MenuItem = doc.PK;

			Factory.Save();

			DocDeliveryContactCollection returnedDetails = AutoDelivery.GetDeliveryContacts(doc, new AutoDeliveryBizO(org));
			AssertEquals("Collection has 3 contacts", 3, returnedDetails.Count);
		}

		public void TestNewGetDeliveryDetailsForContact()
		{
			StmMenuItem doc = CreateNewMenuItem(ContactType.CustomerService.Code);
			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contactDetails = CreateNewDocDeliveryContact("NewContact", org);
			OrgContact contact = CreateNewContactInOrganisation(org, contactDetails);
			contact.Documents.AddNew();
			contact.Documents[0].OD_SU_MenuItem = doc.PK;

			Factory.Save();

			DocDeliveryContact returnedDetails = AutoDelivery.GetDeliveryDetailsForContact(contact);
			AssertContactDetailsAreCorrect(contactDetails, returnedDetails);
		}

		#endregion

		#region GetDeliveryContacts

		public void TestGetDeliveryContactsWithBrokerOrganisation_LCL()
		{
			AssertGetDeliveryContactsWithBrokerOrganisation(Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
		}

		public void TestGetDeliveryContactsWithBrokerOrganisation_FCL()
		{
			AssertGetDeliveryContactsWithBrokerOrganisation(Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
		}

		public void AssertGetDeliveryContactsWithBrokerOrganisation(string transportMode, string containerMode)
		{
			var brokerAddress = CreateNewDeliveryAddress("Broker", "USNYC");
			var broker = CreateNewCompany("Broker", brokerAddress);
			var brokerContactDetails = CreateNewDocDeliveryContact("BrokerContact", broker);
			var brokerContact = CreateNewContactInOrganisation(broker, brokerContactDetails);
			brokerContact.Documents.AddNew();
			brokerContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var seaBrokerAddress = CreateNewDeliveryAddress("Sea Broker", "USCHI");
			var seaBroker = CreateNewCompany("Sea Broker", seaBrokerAddress);
			var seaBrokerContactDetails = CreateNewDocDeliveryContact("SeaBrokerContact", seaBroker);
			var seaBrokerContact = CreateNewContactInOrganisation(seaBroker, seaBrokerContactDetails);
			seaBrokerContact.Documents.AddNew();
			seaBrokerContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			Factory.Save();

			var menuItem = CreateNewMenuItem(ContactType.Consignee.Code);
			var addressForOrg = CreateNewDeliveryAddress("Company", "USBTV");
			var org = CreateNewCompany("Company", addressForOrg);
			org.OH_IsConsignee = true;
			org.MiscServ.OM_IMSendSeaImportDocsTo = OrgConstants.SendDocsTo.Broker;
			org.SetRelatedParty(broker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, transportMode, containerMode);

			var companyContactDetails = CreateNewDocDeliveryContact("CompanyContact", org);
			var companyContact = CreateNewContactInOrganisation(org, companyContactDetails);
			companyContact.Documents.AddNew();
			companyContact.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;

			var autoDelivery1 = new DocAutoDelivery();
			var deliveryContacts1 = autoDelivery1.GetDeliveryContacts(menuItem, new AutoDeliveryBizO(org, transportMode, containerMode));
			AssertEquals("Delivery contacts count should be 1.", 1, deliveryContacts1.Count);
			Assert("Delivery contacts should include Broker contact details.", ContactIsInList(brokerContactDetails, deliveryContacts1));

			org.AllRelatedParties.RemoveAll();
			AssertEquals(0, org.AllRelatedParties.Count);
			org.SetRelatedParty(seaBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, ZString.Empty);

			var autoDelivery2 = new DocAutoDelivery();
			var deliveryContacts2 = autoDelivery2.GetDeliveryContacts(menuItem, new AutoDeliveryBizO(org, transportMode, containerMode));
			AssertEquals("Delivery contacts count should be 1.", 1, deliveryContacts2.Count);
			Assert("Delivery contacts should include fallback Sea Broker contact details.", ContactIsInList(seaBrokerContactDetails, deliveryContacts2));
		}

		#endregion

		#region Fetch Hints
		public void TestDeliveryContactWithCopyRecipientFetchHints()
		{
			var doc = CreateNewMenuItem(ContactType.Consignee.Code);
			DeliveryAddress addressForOrg = CreateNewDeliveryAddress("Company1", "USBTV");
			OrgHeader org = CreateNewCompany("Company1", addressForOrg);

			DocDeliveryContact contactDetails = CreateNewDocDeliveryContact("NewContact", org);
			OrgContact contact = CreateNewContactInOrganisation(org, contactDetails);
			contact.Documents.AddNew();
			contact.Documents[0].OD_SU_MenuItem = doc.PK;

			DocDeliveryContact contactDetails2 = CreateNewDocDeliveryContact("NewContact2", org);
			OrgContact contact2 = CreateNewContactInOrganisation(org, contactDetails2);
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_SU_MenuItem = doc.PK;

			DocDeliveryContact contactDetails3 = CreateNewDocDeliveryContact("NewContact3", org);
			OrgContact contact3 = CreateNewContactInOrganisation(org, contactDetails3);
			contact3.Documents.AddNew();
			contact3.Documents[0].OD_SU_MenuItem = doc.PK;

			OrgDocumentCopyRecipient recipient1 = Factory.New<OrgDocumentCopyRecipient>();
			recipient1.ODR_OD = contact.Documents[0].PK;
			recipient1.ODR_RecipientType = Constants.CopyRecipientType.CarbonCopyRecipient;
			recipient1.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			OrgDocumentCopyRecipient recipient2 = Factory.New<OrgDocumentCopyRecipient>();
			recipient2.ODR_OD = contact.Documents[0].PK;
			recipient2.ODR_RecipientType = Constants.CopyRecipientType.BlindCarbonCopyRecipient;
			recipient2.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			OrgDocumentCopyRecipient recipient3 = Factory.New<OrgDocumentCopyRecipient>();
			recipient3.ODR_OD = contact2.Documents[0].PK;
			recipient3.ODR_RecipientType = Constants.CopyRecipientType.CarbonCopyRecipient;
			recipient3.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			OrgDocumentCopyRecipient recipient4 = Factory.New<OrgDocumentCopyRecipient>();
			recipient4.ODR_OD = contact2.Documents[0].PK;
			recipient4.ODR_RecipientType = Constants.CopyRecipientType.BlindCarbonCopyRecipient;
			recipient4.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			OrgDocumentCopyRecipient recipient5 = Factory.New<OrgDocumentCopyRecipient>();
			recipient5.ODR_OD = contact3.Documents[0].PK;
			recipient5.ODR_RecipientType = Constants.CopyRecipientType.CarbonCopyRecipient;
			recipient5.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			OrgDocumentCopyRecipient recipient6 = Factory.New<OrgDocumentCopyRecipient>();
			recipient6.ODR_OD = contact3.Documents[0].PK;
			recipient6.ODR_RecipientType = Constants.CopyRecipientType.BlindCarbonCopyRecipient;
			recipient6.ODR_EmailAddress = "ethan.zhou@wisetechglobal.com";

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var docInNewFactory = newFactory.Load<StmMenuItem>(doc.PK);
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgDocumentSchema.Constants.TableName, 4 },
				{ OrgDocumentCopyRecipientSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ StmMenuItemSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				AutoDelivery.GetDeliveryContacts(docInNewFactory, new AutoDeliveryBizO(orgInNewFactory));
			}
		}
		#endregion

		#region Implementation

		protected DocAutoDelivery AutoDelivery
		{
			get { return autoDelivery ?? (autoDelivery = new DocAutoDelivery()); }
		}

		DocAutoDelivery autoDelivery;

		#region Create new objects

		OrgSupplierBuyerLink CreateNewSupplierLinks(OrgHeader orgParent, OrgHeader supplier, string transportMode, string containerMode = "")
		{
			var link = orgParent.SupplierLinks.AddNew();
			link.OL_OH_Supplier = supplier.PK;
			link.OL_SendImportDocsTo = OrgConstants.SendDocsTo.Both;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = transportMode;
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = containerMode;
			return link;
		}

		OrgHeader CreateNewCompany(ZString name, DeliveryAddress addressDetails)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = name;
			AddAddressDetailsToOrg(org, addressDetails);
			Factory.Save();

			return org;
		}

		void AddAddressDetailsToOrg(OrgHeader org, DeliveryAddress addressDetails)
		{
			org.OH_RL_NKClosestPort = addressDetails.UNLOCO.RL_Code;
			org.MainAddress.OA_Address1 = addressDetails.Address1;
			org.MainAddress.OA_Address2 = addressDetails.Address2;
			org.MainAddress.OA_City = addressDetails.City;
			org.MainAddress.OA_State = addressDetails.State;
			org.MainAddress.OA_PostCode = addressDetails.PostCode;
			org.MainAddress.OA_Fax = addressDetails.Fax;
			org.MainAddress.OA_Email = addressDetails.Email;
			org.MainAddress.OA_Phone = addressDetails.Phone;
		}

		protected OrgAddress CreateNewAddressInOrganisation(OrgHeader org, DeliveryAddress addressDetails)
		{
			OrgAddress address = org.Addresses.AddNew();
			address.OA_Code = OrgConstants.AddressType.PickupAndDelivery;
			address.OA_Address1 = addressDetails.Address1;
			address.OA_Address2 = addressDetails.Address2;
			address.OA_City = addressDetails.City;
			address.OA_State = addressDetails.State;
			address.OA_PostCode = addressDetails.PostCode;
			return address;
		}

		OrgContact CreateNewContactInOrganisation(OrgHeader org, DocDeliveryContact contactDetails)
		{
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = contactDetails.Name;
			contact.OC_NotifyMode = contactDetails.DeliveryMethod;
			contact.OC_AttachmentType = contactDetails.AttachmentType;
			return contact;
		}

		OrgDocument AddDocumentToContact(OrgContact contact, ZGuid menuItem, ZString notifyMode, ZString attachmentType)
		{
			OrgDocument document = Factory.New<OrgDocument>();
			document.OD_SU_MenuItem = menuItem;
			document.OD_DeliverBy = notifyMode;
			document.OD_AttachmentType = attachmentType;
			contact.Documents.Add(document);
			return document;
		}

		protected OrgDocument AddDocumentToContactAndSuppress(OrgContact contact, ZGuid menuItem)
		{
			OrgDocument document = AddDocumentToContact(contact, menuItem, "EML", "PDF");
			document.OD_DeliverBy = Constants.ContactNotifyModes.DoNotDeliver;
			return document;
		}

		DocDeliveryContact CreateNewDocDeliveryContact(ZString contactName, OrgHeader org)
		{
			return CreateNewDocDeliveryContact(contactName, org, "EML", "PDF");
		}

		DocDeliveryContact CreateNewDocDeliveryContact(ZString contactName, OrgHeader org, ZString deliveryMethod, ZString attachmentType)
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.OrgHeaderPK = org.PK;
			contact.Name = contactName;
			contact.CompanyName = org.OH_FullNameTruncated;
			contact.DeliveryMethod = deliveryMethod;
			contact.AttachmentType = attachmentType;

			contact.Address1 = org.MainAddress.OA_Address1;
			contact.Address2 = org.MainAddress.OA_Address2;
			contact.City = org.MainAddress.OA_City;
			contact.State = org.MainAddress.OA_State;
			contact.PostCode = org.MainAddress.OA_PostCode;
			contact.Fax = org.MainAddress.OA_Fax;
			contact.Email = org.MainAddress.OA_Email;
			contact.Phone = org.MainAddress.OA_Phone;
			contact.UNLOCO = org.UNLOCO;

			return contact;
		}

		DeliveryAddress CreateNewDeliveryAddress(ZString companyName, ZString unlocoCode)
		{
			var result = new DeliveryAddress();

			result.Address1 = companyName + " Address1";
			result.Address2 = companyName + " Address2";
			result.City = companyName + " City";
			result.State = companyName + " State";
			result.PostCode = companyName;
			result.Fax = companyName + " Fax";
			result.Email = companyName + " Email";
			result.Phone = companyName + " Phone";
			result.UNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unlocoCode);

			return result;
		}

		StmMenuItem CreateNewMenuItem(ContactType type)
		{
			return CreateNewMenuItem(type.Code);
		}

		StmMenuItem CreateNewMenuItem(ZString contactType)
		{
			var result = Factory.New<StmMenuItem>();
			result.SU_ContactType = contactType;

			return result;
		}

		#endregion

		#region Assertions

		void AssertCorrectDeliveryMethod(DocDeliveryContactCollection deliveryContacts, ZString deliveryMethod)
		{
			AssertCorrectDeliveryMethod("AssertCorrectDeliveryMethod", deliveryContacts, deliveryMethod);
		}

		void AssertCorrectDeliveryMethod(ZString message, DocDeliveryContactCollection deliveryContacts, ZString deliveryMethod)
		{
			AssertEquals("DeliveryContacts.Count", 1, deliveryContacts.Count);
			DocDeliveryContact accManager = deliveryContacts[0];
			AssertEquals(message, deliveryMethod, accManager.DeliveryMethod);
		}

		ZBool ContactCollectionsMatch(DocDeliveryContactCollection expectedContacts, DocDeliveryContactCollection contactList)
		{
			bool result = expectedContacts.Count == contactList.Count;
			foreach (DocDeliveryContact contact in expectedContacts)
			{
				result &= ContactIsInList(contact, contactList);
			}
			return result;
		}

		ZBool ContactIsInList(DocDeliveryContact expectedContact, DocDeliveryContactCollection contactList)
		{
			foreach (DocDeliveryContact contact in contactList)
			{
				if (expectedContact.Name == contact.Name && expectedContact.CompanyName == contact.CompanyName)
				{
					AssertContactDetailsAreCorrect(expectedContact, contact);
					return true;
				}
			}
			return false;
		}

		void AssertContactDetailsAreCorrect(DocDeliveryContact contact1, DocDeliveryContact contact2)
		{
			AssertEquals("Contact Name", contact1.Name, contact2.Name);
			AssertEquals("Company Name", contact1.CompanyName, contact2.CompanyName);
			AssertEquals("Delivery Method", contact1.DeliveryMethod, contact2.DeliveryMethod);
			AssertEquals("Attachment Type", contact1.AttachmentType, contact2.AttachmentType);

			AssertEquals("Address1", contact1.Address1, contact2.Address1);
			AssertEquals("Address2", contact1.Address2, contact2.Address2);
			AssertEquals("City", contact1.City, contact2.City);
			AssertEquals("State", contact1.State, contact2.State);
			AssertEquals("PostCode", contact1.PostCode, contact2.PostCode);
			AssertEquals("Email", contact1.Email, contact2.Email);
			AssertEquals("Fax", contact1.Fax, contact2.Fax);
			AssertEquals("Phone", contact1.Phone, contact2.Phone);
			if (contact1.UNLOCO != null && contact2.UNLOCO != null)
			{
				AssertEquals("UNLOCO", contact1.UNLOCO.PK, contact2.UNLOCO.PK);
			}
			else
			{
				Assert("Both UNLOCOs should be null", contact1.UNLOCO == null && contact2.UNLOCO == null);
			}
		}

		#endregion

		#endregion

		#region Classes for Testing

		public class DeliveryAddress
		{
			public DeliveryAddress()
			{
				Address1 = "";
				Address2 = "";
				City = "";
				State = "";
				PostCode = "";
				Phone = "";
				Fax = "";
				Email = "";
			}

			public string Address1;
			public string Address2;
			public string City;
			public string State;
			public string PostCode;
			public string Phone;
			public string Fax;
			public string Email;
			public RefUNLOCO UNLOCO;
		}

		class OrgHeaderDocumentSupporter : DocumentSupporter
		{
			readonly OrgHeader orgHeader;

			internal OrgHeaderDocumentSupporter(OrgHeader orgHeader)
				: base(orgHeader)
			{
				this.orgHeader = orgHeader;
			}

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
			{
				return new OrgHeaderContact(orgHeader, null);
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return null; }
			}

			protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return null;
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				return null;
			}

			public override bool AdditionalExcludeFilter(IStmMenuItem commandAboutToBeRun, ZGuid orgDocumentPK)
			{
				if (commandAboutToBeRun.SU_MenuName == "Menu requiring specific document for recipient")
				{
					OrgDocument doc = Factory.Load<OrgDocument>(orgDocumentPK);

					if (doc == null || doc.OD_SU_MenuItem != commandAboutToBeRun.PK)
					{
						return true;
					}
				}

				return base.AdditionalExcludeFilter(commandAboutToBeRun, orgDocumentPK);
			}

			public override string TransportMode
			{
				get { return transportModeExposed; }
			}

			public string transportModeExposed = "";
		}

		#endregion
	}
}
