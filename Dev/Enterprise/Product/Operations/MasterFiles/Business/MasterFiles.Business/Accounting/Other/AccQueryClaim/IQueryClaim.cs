using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IQueryClaim
	{
		ZGuid AY_OH_Debtor { get; }
		ZGuid AY_GB { get; }
		BusinessObjectFactory Factory { get; }
		ZString Ledger { get; }
	}
}
