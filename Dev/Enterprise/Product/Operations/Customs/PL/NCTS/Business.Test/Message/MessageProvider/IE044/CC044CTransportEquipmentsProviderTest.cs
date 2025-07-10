using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business.Message.MessageProviders.IE044;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CTransportEquipmentsProviderTest : DataProviderTestCase<CC044CTransportEquipmentsProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalHeaderContainer", "Value cannot be null.\r\nParameter name: nctsContainer", () => new CC044CTransportEquipmentsProvider(1, null));
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
			AssertEquals("Empty List", 0, Provider.GoodsReference.Count);

			var bill1 = container.NctsArrival.Bills.AddNew();
			var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var goodsItem2 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_DeclarationGoodsItemNumber = 2;
			var goodsItem3 = bill1.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_DeclarationGoodsItemNumber = 3;
			var bill2 = container.NctsArrival.Bills.AddNew();
			var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_DeclarationGoodsItemNumber = 4;

			var provider = GetProvider();
			AssertSequencesEqual("SequenceNumbers", new[] { "1", "2", "3", "4" }, provider.GoodsReference.Select(x => x.SequenceNumber));
			AssertSequencesEqual("DeclarationGoodsItemNumbers", new[] { "1", "2", "3", "4" }, provider.GoodsReference.Select(x => x.DeclarationGoodsItemNumber));
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

	protected override CC044CTransportEquipmentsProvider GetProvider() => new CC044CTransportEquipmentsProvider(99, container);

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType("A");
		container = nctsHeader.ArrivalHeaderContainers.AddNew();
		container.BC_ContainerNum = "A";
	}
	NctsHeader nctsHeader;
	NctsArrivalHeaderContainer container;
}
