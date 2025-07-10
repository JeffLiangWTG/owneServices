using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyContainerStandaloneCollection : BusinessObjectCollection<LinerAndAgencyContainer>
	{
		public LinerAndAgencyContainerStandaloneCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
