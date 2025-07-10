using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module.Testing
{
	public class HRJobOpeningsModuleForTest : HRJobOpeningsModule
	{
		public IFilterControl NewFilterControl
		{
			get
			{
				return GetNewFilterControl();
			}
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get
			{
				return GetNewGridCollection();
			}
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get
			{
				return GetNewFilterBusinessObject();
			}
		}
	}
}
