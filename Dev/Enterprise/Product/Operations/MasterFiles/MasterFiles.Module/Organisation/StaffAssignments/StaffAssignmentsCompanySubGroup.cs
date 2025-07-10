using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StaffAssignmentsCompanySubGroup : ModuleFilterSubGroup
	{
		public StaffAssignmentsCompanySubGroup() : base()
		{ }

		public StaffAssignmentsCompanySubGroup(ModuleFilterSubGroup parent) : base(parent)
		{ }

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(OrgStaffAssignments));
			var glbCompanySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK, OrgStaffAssignmentsSchema.O8_GC);
			glbCompanySubQuery.AddToFilter(filter);
			result.AddSubQuery(glbCompanySubQuery, JoinCondition.And);

			return result;
		}
	}
}
