using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingCollection : ActiveBusinessObjectCollection<HROnBoarding>
	{
		public HROnBoardingCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
