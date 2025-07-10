using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public static class GlbStaffVisibilityHelper
	{
		public static bool CanShowLeave(GlbStaff staff)
		{
			return staff.IsCurrentUser ||
				   CanEditLeave(staff) ||
				   Env.Security.StaffViewOtherLeave.IsAllowed;
		}

		public static bool CanEditLeave(GlbStaff staff)
		{
			return Env.Security.StaffLeave.IsAllowed ||
				   (Env.Security.StaffOwnLeave.IsAllowed && staff.IsCurrentUser) ||
				   staff.IsCurrentUserLocalAdminForThisStaff ||
				   staff.IsCurrentUserLocalAdminForAtLeastOneGroup && !staff.IsInDatabase;
		}
	}
}
