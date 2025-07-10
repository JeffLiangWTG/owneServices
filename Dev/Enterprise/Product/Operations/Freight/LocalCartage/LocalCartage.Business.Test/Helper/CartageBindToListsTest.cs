using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageBindToListsTest : TestCaseWithFactory
	{
		public void TestCartageAddressList()
		{
			var orgProxy = CreateNewOrgHeader("orgProxy");
			var orgProxyAddress1 = SetUpOrgAddress(orgProxy.MainAddress, "orgProxyAddress1");
			var orgProxyAddress2 = SetUpOrgAddress(orgProxy.Addresses.AddNew(), "orgProxyAddress2");
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var org1 = CreateNewOrgHeader("org1");
			var org1Address1 = SetUpOrgAddress(org1.MainAddress, "org1Address1");
			var org1Address2 = SetUpOrgAddress(org1.Addresses.AddNew(), "org1Address2");
			var org2 = CreateNewOrgHeader("org2");
			var org2Address1 = SetUpOrgAddress(org2.MainAddress, "org2Address1");
			var org2Address2 = SetUpOrgAddress(org2.Addresses.AddNew(), "org2Address2");
			var org3 = CreateNewOrgHeader("org3");
			var org3Address1 = SetUpOrgAddress(org3.MainAddress, "org3Address1");
			var org3Address2 = SetUpOrgAddress(org3.Addresses.AddNew(), "org3Address2");
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			var actualList1 = new CartageBindToLists(Factory).CartageAddressList(cartage);
			var expectedList1 = new CodeElement[] { new CodeElement(orgProxy.MainAddress.PK, "CFS (orgProxy) orgProxyAddress1", "ORGPROXYADDRESS1"), new CodeElement(orgProxy.Address_List.List[0].PK, "CFS (orgProxy) orgProxyAddress2", "ORGPROXYADDRESS2"), new CodeElement(cartage.FirstDocAddress.PK, "CTO ()", ""), new CodeElement(cartage.SecondDocAddress.PK, "CFS ()", ""), new CodeElement(cartage.ThirdDocAddress.PK, "CYD ()", ""), };
			AssertContainsExactElementsInAnyOrder(expectedList1, actualList1);
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = org3.MainAddress.PK;
			var actualList2 = new CartageBindToLists(Factory).CartageAddressList(cartage);
			var expectedList2 = new CodeElement[] { new CodeElement(orgProxyAddress1.PK, "CFS (orgProxy) orgProxyAddress1", "ORGPROXYADDRESS1"), new CodeElement(orgProxyAddress2.PK, "CFS (orgProxy) orgProxyAddress2", "ORGPROXYADDRESS2"), new CodeElement(org1Address1.PK, "CTO (org1) org1Address1", "ORG1ADDRESS1"), new CodeElement(org1Address2.PK, "CTO (org1) org1Address2", "ORG1ADDRESS2"), new CodeElement(org2Address1.PK, "CFS (org2) org2Address1", "ORG2ADDRESS1"), new CodeElement(org2Address2.PK, "CFS (org2) org2Address2", "ORG2ADDRESS2"), new CodeElement(org3Address1.PK, "CYD (org3) org3Address1", "ORG3ADDRESS1"), new CodeElement(org3Address2.PK, "CYD (org3) org3Address2", "ORG3ADDRESS2"), };
			AssertContainsExactElementsInAnyOrder(expectedList2, actualList2);
			org1Address1.OA_IsActive = false;
			org3Address2.OA_IsActive = false;
			Factory.Save();
			var actualList3 = new CartageBindToLists(Factory).CartageAddressList(cartage);
			var expectedList3 = new CodeElement[] { new CodeElement(orgProxyAddress1.PK, "CFS (orgProxy) orgProxyAddress1", "ORGPROXYADDRESS1"), new CodeElement(orgProxyAddress2.PK, "CFS (orgProxy) orgProxyAddress2", "ORGPROXYADDRESS2"), new CodeElement(org1Address2.PK, "CTO (org1) org1Address2", "ORG1ADDRESS2"), new CodeElement(org2Address1.PK, "CFS (org2) org2Address1", "ORG2ADDRESS1"), new CodeElement(org2Address2.PK, "CFS (org2) org2Address2", "ORG2ADDRESS2"), new CodeElement(org3Address1.PK, "CYD (org3) org3Address1", "ORG3ADDRESS1"), };
			AssertContainsExactElementsInAnyOrder(expectedList3, actualList3);
			// Miscellaneous address should not be included in the list
			var orgMisc = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);
			var docAddress4 = cartage.DocAddresses.AddNew();
			docAddress4.DocAddressType = DocAddressType.LocalCartageImporter;
			docAddress4.E2_AddressOverride = true;
			docAddress4.E2_CompanyName = "Override Company";
			docAddress4.E2_Address1 = "Override Address";
			var actualList4 = new CartageBindToLists(Factory).CartageAddressList(cartage);
			var expectedList4 = new CodeElement[] { new CodeElement(orgMisc.MainAddress.PK, "CNE (Override Company) OVERRIDE COMPANY OVERRIDE ADDRESS", "OVERRIDE COMPANY OVERRIDE ADDRESS"), new CodeElement(orgProxyAddress1.PK, "CFS (orgProxy) orgProxyAddress1", "ORGPROXYADDRESS1"), new CodeElement(orgProxyAddress2.PK, "CFS (orgProxy) orgProxyAddress2", "ORGPROXYADDRESS2"), new CodeElement(org1Address2.PK, "CTO (org1) org1Address2", "ORG1ADDRESS2"), new CodeElement(org2Address1.PK, "CFS (org2) org2Address1", "ORG2ADDRESS1"), new CodeElement(org2Address2.PK, "CFS (org2) org2Address2", "ORG2ADDRESS2"), new CodeElement(org3Address1.PK, "CYD (org3) org3Address1", "ORG3ADDRESS1"), };
			AssertContainsExactElementsInAnyOrder(expectedList4, actualList4);
		}

		public void TestCartageAddressList_CartageIsNull()
		{
			var cartageList = new CartageBindToLists(Factory).CartageAddressList(null);
			AssertEquals(0, cartageList.Count);
		}

		public void TestCartageAddressElements()
		{
			var org = CreateNewOrgHeader("Org");
			var orgAddress = SetUpOrgAddress(org.MainAddress, "OrgAddress");
			var cartage = Factory.New<CommonCartage>();
			var bindToList = new CartageBindToLists(Factory);
			cartage.FirstDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("Precondition", false, cartage.FirstDocAddress.E2_AddressOverride);
			var addressSelectionElements = bindToList.CartageAddressElements(cartage);
			AssertEquals("Org", addressSelectionElements.Single(e => e.DocOrOrgAddressPK == cartage.FirstDocAddress.PK).OrgCode);
			cartage.FirstDocAddress.E2_AddressOverride = true;
			cartage.FirstDocAddress.E2_CompanyName = "Company Name";
			addressSelectionElements = bindToList.CartageAddressElements(cartage);
			AssertEquals("Company Name", addressSelectionElements.Single(e => e.DocOrOrgAddressPK == cartage.FirstDocAddress.PK).OrgCode);
		}

		public void TestCartageContainerModes()
		{
			var bindToList = new CartageBindToLists(Factory);
			var expectedCodes = new[] { Constants.CartageContainerMode.Containerized, Constants.CartageContainerMode.Loose, Constants.CartageContainerMode.Mixed };
			AssertContainsExactElementsInAnyOrder(expectedCodes, bindToList.CartageContainerModes.ToArray().Select(c => c.Code));
		}

		public void TestParentJobTypes()
		{
			var bindToList = new CartageBindToLists(Factory);
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("STC", "Standalone Cartage");
			expectedList.AddPair("TBK", "Transport Booking");
			expectedList.AddPair("CUS", "Customs Declaration");
			expectedList.AddPair("SHP", "Forwarding Shipment");
			expectedList.AddPair("WHO", "Warehouse Order");
			expectedList.AddPair("CFS", "CFS Shipment");
			expectedList.AddPair("CFC", "CFS Load List Consolidation");
			AssertContainsExactElementsInAnyOrder(expectedList, bindToList.ParentJobTypes);
		}

		OrgHeader CreateNewOrgHeader(ZString oH_Code)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = oH_Code;
			return org;
		}

		OrgAddress SetUpOrgAddress(OrgAddress orgAddress, ZString orgAddressName)
		{
			orgAddress.OA_Code = orgAddressName;
			orgAddress.OA_Address1 = orgAddressName;
			return orgAddress;
		}
	}
}
