using System;
using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class StaffAssignmentPersonAndRoleModuleFilterValidation : ModuleTextFilterValidation
	{
		public StaffAssignmentPersonAndRoleModuleFilterValidation(StaffAssignmentPersonAndRoleModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly StaffAssignmentPersonAndRoleModuleFilter parent;

		public void ValidateStaffAssignmentPerson()
		{
			ValidateCalculatedProperty(parent.StaffAssignmentPersonInfo);
		}

		protected void CheckStaffAssignmentPerson()
		{
			ListValidation.ErrorIfInvalidCode(parent.StaffAssignmentPersonInfo);
		}

		public void ValidateStaffAssignmentRole()
		{
			ValidateCalculatedProperty(parent.StaffAssignmentRoleInfo);
		}

		protected void CheckStaffAssignmentRole()
		{
			ListValidation.ErrorIfInvalidCode(parent.StaffAssignmentRoleInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateStaffAssignmentPerson();
			ValidateStaffAssignmentRole();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
	}
}
