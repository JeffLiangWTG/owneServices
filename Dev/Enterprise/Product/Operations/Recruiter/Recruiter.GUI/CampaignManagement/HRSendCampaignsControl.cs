using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public class HRSendCampaignsControl : SendCampaignsControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		protected override void AddFilterGrid(GlbCompanyCampaign campaign)
		{
			FilterGridAdded = true;

			FilterItemModule = (HRGlbCompanyCampaignContactModule)ZModuleFactory.Instance.Create(ModuleIDs.HRGlbCompanyCampaignContact);
			((IGlbCompanyCampaignContactModule)FilterItemModule).Campaign = campaign;
			((HRGlbCompanyCampaignContactModule)FilterItemModule).FireOnPerformSearch += SendCampaignsControl_FireOnPerformSearch;

			FilterStripControl = (HRGlbCompanyCampaignContactFilterControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 30);
			FilterStripControl.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(SendEmailMainPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(SendEmailMainPanel.Height) - ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ContactLabel.Height) - 30);
			FilterStripControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
			FilterStripControl.FireValidationRequest += FilterStripControl_FireValidationRequest;
			SetIsDripMarketingModeOnFilterStripControl(IsDripMarketingMode);
			SendEmailMainPanel.Controls.Add(FilterStripControl);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (FilterItemModule != null)
			{
				FilterItemModule.Dispose();
				FilterItemModule = null;
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
