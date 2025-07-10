using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrderProcessTasksCollection))]
	sealed class TrackingOrderProcessTasksCollectionTest : ProcessTaskCollectionTest<TrackingOrderProcessTasksCollection>
	{
		#region Implementation

		protected override TrackingOrderProcessTasksCollection GetCollectionToTestCore()
		{
			return new TrackingOrderProcessTasksCollection(Order);
		}

		TrackingOrder Order
		{
			get
			{
				if (order == null)
				{
					order = Factory.NewWithValidTestData<TrackingOrder>();
				}
				return order;
			}
		}
		TrackingOrder order;

		#endregion
	}
}
