using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgStaffAssignmentsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDepartmentList()
		{
			GlbDepartment systemDepartment = Factory.New<GlbDepartment>();
			systemDepartment.GE_SystemCode = true;
			systemDepartment.GE_Code = "ZUB";
			systemDepartment.GE_Desc = "Zubin Department";

			GlbDepartment inactiveSystemDepartment = Factory.New<GlbDepartment>();
			inactiveSystemDepartment.GE_SystemCode = true;
			inactiveSystemDepartment.GE_Code = "ZIN";
			inactiveSystemDepartment.GE_Desc = "Zubin Inactive Department";
			inactiveSystemDepartment.GE_IsActive = false;

			GlbDepartment nonSystemDepartment = Factory.New<GlbDepartment>();
			nonSystemDepartment.GE_Code = "XXX";
			nonSystemDepartment.GE_Desc = "Triple X Department";

			Factory.Save();

			OrgStaffAssignments staffAssignment = Factory.New<OrgStaffAssignments>();
			Assert(staffAssignment.Lookups.DepartmentCodes.ContainsCode("ALL"));
			Assert(staffAssignment.Lookups.DepartmentCodes.ContainsCode("SEA"));
			Assert(staffAssignment.Lookups.DepartmentCodes.ContainsCode("ZUB"));
			AssertEquals("Zubin Department", staffAssignment.Lookups.DepartmentCodes["ZUB"].Description);
			Assert("Non-system department SHOULD be in list", staffAssignment.Lookups.DepartmentCodes.ContainsCode("XXX"));
			Assert("Inactive department should not be in list", !staffAssignment.Lookups.DepartmentCodes.ContainsCode("ZIN"));
		}
	}
}
