using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CTransportEquipmentProvider))]
sealed class CC044CTransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CTransportEquipmentProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CTransportEquipmentProvider(null));

	public void TestSequenceNumeric()
	{
		container.SequenceNumber = 2;
		AssertEquals(2, Provider.SequenceNumeric);
	}

	public void TestSealsAffixedQuantity()
	{
		var seal1 = container.Seals.AddNew();
		seal1.BK_SealNumber = "seal1";
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		var seal2 = container.Seals.AddNew();
		seal2.BK_SealNumber = "seal2";
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		var seal3 = container.Seals.AddNew();
		seal3.BK_SealNumber = "seal3";
		seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;

		AssertEquals(2, Provider.SealsAffixedQuantity);

		container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertNull(Provider.SealsAffixedQuantity);
	}

	public void TestId() => CombineAssertions(() =>
	{
		container.BC_ContainerNum = "MSCU1234566";
		container.BC_Mode = Core.Constants.ContainerModes.NonContainerised;
		AssertEquals("MSCU1234566", Provider.Id);
		container.BC_Mode = Core.Constants.ContainerModes.Containerised;
		AssertEquals("MSCU1234566", Provider.Id);
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertNullOrEmpty(Provider.Id);
	});

	public void TestSeals()
	{
		var seal1 = container.Seals.AddNew();
		seal1.BK_SealNumber = "seal1";
		seal1.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		var seal2 = container.Seals.AddNew();
		seal2.BK_SealNumber = "seal2";
		seal2.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		var seal3 = container.Seals.AddNew();
		seal3.BK_SealNumber = "seal3";
		seal3.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;

		AssertContainsExactElementsInExactOrder(new[] { (2, "seal2"), (3, "seal3") }, Provider.Seals.Select(x => (x.SequenceNumeric, x.Id)).ToArray());

		container.BC_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals(0, Provider.Seals.Count);
	}

	public void TestGoodsReferences()
	{
		var bill = container.NctsArrival.Bills.AddNew();

		var cusInBondCargoDesc1 = bill.ArrivalGoodsItems.AddNew();
		var package1 = cusInBondCargoDesc1.Packages.AddNew();
		cusInBondCargoDesc1.BY_DeclarationGoodsItemNumber = 1;
		package1.ContainersPivot.AddPivotFor(container);

		var cusInBondCargoDesc2 = bill.ArrivalGoodsItems.AddNew();
		var package2 = cusInBondCargoDesc2.Packages.AddNew();
		cusInBondCargoDesc2.BY_DeclarationGoodsItemNumber = 2;
		package2.ContainersPivot.AddPivotFor(container);
		var package3 = cusInBondCargoDesc2.Packages.AddNew();
		package3.ContainersPivot.AddPivotFor(container);
		var package4 = cusInBondCargoDesc2.Packages.AddNew();
		package4.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package4.ContainersPivot.AddPivotFor(container);

		AssertContainsExactElementsInAnyOrder(new[] { (1, 1), (2, 2), (3, 2) }, Provider.GoodsReferences.Select(x => (x.SequenceNumeric, x.GoodsItemNumericValue)));
	}

	protected override CC044CTransportEquipmentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		container = header.ArrivalHeaderContainers.AddNew();
		provider = new CC044CTransportEquipmentProvider(container);
	}
	NctsArrivalHeaderContainer container;
	CC044CTransportEquipmentProvider provider;
}
