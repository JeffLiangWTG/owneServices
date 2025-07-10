using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business
{
	public class CustomsRuleRuleLookups : Customs.Business.CustomsRuleRuleLookups
	{
		public CustomsRuleRuleLookups(CustomsRuleRule parent) : base(parent)
		{
		}

		public new CustomsRuleRule Parent => (CustomsRuleRule)base.Parent;

		public override CodeDescriptionPairList RuleCodes => ruleCodes ?? (ruleCodes = new CustomsRuleRuleCodeList());
		CustomsRuleRuleCodeList ruleCodes;

		public override CodeDescriptionPairList ValueFromCodes
		{
			get
			{
				var ruleCode = Parent.CPR_RuleCode;
				return Factory.GetCachedValue(string.Format("CustomsRuleRule|ValueFromCodes|{0}", ruleCode), () =>
				{
					switch (ruleCode)
					{
						case CustomsRuleRuleCodeList.Codes.PaymentType:
							return new Customs.Business.CustomsRuleRulePaymentTypeValueFromCodeList();

						case CustomsRuleRuleCodeList.Codes.ADCEligible:
							return new CustomsRuleRuleADCEligibleCodeList();
						default:
							return new CodeDescriptionPairList();
					}
				});
			}
		}
	}
}
