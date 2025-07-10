using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class LinkedCusAuthorisationRuleValidation : CusPermitRuleValidation
	{
		public LinkedCusAuthorisationRuleValidation(LinkedCusAuthorisationRule parent) : base(parent)
		{
		}

		public new LinkedCusAuthorisationRule Parent => (LinkedCusAuthorisationRule)base.Parent;

		protected override void CheckCPR_RuleCode()
		{
			base.CheckCPR_RuleCode();
			MandatoryValidation.CheckEntered(Parent.CPR_RuleCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CPR_RuleCodeInfo);
		}

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			MandatoryValidation.CheckEntered(Parent.CPR_ValueFromInfo);
			if (Parent.CPR_ValueFromIsCodeField)
			{
				CheckCPR_ValueFromIsValidCode();
			}
		}

		protected virtual void CheckCPR_ValueFromIsValidCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
		}
	}
}
