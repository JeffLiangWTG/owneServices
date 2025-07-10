
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingConsolManyToManyCollection : ForwardingConsolManyToManyCollection
	{
		public TrackingConsolManyToManyCollection(TrackingShipment shipment) : base(shipment)
		{
		}

		protected new TrackingShipment ParentShipment
		{
			get { return (TrackingShipment)base.ParentShipment; }
		}

		public new TrackingConsol this[int index]
		{
			get { return (TrackingConsol)(Elements[index]); }
		}

		public new TrackingConsol AddNew()
		{
			return (TrackingConsol)base.AddNew();
		}

		public override void Load()
		{
			base.Load();
			if (ParentShipment != null)
			{
				foreach (TrackingConsol consol in this)
				{
					if (consol == ParentShipment.DepartureConsol)
					{
						consol.FlightDetailsSuppressionBizO = ParentShipment;
					}
				}
			}
		}
	}
}
