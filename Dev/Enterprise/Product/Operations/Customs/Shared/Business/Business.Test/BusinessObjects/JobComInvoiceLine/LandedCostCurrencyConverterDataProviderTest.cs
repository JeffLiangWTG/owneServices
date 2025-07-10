using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LandedCostCurrencyConverterDataProviderTest : TestCaseWithFactory
	{
		public void TestCostInLocalCurrencyUsingLCConverter()
		{
			Env.Registry.LandedCostingFallbackExRatesToJobInvoicing = true;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "KRW";

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = declaration.PK;
			BusinessObject exRate = ((IBusinessObjectCollection)job["ExchangeRates"]).AddNew();
			exRate[JobExRateSchema.Constants.JF_RX_NKRateCurrency] = invoice.Invoice_Currency.RX_Code;
			exRate[JobExRateSchema.Constants.JF_BaseRate] = 0.5m;

			invoice.JZ_InvoiceCurrLandedCostExRate = 0.3m;
			AssertEquals(0.3m, invoice.LandedCostingExRateFallBackToJobExRate);
			AssertEquals(3333.33m, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);

			invoice.JZ_InvoiceCurrLandedCostExRate = 0m;
			AssertEquals(0.5m, invoice.LandedCostingExRateFallBackToJobExRate);
			AssertEquals(2000m, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);

			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals(1m, invoice.LandedCostingExRateFallBackToJobExRate);
			AssertEquals(1000m, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);
		}
	}
}
