using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbResourceCapabilityPivotCollection : ActiveBusinessObjectCollection<GlbResourceCapabilityPivot>
	{
		public GlbResourceCapabilityPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbResourceCapabilityPivotCollection(GlbStaff staff)
			: base(staff.Factory, staff, new ZQuery(), GlbResourceCapabilityPivotSchema.G5_GS_Resource)
		{
		}
	}
}
