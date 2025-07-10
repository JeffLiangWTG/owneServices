using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentLineDataObjectWriter : WhsDocketLineDataObjectWriter<WhsAdjustmentLine>
	{
		internal WhsAdjustmentLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(OrderLine adjustmentLineDataObject, WhsAdjustmentLine adjustmentLine)
		{
			adjustmentLineDataObject.ArrivalDate = adjustmentLine.WE_AdjustmentArrivalDate;
			adjustmentLineDataObject.PalletID = adjustmentLine.WE_PalletID;
			adjustmentLineDataObject.AdjustmentReason = ListHelper.GetWithDescription<CodeDescriptionPair>(adjustmentLine.WE_ReasonCode, adjustmentLine.Lookups.AdjustmentReasonCodes);
			adjustmentLineDataObject.InventoryStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(adjustmentLine.WE_OriginalInventoryStatus, adjustmentLine.Lookups.InventoryStatuses);

			PopulateHoldCodes(adjustmentLineDataObject, adjustmentLine);
			PopulateLocation(adjustmentLineDataObject, adjustmentLine);
		}

		static void PopulateHoldCodes(OrderLine adjustmentLineDataObject, WhsAdjustmentLine adjustmentLine)
		{
			var holdCode = adjustmentLine.WE_WHC_NKOriginalInventoryHeldCode;
			if (!holdCode.IsEmpty)
			{
				adjustmentLineDataObject.OriginalHoldCode = ListHelper.GetWithDescription<CodeDescriptionPair9Char>(holdCode, adjustmentLine.Lookups.InventoryHeldCodeCollection);
			}
		}

		static void PopulateLocation(OrderLine adjustmentLineDataObject, WhsAdjustmentLine adjustmentLine)
		{
			var location = adjustmentLine.Location;
			if (location != null)
			{
				adjustmentLineDataObject.Location = new Location
				{
					Column = location.WLV_Column,
					Level = location.WLV_Level,
					Tray = location.WLV_Tray,
					Row = location.RowName
				};
			}
		}
	}
}
