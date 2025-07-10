using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTyreReportCollection : ActiveBusinessObjectCollection<GlbDeviceTyreReport>
	{
		public GlbDeviceTyreReportCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
