
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentList : CFSShipmentList
	{
		public GatePassShipmentList(BusinessObjectFactory factory) : base(factory)
		{
			Sort(GatePassShipment.Schema.JS_UniqueConsignRef, ListSortDirection.Descending);
		}

		public GatePassShipmentList(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public new GatePassShipment this[int index]
		{
			get { return (GatePassShipment)Elements[index]; }
		}

		public new GatePassShipment AddNew()
		{
			return (GatePassShipment)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Fetch Strategy

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new GatePassShipmentCollectionFetchStrategy(this);
		}

		class GatePassShipmentCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public GatePassShipmentCollectionFetchStrategy(GatePassShipmentList collection) : base(collection)
			{
			}

			protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				foreach (TableColumn column in columns)
				{
					if (column.ColumnName == GatePassShipment.Schema.JS_GatePassStatusShort)
					{
						FetchLogsNeededToDetermineSeaCargoStatus(businessObjects);
					}
				}

				base.FetchForViewCore(businessObjects, columns);
			}

			void FetchLogsNeededToDetermineSeaCargoStatus(BusinessObject[] businessObjects)
			{
				ZQuery sL_ReferenceFilter = OldCFSShipmentStatusProvider.CMRSL_ReferenceFilter;

				ZQuery pkFilter = new ZQuery();
				pkFilter.DefaultJoinCondition = JoinCondition.Or;
				foreach (GatePassShipment shipment in businessObjects)
				{
					pkFilter.AddToFilter(StmALogSchema.SL_Parent, shipment.PK);
					shipment.LogsNeededToDetermineSeaCargoStatusWerePrefetched = true;
				}

				ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.SeaCargoDepotEvent.Code);
				filter.AddToFilter(pkFilter);
				filter.AddToFilter(sL_ReferenceFilter);
				filter.OrderBy = StmALogSchema.Constants.SL_EventTime + " " + OrderByClause.Descending;

				Collection.Factory.Load(typeof(StmALog), filter); // pre-fetch all logs we will need to determine Sea Cargo Status
			}

			new GatePassShipmentList Collection
			{
				get { return (GatePassShipmentList)base.Collection; }
			}
		}

		#endregion
	}
}
