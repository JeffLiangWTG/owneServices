using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingShipment))]
	sealed class TrackingShipmentBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingShipment>
	{
		protected override TrackingShipment GetNewBizOForNotification()
		{
			var trackingShipment = Factory.New<TrackingShipment>();
			trackingShipment.JS_IsForwardRegistered = true;
			trackingShipment.JS_TransportMode = "SEA";
			trackingShipment.JS_PackingMode = "FCL";

			var consol = trackingShipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			trackingShipment.OuterPackLines.AddNew();

			var confirm1 = container.Confirms.AddNew();
			confirm1.EU_PickupDeliveryType = Core.Constants.PickupDeliveryConfirmTypes.DestinationDelivery;

			trackingShipment.DeliveryConfirms.AddNew();

			return trackingShipment;
		}
	}
}
