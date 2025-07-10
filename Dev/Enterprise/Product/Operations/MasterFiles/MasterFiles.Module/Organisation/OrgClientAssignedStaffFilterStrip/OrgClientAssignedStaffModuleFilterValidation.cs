using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgClientAssignedStaffModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgClientAssignedStaffModuleFilterValidation(OrgClientAssignedStaffModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgClientAssignedStaffModuleFilter parent;

		#region Validate Properties

		public void ValidateClientType()
		{
			ValidateCalculatedProperty(parent.ClientTypeInfo);
		}

		protected void CheckClientType()
		{
			ListValidation.ErrorIfInvalidCode(parent.ClientTypeInfo);

			if (!parent.StaffRole.IsEmpty || !parent.AssignedStaff.IsEmpty || !parent.Department.IsEmpty || !parent.ControllingBranch.IsEmpty)
			{
				MandatoryValidation.CheckEntered(parent.ClientTypeInfo);
			}
		}

		public void ValidateStaffRole()
		{
			ValidateCalculatedProperty(parent.StaffRoleInfo);
		}

		protected void CheckStaffRole()
		{
			ListValidation.ErrorIfInvalidCode(parent.StaffRoleInfo);
		}

		public void ValidateAssignedStaff()
		{
			ValidateCalculatedProperty(parent.AssignedStaffInfo);
		}

		protected void CheckAssignedStaff()
		{
			ListValidation.ErrorIfInvalidCode(parent.AssignedStaffInfo);
		}

		public void ValidateDepartment()
		{
			ValidateCalculatedProperty(parent.DepartmentInfo);
		}

		protected void CheckDepartment()
		{
			ListValidation.ErrorIfInvalidCode(parent.DepartmentInfo);
		}

		public void ValidateControllingBranch()
		{
			ValidateCalculatedProperty(parent.ControllingBranchInfo);
		}

		protected void CheckControllingBranch()
		{
			TypeValidation.CheckValidGuid(parent.ControllingBranchInfo);
		}

		#endregion

		#region Overrides

		public override void ValidateAll()
		{
			ValidateClientType();
			ValidateStaffRole();
			ValidateAssignedStaff();
			ValidateDepartment();
			ValidateControllingBranch();
		}

		#endregion
	}
}
