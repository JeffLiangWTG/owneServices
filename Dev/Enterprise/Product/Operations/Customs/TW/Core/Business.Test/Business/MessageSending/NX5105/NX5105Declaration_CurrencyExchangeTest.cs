using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Declaration_CurrencyExchangeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCurrencyExchange()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_InvoiceCurrExRate = 1.2m;
			var invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_RX_NKCurrency = "USD";
			invoiceCharge.J7_ExchangeRate = 1.2m;
			var invoiceLine1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine1Charge = invoiceLine1.Charges.AddNew();
			invoiceLine1Charge.J7_RX_NKCurrency = "USD";
			invoiceLine1Charge.J7_ExchangeRate = 1.2m;
			var invoiceLine2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var invoiceLine2Charge = invoiceLine2.Charges.AddNew();
			invoiceLine2Charge.J7_RX_NKCurrency = "USD";
			invoiceLine2Charge.J7_ExchangeRate = 1.2m;
			ICurrencyExchange currencyExchange = new NX5105Declaration_CurrencyExchange(entryHeader);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("USD").Using(CustomComparers.TypeComparison), "CurrencyExchange.CurrencyTypeCode should be");
				NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(1.2m).Using(CustomComparers.TypeComparison), "CurrencyExchange.RateNumeric should be");
			}

			);
			currencyExchange = new NX5105Declaration_CurrencyExchange(entryHeader);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceCurrExRate = 1.4m;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(currencyExchange.CurrencyTypeCode, NUnit.Framework.Is.EqualTo("EUR").Using(CustomComparers.TypeComparison), "CurrencyExchange.CurrencyTypeCode should be");
				NUnit.Framework.Assert.That(currencyExchange.RateNumeric, NUnit.Framework.Is.EqualTo(1.4m).Using(CustomComparers.TypeComparison), "CurrencyExchange.RateNumeric should be");
			}

			);
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105Declaration_CurrencyExchange(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new NX5105Declaration_CurrencyExchange(Factory.New<CusEntryHeader>());
			}

			);
		}
	}
}
