using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceChargeWrapper : IInvoiceChargeInformation
	{
		public InvoiceChargeWrapper(BaseJobComInvHeaderCharge charge)
		{
			this.charge = charge;
		}

		readonly BaseJobComInvHeaderCharge charge;

		#region IInvoiceChargeInformation

		ZString IInvoiceChargeInformation.ChargeDescription => charge.J7_ChargeDescription;

		ZString IInvoiceChargeInformation.ChargeCurrency => charge.J7_RX_NKCurrency;

		ZDecimal IInvoiceChargeInformation.ChargeAmount => charge.J7_Amount;

		ZDecimal IInvoiceChargeInformation.GetChargeCurrencyConversionRate(ZDateTime dateForRate) => charge.Currency?.GetCustomsRate(dateForRate) ?? 0m;

		ZString IInvoiceChargeInformation.MonetaryAmountChargeType =>
			ZZRefCusMapCombined.MapCW1CodeToCustomsCode(charge.Factory, Core.Constants.CountryCodes.SouthAfrica, RefCusMapTypeList.Codes.ChargeCode, charge.J7_ChargeType, ZDateTime.Today);

		ZDecimal IInvoiceChargeInformation.MonetaryDiscountAmount => charge.IsDiscount ? charge.J7_Amount : 0m;

		ZBool IInvoiceChargeInformation.IsOtherCharge => charge.J7_ChargeType == InvoiceLineCustomsChargeTypeList.Codes.OtherCharges;

		#endregion
	}
}
