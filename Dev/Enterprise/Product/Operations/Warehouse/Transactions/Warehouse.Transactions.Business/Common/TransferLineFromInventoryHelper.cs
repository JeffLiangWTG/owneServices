using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class TransferLineFromInventoryHelper : DocketLineFromInventoryHelper<WhsTransferLine>
	{
		public TransferLineFromInventoryHelper(INotifications notifications, WhsTransfer transfer)
			: base(notifications, transfer)
		{
		}

		protected override void SetDocketLineFromInventoryCore(WhsTransferLine transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			base.SetDocketLineFromInventoryCore(transactionLine, inventoryLine, exclude);
			transactionLine.WE_AdjustmentArrivalDate = inventoryLine.WE_AdjustmentArrivalDate;

			if (!exclude.HasFlag(ExcludeFromCopy.Qty))
			{
				transactionLine.WE_TransactionQuantity = inventoryLine.Inventory.Count > 0 ? inventoryLine.Inventory[0].WI_AvailableToTransferQuantity : ZDecimal.Zero;
			}

			transactionLine.WE_WHC_NKOriginalInventoryHeldCode = inventoryLine.WE_WHC_NKCurrentInventoryHeldCode;
			transactionLine.WE_OriginalInventoryStatus = inventoryLine.WE_CurrentInventoryStatus;
			transactionLine.WE_CurrentInventoryStatus = inventoryLine.WE_CurrentInventoryStatus;
			transactionLine.WE_WHC_NKCurrentInventoryHeldCode = inventoryLine.WE_WHC_NKCurrentInventoryHeldCode;
		}

		protected override void SetLocationDataFromInventory(WhsTransferLine transactionLine, WhsDocketLine inventoryLine)
		{
			transactionLine.TransferFromWarehousePK = inventoryLine.WarehousePK;
			if (Docket.WD_IsPutawayTransfer)
			{
				transactionLine.WE_PalletID = inventoryLine.WE_PalletID;
				transactionLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			}

			transactionLine.WE_WL_TransferFrom = inventoryLine.WE_WL;
			transactionLine.WE_TransferFromPalletId = inventoryLine.WE_PalletID;
		}
	}
}
