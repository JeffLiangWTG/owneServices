using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingWhsOrderCollection : NonPersistentBusinessObjectCollection<TrackingWhsOrder>
	{
		public TrackingWhsOrderCollection(BusinessObjectFactory factory) : base(factory)
		{
			orders = new WhsOrderCollection(factory);
		}

		protected WhsOrderCollection orders;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return TrackingHelper.Get(orders.AddNew());
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public override void Load(ZQuery filter)
		{
			RemoveAll();

			// Architecture calls Load(query) with a ZQuery that has an Order By.
			// Since Order Bys are not supported on Active Business Object Collections,
			// we simply clone the original filter and remove the Order by.
			var clone = filter.ShallowClone();
			clone.OrderBy = "";
			orders.AdditionalFilter = clone;

			foreach (WhsOrder declaration in orders)
			{
				Add(TrackingHelper.Get(declaration));
			}
		}
	}
}
