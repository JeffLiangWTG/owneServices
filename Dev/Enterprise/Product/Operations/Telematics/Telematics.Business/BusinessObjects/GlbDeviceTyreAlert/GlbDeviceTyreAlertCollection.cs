using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTyreAlertCollection : ActiveBusinessObjectCollection<GlbDeviceTyreAlert>
	{
		public GlbDeviceTyreAlertCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
