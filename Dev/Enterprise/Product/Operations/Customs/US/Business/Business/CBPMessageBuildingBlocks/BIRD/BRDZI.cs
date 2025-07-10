using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD
{
	partial class BRDZI : Messaging.Business.MessageBuildingBlocks.BIRD.Abstract.BRDZI, IBIRDHeaderRecord
	{
		#region IBIRDHeaderRecord Members

		void IBIRDHeaderRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			JobComInvoiceHeader invoice = (JobComInvoiceHeader)declaration.Invoices.Find(InvoiceSequence);

			if (invoice != null)
			{
				RefCurrency invoiceCurrency = declaration.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);

				if (invoiceCurrency != null && invoiceCurrency.RX_Code != invoice.JZ_RX_NKInvoice_Currency)
				{
					RefCurrency oldCurrency = invoice.Invoice_Currency;

					invoice.JZ_InvoiceAmount = InvoiceValue;
					invoice.JZ_RX_NKInvoice_Currency = invoiceCurrency.RX_Code;

					if (invoice.JZ_InvoiceCurrExRate != ExchangeRates)
					{
						invoice.JZ_InvoiceCurrExRateType = JobComInvoiceHeader.FixedExchangeRateTypeString;
						invoice.JZ_InvoiceCurrExRate = ExchangeRates;
					}

					if (oldCurrency != null)
					{
						CurrencyConverter converter = invoice.CurrencyConverter;

						foreach (JobComInvoiceLine invoiceLine in invoice.JobComInvoiceLines)
						{
							invoiceLine.JI_LinePrice = converter.ConvertExact(new Money(invoiceLine.JI_LinePrice, oldCurrency), invoiceCurrency).Amount;
						}
					}
				}
			}
		}

		#endregion
	}
}
