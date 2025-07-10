using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CGoodsMeasureProvider))]
sealed class CC044CGoodsMeasureProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CGoodsMeasureProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CGoodsMeasureProvider(null));

	public void TestGrossMassMeasure() => CombineAssertions(() =>
	{
		item.BY_GrossWeight = 5m;
		item.BY_GrossWeightUnit = "KG";
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("No unloadedGoodsItem available because unloaded state is not 'DIF' and not 'NEW'", 0m, Provider.GrossMassMeasure);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("New data", 5m, provider.GrossMassMeasure);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		provider = new CC044CGoodsMeasureProvider(item.UnloadedGoodsItem);
		AssertEquals("No change registered", 0m, provider.GrossMassMeasure);
		item.UnloadedGoodsItem.BY_GrossWeight = 10m;
		AssertEquals("Change registered", 10m, provider.GrossMassMeasure);
	});

	public void TestNetNetWeightMeasure() => CombineAssertions(() =>
	{
		item.BY_NetWeight = 5m;
		item.BY_NetWeightUnit = "KG";
		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("No unloadedGoodsItem available because unloaded state is not 'DIF' and not 'NEW'", 0m, Provider.NetNetWeightMeasure);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("New data", 5m, provider.NetNetWeightMeasure);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		provider = new CC044CGoodsMeasureProvider(item.UnloadedGoodsItem);
		AssertEquals("No change registered", 0m, provider.NetNetWeightMeasure);
		item.UnloadedGoodsItem.BY_NetWeight = 10m;
		AssertEquals("Change registered", 10m, provider.NetNetWeightMeasure);
	});

	public void TestTariffQuantity() => AssertEquals(0m, Provider.TariffQuantity);

	public void TestSupplementaryUnitsQty() => AssertNull(Provider.SupplementaryUnitsQty);

	protected override CC044CGoodsMeasureProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();

		provider = new CC044CGoodsMeasureProvider(item);
	}

	NctsArrivalCargoDesc item;
	CC044CGoodsMeasureProvider provider;
}
