using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceCollection : ActiveBusinessObjectCollection<GlbDevice>
	{
		public GlbDeviceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
