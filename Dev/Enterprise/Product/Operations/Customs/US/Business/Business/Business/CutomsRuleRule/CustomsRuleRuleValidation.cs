using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CustomsRuleRuleValidation : Customs.Business.CustomsRuleRuleValidation
	{
		internal const string OnlyOneSTBRuleAvailable = "There is already an STB rule exists. Only one STB rule available for each Customs Rule.";

		public override string NoNeedToUseCPR_ValueFromError => "Value From is not used for Rule Code CSD, CVL, STB and DTY, and it should be set as 0.";

		public CustomsRuleRuleValidation(AutoCusPermitRule parent) : base(parent)
		{
		}

		public new CustomsRuleRule Rule => (CustomsRuleRule)base.Rule;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			var rule = Rule;
			if (rule.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.ADCEligible)
			{
				ListValidation.ErrorIfInvalidCode(rule.CPR_ValueFromInfo);
			}
		}

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();
			var rule = Rule;
			if (rule.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount
				&& rule.CustomsRule != null
				&& rule.CustomsRule.Rules.Any(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.SingleTransactionBondAmount && x.PK != rule.PK))
			{
				rule.CPR_RuleCodeInfo.AddError(OnlyOneSTBRuleAvailable);
			}
		}
	}
}
