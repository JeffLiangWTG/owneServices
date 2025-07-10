using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Collection of orders for web tracking
	/// </summary>
	public class TrackingOrderCollection : OrderCollection
	{
		public TrackingOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TrackingOrderCollection(BusinessObjectFactory factory, IAttachOrders parent)
			: base(factory, parent)
		{
		}

		public new TrackingOrder this[int i]
		{
			get { return (TrackingOrder)base[i]; }
		}

		public new TrackingOrder AddNew()
		{
			return (TrackingOrder)base.AddNew();
		}
	}
}
