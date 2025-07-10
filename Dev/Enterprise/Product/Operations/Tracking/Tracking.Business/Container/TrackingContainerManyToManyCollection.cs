
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingContainerManyToManyCollection : ForwardingContainerManyToManyCollection
	{
		public TrackingContainerManyToManyCollection(TrackingPackLine parent) : base(parent)
		{
		}

		public new TrackingContainer this[int index]
		{
			get { return (TrackingContainer)(Elements[index]); }
		}

		public new TrackingContainer AddNew()
		{
			return (TrackingContainer)base.AddNew();
		}
	}
}
