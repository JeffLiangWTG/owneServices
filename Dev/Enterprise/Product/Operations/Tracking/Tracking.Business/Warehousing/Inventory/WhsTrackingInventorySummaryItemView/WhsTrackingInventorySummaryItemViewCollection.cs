using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class WhsTrackingInventorySummaryItemViewCollection : BusinessObjectCollection<WhsTrackingInventorySummaryItemView>
	{
		public WhsTrackingInventorySummaryItemViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsTrackingInventorySummaryItemViewCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(WhsTrackingInventorySummaryItemView);
	}
}
