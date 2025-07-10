using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Summary description for TrackingShipmentCollection.
	/// </summary>
	public class TrackingConsolShipmentCollection : ForwardingConsolShipmentCollection
	{
		public TrackingConsolShipmentCollection(TrackingConsol parent) : base(parent)
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

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingShipment);
		}
	}
}
