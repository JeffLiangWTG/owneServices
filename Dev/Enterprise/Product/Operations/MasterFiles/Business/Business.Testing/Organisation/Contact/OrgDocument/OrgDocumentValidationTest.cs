using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMenuItemAndDocGroupAreMutuallyExclusive()
		{
			OrgDocument doc = Factory.New<OrgDocument>();
			doc.OD_DocumentGroup = ContactType.All.Code;
			Assert("Should be no error on OD_DocumentGroup", !doc.OD_DocumentGroupInfo.HasErrors());
			Assert("Should be no error on OD_SU_MenuItem", !doc.OD_SU_MenuItemInfo.HasErrors());

			doc.OD_SU_MenuItem = ZGuid.NewZGuid();
			Assert("Should be error on OD_DocumentGroup", doc.OD_DocumentGroupInfo.HasErrors());
			Assert("Should be error on OD_SU_MenuItem", doc.OD_SU_MenuItemInfo.HasErrors());

			doc.OD_DocumentGroup = "";
			Assert("Should be no error on OD_DocumentGroup", !doc.OD_DocumentGroupInfo.HasErrors());
			Assert("Should be no error on OD_SU_MenuItem", !doc.OD_SU_MenuItemInfo.HasErrors());

			doc.OD_SU_MenuItem = ZGuid.Empty;
			Assert("Should be error on OD_DocumentGroup", doc.OD_DocumentGroupInfo.HasErrors());
			Assert("Should be error on OD_SU_MenuItem", doc.OD_SU_MenuItemInfo.HasErrors());

			doc.OD_DocumentGroup = ContactType.CustomerService.Code;
			Assert("Should be no error on OD_DocumentGroup", !doc.OD_DocumentGroupInfo.HasErrors());
			Assert("Should be no error on OD_SU_MenuItem", !doc.OD_SU_MenuItemInfo.HasErrors());
		}

		public void TestCheckOD_OH_RelatedFilterByPartyWillNotThrowNullReferenceException()
		{
			var doc = Factory.New<OrgDocument>();
			doc.OD_DocumentGroup = ContactType.All.Code;
			doc.OD_SU_MenuItem = ZGuid.NewZGuid();

			var org = Factory.New<OrgHeader>();
			doc.OD_OH_RelatedFilterByParty = org.PK;

			AssertNoExceptionThrown("should have no null reference exception", () => doc.Validation.ValidateOD_OH_RelatedFilterByParty());
		}

		public void TestDefaultContactForIndividualDocument()
		{
			StmMenuItem menuItem1 = Factory.New<StmMenuItem>();
			StmMenuItem menuItem2 = Factory.New<StmMenuItem>();

			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument contact1Doc = contact1.Documents.AddNew();
			contact1Doc.OD_SU_MenuItem = menuItem1.PK;
			contact1Doc.OD_DefaultContact = true;

			OrgContact contact2 = org.Contacts.AddNew();
			OrgDocument contact2Doc = contact2.Documents.AddNew();
			contact2Doc.OD_SU_MenuItem = menuItem2.PK;
			contact2Doc.OD_DefaultContact = true;

			menuItem1.SU_MenuPath = "Legacy Documents/Departure";
			menuItem2.SU_MenuPath = "Departure";

			contact1Doc.OD_DefaultContact = false;
			contact1Doc.OD_DefaultContact = true;
			contact2Doc.OD_DefaultContact = false;
			contact2Doc.OD_DefaultContact = true; //to re-do validation

			AssertHasErrors("Error expected - They have the same DocumentId despite their different PKs", contact1Doc.OD_DefaultContactInfo);
			AssertHasErrors("Error expected - They have the same DocumentId despite their different PKs", contact2Doc.OD_DefaultContactInfo);

			menuItem1.SU_MenuName = "a";
			menuItem2.SU_MenuName = "b";
			contact1Doc.OD_DefaultContact = false;
			contact1Doc.OD_DefaultContact = true;
			contact2Doc.OD_DefaultContact = false;
			contact2Doc.OD_DefaultContact = true; //to re-do validation

			AssertNoErrors("No errors expected - each row is for a different document", contact1Doc.OD_DefaultContactInfo);
			AssertNoErrors("No errors expected - each row is for a different document", contact2Doc.OD_DefaultContactInfo);

			contact2Doc.OD_SU_MenuItem = menuItem1.PK;
			contact2Doc.OD_DefaultContact = true;
			AssertHasErrors("Error expected - two defaults for the same doc", contact1Doc.OD_DefaultContactInfo);
			AssertHasErrors("Error expected - two defaults for the same doc", contact2Doc.OD_DefaultContactInfo);

			contact2Doc.OD_SU_MenuItem = menuItem2.PK;
			contact1Doc.OD_DefaultContact = false;
			AssertNoErrors("No errors expected - only one default per doc", contact1Doc.OD_DefaultContactInfo);
			AssertNoErrors("No errors expected - only one default per doc", contact2Doc.OD_DefaultContactInfo);
		}

		public void TestOnlyOneDefaultPerDocGroupPerOrganisation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument contact1Doc = contact1.Documents.AddNew();
			contact1Doc.OD_DocumentGroup = ContactType.Consignee.Code;
			contact1Doc.OD_DefaultContact = true;

			OrgContact contact2 = org.Contacts.AddNew();
			OrgDocument contact2Doc = contact2.Documents.AddNew();
			contact2Doc.OD_DocumentGroup = ContactType.Receivables.Code;
			contact2Doc.OD_DefaultContact = true;

			Assert("No errors expected - only one default per type", !contact1Doc.OD_DefaultContactInfo.HasErrors());
			Assert("No errors expected - only one default per type", !contact2Doc.OD_DefaultContactInfo.HasErrors());

			contact2Doc.OD_DocumentGroup = ContactType.Consignee.Code;
			contact2Doc.OD_DefaultContact = true;
			Assert("Error expected - two defaults per type", contact1Doc.OD_DefaultContactInfo.HasErrors());
			Assert("Error expected - two defaults per type", contact2Doc.OD_DefaultContactInfo.HasErrors());

			contact1Doc.OD_DocumentGroup = ContactType.Consignee.Code;
			contact1Doc.OD_DefaultContact = false;
			Assert("No errors expected - only one default", !contact1Doc.OD_DefaultContactInfo.HasErrors());
			Assert("No errors expected - only one default", !contact2Doc.OD_DefaultContactInfo.HasErrors());
		}

		public void TestValidationForDefaultShouldExcludeInactiveContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact1Doc = contact1.Documents.AddNew();
			contact1Doc.OD_DocumentGroup = ContactType.Consignee.Code;
			contact1Doc.OD_DefaultContact = true;
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var contact1Menu = contact1.Documents.AddNew();
			contact1Menu.OD_SU_MenuItem = menuItem.PK;
			contact1Menu.OD_DefaultContact = true;

			var contact2 = org.Contacts.AddNew();
			var contact2Doc = contact2.Documents.AddNew();
			contact2Doc.OD_DocumentGroup = ContactType.Consignee.Code;
			contact2Doc.OD_DefaultContact = true;

			var message = "There can only be one official contact for each document / document group.";

			AssertHasError(contact2Doc.OD_DefaultContactInfo, message);

			contact1.OC_IsActive = false;
			contact2Doc.Validation.ValidateOD_DefaultContact();

			AssertNoErrors(contact2Doc.OD_DefaultContactInfo);

			contact2Doc.OD_DefaultContact = false;
			contact1.OC_IsActive = true;
			var contact2Menu = contact2.Documents.AddNew();
			contact2Menu.OD_SU_MenuItem = menuItem.PK;
			contact2Menu.OD_DefaultContact = true;
			AssertHasError(contact2Menu.OD_DefaultContactInfo, message);

			contact1.OC_IsActive = false;
			contact2Menu.Validation.ValidateOD_DefaultContact();

			AssertNoErrors(contact2Menu.OD_DefaultContactInfo);
		}

		public void TestOnlyOneNotifyPartyPerOrganisation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.Documents.AddNew();
			contact1.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.Documents.AddNew();
			contact2.Documents[0].OD_DocumentGroup = ContactType.NotifyParty.Code;

			Assert("2 notify parties, so error expected on Contact1 OD_DocumentGroup", contact1.Documents[0].OD_DocumentGroupInfo.HasErrors());
			Assert("2 notify parties, so error expected on Contact2 OD_DocumentGroup", contact2.Documents[0].OD_DocumentGroupInfo.HasErrors());

			contact1.Documents[0].OD_DocumentGroup = ContactType.Consignee.Code;
			Assert("1 notify parties, so no error expected on Contact1 OD_DocumentGroup", !contact1.Documents[0].OD_DocumentGroupInfo.HasErrors());
			Assert("1 notify parties, so no error expected on Contact2 OD_DocumentGroup", !contact2.Documents[0].OD_DocumentGroupInfo.HasErrors());
		}

		public void TestAttachmentTypeValidation()
		{
			var org = Factory.New<OrgHeader>();

			OrgContact contact = org.Contacts.AddNew();
			OrgDocument doc = contact.Documents.AddNew();
			doc.OD_DeliverBy = Constants.ContactNotifyModes.Email;

			doc.OD_AttachmentType = "ZZZ";
			AssertHasErrors(doc.OD_AttachmentTypeInfo);

			doc.OD_AttachmentType = OrgConstants.AttachmentType.PDF;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.HTML;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.HTMF;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.PDFC;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);

			doc.OD_AttachmentType = "";
			AssertHasErrors("Mandatory for DeliveryBy = Email", doc.OD_AttachmentTypeInfo);

			doc.OD_AttachmentType = OrgConstants.AttachmentType.XLS;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);

			doc.OD_DeliverBy = Constants.ContactNotifyModes.EPrint;
			doc.OD_AttachmentType = "";
			AssertHasErrors("Mandatory for DeliveryBy = Eprint", doc.OD_AttachmentTypeInfo);

			doc.OD_AttachmentType = OrgConstants.AttachmentType.XLS;
			AssertNoErrors("Valid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.HTML;
			AssertHasErrors("Invalid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.HTMF;
			AssertHasErrors("Invalid", doc.OD_AttachmentTypeInfo);
			doc.OD_AttachmentType = OrgConstants.AttachmentType.PDFC;
			AssertHasErrors("Invalid", doc.OD_AttachmentTypeInfo);

			doc.OD_DeliverBy = Constants.ContactNotifyModes.Fax;
			doc.OD_AttachmentType = "";
			AssertNoErrors("Not Mandatory for DeliveryBy != Email/EPrint", doc.OD_AttachmentTypeInfo);
		}

		public void TestShipmentModeValidation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument doc = contact1.Documents.AddNew();

			doc.OD_FilterShipmentMode = "ZZZ";
			AssertHasErrors(doc.OD_FilterShipmentModeInfo);

			doc.OD_FilterShipmentMode = "AIR";
			AssertNoErrors("Valid", doc.OD_FilterShipmentModeInfo);

			doc.OD_FilterShipmentMode = "";
			AssertHasErrors("Mandatory", doc.OD_FilterShipmentModeInfo);
		}

		public void TestFilterDirectionValidation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument doc = contact1.Documents.AddNew();

			doc.OD_FilterDirection = "ZZZ";
			AssertHasErrors(doc.OD_FilterDirectionInfo);

			doc.OD_FilterDirection = "IMP";
			AssertNoErrors("Valid", doc.OD_FilterDirectionInfo);

			doc.OD_FilterDirection = "";
			AssertHasErrors("Mandatory", doc.OD_FilterDirectionInfo);
		}

		public void TestLocalPortValidation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument doc = contact1.Documents.AddNew();

			doc.OD_FilterLocalPort = "";
			AssertNoErrors("Not Mandatory", doc.OD_FilterLocalPortInfo);

			doc.OD_FilterLocalPort = "ZZZ";
			AssertHasErrors(doc.OD_FilterLocalPortInfo);

			doc.OD_FilterLocalPort = "AUSYD";
			AssertNoErrors("Valid", doc.OD_FilterLocalPortInfo);

			doc.OD_FilterLocalPort = "NZ";
			AssertNoErrors("Valid", doc.OD_FilterLocalPortInfo);
		}

		public void TestForeignPortValidation()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgContact contact1 = org.Contacts.AddNew();
			OrgDocument doc = contact1.Documents.AddNew();

			doc.OD_FilterForeignPort = "";
			AssertNoErrors("Not Mandatory", doc.OD_FilterForeignPortInfo);

			doc.OD_FilterForeignPort = "ZZZ";
			AssertHasErrors(doc.OD_FilterForeignPortInfo);

			doc.OD_FilterForeignPort = "AUSYD";
			AssertNoErrors("Valid", doc.OD_FilterForeignPortInfo);

			doc.OD_FilterForeignPort = "NZ";
			AssertNoErrors("Valid", doc.OD_FilterForeignPortInfo);
		}

		public void TestCopyRecipientValidation()
		{
			// Arrange
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var organizationContact = Factory.NewWithValidTestData<OrgContact>();
			organizationContact.OC_OH = organization.PK;
			var organizationDocument = Factory.NewWithValidTestData<OrgDocument>();
			organizationDocument.OD_OC = organizationContact.PK;
			organizationDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Factory.Save();
			// Act
			organizationDocument.OD_CarbonCopyRecipientsAsString = "  test1@test.com,   a@b.com ";
			// Assert
			AssertNoErrors(organizationDocument.OD_CarbonCopyRecipientsAsStringInfo);
			// Act
			organizationDocument.OD_CarbonCopyRecipientsAsString = "admin@microsoft.com, test1.test.com";
			// Assert
			AssertHasErrors(organizationDocument.OD_CarbonCopyRecipientsAsStringInfo);
			// Act
			organizationDocument.OD_BlindCarbonCopyRecipientsAsString = "test2@test.com";
			// Assert
			AssertNoErrors(organizationDocument.OD_BlindCarbonCopyRecipientsAsStringInfo);
			// Act
			organizationDocument.OD_BlindCarbonCopyRecipientsAsString = "123456@123.com;abc@def.net";
			// Assert
			AssertHasErrors(organizationDocument.OD_BlindCarbonCopyRecipientsAsStringInfo);
		}

		public void TestRelatedPartyShouldHaveWarningWhenDocumentGroupIsNotCNEorCNR()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = organization.PK;
			var organizationDocument = Factory.NewWithValidTestData<OrgDocument>();
			organizationDocument.OD_OC = contact.PK;
			organizationDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Factory.Save();
			foreach (ICodeDescription codeDescription in OrgCodeLists.ContactType_List)
			{
				if (codeDescription.Code != ContactType.Consignee && codeDescription.Code != ContactType.Consignor)
				{
					organizationDocument.OD_DocumentGroup = codeDescription.Code;
					organizationDocument.OD_OH_RelatedFilterByParty = relatedParty.PK;
					AssertHasWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
					organizationDocument.OD_OH_RelatedFilterByParty = ZGuid.Empty;
					AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
				}
			}
		}

		public void TestRelatedPartyShouldNotHaveWarningWhenDocumentGroupIsCNEorCNR()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = organization.PK;
			var organizationDocument = Factory.NewWithValidTestData<OrgDocument>();
			organizationDocument.OD_OC = contact.PK;
			organizationDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			Factory.Save();
			organizationDocument.OD_DocumentGroup = ContactType.Consignee.Code;
			organizationDocument.OD_OH_RelatedFilterByParty = relatedParty.PK;
			AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
			organizationDocument.OD_DocumentGroup = ContactType.Consignor.Code;
			AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
		}

		public void TestRelatedPartyShouldHaveWarningWhenDocumentBelongsToGroupWhichIsNotCNEorCNR()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_ContactType = ContactType.ControllingCustomer.Code;
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = organization.PK;
			var organizationDocument = Factory.NewWithValidTestData<OrgDocument>();
			organizationDocument.OD_OC = contact.PK;
			organizationDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			organizationDocument.OD_SU_MenuItem = document.PK;
			Factory.Save();
			foreach (ICodeDescription codeDescription in OrgCodeLists.ContactType_List)
			{
				if (codeDescription.Code != ContactType.Consignee && codeDescription.Code != ContactType.Consignor)
				{
					document.SU_ContactType = codeDescription.Code;
					Factory.Save();
					organizationDocument.OD_OH_RelatedFilterByParty = relatedParty.PK;
					AssertHasWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
					organizationDocument.OD_OH_RelatedFilterByParty = ZGuid.Empty;
					AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
				}
			}
		}

		public void TestRelatedPartyShouldNotHaveWarningWhenDocumentBelongsToGroupWhichIsCNEorCNR()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_ContactType = ContactType.Consignee.Code;

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = organization.PK;
			var organizationDocument = Factory.NewWithValidTestData<OrgDocument>();
			organizationDocument.OD_OC = contact.PK;
			organizationDocument.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			organizationDocument.OD_SU_MenuItem = document.PK;
			Factory.Save();
			organizationDocument.OD_OH_RelatedFilterByParty = relatedParty.PK;
			AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
			document.SU_ContactType = ContactType.Consignor.Code;
			Factory.Save();
			organizationDocument.OD_OH_RelatedFilterByParty = relatedParty.PK;
			AssertNoWarnings(organizationDocument.OD_OH_RelatedFilterByPartyInfo);
		}
	}
}
