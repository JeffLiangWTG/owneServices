using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PutawayHelper
	{
		public static ZQuery FindInventoryQuery(string[] palletIDs, ZGuid whsPK)
		{
			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_InDocketLineType, DocketType.Codes.Receive);
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, whsPK);

			var receiveLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.PK);

			AddCommonFiltersToReceiveLineQuery(receiveLineSubQuery, palletIDs);
			inventoryQuery.AddSubQuery(receiveLineSubQuery, JoinCondition.And);
			inventoryQuery.ReLoadExistingRows = true;

			return inventoryQuery;
		}

		static void AddCommonFiltersToReceiveLineQuery(ZDBOnlyQuery receiveLineQuery, IEnumerable<string> palletIDs)
		{
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletIDs);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, SQLComparisonOperator.Equal, DocketType.Codes.Receive);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			receiveLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Cancelled);
		}

		public static WhsTransferLine GetPutawayTransferLineFromInventory(WhsInventoryView inventory)
		{
			WhsTransferLine result = null;
			var docketLine = inventory.InDocketLine;

			if (docketLine is WhsReceiveLine receiveLine)
			{
				result = receiveLine.PutawayTransferLine;
			}
			else if (docketLine is WhsTransferLine transferLine)
			{
				result = transferLine.IsPutawayTransferLine ? transferLine : null;
			}

			return result;
		}
	}
}
