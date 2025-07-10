using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgLocatedWithinModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUnitForGeolocation()
		{
			ModuleFilter.Clear();
			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitMile;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.UnitForGeolocationInfo);

			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitKilometer;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.UnitForGeolocationInfo);

			ModuleFilter.UnitForGeolocation = "ss";
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.UnitForGeolocationInfo, "Enter a valid selection.");

			ModuleFilter.UnitForGeolocation = ZString.Empty;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.UnitForGeolocationInfo, "Please enter a value.");
		}

		public void TestCentraGeolocation()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			var orgAddress1 = org1.Addresses[0];
			orgAddress1.OA_Address1 = "Test Address 001";
			orgAddress1.OA_Address2 = "Test Address 002";
			orgAddress1.OA_GeoLocation = ZGeography.Empty;
			var orgAddress2 = org1.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Test Address 003";
			orgAddress2.OA_Address2 = "Test Address 004";
			orgAddress2.OA_GeoLocation = point1;
			Factory.Save();

			ModuleFilter.Clear();

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.OrgPK = org1.PK;
			ModuleFilter.AddressPK = orgAddress2.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.AddressPKInfo);

			ModuleFilter.Clear();
			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.OrgPK = org1.PK;
			ModuleFilter.AddressPK = orgAddress1.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.AddressPKInfo, "The org. address should have a none-empty Geo-location.");

			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO1.Code = "AABBD";
			refUNLOCO1.RL_GeoLocation = ZGeography.Empty;

			ModuleFilter.Clear();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Test 002";
			org2.OH_RL_NKClosestPort = refUNLOCO1.Code;
			Factory.Save();

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			ModuleFilter.UNLOCO = refUNLOCO1.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.UNLOCOInfo, "The UNLOCO selected does not have any co-ordinates configured. Wise Tech are working to source accurate co-ordinates for more UNLOCO's. You can fill them in manually against the UNLOCO record if you wish to use this search.");
		}

		public void TestDistance()
		{
			ModuleFilter.Clear();
			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitKilometer;
			ModuleFilter.Distance = 100;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.DistanceInfo);
			AssertNoErrors(ModuleFilter.UnitForGeolocationInfo);

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitKilometer;
			ModuleFilter.Distance = -1;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.DistanceInfo, "The Distance is out of range(0.1KM to 500KM)");
			AssertHasErrorContaining(ModuleFilter.UnitForGeolocationInfo, "The Distance is out of range(0.1KM to 500KM)");

			ModuleFilter.Clear();
			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitMile;
			ModuleFilter.Distance = 100;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.DistanceInfo);
			AssertNoErrors(ModuleFilter.UnitForGeolocationInfo);

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.UnitForGeolocation = OrgLocatedWithinModuleFilter.UnitMile;
			ModuleFilter.Distance = -1;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.DistanceInfo, "The Distance is out of range(0.1 Miles to 310 Miles)");
			AssertHasErrorContaining(ModuleFilter.UnitForGeolocationInfo, "The Distance is out of range(0.1 Miles to 310 Miles)");
		}

		public void TestOrgPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			Factory.Save();

			ModuleFilter.Clear();
			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.OrgPK = org1.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.OrgPKInfo);

			ModuleFilter.OrgPK = Guid.NewGuid();
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.OrgPKInfo, "Enter a valid selection.");
		}

		public void TestAddressPK()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			var orgAddress1 = org1.Addresses[0];
			orgAddress1.OA_Address1 = "Test Address 001";
			orgAddress1.OA_Address2 = "Test Address 002";
			orgAddress1.OA_GeoLocation = point1;
			Factory.Save();

			ModuleFilter.Clear();
			moduleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.OrgPK = org1.PK;
			ModuleFilter.AddressPK = orgAddress1.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.AddressPKInfo);

			ModuleFilter.Clear();
			moduleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.OrgPK = org1.PK;
			ModuleFilter.AddressPK = Guid.NewGuid();
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.AddressPKInfo, "Enter a valid selection.");
		}

		public void TestFilterType()
		{
			ModuleFilter.Clear();
			ModuleFilter.FilterType = ZString.Empty;
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.FilterTypeInfo, "Please enter a value.");

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.FilterTypeInfo);

			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.FilterTypeInfo);

			ModuleFilter.FilterType = "ABC";
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.FilterTypeInfo, "Enter a valid selection.");
		}

		public void TestUNLOCO()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);

			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO1.Code = "AABBD";
			refUNLOCO1.RL_GeoLocation = point1;
			refUNLOCO1.Longitude = 1;
			refUNLOCO1.Latitude = 0;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test 001";
			org1.OH_RL_NKClosestPort = refUNLOCO1.Code;
			Factory.Save();

			ModuleFilter.Clear();
			ModuleFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.UNLOCOInfo);

			ModuleFilter.UNLOCO = refUNLOCO1.PK;
			ModuleFilter.Validation.ValidateAll();
			AssertNoErrors(ModuleFilter.UNLOCOInfo);

			ModuleFilter.UNLOCO = Guid.NewGuid();
			ModuleFilter.Validation.ValidateAll();
			AssertHasErrorContaining(ModuleFilter.UNLOCOInfo, "The UNLOCO selected does not have any co-ordinates configured. Wise Tech are working to source accurate co-ordinates for more UNLOCO's. You can fill them in manually against the UNLOCO record if you wish to use this search.");
			AssertHasErrorContaining(ModuleFilter.UNLOCOInfo, "Enter a valid selection.");
		}

		#region Implementation

		OrgLocatedWithinModuleFilter ModuleFilter
		{
			get
			{
				if (moduleFilter == null)
				{
					moduleFilter = new OrgLocatedWithinModuleFilter("TEST", Factory);
				}

				return moduleFilter;
			}
		}
		OrgLocatedWithinModuleFilter moduleFilter;

		#endregion
	}
}
