using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceCombinationReportCollection : ActiveBusinessObjectCollection<GlbDeviceCombinationReport>
	{
		public GlbDeviceCombinationReportCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
