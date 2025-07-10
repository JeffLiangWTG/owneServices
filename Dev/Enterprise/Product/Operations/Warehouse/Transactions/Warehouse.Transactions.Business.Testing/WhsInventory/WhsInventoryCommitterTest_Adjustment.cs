using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsInventoryCommitterTest_Adjustment : WhsInventoryCommitterTest<WhsAdjustmentLine, WhsAdjustment>
	{
		#region Implementation

		protected override WhsAdjustment GetNewTransactionLineParentCore(OrgHeader client, WhsWarehouse warehouse, ZString reference, ZString docketSubType, WhsTestHelperFunctions helper)
		{
			var clientPK = client != null ? client.PK : ZGuid.Empty;
			var adjustment = helper.CreateWhsAdjustment(clientPK, warehouse.PK, reference, Notify);
			if (!docketSubType.IsEmpty)
			{
				adjustment.WD_DocketSubType = docketSubType;
			}

			return adjustment;
		}

		protected override ZPropertyInfo GetTotalTransactionQtyInfo(WhsAdjustmentLine transactionLine)
		{
			return transactionLine.WE_TransactionQuantityInfo;
		}

		protected override ZDecimal GetPerPackageQty(WhsAdjustmentLine transactionLine)
		{
			return transactionLine.WE_PerPackageQty;
		}

		protected override void SetInventoryStatusAndHeldCodeCore(WhsAdjustmentLine transactionLine, ZString status, string heldCode)
		{
			transactionLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
			transactionLine.WE_OriginalInventoryStatus = status;
		}

		protected override void SetPalletIdToCommitCore(WhsAdjustmentLine transactionLine, ZString palletId)
		{
			transactionLine.WE_PalletID = palletId;
		}

		protected override void SetProductCore(WhsAdjustmentLine transactionLine, OrgSupplierPart product)
		{
			transactionLine.WE_OP = product != null ? product.PK : ZGuid.Empty;
		}

		protected override void SetPackGroupIdCore(WhsAdjustmentLine transactionLine, ZString packGroupId)
		{
			transactionLine.WE_PackageGroupId = packGroupId;
		}

		protected override void SetPalletIdCore(WhsAdjustmentLine transactionLine, ZString palletID)
		{
			transactionLine.WE_PalletID = palletID;
		}

		protected override WhsAdjustmentLine GetNewTransactionLineCore(WhsAdjustment parent, LineWithCommittedPickLinesData data, WhsTestHelperFunctions helper)
		{
			var result = helper.CreateWhsAdjustmentLine(parent, data.ProductPK, -data.QuantityToCommit, data.LocationString, "", data.ArrivalDate,
				data.InventoryHeldCode, data.PartAttrib1, data.PartAttrib2, data.PartAttrib3, data.SerialNumber, data.ExpiryDate, data.PackingDate);
			result.WE_BondedEntryKey = data.BondedEntryKey;
			result.WE_PackageGroupId = data.PackGroupId;
			result.WE_PalletID = data.PalletId;
			result.WE_CurrentInventoryStatus = data.InventoryStatus;

			return result;
		}

		#endregion
	}
}
