using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(TransportEquipmentsForNctsHeaderContainerProvider))]
sealed class TransportEquipmentsProviderForNctsHeaderContainerProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForNctsHeaderContainerProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentsForNctsHeaderContainerProvider(null, 1));
	}

	public void TestGoodsReferences()
	{
		var bill = container.Header.Bills.AddNew();

		var cusInBondCargoDesc1 = bill.GoodsItems.AddNew();
		var package1 = cusInBondCargoDesc1.Packages.AddNew();
		cusInBondCargoDesc1.BY_DeclarationGoodsItemNumber = 1;
		package1.ContainersPivot.AddPivotFor(container);

		var cusInBondCargoDesc2 = bill.GoodsItems.AddNew();
		var package2 = cusInBondCargoDesc2.Packages.AddNew();
		cusInBondCargoDesc2.BY_DeclarationGoodsItemNumber = 2;
		package2.ContainersPivot.AddPivotFor(container);

		AssertContainsExactElementsInAnyOrder(new[] { (1, 1), (2, 2) }, Provider.GoodsReferences.Select(x => (x.SequenceNumeric, x.GoodsItemNumericValue)));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestId()
	{
		container.BC_ContainerNum = "contnr";
		AssertEquals("contnr", Provider.Id);
	}

	public void TestSealsAffixedQuantity()
	{
		container.BC_Seal1 = "seal1";
		container.BC_Seal2 = "seal2";
		container.AdditionalSeals.AddNew().BK_SealNumber = "seal3";
		AssertEquals(3, Provider.SealsAffixedQuantity);
	}

	public void TestSeals()
	{
		container.BC_Seal1 = "seal1";
		container.BC_Seal2 = "seal2";
		container.AdditionalSeals.AddNew().BK_SealNumber = "seal3";
		AssertArrayEqualsByElements(new[] { (1, "seal1"), (2, "seal2"), (3, "seal3") },
			provider.Seals.Select(x => (x.SequenceNumeric, x.Id)).ToArray());
	}

	protected override TransportEquipmentsForNctsHeaderContainerProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		container = header.DepartureHeaderContainers.AddNew();
		provider = new TransportEquipmentsForNctsHeaderContainerProvider(container, 1);
	}

	TransportEquipmentsForNctsHeaderContainerProvider provider;
	NctsDepartureHeaderContainer container;
}
