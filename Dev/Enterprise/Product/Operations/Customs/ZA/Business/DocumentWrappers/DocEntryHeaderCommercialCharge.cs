using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocEntryHeaderCommercialCharge : DocBaseWrapper
	{
		DocEntryHeaderCommercialCharge(BaseInvoiceLineApportionedCharge apportionedCharge, BusinessObjectFactory factoryToWrap)
			: base(apportionedCharge, factoryToWrap)
		{
			this.apportionedCharge = apportionedCharge;
			this.chargeAmount = apportionedCharge.J7_Amount;
		}

		public static DocEntryHeaderCommercialCharge New(BaseInvoiceLineApportionedCharge apportionedCharge, BusinessObjectFactory factoryToWrap)
		{
			return new DocEntryHeaderCommercialCharge(apportionedCharge, factoryToWrap);
		}

		public void AddAdditionalAmountInChargeCurrency(ZDecimal additionalAmount)
		{
			chargeAmount += additionalAmount;
		}

		#region Wrapper Fields

		public ZString InvoiceNumber => ApportionedCharge.InvoiceLine.InvoiceNumber;
		public ZBool IsFreight => ApportionedCharge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasFreight;
		public ZBool IsInsurance => ApportionedCharge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance;
		public ZDecimal ChargeAmount => chargeAmount;
		public ZString CurrencyCode => ApportionedCharge.J7_RX_NKCurrency;
		public ZDecimal ExchangeRate => ApportionedCharge.J7_ExchangeRate;
		public ZBool IsDutiable => ApportionedCharge.J7_IsDutiable;
		public ZBool IsIncludedInLines => !ApportionedCharge.J7_IsNotIncludedInInvoice;

		#endregion

		BaseInvoiceLineApportionedCharge ApportionedCharge => apportionedCharge;
		readonly BaseInvoiceLineApportionedCharge apportionedCharge;

		ZDecimal chargeAmount;
	}
}
