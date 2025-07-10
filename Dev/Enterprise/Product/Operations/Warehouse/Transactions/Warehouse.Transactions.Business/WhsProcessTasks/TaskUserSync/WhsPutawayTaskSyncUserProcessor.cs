using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayTaskSyncUserProcessor : IProcessTaskSyncUserProcessor
	{
		public void SyncUser(BusinessObjectFactory factory, IProcessTask task)
		{
			var query = new ZQuery(WhsDocketLineSchema.WE_P9_Task, task.PK);
			query.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.Equal, DocketLineStatus.Codes.Entered);
			var putawayTransferLines = factory.Load<WhsTransferLine>(query);
			if (putawayTransferLines.Length > 0)
			{
				var warehouse = putawayTransferLines[0].Warehouse;
				if (warehouse.IsTaskManagementEnabled)
				{
					AddFetchHints(factory, putawayTransferLines);
					var userToSync = task.P9_GS_NKAssignedStaffMember;
					foreach (var line in putawayTransferLines)
					{
						line.WE_GS_NKPutawayBy = userToSync;
					}
					var pickLinesQuery = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, putawayTransferLines.Select(t => t.PK));
					var pickLines = factory.Load<WhsPickLine>(pickLinesQuery);
					foreach (var pickLine in pickLines)
					{
						pickLine.WZ_GS_NKAssignedTo = userToSync;
					}
				}
			}
		}

		void AddFetchHints(BusinessObjectFactory factory, WhsTransferLine[] putawayTransferLines)
		{
			foreach (var line in putawayTransferLines)
			{
				factory.AddFetchHint(WhsDocketLineSchema.WE_WE_MatchingLine, line.PK);
				factory.AddFetchHint(WhsDocketLineSchema.WE_WE_ParentDocketLine, line.PK);
			}
		}
	}
}
