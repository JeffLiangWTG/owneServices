using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class AdjustmentLineFromInventoryHelper : DocketLineFromInventoryHelper<WhsAdjustmentLine>
	{
		public AdjustmentLineFromInventoryHelper(INotifications notifications, WhsAdjustment adjustment)
			: base(notifications, adjustment)
		{
		}

		protected override ICopyCustomsStrategy CreateCopyCustomsStrategyCore() => new AdjustmentCopyCustomsStrategy();

		protected override void SetDocketLineFromInventoryCore(WhsAdjustmentLine transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			base.SetDocketLineFromInventoryCore(transactionLine, inventoryLine, exclude);
			transactionLine.WE_AdjustmentArrivalDate = inventoryLine.WE_AdjustmentArrivalDate;
			transactionLine.WE_WHC_NKOriginalInventoryHeldCode = inventoryLine.WE_WHC_NKCurrentInventoryHeldCode;
			transactionLine.WE_OriginalInventoryStatus = inventoryLine.WE_CurrentInventoryStatus;
			transactionLine.WE_WHC_NKCurrentInventoryHeldCode = inventoryLine.WE_WHC_NKCurrentInventoryHeldCode;
			transactionLine.WE_CurrentInventoryStatus = inventoryLine.WE_CurrentInventoryStatus;
			transactionLine.WE_ExpiryDate = inventoryLine.WE_ExpiryDate;
		}

		protected override void SetDocketLineFromInventoryBeforeSettingAdjustmentArrivalDate(WhsAdjustmentLine transactionLine, WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
		{
			base.SetDocketLineFromInventoryBeforeSettingAdjustmentArrivalDate(transactionLine, inventoryLine, exclude);

			if (!exclude.HasFlag(ExcludeFromCopy.Qty))
			{
				transactionLine.WE_TransactionQuantity = -inventoryLine.AvailableToPickQuantity;
			}
		}
	}
}
