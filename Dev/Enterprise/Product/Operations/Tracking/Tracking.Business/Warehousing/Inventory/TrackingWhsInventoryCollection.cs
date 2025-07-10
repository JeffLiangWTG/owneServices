using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsInventoryCollection : WhsInventoryViewCollection, IModuleManualSortCollection
	{
		public TrackingWhsInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TrackingWhsInventoryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public TrackingWhsInventoryCollection(BusinessObjectFactory factory, TrackingWhsReceive receive)
			: base(factory, receive.WhsReceive)
		{
		}

		public TrackingWhsInventoryCollection(BusinessObjectFactory factory, TrackingWhsReceiveLine receiveLine)
			: base(factory, receiveLine.WhsReceiveLine)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingWhsInventory);
		}

		public new TrackingWhsInventory this[int index]
		{
			get { return (TrackingWhsInventory)(Elements[index]); }
		}

		public new TrackingWhsInventory AddNew()
		{
			return (TrackingWhsInventory)base.AddNew();
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new TrackingWhsInventoryCollectionFetchStrategy(this);
		}

		public void Load(ZQuery query, ListSortDescriptionCollection sortInfos)
		{
			this.Load(query);
			((IBindingListView)this).ApplySort(sortInfos);
		}
	}
}
