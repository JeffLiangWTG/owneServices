using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingMAWBHeaderCollection : BusinessObjectCollection<TrackingMAWBHeader>
	{
		public TrackingMAWBHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
