using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityGroupPivotCollection : ActiveBusinessObjectCollection<GlbCapabilityGroupPivot>
	{
		public GlbCapabilityGroupPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbCapabilityGroupPivotCollection(GlbCapability capability)
			: base(capability.Factory, capability, new ZQuery(), GlbCapabilityGroupPivotSchema.GGC_G4_Capability)
		{
		}
	}
}
