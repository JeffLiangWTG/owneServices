using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class ImportJobDeclarationValidation
{
	protected override void CheckJE_ExciseCode()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_ExciseCodeInfo);
	}

	protected override void CheckJE_VATDeferType()
	{
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_VATDeferTypeInfo);
	}
}
