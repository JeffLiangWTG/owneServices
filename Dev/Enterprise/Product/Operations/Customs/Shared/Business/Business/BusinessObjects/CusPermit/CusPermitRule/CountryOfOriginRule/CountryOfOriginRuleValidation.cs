using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CountryOfOriginRuleValidation : BaseCusPermitRuleValidation
	{
		public CountryOfOriginRuleValidation(BaseCusPermitRule parent) : base(parent)
		{
		}

		protected new BaseCusPermitRule Parent => base.Parent;

		protected override void CheckCPR_ValueFrom()
		{
			base.CheckCPR_ValueFrom();
			ListValidation.ErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
		}
	}
}
