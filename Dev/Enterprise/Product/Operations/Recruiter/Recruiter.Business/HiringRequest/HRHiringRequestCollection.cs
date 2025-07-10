using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestCollection : ActiveBusinessObjectCollection<HRHiringRequest>
	{
		public HRHiringRequestCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
