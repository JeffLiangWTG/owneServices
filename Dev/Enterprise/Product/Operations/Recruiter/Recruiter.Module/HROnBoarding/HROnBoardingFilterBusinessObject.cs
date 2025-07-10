using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Module
{
	public class HROnBoardingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
