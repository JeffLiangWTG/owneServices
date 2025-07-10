using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StaffAssignmentsFilterBusinessObject : FilterStripBusinessObject
	{
		public StaffAssignmentsFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var roleFilter = filters.AddTextFilter("Role", OrgStaffAssignmentsSchema.O8_Role, Env.Registry.OrgStaffMemberAssignmentRoles);
			roleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StaffAssignmentsFilter|Role", "Role");
			roleFilter.Visibility = FilterVisibility.AlwaysVisible;

			var departmentFilter = filters.AddTextFilter("Department", OrgStaffAssignmentsSchema.O8_Department, OrgStaffAssignmentsLookupsImplementer.Get(Factory).DepartmentCodes);
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StaffAssignmentsFilter|Department", "Department");
			departmentFilter.Visibility = FilterVisibility.AlwaysVisible;

			var staffFilter = filters.AddGuidFilter(
				"Staff",
				ModuleIDs.GlbStaff,
				GlbStaffSchema.PK,
				new GlbStaffCollection(Factory));
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StaffAssignmentsFilter|Staff", "Staff");
			staffFilter.Visibility = FilterVisibility.AlwaysVisible;
			staffFilter.SubGroup = new StaffAssignmentsStaffSubGroup();

			var companyFilter = filters.AddGuidFilter(
				"Company",
				ModuleIDs.GlbCompany,
				GlbCompanySchema.PK,
				new GlbCompanyCollection(Factory));
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StaffAssignmentsFilter|Company", "Company");
			companyFilter.Visibility = FilterVisibility.AlwaysVisible;
			companyFilter.SubGroup = new StaffAssignmentsCompanySubGroup();

			if (!Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed)
			{
				companyFilter.ReadOnly = true;
				companyFilter.Property = GlbCompany.CurrentCompany.PK;
			}

			return filters;
		}
	}
}
