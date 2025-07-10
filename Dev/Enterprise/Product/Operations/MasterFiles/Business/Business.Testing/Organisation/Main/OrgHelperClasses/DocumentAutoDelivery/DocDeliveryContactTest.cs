using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocDeliveryContact))]
	sealed class DocDeliveryContactTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableNameOfDeliveryAddress()
		{
			var deliveryContact = new DocDeliveryContact(Factory) { DeliveryMethod = Constants.ContactNotifyModes.Email };
			AssertEquals("E-Mail Address", deliveryContact.DeliveryAddressInfo.HumanReadableName);

			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			AssertEquals("Fax Address", deliveryContact.DeliveryAddressInfo.HumanReadableName);

			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.EDoc;
			AssertEquals("Delivery Address", deliveryContact.DeliveryAddressInfo.HumanReadableName);
		}

		// This is aimed to reduce Validate count to improve performance
		public void TestDoNotValidateIfPropertyHasNoErrorAndRelatedValueIsNotEmpty()
		{
			var docDeliveryContact = new DummyDocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Email };

			docDeliveryContact.DeliveryAddress = "invalidemail";
			Assert("The email address \"invalidemail\" is invalid.", docDeliveryContact.HasErrors);
			var recipient = docDeliveryContact.EmailToRecipients[0];
			AssertEquals("invalidemail", recipient.EmailAddress);
			recipient.EmailAddress = "receive@123.com";
			AssertEquals("DeliveryAddress is updated.", "receive@123.com", docDeliveryContact.DeliveryAddress);
			Assert(!docDeliveryContact.HasErrors);
			var current = docDeliveryContact.CheckDeliveryAddressCount;
			var newRecipient = docDeliveryContact.EmailToRecipients.AddNew();
			newRecipient.EmailAddress = "newreceive@123.com";
			AssertEquals("DeliveryAddress is updated.", "receive@123.com, newreceive@123.com", docDeliveryContact.DeliveryAddress);
			Assert(!docDeliveryContact.HasErrors);
			AssertEquals(current, docDeliveryContact.CheckDeliveryAddressCount);

			docDeliveryContact.EmailCarbonCopyRecipientsAsString = "invalidemail";
			Assert("The email address \"invalidemail\" is invalid.", docDeliveryContact.HasErrors);
			var cc = docDeliveryContact.EmailCarbonCopyRecipients[0];
			AssertEquals("invalidemail", cc.EmailAddress);
			cc.EmailAddress = "receive@123.com";
			AssertEquals("EmailCarbonCopyRecipientsAsString is updated.", "receive@123.com", docDeliveryContact.EmailCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);
			current = docDeliveryContact.CheckEmailCarbonCopyRecipientsAsStringCount;
			var newCC = docDeliveryContact.EmailCarbonCopyRecipients.AddNew();
			newCC.EmailAddress = "newreceive@123.com";
			AssertEquals("EmailCarbonCopyRecipientsAsString is updated.", "receive@123.com, newreceive@123.com", docDeliveryContact.EmailCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);
			AssertEquals(current, docDeliveryContact.CheckEmailCarbonCopyRecipientsAsStringCount);

			docDeliveryContact.EmailBlindCarbonCopyRecipientsAsString = "invalidemail";
			Assert("The email address \"invalidemail\" is invalid.", docDeliveryContact.HasErrors);
			var bcc = docDeliveryContact.EmailBlindCarbonCopyRecipients[0];
			AssertEquals("invalidemail", bcc.EmailAddress);
			bcc.EmailAddress = "receive@123.com";
			AssertEquals("EmailBlindCarbonCopyRecipientsAsString is updated.", "receive@123.com", docDeliveryContact.EmailBlindCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);
			current = docDeliveryContact.CheckEmailBlindCarbonCopyRecipientsAsStringCount;
			var newBCC = docDeliveryContact.EmailBlindCarbonCopyRecipients.AddNew();
			newBCC.EmailAddress = "newreceive@123.com";
			AssertEquals("EmailBlindCarbonCopyRecipientsAsString is updated.", "receive@123.com, newreceive@123.com", docDeliveryContact.EmailBlindCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);
			AssertEquals(current, docDeliveryContact.CheckEmailBlindCarbonCopyRecipientsAsStringCount);
		}

		#region Test class

		class DummyDocDeliveryContact : DocDeliveryContact
		{
			public DummyDocDeliveryContact(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override DocDeliveryContactValidation GetNewValidation()
			{
				return new DummyDocDeliveryContactValidation(this);
			}

			internal int CheckEmailBlindCarbonCopyRecipientsAsStringCount;
			internal int CheckEmailCarbonCopyRecipientsAsStringCount;
			internal int CheckDeliveryAddressCount;
		}

		class DummyDocDeliveryContactValidation : DocDeliveryContactValidation
		{
			public DummyDocDeliveryContactValidation(DummyDocDeliveryContact parent) : base(parent)
			{
				Parent = parent;
			}

			readonly DummyDocDeliveryContact Parent;

			protected override void CheckEmailBlindCarbonCopyRecipientsAsString()
			{
				base.CheckEmailBlindCarbonCopyRecipientsAsString();
				Parent.CheckEmailBlindCarbonCopyRecipientsAsStringCount++;
			}

			protected override void CheckEmailCarbonCopyRecipientsAsString()
			{
				base.CheckEmailCarbonCopyRecipientsAsString();
				Parent.CheckEmailCarbonCopyRecipientsAsStringCount++;
			}

			protected override void CheckDeliveryAddress()
			{
				base.CheckDeliveryAddress();
				Parent.CheckDeliveryAddressCount++;
			}
		}

		#endregion

		public void TestRecipientsUpdateShouldTriggerValidation()
		{
			var docDeliveryContact = new DocDeliveryContact(Factory) { DeliveryMethod = Core.Constants.ContactNotifyModes.Email };

			docDeliveryContact.DeliveryAddress = "invalidemail";
			Assert("The email address \"invalidemail\" is invalid.", docDeliveryContact.HasErrors);
			var recipient = docDeliveryContact.EmailToRecipients[0];
			AssertEquals("invalidemail", recipient.EmailAddress);
			recipient.EmailAddress = "receive@123.com";
			AssertEquals("DeliveryAddress is updated.", "receive@123.com", docDeliveryContact.DeliveryAddress);
			Assert(!docDeliveryContact.HasErrors);

			docDeliveryContact.EmailCarbonCopyRecipientsAsString = "invalidemail1";
			Assert("The email address \"invalidemail1\" is invalid.", docDeliveryContact.HasErrors);
			var cc = docDeliveryContact.EmailCarbonCopyRecipients[0];
			AssertEquals("invalidemail1", cc.EmailAddress);
			cc.EmailAddress = "carbon@123.com";
			AssertEquals("EmailCarbonCopyRecipientsAsString is updated.", "carbon@123.com", docDeliveryContact.EmailCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);

			docDeliveryContact.EmailBlindCarbonCopyRecipientsAsString = "invalidemail2";
			Assert("The email address \"invalidemail2\" is invalid.", docDeliveryContact.HasErrors);
			var blindCC = docDeliveryContact.EmailBlindCarbonCopyRecipients[0];
			AssertEquals("invalidemail2", blindCC.EmailAddress);
			blindCC.EmailAddress = "blindcarbon@123.com";
			AssertEquals("EmailBlindCarbonCopyRecipientsAsString is updated.", "blindcarbon@123.com", docDeliveryContact.EmailBlindCarbonCopyRecipientsAsString);
			Assert(!docDeliveryContact.HasErrors);
		}

		public void TestDeliveryMethodDescriptionHasOwnPropertyInfo()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			AssertEquals("DeliveryMethodDescription", contact.DeliveryMethodDescriptionInfo.Name);
		}

		public void TestAddressChangedAccordinglyWhenContactOrOrgHeaderChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "Main Address 1";

			var anotherAddress = org.Addresses.AddNew();
			anotherAddress.Address1 = "Another Address 1";

			var contactWithMainAddress = org.Contacts.AddNew();
			contactWithMainAddress.OC_ContactName = "Main";
			contactWithMainAddress.OC_Email = "main@email.com";
			contactWithMainAddress.WorkingAddressPK = org.MainAddress.PK;

			var contactWithAnotherAddress = org.Contacts.AddNew();
			contactWithAnotherAddress.OC_ContactName = "Not Main";
			contactWithAnotherAddress.OC_Email = "notmain@email.com";
			contactWithAnotherAddress.WorkingAddressPK = anotherAddress.PK;

			var anotherOrg = Factory.NewWithValidTestData<OrgHeader>();
			anotherOrg.MainAddress.Address1 = "Another Main Address 1";

			var notMainAddressInAnotherOrg = anotherOrg.Addresses.AddNew();
			notMainAddressInAnotherOrg.Address1 = "Another Address 1 in Another Org";
			var anotherContactWithNotMainAddress = anotherOrg.Contacts.AddNew();
			anotherContactWithNotMainAddress.OC_ContactName = "Not Main";
			anotherContactWithNotMainAddress.OC_Email = "anothernotmain@email.com";
			anotherContactWithNotMainAddress.WorkingAddressPK = notMainAddressInAnotherOrg.PK;

			var deliveryContact = new DocDeliveryContact(Factory)
			{
				DeliveryMethod = Constants.ContactNotifyModes.Email,
				OrgHeaderPK = org.PK,
				Name = "Main"
			};
			AssertEquals("Main Address 1", deliveryContact.Address1);

			deliveryContact.Name = "Not Main";
			AssertEquals("Another Address 1", deliveryContact.Address1);

			deliveryContact.OrgHeaderPK = anotherOrg.PK;
			AssertEquals("Another Address 1 in Another Org", deliveryContact.Address1);
		}

		public void TestMostRelevantPostalCountry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.Address1 = "Parent Org Address1";
			var addressAlternate = org.Addresses.AddNew();
			addressAlternate.Address1 = "Parent Org Alternative Address1";
			var orgOverride = Factory.NewWithValidTestData<OrgHeader>();
			orgOverride.MainAddress.Address1 = "Override Org Address1";
			var addressAlternateOverride = orgOverride.Addresses.AddNew();
			addressAlternateOverride.Address1 = "Override Org Alternative Address1";

			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = "Just1n";
			var docAutoDelivery = new DocAutoDelivery();

			org.MainAddress.OA_RN_NKCountryCode = "AU";
			var docDeliveryContact = docAutoDelivery.GetDeliveryDetailsForContact(orgContact);
			AssertEquals("AU", docDeliveryContact.MostRelevantPostalCountry.Code);

			org.MainAddress.OA_RN_NKCountryCode = "";
			org.MainAddress.OA_RL_NKRelatedPortCode = "ADALV";
			docDeliveryContact = docAutoDelivery.GetDeliveryDetailsForContact(orgContact);
			AssertEquals("AD", docDeliveryContact.MostRelevantPostalCountry.Code);

			addressAlternate.OA_RN_NKCountryCode = "CN";
			orgContact.OC_OA_OrgAddress = addressAlternate.PK;
			docDeliveryContact = docAutoDelivery.GetDeliveryDetailsForContact(orgContact);
			AssertEquals("CN", docDeliveryContact.MostRelevantPostalCountry.Code);

			orgContact.OC_OA_OrgAddress = ZGuid.Empty;
			orgOverride.MainAddress.OA_RN_NKCountryCode = "US";
			orgContact.OC_OH_AddressOverride = orgOverride.PK;
			docDeliveryContact = docAutoDelivery.GetDeliveryDetailsForContact(orgContact);
			AssertEquals("US", docDeliveryContact.MostRelevantPostalCountry.Code);

			addressAlternateOverride.OA_RN_NKCountryCode = "FR";
			orgContact.OC_OA_OrgAddress = addressAlternateOverride.PK;
			docDeliveryContact = docAutoDelivery.GetDeliveryDetailsForContact(orgContact);
			AssertEquals("FR", docDeliveryContact.MostRelevantPostalCountry.Code);
		}

		public void TestDeliveryMethodFieldType()
		{
			var deliveryContact = new DocDeliveryContact(Factory.GetCachedReadOnlyFactory())
			{
				DeliveryMethod = Constants.ContactNotifyModes.Email
			};
			AssertEquals(FieldType.TextCodeFindBox, deliveryContact.DeliveryMethodFieldType);

			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			AssertEquals(FieldType.Text, deliveryContact.DeliveryMethodFieldType);
		}

		public void TestDocEmailSubjectMacro()
		{
			var deliveryContact = new DocDeliveryContact(Factory.GetCachedReadOnlyFactory())
			{
				DeliveryMethod = Constants.ContactNotifyModes.Email,
				EmailSubjectMacro = "Some <Macro>"
			};
			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			Assert(deliveryContact.EmailSubjectMacro_ReadOnly);

			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			Assert(!deliveryContact.EmailSubjectMacro_ReadOnly);
			AssertEquals("Some <Macro>", deliveryContact.EmailSubjectMacro);
		}

		public void TestDeliveryGroup()
		{
			var deliveryContact = new DocDeliveryContact(Factory.GetCachedReadOnlyFactory());

			AssertExceptionThrown<InvalidOperationException>(() => { deliveryContact.DeliveryGroupId = ZGuid.NewZGuid(); });

			deliveryContact.EmailSubjectMacro = "Some Macro";
			deliveryContact.DeliveryGroupId = ZGuid.NewZGuid();
			Assert(!deliveryContact.DeliveryGroupId.IsEmpty);
		}

		public void TestContactsLookupContainsOneDefaultContactOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST";
			Factory.Save();

			var deliveryContact = new DocDeliveryContact(Factory.GetCachedReadOnlyFactory());
			deliveryContact.OrgHeaderPK = orgHeader.PK;

			var documentSupporter = new OrgHeaderDocumentSupporter(orgHeader);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Warehouse.Code;

			var deliveryContacts = new DocAutoDelivery().GetDeliveryContactsForDocPack(menuItem, documentSupporter, null, null);
			var anotherSystemContact = new DefaultContactFinder(orgHeader).DefaultContact(ContactType.Consignor, Enterprise.Core.Constants.TransportModes.Air);

			AssertArrayEqualsByElements(deliveryContacts.Cast<DocDeliveryContact>().Select(contact => contact.Contact.OC_ContactName).ToArray(), deliveryContact.Contacts.Cast<OrgContact>().Select(contact => contact.OC_ContactName).ToArray());
		}

		public void TestAttachmentTypes()
		{
			var mock = new Mock<IStmMenuItem>();
			var attachmentTypes = new CodeDescriptionPairList();
			attachmentTypes.AddPair("Foo");
			attachmentTypes.AddPair("Bar");
			mock.Setup(m => m.AttachmentTypes).Returns(attachmentTypes);

			var contacts = new DocDeliveryContactCollection(mock.Object, null, Factory);
			var contact = contacts.AddNew();

			AssertEquals(2, contact.AttachmentTypes.Count);
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode("Foo"));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode("Bar"));
		}

		#region Tests AttachmentTypeWithFormatSwitching for Email/ePrint Delivery

		public void TestAttachmentTypeWithFormatSwitching_EmailDelivery()
		{
			AssertAttachmentTypeWithFormatSwitching_EmailDelivery(Core.Constants.ContactNotifyModes.Email, true);
			AssertAttachmentTypeWithFormatSwitching_EmailDelivery(Core.Constants.ContactNotifyModes.EPrint, true);
		}

		public void TestAttachmentTypeWithoutFormatSwitching_EmailDelivery()
		{
			AssertAttachmentTypeWithFormatSwitching_EmailDelivery(Core.Constants.ContactNotifyModes.Email, false);
			AssertAttachmentTypeWithFormatSwitching_EmailDelivery(Core.Constants.ContactNotifyModes.EPrint, false);
		}

		void AssertAttachmentTypeWithFormatSwitching_EmailDelivery(string deliveryMethod, bool isFormatSwitchingRequired)
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = deliveryMethod;
			contact.IsFormatSwitchingRequired = isFormatSwitchingRequired;

			foreach (CodeDescriptionPair attachmentType in new AttachmentTypeList())
			{
				contact.AttachmentType = attachmentType.Code;
				AssertEquals(attachmentType.Code, contact.AttachmentType);

				if (isFormatSwitchingRequired && attachmentType.Code == AttachmentTypeList.Codes.Xls)
				{
					AssertEquals(AttachmentTypeList.Codes.Xlsx, contact.AttachmentTypeWithFormatSwitching);
				}
				else
				{
					AssertEquals(attachmentType.Code, contact.AttachmentTypeWithFormatSwitching);
				}
			}
		}

		#endregion

		#region Tests AttachmentTypeWithFormatSwitching for Non-Email Delivery

		public void TestAttachmentTypeWithFormatSwitching_NonEmailDelivery()
		{
			AssertAttachmentTypeWithFormatSwitching_NonEmailDelivery(true);
		}

		public void TestAttachmentTypeWithoutFormatSwitching_NonEmailDelivery()
		{
			AssertAttachmentTypeWithFormatSwitching_NonEmailDelivery(false);
		}

		void AssertAttachmentTypeWithFormatSwitching_NonEmailDelivery(bool isFormatSwitchingRequired)
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			contact.IsFormatSwitchingRequired = isFormatSwitchingRequired;

			foreach (CodeDescriptionPair attachmentType in new AttachmentTypeList())
			{
				contact.AttachmentType = attachmentType.Code;
				AssertEquals(string.Empty, contact.AttachmentType);
				AssertEquals(string.Empty, contact.AttachmentType);
			}
		}

		#endregion

		public void TestIsFormatSwitchingRequired()
		{
			var contact = new DocDeliveryContact(Factory);

			Assert("IsFormatSwitchingRequired should be false by default", !contact.IsFormatSwitchingRequired);

			contact.IsFormatSwitchingRequired = true;
			Assert("IsFormatSwitchingRequired should be true", contact.IsFormatSwitchingRequired);

			contact.IsFormatSwitchingRequired = false;
			Assert("IsFormatSwitchingRequired should be false", !contact.IsFormatSwitchingRequired);
		}

		public void TestConstructorNullFactoryThrowsArgumentNullException()
		{
			AssertExceptionThrown("new DocDeliveryContact(null)", typeof(ArgumentNullException), new AnonymousMethod(() => new DocDeliveryContact(null)));
			AssertNoExceptionThrown("new DocDeliveryContact(new BusinessObjectFactory())", new AnonymousMethod(() => new DocDeliveryContact(new BusinessObjectFactory())));
		}

		public void TestOrgAddressPK()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "OH_Code";
			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "Main Address Line 1";
			mainAddress.OA_Address2 = "Main Address Line 2";
			OrgAddress address = organisation.Addresses.AddNew();
			address.OA_Address1 = "Selected Address Line 1";
			address.OA_Address2 = "Selected Address Line 2";

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.OrgAddressPK = address.PK;

			AssertEquals("contact.Address1", "Selected Address Line 1", contact.Address1);
			AssertEquals("contact.Address2", "Selected Address Line 2", contact.Address2);

			AssertEquals(organisation.PK, contact.OrgHeaderPK);
			AssertEquals(address.PK, contact.OrgAddressPK);
		}

		public void TestOrgHeaderPKSetsOrgAddressPK()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "OH_Code";
			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "Main Address Line 1";
			mainAddress.OA_Address2 = "Main Address Line 2";
			OrgAddress otherAddress = organisation.Addresses.AddNew();
			otherAddress.OA_Address1 = "Other Address Line 1";
			otherAddress.OA_Address2 = "Other Address Line 2";

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.OrgHeaderPK = organisation.PK;

			AssertEquals("contact.Address1", "Main Address Line 1", contact.Address1);
			AssertEquals("contact.Address2", "Main Address Line 2", contact.Address2);

			AssertEquals(organisation.PK, contact.OrgHeaderPK);
			AssertEquals(mainAddress.PK, contact.OrgAddressPK);
		}

		public void TestInitialise()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			OrgDocument documentDeliveryType = Factory.New<OrgDocument>();
			contact.Initialise(menuItem, documentDeliveryType);

			AssertEquals("contact.MenuItem", menuItem, contact.MenuItem);
			AssertEquals("contact.DocumentDeliveryType", documentDeliveryType, contact.DocumentDeliveryType);
		}

		public void TestDeliveryAddressMaxLength()
		{
			var contact = new DocDeliveryContact(Factory);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;

			AssertEquals("DeliveryAddress should have MaxLength of 254 for DeliveryMethod = Fax", 254, contact.DeliveryAddressInfo.MaxLength);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			AssertEquals("DeliveryAddress should have no MaxLength for DeliveryMethod = Email", -1, contact.DeliveryAddressInfo.MaxLength);
		}

		public void TestAddressDetailsSetWhenOrgSet()
		{
			OrgHeader org1 = OrgHeader.New(Factory);
			org1.OH_FullName = "First Org";
			org1.OH_Code = "ZUB1";
			org1.MainAddress.OA_Address1 = "24 Kingfishers Avenue";
			org1.MainAddress.OA_Address2 = "Grove";
			org1.MainAddress.OA_City = "Wantage";
			org1.MainAddress.OA_State = "Oxford";
			org1.MainAddress.OA_PostCode = "OX1234J";
			org1.OH_RL_NKClosestPort = "GBOXF";
			org1.MainAddress.OA_Phone = "919384";
			org1.MainAddress.OA_Fax = "1234556";
			org1.MainAddress.OA_Email = "oxford@example.com";
			org1.MainAddress.OA_Language = Constants.Languages.EnglishAmerican;
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "Tom";

			OrgHeader org2 = OrgHeader.New(Factory);
			org2.OH_FullName = "Second Org";
			org2.OH_Code = "ZUB2";
			org2.MainAddress.OA_Address1 = "Mirzapur Road";
			org2.MainAddress.OA_Address2 = "Opp. Police Chowky";
			org2.MainAddress.OA_City = "Ahmedabad";
			org2.MainAddress.OA_State = "Gujarat";
			org2.MainAddress.OA_PostCode = "380001";
			org2.OH_RL_NKClosestPort = "INAMD";
			org2.MainAddress.OA_Phone = "07925623972";
			org2.MainAddress.OA_Fax = "07925623971";
			org2.MainAddress.OA_Email = "ahmedabad@example.com";
			org2.MainAddress.OA_Language = Constants.Languages.Icelandic;

			Factory.Save();

			DocDeliveryContact deliveryContact = contact1.DocDeliveryDetails();

			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Address1, deliveryContact.Address1);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Address2, deliveryContact.Address2);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_City, deliveryContact.City);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_State, deliveryContact.State);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_PostCode, deliveryContact.PostCode);
			AssertEquals("Doc Contact has correct address details", org1.UNLOCO, deliveryContact.UNLOCO);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Phone, deliveryContact.Phone);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Fax, deliveryContact.Fax);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Email, deliveryContact.Email);
			AssertEquals("Doc Contact has correct address details", org1.MainAddress.OA_Language, deliveryContact.Language);

			deliveryContact.OrgHeaderPK = org2.PK;
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Address1, deliveryContact.Address1);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Address2, deliveryContact.Address2);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_City, deliveryContact.City);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_State, deliveryContact.State);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_PostCode, deliveryContact.PostCode);
			AssertEquals("Doc Contact has correct address details", org2.UNLOCO, deliveryContact.UNLOCO);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Phone, deliveryContact.Phone);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Fax, deliveryContact.Fax);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Email, deliveryContact.Email);
			AssertEquals("Doc Contact has correct address details", org2.MainAddress.OA_Language, deliveryContact.Language);

			deliveryContact.OrgHeaderPK = ZGuid.Empty;
			AssertEquals("Doc Contact has correct address details", "*** NO ORGANIZATION DETAILS FOUND ***", deliveryContact.Address1);
			AssertEquals("Doc Contact has correct address details", ZString.Empty, deliveryContact.Address2);
			AssertEquals("Doc Contact has correct address details", ZString.Empty, deliveryContact.City);
			AssertEquals("Doc Contact has correct address details", ZString.Empty, deliveryContact.State);
			AssertEquals("Doc Contact has correct address details", ZString.Empty, deliveryContact.PostCode);
			AssertEquals("Doc Contact has correct address details", null, deliveryContact.UNLOCO);
			AssertEquals("Doc Contact has correct address details", ZString.Empty, deliveryContact.Language);

			deliveryContact.DeliveryLanguage = "RU-RU";
			AssertEquals("*** ДЕТАЛИ ОРГАНИЗАЦИИ НЕ НАЙДЕНЫ ***", deliveryContact.Address1);
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("*** ДЕТАЛИ ОРГАНИЗАЦИИ НЕ НАЙДЕНЫ ***", deliveryContact.Address1);
		}

		public void TestDeliveryAddressDefaultsFromOrgIfContactNotSpecified()
		{
			OrgHeader newOrg = OrgHeader.New(Factory);
			newOrg.OH_FullName = "Zubins Org 123";
			newOrg.OH_Code = "ZUB";
			newOrg.MainAddress.OA_Address1 = "hello street";
			newOrg.MainAddress.OA_Email = "mainemail@example.com";
			newOrg.MainAddress.OA_Fax = "91919191";

			OrgContact contact = newOrg.Contacts.AddNew();
			contact.OC_ContactName = "Mary";
			contact.OC_Fax = "11119999";
			contact.OC_Email = "mary@example.com";

			Factory.Save();

			DocDeliveryContact docContact = new DocDeliveryContact(Factory);
			docContact.OrgHeaderPK = newOrg.PK;
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Email address from org taken", "mainemail@example.com", docContact.Email);
			AssertEquals("Email address from org taken", "mainemail@example.com", docContact.DeliveryAddress);

			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Fax from org taken", "91919191", docContact.Fax);
			AssertEquals("Fax from org taken", "91919191", docContact.DeliveryAddress);

			docContact.Name = "Mary";
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Email address from contact taken", "mary@example.com", docContact.Email);
			AssertEquals("Email address from contact taken", "mary@example.com", docContact.DeliveryAddress);

			docContact.Name = "Someone else";
			AssertEquals("Email address not changed if contact is not specified", "mary@example.com", docContact.Email);
			AssertEquals("Email address not changed if contact is not specified", "mary@example.com", docContact.DeliveryAddress);

			docContact.Name = "Mary";
			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Fax from contact taken", "11119999", docContact.Fax);
			AssertEquals("Fax from contact taken", "11119999", docContact.DeliveryAddress);

			docContact.Name = "Someone else";
			AssertEquals("Fax not changed if contact is not specified", "11119999", docContact.Fax);
			AssertEquals("Fax not changed if contact is not specified", "11119999", docContact.DeliveryAddress);

			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Email address from org taken", "mainemail@example.com", docContact.Email);
			AssertEquals("Email address from org taken", "mainemail@example.com", docContact.DeliveryAddress);

			docContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Fax from org taken", "91919191", docContact.Fax);
			AssertEquals("Fax from org taken", "91919191", docContact.DeliveryAddress);
		}

		public void TestNotifyModes()
		{
			var contact = new DocDeliveryContact(Factory);
			Assert(contact.NotifyModes.ContainsCode("E-Mail"));
			AssertEquals("E-Mail", contact.NotifyModes.GetDescriptionFromCode("E-Mail"));

			Assert(contact.NotifyModes.ContainsCode("Fax"));
			AssertEquals("Fax", contact.NotifyModes.GetDescriptionFromCode("Fax"));

			Assert(contact.NotifyModes.ContainsCode("Print"));
			AssertEquals("Print", contact.NotifyModes.GetDescriptionFromCode("Print"));

			contact = new DocDeliveryContact(Factory);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_BusinessContext = "SHP";
			var documentDeliveryType = Factory.New<OrgDocument>();
			contact.Initialise(menuItem, documentDeliveryType);

			Assert(contact.NotifyModes.ContainsCode("eDoc"));
			AssertEquals("eDoc", contact.NotifyModes.GetDescriptionFromCode("eDoc"));

			contact = new DocDeliveryContact(Factory);
			menuItem.SU_BusinessContext = Constants.BusinessContextPrefixes.Reports;
			contact.Initialise(menuItem, documentDeliveryType);
			AssertEquals("Should NOT contains eDoc delivery method for Report", false, contact.NotifyModes.ContainsCode("eDoc"));
		}

		public void TestEmailToRecipients()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			for (int i = 1; i <= 5; i++)
			{
				var emailToRecipient = new NonPersistentCopyRecipient(org, Core.Constants.CopyRecipientType.EmailToRecipient) { EmailAddress = "unit.test" + i + "@cargowise.com" };
				contact.EmailToRecipients.Add(emailToRecipient);
			}

			AssertEquals("DocDeliveryContact.Email should return correct emails.", "unit.test1@cargowise.com, unit.test2@cargowise.com, unit.test3@cargowise.com, unit.test4@cargowise.com, unit.test5@cargowise.com", contact.Email);
		}

		public void TestEmailStaysInSyncWithEmailToRecipients()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			DocDeliveryContact contact = new DocDeliveryContact(Factory);

			// Add emails
			contact.Email = "unit.test1@cargowise.com, unit.test2@cargowise.com";

			// Check correct emails returned
			AssertEquals("DocDeliveryContact.Email should return correct emails.", "unit.test1@cargowise.com, unit.test2@cargowise.com", contact.Email);

			// Add an email directly to the collection
			var emailToRecipient3 = new NonPersistentCopyRecipient(org, Core.Constants.CopyRecipientType.EmailToRecipient) { EmailAddress = "unit.test3@cargowise.com" };
			contact.EmailToRecipients.Add(emailToRecipient3);

			// Check new email added to the collection
			AssertEquals("DocDeliveryContact.Email should return correct emails.", contact.Email, "unit.test1@cargowise.com, unit.test2@cargowise.com, unit.test3@cargowise.com");
		}

		public void TestAddressUpdatedWhenChangingNotifyMode()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Main";

			OrgAddress postalAddress = org.Addresses.AddNew();
			postalAddress.AddressCapability.DisableAllCapabilities();
			postalAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Postal.Code);
			postalAddress.OA_Address1 = "Postal";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John";
			DocDeliveryContact deliveryContact = contact.DocDeliveryDetails();

			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.Receivables.Code;
			deliveryContact.Initialise(menuItem, null);

			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Address should be main address", "Main", deliveryContact.Address1);

			contact.WorkingAddressPK = postalAddress.PK;
			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("Address should be postal address", "Postal", deliveryContact.Address1);

			deliveryContact.Name = "Mary";
			AssertEquals("Address should stay as postal", "Postal", deliveryContact.Address1);

			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Address should now be main address again", "Main", deliveryContact.Address1);
		}

		public void TestAddressUpdatedWhenChangingNotifyModeWithOveriddenAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Main";

			DocDeliveryContact deliveryContact = org.Contacts.AddNew().DocDeliveryDetails();

			deliveryContact.Initialise(null, Factory.New<JobDocAddress>(), null, null, ZString.Empty, false, null, ZString.Empty);

			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Address should now be main address again", "Main", deliveryContact.Address1);

			deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("Address should now be main address again", "Main", deliveryContact.Address1);
		}

		public void TestEDocDefaultAttachmentType()
		{
			AssertEDocDefaultAttachmentType(Constants.FileFormats.PDF);
			AssertEDocDefaultAttachmentType(Constants.FileFormats.PDFA);
			AssertEDocDefaultAttachmentType(Constants.FileFormats.TIF);

			void AssertEDocDefaultAttachmentType(string value)
			{
				var dummyContact = new DocDeliveryContact(Factory);

				using (SystemDataRegistry.Instance.EDocImportFileFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					dummyContact.DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
					AssertEquals(value, dummyContact.AttachmentType);
				}
			}
		}

		public void TestDefaultAttachmentType()
		{
			AssertDefaultAttachmentType(Core.Constants.ContactNotifyModes.Email);
			AssertDefaultAttachmentType(Core.Constants.ContactNotifyModes.EPrint);
			AssertDefaultAttachmentTypeWithMenuItem(Core.Constants.ContactNotifyModes.Email);
			AssertDefaultAttachmentTypeWithMenuItem(Core.Constants.ContactNotifyModes.EPrint);
		}

		void AssertDefaultAttachmentType(string deliveryMethod)
		{
			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("PDF is default", OrgConstants.AttachmentType.PDF, contact.AttachmentType);

			contact = new DocDeliveryContact(Factory);
			contact.AttachmentTypes = ListWith1Type;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("XYZ is default as it's the only item", "XYZ", contact.AttachmentType);

			contact = new DocDeliveryContact(Factory);
			contact.AttachmentTypes = ListWithNoPDF;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("First item in list is default", "DAP", contact.AttachmentType);
		}

		void AssertDefaultAttachmentTypeWithMenuItem(string deliveryMethod)
		{
			TestOrgHeader.Contacts.RemoveAll();

			var orgContact = TestOrgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "Zubin Appoo";
			orgContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			orgContact.OC_AttachmentType = "PDFA";
			orgContact.OC_Salutation = "Zubs";

			//MenuItem is null
			contact = new DocDeliveryContact(Factory);
			contact.Initialise(null, null);
			contact.OrgHeaderPK = TestOrgHeader.PK;
			contact.Name = "Zubin Appoo";
			contact.DeliveryMethod = String.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("PDFA is default", OrgConstants.AttachmentType.PDFA, contact.AttachmentType);

			//MenuItem is not null
			var menuItem = Factory.New<StmMenuItem>();
			contact = new DocDeliveryContact(Factory);
			contact.Initialise(menuItem, null);
			contact.OrgHeaderPK = TestOrgHeader.PK;

			// -- Contact is not null 
			contact.Name = "Zubin Appoo";

			menuItem.SU_DefaultAttachmentType = OrgConstants.AttachmentType.XLS;
			contact.DeliveryMethod = String.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("PDFA is default because Contact's AttachmentType is not null ", OrgConstants.AttachmentType.PDFA, contact.AttachmentType);

			orgContact.OC_AttachmentType = string.Empty;
			menuItem.SU_DefaultAttachmentType = OrgConstants.AttachmentType.XLS;
			contact.DeliveryMethod = String.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("XLS is default because Contact's AttachmentType is null ", OrgConstants.AttachmentType.XLS, contact.AttachmentType);

			// -- Contact is null
			contact.Name = string.Empty;

			menuItem.SU_DefaultAttachmentType = OrgConstants.AttachmentType.XLS;
			contact.DeliveryMethod = string.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("XLS is default because Contact is null", OrgConstants.AttachmentType.XLS, contact.AttachmentType);

			menuItem.SU_DefaultAttachmentType = OrgConstants.AttachmentType.FIL;
			contact.DeliveryMethod = String.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("PDF is default because FIL not int the AttachmentType list", OrgConstants.AttachmentType.PDF, contact.AttachmentType);

			menuItem.SU_DefaultAttachmentType = string.Empty;
			contact.DeliveryMethod = String.Empty;
			contact.DeliveryMethod = deliveryMethod;
			AssertEquals("PDF is default", OrgConstants.AttachmentType.PDF, contact.AttachmentType);
		}

		CodeDescriptionPairList ListWith1Type
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("XYZ", "XYZ");
				return list;
			}
		}

		CodeDescriptionPairList ListWithNoPDF
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("DAP", "DAP");
				list.AddPair("XYZ", "XYZ");
				return list;
			}
		}

		public void TestOverrideAttachmentTypes()
		{
			var contacts = new DocDeliveryContactCollection(Factory);
			DocDeliveryContact contact = contacts.AddNew();
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDF));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDFA));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.PDFC));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLS));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.XLSX));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.TIF));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.HTML));
			Assert("Contact has normal attachment types", contact.AttachmentTypes.ContainsCode(OrgConstants.AttachmentType.HTMF));

			contacts.OverrideAttachmentTypeListOnChildren(OverriddenList);

			AssertEquals("Existing Contact has the new attachment types", 2, contact.AttachmentTypes.Count);
			Assert("Existing Contact has new attachment types", contact.AttachmentTypes.ContainsCode("Daph"));
			Assert("Existing Contact has new attachment types", contact.AttachmentTypes.ContainsCode("Zubs"));
		}

		public void TestDeliveryAddress()
		{
			var ePrinterAddress = "email@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);

			var contact = new DocDeliveryContact(Factory);
			contact.Email = "something@example.com";
			contact.Fax = "90251199";
			AssertEquals("Delivery address blank", "", contact.DeliveryAddress);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Delivery address is email", "something@example.com", contact.DeliveryAddress);
			AssertEquals("Delivery address not readonly for email", false, contact.DeliveryAddressInfo.ReadOnly);
			AssertEquals("Attachment Type not readonly for email", false, contact.AttachmentTypeInfo.ReadOnly);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			AssertEquals("Delivery address is ePrint email address", ePrinterAddress, contact.DeliveryAddress);
			AssertEquals("Delivery address readonly for ePrint", true, contact.DeliveryAddressInfo.ReadOnly);
			AssertEquals("Attachment Type not readonly for ePrint", false, contact.AttachmentTypeInfo.ReadOnly);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Delivery address fax", "90251199", contact.DeliveryAddress);
			AssertEquals("Delivery address not readonly for fax", false, contact.DeliveryAddressInfo.ReadOnly);
			AssertEquals("Attachment Type readonly for fax", true, contact.AttachmentTypeInfo.ReadOnly);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
			AssertEquals("Delivery address blank for print", "", contact.DeliveryAddress);
			AssertEquals("Delivery address readonly for print", true, contact.DeliveryAddressInfo.ReadOnly);
			AssertEquals("Attachment Type readonly for print", true, contact.AttachmentTypeInfo.ReadOnly);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("Delivery address fax", "90251199", contact.DeliveryAddress);
			contact.DeliveryAddress = "11112222";
			AssertEquals("Fax updated", "11112222", contact.Fax);
			AssertEquals("Delivery address fax", "11112222", contact.DeliveryAddress);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Delivery address email", "something@example.com", contact.DeliveryAddress);
			contact.DeliveryAddress = "goaway@example.com";
			AssertEquals("Email updated", "goaway@example.com", contact.Email);
			AssertEquals("Delivery address email", "goaway@example.com", contact.DeliveryAddress);
		}

		public void TestEPrintEmailAddress()
		{
			var ePrinterAddress = "email@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);
			var contact = new DocDeliveryContact(Factory);

			AssertEquals(ePrinterAddress, contact.EPrintEmail);

			ePrinterAddress = "email2@printer.com";
			DocumentsDataRegistry.Instance.EPrintEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrinterAddress);
			AssertEquals(ePrinterAddress, contact.EPrintEmail);
		}

		public void TestDeliveryMethodDescription()
		{
			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			contact.DeliveryMethod = "ABC";
			AssertEquals("Description is blank", "", contact.DeliveryMethodDescription);

			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("Description is Email", "E-Mail", contact.DeliveryMethodDescription);

			contact.DeliveryMethodDescription = "Fax";
			AssertEquals("Code is set correctly", "FAX", contact.DeliveryMethod);

			contact.DeliveryMethodDescription = "Zubin";
			AssertEquals("Code is empty", "", contact.DeliveryMethod);
			contact.DeliveryMethod_ReadOnly = true;
			Assert("Description readonly", contact.DeliveryMethodDescriptionInfo.ReadOnly);
			contact.DeliveryMethodDescription_ReadOnly = false;
			Assert("Code not readonly", !contact.DeliveryMethodInfo.ReadOnly);
		}

		public void TestSettingOrgPKSetsCompanyName()
		{
			OrgHeader newOrg = OrgHeader.New(Factory);
			newOrg.OH_FullName = "Zubins Org 123";
			newOrg.OH_Code = "ZUB";
			newOrg.MainAddress.OA_Address1 = "hello street";
			Factory.Save();

			DocDeliveryContact contact = new DocDeliveryContact(Factory);
			AssertEquals("Org name not set", "", contact.CompanyName);

			contact.OrgHeaderPK = ZGuid.NewZGuid();
			AssertEquals("Org name not set", "", contact.CompanyName);

			contact.OrgHeaderPK = newOrg.PK;
			AssertEquals("Org name set", "Zubins Org 123", contact.CompanyName);
		}

		public void TestSettingContactPKSetsContactDetails()
		{
			OrgHeader newOrg = OrgHeader.New(Factory);
			newOrg.OH_Code = "ZUB";
			newOrg.MainAddress.OA_Address1 = "hello street";
			OrgContact contact1 = newOrg.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin Appoo";
			contact1.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact1.OC_AttachmentType = "TIF";
			contact1.OC_Salutation = "Zubs";

			Factory.Save();

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			AssertEquals("Contact name not set", "", deliveryContact.Name);
			AssertEquals("Contact salutation not set", "", deliveryContact.Salutation);
			AssertEquals("Delivery Mode not set", "", deliveryContact.DeliveryMethod);
			AssertEquals("Attachment Type not set", "", deliveryContact.AttachmentType);

			deliveryContact.OrgHeaderPK = newOrg.PK;

			deliveryContact.Name = "Mary Jane";
			AssertEquals("Contact name set", "Mary Jane", deliveryContact.Name);
			AssertEquals("Contact salutation not set", "", deliveryContact.Salutation);
			AssertEquals("Delivery Mode not set", "", deliveryContact.DeliveryMethod);
			AssertEquals("Attachment Type not set", "", deliveryContact.AttachmentType);

			deliveryContact.Name = "Zubin Appoo";
			AssertEquals("Contact name set", "Zubin Appoo", deliveryContact.Name);
			AssertEquals("Contact set", contact1.PK, deliveryContact.Contact.PK);
			AssertEquals("Contact salutation set", "Zubs", deliveryContact.Salutation);
			AssertEquals("Delivery Mode set", Core.Constants.ContactNotifyModes.Email, deliveryContact.DeliveryMethod);
			AssertEquals("Attachment Type set", "TIF", deliveryContact.AttachmentType);
		}

		public void TestContactWithAddressOverrideWillNotChangeOrgHeaderPK()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var orgB = Factory.NewWithValidTestData<OrgHeader>();

			var addressA = orgA.Addresses.AddNew();
			addressA.OA_Address1 = "123 test street";

			var addressB = orgB.Addresses.AddNew();
			addressB.OA_Address1 = "456 test street";

			var contact = orgA.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_OH_AddressOverride = orgA.PK;
			contact.OC_OA_OrgAddress = addressB.PK;

			Factory.Save();

			var deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.DeliveryMethod = ZString.Empty;
			deliveryContact.OrgAddressPK = addressA.PK;
			deliveryContact.OrgHeaderPK = orgA.PK;

			deliveryContact.Name = contact.Name;

			AssertEquals("Choose contact with address override should change current OrgAddressPK", addressB.PK, deliveryContact.OrgAddressPK);
			AssertEquals("Choose contact with address override should not change current OrgHeaderPK", orgA.PK, deliveryContact.OrgHeaderPK);
		}

		#region TestContactIsCachedCorrectly

		public void TestContactIsCachedCorrectly()
		{
			// Arrange
			var org = OrgHeader.New(Factory);
			org.OH_Code = "YSZ";
			org.MainAddress.OA_Address1 = "Heaven Road";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Stone Zhou";
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact.OC_AttachmentType = "TIF";
			contact.OC_Salutation = "Yeap";
			var deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.OrgHeaderPK = org.PK;
			deliveryContact.Name = "Stone Zhou";
			Factory.Save();
			factoryRowsLoadedCounterForTestContactIsCachedCorrectly = 0;
			Factory.RowsLoaded += Factory_RowsLoaded_ForTestContactIsCachedCorrectly;

			// Act
			AssertEquals("Contact set", contact.PK, deliveryContact.Contact.PK);
			var currentFactoryRowsLoadedCounter = factoryRowsLoadedCounterForTestContactIsCachedCorrectly;
			AssertEquals("Contact set", contact.PK, deliveryContact.Contact.PK);

			// Assert
			AssertEquals("No more Factory hits", currentFactoryRowsLoadedCounter, factoryRowsLoadedCounterForTestContactIsCachedCorrectly);
		}

		void Factory_RowsLoaded_ForTestContactIsCachedCorrectly(object sender, RowsLoadedEventArgs e)
		{
			factoryRowsLoadedCounterForTestContactIsCachedCorrectly++;
		}

		int factoryRowsLoadedCounterForTestContactIsCachedCorrectly;

		#endregion

		public void TestSalutaionMessageWhenItIsExceedOfMaxLength()
		{
			string value = "This is over 50 salutation for test, This is over 50 salutation for test, This is over 50 salutation for test";
			Contact.Salutation = value;
			Factory.Save();
			try
			{
				AssertEquals("Provided Salutation exceeds " + Contact.SalutationInfo.MaxLength + " characters. " + value.Length + " characters were entered. Salutation: " + value, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestContacts()
		{
			OrgHeader newOrg = OrgHeader.New(Factory);
			newOrg.OH_Code = "ZUB";
			newOrg.MainAddress.OA_Address1 = "hello street";

			OrgContact contact1 = newOrg.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin Appoo";
			contact1.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact1.OC_AttachmentType = "TIF";
			contact1.OC_Salutation = "Zubs";

			OrgContact contact2 = newOrg.Contacts.AddNew();
			contact2.OC_ContactName = "Inactive Dead Man";
			contact2.OC_IsActive = false;
			contact2.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;

			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.OrgHeaderPK = newOrg.PK;
			AssertEquals("There should be only one contact in list", 1, deliveryContact.Contacts.Count);
			AssertEquals("Contact is Contact 1", contact1, deliveryContact.Contacts[0]);

			OrgContact contact3 = newOrg.Contacts.AddNew();
			contact3.OC_ContactName = "Rick";
			contact3.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact3.OC_AttachmentType = "TIF";
			contact3.OC_Salutation = "Senor";
			AssertEquals("There should be two contacts in list", 2, deliveryContact.Contacts.Count);
			AssertEquals("First Contact is Contact 1(Zubin Appoo)", contact1, deliveryContact.Contacts[0]);
			AssertEquals("Second Contact is Contact 3(Rick)", contact3, deliveryContact.Contacts[1]);

			contact1.Delete();
			contact3.Delete();
			AssertEquals("There are no contacts in list", 0, deliveryContact.Contacts.Count);

			contact2.OC_IsActive = true;
			AssertEquals("There should be 1 contact in the list", 1, deliveryContact.Contacts.Count);
		}

		public void TestSalutationFieldMaxLength()
		{
			// Should also be the same as OrgContact fields, but this hasn't been done yet
			var contact = new DocDeliveryContact(Factory);
			AssertEquals(100, contact.SalutationInfo.MaxLength);
		}

		public void TestIsDeliveryAddressDifferentFromContact()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Contact.DeliveryAddress = "x";
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.OrgHeaderPK = TestOrgHeader.PK;
			Contact.Name = TestOrgContact.OC_ContactName;
			AssertEquals("DeliveryAddress", "TestOrgContact@TestOrgHeader.com", Contact.DeliveryAddress);
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryAddress = "x@x.com";
			AssertEquals("IsDeliveryAddressDifferentFromContact", true, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryAddress = "TestOrgContact@TestOrgHeader.com";
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("DeliveryAddress", "+61 2 9025 1199", Contact.DeliveryAddress);
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryAddress = "+61290251100";
			AssertEquals("IsDeliveryAddressDifferentFromContact", true, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryAddress = "+61290251199";
			AssertEquals("DeliveryAddress", "+61 2 9025 1199", Contact.DeliveryAddress);
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			Contact.DeliveryAddress = "+61290251199";
			AssertEquals("DeliveryAddress", "+61290251199", Contact.DeliveryAddress);
			AssertEquals("IsDeliveryAddressDifferentFromContact", false, Contact.IsDeliveryAddressDifferentFromContact);

			Contact.DeliveryAddress = "x";
			AssertEquals("IsDeliveryAddressDifferentFromContact", true, Contact.IsDeliveryAddressDifferentFromContact);
		}

		public void TestContactDeliveryAddress()
		{
			AssertEquals("ContactDeliveryAddress", "", Contact.ContactDeliveryAddress);

			Contact.OrgHeaderPK = TestOrgHeader.PK;
			Contact.Name = TestOrgContact.OC_ContactName;

			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("ContactDeliveryAddress", "TestOrgContact@TestOrgHeader.com", Contact.ContactDeliveryAddress);

			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("ContactDeliveryAddress", "+61290251199", Contact.ContactDeliveryAddress);
		}

		public void TestEmailAndFaxValidatesDeliveryAddress()
		{
			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			Contact.DeliveryAddress = "";
			AssertHasErrors(Contact.DeliveryAddressInfo);
			Contact.Email = "bob@bob.com";
			AssertNoErrors(Contact.DeliveryAddressInfo);

			Contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			Contact.DeliveryAddress = "";
			AssertHasErrors(Contact.DeliveryAddressInfo);
			Contact.Fax = "+61290251199";
			AssertNoErrors(Contact.DeliveryAddressInfo);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			Contact.Fax = "+6129025119";
			AssertEquals("+6129025119", Contact.Fax);
			AssertHasErrors(Contact.DeliveryAddressInfo);
		}

		public void TestSystemDefaultContactSalutation()
		{
			StmMenuItem recMenuItem = Factory.New<StmMenuItem>();
			recMenuItem.SU_ContactType = ContactType.Receivables.Code;
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DocAutoDelivery autoDelivery = new DocAutoDelivery();
			DocDeliveryContactCollection contacts = autoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company));

			AssertEquals(1, contacts.Count);
			AssertEquals(DefaultSalutationProvider.GetDefaultSalutation(Enterprise.Core.Constants.Languages.EnglishAmerican, string.Empty).Replace(Core.Constants.SalutationMacros.Name, contacts[0].Name), contacts[0].Salutation);
			AssertEquals(ContactType.Receivables.DefaultName, contacts[0].SystemDefaultContactName);
		}

		public void TestSalutationList()
		{
			StmMenuItem recMenuItem = Factory.New<StmMenuItem>();
			recMenuItem.SU_ContactType = ContactType.Receivables.Code;
			recMenuItem.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

			OrgHeader company = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DocAutoDelivery autoDelivery = new DocAutoDelivery();
			DocDeliveryContactCollection contacts = autoDelivery.GetDeliveryContacts(recMenuItem, new AutoDeliveryBizO(company));

			AssertEquals(1, contacts.Count);
			Assert(contacts[0].Salutations.ContainsCode(DefaultSalutationProvider.GetDefaultSalutation(Constants.Languages.EnglishAmerican, string.Empty).Replace(Constants.SalutationMacros.Name, contacts[0].Name)));
			contacts[0].DeliveryLanguage = Core.Constants.Languages.ChineseSimplified;
			Assert(contacts[0].Salutations.ContainsCode(DefaultSalutationProvider.GetDefaultSalutation(Constants.Languages.ChineseSimplified, string.Empty).Replace(Constants.SalutationMacros.Name, contacts[0].Name)));
		}

		public void TestSalutationMatchesCorrectMaleFemaleGender()
		{
			var contactWithEmptyGender = Factory.NewWithValidTestData<OrgContact>();
			contactWithEmptyGender.OC_ContactName = "EmptyGenderContact";
			contactWithEmptyGender.OC_Gender = "";

			var contactWithMaleGender = Factory.NewWithValidTestData<OrgContact>();
			contactWithMaleGender.OC_ContactName = "MaleContact";
			contactWithMaleGender.OC_Gender = "M";

			var contactWithFemaleGender = Factory.NewWithValidTestData<OrgContact>();
			contactWithFemaleGender.OC_ContactName = "FemaleContact";
			contactWithFemaleGender.OC_Gender = "F";
			Factory.Save();

			var deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.Name = contactWithEmptyGender.Name;
			var allSalutations = deliveryContact.Salutations.ToArray();

			deliveryContact.Name = contactWithMaleGender.Name;
			var maleSalutations = deliveryContact.Salutations.ToArray().Where(m => !allSalutations.Contains(m));

			deliveryContact.Name = contactWithFemaleGender.Name;
			var femaleSalutations = deliveryContact.Salutations.ToArray().Where(f => !allSalutations.Contains(f));

			AssertEquals(true, maleSalutations.Any());
			AssertEquals(true, femaleSalutations.Any());
			AssertEquals(false, femaleSalutations.Any(f => maleSalutations.Contains(f)));
		}

		public void TestFax_MaxCharactersShouldBeSameAsOrgContactEmailField()
		{
			var orgContactEmailMaxLength = OrgContactSchema.OC_Email.MaxLength;
			AssertEquals(orgContactEmailMaxLength, Contact.FaxInfo.MaxLength);
		}

		public void TestAddressFieldMaxLength()
		{
			// Should also be the same as OrgContact fields, but this hasn't been done yet
			var contact = new DocDeliveryContact(Factory);
			AssertEquals(60, contact.Address1Info.MaxLength);
		}

		public void TestOrgHeaderPKSetsOrgAddressPKWithLanguagePreference()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "OH_Code";

			var mainAddressEnglish = organisation.Addresses.AddNew();
			mainAddressEnglish.OA_Address1 = "Main Address";
			mainAddressEnglish.AddressCapability.SetIsMainAddress(OrgAddressType.Office);

			var mainAddressChinese = organisation.Addresses.AddNew();
			mainAddressChinese.OA_Language = Core.Constants.Languages.ChineseSimplified;
			mainAddressChinese.OA_Address1 = "主地址";
			mainAddressChinese.AddressCapability.SetIsMainAddress(OrgAddressType.Office);

			var contact = new DocDeliveryContact(Factory);
			contact.DeliveryLanguage = Core.Constants.Languages.ChineseSimplified;
			contact.OrgHeaderPK = organisation.PK;

			AssertEquals("contact.Address1", "主地址", contact.Address1);

			AssertEquals(organisation.PK, contact.OrgHeaderPK);
			AssertEquals(mainAddressChinese.PK, contact.OrgAddressPK);
		}

		public void TestEmailRecipientsOrganization()
		{
			var organisation1 = Factory.New<OrgHeader>();
			organisation1.OH_Code = "OH1";
			var organisation2 = Factory.New<OrgHeader>();
			organisation2.OH_Code = "OH2";

			Contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			Contact.OrgHeaderPK = organisation1.PK;
			AssertEquals(organisation1, Contact.EmailBlindCarbonCopyRecipients.Organization);
			AssertEquals(organisation1, Contact.EmailCarbonCopyRecipients.Organization);
			AssertEquals(organisation1, Contact.EmailToRecipients.Organization);
			Contact.OrgHeaderPK = organisation2.PK;
			AssertEquals(organisation2, Contact.EmailBlindCarbonCopyRecipients.Organization);
			AssertEquals(organisation2, Contact.EmailCarbonCopyRecipients.Organization);
			AssertEquals(organisation2, Contact.EmailToRecipients.Organization);
		}

		public void TestEmailCarbonCopyRecipientsAsString()
		{
			// Arrange
			Contact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			// Act
			var isReadOnly = Contact.EmailCarbonCopyRecipientsAsStringInfo.ReadOnly;
			// Assert
			Assert(isReadOnly);

			// Arrange
			Contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Contact.EmailCarbonCopyRecipientsAsStringInfo.ReadOnly;
			Contact.EmailCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			var emailCarbonCopyRecipients = Contact.EmailCarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals(2, emailCarbonCopyRecipients.Count);
			AssertCollectionContains(emailCarbonCopyRecipients, eccr => ((NonPersistentCopyRecipient)eccr).EmailAddress == "test1@test.com");
			AssertCollectionContains(emailCarbonCopyRecipients, eccr => ((NonPersistentCopyRecipient)eccr).EmailAddress == "test2@test.com");
		}

		public void TestEmailBlindCarbonCopyRecipientsAsString()
		{
			// Arrange
			Contact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			// Act
			var isReadOnly = Contact.EmailBlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			// Assert
			Assert(isReadOnly);

			// Arrange
			Contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			// Act
			isReadOnly = Contact.EmailBlindCarbonCopyRecipientsAsStringInfo.ReadOnly;
			Contact.EmailBlindCarbonCopyRecipientsAsString = "test1@test.com, test2@test.com";
			var emailBlindCarbonCopyRecipients = Contact.EmailBlindCarbonCopyRecipients;
			// Assert
			Assert(!isReadOnly);
			AssertEquals(2, emailBlindCarbonCopyRecipients.Count);
			AssertCollectionContains(emailBlindCarbonCopyRecipients, eccr => ((NonPersistentCopyRecipient)eccr).EmailAddress == "test1@test.com");
			AssertCollectionContains(emailBlindCarbonCopyRecipients, eccr => ((NonPersistentCopyRecipient)eccr).EmailAddress == "test2@test.com");
		}

		public void TestUpdateCcAndBccEmailDetailsWhenSettingName()
		{
			var deliveryContact = CreateNewDeliveryContact();
			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			deliveryContact.AttachmentType = OrgConstants.AttachmentType.PDF;

			AssertNullOrEmpty(deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertNullOrEmpty(deliveryContact.EmailBlindCarbonCopyRecipientsAsString);

			deliveryContact.Name = "Name1";
			AssertContainsExactElementsInAnyOrder(new List<string>() { "cc1@contact1.com", "cc2@contact1.com" }, deliveryContact.EmailCarbonCopyRecipientsAsString.Split(", "));
			AssertContainsExactElementsInAnyOrder(new List<string>() { "bcc1@contact1.com", "bcc2@contact1.com" }, deliveryContact.EmailBlindCarbonCopyRecipientsAsString.Split(", "));

			deliveryContact.Name = "Name2";
			AssertEquals(deliveryContact.EmailCarbonCopyRecipientsAsString, "cc2@contact2.com");
			AssertEquals(deliveryContact.EmailBlindCarbonCopyRecipientsAsString, "bcc2@contact2.com");

			deliveryContact.Name = "Name3";
			AssertNullOrEmpty(deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertNullOrEmpty(deliveryContact.EmailBlindCarbonCopyRecipientsAsString);

			deliveryContact.Name = "Name4";
			AssertNullOrEmpty(deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertNullOrEmpty(deliveryContact.EmailBlindCarbonCopyRecipientsAsString);
		}

		public void TestUpdateCcAndBccEmailDetailsWhenSettingDeliveryMethod()
		{
			var deliveryContact = CreateNewDeliveryContact();
			deliveryContact.Name = "Name1";
			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			deliveryContact.AttachmentType = OrgConstants.AttachmentType.PDF;

			AssertContains("cc1@contact1.com", deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertContains("cc2@contact1.com", deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertContains("bcc1@contact1.com", deliveryContact.EmailBlindCarbonCopyRecipientsAsString);
			AssertContains("bcc2@contact1.com", deliveryContact.EmailBlindCarbonCopyRecipientsAsString);

			deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			AssertNullOrEmpty(deliveryContact.EmailCarbonCopyRecipientsAsString);
			AssertNullOrEmpty(deliveryContact.EmailBlindCarbonCopyRecipientsAsString);
		}

		public void TestEmailFromAddressWithTypeList()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "main@test.com";
			var emailAddresses = GlbStaff.CurrentUser.EmailAddresses;

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "WTG";
			company.CompanyName = "WiseTech";

			Factory.Save();

			var typeList = new CodeDescriptionPairList();
			typeList.AddPair("IMP", "Import");
			typeList.AddPair("EXP", "Export");

			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			using (SystemDataRegistry.Instance.StaffEmailTypeList.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, typeList))
			{
				var emailAddress1 = emailAddresses.AddNew();
				emailAddress1.GSE_GC_Company = company.PK;
				emailAddress1.GSE_EmailAddress = "import@wtg.com";
				emailAddress1.GSE_Type = "IMP";

				var emailAddress2 = emailAddresses.AddNew();
				emailAddress2.GSE_GC_Company = company.PK;
				emailAddress2.GSE_EmailAddress = "export@wtg.com";
				emailAddress2.GSE_Type = "EXP";

				var emailAddress3 = emailAddresses.AddNew();
				emailAddress3.GSE_EmailAddress = "import@bar.com";
				emailAddress3.GSE_Type = "IMP";

				var emailAddress4 = emailAddresses.AddNew();
				emailAddress4.GSE_EmailAddress = "export@bar.com";
				emailAddress4.GSE_Type = "EXP";

				Contact.DeliveryMethod = Constants.ContactNotifyModes.Email;

				AssertEquals(5, Contact.EmailFromAddressWithTypeList.Count);
				AssertEquals("Main - main@test.com", Contact.EmailFromAddressWithTypeList[0].Code);
				AssertEquals("main@test.com", Contact.EmailFromAddressWithTypeList[0].Description);
				AssertEquals("Export (WTG) - export@wtg.com", Contact.EmailFromAddressWithTypeList[1].Code);
				AssertEquals("export@wtg.com", Contact.EmailFromAddressWithTypeList[1].Description);
				AssertEquals("Export - export@bar.com", Contact.EmailFromAddressWithTypeList[2].Code);
				AssertEquals("export@bar.com", Contact.EmailFromAddressWithTypeList[2].Description);
				AssertEquals("Import (WTG) - import@wtg.com", Contact.EmailFromAddressWithTypeList[3].Code);
				AssertEquals("import@wtg.com", Contact.EmailFromAddressWithTypeList[3].Description);
				AssertEquals("Import - import@bar.com", Contact.EmailFromAddressWithTypeList[4].Code);
				AssertEquals("import@bar.com", Contact.EmailFromAddressWithTypeList[4].Description);
				Assert(Contact.EmailFromAddress.IsEmpty);

				Contact.EmailFromAddressWithType = "XXX";

				AssertEquals("Wrong email address will be shown", "XXX", Contact.EmailFromAddressWithType);
				AssertEquals("Wrong email address will be set", "XXX", Contact.EmailFromAddress);

				Contact.DefaultEmailFromAddress = "main@test.com";
				Contact.ClearEmailFromAddressList();
				Contact.EmailFromAddressWithType = ZString.Empty;

				AssertEquals(5, Contact.EmailFromAddressWithTypeList.Count);
				AssertEquals("Default [Main] - main@test.com", Contact.EmailFromAddressWithTypeList[0].Code);
				AssertEquals("main@test.com", Contact.EmailFromAddressWithTypeList[0].Description);
				AssertEquals("Default [Main] - main@test.com", Contact.EmailFromAddressWithType);
				AssertEquals("main@test.com", Contact.EmailFromAddress);

				Contact.DefaultEmailFromAddress = "default@test.com";
				Contact.ClearEmailFromAddressList();
				Contact.EmailFromAddressWithType = ZString.Empty;

				AssertEquals(6, Contact.EmailFromAddressWithTypeList.Count);
				AssertEquals("Default - default@test.com", Contact.EmailFromAddressWithTypeList[0].Code);
				AssertEquals("default@test.com", Contact.EmailFromAddressWithTypeList[0].Description);
				AssertEquals("Main - main@test.com", Contact.EmailFromAddressWithTypeList[1].Code);
				AssertEquals("main@test.com", Contact.EmailFromAddressWithTypeList[1].Description);
				AssertEquals("Default - default@test.com", Contact.EmailFromAddressWithType);
				AssertEquals("default@test.com", Contact.EmailFromAddress);

				Contact.DefaultEmailFromAddress = "import@bar.com";
				Contact.ClearEmailFromAddressList();
				Contact.EmailFromAddressWithType = ZString.Empty;

				AssertEquals(5, Contact.EmailFromAddressWithTypeList.Count);
				AssertEquals("Default [Import] - import@bar.com", Contact.EmailFromAddressWithTypeList[0].Code);
				AssertEquals("import@bar.com", Contact.EmailFromAddressWithTypeList[0].Description);
				AssertEquals("Main - main@test.com", Contact.EmailFromAddressWithTypeList[1].Code);
				AssertEquals("main@test.com", Contact.EmailFromAddressWithTypeList[1].Description);
				AssertEquals("Export (WTG) - export@wtg.com", Contact.EmailFromAddressWithTypeList[2].Code);
				AssertEquals("export@wtg.com", Contact.EmailFromAddressWithTypeList[2].Description);
				AssertEquals("Export - export@bar.com", Contact.EmailFromAddressWithTypeList[3].Code);
				AssertEquals("export@bar.com", Contact.EmailFromAddressWithTypeList[3].Description);
				AssertEquals("Import (WTG) - import@wtg.com", Contact.EmailFromAddressWithTypeList[4].Code);
				AssertEquals("import@wtg.com", Contact.EmailFromAddressWithTypeList[4].Description);
				AssertEquals("Default [Import] - import@bar.com", Contact.EmailFromAddressWithType);
				AssertEquals("import@bar.com", Contact.EmailFromAddress);

				Contact.DefaultEmailFromAddress = "import@wtg.com";
				Contact.ClearEmailFromAddressList();
				Contact.EmailFromAddressWithType = ZString.Empty;

				AssertEquals(5, Contact.EmailFromAddressWithTypeList.Count);
				AssertEquals("Default [Import (WTG)] - import@wtg.com", Contact.EmailFromAddressWithTypeList[0].Code);
				AssertEquals("import@wtg.com", Contact.EmailFromAddressWithTypeList[0].Description);
				AssertEquals("Main - main@test.com", Contact.EmailFromAddressWithTypeList[1].Code);
				AssertEquals("main@test.com", Contact.EmailFromAddressWithTypeList[1].Description);
				AssertEquals("Export (WTG) - export@wtg.com", Contact.EmailFromAddressWithTypeList[2].Code);
				AssertEquals("export@wtg.com", Contact.EmailFromAddressWithTypeList[2].Description);
				AssertEquals("Export - export@bar.com", Contact.EmailFromAddressWithTypeList[3].Code);
				AssertEquals("export@bar.com", Contact.EmailFromAddressWithTypeList[3].Description);
				AssertEquals("Import - import@bar.com", Contact.EmailFromAddressWithTypeList[4].Code);
				AssertEquals("import@bar.com", Contact.EmailFromAddressWithTypeList[4].Description);
				AssertEquals("Default [Import (WTG)] - import@wtg.com", Contact.EmailFromAddressWithType);
				AssertEquals("import@wtg.com", Contact.EmailFromAddress);
			}
		}

		public void TestDocDeliveryContactStaffRecipient()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SSS";
			staff.GS_LoginName = "Tester";
			staff.GS_FullName = "full name";
			staff.GS_EmailAddress = "Test@test.com";
			Factory.Save();

			var docDeliveryContact = new DocDeliveryContact(Factory);
			docDeliveryContact.StaffCode = staff.GS_Code;

			AssertEquals(staff.PK, docDeliveryContact.Staff.PK);
			AssertEquals("Test@test.com", docDeliveryContact.Email);
			AssertEquals("full name", docDeliveryContact.Name);
			AssertEquals(true, docDeliveryContact.OrgHeaderPK_ReadOnly);
		}

		public void TestDocDeliveryContactOrgContact()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = "Tester";
			orgContact.OC_Email = "Test@test.com";
			Factory.Save();

			var docDeliveryContact = new DocDeliveryContact(Factory);
			docDeliveryContact.OrgHeaderPK = orgHeader.PK;
			docDeliveryContact.Name = orgContact.OC_ContactName;

			AssertEquals(orgContact.PK, docDeliveryContact.Contact.PK);
			AssertEquals("Test@test.com", docDeliveryContact.Email);
			AssertEquals(true, docDeliveryContact.StaffCode_ReadOnly);
		}

		public void TestDocDeliveryContactDeliveryRecipientType()
		{
			var docDeliveryContact = new DocDeliveryContact(Factory);
			docDeliveryContact.DeliveryRecipientType = "Contact";

			AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Contact, docDeliveryContact.DeliveryToTypeCode);
			AssertEquals(true, docDeliveryContact.StaffCode_ReadOnly);
			AssertEquals(false, docDeliveryContact.OrgHeaderPK_ReadOnly);

			docDeliveryContact.DeliveryRecipientType = "Staff";

			AssertEquals(ScheduledReportDeliveryRecipientConstants.RecipientType.Staff, docDeliveryContact.DeliveryToTypeCode);
			AssertEquals(false, docDeliveryContact.StaffCode_ReadOnly);
			AssertEquals(true, docDeliveryContact.OrgHeaderPK_ReadOnly);
		}

		public void TestSendIndividually()
		{
			var contact = new DocDeliveryContact(Factory);
			AssertEquals("SendIndividually", false, contact.SendIndividually);

			contact.DeliveryMethod = Constants.ContactNotifyModes.Print;
			AssertEquals("SendIndividually", true, contact.SendIndividually);

			contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			AssertEquals("SendIndividually", false, contact.SendIndividually);

			contact.SendIndividually = true;
			contact.DeliveryMethod = Constants.ContactNotifyModes.Print;
			AssertEquals("SendIndividually", true, contact.SendIndividually);

			contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			AssertEquals("SendIndividually", false, contact.SendIndividually);
		}

		public void TestSendIndividually_ReadOnly()
		{
			var contact = new DocDeliveryContact(Factory);

			contact.DeliveryMethod = Constants.ContactNotifyModes.Print;
			AssertEquals("SendIndividually_ReadOnly", true, contact.SendIndividually_ReadOnly);

			contact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			AssertEquals("SendIndividually_ReadOnly", false, contact.SendIndividually_ReadOnly);
		}

		DocDeliveryContact CreateNewDeliveryContact()
		{
			var organization = GenerateOrgHeaderWithCcAndBcc();

			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.All.Code;
			Factory.Save();

			var documentSupporter = new AutoDeliveryBizO(organization);
			var docAutoDelivery = new DocAutoDelivery();
			var docDeliveryContacts = docAutoDelivery.GetDeliveryContacts(menuItem, documentSupporter);

			var docDeliveryContact = docDeliveryContacts.AddNew();
			docDeliveryContact.OrgHeaderPK = organization.PK;

			return docDeliveryContact;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDeliveryContact(Factory);
		}

		CodeDescriptionPairList OverriddenList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair("Daph", "Daph");
				list.AddPair("Zubs", "Zubs");

				return list;
			}
		}

		DocDeliveryContact Contact
		{
			get
			{
				if (contact == null)
				{
					contact = new DocDeliveryContact(Factory);
				}
				return contact;
			}
		}

		OrgContact TestOrgContact
		{
			get
			{
				if (testOrgContact == null)
				{
					testOrgContact = TestOrgHeader.Contacts.AddNew();
					testOrgContact.OC_ContactName = "TestOrgContact";
					testOrgContact.OC_Email = "TestOrgContact@TestOrgHeader.com";
					testOrgContact.OC_Fax = "+61290251199";
				}
				return testOrgContact;
			}
		}

		OrgHeader TestOrgHeader
		{
			get
			{
				if (testOrgHeader == null)
				{
					testOrgHeader = Factory.New<OrgHeader>();
				}
				return testOrgHeader;
			}
		}

		OrgHeader GenerateOrgHeaderWithCcAndBcc()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = organization.Contacts.AddNew();
			contact1.OC_ContactName = "Name1";
			contact1.OC_Email = "email1@email.com";
			var document1ForContact1 = contact1.Documents.AddNew();
			document1ForContact1.OD_DocumentGroup = ContactType.All.Code;
			document1ForContact1.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document1ForContact1.OD_AttachmentType = OrgConstants.AttachmentType.PDF;
			document1ForContact1.OD_CarbonCopyRecipientsAsString = "cc1@contact1.com";
			document1ForContact1.OD_BlindCarbonCopyRecipientsAsString = "bcc1@contact1.com";
			var document2ForContact1 = contact1.Documents.AddNew();
			document2ForContact1.OD_DocumentGroup = ContactType.All.Code;
			document2ForContact1.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document2ForContact1.OD_AttachmentType = OrgConstants.AttachmentType.PDF;
			document2ForContact1.OD_CarbonCopyRecipientsAsString = "cc2@contact1.com";
			document2ForContact1.OD_BlindCarbonCopyRecipientsAsString = "bcc2@contact1.com";

			var contact2 = organization.Contacts.AddNew();
			contact2.OC_ContactName = "Name2";
			contact2.OC_Email = "email2@email.com";
			var document1ForContact2 = contact2.Documents.AddNew();
			document1ForContact2.OD_DocumentGroup = ContactType.All.Code;
			document1ForContact2.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document1ForContact2.OD_AttachmentType = OrgConstants.AttachmentType.XLSX;
			document1ForContact2.OD_CarbonCopyRecipientsAsString = "cc1@contact2.com";
			document1ForContact2.OD_BlindCarbonCopyRecipientsAsString = "bcc1@contact2.com";
			var document2ForContact2 = contact2.Documents.AddNew();
			document2ForContact2.OD_DocumentGroup = ContactType.All.Code;
			document2ForContact2.OD_DeliverBy = Constants.ContactNotifyModes.Email;
			document2ForContact2.OD_AttachmentType = OrgConstants.AttachmentType.PDF;
			document2ForContact2.OD_CarbonCopyRecipientsAsString = "cc2@contact2.com";
			document2ForContact2.OD_BlindCarbonCopyRecipientsAsString = "bcc2@contact2.com";

			var contact3 = organization.Contacts.AddNew();
			contact3.OC_ContactName = "Name3";
			contact3.OC_Email = "email3@email.com";

			return organization;
		}

		DocDeliveryContact contact;
		OrgContact testOrgContact;
		OrgHeader testOrgHeader;

		#endregion
	}
}
