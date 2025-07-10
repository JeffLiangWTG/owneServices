using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleRuleLookups : CusPermitRuleLookups
	{
		public CustomsRuleRuleLookups(CustomsRuleRule parent) : base(parent)
		{
		}

		public new CustomsRuleRule Parent => (CustomsRuleRule)base.Parent;

		public virtual CodeDescriptionPairList RuleCodes => ruleCodes ?? (ruleCodes = new CustomsRuleRuleCodeList());
		CustomsRuleRuleCodeList ruleCodes;

		public virtual CodeDescriptionPairList ValueFromCodes
		{
			get
			{
				var ruleCode = Parent.CPR_RuleCode;
				return Factory.GetCachedValue(string.Format("CustomsRuleRule|ValueFromCodes|{0}", ruleCode), () =>
				{
					return ruleCode == CustomsRuleRuleCodeList.Codes.PaymentType ? new CustomsRuleRulePaymentTypeValueFromCodeList() : new CodeDescriptionPairList();
				});
			}
		}
	}
}
