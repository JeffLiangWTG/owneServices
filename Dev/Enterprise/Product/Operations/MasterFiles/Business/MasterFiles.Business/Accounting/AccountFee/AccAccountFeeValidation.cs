//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccAccountFeeValidation
//
//    This class should be used for overriding validation in AutoAccAccountFeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccAccountFeeValidation : AutoAccAccountFeeValidation
	{
		public AccAccountFeeValidation(AutoAccAccountFee parent)
			: base(parent)
		{
		}
		protected override void CheckAAF_RX_NKFeeCurrency()
		{
			base.CheckAAF_RX_NKFeeCurrency();
			AccAccountFeeValidationHelper.ValidateAAF_RX_NKFeeCurrency(Parent.AAF_RX_NKFeeCurrencyInfo, Parent.Lookups.FeeCurrencies);
		}

		protected override void CheckAAF_Rule()
		{
			base.CheckAAF_Rule();
			AccAccountFeeValidationHelper.ValidateAAF_Rule(Parent.AAF_RuleInfo, Parent.Lookups.AccountFeeCalculationRuleList);
		}

		protected override void CheckAAF_FeeAmount()
		{
			AccAccountFeeValidationHelper.ValidateAAF_FeeAmount(Parent.AAF_FeeAmountInfo, Parent.AAF_FeeAmount, Parent.AAF_Rule);
		}

		protected override void CheckAAF_AG_GLAccount()
		{
			base.CheckAAF_AG_GLAccount();
			AccAccountFeeValidationHelper.ValidateAAF_AG_GLAccount(Parent.AAF_AG_GLAccountInfo);
		}
	}
}
