namespace Enterprise.Customs.PL.Business.Declaration;

public abstract class BaseExportJobDeclarationValidation(JobDeclaration parent) : JobDeclarationValidation(parent)
{
	protected override void CheckJE_UCR()
	{
		base.CheckJE_UCR();

		CheckRuleC0059();
	}

	void CheckRuleC0059()
	{
		var declaration = Parent;

		if (declaration.ZG_SpecificCircumstanceIndicator == SpecificCircumstanceIndicatorForUCCList.Codes.A20
			&& !declaration.JE_UCR.IsEmpty)
		{
			declaration.JE_UCRInfo.AddMessageError(Res.GetString("6296E90F-4052-4813-ADB5-0E317FE26AA0", "[C0059] DUCR number should be empty when Circumstance contains value 'A20'."));
		}

		if (declaration.ZG_SpecificCircumstanceIndicator.IsEmpty
			&& declaration.JE_UCR.IsEmpty)
		{
			declaration.JE_UCRInfo.AddMessageError(Res.GetString("D6B8D25B-4C00-47CB-A32F-7D8648F7C3E1", "[C0059] DUCR number is required when Circumstance is empty."));
		}
	}
}
