using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Module
{
	public class HRHiringRequestFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
