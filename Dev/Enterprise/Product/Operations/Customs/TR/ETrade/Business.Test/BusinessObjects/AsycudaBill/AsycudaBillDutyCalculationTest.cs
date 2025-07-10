using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaBillDutyCalculationTest : TestCaseWithFactory
	{
		public void TestCalculateBanderolDuty()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.Branch.Company.GC_IsReciprocal = true;

			var bill = header.Bills.AddNew();
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_CustomsQty = 2;
			pack1.PackedItem.API_CustomsUQ = "BI";
			pack1.PackedItem.BanderolTariff = "5.b.11";
			var pack2 = bill.Packs.AddNew();
			pack2.PackedItem.API_CustomsQty = 4;
			pack2.PackedItem.BanderolTariff = "5.b.11";

			header.CalculateDuties();

			var banderolTax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.TRTBandrol);
			if (banderolTax != null)
			{
				CombineAssertions(() =>
				{
					AssertEquals("Arrival Date", ZDateTime.Today, header.AMA_DateAtCustomsOffice);
					AssertEquals("Banderol Type", TaxCodeList.Codes.TRTBandrol, banderolTax.AET_ChargeType);
					AssertEquals("Charge Amount", 180m, banderolTax.AET_ChargeAmount);
					AssertEquals("Rate", 6m, banderolTax.AET_Rate);
					AssertEquals("Base Value", 30m, banderolTax.AET_BaseValue);
				});
			}
		}

		public void TestCalculateDutyWithAdditionalCodes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExportCountry = Core.Constants.CountryCodes.Germany;
				bill.ABL_CustomsValue = 500;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack1 = bill.Packs.AddNew();
				var pack2 = bill.Packs.AddNew();
				pack1.PackedItem.API_GoodsValue = (ZDecimal)300;
				pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack1.PackedItem.API_Tariff = "1000";
				pack1.PackedItem.API_ChemicalSubstanceCode = "AC1";

				Factory.Save();

				header.CalculateDuties();
				var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

				CombineAssertions(() =>
				{
					AssertEquals(10m, tax.AET_Rate);
					AssertEquals(100m, tax.AET_ChargeAmount);
					AssertEquals(TaxCodeList.Codes.CustomsDuty ,tax.AET_ChargeType);
					AssertEquals(MethodOfPaymentList.Codes.Cash, tax.AET_MethodOfPayment);
					AssertEquals(1000m, tax.AET_BaseValue);
				});
			}
		}

		public void TestCalculateDutyCombinedWithSCD()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExportCountry = Core.Constants.CountryCodes.Turkey;
			bill.ABL_CustomsValue = 400;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_Tariff = "1000";
			pack1.PackedItem.API_ChemicalSubstanceCode = "AC1";

			Factory.Save();

			header.CalculateDuties();
			var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			CombineAssertions(() =>
			{
				AssertEquals("AET_Rate", 20m , tax.AET_Rate);
				AssertEquals("AET_ChargeAmount", 160m, tax.AET_ChargeAmount);
				AssertEquals("AET_ChargeType", TaxCodeList.Codes.CustomsDuty, tax.AET_ChargeType);
				AssertEquals("AET_MethodOfPayment", MethodOfPaymentList.Codes.Cash, tax.AET_MethodOfPayment);
				AssertEquals("AET_BaseValue", 800m, tax.AET_BaseValue);
			});
		}

		public void TestCalculateDutyCombinedWithSCDWithoutTradeGroup()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExportCountry = Core.Constants.CountryCodes.Turkey;
			bill.ABL_CustomsValue = 100;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_Tariff = "2000";

			Factory.Save();

			header.CalculateDuties();
			var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			CombineAssertions(() =>
			{
				AssertEquals("AET_Rate", 20m, tax.AET_Rate);
				AssertEquals("AET_ChargeAmount", 40m, tax.AET_ChargeAmount);
				AssertEquals("AET_ChargeType", TaxCodeList.Codes.CustomsDuty, tax.AET_ChargeType);
				AssertEquals("AET_MethodOfPayment", MethodOfPaymentList.Codes.Cash, tax.AET_MethodOfPayment);
				AssertEquals("AET_BaseValue", 200m, tax.AET_BaseValue);
			});
		}

		public void TestCalculateDutyCombinedWithExemptionCodes()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExemptionCode1 = "ILAC18";
			bill.ABL_CustomsValue = 80;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			bill.ExportCountry = Core.Constants.CountryCodes.Germany;
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_GoodsValue = 100;
			pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			pack1.PackedItem.API_Tariff = "1000";
			Factory.Save();

			header.CalculateDuties();
			var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			CombineAssertions(() =>
			{
				AssertEquals("AET_Rate", 38m, tax.AET_Rate);
				AssertEquals("AET_ChargeAmount", 60.80m, tax.AET_ChargeAmount);
				AssertEquals("AET_ChargeType", TaxCodeList.Codes.CustomsDuty, tax.AET_ChargeType);
				AssertEquals("AET_MethodOfPayment", MethodOfPaymentList.Codes.Cash, tax.AET_MethodOfPayment);
				AssertEquals("AET_BaseValue", 160m, tax.AET_BaseValue);
			});
		}

		public void TestGetFixedDutyFormulaShouldNoException()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExportCountry = Core.Constants.CountryCodes.Turkey;
			bill.ABL_CustomsValue = 400;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			Factory.Save();

			header.CalculateDuties();
			AssertNoExceptionThrown("NoExceptionThrown", () => header.CalculateDuties());

			var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
			AssertNull(tax);

			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_Tariff = ZString.Empty;
			header.CalculateDuties();
			tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			AssertNull(tax);

			pack1.PackedItem.API_Tariff = "1000";
			header.CalculateDuties();
			tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			CombineAssertions(() =>
			{
				AssertEquals("AET_Rate", 20m, tax.AET_Rate);
				AssertEquals("AET_ChargeAmount", 160m, tax.AET_ChargeAmount);
				AssertEquals("AET_ChargeType", TaxCodeList.Codes.CustomsDuty, tax.AET_ChargeType);
				AssertEquals("AET_MethodOfPayment", MethodOfPaymentList.Codes.Cash, tax.AET_MethodOfPayment);
				AssertEquals("AET_BaseValue", 800m, tax.AET_BaseValue);
			});
		}

		void PrepareReferenceTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffTypeHSN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "HSN");
			var tariffTypeETR = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETR");
			var tariffTypeETRBN = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETRBN");

			var rateTypeSCD = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "SCD");
			var rateTypeETR = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "ETR");
			var rateTypeDTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "DTY");
			var rateTypeBAN = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "BAN");

			var rateCode10SCD = helper.CreateCusRateCode(Factory, "10", rateTypeSCD.PK);
			var rateCode10DTY = helper.CreateCusRateCode(Factory, "10", rateTypeDTY.PK);
			var rateCode10ETR = helper.CreateCusRateCode(Factory, "10", rateTypeETR.PK);
			var rateCode75BAN = helper.CreateCusRateCode(Factory, "75", rateTypeBAN.PK);

			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);

			var tradeGroupCountryEU = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", startDate, endDate);
			var tradeGroupCountryAllCountries = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "All Countries", startDate, endDate);

			Factory.Save();

			var tariffHSN1 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "1000", startDate, endDate);
			var tariffHSN2 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "2000", startDate, endDate);
			var tariffHSN3 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeHSN.PK, "3000", startDate, endDate);

			var rateHSN1 = helper.CreateRefCusRate(tariffHSN1.PK, rateCode10SCD.PK, startDate, endDate, "VFD * 0.10", null, "10%", Core.Constants.CountryCodes.Turkey);
			var rateHSN2 = helper.CreateRefCusRate(tariffHSN2.PK, rateCode10SCD.PK, startDate, endDate, "VFD * 0.10", null, "10%", Core.Constants.CountryCodes.Turkey);
			var rateHSN3 = helper.CreateRefCusRate(tariffHSN2.PK, rateCode10DTY.PK, startDate, endDate, "VFD * 0.12", null, "12%", Core.Constants.CountryCodes.Turkey);

			var tariffETRBN1 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeETRBN.PK, "5.b.11", startDate, endDate);
			var rateETRBN1 = helper.CreateRefCusRate(tariffETRBN1.PK, rateCode75BAN.PK, startDate, endDate, "15*[BI]", null, "", Core.Constants.CountryCodes.Turkey);

			var tariffETR1 = helper.CreateTariff(Core.Constants.CountryCodes.Turkey, tariffTypeETR.PK, "ILAC18", startDate, endDate);
			var rateETR1 = helper.CreateRefCusRate(tariffETR1.PK, rateCode10ETR.PK, startDate, endDate, "VFD * 0.18", null, "18%", Core.Constants.CountryCodes.Turkey);

			CurrencyTestHelper curHelper = new CurrencyTestHelper(Factory);
			curHelper.SetExchangeRate(curHelper.EURCurrency, 2m, ZDateTime.Today);
			curHelper.SetExchangeRate(curHelper.USDCurrency, 3m, ZDateTime.Today);
			curHelper.SetExchangeRate(curHelper.TRYCurrency, 1m, ZDateTime.Today);

			helper.AddCountry(tradeGroupCountryEU, Core.Constants.CountryCodes.Germany);
			helper.CreateCusApplicability(rateHSN1.PK, tradeGroupCountryAllCountries, startDate, endDate);
			helper.CreateCusApplicability(rateHSN1.PK, tradeGroupCountryAllCountries, startDate, endDate, "AC1");
			helper.CreateCusApplicability(rateETRBN1.PK, tradeGroupCountryAllCountries, startDate, endDate);
			helper.CreateCusApplicability(rateETR1.PK, tradeGroupCountryEU, startDate, endDate);
			helper.CreateCusApplicability(rateHSN3.PK, tradeGroupCountryAllCountries, startDate, endDate);
			helper.CreateCusApplicability(rateHSN2.PK, tradeGroupCountryAllCountries, startDate, endDate);
		}

		public void TestTariffWithoutSCDRates()
		{
			PrepareTestDataForRefSys();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExportCountry = Core.Constants.CountryCodes.Turkey;
			bill.ABL_CustomsValue = 100;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_Tariff = "3000";

			Factory.Save();

			header.CalculateDuties();
			var tax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
			AssertNull(tax);
		}

		void PrepareTestDataForRefSys()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType("TRSCDRATE", "TRSCDRATE DESCRIPTION", "TRSCDRATE LONG DESCRIPTION");
			helper.CreateRefSysConfig("TRSCDRATE", "VFD * 0.20", new ZDateTime(2023, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareReferenceTestData();
		}
	}
}
