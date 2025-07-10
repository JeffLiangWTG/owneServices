using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class InvoiceChargeInformationForTest : IInvoiceChargeInformation
	{
		public ZString ChargeDescription { get; set; }

		public ZString ChargeCurrency { get; set; }

		public ZDecimal ChargeAmount { get; set; }

		public ZString MonetaryAmountChargeType { get; set; }

		public ZDecimal MonetaryDiscountAmount { get; set; }

		public ZBool IsOtherCharge { get; set; }

		public ZDecimal ChargeCurrencyConversionRate { get; set; }

		public ZDecimal GetChargeCurrencyConversionRate(ZDateTime dateForRate) => ChargeCurrencyConversionRate;
	}
}
