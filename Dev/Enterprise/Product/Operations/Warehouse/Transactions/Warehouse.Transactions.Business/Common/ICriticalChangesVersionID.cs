using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICriticalChangesVersionID
	{
		ZGuid CriticalChangesVersionID { get; set; }
		bool IsImmutableStatus { get; }
	}
}
