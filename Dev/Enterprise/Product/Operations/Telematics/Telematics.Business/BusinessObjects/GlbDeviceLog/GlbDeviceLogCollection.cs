using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLogCollection : ActiveBusinessObjectCollection<GlbDeviceLog>
	{
		public GlbDeviceLogCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
