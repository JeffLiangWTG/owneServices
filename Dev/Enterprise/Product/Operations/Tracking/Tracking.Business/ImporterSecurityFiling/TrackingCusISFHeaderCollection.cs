using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business.ImporterSecurityFiling
{
	public class TrackingCusISFHeaderCollection : ActiveBusinessObjectCollection<TrackingCusISFHeader>
	{
		public TrackingCusISFHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}
