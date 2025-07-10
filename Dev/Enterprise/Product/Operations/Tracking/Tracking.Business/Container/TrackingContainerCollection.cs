
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingContainerCollection : ForwardingContainerCollection
	{
		public TrackingContainerCollection(TrackingConsol parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public new TrackingContainer this[int index]
		{
			get { return (TrackingContainer)Elements[index]; }
		}

		public new TrackingContainer AddNew()
		{
			return (TrackingContainer)base.AddNew();
		}
	}
}
