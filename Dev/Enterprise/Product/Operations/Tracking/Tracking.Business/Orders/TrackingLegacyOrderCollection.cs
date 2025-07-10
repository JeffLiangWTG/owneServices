using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Collection of orders for web tracking
	/// </summary>
	public class TrackingLegacyOrderCollection : BusinessObjectCollection<TrackingOrder>
	{
		public TrackingLegacyOrderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
