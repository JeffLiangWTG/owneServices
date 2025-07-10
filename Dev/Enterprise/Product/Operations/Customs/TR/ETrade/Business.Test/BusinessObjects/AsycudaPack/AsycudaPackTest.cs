using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTotalTaxOfPacks()
		{
			var helper = new CurrencyTestHelper(Factory);
			helper.SetRefValues();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_DateAtCustomsOffice = ZDateTime.Today;
			header.Branch.Company.GC_IsReciprocal = true;
			var bill = header.Bills.AddNew();
			bill.ExemptionCode1 = "HK18";
			bill.ExemptionCode2 = "DOC";
			bill.ABL_CustomsValue = 80;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			bill.ExportCountry = Core.Constants.CountryCodes.Germany;
			var pack1 = bill.Packs.AddNew();
			pack1.PackedItem.API_GoodsValue = 120;
			pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			pack1.PackedItem.API_Tariff = "1000";
			Factory.Save();

			header.CalculateDuties();
			var totalTax = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
			var taxCount = bill.AsycudaTaxes.Cast<AsycudaTax>().Count(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);

			CombineAssertions("One pack", () =>
			{
				AssertEquals("Tax Count", taxCount, (ZInt)1);
				AssertEquals("Charge Type", totalTax.AET_ChargeType, TaxCodeList.Codes.CustomsDuty);
				AssertEquals("Charge Amount", 258.4m, totalTax.AET_ChargeAmount);
				AssertEquals("Rate", 38m, totalTax.AET_Rate);
			});

			var pack2 = bill.Packs.AddNew();
			pack2.PackedItem.API_GoodsValue = 200;
			pack2.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			pack2.PackedItem.API_Tariff = "1000";
			bill.ABL_CustomsValue = 280;

			header.CalculateDuties();
			taxCount = bill.AsycudaTaxes.Cast<AsycudaTax>().Count(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
			CombineAssertions("Two packs", () =>
			{
				AssertEquals("Tax Count", (ZInt)1, taxCount);
				AssertEquals("Charge Type", TaxCodeList.Codes.CustomsDuty, totalTax.AET_ChargeType);
				AssertEquals("Charge Amount", 1380.4m, totalTax.AET_ChargeAmount);
				AssertEquals("Rate", 58m, totalTax.AET_Rate);
			});
		}

		public void TestClearCharge()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var emptyTax = bill.AsycudaTaxes.AddNew(TaxCodeList.Codes.TRTBandrol);

			AssertEquals("To make sure the new tax exists before calculating.", true, bill.AsycudaTaxes.Contains(emptyTax));
			bill.CalculateBanderolDuty();
			AssertEquals("Should be removed in calculation.", false, bill.AsycudaTaxes.Contains(emptyTax));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
	}
}
