using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ModuleTextFilter))]
	sealed class TestOrgClientAssignedModuleFilterValidation : ModuleTextFilterTest
	{
		public void TestClientTypeValidation()
		{
			ModuleFilter.Validation.ValidateClientType();
			AssertNoErrors("Precondition", ModuleFilter.ClientTypeInfo);

			ModuleFilter.ClientType = "BLAH";
			ModuleFilter.Validation.ValidateClientType();
			AssertHasErrors("Error for invalid code", moduleFilter.ClientTypeInfo);

			ModuleFilter.ClientType = "";
			ModuleFilter.StaffRole = "XX";
			AssertHasErrors("Error if not entered", moduleFilter.ClientTypeInfo);

			ModuleFilter.StaffRole = "";
			ModuleFilter.AssignedStaff = "XX";
			AssertHasErrors("Error if not entered", moduleFilter.ClientTypeInfo);

			ModuleFilter.AssignedStaff = "";
			ModuleFilter.Department = "XX";
			AssertHasErrors("Error if not entered", moduleFilter.ClientTypeInfo);

			ModuleFilter.Department = "";
			ModuleFilter.ControllingBranch = Factory.New<GlbBranch>().PK;
			AssertHasErrors("Error if not entered", moduleFilter.ClientTypeInfo);

			ModuleFilter.ClientType = "CNE";
			ModuleFilter.ControllingBranch = ZGuid.Empty;
			AssertNoErrors("Valid code", moduleFilter.ClientTypeInfo);
		}

		public void TestAssignedStaffValidation()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Arthur Dent";
			staff.GS_Code = "ARD";
			staff.GS_LoginName = "Arthur.Dent";

			Factory.Save();

			ModuleFilter.Validation.ValidateAssignedStaff();
			AssertNoErrors("Precondition", ModuleFilter.AssignedStaffInfo);

			ModuleFilter.AssignedStaff = "XXX";
			ModuleFilter.Validation.ValidateAssignedStaff();
			AssertHasErrors("Error for invalid code", ModuleFilter.AssignedStaffInfo);

			ModuleFilter.AssignedStaff = "ARD";
			ModuleFilter.Validation.ValidateAssignedStaff();
			AssertNoErrors("Valid code", ModuleFilter.AssignedStaffInfo);
		}

		public void TestStaffRoleValidation()
		{
			ModuleFilter.Validation.ValidateStaffRole();
			AssertNoErrors("Precondition", ModuleFilter.StaffRoleInfo);

			ModuleFilter.StaffRole = "XXX";
			ModuleFilter.Validation.ValidateStaffRole();
			AssertHasErrors("Invalid code", ModuleFilter.StaffRoleInfo);

			ModuleFilter.StaffRole = "SAL";
			ModuleFilter.Validation.ValidateStaffRole();
			AssertNoErrors("Valid code", ModuleFilter.StaffRoleInfo);
		}

		public void TestDepartmentValidation()
		{
			ModuleFilter.Validation.ValidateDepartment();
			AssertNoErrors("Precondition", ModuleFilter.DepartmentInfo);

			ModuleFilter.Department = "XXX";
			ModuleFilter.Validation.ValidateDepartment();
			AssertHasErrors("Invalid code", ModuleFilter.DepartmentInfo);

			ModuleFilter.Department = "AIR";
			ModuleFilter.Validation.ValidateDepartment();
			AssertNoErrors("Valid code", ModuleFilter.DepartmentInfo);
		}

		public void TestControllingBranchValidation()
		{
			ModuleFilter.ControllingBranch = ZGuid.Invalid;
			ModuleFilter.Validation.ValidateControllingBranch();
			AssertHasErrors("Invalid branch", ModuleFilter.ControllingBranchInfo);

			ModuleFilter.ControllingBranch = Factory.New<GlbBranch>().PK;
			ModuleFilter.Validation.ValidateControllingBranch();
			AssertNoErrors("Valid branch", ModuleFilter.ControllingBranchInfo);
		}

		#region Implementation

		OrgClientAssignedStaffModuleFilter ModuleFilter
		{
			get
			{
				if (moduleFilter == null)
				{
					moduleFilter = new OrgClientAssignedStaffModuleFilter("Test Filter");
				}

				return moduleFilter;
			}
		}
		OrgClientAssignedStaffModuleFilter moduleFilter;

		#endregion
	}
}
