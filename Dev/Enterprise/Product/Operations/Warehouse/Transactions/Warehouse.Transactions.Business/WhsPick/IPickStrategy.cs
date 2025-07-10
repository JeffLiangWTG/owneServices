using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickStrategy
	{
		bool CanInventoryBeAllocated(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory);

		ZDecimal GetAutoAllocateQuantity(ZDecimal orderedInventoryQtyShort, WhsPickAvailableInventory availableInventory);

		ZDecimal GetQuantityUnPicked(WhsPickAvailableInventory availableInventory);

		bool CanAllocateInQuantitiesDifferentToAutoAllocateQuantity(WhsPickAvailableInventory availableInventory);

		void OnInventoryPicked(WhsPickOrderedInventory orderedInventory, WhsPickAvailableInventory availableInventory, ZDecimal qtyDelta);
	}
}
