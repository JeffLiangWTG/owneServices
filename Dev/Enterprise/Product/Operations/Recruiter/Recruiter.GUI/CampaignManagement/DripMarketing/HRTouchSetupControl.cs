using Enterprise.MarketingManager.GUI;

namespace Enterprise.Recruiter.GUI
{
	public class HRTouchSetupControl : TouchSetupControl
	{
		#region SendCampaignsControl overrides

		protected override void SetupSendCampaignControl()
		{
			SendCampaignControl = new HRSendCampaignsControl();
		}

		#endregion

		#region TouchSetupScheduleUserControl overrides

		protected override void SetupTouchSetupScheduleUserControl()
		{
			TouchSetupScheduleUserControl = new TouchScheduleUserControl(true);
		}

		#endregion

		internal SendCampaignsControl InternalSendCampaignControl => SendCampaignControl;
	}
}
