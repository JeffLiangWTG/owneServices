using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module.Testing
{
	public class LearningCentreCampaignModuleForTest : LearningCentreCampaignModule
	{
		public ZController GetNewController(LearningCentreCampaign campaign)
		{
			return base.GetNewController(campaign);
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
