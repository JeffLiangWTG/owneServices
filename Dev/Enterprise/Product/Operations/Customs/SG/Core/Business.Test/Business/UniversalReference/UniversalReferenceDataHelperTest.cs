using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class UniversalReferenceDataHelperTest : TestCaseWithFactory
	{
		public void TestLoadLatestTariff()
		{
			AssertNotNull("Tariff View tariff code 1234 exists", tariff);
			tariff = UniversalReferenceDataHelper.LoadLatestTariff(Factory, "5678");
			AssertNull("Tariff View tariff code 5678 does not exist", tariff);
		}

		public void TestCreateTariffExciseRate()
		{
			var exciseRate = helper.CreateTariffExciseRate(tariff, 12.0m);
			Factory.Save();
			AssertEquals("VFD * 0.12", exciseRate.ZZ2_RateFormula);
			AssertRateType(exciseRate, "EXC");
		}

		void AssertRateType(RateView rate, string rateType)
		{
			var rateCodePK = rate.ZZ2_ZY1_RateCode;
			var rateCode = Factory.Load<CusRefRateCodeView>(rateCodePK);
			var cusRateType = RefCusRateType.Loader.Load(Factory, rateCode.ZY1_ZZZ_NKDataGrouping, rateCode.ZY1_RateType);
			AssertEquals(cusRateType.ZZR_RateType, rateType);
		}

		[TestDate(2019, 1, 01)]
		public void TestGetExciseRate()
		{
			helper.CreateTariffExciseRate(tariff, 20);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "8541");
			helper.CreateTariffExciseRate(tariff2, 388, UnitOfQuantityCodeList.Codes.KGM);
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "8542");
			helper.CreateRate(tariff3, helper.CreateRateCode(Constants.RateTypes.Excise).PK, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddDays(-1), "[KGM] * 402.2");
			Factory.Save();
			var rate = tariff.GetExciseRate(ZDateTime.Today);
			AssertEquals(20.0m, rate.PercentageRate);
			rate = tariff2.GetExciseRate(ZDateTime.Today);
			AssertEquals(388.0m, rate.UnitRate);
			AssertEquals(UnitOfQuantityCodeList.Codes.KGM, rate.UnitQty);
			rate = tariff3.GetExciseRate(ZDateTime.Today);
			AssertEquals("Rate outside period, meaning no rate, should have a zero rate", 0m, rate.UnitRate);
			helper.CreateTariffExciseRate(tariff3, 402.2m, UnitOfQuantityCodeList.Codes.KGM);
			Factory.Save();
			Factory.ClearCachedValue<RateView>(string.Join("_", "LoadMostRecentCachedRate", tariff3.PK, Constants.RateTypes.Excise, ZDateTime.Today));
			rate = tariff3.GetExciseRate(ZDateTime.Today);
			AssertEquals("Same rate but within period should be found", 402.2m, rate.UnitRate);
			AssertEquals(UnitOfQuantityCodeList.Codes.KGM, rate.UnitQty);
		}

		[TestDate(2019, 1, 01)]
		public void TestGetDutyRate()
		{
			var stdPreference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.STD, "STD", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			var prfPreference = helper.CreatePreferenceForCountryAndGrouping(PreferentialIndicatorCodeList.Codes.PRF, "PRF", Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore);
			helper.CreateDutyRate(tariff, stdPreference, 20);
			helper.CreateDutyRate(tariff, prfPreference, 15);
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "8541");
			helper.CreateDutyRate(tariff2, stdPreference, 40);
			helper.CreateDutyRate(tariff2, prfPreference, 35);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			Factory.Save();
			invoiceLine.JI_PrimaryPreference = "STD";
			AssertEquals(20.0m, tariff.GetDutyRate(invoiceLine).PercentageRate);
			invoiceLine.JI_PrimaryPreference = "PRF";
			AssertEquals(15.0m, tariff.GetDutyRate(invoiceLine).PercentageRate);
			invoiceLine.JI_PrimaryPreference = "STD";
			AssertEquals(40.0m, tariff2.GetDutyRate(invoiceLine).PercentageRate);
			invoiceLine.JI_PrimaryPreference = "PRF";
			AssertEquals(35.0m, tariff2.GetDutyRate(invoiceLine).PercentageRate);
		}

		public void TestIsDutiableType()
		{
			Assert("Is not dutiable when attribute is undefined", !tariff.IsDutiableType());
			var tariff1 = helper.LoadOrCreateNewTariff(tariffType, "2345");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Alcohol, tariff1);
			Factory.Save();
			Assert("Is dutiable when commodity is ALC", tariff1.IsDutiableType());
			var tariff2 = helper.LoadOrCreateNewTariff(tariffType, "3456");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Tobacco, tariff2);
			Factory.Save();
			Assert("Is dutiable when commodity is TOB", tariff2.IsDutiableType());
			var tariff3 = helper.LoadOrCreateNewTariff(tariffType, "4567");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Vehicle, tariff3);
			Factory.Save();
			Assert("Is dutiable when commodity is VEH", tariff3.IsDutiableType());
			var tariff4 = helper.LoadOrCreateNewTariff(tariffType, "5678");
			helper.CreateTariffAttribute(SGConstants.Attributes.Names.CommodityType, CommodityTypeList.Codes.Petroleum, tariff4);
			Factory.Save();
			Assert("Is dutiable when commodity is PET", tariff4.IsDutiableType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			tariff = helper.LoadOrCreateNewTariff(tariffType, "1234");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		TariffView tariff;
		RefCusTariffType tariffType;
	}
}
