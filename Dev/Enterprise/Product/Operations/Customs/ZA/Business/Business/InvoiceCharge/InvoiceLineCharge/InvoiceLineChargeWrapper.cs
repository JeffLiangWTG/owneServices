using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceLineChargeWrapper : IInvoiceLineChargeInformation
	{
		public InvoiceLineChargeWrapper(BaseJobComInvHeaderCharge charge)
		{
			this.charge = charge;
		}

		readonly BaseJobComInvHeaderCharge charge;

		#region IInvoiceLineChargeInformation

		ZString IInvoiceLineChargeInformation.ChargeCurrency => charge.J7_RX_NKCurrency;

		ZDecimal IInvoiceLineChargeInformation.MonetaryDiscountAmount => charge.IsDiscount ? charge.J7_Amount : 0m;

		#endregion
	}
}
