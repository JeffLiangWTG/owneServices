using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class LocationProviderTest : TestCaseWithFactory
	{
		public void TestGetCountryQuery()
		{
			var locationProvider = new LocationProvider();
			var country1 = Factory.NewWithValidTestData<RefCountry>();
			var country2 = Factory.NewWithValidTestData<RefCountry>();

			country1.RN_Code = "AB";
			country1.RN_Desc = "Desc";
			country1.RN_IsActive = true;

			country2.RN_Code = "XY";
			country2.RN_Desc = "XYDesc";
			country2.RN_IsActive = false;

			var locations = new LocationCollection(Factory);
			locations.LoadCountry(locationProvider.GetLocationQuery(LocationTypeEnum.Country, "XY"));
			AssertCollectionNotContains("country2 would not be hitted due to the LocationFilterBusinessObject.GetLocationTypeQuery.", country2, locations);

			locations.LoadCountry(locationProvider.GetLocationQuery(LocationTypeEnum.Country, "AB"));
			AssertCollectionContains("country1 would be hitted due to the code query.", country1, locations);

			locations.LoadCountry(locationProvider.GetLocationQuery(LocationTypeEnum.Country, "Desc"));
			AssertCollectionContains("country1 would be hitted due to the description query.", country1, locations);

			locations.LoadCountry(locationProvider.GetLocationQuery(LocationTypeEnum.Country, "--"));
			AssertEquals("No element would be loaded.", 0, locations.Count);
		}

		public void TestGetPortQuery()
		{
			var locationProvider = new LocationProvider();
			var unloco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			var country = Factory.NewWithValidTestData<RefCountry>();
			var unloco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			unloco1.RL_Code = "ABCDE";
			unloco1.RL_RN_NKCountryCode = country.Code;
			unloco1.RL_PortName = "Desc";
			unloco1.RL_IsActive = true;

			unloco2.RL_Code = "VWXYZ";
			unloco2.RL_PortName = "VWXYZDesc";
			unloco2.RL_RN_NKCountryCode = country.Code;
			unloco2.RL_IsActive = false;

			var locations = new LocationCollection(Factory);
			locations.LoadUNLoco(locationProvider.GetLocationQuery(LocationTypeEnum.Port, "VWXYZ"));
			AssertCollectionNotContains("unloco2 would not be hitted due to the LocationFilterBusinessObject.GetLocationTypeQuery.", unloco2, locations);

			locations.LoadUNLoco(locationProvider.GetLocationQuery(LocationTypeEnum.Port, "ABCDE"));
			AssertCollectionContains("unloco1 would be hitted due to the code query.", unloco1, locations);

			locations.LoadUNLoco(locationProvider.GetLocationQuery(LocationTypeEnum.Port, "ABCDEDesc"));
			AssertCollectionContains("unloco1 would be hitted due to the description query.", unloco1, locations);

			locations.LoadUNLoco(locationProvider.GetLocationQuery(LocationTypeEnum.Port, "--"));
			AssertEquals("No element would be loaded.", 0, locations.Count);
		}

		public void TestGetZoneQuery()
		{
			var locationProvider = new LocationProvider();
			var region1 = Factory.NewWithValidTestData<RefZoneHeader>();
			var region2 = Factory.NewWithValidTestData<RefZoneHeader>();
			var regionWRS = Factory.NewWithValidTestData<RefZoneHeader>();

			region1.FZ_Code = "AB";
			region1.FZ_Description = "Desc";
			region1.FZ_IsActive = true;

			region2.FZ_Code = "XY";
			region2.FZ_Description = "XYDesc";
			region2.FZ_IsActive = false;

			regionWRS.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean;
			regionWRS.FZ_Code = "ZY";
			regionWRS.FZ_Description = "ZYDesc";

			var locations = new LocationCollection(Factory);
			locations.LoadZone(locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, "XY"));
			AssertCollectionNotContains("region2 would not be hitted due to the LocationFilterBusinessObject.GetLocationTypeQuery.", region2, locations);

			locations.LoadZone(locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, "ZY"));
			AssertCollectionNotContains("regionWRS would not be hitted due to the LocationFilterBusinessObject.GetLocationTypeQuery.", regionWRS, locations);

			locations.LoadZone(locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, "AB"));
			AssertCollectionContains("region1 would be hitted due to the code query.", region1, locations);

			locations.LoadZone(locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, "Desc"));
			AssertCollectionContains("region1 would be hitted due to the description query.", region1, locations);

			locations.LoadZone(locationProvider.GetLocationQuery(LocationTypeEnum.InternationalZone, "--"));
			AssertEquals("No element would be loaded.", 0, locations.Count);
		}
	}
}
