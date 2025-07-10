using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclarationValidation
{
	#region JE_SpecificCurcumstanceIndicator

	protected override void CheckJE_SpecificCircumstanceIndicator()
	{
		base.CheckJE_SpecificCircumstanceIndicator();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo);
	}

	#endregion

	#region JE_CTStatusID

	protected override void CheckJE_CTStatusID()
	{
		base.CheckJE_CTStatusID();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_CTStatusIDInfo);
	}

	#endregion
}
