using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OverriddenInvoiceTypeTestCombination
	{
		public string TestRuleInfoString { private get; set; }
		public RefUNLOCO Origin { private get; set; }
		public RefUNLOCO Destination { private get; set; }
		public InputForPaymentTerm InputForPaymentTerm { private get; set; }
		public RefCurrency Currency { private get; set; }
		public string DebtorPostingStyle { private get; set; }
		public string ExpectedInvoiceTypeDefault { private get; set; }

		public override string ToString()
		{
			return string.Format("Origin: '{0}', Destination: '{1}', Payment Terms: '{2}', Currency: '{3}', Posting Style: '{4}', Expecting: '{5}'",
				Origin.Code, Destination.Code, InputForPaymentTerm, Currency.RX_Code, DebtorPostingStyle, ExpectedInvoiceTypeDefault);
		}

		public void PerformAssert(IJobInvoicingPlugIn job, AgencyShipmentMockJobInvoicingSupporter invoicingSupporter, JobInvoicingConsumerType consumerType)
		{
			invoicingSupporter.Origin = Origin;
			invoicingSupporter.Destination = Destination;

			if (!string.IsNullOrWhiteSpace(InputForPaymentTerm.IncoTerm))
			{
				var paymentTerms = new PaymentTermInfos();
				paymentTerms.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, InputForPaymentTerm.IncoTerm));
				invoicingSupporter.PaymentTerm = paymentTerms;
			}
			else
			{
				invoicingSupporter.PaymentTerm = null;
			}

			var message = string.Format("Testing rule = {0}. Specific case within rule = {1}", TestRuleInfoString, ToString());

			var result = consumerType.GetOverriddenInvoiceType(
				job,
				InputForPaymentTerm.ChargeCode,
				Currency,
				DebtorPostingStyle,
				InputForPaymentTerm.CurrentInvoiceType);

			Assertion.AssertEquals(message, ExpectedInvoiceTypeDefault, result);
		}
	}
}
