using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class FTZZoneStatusPermitRuleValidation : BaseCusPermitRuleValidation
	{
		public FTZZoneStatusPermitRuleValidation(BaseCusPermitRule parent) : base(parent)
		{
		}

		protected new CusPermitRule Parent => (CusPermitRule)base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			ListValidation.ErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
		}
	}
}
