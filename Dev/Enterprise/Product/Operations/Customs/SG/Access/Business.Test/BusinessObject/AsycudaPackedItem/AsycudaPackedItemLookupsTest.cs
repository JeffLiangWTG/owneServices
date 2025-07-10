using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.SG.V4.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGoodsTypeList()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.DutiableGoods, "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.NormalGoods, "NormalGoods Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.MajorExporter, "MajorExporter Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, Constants.GoodsType.ControlledGoods, "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var list = packedItem.Lookups.GoodsTypeList;
			AssertEquals(4, list.Count);
			AssertEquals("Dutiable Goods", list.GetDescriptionFromCode(Constants.GoodsType.DutiableGoods));
			AssertEquals("Controlled Goods", list.GetDescriptionFromCode(Constants.GoodsType.ControlledGoods));
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var packedItem2 = pack.PackedItem;
			var list2 = packedItem.Lookups.GoodsTypeList;
			list2 = packedItem.Lookups.GoodsTypeList;
			AssertEquals(4, list2.Count);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			var packedItem3 = pack.PackedItem;
			var list3 = packedItem.Lookups.GoodsTypeList;
			list3 = packedItem.Lookups.GoodsTypeList;
			AssertEquals(2, list3.Count);
			AssertEquals(true, list3.ContainsCode(Constants.GoodsType.NormalGoods));
			AssertEquals(true, list3.ContainsCode(Constants.GoodsType.ControlledGoods));
			AssertEquals(false, list3.ContainsCode(Constants.GoodsType.MajorExporter));
			AssertEquals(false, list3.ContainsCode(Constants.GoodsType.DutiableGoods));
		}

		public void TestYesNoList()
		{
			var packedItem = Factory.New<AsycudaPackedItem>();
			Assert("YesNoList should be cached", ReferenceEquals(packedItem.Lookups.YesNoList, packedItem.Lookups.YesNoList));
		}

		[StressTest]
		[TestDate(2018, 1, 2)]
		public void TestTariffList()
		{
			var tariff1 = CreateTariff("99999991", "test1", new ZDateTime(2017, 11, 1), new ZDateTime(2017, 12, 1));
			var tariff2 = CreateTariff("99999992", "test2", new ZDateTime(2017, 12, 1), new ZDateTime(2018, 1, 1));
			var tariff3 = CreateTariff("99999993", "test3", new ZDateTime(2018, 1, 1), new ZDateTime(2018, 2, 1));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_E_DEP = new ZDateTime(2017, 11, 2);
			header.AMA_E_ARV = new ZDateTime(2017, 12, 2);
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			AssertNotNull(packedItem.Lookups.TariffList);
			header.AMA_ManifestType = Constants.ManifestType.Import;
			AssertEquals("Import has Arrival Date as Effective Date", new ZDateTime(2017, 12, 2), packedItem.EffectiveDateForDutyRate);
			var tariffs = packedItem.Lookups.TariffList;
			tariffs.Load();
			AssertCollectionNotContains("Tariff expired", tariff1, tariffs);
			AssertCollectionContains("Tariff applies", tariff2, tariffs);
			AssertCollectionNotContains("Tariff not commenced", tariff3, tariffs);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			AssertEquals("Export has Departure Date as Effective Date", new ZDateTime(2017, 11, 2), packedItem.EffectiveDateForDutyRate);
			tariffs = packedItem.Lookups.TariffList;
			tariffs.Load();
			AssertCollectionContains("Tariff applies", tariff1, tariffs);
			AssertCollectionNotContains("Tariff not commenced", tariff2, tariffs);
			AssertCollectionNotContains("Tariff not commenced", tariff3, tariffs);
			var headerZA = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			headerZA.AMA_E_ARV = new ZDateTime(2017, 11, 2);
			headerZA.AMA_E_DEP = new ZDateTime(2017, 12, 2);
			var packedItemZA = Enterprise.Customs.ASYCUDA.Business.AsycudaPackHelper.CreatePackedItemForTesting(headerZA.Bills.AddNew().Packs.AddNew());
			AssertEquals(new ZDateTime(2018, 1, 2), packedItemZA.EffectiveDateForDutyRate);
			var query = new ZQuery(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualTo, packedItemZA.EffectiveDateForDutyRate);
			query.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, packedItemZA.EffectiveDateForDutyRate);
			var tariffsZA = Factory.Load<TariffView>(query);
			AssertCollectionNotContains(tariff1, tariffsZA);
			AssertCollectionNotContains(tariff2, tariffsZA);
			AssertCollectionContains(tariff3, tariffsZA);
		}

		public void TestTestGoodsTypeListNonTranslatable()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var list = packedItem.Lookups.GoodsTypeList;

			AssertEquals(typeof(UntranslatableCodeDescriptionPairList), list.GetType());
		}

		TariffView CreateTariff(ZString tariffCode, ZString tariffDescription, ZDateTime startDate, ZDateTime endDate)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, tariffCode, startDate, endDate, tariffDescription);
			return tariff;
		}
	}
}
