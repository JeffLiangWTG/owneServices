using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsReceiveCollection : NonPersistentBusinessObjectCollection<TrackingWhsReceive>
	{
		public TrackingWhsReceiveCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			receives = new WhsReceiveCollection(Factory);
		}

		readonly WhsReceiveCollection receives;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingWhsReceive);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new TrackingWhsReceiveCollectionFetchStrategy(this);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return TrackingHelper.Get(receives.AddNew());
		}

		public override void Load(ZQuery filter)
		{
			RemoveAll();

			// Architecture calls Load(query) with a ZQuery that has an Order By.
			// Since Order Bys are not supported on Active Business Object Collections,
			// we simply clone the original filter and remove the Order by.
			var clone = filter.ShallowClone();
			clone.OrderBy = "";
			receives.AdditionalFilter = clone;

			foreach (WhsReceive receive in receives)
			{
				Add(TrackingHelper.Get(receive));
			}
		}
	}
}
