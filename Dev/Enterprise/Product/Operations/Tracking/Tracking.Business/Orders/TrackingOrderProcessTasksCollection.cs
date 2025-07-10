using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderProcessTasksCollection : OrderProcessTasksCollection
	{
		public TrackingOrderProcessTasksCollection(TrackingOrder order)
			: base(order)
		{
		}

		public new TrackingOrderProcessTasks this[int index]
		{
			get { return (TrackingOrderProcessTasks)Elements[index]; }
		}

		public new TrackingOrderProcessTasks AddNew()
		{
			return (TrackingOrderProcessTasks)base.AddNew();
		}
	}
}
