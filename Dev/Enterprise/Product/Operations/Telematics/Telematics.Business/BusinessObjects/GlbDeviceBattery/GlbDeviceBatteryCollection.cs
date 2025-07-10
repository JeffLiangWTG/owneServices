using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceBatteryCollection : ActiveBusinessObjectCollection<GlbDeviceBattery>
	{
		public GlbDeviceBatteryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
