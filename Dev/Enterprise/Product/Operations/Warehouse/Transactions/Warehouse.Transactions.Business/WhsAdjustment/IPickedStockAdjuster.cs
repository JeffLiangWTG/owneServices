using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickedStockAdjuster
	{
		void AdjustOutInventory(WhsInventoryView inventory, ZGuid originalInventoryPK, ZDecimal quantity);
		void LinkToDocket(WhsPickableDocket docket);
		ActionResult PrepareForSaving();
	}
}
