using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StaffFormForTest : IDisposable
	{
		public StaffFormForTest(GlbStaff staff)
		{
			fStaff = staff;
			SecurityEventFired = false;
			SaveNewStaffFired = false;
			fStaff.ChangeActualSecurityPermissions += new EventHandler(OnChangeSecurityFired);
			fStaff.SuccessfulSaveNewStaffMember += new EventHandler(OnSaveNewStaffFired);
		}

		readonly GlbStaff fStaff;

		void OnChangeSecurityFired(object sender, EventArgs e)
		{
			SecurityEventFired = true;
		}
		public bool SecurityEventFired;

		void OnSaveNewStaffFired(object sender, EventArgs e)
		{
			SaveNewStaffFired = true;
		}
		public bool SaveNewStaffFired;

		#region Dispose

		public void Dispose()
		{
			fStaff.ChangeActualSecurityPermissions -= new EventHandler(OnChangeSecurityFired);
			fStaff.SuccessfulSaveNewStaffMember -= new EventHandler(OnSaveNewStaffFired);
		}

		#endregion
	}
}
