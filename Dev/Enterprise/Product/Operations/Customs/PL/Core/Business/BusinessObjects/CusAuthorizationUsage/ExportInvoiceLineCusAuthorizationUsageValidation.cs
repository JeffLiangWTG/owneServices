using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class ExportInvoiceLineCusAuthorizationUsageValidation : CusAuthorizationUsageValidation
{
	public ExportInvoiceLineCusAuthorizationUsageValidation(EU.Business.AutoCusAuthorizationUsage parent) : base(parent)
	{
	}

	protected override void CheckAGC_OH_Owner()
	{
		base.CheckAGC_OH_Owner();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.AGC_OH_OwnerInfo);
	}
}
