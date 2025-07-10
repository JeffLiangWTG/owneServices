using CargoWise.EntityFramework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public class ImportEntryInstructionCusAuthorizationUsageValidation : CusAuthorizationUsageValidation
{
	public ImportEntryInstructionCusAuthorizationUsageValidation(EU.Business.AutoCusAuthorizationUsage parent) : base(parent)
	{
	}

	protected new CusAuthorizationUsage Parent => (CusAuthorizationUsage)base.Parent;

	protected override void CheckAGC_OH_Owner()
	{
		base.CheckAGC_OH_Owner();

		if (Parent.CustomsCode != CusAuthorizationUsageType.C506)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AGC_OH_OwnerInfo);
		}
	}
}
