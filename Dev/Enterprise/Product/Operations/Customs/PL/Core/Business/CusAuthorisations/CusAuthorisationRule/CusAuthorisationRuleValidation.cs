using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
{
	public CusAuthorisationRuleValidation(CusAuthorisationRule parent) : base(parent)
	{
	}

	protected override void CheckCPR_ValueFrom()
	{
		base.CheckCPR_ValueFrom();

		if (Parent.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.Location && !Parent.CPR_ValueFrom.IsLettersAndNumbersOnlyOrEmpty)
		{
			Parent.CPR_ValueFromInfo.AddError(Res.GetString("54322088-5010-4513-8940-D121B9F66A93", "Only alphanumeric up to 17 characters are allowed"));
		}
	}
}
