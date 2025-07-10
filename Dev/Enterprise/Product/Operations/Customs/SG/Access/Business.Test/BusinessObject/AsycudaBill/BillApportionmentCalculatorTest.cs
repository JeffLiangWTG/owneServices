using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class BillApportionmentCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2010, 01, 01)]
		public void TestCalculate_Apportionment()
		{
			var bill = CreateBillForCalculation();
			var pack = bill.Packs[0];
			var packedItem = pack.PackedItem;
			var header = packedItem.Header;
			header.AMA_ManifestType = Constants.ManifestType.Import;
			AssertEquals("Original Value", 15m, packedItem.API_CustomsValue);
			var calculator = new SGBillApportionmentCalculator(bill);
			calculator.Calculate();
			AssertEquals("ConvertedLinePrice + ((otherCharges + transportValue + insuranceValue - discountValue) * LineFactor", 445m, packedItem.API_CustomsValue);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			calculator = new SGBillApportionmentCalculator(bill);
			calculator.Calculate();
			AssertEquals(false, bill.ApportionmentDirty);
			AssertEquals("ConvertedLinePrice + ((otherCharges - discountValue) * LineFactor", 320m, packedItem.API_CustomsValue);
			pack.LinePrice = 90m;
			calculator = new SGBillApportionmentCalculator(bill);
			calculator.Calculate();
			AssertEquals(false, bill.ApportionmentDirty);
			AssertEquals("LinePrice is changed.", 50m, packedItem.API_CustomsValue);
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;
			calculator = new SGBillApportionmentCalculator(bill);
			calculator.Calculate();
			AssertEquals(false, bill.ApportionmentDirty);
			AssertEquals("LinePriceCurrency is changed.", 95m, packedItem.API_CustomsValue);
		}

		[TestDate(2010, 01, 01)]
		public void TestCalculate_Statistics()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");
			var bill = CreateBillForCalculation();
			((Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill).CycleDate = ZDateTime.Today;
			bill.ABL_CustomsValue = 0m;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Singapore;
			bill.TaxAmount = 0m;
			bill.DutyAmount = 0m;
			bill.ApportionmentDirty = false;
			AssertBillValues(bill, 0m, 0m, 0m);
			bill.ApportionmentDirty = true;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Australia;
			Factory.Save();
			AssertBillValues(bill, 445m, 33.81m, 38m);
		}

		public void TestApportionmentUseExactConvertion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");

			var idr = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Indonesia) ?? Factory.New<RefCurrency>();
			if (!idr.IsInDatabase)
			{
				idr.FillWithValidTestData();
				idr.RX_Code = Core.Constants.CurrencyCodes.Indonesia;
			}

			var now = ZDateTime.Now;
			var rate = idr.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = now.AddDays(-2);
			rate.RE_ExpiryDate = now.AddDays(2);
			rate.RE_SellRate = 10000m;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			((Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill).CycleDate = ZDateTime.Today;
			bill.ABL_TransportValue = 0m;
			bill.ABL_InsuranceValue = 0m;
			bill.OtherChargesValue = 0m;
			bill.DiscountValue = 0m;
			bill.ABL_CustomsValue = 0m;
			bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			bill.DutyAmount = 0m;
			bill.TaxAmount = 0m;
			var pack1 = bill.Packs.AddNew();
			pack1.APA_LineNo = 1;
			pack1.LinePrice = 1m;
			pack1.LinePriceCurrency = Core.Constants.CurrencyCodes.Indonesia;
			var packedItem1 = pack1.PackedItem;
			packedItem1.API_DutyAmount = 0.01m;
			var pack2 = bill.Packs.AddNew();
			pack2.APA_LineNo = 2;
			pack2.LinePrice = 1m;
			pack2.LinePriceCurrency = Core.Constants.CurrencyCodes.Indonesia;
			var packedItem2 = pack2.PackedItem;
			packedItem2.API_DutyAmount = 0.01m;
			var pack3 = bill.Packs.AddNew();
			pack3.APA_LineNo = 3;
			pack3.LinePrice = 0m;
			pack3.LinePriceCurrency = Core.Constants.CurrencyCodes.Indonesia;
			var packedItem3 = pack3.PackedItem;
			packedItem3.API_DutyAmount = 0.01m;
			var pack4 = bill.Packs.AddNew();
			pack4.APA_LineNo = 4;
			pack4.LinePrice = 1m;
			pack4.LinePriceCurrency = Core.Constants.CurrencyCodes.Indonesia;
			var packedItem4 = pack4.PackedItem;
			packedItem4.API_DutyAmount = 0.01m;
			new SGBillApportionmentCalculator(bill).Calculate();
			AssertPackedItem(packedItem1, 0.01m, 0m, 0.01m);
			AssertPackedItem(packedItem2, 0.01m, 0m, 0.01m);
			AssertPackedItem(packedItem3, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem4, 0.01m, 0m, 0.01m);
			AssertBillValues(bill, 0.03m, 0m, 0.04m);
			pack1.LinePrice = 0m;
			pack2.LinePrice = 0m;
			pack3.LinePrice = 0m;
			pack4.LinePrice = 0m;
			pack1.PackedItem.API_CustomsValue = 1m;
			pack2.PackedItem.API_CustomsValue = 1m;
			pack3.PackedItem.API_CustomsValue = 1m;
			pack4.PackedItem.API_CustomsValue = 1m;
			new SGBillApportionmentCalculator(bill).Calculate();
			AssertPackedItem(packedItem1, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem2, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem3, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem4, 0m, 0m, 0.01m);
			AssertBillValues(bill, 0m, 0m, 0.04m);
			rate.RE_SellRate = 0.1m;
			Factory.Save();
			pack1.LinePrice = 1m;
			pack2.LinePrice = 1m;
			pack4.LinePrice = 1m;
			bill.OtherChargesValue = 250m;
			bill.DiscountValue = 150m;
			bill.ABL_TransportValue = 60m;
			bill.ABL_InsuranceValue = 40m;
			pack1.PackedItem.API_CustomsValue = 1m;
			pack2.PackedItem.API_CustomsValue = 1m;
			pack3.PackedItem.API_CustomsValue = 1m;
			pack4.PackedItem.API_CustomsValue = 1m;
			new SGBillApportionmentCalculator(bill).Calculate();
			AssertPackedItem(packedItem1, 676.67m, 47.37m, 0.01m);
			AssertPackedItem(packedItem2, 676.67m, 47.37m, 0.01m);
			AssertPackedItem(packedItem3, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem4, 676.66m, 47.37m, 0.01m);
			AssertBillValues(bill, 2030m, 142.11m, 0.04m);
			header.AMA_ManifestType = Constants.ManifestType.Export;
			pack1.PackedItem.API_CustomsValue = 1m;
			pack2.PackedItem.API_CustomsValue = 1m;
			pack3.PackedItem.API_CustomsValue = 1m;
			pack4.PackedItem.API_CustomsValue = 1m;
			bill.TaxAmount = 110m;
			new SGBillApportionmentCalculator(bill).Calculate();
			AssertPackedItem(packedItem1, 343.33m, 24.03m, 0.01m);
			AssertPackedItem(packedItem2, 343.33m, 24.03m, 0.01m);
			AssertPackedItem(packedItem3, 0m, 0m, 0.01m);
			AssertPackedItem(packedItem4, 343.34m, 24.03m, 0.01m);
			AssertBillValues(bill, 1030m, 72.09m, 0.04m);
		}

		void AssertPackedItem(AsycudaPackedItem packedItem, ZDecimal customsValue, ZDecimal taxAmount, ZDecimal dutyAmount)
		{
			CombineAssertions(() =>
			{
				var lineNo = packedItem.Pack.APA_LineNo;
				AssertEquals(lineNo + " CustomsValue", customsValue, packedItem.API_CustomsValue);
				AssertEquals(lineNo + " TaxAmount", taxAmount, packedItem.API_TaxAmount);
				AssertEquals(lineNo + " DutyAmount", dutyAmount, packedItem.API_DutyAmount);
			});
		}

		void AssertBillValues(AsycudaBill bill, ZDecimal customsValue, ZDecimal taxAmount, ZDecimal dutyAmount)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Should be false after calculate the bill.", false, bill.ApportionmentDirty);
				AssertEquals("ABL_CustomsValue", customsValue, bill.ABL_CustomsValue);
				AssertEquals("ABL_RX_NKCustomsValueCurrency", Core.Constants.CurrencyCodes.Singapore, bill.ABL_RX_NKCustomsValueCurrency);
				AssertEquals("TaxAmount", taxAmount, bill.TaxAmount);
				AssertEquals("DutyAmount", dutyAmount, bill.DutyAmount);
			});
		}

		AsycudaBill CreateBillForCalculation()
		{
			var now = ZDateTime.Now;
			var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates) ?? Factory.New<RefCurrency>();
			if (!currency.IsInDatabase)
			{
				currency.FillWithValidTestData();
				currency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			}

			var rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = now.AddDays(-2);
			rate.RE_ExpiryDate = now.AddDays(2);
			rate.RE_SellRate = 2;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = Constants.ManifestType.Import;
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.ABL_TransportValue = 120m;
			bill.ABL_InsuranceValue = 130m;
			bill.OtherChargesValue = 70m;
			bill.DiscountValue = 60m;
			bill.ABL_CustomsValue = 0m;
			bill.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.DutyAmount = 0m;
			bill.TaxAmount = 0m;
			var pack = bill.Packs.AddNew();
			pack.FillWithValidTestData();
			pack.LinePrice = 630m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var packedItem = pack.PackedItem;
			packedItem.API_CustomsValue = 15m;
			packedItem.API_DutyAmount = 38m;
			return bill;
		}
	}
}
