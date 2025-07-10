using System.Linq;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceChargeValidation : EU.Business.Declaration.InvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();

			if ((Parent.IsLocalCharge() || Parent.J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges) && Parent.J7_RX_NKCurrency != Core.Constants.CurrencyCodes.Turkey)
			{
				Parent.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString(
					"9E205893-C51D-40DF-9B1D-4FDF4F2FF362",
					"Currency of Local charges must be {0}.",
					Core.Constants.CurrencyCodes.Turkey
				));
			}
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();

			var chargeType = Parent.J7_ChargeType;

			if (Parent.IsTotalCharge())
			{
				var invoiceLineCharges = Parent.Invoice.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(invoiceLine => invoiceLine.ApportionedCharges.Cast<JobComInvCharge>().Union(invoiceLine.Charges));
				var invoiceLineChargesForSum = invoiceLineCharges.Where(charge => charge.J7_ChargeType == chargeType);

				var invoiceLineTotal = invoiceLineChargesForSum.Sum(charge => charge.J7_Amount);
				if (invoiceLineTotal != Parent.J7_Amount)
				{
					Parent.J7_AmountInfo.AddMessageError(Res.GetString(
						"16CB63C9-F5CE-4A0A-ADBC-E48ED143468E",
						@"Sum of {0} charges in the Invoice Lines should be equal to the amount of {0} charge in the Invoice Header.
Amount of Invoice Lines: {1}, Invoice Header input: {2}", chargeType, invoiceLineTotal, Parent.J7_Amount
					));
				}
			}
		}
	}
}
