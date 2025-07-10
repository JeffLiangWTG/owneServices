using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CustomsValuationCalculator : ICustomsValuationCalculator
	{
		public CustomsValuationCalculator(IChargeApportionee chargeApportionee)
		{
			this.ChargeApportionee = chargeApportionee;
		}

		public virtual ZDecimal GetAmountToAddToITOTForDutiable(RefCurrency currency)
		{
			return ChargeApportionee.Charges.AmountToAddToITOTForDutiableCharges(currency) +
				ChargeApportionee.ApportionedCharges.AmountToAddToITOTForDutiableCharges(currency);
		}

		public virtual ZDecimal GetAmountToAddToITOTForStatistical(RefCurrency currency)
		{
			return ChargeApportionee.Charges.AmountToAddToITOTForStatisticalCharges(currency) +
				ChargeApportionee.ApportionedCharges.AmountToAddToITOTForStatisticalCharges(currency);
		}

		public virtual ZDecimal GetAmountToAddToITOTForVatableGstable(RefCurrency currency)
		{
			return ChargeApportionee.Charges.AmountToAddToITOTForVatableGstableCharges(currency) +
				ChargeApportionee.ApportionedCharges.AmountToAddToITOTForVatableGstableCharges(currency);
		}

		public ZDecimal GetOverseasFreight(RefCurrency currency)
		{
			return ChargeApportionee.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, currency)
				+ ChargeApportionee.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, currency);
		}

		public ZDecimal GetOverseasInsurance(RefCurrency currency)
		{
			return ChargeApportionee.Charges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, currency)
				+ ChargeApportionee.ApportionedCharges.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, currency);
		}

		#region Implementation

		readonly protected IChargeApportionee ChargeApportionee;

		#endregion
	}
}
