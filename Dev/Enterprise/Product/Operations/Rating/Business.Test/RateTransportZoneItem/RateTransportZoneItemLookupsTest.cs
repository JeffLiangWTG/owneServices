using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateTransportZoneItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCityTownPostCodeCollection()
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "International Space Station";
			cityTown.R9_RN_NKCountry = Constants.CountryCodes.Australia;
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "Z1000";
			postCode.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			var postCode2 = Factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "Z2000";
			postCode2.RK_RN_NKCountry = Constants.CountryCodes.Australia;
			cityTown.PostCodes.Add(postCode);
			cityTown.PostCodes.Add(postCode2);
			Factory.Save();

			var prov = Factory.New<RateTransportProvider>();
			var zone = prov.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();
			zoneItem.TQ_RN_NKCountry = Constants.CountryCodes.Australia;
			zoneItem.TQ_ToPostCode = "Z2000";
			zoneItem.TQ_R9_CityTown = cityTown.PK;
			Assert("To post code will be cleared when city town is set", zoneItem.TQ_ToPostCode.IsEmpty);

			AssertEquals(2, zoneItem.Lookups.PostCodes.Count);
			AssertEquals("Z1000", zoneItem.Lookups.PostCodes[0].RK_CityTownPostCode);
			AssertEquals("Z2000", zoneItem.Lookups.PostCodes[1].RK_CityTownPostCode);

			zoneItem.TQ_FromPostCode = postCode.RK_CityTownPostCode;
			AssertEquals(1, zoneItem.Lookups.CityTowns.Count);
			AssertEquals("International Space Station", zoneItem.Lookups.CityTowns[0].R9_InternationalName);
		}

		public void TestCityTownCollection_ParentTransportProviderHasCountry_AddDefaultFilterByCountry()
		{
			var transportZoneSet = Factory.New<RateTransportProvider>();
			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Bulgaria;
			var zone = transportZoneSet.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.CityTowns.FilterBusinessObjectDefaults["CountryState" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);

			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Austria;
			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.CityTowns.FilterBusinessObjectDefaults["CountryState" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);
		}

		public void TestCityTownCollection_ParentTransportProviderHasZoneHub_AddDefaultFilterByCountry()
		{
			var zoneHubLocation = Factory.New<RefCityTown>();
			zoneHubLocation.R9_RN_NKCountry = Constants.CountryCodes.Bulgaria;
			var transportZoneSet = Factory.New<RateTransportProvider>();
			transportZoneSet.TP_R9_ZoneHubLocation = zoneHubLocation.PK;
			var zone = transportZoneSet.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.CityTowns.FilterBusinessObjectDefaults["CountryState" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);

			transportZoneSet.ZoneHubLocation.R9_RN_NKCountry = Constants.CountryCodes.Austria;
			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.CityTowns.FilterBusinessObjectDefaults["CountryState" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"].Value);
		}

		public void TestPostCodeCollection_ParentTransportProviderHasCountry_AddDefaultFilterByCountry()
		{
			var transportZoneSet = Factory.New<RateTransportProvider>();
			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Bulgaria;
			var zone = transportZoneSet.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.PostCodes.FilterBusinessObjectDefaults["Country" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			transportZoneSet.TP_RN_NKCountry = Constants.CountryCodes.Austria;
			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.PostCodes.FilterBusinessObjectDefaults["Country" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestPostCodeCollection_ParentTransportProviderHasZoneHub_AddDefaultFilterByCountry()
		{
			var zoneHubLocation = Factory.New<RefCityTown>();
			zoneHubLocation.R9_RN_NKCountry = Constants.CountryCodes.Bulgaria;
			var transportZoneSet = Factory.New<RateTransportProvider>();
			transportZoneSet.TP_R9_ZoneHubLocation = zoneHubLocation.PK;
			var zone = transportZoneSet.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.PostCodes.FilterBusinessObjectDefaults["Country" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			transportZoneSet.ZoneHubLocation.R9_RN_NKCountry = Constants.CountryCodes.Austria;
			AssertEquals(Constants.CountryCodes.Bulgaria, zoneItem.Lookups.PostCodes.FilterBusinessObjectDefaults["Country" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}
	}
}
