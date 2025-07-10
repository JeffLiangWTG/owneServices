using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DefaultContactFinderTest : TestCaseWithFactory
	{
		public void TestStandardDefault()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.CustomerService, false);
			OrgContact contact2 = CreateNewContact(ContactType.CustomerService, true);
			OrgContact contact3 = CreateNewContact(ContactType.All, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			AssertEquals("Default Contact", contact2.PK, new DefaultContactFinder(Organisation).DefaultContact(ContactType.CustomerService).PK);
		}

		public void TestDocumentFallbackToStandardDefault()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);

			OrgContact contact1 = CreateNewContact(ContactType.CustomerService, true);
			OrgContact contact2 = CreateNewContact(ContactType.CustomerService, false);
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			OrgDocument doc = contact2.Documents.AddNew();
			doc.OD_SU_MenuItem = menuItem.PK;
			doc.OD_DefaultContact = true;
			OrgContact contact3 = CreateNewContact(ContactType.All, true);

			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			AssertEquals("Default Contact for Menu Item", contact2.PK, new DefaultContactFinder(Organisation).DefaultContact(menuItem.PK.ToGuid(), ContactType.CustomerService).PK);
			AssertEquals("Default Contact", contact1.PK, new DefaultContactFinder(Organisation).DefaultContact(ContactType.CustomerService).PK);
		}

		public void TestFallbackToALL()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.CustomerService, false);
			OrgContact contact2 = CreateNewContact(ContactType.CustomerService, true);
			OrgContact contact3 = CreateNewContact(ContactType.All, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			AssertEquals("Default Contact", contact3.PK, new DefaultContactFinder(Organisation).DefaultContact(ContactType.Marketing).PK);
		}

		public void TestFallbackToSystemDefault()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.CustomerService, false);
			OrgContact contact2 = CreateNewContact(ContactType.CustomerService, true);
			OrgContact contact3 = CreateNewContact(ContactType.Miscellaneous, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Marketing);
			AssertEquals("Default Contact", ContactType.Marketing.DefaultName, defaultContact.OC_ContactName);

			defaultContact = new DefaultContactFinder(Organisation, false).DefaultContact(ContactType.Marketing);
			AssertNull("Default Contact not returned", defaultContact);
		}

		public void TestAggregateTypeFallBackReturnActiveContact()
		{
			var contact1 = CreateNewContact(ContactType.ExportSeaDepot, false);

			var contact2 = CreateNewContact(ContactType.ExportSeaDepot, true);
			contact2.OC_IsActive = false;

			var contact3 = CreateNewContact(ContactType.ExportSeaDepot, true);
			contact3.OC_IsActive = true;

			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			var defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.ExportDepot, Constants.TransportModes.Sea);
			AssertEquals("Default Contact", contact3.PK, defaultContact.PK);
		}

		public void TestAggregateTypeFallback()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);

			OrgContact contact1 = CreateNewContact(ContactType.ExportSeaDepot, false);
			OrgContact contact2 = CreateNewContact(ContactType.ExportSeaDepot, true);
			AssertEquals("Organisation.Contacts.Count", 2, Organisation.Contacts.Count);
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.ExportDepot, Constants.TransportModes.Sea);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.ExportDepot, false);
			contact2 = CreateNewContact(ContactType.ExportDepot, true);
			AssertEquals("Organisation.Contacts.Count", 2, Organisation.Contacts.Count);
			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.ExportDepot, Constants.TransportModes.Sea);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.Depot, false);
			contact2 = CreateNewContact(ContactType.Depot, true);
			AssertEquals("Organisation.Contacts.Count", 2, Organisation.Contacts.Count);
			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.ExportDepot, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.ExportFreightAgent, true);
			contact2 = CreateNewContact(ContactType.All, true);
			AssertEquals("Organisation.Contacts.Count", 2, Organisation.Contacts.Count);
			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.ExportDepot, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);
		}

		public void TestConsigneeAndNotForwarder()
		{
			Organisation.OH_IsForwarder = false;
			Assert("Oranisation is not Forwarder", !Organisation.OH_IsForwarder);

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.Consignee, false);
			OrgContact contact2 = CreateNewContact(ContactType.Consignee, true);
			OrgContact contact3 = CreateNewContact(ContactType.ImportFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.Depot, false);
			contact2 = CreateNewContact(ContactType.Depot, true);
			contact3 = CreateNewContact(ContactType.ImportAirFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee, Constants.TransportModes.Air);
			AssertEquals("Default Contact", ContactType.Consignee.DefaultName, defaultContact.OC_ContactName);
		}

		public void TestConsigneeAndForwarder()
		{
			Organisation.OH_IsForwarder = true;
			Assert("Oranisation is Forwarder", Organisation.OH_IsForwarder);

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.Consignee, false);
			OrgContact contact2 = CreateNewContact(ContactType.Consignee, true);
			OrgContact contact3 = CreateNewContact(ContactType.ImportFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.Depot, false);
			contact2 = CreateNewContact(ContactType.Depot, true);
			contact3 = CreateNewContact(ContactType.ImportAirFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact3.PK, defaultContact.PK);
		}

		public void TestConsignorAndNotForwarder()
		{
			Organisation.OH_IsForwarder = false;
			Assert("Oranisation is not Forwarder", !Organisation.OH_IsForwarder);

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.Consignor, false);
			OrgContact contact2 = CreateNewContact(ContactType.Consignor, true);
			OrgContact contact3 = CreateNewContact(ContactType.ExportFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.Depot, false);
			contact2 = CreateNewContact(ContactType.Depot, true);
			contact3 = CreateNewContact(ContactType.ExportAirFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor, Constants.TransportModes.Air);
			AssertEquals("Default Contact", ContactType.Consignor.DefaultName, defaultContact.OC_ContactName);
		}

		public void TestConsignorAndForwarder()
		{
			Organisation.OH_IsForwarder = true;
			Assert("Oranisation is Forwarder", Organisation.OH_IsForwarder);

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.Consignor, false);
			OrgContact contact2 = CreateNewContact(ContactType.Consignor, true);
			OrgContact contact3 = CreateNewContact(ContactType.ExportFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);
			OrgContact defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact2.PK, defaultContact.PK);

			Organisation.Contacts.RemoveAll();
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			contact1 = CreateNewContact(ContactType.Depot, false);
			contact2 = CreateNewContact(ContactType.Depot, true);
			contact3 = CreateNewContact(ContactType.ExportAirFreightAgent, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			defaultContact = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor, Constants.TransportModes.Air);
			AssertEquals("Default Contact", contact3.PK, defaultContact.PK);
		}

		public void TestSystemDefaultContactReturnsSameContact_WithPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var anotherFactory = new BusinessObjectFactory();
			var anotherOrg = anotherFactory.NewWithValidTestData<OrgHeader>();
			var anotherContact = anotherOrg.Contacts.AddNew();
			anotherContact.OC_ContactName = "The Import Manager";
			anotherContact.OC_Email = "wrong@email.com";
			anotherFactory.Save();

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);

			var defaultContactA1 = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor);
			AssertEquals("Default Contact Consignor", ContactType.Consignor.DefaultName, defaultContactA1.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactA1.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactA1.OC_OH);

			var defaultContactA2 = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor);
			AssertEquals("Default Contact Consignor", ContactType.Consignor.DefaultName, defaultContactA2.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactA2.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactA2.OC_OH);

			AssertSame("Should return the same contact everytime.", defaultContactA1, defaultContactA2);

			var defaultContactB = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee);
			AssertEquals("Default Contact Consignee", ContactType.Consignee.DefaultName, defaultContactB.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactB.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactB.OC_OH);

			AssertNotEquals("Contacts should be different", defaultContactA1.PK, defaultContactB.PK);
		}

		public void TestSystemDefaultContactReturnsSameContact_WithoutPrintSystemCreatedContactWhenNoRealContactFound()
		{
			DocumentsDataRegistry.Instance.PrintSystemCreatedContactWhenNoRealContactFound.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var anotherFactory = new BusinessObjectFactory();
			var anotherOrg = anotherFactory.NewWithValidTestData<OrgHeader>();
			var anotherContact = anotherOrg.Contacts.AddNew();
			anotherContact.OC_ContactName = "The Import Manager";
			anotherContact.OC_Email = "wrong@email.com";
			anotherFactory.Save();

			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);

			var defaultContactA1 = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor);
			AssertEquals("Contact name should be empty as PrintSystemCreatedContactWhenNoRealContactFound is set to false", string.Empty, defaultContactA1.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactA1.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactA1.OC_OH);

			var defaultContactA2 = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignor);
			AssertEquals("Contact name should be empty as PrintSystemCreatedContactWhenNoRealContactFound is set to false", string.Empty, defaultContactA2.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactA2.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactA2.OC_OH);

			AssertSame("Should return the same contact everytime.", defaultContactA1, defaultContactA2);

			var defaultContactB = new DefaultContactFinder(Organisation).DefaultContact(ContactType.Consignee);
			AssertEquals("Contact name should be empty as PrintSystemCreatedContactWhenNoRealContactFound is set to false", string.Empty, defaultContactB.OC_ContactName);
			AssertEquals("No email address", string.Empty, defaultContactB.OC_Email);
			AssertEquals("Linked to proper org", Organisation.PK, defaultContactB.OC_OH);

			AssertEquals("There should be only one system default contact with an empty name per organisation when PrintSystemCreatedContactWhenNoRealContactFound is set to false",
				defaultContactA1.PK, defaultContactB.PK);
		}

		public void TestSystemDefaultContactFactoryReturnsSingleton()
		{
			var defaultContactFinderWithoutFactory1 = new DefaultContactFinder(null, true);
			var defaultContactFinderWithoutFactory2 = new DefaultContactFinder(null, true);
			var systemDefaultContactFactoryForDefaultContactFinderWithoutFactory1 = defaultContactFinderWithoutFactory1.SystemDefaultContactFactory;
			var systemDefaultContactFactoryForDefaultContactFinderWithoutFactory2 = defaultContactFinderWithoutFactory2.SystemDefaultContactFactory;
			Assert("The system default contact factory should not be singleton without a desigated business object factory.", systemDefaultContactFactoryForDefaultContactFinderWithoutFactory1 != systemDefaultContactFactoryForDefaultContactFinderWithoutFactory2);

			var defaultContactFinderWithFactory1 = new DefaultContactFinder(null, true, Factory);
			var defaultContactFinderWithFactory2 = new DefaultContactFinder(null, true, Factory);
			var systemDefaultContactFactoryForDefaultContactFinderWithFactory1 = defaultContactFinderWithFactory1.SystemDefaultContactFactory;
			var systemDefaultContactFactoryForDefaultContactFinderWithFactory2 = defaultContactFinderWithFactory2.SystemDefaultContactFactory;
			Assert("The system default contact factory should be singleton when a business object factory is designated.", systemDefaultContactFactoryForDefaultContactFinderWithFactory1 == systemDefaultContactFactoryForDefaultContactFinderWithFactory2);
		}

		public void TestNoExistingSystemDefaultContacts()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";

			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailFaxThenPrint);
			var defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Email, defaultContact.OC_NotifyMode);
			AssertEquals(OrgConstants.AttachmentType.XLS, defaultContact.OC_AttachmentType);

			defaultContact.Delete();
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailThenPrint);
			defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Email, defaultContact.OC_NotifyMode);
			AssertEquals(OrgConstants.AttachmentType.XLS, defaultContact.OC_AttachmentType);

			defaultContact.Delete();
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.FaxEmailThenPrint);
			defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Fax, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);

			defaultContact.Delete();
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.FaxThenPrint);
			defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Fax, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);

			defaultContact.Delete();
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.PrintOnly);
			defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Print, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);
		}

		public void TestSystemDefaultContactsWhenOrgChange()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";

			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailFaxThenPrint);
			var defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Email, defaultContact.OC_NotifyMode);
			AssertEquals(OrgConstants.AttachmentType.XLS, defaultContact.OC_AttachmentType);

			organisation.MainAddress.OA_Email = "";
			organisation.Factory.Save();

			defaultContact = new DefaultContactFinder(organisation, true).DefaultContact(ContactType.Consignor);
			AssertEquals(Constants.ContactNotifyModes.Fax, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);
		}

		public void TestSystemDefaultContactWhenOrgHeaderChangeAndOrgHeaderObjectIsDifferentBetweenDeliveryAndCreated()
		{
			DocumentsDataRegistry.Instance.DeliveryMethodWhenNoContactSpecified.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeliveryMethodOptionList.Codes.EmailFaxThenPrint);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Email = "test@Organisation.com";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.Factory.Save();

			var organisationForDelivery = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
			var defaultContact = new DefaultContactFinder(organisationForDelivery).DefaultContact(ContactType.LocalTransport);
			AssertEquals(Constants.ContactNotifyModes.Email, defaultContact.OC_NotifyMode);
			AssertEquals(OrgConstants.AttachmentType.XLS, defaultContact.OC_AttachmentType);

			organisation.MainAddress.OA_Email = "";
			organisation.Factory.Save();
			defaultContact = new DefaultContactFinder(organisationForDelivery).DefaultContact(ContactType.LocalTransport);
			AssertEquals(Constants.ContactNotifyModes.Fax, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);

			organisation.MainAddress.OA_Fax = "";
			organisation.Factory.Save();
			defaultContact = new DefaultContactFinder(organisationForDelivery).DefaultContact(ContactType.LocalTransport);
			AssertEquals(Constants.ContactNotifyModes.Print, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);

			organisation.MainAddress.OA_Email = "";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.Factory.Save();
			var organisationForDelivery2 = new BusinessObjectFactory().Load<OrgHeader>(organisation.PK);
			defaultContact = new DefaultContactFinder(organisationForDelivery2).DefaultContact(ContactType.LocalTransport);
			AssertEquals(Constants.ContactNotifyModes.Fax, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);

			organisation.MainAddress.OA_Fax = "";
			organisation.Factory.Save();
			defaultContact = new DefaultContactFinder(organisationForDelivery2).DefaultContact(ContactType.LocalTransport);
			AssertEquals(Constants.ContactNotifyModes.Print, defaultContact.OC_NotifyMode);
			AssertEquals("", defaultContact.OC_AttachmentType);
		}

		public void TestContactTypeAsString()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			OrgContact contact1 = CreateNewContact(ContactType.CustomerService, false);
			OrgContact contact2 = CreateNewContact(ContactType.CustomerService, true);
			OrgContact contact3 = CreateNewContact(ContactType.All, true);
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			AssertEquals("Default Contact", contact2.PK, new DefaultContactFinder(Organisation).DefaultContact(ContactType.CustomerService.Code).PK);
		}

		public void TestOrganisationIsNull()
		{
			OrgHeader org = null;
			OrgContact defaultContact = new DefaultContactFinder(org).DefaultContact(ContactType.Marketing);
			AssertEquals("Default Contact", ContactType.Marketing.DefaultName, defaultContact.OC_ContactName);
		}

		public void TestDefaultContactAllocationType()
		{
			AssertEquals("Organisation.Contacts.Count", 0, Organisation.Contacts.Count);
			var contact1 = CreateNewContact(ContactType.CustomerService, false);
			var allocation1 = contact1.Allocations.AddNew();
			allocation1.PC_Type = "XXX";
			var contact2 = CreateNewContact(ContactType.CustomerService, true);
			var allocation2 = contact2.Allocations.AddNew();
			allocation2.PC_Type = "YYY";
			var contact3 = CreateNewContact(ContactType.All, false);
			var allocation3 = contact3.Allocations.AddNew();
			allocation3.PC_Type = "CUS";
			AssertEquals("Organisation.Contacts.Count", 3, Organisation.Contacts.Count);

			AssertEquals("Default Contact", contact3.PK, new DefaultContactFinder(Organisation).DefaultContactAllocationType("CUS").PK);
		}

		public void TestGetDefaultNotifyPartyContact()
		{
			AssertEquals(ZGuid.Empty, DefaultContactFinder.GetDefaultNotifyPartyContact(null));

			var org = Factory.New<OrgHeader>();
			AssertEquals(ZGuid.Empty, DefaultContactFinder.GetDefaultNotifyPartyContact(org));

			var contact = org.Contacts.AddNew();
			contact.OC_IsActive = true;
			var contactDocument = contact.Documents.AddNew();
			contactDocument.OD_DocumentGroup = ContactType.NotifyParty.Code;
			contactDocument.OD_DefaultContact = true;
			AssertEquals(contact.PK, DefaultContactFinder.GetDefaultNotifyPartyContact(org));
		}

		#region Implementation

		OrgHeader Organisation;

		protected override void SetUp()
		{
			base.SetUp();
			Organisation = Factory.New<OrgHeader>();
			Organisation.OH_Code = "ORG";
			Factory.Save();
		}

		OrgContact CreateNewContact(ContactType contactType, ZBool isDefault)
		{
			OrgContact contact = Organisation.Contacts.AddNew();
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = contactType.Code;
			contact.Documents[0].OD_DefaultContact = isDefault;
			return contact;
		}

		#endregion
	}
}
