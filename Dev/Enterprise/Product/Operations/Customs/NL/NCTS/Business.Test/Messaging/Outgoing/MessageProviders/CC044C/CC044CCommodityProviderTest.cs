using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CCommodityProvider))]
sealed class CC044CCommodityProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CCommodityProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CCommodityProvider(null));

	public void TestDescriptionOfGoods() => CombineAssertions(() =>
	{
		item.BY_Description = "description";

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS-item", "", Provider.DescriptionOfGoods);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("NEW-item", "description", Provider.DescriptionOfGoods);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		provider = new CC044CCommodityProvider(unloadedItem);

		AssertNullOrEmpty("No changes are made to BY_Description", GetProvider().DescriptionOfGoods);
		unloadedItem.BY_Description = "description changed";
		AssertEquals("Changes are made to BY_Description", "description changed", GetProvider().DescriptionOfGoods);
	});

	public void TestCusCode()
	{
		item.BY_CusC4Number = "CusCode";
		AssertEquals("CusCode", Provider.CusCode);
	}

	public void TestHarmonizedSystemSubHeadingCode() => CombineAssertions(() =>
	{
		item.BY_HarmonisedTariff = "87654321";

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS item", "", Provider.HarmonizedSystemSubHeadingCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("NEW item", "876543", Provider.HarmonizedSystemSubHeadingCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		provider = new CC044CCommodityProvider(unloadedItem);

		AssertNullOrEmpty("No changes are made to BY_HarmonisedTariff", GetProvider().HarmonizedSystemSubHeadingCode);
		unloadedItem.BY_HarmonisedTariff = "12345678";
		AssertEquals("Changes are made to BY_HarmonisedTariff", "123456", GetProvider().HarmonizedSystemSubHeadingCode);
	});

	public void TestCombinedNomenclatureCode() => CombineAssertions(() =>
	{
		item.BY_HarmonisedTariff = "87654321";

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		AssertEquals("MIS item", "", Provider.CombinedNomenclatureCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		AssertEquals("NEW item", "21", Provider.CombinedNomenclatureCode);

		item.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		var unloadedItem = item.UnloadedGoodsItem;
		provider = new CC044CCommodityProvider(unloadedItem);

		AssertNullOrEmpty("No changes are made to HarmonizedSystemSubHeadingCode", GetProvider().CombinedNomenclatureCode);
		unloadedItem.BY_HarmonisedTariff = "12345678";
		AssertEquals("Changes are made to HarmonizedSystemSubHeadingCode", "78", GetProvider().CombinedNomenclatureCode);
		unloadedItem.BY_HarmonisedTariff = "123456";
		AssertNullOrEmpty("Changes are made to HarmonizedSystemSubHeadingCode, length less than 8", GetProvider().CombinedNomenclatureCode);
	});

	public void TestGoodsMeasure() => AssertNotNull(Provider.GoodsMeasure);

	public void TestDangerousGoods() => AssertEquals(0, Provider.DangerousGoods.Count);

	public void TestGrossMass() => AssertEquals(decimal.Zero, Provider.GrossMass);

	public void TestNetMass() => AssertNull(Provider.NetMass);

	protected override CC044CCommodityProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		item = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
		provider = new CC044CCommodityProvider(item);
	}
	CC044CCommodityProvider provider;
	NctsArrivalCargoDesc item;
}
