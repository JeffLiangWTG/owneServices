using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class HRJobRoleCollection : BusinessObjectCollection<HRJobRole>
	{
		public HRJobRoleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public HRJobRoleCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
