using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicationCollection : ActiveBusinessObjectCollection<HRJobApplication>
	{
		public HRJobApplicationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
