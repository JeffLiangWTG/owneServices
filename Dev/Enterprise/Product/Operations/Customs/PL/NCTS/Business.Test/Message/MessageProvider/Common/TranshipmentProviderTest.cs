using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TranshipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TranshipmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TranshipmentProvider(null));
	}

	public void TestContainerIndicator()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Incident without containers", NCTSIndicator.NO, Provider.ContainerIndicator);

			var container = incident.IncidentContainers.AddNew();
			AssertEquals("Incident without containerised containers", NCTSIndicator.NO, Provider.ContainerIndicator);

			container.BC_Mode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("Incident has containerised container", NCTSIndicator.YES, Provider.ContainerIndicator);
		});
	}

	public void TestTransportMeans()
	{
		CombineAssertions(() =>
		{
			AssertNull("TransportMeans is null if all nested items are empty", GetProvider().TransportMeans);

			incident.BN_TransportAtDepartureType = "A";
			AssertEquals("TransportMeans is added and contains TypeOfIdentification", "A", GetProvider().TransportMeans.TypeOfIdentification);

			incident.BN_TransportAtDepartureType = ZString.Empty;
			incident.BN_TransportAtDepartureID = "B";
			AssertEquals("TransportMeans is added and contains IdentificationNumber", "B", GetProvider().TransportMeans.IdentificationNumber);

			incident.BN_TransportAtDepartureID = ZString.Empty;
			incident.BN_RN_NKTransportAtDepartureIDNationality = "C";
			AssertEquals("TransportMeans is added and contains Nationality", "C", GetProvider().TransportMeans.Nationality);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		incident = header.EnRouteIncidents.AddNew();
	}

	EnRouteIncident incident;

	protected override TranshipmentProvider GetProvider() => new TranshipmentProvider(incident);
}
