using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public static class SupervisorOverridesHelper
	{
		public static bool IsSupervisorApproved(SupervisorOverrides supervisorOverrides, Logs logsForReporting)
		{
			supervisorOverrides.CreateMessages();

			if (supervisorOverrides.ShouldLogAuthorisedChanges)
			{
				supervisorOverrides.LogAuthorisedActions(logsForReporting);
			}

			if (!GlbStaff.CurrentUser.GS_IsController && supervisorOverrides.SupervisorShouldApproveChanges)
			{
				if (ZFormModaliser.ShowDialogAndDispose(new SupervisorOverridesForm(supervisorOverrides)) == DialogResult.OK)
				{
					supervisorOverrides.LogSupervisorActions(logsForReporting);
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}
}
