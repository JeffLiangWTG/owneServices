using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.US.Testing
{
	//  IF YOU CHANGE ANY OF THIS CODE YOU MUST RUN ALL WAREHOUSE TESTS !!!!!
	//  MOST WAREHOUSE TESTS RELY ON THE EXACT DETAILS OF THESE TEST HELPERS.
	//  SPECIFICALLY THE DATA SETUPS, THE COMPILER WILL NOT FIND PROBLEMS.
	//  RUNNING LOCALISED TESTS AFTER CHANGING THIS CODE IS NOT GOOD ENOUGH!!
	public class WhsTestHelperFunctionsEnvUS : WhsTestHelperFunctionsEnv
	{
		public WhsTestHelperFunctionsEnvUS(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region US Warehouse

		public override WhsWarehouse CreateWarehouse(ZString name, bool shouldPrePopulateDDL = true)
		{
			var warehouse = base.CreateWarehouse(name, shouldPrePopulateDDL);
			var org = CreateOrganization();
			SetupUSOrganization(org);
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;

			AssertNotNull("Warehouse address is not null", warehouse.WarehouseAddress);
			AssertNotNull("Warehouse address related port code is not null", warehouse.WarehouseAddress.RelatedPortCode);
			AssertNotNull("Warehouse address country is not null", warehouse.WarehouseAddress.RelatedPortCode.Country);
			AssertEquals("Warehouse address country is US", Enterprise.Core.Constants.CountryCodes.UnitedStates, warehouse.WarehouseAddress.RelatedPortCode.RL_RN_NKCountryCode);

			return warehouse;
		}

		#endregion

		#region TSA Status

		public void SetOrgAddressTSAStatus(OrgAddress address, ZString tSAStatus)
		{
			OrgCountryData countryData;
			ZQuery filter = new ZQuery(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			filter.AddToFilter(new ZQuery(OrgCountryDataSchema.OV_OA_ApprovedLocation, SQLComparisonOperator.Equal, address.PK), JoinCondition.And);
			countryData = Factory.LoadTop1<OrgCountryData>(filter);
			if (countryData == null)
			{
				countryData = Factory.New<OrgCountryData>();
				countryData.OV_OA_ApprovedLocation = address.PK;
				countryData.OV_OH_OrgHeader = address.OA_OH;
				countryData.OV_RN_NKClientCountryRelation = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			}
			countryData.OV_EXApprovedOrMajorExporter = tSAStatus;
		}

		public void SetWarehouseTSAStatus(WhsWarehouse warehouse, ZString tSAStatus)
		{
			SetOrgAddressTSAStatus(warehouse.WarehouseAddress, tSAStatus);
		}

		#endregion

		#region US Organization

		public OrgHeader CreateOrganization()
		{
			return CreateOrganization("TST", "TEST ORG");
		}

		public OrgHeader CreateOrganization(string code, string name)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = name;

			return org;
		}

		public void SetupUSOrganization(OrgHeader org)
		{
			OrgAddress address = org.MainAddress;
			if (address == null)
			{
				address = org.Addresses.AddNew();
				address.FillWithValidTestData();
			}
			AssertNotNull("Org should have main address", org.MainAddress);

			org.MainAddress.OA_OH = org.PK;
			org.MainAddress.OA_RL_NKRelatedPortCode = GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.UnitedStates).RL_Code;

			AssertNotNull("Org main address related port code is not null", org.MainAddress.RelatedPortCode);
			AssertNotNull("Org main address country is not null", org.MainAddress.RelatedPortCode.Country);
			AssertEquals("Org main address country is US", Enterprise.Core.Constants.CountryCodes.UnitedStates, org.MainAddress.RelatedPortCode.RL_RN_NKCountryCode);
		}

		#endregion
	}
}
