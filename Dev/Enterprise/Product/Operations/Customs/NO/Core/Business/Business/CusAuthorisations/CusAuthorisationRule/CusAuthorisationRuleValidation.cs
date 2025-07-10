using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
	{
		public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
		{
		}

		protected override void CheckCPR_Description()
		{
			base.CheckCPR_Description();
			if (Parent.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice)
			{
				MandatoryValidation.CheckEntered(Parent.CPR_DescriptionInfo);
			}
		}
	}
}
