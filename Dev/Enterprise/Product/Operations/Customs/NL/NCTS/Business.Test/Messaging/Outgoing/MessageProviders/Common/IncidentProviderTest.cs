using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(IncidentProvider))]
sealed class IncidentProviderTest : Customs.Business.Testing.DataProviderTestCase<IncidentProvider>
{
	public void TestCode()
	{
		incident.BN_IncidentCode = "1";
		AssertEquals("1", Provider.Code);
	}

	public void TestSequenceNumeric() => AssertEquals("SequenceNumeric", 1, Provider.SequenceNumeric);

	public void TestText()
	{
		incident.BN_Information = "INFORMATION";
		AssertEquals("INFORMATION", Provider.Text);
	}

	public void TestEndorsement() => CombineAssertions(() =>
	{
		incident.BN_EndorsementDate = ZDateTime.Today;
		AssertNotNull(Provider.Endorsement);
		AssertType<EndorsementProvider>(Provider.Endorsement);
	});

	public void TestLocation() => CombineAssertions(() =>
	{
		incident.GoodsLocation.CGL_Qualifier = "T";
		AssertNotNull(Provider.Location);
		AssertType<LocationForIncidentsProvider>(Provider.Location);
	});

	public void TestTransportEquipments() => CombineAssertions(() =>
	{
		AssertEquals("Empty TransportEquipments", 0, Provider.TransportEquipments.Count);
		var cnt = incident.IncidentContainers.AddNew();
		cnt.BC_ContainerNum = "MSCU1234566";
		incident.BN_IncidentCode = "3";
		var incidentProvider = new IncidentProvider(incident, 1);
		AssertEquals("1 container in incident", 1, incidentProvider.TransportEquipments.Count);
	});

	public void TestTranshipment() => CombineAssertions(() =>
	{
		AssertNull(Provider.Transhipment);
		incident.BN_IncidentCode = "3";
		AssertNotNull(Provider.Transhipment);
		AssertType<TranshipmentProvider>(Provider.Transhipment);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		incident = nctsHeader.EnRouteIncidents.AddNew();

		provider = new IncidentProvider(incident, 1);
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;
	IncidentProvider provider;

	protected override IncidentProvider GetProvider() => provider;
}
