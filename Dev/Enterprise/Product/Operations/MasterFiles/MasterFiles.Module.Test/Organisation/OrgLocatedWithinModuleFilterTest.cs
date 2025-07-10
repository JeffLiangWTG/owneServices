using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgLocatedWithinModuleFilter))]
	sealed class OrgLocatedWithinModuleFilterTest : ModuleFilterTestCase<OrgLocatedWithinModuleFilter>
	{
		public void TestFilterUNLOCOWithDistance()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);
			var point2 = ZGeography.CreatePoint(1.00028, 0, 4326);
			var point3 = ZGeography.CreatePoint(1.00084, 0, 4326);

			var setDistance = 0.07m;

			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO1.Code = "AABBC";
			refUNLOCO1.RL_GeoLocation = point1;
			refUNLOCO1.Longitude = 1;
			refUNLOCO1.Latitude = 0;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			org1.OH_RL_NKClosestPort = refUNLOCO1.Code;

			var refUNLOCO2 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO2.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO2.Code = "AABBD";
			refUNLOCO2.RL_GeoLocation = point2;
			refUNLOCO2.Longitude = 1.00028;
			refUNLOCO2.Latitude = 0;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Test 002";
			org2.OH_RL_NKClosestPort = refUNLOCO2.Code;

			var refUNLOCO3 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO3.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO3.Code = "AABBE";
			refUNLOCO3.RL_GeoLocation = point3;
			refUNLOCO3.Longitude = 1.00084;
			refUNLOCO3.Latitude = 0;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "Test 003";
			org3.OH_RL_NKClosestPort = refUNLOCO3.Code;

			Factory.Save();

			var filter = new OrganisationFilterBusinessObject();

			var addressFilter = filter["Located Within"] as OrgLocatedWithinModuleFilter;
			AssertNotNull("Filter should exist", addressFilter);

			addressFilter.IsActive = true;
			addressFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			addressFilter.UNLOCO = refUNLOCO1.PK;
			addressFilter.Distance = setDistance;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals("Address filter is empty due to property is empty", 2, collection.Count);
			AssertCollectionContains("Get the org1 with Set Distance", org1.PK, collection.GetPKs());
			AssertCollectionContains("Get the org2 with Set Distance", org2.PK, collection.GetPKs());
		}

		public void TestFilterAddressOfTheOrganisationWithDistance()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);
			var point2 = ZGeography.CreatePoint(1.00028, 0, 4326);
			var point3 = ZGeography.CreatePoint(1.00084, 0, 4326);

			var setDistance = 0.07m;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			var orgAddress1 = org1.Addresses[0];
			orgAddress1.OA_Address1 = "Test Address 001";
			orgAddress1.OA_Address2 = "Test Address 002";
			orgAddress1.OA_GeoLocation = point1;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Test 002";
			var orgAddress2 = org2.Addresses[0];
			orgAddress2.OA_Address1 = "Test Address 003";
			orgAddress2.OA_Address2 = "Test Address 004";
			orgAddress2.OA_GeoLocation = point2;

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "Test 003";
			var orgAddress3 = org3.Addresses[0];
			orgAddress3.OA_Address1 = "Test Address 005";
			orgAddress3.OA_Address2 = "Test Address 006";
			orgAddress3.OA_GeoLocation = point3;

			Factory.Save();

			var filter = new OrganisationFilterBusinessObject();

			var addressFilter = filter["Located Within"] as OrgLocatedWithinModuleFilter;
			AssertNotNull("Filter should exist", addressFilter);

			addressFilter.IsActive = true;
			addressFilter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			addressFilter.OrgPK = org1.PK;
			addressFilter.AddressPK = orgAddress1.PK;
			addressFilter.Distance = setDistance;
			var collection = new OrgHeaderCollection(Factory, filter.Filter);
			collection.Load();
			AssertEquals("Address filter is empty due to property is empty", 2, collection.Count);
			AssertCollectionContains("Get the org2 with inactive address", org1.PK, collection.GetPKs());
			AssertCollectionContains("Get the org2 with inactive address", org2.PK, collection.GetPKs());
		}

		public void TestSerialize_Deserialize_PropertiesFromToXmlForUNLOCO()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);

			var setDistance = 70m;

			var refUNLOCO1 = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			refUNLOCO1.Code = "AABBC";
			refUNLOCO1.RL_GeoLocation = point1;
			refUNLOCO1.Longitude = 1;
			refUNLOCO1.Latitude = 0;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			org1.OH_RL_NKClosestPort = refUNLOCO1.Code;

			Factory.Save();

			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new OrgLocatedWithinModuleFilter("TEST", Factory);

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			filter.UNLOCO = refUNLOCO1.PK;
			filter.Distance = setDistance;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = ZString.Empty;
			strip.Delete();

			var loadedFilter = (OrgLocatedWithinModuleFilter)filterStripBizO[filter.Description];
			filterStripBizO.LoadLayout(savedLayout);
			AssertEquals("FilterType", OrgLocatedWithinModuleFilter.SearchTypeUNLOCO, loadedFilter.FilterType);
			AssertEquals("UNLOCO", refUNLOCO1.PK, loadedFilter.UNLOCO);
			AssertEquals("Distance", setDistance, loadedFilter.Distance);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXmlForOrgAddress()
		{
			var point1 = ZGeography.CreatePoint(1, 0, 4326);
			var setDistance = 70m;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Test001";
			var orgAddress1 = org1.Addresses[0];
			orgAddress1.OA_Address1 = "Test Address 001";
			orgAddress1.OA_Address2 = "Test Address 002";
			orgAddress1.OA_GeoLocation = point1;

			Factory.Save();

			var filterStripBizO = new DummyFilterStripBusinessObject();
			var filter = new OrgLocatedWithinModuleFilter("TEST", Factory);

			filterStripBizO.AddModuleFilterForTest(filter);
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			filter.OrgPK = org1.PK;
			filter.AddressPK = orgAddress1.PK;
			filter.Distance = setDistance;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = ZString.Empty;
			strip.Delete();

			var loadedFilter = (OrgLocatedWithinModuleFilter)filterStripBizO[filter.Description];
			filterStripBizO.LoadLayout(savedLayout);
			AssertEquals("FilterType", OrgLocatedWithinModuleFilter.SearchTypeOrgAddress, loadedFilter.FilterType);
			AssertEquals("OrgPK", org1.PK, loadedFilter.OrgPK);
			AssertEquals("AddressPK", orgAddress1.PK, loadedFilter.AddressPK);
			AssertEquals("Distance", setDistance, loadedFilter.Distance);
		}

		public void TestConstructor_SetsDefaultPropertyValues()
		{
			var filter = new OrgLocatedWithinModuleFilter("TEST");
			AssertEquals(OrgLocatedWithinModuleFilter.SearchTypeUNLOCO, filter.FilterType);
		}

		public void TestClear_UseDefaultPropertyValues()
		{
			var filter = new OrgLocatedWithinModuleFilter("TEST");
			filter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeUNLOCO;
			AssertEquals(OrgLocatedWithinModuleFilter.SearchTypeUNLOCO, filter.FilterType);

			filter.FilterType = OrgLocatedWithinModuleFilter.SearchTypeOrgAddress;
			AssertEquals(OrgLocatedWithinModuleFilter.SearchTypeOrgAddress, filter.FilterType);

			filter.Clear();
			AssertEquals(OrgLocatedWithinModuleFilter.SearchTypeUNLOCO, filter.FilterType);
		}

		#region override
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Locations;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrgLocatedWithinModuleFilter GetNewModuleFilter()
		{
			return new OrgLocatedWithinModuleFilter((ZString)"Test", Factory);
		}

		protected override ZString ExpectedDescription => "Test";

		#endregion
	}
}
