using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class InvoiceLineChargeInformationForTest : IInvoiceLineChargeInformation
	{
		public ZString ChargeCurrency { get; set; }

		public ZDecimal MonetaryDiscountAmount { get; set; }
	}
}
