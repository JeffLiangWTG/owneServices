using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CusAuthorizationUsageValidation : EU.Business.CusAuthorizationUsageValidation
{
	public CusAuthorizationUsageValidation(AutoCusAuthorizationUsage parent) : base(parent)
	{
	}

	protected CusEntryInstruction Instruction => ((CusAuthorizationUsage)Parent).Instruction;
}
