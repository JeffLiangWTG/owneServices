using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.GUI.CampaignManagement
{
	class HRCampaignFilterStrip : CampaignFilterStrip
	{
		#region Filter Controls

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is StaffSecurityModuleFilter)
			{
				var securityControl = new StaffSecurityFilterControl();
				PreferredHeight = securityControl.Height + ControlDpiScalingHelper.OnePixel;

				result = new Control[] { securityControl };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		internal Control[] InternalGetCurrentFilterControls(ModuleFilter currentModuleFilter) => GetCurrentFilterControls(currentModuleFilter);

		#endregion

	}
}
