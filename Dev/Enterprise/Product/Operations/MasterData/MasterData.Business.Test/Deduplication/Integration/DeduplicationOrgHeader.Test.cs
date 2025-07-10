using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationOrgHeaderTest : TestCaseWithFactory
	{
		public void TestPreventRelationLoops()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var orgC = Factory.NewWithValidTestData<OrgHeader>();
			var orgD = Factory.NewWithValidTestData<OrgHeader>();
			var orgE = Factory.NewWithValidTestData<OrgHeader>();
			orgA.OH_Code = "AAA";
			orgB.OH_Code = "BBB";
			orgC.OH_Code = "CCC";
			orgD.OH_Code = "DDD";
			orgE.OH_Code = "EEE";

			Factory.Save();

			var relation_A_B = Factory.New<OrgRelatedParty>();
			relation_A_B.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_A_B.PR_OH_Parent = orgB.PK;
			relation_A_B.PR_OH_RelatedParty = orgA.PK;

			var relation_A_C = Factory.New<OrgRelatedParty>();
			relation_A_C.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_A_C.PR_OH_Parent = orgC.PK;
			relation_A_C.PR_OH_RelatedParty = orgA.PK;

			var relation_C_D = Factory.New<OrgRelatedParty>();
			relation_C_D.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_C_D.PR_OH_Parent = orgD.PK;
			relation_C_D.PR_OH_RelatedParty = orgC.PK;

			var relation_D_E = Factory.New<OrgRelatedParty>();
			relation_D_E.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_D_E.PR_OH_Parent = orgE.PK;
			relation_D_E.PR_OH_RelatedParty = orgD.PK;

			var relation_E_A = Factory.New<OrgRelatedParty>();
			relation_E_A.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			relation_E_A.PR_OH_Parent = orgA.PK;
			relation_E_A.PR_OH_RelatedParty = orgE.PK;

			Factory.Save();

			var dedupOrgHeader = new DeduplicationOrgHeader(orgE);

			var relatedOrgs = dedupOrgHeader.AllRelatedOrganizationsPK.ToArray();
			AssertEquals(5, relatedOrgs.Length);
			AssertCollectionContains(orgA.PK, dedupOrgHeader.AllRelatedOrganizationsPK);
			AssertCollectionContains(orgB.PK, dedupOrgHeader.AllRelatedOrganizationsPK);
			AssertCollectionContains(orgC.PK, dedupOrgHeader.AllRelatedOrganizationsPK);
			AssertCollectionContains(orgD.PK, dedupOrgHeader.AllRelatedOrganizationsPK);
			AssertCollectionContains(orgE.PK, dedupOrgHeader.AllRelatedOrganizationsPK);
		}

		public void TestContactsPopulated()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			((IDeduplicatable)header).ShouldRunDeduplication = true;
			header.Contacts.AddNew();

			AssertEquals("Precondition", 1, header.Contacts.Count);

			var dedupHeader = new DeduplicationOrgHeader(header);

			AssertEquals("Children not populated", 1, dedupHeader.OrgContacts.Count);

			var headerContact = header.Contacts[0];
			var dedupContact = dedupHeader.OrgContacts.First();

			AssertEquals("OC_PK not populated", headerContact.PK, dedupContact.OC_PK);
			AssertEquals("OC_ContactName not populated", headerContact.OC_ContactName, dedupContact.OC_ContactName);
			AssertEquals("OC_IsActive not populated", headerContact.OC_IsActive, dedupContact.OC_IsActive);
			AssertEquals("OC_Email not populated", headerContact.OC_Email, dedupContact.OC_Email);
			AssertEquals("OC_HomePhone not populated", headerContact.OC_HomePhone, dedupContact.OC_HomePhone);
			AssertEquals("OC_Phone not populated", headerContact.OC_Phone, dedupContact.OC_Phone);
			AssertEquals("OC_Mobile not populated", headerContact.OC_Mobile, dedupContact.OC_Mobile);
		}

		public void TestContactsPopulated_FactorySave()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.Contacts.AddNew();
			Factory.Save();

			AssertEquals("Precondition", 1, header.Contacts.Count);

			var dedupHeader = new DeduplicationOrgHeader(header);

			AssertEquals("Children not populated", 1, dedupHeader.OrgContacts.Count);

			var headerContact = header.Contacts[0];
			var dedupContact = dedupHeader.OrgContacts.First();

			AssertEquals("OC_PK not populated", headerContact.PK, dedupContact.OC_PK);
			AssertEquals("OC_ContactName not populated", headerContact.OC_ContactName, dedupContact.OC_ContactName);
			AssertEquals("OC_IsActive not populated", headerContact.OC_IsActive, dedupContact.OC_IsActive);
			AssertEquals("OC_Email not populated", headerContact.OC_Email, dedupContact.OC_Email);
			AssertEquals("OC_HomePhone not populated", headerContact.OC_HomePhone, dedupContact.OC_HomePhone);
			AssertEquals("OC_Phone not populated", headerContact.OC_Phone, dedupContact.OC_Phone);
			AssertEquals("OC_Mobile not populated", headerContact.OC_Mobile, dedupContact.OC_Mobile);
		}

		public void TestAddressPopulated()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			((IDeduplicatable)header).ShouldRunDeduplication = true;

			AssertEquals("Precondition", 1, header.Addresses.Count);

			var dedupHeader = new DeduplicationOrgHeader(header);

			AssertEquals("Children not populated", 1, dedupHeader.OrgAddresses.Count);

			var headerAddress = header.Addresses[0];
			var dedupAddress = dedupHeader.OrgAddresses.First();

			AssertEquals("OA_PK not populated", headerAddress.PK, dedupAddress.OA_PK);
			AssertEquals("OA_Address1 not populated", headerAddress.OA_Address1, dedupAddress.OA_Address1);
			AssertEquals("OA_Address2 not populated", headerAddress.OA_Address2, dedupAddress.OA_Address2);
			AssertEquals("OA_Phone not populated", headerAddress.OA_Phone, dedupAddress.OA_Phone);
			AssertEquals("OA_City not populated", headerAddress.OA_City, dedupAddress.OA_City);
			AssertEquals("OA_State not populated", headerAddress.OA_State, dedupAddress.OA_State);
			AssertEquals("OA_PostCode not populated", headerAddress.OA_PostCode, dedupAddress.OA_PostCode);
			AssertEquals("OA_ValidationStatus not populated", "NTC", dedupAddress.OA_ValidationStatus);

			header.Addresses[0].OA_RN_NKCountryCode = "AU";
			using (Env.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dedupHeader1 = new DeduplicationOrgHeader(header);
				AssertEquals("OA_ValidationStatus not populated", header.Addresses[0].OA_ValidationStatus, dedupHeader1.OrgAddresses.First().OA_ValidationStatus);
			}

			AssertEquals("OrgAddressCapacilities not populated", 1, dedupAddress.OrgAddressCapabilities.Count);

			var headerAddressCapabilityCollection = headerAddress.CapabilitiesCollection;
			var dedupAddressCapabilityCollection = dedupAddress.OrgAddressCapabilities;

			foreach (var addressCapability in headerAddressCapabilityCollection)
			{
				var dedupaddressCapability = dedupAddressCapabilityCollection.FirstOrDefault(x => x.PZ_PK == addressCapability.PK);

				AssertEquals("PZ_PK not populated", addressCapability.PK, dedupaddressCapability.PZ_PK);
				AssertEquals("PZ_IsMainAddress not populated", addressCapability.PZ_IsMainAddress, dedupaddressCapability.PZ_IsMainAddress);
			}
		}

		public void TestAddressPopulated_FactorySave()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertEquals("Precondition", 1, header.Addresses.Count);

			var dedupHeader = new DeduplicationOrgHeader(header);

			AssertEquals("Children not populated", 1, dedupHeader.OrgAddresses.Count);

			var headerAddress = header.Addresses[0];
			var dedupAddress = dedupHeader.OrgAddresses.First();

			AssertEquals("OA_PK not populated", headerAddress.PK, dedupAddress.OA_PK);
			AssertEquals("OA_Address1 not populated", headerAddress.OA_Address1, dedupAddress.OA_Address1);
			AssertEquals("OA_Address2 not populated", headerAddress.OA_Address2, dedupAddress.OA_Address2);
			AssertEquals("OA_Phone not populated", headerAddress.OA_Phone, dedupAddress.OA_Phone);
			AssertEquals("OA_City not populated", headerAddress.OA_City, dedupAddress.OA_City);
			AssertEquals("OA_State not populated", headerAddress.OA_State, dedupAddress.OA_State);
			AssertEquals("OA_PostCode not populated", headerAddress.OA_PostCode, dedupAddress.OA_PostCode);
			AssertEquals("OA_ValidationStatus not populated", "NTC", dedupAddress.OA_ValidationStatus);

			header.Addresses[0].OA_RN_NKCountryCode = "AU";
			Factory.Save();
			using (Env.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dedupHeader1 = new DeduplicationOrgHeader(header);
				AssertEquals("OA_ValidationStatus not populated", header.Addresses[0].OA_ValidationStatus, dedupHeader1.OrgAddresses.First().OA_ValidationStatus);
			}

			AssertEquals("OrgAddressCapacilities not populated", 1, dedupAddress.OrgAddressCapabilities.Count);

			var headerAddressCapabilityCollection = headerAddress.CapabilitiesCollection;
			var dedupAddressCapabilityCollection = dedupAddress.OrgAddressCapabilities;

			foreach (var addressCapability in headerAddressCapabilityCollection)
			{
				var dedupaddressCapability = dedupAddressCapabilityCollection.FirstOrDefault(x => x.PZ_PK == addressCapability.PK);

				AssertEquals("PZ_PK not populated", addressCapability.PK, dedupaddressCapability.PZ_PK);
				AssertEquals("PZ_IsMainAddress not populated", addressCapability.PZ_IsMainAddress, dedupaddressCapability.PZ_IsMainAddress);
			}
		}

		public void TestGetOrgCountryCodeWithFallbackLogic()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "";
			org.MainAddress.OA_RN_NKCountryCode = "";
			var dedupOrgHeader = new DeduplicationOrgHeader(org);
			AssertEquals(string.Empty, dedupOrgHeader.CountryCode);

			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			dedupOrgHeader = new DeduplicationOrgHeader(org);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, dedupOrgHeader.CountryCode);

			org.OH_RL_NKClosestPort = "AUSYD";
			dedupOrgHeader = new DeduplicationOrgHeader(org);
			AssertEquals(Core.Constants.CountryCodes.Australia, dedupOrgHeader.CountryCode);
		}

		public void TestChildrenOfOrgContactAreNotIncluded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var contactItem = contact.ContactItems.AddNew();
			Factory.Save();
			var dedupOrgHeader = new DeduplicationOrgHeader(org);

			AssertEquals(1, dedupOrgHeader.OrgContacts.Count);
			AssertNull(dedupOrgHeader.OrgContacts.First().OrgContactItems);
		}

		public void TestConstructorSetRawNameCorrectly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ABC";
			var dedupOrgHeader = new DeduplicationOrgHeader(org);
			AssertEquals("ABC", dedupOrgHeader.OH_FullName);
			AssertEquals("ABC", dedupOrgHeader.RawName);

			dedupOrgHeader = new DeduplicationOrgHeader("EFG");
			AssertEquals("EFG", dedupOrgHeader.OH_FullName);
			AssertEquals("EFG", dedupOrgHeader.RawName);
		}

		public void TestNoCollectionModifiedErrorWhenOrgDoNotHaveMainAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			AssertEquals("Precondition", 3, org.Addresses.Count);

			org.MainAddress.Delete();
			address1.Address1 = "ABC ABC";
			address2.Address1 = "CBA CBA";
			Factory.Save();

			AssertEquals(2, org.Addresses.Count);
			AssertNoExceptionThrown(() => new DeduplicationOrgHeader(org));
		}
	}
}
