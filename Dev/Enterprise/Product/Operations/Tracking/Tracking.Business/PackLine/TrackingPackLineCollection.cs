using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingPackLineCollection : ForwardingPackLineCollection
	{
		public TrackingPackLineCollection(TrackingShipment master)
			: base(master)
		{
		}

		public new TrackingPackLine AddNew()
		{
			return (TrackingPackLine)base.AddNew();
		}

		public new TrackingPackLine this[int index]
		{
			get { return (TrackingPackLine)Elements[index]; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(TrackingPackLine);
		}
	}
}
