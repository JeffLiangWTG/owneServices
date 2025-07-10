using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGovRegNumTypes()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;

			AssertEquals(true, address.Lookups.GovRegNumTypes.Count > 1);
			AssertEquals("DEF", address.Lookups.GovRegNumTypes[0].Code);

			address.E2_RN_NKCountryCode = "";
			AssertEquals(1, address.Lookups.GovRegNumTypes.Count);
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.LookupsGovRegNumTypes = delegate(JobDocAddressLookups lookups)
				{
					CodeDescriptionPairList result = new CodeDescriptionPairList();
					result.AddPair("HWL", "HELLO WORLD");
					return result;
				};

			AssertEquals(1, address.Lookups.GovRegNumTypes.Count);
			AssertEquals("HWL", address.Lookups.GovRegNumTypes[0].Code);
		}

		public void TestSelectedOrganisationAddresses()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			IBusinessObjectCollection collection = docAddress.Lookups.SelectedOrganisationAddresses;
			AssertNotNull(collection);
			docAddress.OrganisationPK = org.PK;
			AssertNotEquals(collection, docAddress.Lookups.SelectedOrganisationAddresses);
			AssertEquals(org.Addresses, docAddress.Lookups.SelectedOrganisationAddresses);
		}

		public void TestStateList()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			RefCountryStates state1 = Factory.New<RefCountryStates>();
			RefCountryStates state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_RN_NKCountryCode = country.RN_Code;

			JobDocAddress address = Factory.New<JobDocAddress>();
			AssertEquals("State List should have no elements.", 0, address.Lookups.State_List.Count);
			AssertEquals("State List should have no members.", false, address.StateListHasMembers);

			address.E2_AddressOverride = true;
			address.E2_RN_NKCountryCode = "X7";
			AssertEquals("State List should have 2 elements.", 2, address.Lookups.State_List.Count);
			AssertEquals("State List should have members.", true, address.StateListHasMembers);
		}

		public void TestDefaultAddressTypeList()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			CodeDescriptionPairList result = address.Lookups.SelectableDocAddressType_List;

			DocAddressTypes defaultList = Factory.GetCachedValue<DocAddressTypes>();

			AssertEquals("Default list should contain all values.", result.Count, defaultList.Count);
		}

		public void TestAddressTypeLookupList()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();

			var addressTypesList = address.Lookups.ResidentialCommercialAddressType_List;

			AssertNotNull(addressTypesList);
			AssertEquals("AddressTypes contains 2 Codes", addressTypesList.Count, 2);
			AssertEquals("1st addressType in the list is 'RES'", addressTypesList[0].Code, "RES");
			AssertEquals("2nd addressType in the list is 'COM'", addressTypesList[1].Code, "COM");
		}

		public void TestDynamicListContainsApplicableCodes()
		{
			MockJobDocAddressParentForLookupsTest parent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			JobDocAddress address = parent.DocAddresses.AddNew(DocAddressType.LocalCartageAddress1);
			address.E2_OA_Address = ZGuid.NewZGuid();
			address.DocAddressManager = parent.DocAddressManager;
			AssertEquals("Default list should contain 2 primary values.", 2, address.Lookups.SelectableDocAddressType_List.Count);

			CodeDescriptionPairList list = address.Lookups.SelectableDocAddressType_List;
			bool containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].DefaultDocAddressType));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].DefaultDocAddressType));
			AssertEquals("List should contain the 2 primary codes.", true, containsExpected);

			JobDocAddress address2 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[0].DefaultDocAddressType);
			address2.E2_OA_Address = ZGuid.NewZGuid();
			list = address.Lookups.SelectableDocAddressType_List;

			containsExpected =
				list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].SupportedDocAddressTypes[0])) &&
				list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].DefaultDocAddressType));

			AssertEquals("List should contain the 2nd Consignee code and consignor primary code only.", 2, list.Count);
			AssertEquals("List should contain the 2nd Consignee code and consignor primary code.", true, containsExpected);

			JobDocAddress address3 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[1].DefaultDocAddressType);
			address3.E2_OA_Address = ZGuid.NewZGuid();
			list = address.Lookups.SelectableDocAddressType_List;
			containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].SupportedDocAddressTypes[0]));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].SupportedDocAddressTypes[0]));
			AssertEquals("List should contain the 2nd Consignee code and 2nd consignor code only.", 2, list.Count);
			AssertEquals("List should contain the 2nd Consignee code and 2nd consignor code.", true, containsExpected);

			JobDocAddress address4 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[1].SupportedDocAddressTypes[0]);
			address4.E2_OA_Address = ZGuid.NewZGuid();
			list = address.Lookups.SelectableDocAddressType_List;
			containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].SupportedDocAddressTypes[0]));
			AssertEquals("List should contain the 2nd consignee code only.", 1, list.Count);
			AssertEquals("List should contain the 2nd consignee code.", true, containsExpected);
		}

		public void TestDynamicListContainsApplicableCodesWithMixUp()
		{
			MockJobDocAddressParentForLookupsTest parent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			JobDocAddress address = parent.DocAddresses.AddNew(DocAddressType.LocalCartageAddress1);
			address.E2_OA_Address = ZGuid.NewZGuid();
			address.DocAddressManager = parent.DocAddressManager;
			AssertEquals("Default list should contain 2 primary values.", 2, address.Lookups.SelectableDocAddressType_List.Count);
			CodeDescriptionPairList list = address.Lookups.SelectableDocAddressType_List;
			bool containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].DefaultDocAddressType));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].DefaultDocAddressType));
			AssertEquals("List should contain the 2 primary codes.", true, containsExpected);

			JobDocAddress address2 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[0].DefaultDocAddressType);
			address2.E2_OA_Address = ZGuid.NewZGuid();
			list = address.Lookups.SelectableDocAddressType_List;
			containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].SupportedDocAddressTypes[0]));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].DefaultDocAddressType));
			AssertEquals("List should contain the 2nd Consignee code and consignor primary code only.", 2, list.Count);
			AssertEquals("List should contain the 2nd Consignee code and consignor primary code.", true, containsExpected);

			parent.DocAddresses.Remove(address2.PK);

			JobDocAddress address3 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[1].DefaultDocAddressType);
			address3.E2_OA_Address = ZGuid.NewZGuid();

			list = address.Lookups.SelectableDocAddressType_List;
			containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].DefaultDocAddressType));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].SupportedDocAddressTypes[0]));
			AssertEquals("List should contain the 1st Consignee code and 2nd consignor code only.", 2, list.Count);
			AssertEquals("List should contain the 1st Consignee code and 2nd consignor code.", true, containsExpected);

			JobDocAddress address4 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[0].SupportedDocAddressTypes[0]);
			address4.E2_OA_Address = ZGuid.NewZGuid();

			list = address.Lookups.SelectableDocAddressType_List;
			containsExpected = list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[0].DefaultDocAddressType));
			containsExpected = containsExpected && list.ContainsCode(DocAddressTypes.GetCode(Factory, parent.DocAddressManager.Requirements[1].SupportedDocAddressTypes[0]));
			AssertEquals("List should contain the 1st Consignee code and 2nd consignor code only.", 2, list.Count);
			AssertEquals("List should contain the 1st Consignee code and 2nd consignor code.", true, containsExpected);

			JobDocAddress address5 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[0].DefaultDocAddressType);
			JobDocAddress address6 = parent.DocAddresses.AddNew(parent.DocAddressManager.Requirements[1].SupportedDocAddressTypes[0]);
			address5.E2_OA_Address = ZGuid.NewZGuid();
			address6.E2_OA_Address = ZGuid.NewZGuid();

			list = address.Lookups.SelectableDocAddressType_List;
			AssertEquals("List should be empty.", 0, list.Count);
		}

		#region TestOrgHeader_List

		public void TestOrgHeader_List()
		{
			IDocAddresses docAddressesParent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			JobDocAddress docAddress = docAddressesParent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeAddress);
			AssertEquals("Specific collection of available Organisations was specified for Consignee DocAddressType, so lookups binding should use it.", typeof(ConsigneeCollection), docAddress.Lookups.OrgHeader_List.GetType());

			docAddress.DocAddressType = DocAddressType.PickUpAddress;
			AssertEquals("No Specific collection of available Organisations was specified for PickUp DocAddressType, so lookups binding should use default collection.", typeof(OrgHeaderCollection), docAddress.Lookups.OrgHeader_List.GetType());
		}

		#endregion

		#region TestAddress_List

		#region TestAddress_List

		public void TestAddress_List()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Test Organisation Pty Limited";
			organisation.OH_RL_NKClosestPort = "AUSYD";
			organisation.OH_Code = "DUUUUH";
			organisation.MainAddress.OA_Address1 = "This should have been updated";
			organisation.MainAddress.OA_City = "A city";
			organisation.MainAddress.OA_CompanyNameOverride = "Test Organisation Different Name";
			organisation.MainAddress.OA_City = "abcd";
			organisation.MainAddress.OA_State = "nsw";
			organisation.MainAddress.OA_PostCode = "1234";

			var address1 = organisation.Addresses.AddNew();
			var address2 = organisation.Addresses.AddNew();
			address1.FillWithValidTestData();
			address2.FillWithValidTestData();

			var cap1 = Factory.New<OrgAddressCapability>();
			cap1.PZ_AddressType = OrgAddressType.Delivery.Code;
			cap1.PZ_OA = address1.PK;

			var cap2 = Factory.New<OrgAddressCapability>();
			cap2.PZ_AddressType = OrgAddressType.Pickup.Code;
			cap2.PZ_OA = address2.PK;

			address2.OA_IsActive = ZBool.False;

			var docAddressesParent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			var docAddress = docAddressesParent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeAddress);
			AssertEquals("If no organisation specified, then address list should be empty.", 0, docAddress.Lookups.Address_List.Count);

			docAddress.OrganisationPK = organisation.PK;
			var list = docAddress.Lookups.Address_List;
			AssertEquals(2, list.Count);
			AssertNotNull(Array.Find(list.List.ToArray(), c => (ZGuid)c.PK == organisation.MainAddress.PK));
			AssertNotNull(Array.Find(list.List.ToArray(), c => (ZGuid)c.PK == address1.PK));
			AssertNull(Array.Find(list.List.ToArray(), c => (ZGuid)c.PK == address2.PK));
		}

		#endregion

		#region TestAddress_List_AddressOverride

		public void TestAddress_List_AddressOverride()
		{
			var docAddressesParent = Factory.New<MockJobDocAddressParentForLookupsTest>();
			var docAddress = docAddressesParent.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeAddress);
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "Address 1";
			docAddress.E2_Address2 = "Address 2";
			docAddress.E2_Postcode = "12345";
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_State = "NSW";
			AssertEquals("Address list should be built from overriden address.", 1, docAddress.Lookups.Address_List.Count);
			AssertEquals("Address 1", docAddress.Lookups.Address_List.List[0].Code);
			AssertEquals("ADDRESS 1 ADDRESS 2 NSW 12345", docAddress.Lookups.Address_List.List[0].Description);
		}

		#endregion

		#endregion
	}
}
