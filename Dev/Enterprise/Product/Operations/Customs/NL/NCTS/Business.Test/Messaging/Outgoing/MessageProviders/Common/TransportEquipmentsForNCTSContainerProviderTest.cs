using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(TransportEquipmentsForNCTSContainerProvider))]
sealed class TransportEquipmentsForNCTSContainerProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForNCTSContainerProvider>
{
	public void TestSequenceNumeric() => AssertEquals("SequenceNumeric", 1, Provider.SequenceNumeric);

	public void TestId()
	{
		container.BC_ContainerNum = "MSCU1234566";
		AssertEquals("Incident Code 2", "MSCU1234566", Provider.Id);
	}

	public void TestSealsAffixedQuantity()
	{
		container.BC_Seal1 = "1234";
		container.BC_Seal2 = "5678";
		var seal = container.Seals.AddNew();
		seal.BK_SealNumber = "91011";
		AssertEquals("SealsAffixedQuantity", 3, Provider.SealsAffixedQuantity);
	}

	public void TestGoodsReferences() => CombineAssertions(() =>
	{
		container.ItemNumbers.AddNew();
		AssertNotNull(Provider.GoodsReferences);
		AssertEquals("Number of GoodsReferences", 1, Provider.GoodsReferences.Count);
	});

	public void TestSeals() => CombineAssertions(() =>
	{
		container.BC_Seal1 = "1234";
		container.BC_Seal2 = "5678";
		var seal = container.Seals.AddNew();
		seal.BK_SealNumber = "91011";
		AssertNotNull(Provider.Seals);
		AssertEquals("Number of seals", 3, Provider.Seals.Count);
	});

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ExportFlag = "Y";
		incident = nctsHeader.EnRouteIncidents.AddNew();
		incident.BN_IncidentCode = "2";
		container = incident.IncidentContainers.AddNew();

		provider = new TransportEquipmentsForNCTSContainerProvider(container, 1);
	}

	NctsHeader nctsHeader;
	EnRouteIncident incident;
	NctsContainer container;
	TransportEquipmentsForNCTSContainerProvider provider;

	protected override TransportEquipmentsForNCTSContainerProvider GetProvider() => provider;
}
