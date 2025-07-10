using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class StaffManagerTestHelper
	{
		public static void SetupBasicRoles()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ DefaultStaffReportingRoles.Codes.DirectManager, DefaultStaffReportingRoles.Descriptions.DirectManager, true, false, false },
				{ "HRM", (NoResString)"Human Resources Manager", true, false, true },
				{ "PRM", (NoResString)"Payroll Manager", true, false, false },
				{ "TRM", (NoResString)"Disabled Manager", false, false, false }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
		}

		public static GlbStaffManager AddManager(GlbStaff staff, GlbStaff manager, ZString managerType)
		{
			return AddManager(staff, manager, managerType, ZDateTime.Today);
		}

		public static GlbStaffManager AddManager(GlbStaff staff, GlbStaff manager, ZString managerType, ZDateTime effectiveDate)
		{
			return AddManager(staff, manager, managerType, effectiveDate, ZDateTime.Empty);
		}

		public static GlbStaffManager AddManager(GlbStaff staff, GlbStaff manager, ZString managerType, ZDateTime effectiveDate, ZDateTime endDate)
		{
			var managerRole = staff.Factory.New<GlbStaffManager>();
			managerRole.GSM_GS_Staff = staff.PK;
			managerRole.GSM_GS_Manager = manager.PK;
			managerRole.GSM_EffectiveDate = effectiveDate;
			managerRole.GSM_EndDate = endDate;
			managerRole.GSM_ManagerType = managerType;
			return managerRole;
		}
	}
}
