using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsTransferValidationHelper
	{
		#region FindUNDGsOverLimit

		public static bool IsLineUNDGValidationRequired(WhsTransfer transfer) =>
			transfer.WD_IsPutawayTransfer || transfer.IsInterWarehouseTransfer;

		public static ZString FindUNDGsOverLimit(WhsTransfer transfer, IEnumerable<WhsTransferLine> unfinalisedLines)
		{
			var returnMessage = ZString.Empty;
			if (IsLineUNDGValidationRequired(transfer))
			{
				var validationMessage = ZString.Empty;
				if (transfer.WD_IsPutawayTransfer || transfer.WD_DocketSubType == TransferType.Codes.InterWhsDest)
				{
					validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(transfer.Factory, transfer.Warehouse, transfer.PK, unfinalisedLines.ToArray()).OverLimitMessage;
				}
				else if (transfer.WD_DocketSubType == TransferType.Codes.InterWhsSource)
				{
					validationMessage = CheckUNDGForInterWhsSourceTransfer(transfer, unfinalisedLines);
				}

				if (!validationMessage.IsEmpty)
				{
					returnMessage = transfer.Validation.BuildUNDGLimitExceededError(validationMessage);
				}
			}

			return returnMessage;
		}

		static ZString CheckUNDGForInterWhsSourceTransfer(WhsTransfer transfer, IEnumerable<WhsTransferLine> transferLines)
		{
			var childTransfers = transferLines
				.GroupBy(g => g.DestinationWarehousePK)
				.ToDictionary(t => t.Key, t => t.ToList());

			var factory = transfer.Factory;
			var warehouses = factory.Load<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.PK, childTransfers.Keys)).ToDictionary(d => d.PK);

			var stringBuilder = new ZStringBuilder();
			foreach (var childTransfer in childTransfers)
			{
				var destinationWarehouse = warehouses[childTransfer.Key];
				if (destinationWarehouse.WW_IsDangerousGoodsManagementEnabled)
				{
					var validationMessage = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(factory, destinationWarehouse, transfer.PK, childTransfer.Value).OverLimitMessage;
					if (!validationMessage.IsEmpty)
					{
						stringBuilder.AppendLine(Res.GetString("aeda2031-02dd-47d1-a6ca-db5d50517087", "Warehouse {0}", destinationWarehouse.WW_WarehouseCode));
						stringBuilder.AppendLine(validationMessage);
					}
				}
			}

			return stringBuilder.ToString();
		}

		#endregion
	}
}
