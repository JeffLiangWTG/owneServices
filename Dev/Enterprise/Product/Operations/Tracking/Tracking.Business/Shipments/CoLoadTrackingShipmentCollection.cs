using System;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class CoLoadTrackingShipmentCollection : CoLoadForwardingShipmentCollection
	{
		public CoLoadTrackingShipmentCollection(TrackingShipment master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public new TrackingShipment this[int i]
		{
			get { return (TrackingShipment)base[i]; }
		}

		public new TrackingShipment AddNew()
		{
			return (TrackingShipment)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingShipment);
		}
	}
}
