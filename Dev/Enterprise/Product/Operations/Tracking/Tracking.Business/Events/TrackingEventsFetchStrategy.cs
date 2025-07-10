using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	sealed class TrackingEventsFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public TrackingEventsFetchStrategy(TrackingEventsCollection collection)
			: base(collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			factory = collection.Factory;
		}
		readonly BusinessObjectFactory factory;

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);

			if (columns.Any(x => x.ColumnName == BaseStmALog.Schema.DisplayEventReference || x.ColumnName == "Event.SE_Desc"))
			{
				foreach (StmALog trackingEvent in businessObjects)
				{
					factory.AddFetchHint(StmEventSchema.Instance, new ZQuery(StmEventSchema.SE_Code, trackingEvent.SL_SE_NKEvent));
				}
			}
		}
	}
}
