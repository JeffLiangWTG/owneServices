using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CustomsRuleRuleValidation : CusPermitRuleValidation
	{
		public CustomsRuleRuleValidation(AutoCusPermitRule parent) : base(parent)
		{
		}

		public new CustomsRuleRule Parent => (CustomsRuleRule)base.Parent;

		public CustomsRuleRule Rule => Parent;

		public virtual string NoNeedToUseCPR_ValueFromError => NoNeedToUseCPR_ValueFrom;

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();

			MandatoryValidation.CheckEntered(Rule.CPR_RuleCodeInfo);

			var ruleCode = Rule.CPR_RuleCode;
			if (!ruleCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Rule.CPR_RuleCodeInfo);
				var customsRule = Rule.CustomsRule;
				if (customsRule != null)
				{
					if (ruleCode == CustomsRuleRuleCodeList.Codes.PaymentType
						&& !customsRule.Rules.Any(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement))
					{
						Rule.CPR_RuleCodeInfo.AddError(PaymentTypeShouldPairWithTotalCustomsDisbursement);
					}

					if (ruleCode == CustomsRuleRuleCodeList.Codes.TotalDuty
						&& customsRule.Rules.Any(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TotalDuty && x.PK != Rule.PK))
					{
						Rule.CPR_RuleCodeInfo.AddError(OnlyOneDTYRuleAvailable);
					}
				}
			}
		}

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();

			MandatoryValidation.CheckEntered(Rule.CPR_ValueFromInfo);

			if (Rule.UnUseCPR_ValueFrom && Rule.CPR_ValueFrom != ZDecimal.Zero.ToString())
			{
				Rule.CPR_ValueFromInfo.AddError(NoNeedToUseCPR_ValueFromError);
			}

			if (Rule.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.PaymentType)
			{
				ListValidation.ErrorIfInvalidCode(Rule.CPR_ValueFromInfo);
			}
			else if (Rule.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TariffNumber && !Rule.CPR_ValueFrom.IsEmpty)
			{
				CheckANotEmptyTariffValue(Rule.CPR_ValueFrom, Rule.CPR_ValueFromInfo);
			}
		}

		protected override void CheckCPR_ValueTo()
		{
			base.CheckCPR_ValueTo();
			var rule = Rule;
			var valueTo = rule.CPR_ValueTo;
			if (rule.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TariffNumber && !valueTo.IsEmpty)
			{
				var valueFrom = rule.CPR_ValueFrom;
				CheckANotEmptyTariffValue(valueTo, rule.CPR_ValueToInfo);
				if (valueTo.Length != valueFrom.Length)
				{
					rule.CPR_ValueToInfo.AddError(TariffNumberFromAndToLengthBeSame);
				}
				if (!valueFrom.IsEmpty && valueTo < valueFrom)
				{
					rule.CPR_ValueToInfo.AddError(TariffNumberToShouldGreaterThanFrom);
				}
			}
		}

		void CheckANotEmptyTariffValue(ZString value, ZPropertyInfo propertyInfo)
		{
			if (long.TryParse(value, out var tariffNumber))
			{
				if (tariffNumber < 0)
				{
					propertyInfo.AddError(TariffNumberNegativeNotAllowed);
				}
				if (value.Length < 2 || value.Length > 10 || value.Length % 2 != 0)
				{
					propertyInfo.AddError(TariffNumberLengthLeastError);
				}
			}
			else
			{
				propertyInfo.AddError(TariffNumberLengthLeastError);
			}
		}

		public static string PaymentTypeShouldPairWithTotalCustomsDisbursement => Res.GetString("578F99A5-AEC2-4B35-B954-2E923A4245C3", "Payment Type(PMT) should pair with Total Customs Disbursement(CSD).");

		public static string NoNeedToUseCPR_ValueFrom => Res.GetString("F50771D6-90B2-408A-85D1-7ED03BDFA03D", "Value From is not used for Rule Code CSD and CVL, and it should be set as 0.");

		internal static string TariffNumberLengthLeastError => Res.GetString("5b138dea-ab76-4e32-aa61-e074956f2506", "Please enter an even number (minimum 2 and maximum 10) of digits.");
		internal static string TariffNumberNegativeNotAllowed => Res.GetString("e391f556-6aae-452c-b5a7-cba7565bcf13", "Negative Value not allowed for Rule Code TRF.");
		internal static string TariffNumberFromAndToLengthBeSame => Res.GetString("7eb0e0fd-22e5-4b8c-9fc8-3bdb19b470f8", "If entered, Value To and Value From should be same length.");
		internal static string TariffNumberToShouldGreaterThanFrom => Res.GetString("d0b19bdf-5e32-4e5a-8485-2f8a347697b6", "If entered, Value To should not be less than Value From.");
		internal static string OnlyOneDTYRuleAvailable => Res.GetString("5585ed94-1b75-49e0-81af-c7c8e89dff2a", "There is already an DTY rule exists. Only one DTY rule available for each Customs Rule.");
	}
}
