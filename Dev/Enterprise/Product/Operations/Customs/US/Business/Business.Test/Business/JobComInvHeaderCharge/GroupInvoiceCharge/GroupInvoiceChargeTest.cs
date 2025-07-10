using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GroupInvoiceCharge))]
	sealed class GroupInvoiceChargeTest : Customs.Business.Testing.BaseGroupInvoiceChargeTest
	{
		public void TestExchangeCurrencyCodeChanged_RefreshExchangeRates()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			var nzdCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			nzdCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 4), 1.05m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 2), 0.65m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2012, 3, 3), new ZDateTime(2012, 3, 3), 0.66m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 5);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;

			var charge = declaration.TopGroupInvoice.Charges.AddNew();
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals(1.05m, charge.J7_ExchangeRate);
			AssertEquals(new ZDateTime(2012, 3, 4), declaration.TopGroupInvoice.EffectiveValuationDate);

			declaration.JE_ExportDate = new ZDateTime(2012, 3, 2);
			var charge2 = declaration.TopGroupInvoice.Charges.AddNew();
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(0.65m, charge2.J7_ExchangeRate);

			declaration.JE_ExportDate = new ZDateTime(2012, 3, 3);
			charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;
			AssertEquals(0.66m, charge.J7_ExchangeRate);
		}

		public void TestDefaultValueOfInclInLineForDeductionChargeIsYes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			var charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.DeductionCharge;
			AssertEquals(true, charge.J7_IsIncludedInITOT);
			AssertEquals(Customs.Common.GroupIsIncludedInLinesOptionList.Codes.Yes, charge.J7_Calc_IsIncludedInITOT);
		}

		void SetUpExchangeRates(ZString currencyCode, ZDateTime date, ZDateTime endDate, ZDecimal rate)
		{
			Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode).SetUpExchangeRates(date, endDate, rate);
		}
	}
}
