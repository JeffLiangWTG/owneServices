using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NonPersistentNctsUnloadingRemarkValidation : AutoNonPersistentNctsUnloadingRemarkValidation
{
	public NonPersistentNctsUnloadingRemarkValidation(AutoNonPersistentNctsUnloadingRemark parent) : base(parent)
	{
	}

	protected override void CheckCode()
	{
		base.CheckCode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CodeInfo);
	}
}
