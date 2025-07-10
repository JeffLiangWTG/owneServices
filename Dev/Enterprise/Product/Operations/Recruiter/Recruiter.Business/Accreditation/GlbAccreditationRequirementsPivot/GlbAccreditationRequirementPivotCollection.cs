using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationRequirementPivotCollection : ActiveBusinessObjectCollection<GlbAccreditationRequirementPivot>
	{
		public GlbAccreditationRequirementPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlbAccreditationRequirementPivotCollection(GlbAccreditation accreditation)
			: base(accreditation)
		{
		}

		public GlbAccreditationRequirementPivotCollection(GlbAccreditation accreditation, ZQuery query)
			: base(accreditation, query)
		{
		}
	}
}
