using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD.Testing
{
	sealed class BRDZITest : TestCaseWithFactory
	{
		public void TestUpdateInvoiceDetails()
		{
			using (Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetReciprocalFlagForCurrentCompany(true))
			{
				RefCurrency aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				RefCurrency nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");

				SetUpExchangeRate(aud, 0.89547m);
				SetUpExchangeRate(nzd, 0.61422m);

				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;//USD

				JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_LinePrice = 3000m;
				JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 7000m;

				JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 20000m;
				invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;//USD

				JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_LinePrice = 14000m;
				JobComInvoiceLine invoiceLine4 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine4.JI_LinePrice = 6000m;

				BRDZI zi = new BRDZI();
				zi.InvoiceSequence = 1;
				zi.InvoiceValue = 11167.32m;
				zi.CurrencyCode = "AUD";
				zi.ExchangeRates = 0.89547m;//consistent with the exchange rate

				((IBIRDHeaderRecord)zi).Update(declaration, new NotificationCollection());
				AssertEquals("Invoice1's currency changed to AUD", "AUD", invoice.Invoice_Currency.RX_Code);
				AssertEquals("Invoice Amount changed", 11167.32m, invoice.JZ_InvoiceAmount);
				AssertEquals("Line Price changed", 3350.20m, invoiceLine1.JI_LinePrice);
				AssertEquals("Line Price changed", 7817.12m, invoiceLine2.JI_LinePrice);

				AssertEquals("Invoice2 stays same", 20000m, invoice2.JZ_InvoiceAmount);
				AssertEquals("Invoice2 stays same", 14000m, invoiceLine3.JI_LinePrice);
				AssertEquals("Invoice2 stays same", 6000m, invoiceLine4.JI_LinePrice);

				zi.InvoiceSequence = 2;
				zi.InvoiceValue = 31964.20m;
				zi.CurrencyCode = "NZD";
				zi.ExchangeRates = 0.6257m;//inconsistent with the exchange rate

				((IBIRDHeaderRecord)zi).Update(declaration, new NotificationCollection());
				AssertEquals("Invoice1's currency changed to NZD", "NZD", invoice2.Invoice_Currency.RX_Code);
				AssertEquals("Invoice Amount changed", 31964.20m, invoice2.JZ_InvoiceAmount);
				AssertEquals("Line Price changed", 22374.94m, invoiceLine3.JI_LinePrice);
				AssertEquals("Line Price changed", 9589.26m, invoiceLine4.JI_LinePrice);
			}
		}

		void SetUpExchangeRate(RefCurrency currency, ZDecimal exchangeRate)
		{
			var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, currency.RX_Code, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK) ?? currency.ExchangeRates.AddNew();

			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Today;
			rate.RE_ExpiryDate = ZDateTime.Today;
			rate.RE_SellRate = exchangeRate;
		}
	}
}
