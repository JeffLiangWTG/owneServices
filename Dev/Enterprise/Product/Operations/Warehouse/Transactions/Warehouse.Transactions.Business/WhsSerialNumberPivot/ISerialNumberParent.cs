using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ISerialNumberParent
	{
		ZGuid PK { get; }
		ZGuid ClientPK { get; }
		ZGuid ProductPK { get; }
		string TablePrefix { get; }
		bool SerialNumberReadOnly { get; }
		bool IsInDatabase { get; }
		bool IsSerialNumberUsed { get; }
		BusinessObjectFactory Factory { get; }
		bool IsAllowedToCreateOriginalSerialNumberRecord { get; }
		WhsSerialNumberPivotCollection SerialNumbers { get; }
		bool IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot);
	}
}
