namespace Enterprise.Customs.Business
{
	public class AccessCodePinRuleValidation : SharedCusPermitRuleValidation
	{
		public AccessCodePinRuleValidation(CusGuaranteeRule parent) : base(parent)
		{
		}

		public new CusGuaranteeRule Parent => (CusGuaranteeRule)base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			if (!Parent.IsDefaultAccessCodeDefaultPin)
			{
				base.CheckCPR_ValueFrom();
			}
		}
	}
}
