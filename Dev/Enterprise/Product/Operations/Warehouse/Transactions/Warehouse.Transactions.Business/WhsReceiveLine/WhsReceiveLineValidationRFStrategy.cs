using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsReceiveLineValidationRFStrategy : WhsReceiveLineValidationStrategy
	{
		public WhsReceiveLineValidationRFStrategy(WhsReceiveLine parent)
			: base(parent)
		{
		}

		protected override bool IsPutawayTransferCreatedForThisPalletID(WhsReceive receive, string palletID)
		{
			var result = false;
			var palletIDQuery = new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK);
			palletIDQuery.FetchOnlyFromLocalCache = true;
			palletIDQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletID);

			var inventoryLinesInMemory = Parent.Factory.Load<WhsReceiveLine>(palletIDQuery);
			result = inventoryLinesInMemory.Any(l => l.HasPutawayTransfer);

			if (!result)
			{
				var lineWithPutawaTransfer = new DynamicBusinessObjectCollection(Parent.Factory);
				var query = FormattableString.Invariant($@"SELECT TOP(1) RowExists = CONVERT(bit, NULL) 
	FROM {WhsDocketLineSchema.Constants.SqlSchemaName}.{WhsDocketLineSchema.Constants.TableName} as ReceiveLine
	JOIN {WhsDocketSchema.Constants.SqlSchemaName}.{WhsDocketSchema.Constants.TableName} as Receive ON ReceiveLine.{WhsDocketLineSchema.Constants.WE_WD} = Receive.{WhsDocketSchema.Constants.PK}
	LEFT JOIN {WhsPickLineSchema.Constants.SqlSchemaName}.{WhsPickLineSchema.Constants.TableName} as PickLine ON PickLine.{WhsPickLineSchema.Constants.WZ_WE_InventoryLine} = ReceiveLine.{WhsDocketLineSchema.Constants.PK}
	LEFT JOIN {WhsDocketLineSchema.Constants.SqlSchemaName}.{WhsDocketLineSchema.Constants.TableName} as TransferLine ON PickLine.{WhsPickLineSchema.Constants.WZ_WE_TransactionLine} = TransferLine.{WhsDocketLineSchema.Constants.PK}
	LEFT JOIN {WhsDocketSchema.Constants.SqlSchemaName}.{WhsDocketSchema.Constants.TableName} as Transfer ON TransferLine.{WhsDocketLineSchema.Constants.WE_WD} = Transfer.{WhsDocketSchema.Constants.PK} AND Transfer.{WhsDocketSchema.Constants.WD_IsPutawayTransfer} = 1
	WHERE
		ReceiveLine.{WhsDocketLineSchema.Constants.WE_PalletID} = @PalletID AND
		ReceiveLine.{WhsDocketLineSchema.Constants.WE_PalletID} <> '' AND
		ReceiveLine.{WhsDocketLineSchema.Constants.WE_StockOnHand} > 0 AND
		Receive.{WhsDocketSchema.Constants.WD_WW_Whs} = @Warehouse AND
		(
			(ReceiveLine.{WhsDocketLineSchema.Constants.WE_DocketLineStatus} = '{DocketLineStatus.Codes.PickedForUnload}') OR
			(ReceiveLine.{WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus} = '{InventoryStatus.Codes.PuttingAway}') OR
			(
				ReceiveLine.{WhsDocketLineSchema.Constants.WE_WL_TransferFrom} IS NOT NULL AND
				ReceiveLine.{WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus} = '{InventoryStatus.Codes.Putaway}' AND
				Transfer.{WhsDocketSchema.Constants.PK} IS NOT NULL
			)
		)");

				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@Warehouse", receive.WD_WW_Whs, WhsDocketSchema.WD_WW_Whs);
				sqlParams.Add("@PalletID", palletID, WhsDocketLineSchema.WE_PalletID);

				lineWithPutawaTransfer.Load(query, sqlParams);
				result = lineWithPutawaTransfer.Count > 0;
			}

			return result;
		}
	}
}
