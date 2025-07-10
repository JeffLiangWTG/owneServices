using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	class TrackingWhsOrderLookups : WhsOrderLookups
	{
		public TrackingWhsOrderLookups(TrackingWhsOrder parent)
			: base(parent.WhsOrder)
		{
		}

		public override WhsWarehouseCollection Warehouses
		{
			get
			{
				if (fWarehouses == null)
				{
					ZQuery whsQuery = (WebEnv.AppInstance != null) ? OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>() : ZQuery.NoResultQuery;
					fWarehouses = new WhsWarehouseCollection(Factory, whsQuery);
				}
				return fWarehouses;
			}
		}
		WhsWarehouseCollection fWarehouses;
	}
}
