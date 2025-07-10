using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingShipmentCollection : ForwardingShipmentCollection
	{
		public TrackingShipmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new TrackingShipment this[int index]
		{
			get { return (TrackingShipment)Elements[index]; }
		}

		public new TrackingShipment AddNew()
		{
			return (TrackingShipment)base.AddNew();
		}
	}
}
