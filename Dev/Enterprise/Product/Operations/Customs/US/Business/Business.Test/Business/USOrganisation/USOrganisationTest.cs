using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOrganisation))]
	public class USOrganisationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddressPKOverride()
		{
			var address1 = org1.Addresses.AddNew();
			address1.OA_Address1 = "Address 2";
			AssertEquals(2, org1.Addresses.Count);
			AssertEquals(1, org2.Addresses.Count);
			organisation.ZO_OH_Organisation = org1.PK;
			AssertEquals(org1.MainAddress.PK, organisation.ZO_OA_Address);
			organisation.ZO_OH_Organisation = org2.PK;
			AssertEquals(org2.MainAddress.PK, organisation.ZO_OA_Address);

			organisation.GetDefaultAddressPK = GetDefaultAddressPK;
			organisation.ZO_OH_Organisation = org1.PK;
			AssertEquals(address1.PK, organisation.ZO_OA_Address);
			organisation.ZO_OH_Organisation = org2.PK;
			AssertEquals(org2.MainAddress.PK, organisation.ZO_OA_Address);
		}

		ZGuid GetDefaultAddressPK(OrgHeader org)
		{
			return org.Addresses.Count > 1 ? org.Addresses[1].PK : org.MainAddress.PK;
		}

		public void TestContactOverride()
		{
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "CONTACT 2";
			contact1.OC_Phone = "9456 5685";
			org1.Contacts[0].OC_Phone = "9365 3528";
			org2.Contacts[0].OC_Phone = "5234 7892";

			AssertEquals(2, org1.Contacts.Count);
			AssertEquals(1, org2.Contacts.Count);
			organisation.ZO_OH_Organisation = org1.PK;
			AssertEquals("BOB SMITH", organisation.ZO_Contact);
			AssertEquals("93653528", organisation.ZO_Phone);
			organisation.ZO_OH_Organisation = org2.PK;
			AssertEquals("JOE BROWN", organisation.ZO_Contact);
			AssertEquals("52347892", organisation.ZO_Phone);

			organisation.GetDefaultContact = GetDefaultContact;
			organisation.ZO_OH_Organisation = org1.PK;
			AssertEquals("CONTACT 2", organisation.ZO_Contact);
			AssertEquals("94565685", organisation.ZO_Phone);
			organisation.ZO_OH_Organisation = org2.PK;
			AssertEquals("JOE BROWN", organisation.ZO_Contact);
			AssertEquals("52347892", organisation.ZO_Phone);
		}

		public void TestSynchroniseWithUnformattedPhoneNumber()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(true);

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "CONTACT 2";
			contact1.OC_Phone_Formatted = "02 9456 5685";
			organisation.GetDefaultContact = delegate
			{ return contact1; };
			organisation.ZO_OH_Organisation = org.PK;

			AssertEquals("PreCondition:Phone number formatted", "+61 2 9456 5685", contact1.OC_Phone_Formatted);
			AssertEquals("61294565685", organisation.ZO_Phone);
		}

		OrgContact GetDefaultContact(OrgHeader org)
		{
			return org.Contacts.Count > 1 ? org.Contacts[1] : org.Contacts[0];
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullOrganisationPKInfo()
		{
			var organisation = new USOrganisation(null, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.Consignee, IsSynchronizationEnable, Factory);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullAddressPKInfo()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, null, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.Consignee, IsSynchronizationEnable, Factory);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullContactInfo()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, null, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.Consignee, IsSynchronizationEnable, Factory);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullPhoneInfo()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.AddressOverrideInfo, null, ContactType.Consignee, IsSynchronizationEnable, Factory);
		}

#if NETFRAMEWORK
		[ExpectExceptionMessage(typeof(ArgumentException), "Value cannot be empty string (\"\").\r\nParameter name: docGroup")]
#else
		[ExpectExceptionMessage(typeof(ArgumentException), "Value cannot be empty string (\"\"). (Parameter 'docGroup')")]
