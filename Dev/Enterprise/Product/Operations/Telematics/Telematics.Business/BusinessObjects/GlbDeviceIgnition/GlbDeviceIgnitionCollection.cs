using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceIgnitionCollection : ActiveBusinessObjectCollection<GlbDeviceIgnition>
	{
		public GlbDeviceIgnitionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
