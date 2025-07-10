using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationRuleValidation : CusPermitRuleValidation
	{
		public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
		{
		}

		public new CusAuthorisationRule Parent => (CusAuthorisationRule)base.Parent;

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();
			var cusAuthorizationRule = Parent;
			MandatoryValidation.CheckEntered(cusAuthorizationRule.CPR_RuleCodeInfo);
			ListValidation.ErrorIfInvalidCode(cusAuthorizationRule.CPR_RuleCodeInfo);

			var ruleCode = cusAuthorizationRule.CPR_RuleCode;

			var authorisationType = authorisationHeader.CPH_Type;
			var ruleRequirement = authorisationHeader.Provider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationType).SingleOrDefault(x => x.RuleType == ruleCode);
			if (ruleRequirement != null)
			{
				CheckAttributeTypeMaxRepetition(authorisationType, ruleRequirement.RuleType, ruleRequirement.MaxAllowed);
			}
			if (authorisationHeader.Provider.GetValidLinkedAuthorizationRuleRepetitions(cusAuthorizationRule.Factory).TryGetValue(ruleCode, out var linkedAuthorizationRuleRange))
			{
				foreach (var range in linkedAuthorizationRuleRange)
				{
					CheckLinkedAuthorizationRuleRepetition(range);
				}
			}
		}

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			var targetPropertyInfo = Parent.CPR_ValueFromInfo;
			if (Parent.CPR_ValueFrom.IsEmpty)
			{
				targetPropertyInfo.AddError(MandatoryValidation.MustBeEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(targetPropertyInfo)));
			}
			else
			{
				if (Parent.CPR_ValueFromIsCodeField)
				{
					CheckCPR_ValueFromIsValidCode();
				}
			}

			var authorisationType = authorisationHeader.CPH_Type;
			var ruleRequirement = authorisationHeader.Provider.GetValidAuthorisationRuleRequirements(authorisationHeader, authorisationType).SingleOrDefault(x => x.RuleType == Parent.CPR_RuleCode);
			if (ruleRequirement != null && ruleRequirement.AdditionalValidatorOnValueCollection != null)
			{
				foreach (var additionalValidatorOnValue in ruleRequirement.AdditionalValidatorOnValueCollection)
				{
					var validationResult = additionalValidatorOnValue.Invoke(Parent.CPR_ValueFrom);
					var validationMessage = validationResult.ValidationMessage;
					if (!string.IsNullOrEmpty(validationMessage))
					{
						targetPropertyInfo.AddNotification(validationResult.NotificationType, validationMessage);
					}
				}
			}
		}

		protected virtual void CheckCPR_ValueFromIsValidCode()
		{
			var targetPropertyInfo = Parent.CPR_ValueFromInfo;
			ListValidation.ErrorIfInvalidCode(targetPropertyInfo);
		}

		void CheckAttributeTypeMaxRepetition(ZString authorisationType, ZString ruleType, ZInt maxCount)
		{
			if (!maxCount.IsEmpty && authorisationHeader.CusAuthorisationRules.OfType<CusAuthorisationRule>().Count(x => x.CPR_RuleCode == ruleType) > maxCount)
			{
				Parent.CPR_RuleCodeInfo.AddError(Res.GetString("3B16ACE8-90C4-408F-80B5-395B987EA4C4",
					"You are allowed to have a maximum of {0} authorization rules of type '{1}' for authorization type '{2}'.", maxCount, ruleType, authorisationType));
			}
		}

		void CheckLinkedAuthorizationRuleRepetition(LinkedCusAuthorisationRuleRange range)
		{
			var linkedRuleCode = range.RuleType;
			var minRequired = range.MinRequired;
			var maxAllowed = range.MaxAllowed;
			var parent = Parent;
			var ruleCode = parent.CPR_RuleCode;
			var linkedRulesCount = parent.LinkedCusAuthorisationRules.Count(x => x.CPR_RuleCode == linkedRuleCode);
			if (linkedRulesCount < minRequired)
			{
				if (minRequired == maxAllowed)
				{
					parent.CPR_RuleCodeInfo.AddError(Res.GetString("DEE0D036-D93E-4F95-9D6F-B43C26CF0BB3",
						"You are required to have {0} linked rule(s) of type '{1}' for authorization rule type '{2}'.", minRequired, linkedRuleCode, ruleCode));
				}
				else
				{
					parent.CPR_RuleCodeInfo.AddError(Res.GetString("C9422146-8619-41CE-8E4D-DF8CFE448742",
						"You are required to have a minimum of {0} linked rule(s) of type '{1}' for authorization rule type '{2}'.", minRequired, linkedRuleCode, ruleCode));
				}
			}
			else if (maxAllowed > 0 && linkedRulesCount > maxAllowed)
			{
				parent.CPR_RuleCodeInfo.AddError(Res.GetString("B3691FAC-DF3C-4333-B57B-1D5638CD06A3",
					"You are allowed to have a maximum of {0} linked rule(s) of type '{1}' for authorization rule type '{2}'.", maxAllowed, linkedRuleCode, ruleCode));
			}
		}

		CusAuthorisationHeader authorisationHeader => Parent.AuthorisationHeader;
	}
}
