using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class ValuationDateTrackerTest : TestCaseWithFactory
	{
		public void TestWhenDeclarationHasAllTheSameExportDate()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 1), new ZDateTime(2012, 3, 1), 1.05m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 2);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			declaration.ResumeApportionment();
			AssertEquals("Valuation Date is set to the latest date an exchange rate is available", new ZDateTime(2012, 3, 1), declaration.DateOfValuation);
		}

		public void TestLatestDateIsMoreThan7DaysAgo()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 1), new ZDateTime(2012, 3, 1), 1.05m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 10);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Available rate is more than 7 days ago, therefore should not store any actual valuation date. returns the entered valuation date", new ZDateTime(2012, 3, 10), declaration.DateOfValuation);
		}

		public void TestWhenExchangeRatesAreAvailable()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 1), new ZDateTime(2012, 3, 1), 1.05m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 2), 1.04m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals(new ZDateTime(2012, 3, 1), declaration.DateOfValuation);
		}

		public void TestWhenExportDatesAreDifferent()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 1), new ZDateTime(2012, 3, 1), 1.05m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 2), 1.04m);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = new ZDateTime(2012, 3, 3);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice2.US_DateOfExport = new ZDateTime(2012, 3, 1);
			declaration.ResumeApportionment();
			AssertEquals(new ZDateTime(2012, 3, 2), invoice.EffectiveValuationDate);
			AssertEquals(new ZDateTime(2012, 3, 1), invoice2.EffectiveValuationDate);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 3), 1.03m);
			declaration.RefreshExchangeRates();
			AssertEquals(1.03m, invoice.JZ_InvoiceCurrExRate);
			AssertEquals(new ZDateTime(2012, 3, 3), invoice.EffectiveValuationDate);
			AssertEquals(new ZDateTime(2012, 3, 1), invoice2.EffectiveValuationDate);
		}

		public void TestWhenMultipleForeignCurrenciesExistWithDifferentDateRanges()
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
			declaration.ResumeApportionment();
			AssertEquals("ExRate", 1.05m, invoice.JZ_InvoiceCurrExRate);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			declaration.ResumeApportionment();
			AssertEquals("ExRate does not exist on 4/3", 0m, invoice2.JZ_InvoiceCurrExRate);
			AssertEquals(new ZDateTime(2012, 3, 4), invoice.EffectiveValuationDate);
			declaration.ResumeApportionment();
			AssertEquals("the earliest date when all the foreign currencies have a rate", new ZDateTime(2012, 3, 4), invoice2.EffectiveValuationDate);
		}

		public void TestForeignCurrencyInGroupInvoice()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			audCurr.ExchangeRates.DeleteAll();
			var nzdCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			nzdCurr.ExchangeRates.DeleteAll();
			SetUpExchangeRates(Core.Constants.CurrencyCodes.Australia, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 4), 1.05m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2012, 3, 2), new ZDateTime(2012, 3, 2), 0.65m);
			SetUpExchangeRates(Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2012, 3, 3), new ZDateTime(2012, 3, 3), 0.66m);
			using (DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.ApportionmentDirty = false;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ExportDate = new ZDateTime(2012, 3, 4);
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals(new ZDateTime(2012, 3, 4), invoice.EffectiveValuationDate);
				var charge = declaration.TopGroupInvoice.Charges.AddNew();
				charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
				charge.J7_Amount = 100m;
				charge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.NewZealand;
				declaration.ResumeApportionment();
				AssertEquals(new ZDateTime(2012, 3, 3), declaration.US_LatestRateDate);
				AssertEquals("Exchange rate", 0.66m, charge.J7_ExchangeRate);
				SetUpExchangeRates(Core.Constants.CurrencyCodes.NewZealand, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 4), 0.67m);
				AssertEquals("Still have to use 3/3 rate", 66m, charge.MoneyInLocalCurrency.Amount);
			}
		}

		void SetUpExchangeRates(ZString currencyCode, ZDateTime date, ZDateTime endDate, ZDecimal rate)
		{
			Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode).SetUpExchangeRates(date, endDate, rate);
		}
	}
}
