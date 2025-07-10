using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business;

public class CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster> : EU.Business.CusAuthorizationUsageCollection<TCusAuthorizationUsage, TMaster>
	where TCusAuthorizationUsage : CusAuthorizationUsage
	where TMaster : BusinessObject, ICusAuthorizationUsageMaster, ILinkable
{
	public CusAuthorizationUsageCollection(TMaster master, BusinessObjectFactory factory)
		: base(master, factory)
	{
	}

	internal void ReloadMaxCountValidation()
	{
		MaxCountValidationDisable();
		EnableMaxCountValidation();
	}

	protected override int MaxCountForValidation => Master is Declaration.CusEntryInstruction instruction
													&& (instruction.JobDeclaration?.IsImport ?? ZBool.False)
		? 1 : base.MaxCountForValidation;
}
