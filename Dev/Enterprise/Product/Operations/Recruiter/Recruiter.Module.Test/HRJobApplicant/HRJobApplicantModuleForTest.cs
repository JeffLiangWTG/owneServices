using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module.Testing
{
	public class HRJobApplicantModuleForTest : HRJobApplicantModule
	{
		public HRJobApplicantModuleForTest()
		{
		}

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
