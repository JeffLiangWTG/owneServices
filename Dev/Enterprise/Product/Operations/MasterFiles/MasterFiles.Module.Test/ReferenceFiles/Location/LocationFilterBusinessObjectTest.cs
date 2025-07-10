using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(LocationFilterBusinessObject))]
	sealed class LocationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text Filters Tests

		public void TestActiveFilterAppliedByDefault()
		{
			RefCountry country1 = Factory.NewWithValidTestData<RefCountry>();
			RefCountry country2 = Factory.NewWithValidTestData<RefCountry>();

			country1.RN_Code = "AB";
			country1.RN_Desc = "Desc";
			country1.RN_IsActive = true;

			country2.RN_Code = "XY";
			country2.RN_Desc = "ASC";
			country2.RN_IsActive = false;

			LocationCollection locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			CheckFilterForCountry(country1, true, country2, false, locations, filter);
		}

		public void TestUnloco_Code()
		{
			RefUNLOCO unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			RefCountry country = Factory.NewWithValidTestData<RefCountry>();
			RefUNLOCO unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Asc";
			unloco2.RL_RN_NKCountryCode = country.Code;

			Factory.Save();
			LocationCollection locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "ABC";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			CheckFilterForUNLoco(unloco1, true, unloco2, false, locations, filter);
		}

		public void TestUnloco_Description()
		{
			RefUNLOCO unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			RefCountry country = Factory.NewWithValidTestData<RefCountry>();
			RefUNLOCO unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Asc";
			unloco2.RL_RN_NKCountryCode = country.Code;

			Factory.Save();
			LocationCollection locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;
			CheckFilterForUNLoco(unloco1, true, unloco2, false, locations, filter);
		}

		public void TestUnloco_ActiveStatus_True()
		{
			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";
			unloco1.RL_IsActive = true;

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Desc";
			unloco2.RL_RN_NKCountryCode = country.Code;
			unloco2.RL_IsActive = false;

			Factory.Save();
			var locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusActive;
					CheckFilterForUNLoco(unloco1, true, unloco2, false, locations, filter);
				}
			});
		}

		public void TestUnloco_ActiveStatus_False()
		{
			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";
			unloco1.RL_IsActive = true;

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Desc";
			unloco2.RL_RN_NKCountryCode = country.Code;
			unloco2.RL_IsActive = false;

			Factory.Save();
			var locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusInactive;
					CheckFilterForUNLoco(unloco1, false, unloco2, true, locations, filter);
				}
			});
		}

		public void TestUnloco_ActiveStatus_Combined()
		{
			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABC";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";
			unloco1.RL_IsActive = true;

			unloco2.RL_Code = "XYZ";
			unloco2.RL_PortName = "Desc";
			unloco2.RL_RN_NKCountryCode = country.Code;
			unloco2.RL_IsActive = false;

			Factory.Save();
			var locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			AllLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					((ModuleTextFilter)filter["Active Status"]).Property = FilterStripBusinessObject.StatusAll;
					CheckFilterForUNLoco(unloco1, true, unloco2, true, locations, filter);
				}
			});
		}

		public void TestCountry()
		{
			RefCountry country1 = Factory.NewWithValidTestData<RefCountry>();
			RefCountry country2 = Factory.NewWithValidTestData<RefCountry>();

			country1.RN_Code = "AB";
			country1.RN_Desc = "Desc";
			country1.RN_IsActive = true;

			country2.RN_Code = "XY";
			country2.RN_Desc = "ASC";
			country2.RN_IsActive = false;

			LocationCollection locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "AB";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			CheckFilterForCountry(country1, true, country2, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;
			CheckFilterForCountry(country1, true, country2, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "Active";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForCountry(country1, true, country2, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "Inactive";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForCountry(country1, false, country2, true, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "All";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForCountry(country1, true, country2, true, locations, filter);
		}

		public void TestZone()
		{
			var region1 = Factory.NewWithValidTestData<RefZoneHeader>();
			var region2 = Factory.NewWithValidTestData<RefZoneHeader>();
			var regionWRS = Factory.NewWithValidTestData<RefZoneHeader>();

			region1.FZ_Code = "AB";
			region1.FZ_Description = "Desc";
			region1.FZ_IsActive = true;

			region2.FZ_Code = "XY";
			region2.FZ_Description = "ASC";
			region2.FZ_IsActive = false;

			regionWRS.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			regionWRS.FZ_Code = "ZY";
			regionWRS.FZ_Description = "Desc";

			Factory.Save();
			LocationCollection locations = new LocationCollection(Factory);

			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Code"]).Property = "AB";
			((ModuleTextFilter)filter["Code"]).IsActive = true;
			CheckFilterForZone(region1, true, region2, false, regionWRS, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "Desc";
			((ModuleTextFilter)filter["Description"]).IsActive = true;
			CheckFilterForZone(region1, true, region2, false, regionWRS, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "Active";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForZone(region1, true, region2, false, regionWRS, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "Inactive";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForZone(region1, false, region2, true, regionWRS, false, locations, filter);

			filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Active Status"]).Property = "All";
			((ModuleTextFilter)filter["Active Status"]).IsActive = true;
			CheckFilterForZone(region1, true, region2, true, regionWRS, false, locations, filter);
		}

		public void TestLocationType()
		{
			var filter = new LocationFilterBusinessObject();
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			((ModuleTextFilter)filter["Location Type"]).Property = "Country";
			AssertEquals("LocationType property should be Country ", LocationTypeEnum.Country, filter.LocationType);

			((ModuleTextFilter)filter["Location Type"]).Property = "ASD";
			AssertEquals("Default locationType property should be Port", LocationTypeEnum.Port, filter.LocationType);

			((ModuleTextFilter)filter["Location Type"]).Property = "Int. Zone";
			AssertEquals("LocationType property should be Country ", LocationTypeEnum.InternationalZone, filter.LocationType);
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			var locations = new LocationCollection(Factory);
			locations.LoadZone(filter.Filter);

			Assert(locations.Cast<RefZoneHeader>().All(x => x.FZ_ZoneType != RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean));

			((ModuleTextFilter)filter["Location Type"]).Property = "Port";
			AssertEquals("LocationType property should be Country ", LocationTypeEnum.Port, filter.LocationType);
		}

		#endregion

		public void TestChangeSqlFilterTypeAccordingly()
		{
			var filter = new LocationFilterBusinessObject { QueryObjectType = typeof(ILocation) };
			var moduleFilters = filter.ModuleFilters;
			var sqlFilter = moduleFilters[FilterStripBusinessObject.CustomSqlFilterDescription] as ModuleSQLFilter;
			AssertEquals(typeof(RefUNLOCO), sqlFilter.QueryObjectType);

			((ModuleTextFilter)filter["Location Type"]).Property = "Country";
			AssertEquals(typeof(RefCountry), sqlFilter.QueryObjectType);

			((ModuleTextFilter)filter["Location Type"]).Property = "Port";
			AssertEquals(typeof(RefUNLOCO), sqlFilter.QueryObjectType);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LocationFilterBusinessObject();
		}

		void CheckFilterForUNLoco(RefUNLOCO alpha, bool alphaPresent, RefUNLOCO bravo, bool bravoPresent, LocationCollection locations, LocationFilterBusinessObject filter)
		{
			((ModuleTextFilter)filter["Location Type"]).Property = "Port";
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			locations.LoadUNLoco(filter.Filter);
			AssertCollectionWithBools(alpha, alphaPresent, locations);
			AssertCollectionWithBools(bravo, bravoPresent, locations);
		}

		void CheckFilterForCountry(RefCountry alpha, bool alphaPresent, RefCountry bravo, bool bravoPresent, LocationCollection locations, LocationFilterBusinessObject filter)
		{
			((ModuleTextFilter)filter["Location Type"]).Property = "Country";
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			locations.LoadCountry(filter.Filter);
			AssertCollectionWithBools(alpha, alphaPresent, locations);
			AssertCollectionWithBools(bravo, bravoPresent, locations);
		}

		void CheckFilterForZone(RefZoneHeader alpha, bool alphaPresent, RefZoneHeader bravo, bool bravoPresent, RefZoneHeader charlie, bool charliePresent, LocationCollection locations, LocationFilterBusinessObject filter)
		{
			((ModuleTextFilter)filter["Location Type"]).Property = "Int. Zone";
			((ModuleTextFilter)filter["Location Type"]).IsActive = true;

			locations.LoadZone(filter.Filter);
			AssertCollectionWithBools(alpha, alphaPresent, locations);
			AssertCollectionWithBools(bravo, bravoPresent, locations);
			AssertCollectionWithBools(charlie, charliePresent, locations);
		}

		void AssertCollectionWithBools(object value, bool isPresent, LocationCollection locations)
		{
			if (isPresent)
			{
				AssertCollectionContains(value, locations);
			}
			else
			{
				AssertCollectionNotContains(value, locations);
			}
		}

		#endregion
	}
}
