using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Yard.DataTransfer.Gate;

namespace Enterprise.Warehouse.Yard.DataTransfer.Test
{
	class FacilityMatchingHelperTest : TestCaseWithFactory
	{
		public void TestGetFacilityFromCommunityCode_FindsMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "ABC", string.Empty);
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromCommunityCode(Factory, "ABC", WarehouseTypes.Codes.ContainerYard);

			AssertEquals("facilities should match", matchedFacility.PK, facility.PK);
		}

		public void TestGetFacilityFromCommunityCode_WhenCommunityCodeDoesntExist_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.CustomsCodes.AddNew("XYZ", "ABC", string.Empty);
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromCommunityCode(Factory, "ABC", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromCommunityCode_WhenCommunityCodeDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "DEF", string.Empty);
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromCommunityCode(Factory, "ABC", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromCommunityCode_WhenAddressDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_OH = org.PK;
			orgAddress2.OA_Code = "ADDR2";

			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "ABC", string.Empty);
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = orgAddress2.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromCommunityCode(Factory, "ABC", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromCommunityCode_WhenWarehouseTypeDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "ABC", string.Empty);
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromCommunityCode(Factory, "ABC", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromAddressCodeAndOrgCode_FindsMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTTSTTST";
			org.MainAddress.OA_Code = "ADDR1";
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, "TSTTSTTST", "ADDR1", WarehouseTypes.Codes.ContainerYard);

			AssertEquals("facilities should match", matchedFacility.PK, facility.PK);
		}

		public void TestGetFacilityFromAddressCodeAndOrgCode_WhenAddressCodeDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTTSTTST";
			org.MainAddress.OA_Code = "ADDR2";
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, "TSTTSTTST", "ADDR1", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromAddressCodeAndOrgCode_WhenOrgCodeDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "XXXTSTXXX";
			org.MainAddress.OA_Code = "ADDR1";
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, "TSTTSTTST", "ADDR1", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromAddressCodeAndOrgCode_WhenAddressDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSTTSTTST";
			org.MainAddress.OA_Code = "ADDR1";
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_OH = org.PK;
			orgAddress2.OA_Code = "ADDR2";

			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_OA_WarehouseAddress = orgAddress2.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, "TSTTSTTST", "ADDR1", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}

		public void TestGetFacilityFromAddressCodeAndOrgCode_WhenWarehouseTypeDoesntMatch_ThenDoesntFindMatch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusCode = org.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "ABC", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			facility.WW_OA_WarehouseAddress = org.MainAddress.PK;
			Factory.Save();

			var matchedFacility = FacilityMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, "TSTTSTTST", "ADDR1", WarehouseTypes.Codes.ContainerYard);
			AssertNull(matchedFacility);
		}
	}
}
