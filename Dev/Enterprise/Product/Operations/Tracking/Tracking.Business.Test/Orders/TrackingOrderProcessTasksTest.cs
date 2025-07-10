using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrderProcessTasks))]
	sealed class TrackingOrderProcessTasksTest : OrderProcessTasksTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			TrackingOrder order = Factory.New<TrackingOrder>();
			return order.WorkflowItems.AddNew();
		}

		#endregion
	}
}
