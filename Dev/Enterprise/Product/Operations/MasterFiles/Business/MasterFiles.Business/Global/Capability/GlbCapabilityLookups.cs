using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityLookups : AutoGlbCapabilityLookups
	{
		public GlbCapabilityLookups(AutoGlbCapability parent)
			: base(parent)
		{
		}

		public GlbStaffCollection AllResources
		{
			get { return Factory.GetCachedValue("GlbCapabilityLookups.AllResources", () => new GlbStaffCollection(Factory)); }
		}

		public CodeDescriptionPairList ScopeCodes
		{
			get { return Factory.GetCachedValue<GlbCapabilityScopeList>(); }
		}
	}
}
