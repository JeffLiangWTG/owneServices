using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface ICommissionableTransaction
	{
		ZGuid PK { get; }
		ZString AH_TransactionNum { get; }
		ZString AH_Calc_LocalRXCode { get; }
		ZGuid AH_GC { get; }
		ZGuid AH_GB { get; }
		ZDecimal AH_InvoiceAmount { get; }
		ZBool AH_IsCancelled { get; }
		ZGuid AH_JH { get; }
		ZGuid AH_OH { get; }
		ZDateTime AH_PostDate { get; }
		ZGuid AH_TransactionBelongsToGroup { get; }
		BusinessObjectFactory Factory { get; }
		bool IsJobRelated { get; }
		BusinessObjectCollection Lines { get; }
	}
}
