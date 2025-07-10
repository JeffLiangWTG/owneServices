using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CommodityProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonCargoDesc", "Value cannot be null.\r\nParameter name: goodsItem", () => new CommodityProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonCargoDesc.Header", () => new CommodityProvider(Factory.New<NctsDepartureCargoDesc>()));
		});
	}

	public void TestCusCode()
	{
		goodsItem.BY_CusC4Number = "123";
		AssertEquals("123", Provider.CusCode);
	}

	public void TestDescriptionOfGoods()
	{
		goodsItem.BY_Description = "TestDescription";
		AssertEquals("TestDescription", Provider.DescriptionOfGoods);
	}

	public void TestDescriptionOfGoodsMaxLength()
	{
		const int inNCTSTPPeriod = 280;
		const int outNCTSTPPeriod = 512;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("in phase5", inNCTSTPPeriod, GetProvider().DescriptionOfGoodsMaxLength);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("not in phase5", outNCTSTPPeriod, GetProvider().DescriptionOfGoodsMaxLength);
			}
		});
	}

	public void TestCommodityCode() => AssertNotNull(Provider.CommodityCode);

	public void TestDangerousGoods()
	{
		var dangerousDataItem1 = goodsItem.UNDGs.AddNew();
		var dangerousDataItem2 = goodsItem.UNDGs.AddNew();
		var dangerousSubstance = Factory.New<UNDGSubstance>();

		dangerousDataItem1.DI_DG = dangerousSubstance.PK;
		dangerousDataItem2.DI_DG = dangerousSubstance.PK;

		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("2 DangerousGoods", 2, Provider.DangerousGoods.Count);
			AssertEquals("DangerousGoods Sequence number should start with 1", "1", Provider.DangerousGoods.First().SequenceNumber);
			AssertEquals("2nd DangerousGoods Sequence number should be 2", "2", Provider.DangerousGoods.Last().SequenceNumber);
		});
	}

	public void TestDangerousGoods_UNDGSubstanceIsMissing()
	{
		var dangerousDataItem1 = goodsItem.UNDGs.AddNew();
		_ = goodsItem.UNDGs.AddNew();
		var dangerousSubstance = Factory.New<UNDGSubstance>();

		dangerousDataItem1.DI_DG = dangerousSubstance.PK;
		Factory.Save();

		AssertEquals("1 DangerousGoods", 1, Provider.DangerousGoods.Count);
	}

	public void TestGoodsMeasure() => AssertNotNull(Provider.GoodsMeasure);

	protected override CommodityProvider GetProvider() => new CommodityProvider(goodsItem);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
	}

	NctsDepartureCargoDesc goodsItem;
}
