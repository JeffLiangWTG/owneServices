using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(GoodsMeasureProvider))]
sealed class GoodsMeasureProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsMeasureProvider>
{
	public void TestGrossMassMeasure() => CombineAssertions(() =>
	{
		AssertEquals("empty", decimal.Zero, provider.GrossMassMeasure);
		item.BY_GrossWeight = 1500m;
		item.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
		AssertEquals("filled G", 1.5m, provider.GrossMassMeasure);
		item.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
		AssertEquals("filled KG", 1500m, provider.GrossMassMeasure);
		item.BY_GrossWeight = 10.2000m;
		AssertEquals("normalized", "10.2", Provider.GrossMassMeasure.ToString());
	});

	public void TestNetNetWeightMeasure() => CombineAssertions(() =>
	{
		AssertEquals("empty", decimal.Zero, provider.NetNetWeightMeasure);
		item.BY_NetWeight = 1500m;
		item.BY_NetWeightUnit = Core.Constants.Weight.Grams;
		AssertEquals("filled G", 1.5m, provider.NetNetWeightMeasure);
		item.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
		AssertEquals("filled KG", 1500m, provider.NetNetWeightMeasure);
		item.BY_NetWeight = 10.2000m;
		AssertEquals("normalized", "10.2", Provider.NetNetWeightMeasure.ToString());
	});

	public void TestTariffQuantity()
	{
		AssertEquals(decimal.Zero, provider.TariffQuantity);
	}

	public void TestSupplementaryUnitsQty() => CombineAssertions(() =>
	{
		AssertEquals("empty", null, provider.SupplementaryUnitsQty);
		item.BY_CustomsSecondQuantity = 100;
		AssertEquals("filled", 100m, provider.SupplementaryUnitsQty);
		item.BY_CustomsSecondQuantity = 10.2000m;
		AssertEquals("normalized", "10.2", Provider.SupplementaryUnitsQty.ToString());
	});

	protected override GoodsMeasureProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		item = header.Bills.AddNew().GoodsItems.AddNew();
		provider = new GoodsMeasureProvider(item);
	}

	NctsDepartureCargoDesc item;
	GoodsMeasureProvider provider;
}
