using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Collection of orders for web tracking
	/// </summary>
	public class TrackingOrderLineCollection : BusinessObjectCollection<TrackingOrderLine>
	{
		public TrackingOrderLineCollection(BusinessObjectFactory factory)
			: base(factory, ZQuery.NoResultQuery)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
