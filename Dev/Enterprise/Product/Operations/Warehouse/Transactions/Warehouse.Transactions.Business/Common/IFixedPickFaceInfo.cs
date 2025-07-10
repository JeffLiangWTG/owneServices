using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IFixedPickFaceInfo
	{
		ZGuid WarehousePK { get; }
		ZGuid PickToReplenishPK { get; }
		ZGuid ClientPK { get; }
		ZGuid ProductPK { get; }
		ZGuid TransferToLocationPK { get; }
		ZDecimal ReplenishQuantity { get; }
		ZDecimal ReplenishMultiple { get; }
		ZBool IsDeadLocked { get; }
	}
}
