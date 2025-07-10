using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceExternalVoltageCollection : ActiveBusinessObjectCollection<GlbDeviceExternalVoltage>
	{
		public GlbDeviceExternalVoltageCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
