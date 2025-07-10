using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business
{
	class TrackingWhsReceiveLookups : WhsReceiveLookups
	{
		public TrackingWhsReceiveLookups(TrackingWhsReceive parent)
			: base(parent.WhsReceive)
		{
		}

		public override WhsWarehouseCollection Warehouses
		{
			get
			{
				if (fWarehouses == null)
				{
					fWarehouses = new WhsWarehouseCollection(Factory, OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>());
				}
				return fWarehouses;
			}
		}
		WhsWarehouseCollection fWarehouses;
	}
}
