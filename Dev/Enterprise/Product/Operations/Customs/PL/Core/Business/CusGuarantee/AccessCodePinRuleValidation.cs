namespace Enterprise.Customs.PL.Business;

public class AccessCodePinRuleValidation : EU.Business.AccessCodePinRuleValidation
{
	public AccessCodePinRuleValidation(CusGuaranteeRule parent) : base(parent)
	{
	}

	protected override void CheckCPR_ValueFrom()
	{
		base.CheckCPR_ValueFrom();

		var valueFrom = Parent.CPR_ValueFrom;
		if (!valueFrom.IsEmpty)
		{
			if (valueFrom.ContainsAnyLetters)
			{
				Parent.CPR_ValueFromInfo.AddMessageError(Res.GetString("PLAccessCodePinRuleValidation|CPR_ValueFromOnlyDigitsMessage", "Additional Access Code should contain digits only"));
			}

			if (valueFrom.Length < 4)
			{
				Parent.CPR_ValueFromInfo.AddMessageError(Res.GetString("PLAccessCodePinRuleValidation|CPR_ValueFromFourCharactersMessage", "Additional Access Code should contain 4 characters"));
			}
		}
	}
}
