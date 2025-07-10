using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TransportEquipmentsForArrivalProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForArrivalProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsContainer", "Value cannot be null.\r\nParameter name: nctsContainer", () => new TransportEquipmentsForArrivalProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestContainerIdentificationNumber() => AssertEquals("A", Provider.ContainerIdentificationNumber);

	public void TestNumberOfSeals()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", "0", Provider.NumberOfSeals);

			container.BC_Seal1 = "A";
			AssertEquals("BC_Seal1", "1", GetProvider().NumberOfSeals);

			container.BC_Seal1 = ZString.Empty;
			container.BC_Seal2 = "A";
			AssertEquals("BC_Seal2", "1", GetProvider().NumberOfSeals);

			container.BC_Seal2 = ZString.Empty;
			container.Seals.AddNew();
			AssertEquals("1 AdditionalSeal", "1", GetProvider().NumberOfSeals);

			container.BC_Seal1 = "A";
			container.BC_Seal2 = "A";
			AssertEquals("BC_Seal1 and BC_Seal2 and 1 AdditionalSeal", "3", GetProvider().NumberOfSeals);
		});
	}

	public void TestSeal()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, Provider.Seal.Count);

			container.BC_Seal1 = "A";
			var providerWithOneSeal = GetProvider();
			AssertEquals("BC_Seal1", 1, providerWithOneSeal.Seal.Count);
			AssertEquals("BC_Seal1 sequenceNumber", "1", providerWithOneSeal.Seal.First().SequenceNumber);
			AssertEquals("BC_Seal1 identifier", "A", providerWithOneSeal.Seal.First().Identifier);

			container.BC_Seal2 = "B";
			var providerWithTwoSeals = GetProvider();
			AssertEquals("BC_Seal1 and BC_Seal2", 2, providerWithTwoSeals.Seal.Count);
			AssertEquals("BC_Seal1 sequenceNumber", "1", providerWithTwoSeals.Seal.First().SequenceNumber);
			AssertEquals("BC_Seal1 identifier", "A", providerWithTwoSeals.Seal.First().Identifier);
			AssertEquals("BC_Seal2 sequenceNumber", "2", providerWithTwoSeals.Seal.Last().SequenceNumber);
			AssertEquals("BC_Seal2 identifier", "B", providerWithTwoSeals.Seal.Last().Identifier);

			var cusSeal = Factory.New<CusSeal>();
			cusSeal.BK_SealNumber = "C";
			container.Seals.Add(cusSeal);
			var providerWithMoreThanTwoSeals = GetProvider();
			AssertEquals("BC_Seal1 and BC_Seal2 and 1 AdditionalSeal", 3, providerWithMoreThanTwoSeals.Seal.Count);
			AssertEquals("first sequenceNumber", "1", providerWithMoreThanTwoSeals.Seal.First().SequenceNumber);
			AssertEquals("last sequenceNumber", "3", providerWithMoreThanTwoSeals.Seal.Last().SequenceNumber);
			AssertEquals("first identifier", "A", providerWithMoreThanTwoSeals.Seal.First().Identifier);
			AssertEquals("last identifier", container.Seals.Cast<CusSeal>().Last().BK_SealNumber, providerWithMoreThanTwoSeals.Seal.Last().Identifier);
		});
	}

	public void TestGoodsReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, GetProvider().GoodsReference.Count);
			var reference1 = container.ItemNumbers.AddNew();
			reference1.CY_DataNumeric = 23;
			reference1.CY_Order = 1;

			var providerWithOneGoodsReference = GetProvider();
			AssertEquals(1, providerWithOneGoodsReference.GoodsReference.Count);
			AssertEquals("first", "23", providerWithOneGoodsReference.GoodsReference.First().DeclarationGoodsItemNumber);

			var reference2 = container.ItemNumbers.AddNew();
			reference2.CY_DataNumeric = 32;
			reference2.CY_Order = 2;
			var providerWithTwoGoodsReferences = GetProvider();
			AssertEquals(2, providerWithTwoGoodsReferences.GoodsReference.Count);
			AssertEquals("first", "23", providerWithTwoGoodsReferences.GoodsReference.First().DeclarationGoodsItemNumber);
			AssertEquals("last", "32", providerWithTwoGoodsReferences.GoodsReference.Last().DeclarationGoodsItemNumber);
		});
	}

	public void TestDamagedSealsNotRetained()
	{
		container.BC_Seal1 = "seal1";
		container.BC_Seal2 = "seal2";
		var additionalSeal1 = container.Seals.AddNew();
		additionalSeal1.BK_SealNumber = "ADD1";
		additionalSeal1.BK_UnloadingState = "NEW";
		var additionalSeal2 = container.Seals.AddNew();
		additionalSeal2.BK_SealNumber = "ADD2";
		additionalSeal2.BK_UnloadingState = "DEC";
		var additionalSeal3 = container.Seals.AddNew();
		additionalSeal3.BK_SealNumber = "ADD3";
		additionalSeal3.BK_UnloadingState = "MIS";
		var additionalSeal4 = container.Seals.AddNew();
		additionalSeal4.BK_SealNumber = "ADD4";
		additionalSeal4.BK_UnloadingState = "DAM";

		AssertEquals("Do not inlude DAM", "5", Provider.NumberOfSeals);
	}

	protected override TransportEquipmentsForArrivalProvider GetProvider() => new TransportEquipmentsForArrivalProvider(99, container);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		var incident = nctsHeader.EnRouteIncidents.AddNew();
		container = incident.IncidentContainers.AddNew();
		container.BC_ContainerNum = "A";
	}
	NctsHeader nctsHeader;
	NctsContainer container;
}
