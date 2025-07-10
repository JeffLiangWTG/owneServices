using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceOnboardMassCollection : ActiveBusinessObjectCollection<GlbDeviceOnboardMass>
	{
		public GlbDeviceOnboardMassCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
