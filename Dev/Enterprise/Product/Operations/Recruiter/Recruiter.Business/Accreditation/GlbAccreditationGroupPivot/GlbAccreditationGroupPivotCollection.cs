using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationGroupPivotCollection : ActiveBusinessObjectCollection<GlbAccreditationGroupPivot>
	{
		public GlbAccreditationGroupPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationGroupPivotCollection(GlbAccreditationGroup accreditation)
			: base(accreditation)
		{
		}

		public GlbAccreditationGroupPivotCollection(GlbAccreditationGroup accreditation, ZQuery query)
			: base(accreditation, query)
		{
		}
	}
}
