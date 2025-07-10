using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class IncidentProviderTest : Customs.Business.Testing.DataProviderTestCase<IncidentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null EnRouteIncident", "Value cannot be null.\r\nParameter name: routeIncident", () => new IncidentProvider(0, null));
			AssertNoExceptionThrown("EnRouteIncident is not null", () => new IncidentProvider(0, incident));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(testIncedentNumber.ToString(), Provider.SequenceNumber);
	}

	public void TestCode()
	{
		const string testCode = "A";
		incident.BN_IncidentCode = testCode;
		AssertEquals(testCode, Provider.Code);
	}

	public void TestText()
	{
		const string testText = "Test";
		incident.BN_Information = testText;
		AssertEquals(testText, Provider.Text);
	}

	public void TestEndorsement()
	{
		CombineAssertions(() =>
		{
			incident.BN_EndorsementDate = ZDateTime.Today;
			incident.BN_EndorsementAuthority = "ABC";
			incident.BN_EndorsementPlace = "place";
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
			AssertNotNull("All endorsement fields not empty", Provider.Endorsement);

			incident.BN_EndorsementDate = ZDateTime.Empty;
			AssertNull("Endorsement date is empty", new IncidentProvider(0, incident).Endorsement);
			incident.BN_EndorsementDate = ZDateTime.Today;

			incident.BN_EndorsementAuthority = ZString.Empty;
			AssertNull("Endorsement authority is empty", new IncidentProvider(0, incident).Endorsement);
			incident.BN_EndorsementAuthority = "ABC";

			incident.BN_EndorsementPlace = ZString.Empty;
			AssertNull("Endorsement place is empty", new IncidentProvider(0, incident).Endorsement);
			incident.BN_EndorsementPlace = "place";

			incident.BN_EndorsementCountryCode = ZString.Empty;
			AssertNull("Endorsement country code is empty", new IncidentProvider(0, incident).Endorsement);
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
		});
	}

	public void TestLocation()
	{
		AssertNotNull(Provider.Location);
	}

	public void TestTransportEquipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, GetProvider().TransportEquipment.Count);

			incident.IncidentContainers.AddNew();
			incident.IncidentContainers.AddNew();
			AssertEquals("2 IncidentContainers", 2, GetProvider().TransportEquipment.Count);
			AssertEquals("1st IncidentContainers sequenceNumber", "1", GetProvider().TransportEquipment.First().SequenceNumber);
			AssertEquals("2nd IncidentContainers sequenceNumber", "2", GetProvider().TransportEquipment.Last().SequenceNumber);
		});
	}

	public void TestTranshipment()
	{
		AssertType<TranshipmentProvider>(Provider.Transhipment);
	}

	protected override IncidentProvider GetProvider()
	{
		return new IncidentProvider(testIncedentNumber, incident);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		incident = nctsHeader.EnRouteIncidents.AddNew();
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;

	const int testIncedentNumber = 42;
}
