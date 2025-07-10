using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLocationCollection : ActiveBusinessObjectCollection<GlbDeviceLocation>
	{
		public GlbDeviceLocationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