#endif
		public void TestConstructorWithEmptyDocGroup()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, "", IsSynchronizationEnable, Factory);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullSynchronizationInvoker()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.Consignee, null, Factory);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullAddressOverrideInfo()
		{
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, null, ContactType.Consignee, null, Factory);
		}

		#region Synchronization of Wrapped Properties
		public void TestZO_OH_Organisation()
		{
			AssertEquals("PreCondition: DummyUSOrg.OrgPK", ZGuid.Empty, dummyUSOrg.OrgPK);
			AssertEquals("PreCondition: Organisation.ZO_OH_Organisation", ZGuid.Empty, organisation.ZO_OH_Organisation);
			enableForTesting = true;
			dummyUSOrg.OrgPK = org1.PK;
			AssertSynchronizeData(org1, dummyUSOrg, organisation, true);
			enableForTesting = false;
			dummyUSOrg.OrgPK = org2.PK;
			AssertDataNotSynchronizedForOrganisationPKChanges(org1, org2, dummyUSOrg, organisation, true);

			enableForTesting = true;
			organisation.ZO_OH_Organisation = org1.PK;
			AssertSynchronizeData(org1, dummyUSOrg, organisation, true);
			enableForTesting = false;
			organisation.ZO_OH_Organisation = org2.PK;
			AssertDataNotSynchronizedForOrganisationPKChanges(org1, org2, dummyUSOrg, organisation, true);
		}

		void AssertDataNotSynchronizedForOrganisationPKChanges(OrgHeader previousOrg, OrgHeader currentOrg, DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation, bool keepCountryCode)
		{
			AssertEquals("DummyUSOrg.OrgPK", currentOrg.PK, dummyUSOrg.OrgPK);
			AssertEquals("DummyUSOrg.OrgAddressPK", previousOrg.MainAddress.PK, dummyUSOrg.OrgAddressPK);
			AssertEquals("DummyUSOrg.OrgContact", previousOrg.Contacts[0].OC_ContactName, dummyUSOrg.OrgContact);
			AssertEquals("DummyUSOrg.OrgPhone", PhoneNumberCalculator.GetUnformattedPhoneNumber(previousOrg.Contacts[0].OC_Phone, keepCountryCode), dummyUSOrg.OrgPhone);
			AssertWrappedPropertiesAreUpdated(dummyUSOrg, organisation);
		}

		public void TestZO_OA_Address()
		{
			AssertEquals("PreCondition: DummyUSOrg.OrgAddressPK", ZGuid.Empty, dummyUSOrg.OrgAddressPK);
			AssertEquals("PreCondition: Organisation.ZO_OA_Address", ZGuid.Empty, organisation.ZO_OA_Address);
			enableForTesting = true;
			dummyUSOrg.OrgAddressPK = org1.MainAddress.PK;
			AssertSynchronizeData(org1, dummyUSOrg, organisation, true);
			enableForTesting = false;
			dummyUSOrg.OrgAddressPK = org2.MainAddress.PK;
			AssertDataNotSynchronizedForAddressPKChanges(org1, org2, dummyUSOrg, organisation, true);

			enableForTesting = true;
			organisation.ZO_OA_Address = org1.MainAddress.PK;
			AssertSynchronizeData(org1, dummyUSOrg, organisation, true);
			enableForTesting = false;
			organisation.ZO_OA_Address = org2.MainAddress.PK;
			AssertDataNotSynchronizedForAddressPKChanges(org1, org2, dummyUSOrg, organisation, true);
		}

		void AssertDataNotSynchronizedForAddressPKChanges(OrgHeader previousOrg, OrgHeader currentOrg, DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation, bool keepCountryCode)
		{
			AssertEquals("DummyUSOrg.OrgPK", previousOrg.PK, dummyUSOrg.OrgPK);
			AssertEquals("DummyUSOrg.OrgAddressPK", currentOrg.MainAddress.PK, dummyUSOrg.OrgAddressPK);
			AssertEquals("DummyUSOrg.OrgContact", previousOrg.Contacts[0].OC_ContactName, dummyUSOrg.OrgContact);
			AssertEquals("DummyUSOrg.OrgPhone", PhoneNumberCalculator.GetUnformattedPhoneNumber(previousOrg.Contacts[0].OC_Phone, keepCountryCode), dummyUSOrg.OrgPhone);
			AssertWrappedPropertiesAreUpdated(dummyUSOrg, organisation);
		}

		public void TestZO_Contact()
		{
			AssertEquals("PreCondition: DummyUSOrg.OrgContact", "", dummyUSOrg.OrgContact);
			AssertEquals("PreCondition: Organisation.ZO_Contact", "", organisation.ZO_Contact);
			enableForTesting = true;
			dummyUSOrg.OrgPK = org1.PK;
			dummyUSOrg.OrgContact = org2.Contacts[0].OC_ContactName;
			AssertZO_ContactChanges(org1, org2, dummyUSOrg, organisation, true);
			enableForTesting = false;
			dummyUSOrg.OrgContact = org1.Contacts[0].OC_ContactName;
			AssertZO_ContactChanges(org1, org1, dummyUSOrg, organisation, true);

			enableForTesting = true;
			organisation.ZO_Contact = org2.Contacts[0].OC_ContactName;
			AssertZO_ContactChanges(org1, org2, dummyUSOrg, organisation, true);
			enableForTesting = false;
			organisation.ZO_Contact = org1.Contacts[0].OC_ContactName;
			AssertZO_ContactChanges(org1, org1, dummyUSOrg, organisation, true);
		}

		void AssertZO_ContactChanges(OrgHeader previousOrg, OrgHeader currentOrg, DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation, bool keepCountryCode)
		{
			AssertEquals("DummyUSOrg.OrgPK", previousOrg.PK, dummyUSOrg.OrgPK);
			AssertEquals("DummyUSOrg.OrgAddressPK", previousOrg.MainAddress.PK, dummyUSOrg.OrgAddressPK);
			AssertEquals("DummyUSOrg.OrgContact", currentOrg.Contacts[0].OC_ContactName, dummyUSOrg.OrgContact);
			AssertEquals("DummyUSOrg.OrgPhone", PhoneNumberCalculator.GetUnformattedPhoneNumber(previousOrg.Contacts[0].OC_Phone, keepCountryCode), dummyUSOrg.OrgPhone);
			AssertWrappedPropertiesAreUpdated(dummyUSOrg, organisation);
		}

		public void TestZO_Phone()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			AssertEquals("PreCondition: DummyUSOrg.OrgPhone", "", dummyUSOrg.OrgPhone);
			AssertEquals("PreCondition: Organisation.ZO_Phone", "", organisation.ZO_Phone);
			enableForTesting = true;
			dummyUSOrg.OrgPK = org1.PK;
			dummyUSOrg.OrgPhone = org2.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org2, dummyUSOrg, organisation);
			enableForTesting = false;
			dummyUSOrg.OrgPhone = org1.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org1, dummyUSOrg, organisation);

			enableForTesting = true;
			organisation.ZO_Phone = org2.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org2, dummyUSOrg, organisation);
			enableForTesting = false;
			organisation.ZO_Phone = org1.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org1, dummyUSOrg, organisation);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			organisation.ZO_Phone = "90251199";
			AssertEquals("Organisation.ZO_Phone is not formatted any more", "90251199", organisation.ZO_Phone);
		}

		public void TestZO_Phone_Formatted()
		{
			Env.Registry.SetOrgUsePhoneNumberFormatting(false);
			AssertEquals("PreCondition: DummyUSOrg.OrgPhone", "", dummyUSOrg.OrgPhone);
			AssertEquals("PreCondition: Organisation.ZO_Phone", "", organisation.ZO_Phone_Formatted);
			enableForTesting = true;
			dummyUSOrg.OrgPK = org1.PK;
			dummyUSOrg.OrgPhone = org2.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org2, dummyUSOrg, organisation);
			enableForTesting = false;
			dummyUSOrg.OrgPhone = org1.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org1, dummyUSOrg, organisation);

			enableForTesting = true;
			organisation.ZO_Phone = org2.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org2, dummyUSOrg, organisation);
			enableForTesting = false;
			organisation.ZO_Phone = org1.Contacts[0].OC_Phone;
			AssertZO_PhoneChanges(org1, org1, dummyUSOrg, organisation);

			Env.Registry.SetOrgUsePhoneNumberFormatting(true);
			organisation.ZO_Phone = "90251199";
			AssertEquals("Organisation.ZO_Phone is not formatted any more", "90251199", organisation.ZO_Phone_Formatted);
		}

		void AssertZO_PhoneChanges(OrgHeader previousOrg, OrgHeader currentOrg, DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation)
		{
			AssertEquals("DummyUSOrg.OrgPK", previousOrg.PK, dummyUSOrg.OrgPK);
			AssertEquals("DummyUSOrg.OrgAddressPK", previousOrg.MainAddress.PK, dummyUSOrg.OrgAddressPK);
			AssertEquals("DummyUSOrg.OrgContact", previousOrg.Contacts[0].OC_ContactName, dummyUSOrg.OrgContact);
			AssertEquals("DummyUSOrg.OrgPhone", currentOrg.Contacts[0].OC_Phone, dummyUSOrg.OrgPhone);
			AssertWrappedPropertiesAreUpdated(dummyUSOrg, organisation);
		}

		void AssertWrappedPropertiesAreUpdated(DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation)
		{
			AssertEquals("Organisation.ZO_OH_Organisation", dummyUSOrg.OrgPK, organisation.ZO_OH_Organisation);
			AssertEquals("Organisation.ZO_OA_Address", dummyUSOrg.OrgAddressPK, organisation.ZO_OA_Address);
			AssertEquals("Organisation.ZO_ZO_Contact", dummyUSOrg.OrgContact, organisation.ZO_Contact);
			AssertEquals("Organisation.ZO_Phone", dummyUSOrg.OrgPhone, organisation.ZO_Phone);
		}

		void AssertSynchronizeData(OrgHeader org, DummyWithUSOrganisation dummyUSOrg, USOrganisation organisation, bool keepCountryCode)
		{
			AssertEquals("DummyUSOrg.OrgPK", org.PK, dummyUSOrg.OrgPK);
			AssertEquals("DummyUSOrg.OrgAddressPK", org.MainAddress.PK, dummyUSOrg.OrgAddressPK);
			AssertEquals("DummyUSOrg.OrgContact", org.Contacts[0].OC_ContactName, dummyUSOrg.OrgContact);
			AssertEquals("DummyUSOrg.OrgPhone", PhoneNumberCalculator.GetUnformattedPhoneNumber(org.Contacts[0].OC_Phone, keepCountryCode), dummyUSOrg.OrgPhone);
			AssertWrappedPropertiesAreUpdated(dummyUSOrg, organisation);
		}
		#endregion

		public void TestOrgClosestPort()
		{
			var unloco = organisation.OrgClosestPort;
			AssertNull("OrgClosestPort", unloco);
			organisation.ZO_OH_Organisation = org1.PK;
			unloco = organisation.OrgClosestPort;
			AssertNotNull("OrgClosestPort", unloco);
			AssertNotEquals("OrgClosestPort's Code", "USLAX", unloco.Code);
			AssertEquals("OrgClosestPort's PK", org1.ClosestPort.PK, unloco.PK);
			var address2 = org1.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "USLAX";
			organisation.ZO_OA_Address = address2.PK;
			unloco = organisation.OrgClosestPort;
			AssertNotNull("OrgClosestPort", unloco);
			AssertEquals("OrgClosestPort's Code", "USLAX", unloco.Code);
		}

		public void TestIsValid()
		{
			enableForTesting = false;
			AssertNull("PreCondition: Organisation.Organisation should be null", organisation.Organisation);
			AssertNull("PreCondition: Organisation.Address should be null", organisation.Address);
			AssertEquals("IsValid", false, organisation.IsValid);
			organisation.ZO_OH_Organisation = org1.PK;
			AssertNotNull("PreCondition: Organisation.Organisation should not be null", organisation.Organisation);
			AssertNull("PreCondition: Organisation.Address should be null", organisation.Address);
			AssertEquals("IsValid", false, organisation.IsValid);
			organisation.ZO_OA_Address = org1.MainAddress.PK;
			AssertNotNull("PreCondition: Organisation.Organisation should not be null", organisation.Organisation);
			AssertNotNull("PreCondition: Organisation.Address should not be null", organisation.Address);
			AssertEquals("IsValid", true, organisation.IsValid);
		}

		public void TestCopyValueFrom()
		{
			enableForTesting = true;
			organisation.ZO_OH_Organisation = org1.PK;
			organisation.ZO_Contact = org2.Contacts[0].OC_ContactName;
			organisation.ZO_Phone = "12312132";

			var dummyUSOrg2 = new DummyWithUSOrganisation(Factory);
			var organisation2 = new USOrganisation(dummyUSOrg2.OrgPKInfo, dummyUSOrg2.OrgAddressPKInfo, dummyUSOrg2.OrgContactInfo, dummyUSOrg2.OrgPhoneInfo, dummyUSOrg2.AddressOverrideInfo, ContactType.All, IsSynchronizationEnable, Factory);
			AssertEquals("Organisation2.ZO_OH_Organisation", ZGuid.Empty, organisation2.ZO_OH_Organisation);
			AssertEquals("Organisation2.ZO_OA_Address ", ZGuid.Empty, organisation2.ZO_OA_Address);
			AssertEquals("Organisation2.ZO_Contact", "", organisation2.ZO_Contact);
			AssertEquals("Organisation2.ZO_Phone", "", organisation2.ZO_Phone);

			organisation2.CopyValueFrom(organisation);
			AssertEquals("Organisation2.ZO_OH_Organisation", organisation.ZO_OH_Organisation, organisation2.ZO_OH_Organisation);
			AssertEquals("Organisation2.ZO_OA_Address", organisation.ZO_OA_Address, organisation2.ZO_OA_Address);
			AssertEquals("Organisation2.ZO_Contact", organisation.ZO_Contact, organisation2.ZO_Contact);
			AssertEquals("Organisation2.ZO_Phone", organisation2.ZO_Phone, organisation2.ZO_Phone);
		}

		public void TestCopyValueFrom_NoExceptionThrownWhenDocAddressIsDeleted()
		{
			enableForTesting = true;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TSTMPX";
			org.MainAddress.OA_Address1 = "IMP ADDRESS 1";
			org.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_USPPI.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;
			invoice.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;
			invoice.US_IntermediateConsignee.USOrganisationDocAddress.E2_OA_Address = org.MainAddress.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var dec = newFactory.Load<JobDeclaration>(declaration.PK);
			var loadedInv = dec.Invoices[0];
			loadedInv.US_USPPI.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_USPPI.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;
			loadedInv.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_ExportUltimateConsignee.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;
			loadedInv.US_IntermediateConsignee.USOrganisationDocAddress.E2_CompanyName = "Org";
			loadedInv.US_IntermediateConsignee.USOrganisationDocAddress.E2_OA_Address = ZGuid.Empty;
			newFactory.Save();

			var dummyUSOrg = new DummyWithUSOrganisation(Factory);
			var organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.All, IsSynchronizationEnable, Factory);
			AssertNoExceptionThrown(() => organisation.CopyValueFrom(invoice.US_USPPI));
			AssertNoExceptionThrown(() => organisation.CopyValueFrom(invoice.US_ExportUltimateConsignee));
			AssertNoExceptionThrown(() => organisation.CopyValueFrom(invoice.US_IntermediateConsignee));
		}

		bool enableForTesting = true;

		bool IsSynchronizationEnable(ZBool isAddressOverride)
		{
			return enableForTesting && !isAddressOverride;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.Consignee, IsSynchronizationEnable, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			dummyUSOrg = new DummyWithUSOrganisation(Factory);
			aUSYDUnloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			org1 = CreateNewOrg("ORGANISATION 1", "ORG1 ADDRESS", "BOB SMITH");
			org2 = CreateNewOrg("ORGANISATION 2", "ORG2 ADDRESS", "JOE BROWN");
			organisation = new USOrganisation(dummyUSOrg.OrgPKInfo, dummyUSOrg.OrgAddressPKInfo, dummyUSOrg.OrgContactInfo, dummyUSOrg.OrgPhoneInfo, dummyUSOrg.AddressOverrideInfo, ContactType.All, IsSynchronizationEnable, Factory);
		}

		protected virtual OrgHeader CreateNewOrg(ZString companyName, ZString address, ZString contactName)
		{
			var result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = companyName;
			result.OH_RL_NKClosestPort = aUSYDUnloco.Code;
			result.MainAddress.OA_Address1 = address;
			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = "+61 (2) 9025 1100";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			document.OD_DefaultContact = true;
			return result;
		}

		RefUNLOCO aUSYDUnloco;
		OrgHeader org1;
		OrgHeader org2;
		DummyWithUSOrganisation dummyUSOrg;
		USOrganisation organisation;

		#region DummyWithUSOrganisation

		public class DummyWithUSOrganisation : NonPersistentBusinessObject
		{
			public DummyWithUSOrganisation(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			#region OrgPK
			public ZGuid OrgPK
			{
				get { return fOrgPK; }
				set { SetNonPersistentPropertyValue(OrgPKInfo, ref fOrgPK, value); }
			}
			ZGuid fOrgPK = ZGuid.Empty;

			public virtual ZPropertyInfo OrgPKInfo
			{
				get { return GetZPropertyInfo(nameof(OrgPK)); }
			}
			#endregion

			#region OrgAddressPK
			public ZGuid OrgAddressPK
			{
				get { return fOrgAddressPK; }
				set { SetNonPersistentPropertyValue(OrgAddressPKInfo, ref fOrgAddressPK, value); }
			}
			ZGuid fOrgAddressPK = ZGuid.Empty;

			public virtual ZPropertyInfo OrgAddressPKInfo
			{
				get { return GetZPropertyInfo(nameof(OrgAddressPK)); }
			}
			#endregion

			#region OrgContact
			[MaxLength(MasterFiles.Business.OrgContact.Schema.OC_ContactNameMaxLength)]
			public ZString OrgContact
			{
				get { return fOrgContact; }
				set
				{
					value = value.Trim(' ');
					if (fOrgContact != value)
					{
						CheckMaximumLength(OrgContactInfo, value);
						SetNonPersistentPropertyValue(OrgContactInfo, ref fOrgContact, value);
					}
				}
			}
			ZString fOrgContact = ZString.Empty;

			public virtual ZPropertyInfo OrgContactInfo
			{
				get { return GetZPropertyInfo(nameof(OrgContact)); }
			}
			#endregion

			#region OrgPhone
			[MaxLength(MasterFiles.Business.OrgContact.Schema.OC_PhoneMaxLength)]
			public ZString OrgPhone
			{
				get { return fOrgPhone; }
				set
				{
					value = value.Trim(' ');
					if (fOrgPhone != value)
					{
						CheckMaximumLength(OrgPhoneInfo, value);
						SetNonPersistentPropertyValue(OrgPhoneInfo, ref fOrgPhone, value.Trim(' '));
					}
				}
			}
			ZString fOrgPhone = ZString.Empty;

			public virtual ZPropertyInfo OrgPhoneInfo
			{
				get { return GetZPropertyInfo(nameof(OrgPhone)); }
			}
			#endregion

			#region AddressOverride

			public ZBool AddressOverride
			{
				get { return fAddressOverride; }
				set
				{
					if (fAddressOverride != value)
					{
						SetNonPersistentPropertyValue(AddressOverrideInfo, ref fAddressOverride, value);
					}
				}
			}
			ZBool fAddressOverride;

			public virtual ZPropertyInfo AddressOverrideInfo
			{
				get { return GetZPropertyInfo(nameof(AddressOverride)); }
			}

			#endregion

			public USOrganisation Organisation
			{
				get
				{
					if (fOrganisation == null)
					{
						fOrganisation = new USOrganisation(OrgPKInfo, OrgAddressPKInfo, OrgContactInfo, OrgPhoneInfo, AddressOverrideInfo, ContactType.Consignor, IsEnable, Factory);
					}
					return fOrganisation;
				}
			}

			public OrgHeaderCollection Organisations
			{
				get
				{
					if (fOrganisations == null)
					{
						ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "O");
						fOrganisations = new OrgHeaderCollection(Factory, orgFilter);
					}
					return fOrganisations;
				}
			}

			public OrgContactCollection Contacts
			{
				get
				{
					if (fContacts == null)
					{
						ZQuery contactFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "C");
						fContacts = new OrgContactCollection(Factory, contactFilter);
					}
					return fContacts;
				}
			}

			public override void Delete()
			{
				Organisation.Delete();
				base.Delete();
			}

			protected bool IsEnable(ZBool isAddressOverride)
			{
				return false;
			}

			USOrganisation fOrganisation;
			OrgHeaderCollection fOrganisations;
			OrgContactCollection fContacts;
		}

		public class DummyWithUSOrganisationCollection : NonPersistentBusinessObjectCollection<DummyWithUSOrganisation>
		{
			public DummyWithUSOrganisationCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyWithUSOrganisation(Factory);
			}
		}

		#endregion
	}
}
