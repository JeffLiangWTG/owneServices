using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	class TrackingWhsReceiveCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public TrackingWhsReceiveCollectionFetchStrategy(TrackingWhsReceiveCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (columns.Any(x => x.ColumnName.StartsWith((NoResString)"Milestones", StringComparison.OrdinalIgnoreCase)))
			{
				foreach (TrackingWhsReceive bizo in businessObjects)
				{
					var factory = Collection.Factory;

					var headerQuery = new ZQuery(JobHeaderSchema.JH_ParentID, bizo.WhsReceive.PK);
					headerQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					factory.AddFetchHint(JobHeaderSchema.Instance, headerQuery);

					var consolidationQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, bizo.WhsReceive.PK);
					consolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.Consignment);
					factory.AddFetchHint(DtbBookingConsolidationSchema.Instance, consolidationQuery);

					var docketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, bizo.WhsReceive.PK);
					factory.AddFetchHint(WhsDocketLineSchema.Instance, docketLineQuery);

					var docketJobPivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, bizo.WhsReceive.PK);
					docketJobPivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Receive);
					factory.AddFetchHint(WhsDocketJobPivotSchema.Instance, docketJobPivotQuery);

					var processTasksQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, bizo.WhsReceive.PK);
					factory.AddFetchHint(ProcessTasksSchema.Instance, processTasksQuery);

					var processHeadersQuery = new ZQuery(ProcessHeaderSchema.FH_ParentId, bizo.WhsReceive.PK);
					processHeadersQuery.AddToFilter(ProcessHeaderSchema.FH_ParentTableCode, WhsDocketSchema.Constants.Prefix);
					factory.AddFetchHint(ProcessHeaderSchema.Instance, processHeadersQuery);
				}
			}
		}
	}
}
