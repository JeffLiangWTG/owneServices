using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostingExRate))]
	sealed class LandedCostingExRateTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestNoExceptionWhenExRateHolderNotPassed()
		{
			var exRate = new LandedCostingExRate(Factory);
			_ = exRate.ReferenceNumber;
			_ = exRate.CurrencyCode;
			_ = exRate.ExchangeRate;
		}

		public void TestBindableProperties()
		{
			ExRateHolder.ReferenceNumberExposed = "AAA123456";
			ExRateHolder.CurrencyCodeExposed = "USD";
			ExRateHolder.LandedCostExchangeRateExposed = 0.5m;

			AssertEquals("Reference Number", ExRateHolder.ReferenceNumber, ExRate.ReferenceNumber);
			AssertEquals("CurrencyCode", ExRateHolder.CurrencyCode, ExRate.CurrencyCode);
			AssertEquals("LandedCostExchangeRateExposed", ExRateHolder.LandedCostExchangeRate, ExRate.ExchangeRate);
		}

		public void TestExchangeRate()
		{
			ExRateHolder.CurrencyCodeExposed = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("ExRate's exchange rate should be 1 for local currency", 1m, ExRate.ExchangeRate);
		}

		public void TestValidateExchangeRate()
		{
			ExRateHolder.CurrencyCodeExposed = "AUD";

			ExRate.ExchangeRate = 0m;
			AssertEquals("Zero is not acceptable", true, ExRate.ExchangeRateInfo.HasError(LandedCostingExRate.ExchangeRateShouldBeGreaterThanZero));

			ExRate.ExchangeRate = -1m;
			AssertEquals("Negative is not acceptable", true, ExRate.ExchangeRateInfo.HasError(LandedCostingExRate.ExchangeRateShouldBeGreaterThanZero));

			ExRate.ExchangeRate = 1m;
			AssertEquals("Positive is acceptable", false, ExRate.ExchangeRateInfo.HasError(LandedCostingExRate.ExchangeRateShouldBeGreaterThanZero));

			ExRateHolder.CurrencyCodeExposed = "";
			ExRate.ExchangeRate = 0m;
			AssertEquals("Zero is acceptable for an empty currency", false, ExRate.ExchangeRateInfo.HasError(LandedCostingExRate.ExchangeRateShouldBeGreaterThanZero));

			ExRate.ExchangeRate = -1m;
			AssertEquals("Negative is acceptable for an empty Currency", false, ExRate.ExchangeRateInfo.HasError(LandedCostingExRate.ExchangeRateShouldBeGreaterThanZero));
		}

		public void TestReadOnlyOfExchangeRate()
		{
			ExRateHolder.CurrencyCodeExposed = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("ExRate's exchange rate should be readonly", true, ExRate.ExchangeRateInfo.ReadOnly);

			ExRateHolder.CurrencyCodeExposed = "~~D";
			AssertEquals("ExRate's exchange rate should not be readonly", false, ExRate.ExchangeRateInfo.ReadOnly);
		}

		public void TestExchangeRateGetsDefaultedFromDefaultFieldOnConstruction()
		{
			ExRateHolder.LandedCostExchangeRateExposed = 0;
			ExRateHolder.LandedCostExchangeRateDefaultExposed = 10m;

			LandedCostingExRate exRate = new LandedCostingExRate(ExRateHolder);
			AssertEquals("ExRate's exchange rate is defaulted", 10m, exRate.ExchangeRate);
		}

		public void TestSettingExRateShouldChangeHasChanges()
		{
			BusinessObject testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();

			BusinessObject invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;

			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = "USD";
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate] = 5m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate] = 0m;

			LandedCostingExRate exRate = new LandedCostingExRate((ILandedCostExchangeRateHolder)invoice);

			AssertEquals("ExRate's HasChanges", false, exRate.HasChanges);
			AssertEquals("ExRateHolder's HasChanges", true, invoice.HasChanges);

			exRate.ExchangeRate = 10m;
			AssertEquals("ExRate's HasChanges set to true", true, exRate.HasChanges);
			AssertEquals("ExRateHolder's HasChanges", true, invoice.HasChanges);

			exRate.ExchangeRate = 10m;
			AssertEquals("ExRate's HasChanges set to true", true, exRate.HasChanges);
			AssertEquals("ExRateHolder's HasChanges", true, invoice.HasChanges);
		}

		public void TestExchangeRateSavedIntoHolder()
		{
			BusinessObject testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();

			BusinessObject invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = testDec.PK;

			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = "USD";
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrExRate] = 0m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate] = 0m;

			LandedCostingExRate exRate = new LandedCostingExRate((ILandedCostExchangeRateHolder)invoice);
			AssertEquals("Ex Rate is initially 0m", 0m, exRate.ExchangeRate);

			exRate.ExchangeRate = 0.789m;
			AssertEquals("Ex Rate is 0.789m", 0.789m, exRate.ExchangeRate);
			AssertEquals("Invoice Loaded should have the exchangerate", 0.789m, invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate]);

			Factory.Save();

			BusinessObject invoiceLoaded = (BusinessObject)Factory.Load<Integration.Customs.Shared.IBaseJobComInvoiceHeader>(invoice.PK);
			AssertEquals("Invoice Loaded should have the exchangerate", 0.789m, invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceCurrLandedCostExRate]);
		}

		protected override BusinessObject GetNewBusinessObject() => ExRate;

		LandedCostingExRate exRate;
		LandedCostingExRate ExRate => exRate ?? (exRate = new LandedCostingExRate(ExRateHolder));

		DummyExchangeRateHolder exRateHolder;
		DummyExchangeRateHolder ExRateHolder => exRateHolder ?? (exRateHolder = Factory.New<DummyExchangeRateHolder>());
	}
}
