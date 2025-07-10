using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingCartageCollection : ActiveBusinessObjectCollection<TrackingCartage>
	{
		public TrackingCartageCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}
