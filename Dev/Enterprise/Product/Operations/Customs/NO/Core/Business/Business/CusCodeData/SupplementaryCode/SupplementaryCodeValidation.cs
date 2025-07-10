using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class SupplementaryCodeValidation : BaseSupplementaryCodeValidation
{
	public SupplementaryCodeValidation(BaseSupplementaryCode parent) : base(parent)
	{
	}

	protected override void CheckCY_Code()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_CodeInfo);
	}
}
