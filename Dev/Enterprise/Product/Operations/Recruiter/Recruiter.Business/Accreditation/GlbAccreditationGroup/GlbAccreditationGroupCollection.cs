using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupCollection : ActiveBusinessObjectCollection<GlbAccreditationGroup>
	{
		public GlbAccreditationGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationGroupCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
