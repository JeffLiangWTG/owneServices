using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CurrencyConverterWithFixedExchangeRatesDataProvider))]
sealed class CurrencyConverterWithFixedExchangeRatesDataProviderTest : TestCaseWithFactory
{
	public void TestGetExchangeRate()
	{
		var header = Factory.New<JobComInvoiceHeader>();
		header.JZ_InvoiceCurrExRate = 16m;

		var refCurrencyCurrencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, header);
		AssertEquals("Exchange rate should be equal to the JobComInvoiceHeader.JZ_InvoiceCurrExRate", 16m, refCurrencyCurrencyConverter.GetExchangeRate(null));
	}
}
