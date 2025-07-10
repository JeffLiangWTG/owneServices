using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TransportEquipmentForDepartureProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentForDepartureProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsHeaderContainer", "Value cannot be null.\r\nParameter name: container", () => new TransportEquipmentForDepartureProvider(1, container: null, "CC"));
	});

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
			container.AdditionalSeals.AddNew();
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
			var provider = GetProvider();
			AssertEquals("BC_Seal1", 1, provider.Seal.Count);
			AssertEquals("BC_Seal1 sequenceNumber", "1", provider.Seal.First().SequenceNumber);

			container.BC_Seal1 = ZString.Empty;
			container.BC_Seal2 = "A";
			provider = GetProvider();
			AssertEquals("BC_Seal2", 1, provider.Seal.Count);
			AssertEquals("BC_Seal2 sequenceNumber", "1", provider.Seal.First().SequenceNumber);

			container.BC_Seal2 = ZString.Empty;
			container.AdditionalSeals.AddNew();
			provider = GetProvider();
			AssertEquals("1 AdditionalSeal", 1, provider.Seal.Count);
			AssertEquals("AdditionalSeal sequenceNumber", "1", provider.Seal.First().SequenceNumber);

			container.BC_Seal1 = "A";
			container.BC_Seal2 = "B";
			provider = GetProvider();
			AssertEquals("BC_Seal1 and BC_Seal2 and 1 AdditionalSeal", 3, provider.Seal.Count);
			AssertEquals("first sequenceNumber", "1", provider.Seal.First().SequenceNumber);
			AssertEquals("last sequenceNumber", "3", provider.Seal.Last().SequenceNumber);
		});
	}

		public void TestGoodsReference()
		{
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "B";
			var bill1 = nctsHeader.Bills.AddNew();
			var goodsItem1 = bill1.GoodsItems.AddNew();
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			var package1 = goodsItem1.Packages.AddNew();
			var goodsItem2 = bill1.GoodsItems.AddNew();
			goodsItem2.BY_DeclarationGoodsItemNumber = 2;
			var package2 = goodsItem2.Packages.AddNew();
			var goodsItem3 = bill1.GoodsItems.AddNew();
			goodsItem3.BY_DeclarationGoodsItemNumber = 3;
			goodsItem3.Packages.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var goodsItem4 = bill2.GoodsItems.AddNew();
			goodsItem4.BY_DeclarationGoodsItemNumber = 4;
			var package4 = goodsItem4.Packages.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Empty List", 0, Provider.GoodsReference.Count);

			package1.ContainersPivot.AddPivotFor(container);
			package2.ContainersPivot.AddPivotFor(container);
			package4.ContainersPivot.AddPivotFor(container);

			var provider = GetProvider();
			AssertSequencesEqual("SequenceNumbers", new[] { "1", "2", "3" }, provider.GoodsReference.Select(x => x.SequenceNumber));
			AssertSequencesEqual("DeclarationGoodsItemNumbers", new[] { "1", "2", "4" }, provider.GoodsReference.Select(x => x.DeclarationGoodsItemNumber));
		});
	}

	protected override TransportEquipmentForDepartureProvider GetProvider() => new TransportEquipmentForDepartureProvider(99, container, "Common");

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		container = nctsHeader.DepartureHeaderContainers.AddNew();
		container.BC_ContainerNum = "A";
	}
	NctsHeader nctsHeader;
	NctsDepartureHeaderContainer container;
}
