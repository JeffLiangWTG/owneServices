using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IncidentLocationProviderTest : Customs.Business.Testing.DataProviderTestCase<IncidentLocationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null EnRouteIncident", "Value cannot be null.\r\nParameter name: incident", () => new IncidentLocationProvider(null));
			AssertNoExceptionThrown("EnRouteIncident is not null", () => new IncidentLocationProvider(incident));
		});
	}

	public void TestQualifierOfIdentification()
	{
		const string testQualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;

		var location = GetLocation();
		location.CGL_Qualifier = testQualifier;
		AssertEquals(testQualifier, Provider.QualifierOfIdentification);
	}

	public void TestUNLocode()
	{
		const string testUNLocode = "XYZ";

		var location = GetLocation();

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			location.Unlocode = testUNLocode;
			AssertEquals("Correct UNLocode", testUNLocode, Provider.UNLocode);

			location.CGL_ParentTableCode = "XYZ";
			AssertNull($"CGL_ParentTableCode other than \"{CusInBondEventSchema.Constants.Prefix}\"", new IncidentLocationProvider(incident).UNLocode);
			location.CGL_ParentTableCode = CusInBondEventSchema.Constants.Prefix;

			location.CGL_ParentID = ZGuid.NewZGuid();
			AssertNull("CGL_ParentID other than incident.PK", new IncidentLocationProvider(incident).UNLocode);
		});
	}

	public void TestCountry()
	{
		incident.BN_EventCountryCode = Core.Constants.CountryCodes.Poland;
		AssertEquals(Core.Constants.CountryCodes.Poland, Provider.Country);
	}

	public void TestGNSS()
	{
		var location = GetLocation();

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			AssertNotNull($"Unlocode = \"{location.Unlocode}\"", new IncidentLocationProvider(incident).GNSS);

			location.CGL_ParentTableCode = "XYZ";
			AssertNull($"CGL_ParentTableCode other than \"{CusInBondEventSchema.Constants.Prefix}\"", new IncidentLocationProvider(incident).GNSS);
			location.CGL_ParentTableCode = CusInBondEventSchema.Constants.Prefix;

			location.CGL_ParentID = ZGuid.NewZGuid();
			AssertNull("CGL_ParentID other than incident.PK", new IncidentLocationProvider(incident).GNSS);
		});
	}

	public void TestAddress()
	{
		var location = GetLocation();

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			AssertNotNull($"Unlocode = \"{location.Unlocode}\"", new IncidentLocationProvider(incident).Address);

			location.CGL_ParentTableCode = "XYZ";
			AssertNull($"CGL_ParentTableCode other than \"{CusInBondEventSchema.Constants.Prefix}\"", new IncidentLocationProvider(incident).Address);
			location.CGL_ParentTableCode = CusInBondEventSchema.Constants.Prefix;

			location.CGL_ParentID = ZGuid.NewZGuid();
			AssertNull("CGL_ParentID other than incident.PK", new IncidentLocationProvider(incident).Address);
		});
	}

	protected override IncidentLocationProvider GetProvider()
	{
		return new IncidentLocationProvider(incident);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		incident = nctsHeader.EnRouteIncidents.AddNew();
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;

	CusGoodsLocation GetLocation()
	{
		var location = (CusGoodsLocation)incident.GoodsLocation;
		location.CGL_ParentTableCode = CusInBondEventSchema.Constants.Prefix;
		location.CGL_ParentID = incident.PK;
		return location;
	}
}
