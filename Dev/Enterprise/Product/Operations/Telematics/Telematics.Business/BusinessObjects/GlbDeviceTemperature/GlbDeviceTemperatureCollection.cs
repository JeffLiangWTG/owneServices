using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTemperatureCollection : ActiveBusinessObjectCollection<GlbDeviceTemperature>
	{
		public GlbDeviceTemperatureCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
