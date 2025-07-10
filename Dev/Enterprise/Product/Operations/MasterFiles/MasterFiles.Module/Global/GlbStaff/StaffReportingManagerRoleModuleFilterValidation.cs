using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class StaffReportingManagerRoleModuleFilterValidation : ModuleTextFilterValidation
	{
		public StaffReportingManagerRoleModuleFilterValidation(StaffReportingManagerRoleModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly StaffReportingManagerRoleModuleFilter parent;

		public void ValidateReportingRole()
		{
			ValidateCalculatedProperty(parent.ReportingRoleInfo);
		}

		protected void CheckReportingRole()
		{
			ListValidation.ErrorIfInvalidCode(parent.ReportingRoleInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReportingRole();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}
	}
}
