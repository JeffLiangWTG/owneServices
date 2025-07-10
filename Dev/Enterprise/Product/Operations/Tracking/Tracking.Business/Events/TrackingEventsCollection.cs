using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business
{
	public sealed class TrackingEventsCollection : StmALogCollection
	{
		public TrackingEventsCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new TrackingEventsFetchStrategy(this);
	}
}
