using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTRUOMCodeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var list = packedItem.Lookups.TRUOMCodeList;

			CombineAssertions("TR UOM CodeList items", () =>
			{
				AssertEquals(true, list.ContainsCode("B32"));
				AssertEquals(true, list.ContainsCode("KNI"));
				AssertEquals(true, list.ContainsCode("MTK"));
				AssertEquals(true, list.ContainsCode("NCR"));
				AssertEquals(false, list.ContainsCode("XXX"));
			});
		}

		public void TestBanderolTariffList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			var tr = Core.Constants.CountryCodes.Turkey;
			var start = ZDateTime.MinSmallDateTimeValue;
			var end = ZDateTime.MaxSmallDateTimeValue;

			var tariffType = helper.CreateTariffType(tr, TaxCodeList.RelatedMiscCodes.BanderolTariffType);
			Factory.Save();

			var tariff = helper.CreateTariff(tr, tariffType.PK, "5B09", start, end);
			helper.CreateTariffAttribute("ISETRADEBANDEROL", "Y", tariff);

			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "SCD");
			var rateCode = helper.CreateCusRateCode(Factory, "75", rateType.PK);
			helper.CreateTaxOrFee("75", 119, Core.Constants.CountryCodes.Turkey, 0, 0, "OTH", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "E-Trade Stamp Tax");
			helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "VFD * 0.20", null, "20%", Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			helper.CreateRate(tariff, rateCode.PK, start, end, dataGrouping: tr);
			Factory.Save();

			packedItem.API_Tariff = "950691900000";
			packedItem.BanderolTariff = "5B09";
			AssertNotNull(packedItem.BanderolTariffList);

			var tariffs = packedItem.BanderolTariffList;
			tariffs.Load();

			CombineAssertions("TR Banderol Tariff List", () =>
			{
				AssertEquals("Tariff Code", "5B09", tariffs[0].ZZ1_TariffCode);
				AssertEquals("Tariff Type Code", "ETRBN", tariffs[0].ZZ1_ZZI_TariffTypeCode);
				AssertEquals("Data Grouping", "TR", tariffs[0].ZZ1_ZZZ_NKDataGrouping);
				AssertEquals("Attribute", "Y", tariffs[0].GetAttribute("ISETRADEBANDEROL").ZZ3_Value);
				AssertEquals("Rate Code", "75", tariffs[0].Rates[0].RateCode);
			});
		}

		AsycudaPackedItem GetNewPackedItemForTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ExportCountry = Core.Constants.CountryCodes.Turkey;
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItem;
			packedItem.API_NetWeight = 10;
			packedItem.API_NetWeightUQ = Core.Constants.Weight.Kilograms;

			return packedItem;
		}

		public void TestAdditionalCodeShouldExist()
		{
			SetupDataForGetAdditionalCodesForRateType("SCD");

			var packedItem = GetNewPackedItemForTest(Factory);

			packedItem.API_Tariff = "1000";
			var codes = packedItem.Lookups.TariffAdditionalCodeList;

			CombineAssertions(() =>
			{
				AssertNotNull(codes);
				AssertEquals(2, codes.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "AC01", "AC02" }, codes.GetAllCodes());
				AssertContainsExactElementsInAnyOrder(new[] { "EU AC01", "EU AC02" }, codes.Cast<ZArchitecture.Core.CodeDescriptionPair>().Select(c => c.Description));
			});
		}

		public void TestAdditionalCodeShouldNotExist()
		{
			SetupDataForGetAdditionalCodesForRateType("SCD");

			var packedItem = GetNewPackedItemForTest(Factory);

			packedItem.API_Tariff = "2000";
			var codes = packedItem.Lookups.TariffAdditionalCodeList;
			AssertNotNull(codes);
			AssertEquals(0, codes.Count);
		}

		void SetupDataForGetAdditionalCodesForRateType(string rateTypeCode)
		{
			var (helper, testRate1, testRate2, tradeGroup) = SetupRatesForGetAdditionalCodes(rateTypeCode);

			helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC01");
			helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "AC02");

			SetupCusCodeForGetAdditionalCodes(helper);
		}

		void SetupCusCodeForGetAdditionalCodes(UniversalReferenceTestDataHelper helper)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.CountryCodes.Turkey);

			var groupingKey = Core.Constants.CountryCodes.Turkey;
			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var nextMonth = ZDateTime.Today.AddMonths(1);

			helper.CreateNewOrGetExistingCusCodeList(groupingKey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(groupingKey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC02", "EU AC02", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);

			Factory.Save();
		}

		(UniversalReferenceTestDataHelper, RateView, RateView, CusRefTradeGroupView) SetupRatesForGetAdditionalCodes(string rateTypeCode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateRefCusTaxOrFeeType("VAT");
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey");

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);

			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Turkey, Constants.TariffTypes.HarmonizedSystem, nomenclatureGroupType: "TR", ensureDataGroupingExists: false);
			Factory.Save();

			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, s1p1TariffType.PK, "1000", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey,
				"EU",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime,
				ensureDataGroupingExists: false);

			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Turkey);
			var rateType = helper.CreateCusRateType(Core.Constants.CountryCodes.Turkey, rateTypeCode, ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "10", rateType.PK);
			var testRate1 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "24.3 * [FLAT]");
			var testRate2 = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "4.3 * [FLAT]");

			return (helper, testRate1, testRate2, tradeGroup);
		}
	}
}
