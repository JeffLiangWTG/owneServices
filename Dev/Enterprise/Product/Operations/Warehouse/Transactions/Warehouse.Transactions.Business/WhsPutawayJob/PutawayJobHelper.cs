using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PutawayJobHelper
	{
		// Tested in WhsTransfer.TestFinaliseTransfer_SetsAssociatedPutawayLines_ToNotPuttingAway_DBHits and WhsTransferLine.TestFinaliseDocketLine_PutawayLineIsPuttingAwayRemoved
		public static void RemoveIsPuttingAwayFromAssociatedPutawayLines(WhsTransfer transfer, IEnumerable<WhsTransferLine> finalisingLines)
		{
			Argument.NotNull(transfer, nameof(transfer));
			if (transfer.WD_IsPutawayTransfer)
			{
				var jobQuery = new ZDBOnlySubQuery(typeof(WhsPutawayJob), WhsPutawayJobSchema.PK);
				jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_WW_Warehouse, transfer.WD_WW_Whs);
				jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_GS_NKUser, finalisingLines.Select(l => l.WE_GS_NKPutawayBy).Where(u => !string.IsNullOrEmpty(u)));
				jobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_FinalizedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);

				var lineQuery = new ZDBOnlyQuery(typeof(WhsPutawayLine));
				lineQuery.AddToFilter(WhsPutawayLineSchema.WPL_PalletID, finalisingLines.Select(l => l.WE_TransferFromPalletId).Where(p => !string.IsNullOrEmpty(p)).Distinct());
				lineQuery.AddToFilter(WhsPutawayLineSchema.WPL_IsPuttingAway, true);
				lineQuery.AddSubQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, jobQuery, JoinCondition.And);

				var putawayLines = transfer.Factory.Load<WhsPutawayLine>(lineQuery);
				var putawayLinePKForPalletID = new Dictionary<string, ZGuid>(StringComparer.CurrentCultureIgnoreCase);
				foreach (var putawayLine in putawayLines)
				{
					putawayLine.WPL_IsPuttingAway = false;
					if (!putawayLinePKForPalletID.TryGetValue(putawayLine.WPL_PalletID, out _))
					{
						putawayLinePKForPalletID.Add(putawayLine.WPL_PalletID, putawayLine.PK);
					}
				}
				finalisingLines.ForEach(tl => tl.WE_WPL_PutawayLine = putawayLinePKForPalletID.TryGetValue(tl.WE_TransferFromPalletId, out var linePK) ? linePK : ZGuid.Empty);
			}
		}
	}
}

