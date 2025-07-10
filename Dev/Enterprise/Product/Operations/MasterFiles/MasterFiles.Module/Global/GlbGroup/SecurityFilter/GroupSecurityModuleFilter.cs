using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GroupSecurityModuleFilter : StaffSecurityModuleFilter
	{
		public GroupSecurityModuleFilter(ZString description, GlbBranchCollection branchList, GlbDepartmentCollection departmentList)
			: base(description, ModuleIDs.GlbBranch, GlbStaffSchema.GS_GB_HomeBranch, branchList, GlbStaffSchema.GS_GE_HomeDepartment, departmentList)
		{
			// The ModuleID and 2x SchemaColumns passed to the base are never used as we build our query ourselves.
			// But there is no base constructor that accepts just what we need.
		}

		#region Query

		// Not used as the GlbGroupModule.LoadCollection() method does the filtering using business logic rather than DB Queries
		protected override ZQuery GetQuery()
		{
			ZQuery result = new ZQuery();
			return result;
		}

		#endregion
	}
}
