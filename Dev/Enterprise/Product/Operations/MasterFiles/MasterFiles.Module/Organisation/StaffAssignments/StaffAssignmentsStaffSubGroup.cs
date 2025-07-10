using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StaffAssignmentsStaffSubGroup : ModuleFilterSubGroup
	{
		public StaffAssignmentsStaffSubGroup() : base()
		{ }

		public StaffAssignmentsStaffSubGroup(ModuleFilterSubGroup parent) : base(parent)
		{ }

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(OrgStaffAssignments));
			var glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible);
			glbStaffSubQuery.AddToFilter(filter);
			result.AddSubQuery(glbStaffSubQuery, JoinCondition.And);
			return result;
		}
	}
}
