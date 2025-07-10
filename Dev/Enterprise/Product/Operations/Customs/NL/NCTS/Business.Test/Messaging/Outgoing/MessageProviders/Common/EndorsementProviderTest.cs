using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class EndorsementProviderTest : Customs.Business.Testing.DataProviderTestCase<EndorsementProvider>
{
	public void TestDate()
	{
		incident.BN_EndorsementDate = new ZDateTime(2024, 08, 01, 16, 05, 52);
		AssertEquals(new ZDateTime(2024, 08, 01, 16, 05, 52), Provider.Date);
	}

	public void TestAuthority()
	{
		incident.BN_EndorsementAuthority = "PEETERS";
		AssertEquals("PEETERS", Provider.Authority);
	}

	public void TestPlace()
	{
		incident.BN_EndorsementPlace = "BREDA";
		AssertEquals("BREDA", Provider.Place);
	}

	public void TestCountry()
	{
		incident.BN_EndorsementCountryCode = "NL";
		AssertEquals("NL", Provider.Country);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		incident = nctsHeader.EnRouteIncidents.AddNew();

		provider = new EndorsementProvider(incident);
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;
	EndorsementProvider provider;

	protected override EndorsementProvider GetProvider() => provider;
}
