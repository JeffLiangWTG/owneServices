using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public class HRGlbCompanyCampaignForm : GlbCompanyCampaignForm
	{
		public HRGlbCompanyCampaignForm(HRGlbCompanyCampaign campaign)
			: base(campaign)
		{
		}

		public HRGlbCompanyCampaignForm(HRGlbCompanyCampaign campaign, bool shouldShowSendingTab)
			: base(campaign, shouldShowSendingTab)
		{
		}

		new HRGlbCompanyCampaign DataSource
		{
			get { return (HRGlbCompanyCampaign)base.DataSource; }
		}

		protected override void ButtonMasterCampaignOnClick(object sender, EventArgs eventArgs)
		{
			var master = DataSource.MasterCampaign;
			if (master != null)
			{
				ZControllerFactory.Create(ControllerIDs.HRGlbCompanyCampaign).ShowEditForm(master);
			}
		}

		#region TouchSetupControl Override

		protected override void SetupTouchSetupControl()
		{
			TouchSetupControl = new HRTouchSetupControl();
		}
		internal TouchSetupControl InternalTouchSetupControl => TouchSetupControl;

		#endregion

		#region SendCampaignControl Override

		protected override void SetupSendCampaignControl()
		{
			SendCampaignControl = new HRSendCampaignsControl();
		}
		internal SendCampaignsControl InternalSendCampaignControl => SendCampaignControl;

		#endregion

		#region EmailContentControl Override

		protected override void SetupEmailContentControl()
		{
			EmailContentControl = new HRContentDesignerControl();
		}

		#endregion

		#region TrackingStatusControl Override

		protected override void SetupTrackingStatusControl()
		{
			TrackingStatusControl = new HRTrackingStatusChartUserControl();
		}
		internal TrackingStatusChartUserControl InternalTrackingStatusControl => TrackingStatusControl;

		#endregion

		#region TouchSummary Override

		protected override void SetupTouchSummary()
		{
			TouchSummary = new HRIntegratedTouchSummary
			{
				Dock = DockStyle.Fill
			};
		}
		internal IntegratedTouchSummary InternalTouchSummary => TouchSummary;

		#endregion

		internal void InternalTouchesTabPageOnBindingOrFirstShown(object sender, EventArgs eventArgs) => TouchesTabPageOnBindingOrFirstShown(sender, eventArgs);
	}
}
