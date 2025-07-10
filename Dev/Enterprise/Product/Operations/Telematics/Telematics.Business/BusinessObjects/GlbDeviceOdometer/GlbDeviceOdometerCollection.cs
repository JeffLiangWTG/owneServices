using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceOdometerCollection : ActiveBusinessObjectCollection<GlbDeviceOdometer>
	{
		public GlbDeviceOdometerCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
