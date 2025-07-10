using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(LocationForIncidentsProvider))]
sealed class LocationForIncidentsProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationForIncidentsProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new LocationForIncidentsProvider(null));

	public void TestQualifierOfIdentification()
	{
		location.CGL_Qualifier = "V";
		AssertEquals("V", Provider.QualifierOfIdentification);
	}

	public void TestUnLocode() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("Location has qualifier not equal to U", Provider.UnLocode);
		location.CGL_Qualifier = "U";
		location.CGL_AdditionalIdentifier = "NLRTM";
		AssertEquals("Location qualifier is U", "NLRTM", Provider.UnLocode);
	});

	public void TestCountry() => CombineAssertions(() =>
	{
		AssertNullOrEmpty("No Event Country Code", Provider.Country);
		incident.BN_EventCountryCode = "NL";
		AssertEquals("Event Country Code NL", "NL", Provider.Country);
	});

	public void TestGNSSLongitude() => CombineAssertions(() =>
	{
		AssertNull("Qualifier not W", Provider.GNSSLongitude);
		location.CGL_Qualifier = "W";
		location.Address.E2_Longitude = 4.5232m;
		AssertEquals("Qualifier is W", "4.5232", Provider.GNSSLongitude);
	});

	public void TestGNSSLatitute() => CombineAssertions(() =>
	{
		AssertNull("Qualifier not W", Provider.GNSSLatitute);
		location.CGL_Qualifier = "W";
		location.Address.E2_Latitude = 48.9134m;
		AssertEquals("Qualifier is W", "48.9134", Provider.GNSSLatitute);
	});

	public void TestAddress() => CombineAssertions(() =>
	{
		AssertNull("Qualifier not Z", Provider.Address);
		location.CGL_Qualifier = "Z";
		AssertNotNull("Qualifier is Z", Provider.Address);
		AssertType<AddressProvider>(Provider.Address);
	});

	public void TestLocation() => AssertNull(Provider.Location);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		incident = nctsHeader.EnRouteIncidents.AddNew();
		location = incident.GoodsLocation as CusGoodsLocation;
		provider = new LocationForIncidentsProvider(incident);
	}

	protected override LocationForIncidentsProvider GetProvider() => provider;

	NctsHeader nctsHeader;
	EnRouteIncident incident;
	CusGoodsLocation location;
	LocationForIncidentsProvider provider;
}
