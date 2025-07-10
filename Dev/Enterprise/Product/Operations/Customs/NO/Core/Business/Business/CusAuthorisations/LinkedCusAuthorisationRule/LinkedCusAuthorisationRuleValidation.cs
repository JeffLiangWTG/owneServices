using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class LinkedCusAuthorisationRuleValidation : Customs.Business.LinkedCusAuthorisationRuleValidation
	{
		public LinkedCusAuthorisationRuleValidation(LinkedCusAuthorisationRule parent) : base(parent)
		{
		}

		protected override void CheckCPR_Description()
		{
			base.CheckCPR_Description();
			if (Parent.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices)
			{
				MandatoryValidation.CheckEntered(Parent.CPR_DescriptionInfo);
			}
		}

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			if (Parent.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices)
			{
				var matchingRulesFound = Parent.AuthorisationRule?.LinkedCusAuthorisationRules.Count(x => !x.IsDeleted
					&& x.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices && x.CPR_ValueFrom == Parent.CPR_ValueFrom);
				if (matchingRulesFound > 1)
				{
					Parent.CPR_ValueFromInfo.AddError(PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(Parent.CPR_ValueFromInfo.HumanReadableName));
				}
			}
		}
	}
}
