using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitSeal : EU.ExitControl.Business.CusExitSeal
{
	public CusExitSeal(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override bool IsUCC6Core => Container?.IsUCC6 ?? true;
}
