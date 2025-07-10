using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ISerialNumberPivotAssigner
	{
		ZGuid InventoryPK { get; }
		ZBool Selected { get; }
		ZString SerialNumberValue { get; }
		ZGuid PickingLinePK { get; }
		WhsPickLine PickLineforPicking { get; }

		void SelectSerialNumber(ZGuid pickLinePK);
		void RemoveSerialNumberSelection();
		void SelectSerialNumber();
	}
}
