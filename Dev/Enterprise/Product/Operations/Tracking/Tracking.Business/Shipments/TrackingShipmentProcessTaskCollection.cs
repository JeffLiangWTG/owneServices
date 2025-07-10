using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingShipmentProcessTaskCollection : Freight.Forwarding.Business.ForwardingShipmentProcessTaskCollection
	{
		public TrackingShipmentProcessTaskCollection(TrackingShipment shipment)
			: base(shipment)
		{
		}

		public new TrackingShipmentProcessTask this[int index]
		{
			get { return (TrackingShipmentProcessTask)Elements[index]; }
		}

		public new TrackingShipmentProcessTask AddNew()
		{
			return (TrackingShipmentProcessTask)base.AddNew();
		}

		public override ProcessTaskCollection CreateNewCollection()
		{
			return new TrackingShipmentProcessTaskCollection(Parent);
		}

		#region Implementation

		new TrackingShipment Parent
		{
			get { return (TrackingShipment)base.Parent; }
		}

		#endregion
	}
}
