using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingShipmentProcessTaskCollection))]
	sealed class TrackingShipmentProcessTaskCollectionTest : RoutingSupportProcessTaskCollectionTest<TrackingShipmentProcessTaskCollection>
	{
		#region Implementation

		protected override TrackingShipmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new TrackingShipmentProcessTaskCollection(Shipment);
		}

		TrackingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.NewWithValidTestData<TrackingShipment>()); }
		}
		TrackingShipment shipment;

		#endregion
	}
}
