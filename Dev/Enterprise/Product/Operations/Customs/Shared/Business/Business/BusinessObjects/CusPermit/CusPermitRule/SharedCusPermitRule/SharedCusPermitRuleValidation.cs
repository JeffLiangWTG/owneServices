using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitRuleValidation : CusPermitRuleValidation
	{
		public SharedCusPermitRuleValidation(SharedCusPermitRule parent) : base(parent)
		{
		}

		protected new SharedCusPermitRule Parent => (SharedCusPermitRule)base.Parent;

		protected override void CheckCPR_RuleCode()
		{
			var targetInfo = Parent.CPR_RuleCodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
		}

		protected override void CheckCPR_ValueFrom()
		{
			MandatoryValidation.CheckEntered(Parent.CPR_ValueFromInfo);
		}
	}
}
