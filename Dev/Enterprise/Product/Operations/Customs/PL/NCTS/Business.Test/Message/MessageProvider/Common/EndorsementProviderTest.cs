using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class EndorsementProviderTest : Customs.Business.Testing.DataProviderTestCase<EndorsementProvider>
{
	readonly DateTime testDateValue = DateTime.Today;
	const string TestAuthorityValue = "ABC";
	const string TestPlaceValue = "place";
	const string TestCountryValue = Core.Constants.CountryCodes.Poland;

	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null EnRouteIncident", EndorsementProvider.NewOrNull(null));

			incident.BN_EndorsementDate = ZDateTime.Today;
			incident.BN_EndorsementAuthority = "ABC";
			incident.BN_EndorsementPlace = "place";
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
			AssertNotNull("All endorsement fields not empty", EndorsementProvider.NewOrNull(incident));

			incident.BN_EndorsementDate = ZDateTime.Empty;
			AssertNull("Endorsement date is empty", EndorsementProvider.NewOrNull(incident));
			incident.BN_EndorsementDate = ZDateTime.Today;

			incident.BN_EndorsementAuthority = ZString.Empty;
			AssertNull("Endorsement authority is empty", EndorsementProvider.NewOrNull(incident));
			incident.BN_EndorsementAuthority = "ABC";

			incident.BN_EndorsementPlace = ZString.Empty;
			AssertNull("Endorsement place is empty", EndorsementProvider.NewOrNull(incident));
			incident.BN_EndorsementPlace = "place";

			incident.BN_EndorsementCountryCode = ZString.Empty;
			AssertNull("Endorsement country code is empty", EndorsementProvider.NewOrNull(incident));
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
		});
	}

	public void TestDate()
	{
		AssertEquals(testDateValue, Provider.Date);
	}

	public void TestAuthority()
	{
		AssertEquals(TestAuthorityValue, Provider.Authority);
	}

	public void TestPlace()
	{
		AssertEquals(TestPlaceValue, Provider.Place);
	}

	public void TestCountry()
	{
		AssertEquals(TestCountryValue, Provider.Country);
	}

	protected override EndorsementProvider GetProvider()
	{
		return EndorsementProvider.NewOrNull(incident);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		incident = nctsHeader.EnRouteIncidents.AddNew();
		incident.BN_EndorsementDate = testDateValue;
		incident.BN_EndorsementAuthority = TestAuthorityValue;
		incident.BN_EndorsementPlace = TestPlaceValue;
		incident.BN_EndorsementCountryCode = TestCountryValue;
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;
}
