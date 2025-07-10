using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class InvoiceHeaderInformationForTest : IInvoiceHeaderInformation
	{
		public ZString InvoiceNumber { get; set; }

		public ZDateTime InvoiceDate { get; set; }

		public ZString NameOfIssuer { get; set; }

		public ZString CountryOfIssuer { get; set; }

		public ZString Address1 { get; set; }

		public ZString Address2 { get; set; }

		public ZString Address3 { get; set; }

		public ZString Address4 { get; set; }

		public ZDecimal TotalInvoiceAmount { get; set; }

		public ZString InvoiceCurrencyCoded { get; set; }

		public ZDecimal ExchangeRate { get; set; }

		public ZDecimal TotalChargesInLocalCurrency { get; set; }

		public ZDecimal CommonFactor { get; set; }

		public ZString TermsOfDelivery { get; set; }

		public ZDecimal AdvancePaymentAmount { get; set; }

		public ZString AdvancePaymentCurrencyCode { get; set; }

		public ZString[] AdvancePaymentNotificationDetails { get; set; }

		public IEnumerable<IInvoiceLineInformation> InvoiceLineInformations { get; set; }

		public IEnumerable<IInvoiceChargeInformation> InvoiceChargeInformations { get; set; }

		public ZString PaymentTerms { get; set; }
	}
}
