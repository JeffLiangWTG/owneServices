namespace Enterprise.Customs.PL.Business;

public class CusGuaranteeHeaderValidation : EU.Business.CusGuaranteeHeaderValidation
{
	public CusGuaranteeHeaderValidation(CusGuaranteeHeader parent) : base(parent)
	{
	}

	protected override void CheckMainAccessCode()
	{
		base.CheckMainAccessCode();

		var mainAccessCode = Parent.MainAccessCode;
		if (!mainAccessCode.IsEmpty)
		{
			if (mainAccessCode.ContainsAnyLetters)
			{
				Parent.MainAccessCodeInfo.AddMessageError(Res.GetString("PLCusGuaranteeHeaderValidation|MainAccessCodeOnlyDigitsMessage", "Main Access Code should contain digits only"));
			}

			if (mainAccessCode.Length < 4)
			{
				Parent.MainAccessCodeInfo.AddMessageError(Res.GetString("PLCusGuaranteeHeaderValidation|MainAccessCodeFourCharactersMessage", "Main Access Code should contain 4 characters"));
			}
		}
	}
}
