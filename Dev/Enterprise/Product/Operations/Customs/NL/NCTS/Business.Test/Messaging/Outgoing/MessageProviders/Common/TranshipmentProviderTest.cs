using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(TranshipmentProvider))]
sealed class TranshipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TranshipmentProvider>
{
	public void TestContainerIndicator() => CombineAssertions(() =>
	{
		AssertEquals("No containers", false, Provider.ContainerIndicator);
		incident.IncidentContainers.AddNew();
		AssertEquals("ContainerIndicator", true, Provider.ContainerIndicator);
	});

	public void TestTypeOfIdentification()
	{
		incident.BN_TransportAtDepartureType = "1";
		AssertEquals("Type Of Identification", 1, Provider.TypeOfIdentification);
		incident.BN_TransportAtDepartureType = "x";
		AssertNull(Provider.TypeOfIdentification);
	}

	public void TestId()
	{
		incident.BN_TransportAtDepartureID = "123";
		AssertEquals("ID", "123", Provider.Id);
	}

	public void TestNationality()
	{
		incident.BN_RN_NKTransportAtDepartureIDNationality = "NL";
		AssertEquals("Nationality", "NL", Provider.Nationality);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		incident = nctsHeader.EnRouteIncidents.AddNew();
		incident.BN_IncidentCode = "3";

		provider = new TranshipmentProvider(incident);
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;
	TranshipmentProvider provider;

	protected override TranshipmentProvider GetProvider() => provider;
}
