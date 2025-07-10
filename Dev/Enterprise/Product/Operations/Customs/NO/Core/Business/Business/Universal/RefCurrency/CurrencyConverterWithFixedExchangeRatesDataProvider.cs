using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business;

public class CurrencyConverterWithFixedExchangeRatesDataProvider(BusinessObjectFactory factory, ICurrencyConverterDataProviderWithFixedExRates dataProvider)
	: MasterFiles.Business.CurrencyConverterWithFixedExchangeRatesDataProvider(factory, dataProvider)
{
	protected override ZDecimal GetExchangeRateCore(ICurrency currency, out ZDateTime foundRateDate)
	{
		foundRateDate = ZDateTime.Empty;
		return invoiceHeader.JZ_InvoiceCurrExRate;
	}

	readonly JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)dataProvider;
}
