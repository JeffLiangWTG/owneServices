using System;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class GoodsMeasureProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsMeasureProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonCargoDesc", "Value cannot be null.\r\nParameter name: goodsItem", () => new GoodsMeasureProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonCargoDesc.Header", () => new GoodsMeasureProvider(Factory.New<NctsDepartureCargoDesc>()));
		});
	}

	public void TestNetMass_OutsideTransitionPeriod()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			goodsItem.BY_NetWeight = 1000.1234m;
			goodsItem.BY_NetWeightUnit = "G";
			AssertEquals("NetMass in Kgs rounded to 6 digits", 1.000123m, GetProvider().NetMass);
		});
	}

	public void TestNetMass_InTransitionPeriod()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			goodsItem.BY_NetWeight = 1234.123m;
			goodsItem.BY_NetWeightUnit = "G";
			AssertEquals("NetMass in Kgs rounded to 3 digits", 1.234m, GetProvider().NetMass);
		});
	}

	public void TestGrossMass_CheckRuleC0837()
	{
		movementHeader.BM_ReducedDatasetIndicator = false;

		CombineAssertions(() =>
		{
			goodsItem.BY_GrossWeight = 123;
			AssertNotEquals("Assigned", 0, GetProvider().GrossMass);

			movementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("GrossMass not needed", 0m, GetProvider().GrossMass);
		});
	}

	public void TestGrossMass_OutsideTransitionPeriod()
	{
		movementHeader.BM_ReducedDatasetIndicator = false;
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			goodsItem.BY_GrossWeight = 1000.1234m;
			goodsItem.BY_GrossWeightUnit = "G";
			AssertEquals("GrossMass in Kgs rounded to 6 digits", 1.000123m, GetProvider().GrossMass);
		});
	}

	public void TestGrossMass_InTransitionPeriod()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			goodsItem.BY_GrossWeight = 1234.123m;
			goodsItem.BY_GrossWeightUnit = "G";
			AssertEquals("GrossMass in Kgs rounded to 3 digits", 1.234m, GetProvider().GrossMass);
		});
	}

	public void TestSupplementaryUnits()
	{
		CombineAssertions(() =>
		{
			goodsItem.BY_CustomsSecondQuantity = 0;
			AssertNull("SupplementaryUnits is null if BY_CustomsSecondQuantity is zero", GetProvider().SupplementaryUnits);
			goodsItem.BY_CustomsSecondQuantity = 1234;
			AssertEquals("BY_CustomsSecondQuantity rounded to 6 digits", 1234.000000m, GetProvider().SupplementaryUnits);
		});
	}

	protected override GoodsMeasureProvider GetProvider()
	{
		return new GoodsMeasureProvider(goodsItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = header.MovementHeader;
		goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
	}

	NctsDepartureMovementHeader movementHeader;
	NctsDepartureCargoDesc goodsItem;
}
