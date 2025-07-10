using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module.Testing
{
	public class HRJobApplicationModuleForTest : HRJobApplicationModule
	{
		public HRJobApplicationModuleForTest()
		{
		}

		public IFilterControl NewFilterControl
		{
			get
			{
				return GetNewFilterControl();
			}
		}
	}
}
