
using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class TrackingContainerStandaloneCollection : BusinessObjectCollection<TrackingContainer>
	{
		public TrackingContainerStandaloneCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
