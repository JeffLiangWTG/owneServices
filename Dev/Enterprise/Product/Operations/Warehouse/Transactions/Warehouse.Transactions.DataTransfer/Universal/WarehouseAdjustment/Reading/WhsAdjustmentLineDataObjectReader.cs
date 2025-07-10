using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsAdjustmentLineDataObjectReader : WhsAdjustmentAndPickableDocketLineDataObjectReader<WhsAdjustment, WhsAdjustmentLine>
	{
		internal WhsAdjustmentLineDataObjectReader(OrderLine adjustmentLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsAdjustment parent)
			: base(adjustmentLineDataObject, logger, factory, parent)
		{
		}

		protected override bool IsDataSourceCustoms => false;

		#region Create / Update Job (Line)

		protected override void PopulateBusinessObjectCore(WhsAdjustmentLine line)
		{
			base.PopulateBusinessObjectCore(line);

			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_AdjustmentArrivalDate, dataObject.ArrivalDate);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_PalletID, dataObject.PalletID);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_OriginalInventoryStatus, line.Lookups.InventoryStatuses, dataObject.InventoryStatus);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_CurrentInventoryStatus, line.Lookups.InventoryStatuses, dataObject.InventoryStatus);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_ReasonCode, line.Lookups.AdjustmentReasonCodes, dataObject.AdjustmentReason);
			PopulateHoldCode(line);
			PopulateLocation(line);

			SetValue(line.CustomsData, WhsBondedWarehouseAttributeSchema.WB_BondedWhsQty, dataObject.PackageQty);
			SetValue(line.CustomsData, WhsBondedWarehouseAttributeSchema.WB_AllDutiesAmount, dataObject.CustomsData?.AllDutiesAmount);
			SetValue(line.CustomsData, WhsBondedWarehouseAttributeSchema.WB_VATAmount, dataObject.CustomsData?.VATAmount);
		}

		#region PopulateHoldCode

		void PopulateHoldCode(WhsAdjustmentLine line)
		{
			if (dataObject.OriginalHoldCode != null)
			{
				SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_WHC_NKOriginalInventoryHeldCode, line.Lookups.InventoryHeldCodeCollection, dataObject.OriginalHoldCode);
				SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, line.Lookups.InventoryHeldCodeCollection, dataObject.OriginalHoldCode);
			}
			else if (dataObject.InventoryStatus != null && dataObject.InventoryStatus.Code.HasValue)
			{
				var inventoryStatus = dataObject.InventoryStatus.Code;
				if (inventoryStatus.Value == InventoryStatus.Codes.Held)
				{
					SetHoldCode(line, InventoryHoldCodes.Codes.Held);
				}
				else if (inventoryStatus.Value == "DAM")
				{
					SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_OriginalInventoryStatus, InventoryStatus.Codes.Held);
					SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_CurrentInventoryStatus, InventoryStatus.Codes.Held);
					SetHoldCode(line, InventoryHoldCodes.Codes.Damaged);
				}
			}
		}

		void SetHoldCode(WhsAdjustmentLine line, string holdCode)
		{
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_WHC_NKOriginalInventoryHeldCode, holdCode);
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, holdCode);
		}

		#endregion

		#region PopulateLocation

		void PopulateLocation(WhsAdjustmentLine line)
		{
			var locationDO = dataObject.Location;
			if (locationDO != null)
			{
				var rowName = locationDO.Row.GetValueOrDefault();
				var column = locationDO.Column.GetValueOrDefault();
				var level = locationDO.Level.GetValueOrDefault();
				var tray = locationDO.Tray.GetValueOrDefault();

				var row = Parent.Warehouse.Rows.Cast<WhsRow>().FirstOrDefault(r => r.WR_Name == rowName);
				var locationBO = row?.Locations.FindByColumnLevelTray(column, level, tray);

				if (locationBO != null)
				{
					line.WE_WL = locationBO.PK;
				}
				else
				{
					// GE/BRS -- in fixing this defect we discovered that UXML import does not support 0-based locations. we need to address that separately, i.e. not in this defect fix.
					throw new DataObjectReadFailureException(Res.GetString("b1818a4e-1583-46b9-8900-40f9d61f6d8f", "Cannot Import {0} Line {1}:\r\nInvalid Location: Row = {2}, Column = {3}, Level = {4}, Tray = {5}.",
						DocketLineType, dataObject.LineNumber.GetValueOrDefault(), rowName, column, level, tray));
				}
			}
			else if (UseDefaultLocationIfNoneProvided)
			{
				var defaultLocation = Parent.Warehouse.DefaultLocation;
				if (defaultLocation != null)
				{
					line.WE_WL = defaultLocation.PK;
				}
			}
		}

		protected virtual bool UseDefaultLocationIfNoneProvided
		{
			get { return false; }
		}

		#endregion

		#region PopulateProduct

		protected override string DocketLineType
		{
			get { return Res.GetString("ddd6fd39-8293-474c-9dcc-32cd9f065788", "Adjustment"); }
		}

		protected override bool CanCreateNewProducts
		{
			get { return false; }
		}

		#endregion

		#region SetBondedEntryKey

		protected override void SetBondedEntryKey(WhsAdjustmentLine line)
		{
			var bondedEntryKey = dataObject.CustomsData?.GetFormattedCustomsEntryKeyWithLineNo();
			SetValueWhenNotReadOnlyOrCustomsTransaction(line, WhsDocketLineSchema.WE_BondedEntryKey, bondedEntryKey);
		}

		#endregion

		#region SetTransactionQuantity

		protected override void CheckTransactionQuantity(WhsAdjustmentLine line)
		{
			if (dataObject.OrderedQty == 0)
			{
				throw new DataObjectReadFailureException(Res.GetString("a583d69c-11b1-4b96-a5b7-0f60020e7121", "Cannot Import Adjustment Line {0}:\r\nInvalid Quantity: {1}",
					dataObject.LineNumber.GetValueOrDefault(), WhsAdjustmentLineValidation.ErrorMsgCannotBeZero));
			}
		}

		#endregion

		#endregion
	}
}
