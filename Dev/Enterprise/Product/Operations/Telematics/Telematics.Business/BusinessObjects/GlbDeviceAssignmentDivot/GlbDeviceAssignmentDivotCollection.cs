using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceAssignmentDivotCollection : ActiveBusinessObjectCollection<GlbDeviceAssignmentDivot>
	{
		public GlbDeviceAssignmentDivotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
