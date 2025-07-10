using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class IntegratedTouchSummary : ZUserControl
	{
		public IntegratedTouchSummary()
		{
			InitializeComponent();
#if WINZOR
			touchSummaryControl = TouchSummaryContainer;
#else
			touchSummaryControl = new TouchSummary();
			TouchSummaryContainer.Child = (TouchSummary)touchSummaryControl;
#endif
			touchSummaryControl.BeginLongRefresh += OnControlOnBeginLongRefresh;
			touchSummaryControl.LongRefreshProgress += OnControlOnLongRefreshProgress;
			touchSummaryControl.EndLongRefresh += OnControlOnEndLongRefresh;
			touchSummaryControl.CampaignSelected += OnCampaignSelected;
			touchSummaryControl.InitialCampaignSelected += OnInitialCampaignSelected;
			ClickStatControl.SummaryControlCollapsed = true;
			AddTrackingStatusControl();
			AddOpportunityCreationControl();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (touchSummaryControl != null)
				{
					touchSummaryControl.BeginLongRefresh -= OnControlOnBeginLongRefresh;
					touchSummaryControl.LongRefreshProgress -= OnControlOnLongRefreshProgress;
					touchSummaryControl.EndLongRefresh -= OnControlOnEndLongRefresh;
					touchSummaryControl.CampaignSelected -= OnCampaignSelected;
					touchSummaryControl.InitialCampaignSelected -= OnInitialCampaignSelected;
					if (touchSummaryControl is TouchSummary touchSummaryWpf)
					{
						touchSummaryWpf.DataContext = null;
					}
				}

				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		public void SetDataContext(GlbCompanyCampaign dataSource)
		{
			touchSummaryControl.SetDataContext(dataSource);
			TransitionProgressControl.SetDataContext(dataSource);
		}

		public TouchSummaryViewModel ViewModel { get { return touchSummaryControl.ViewModel; } }

		string GetTouchCode(GlbCompanyCampaign campaign)
		{
			if (campaign == null || campaign.G0_HorizontalId == 0)
			{
				return string.Empty;
			}

			return $"{campaign.G0_HorizontalId}{campaign.G0_VerticalId}";
		}

		void SetTrackingStatusGroupBoxCaption(GlbCompanyCampaign campaign)
		{
			string touchCode = GetTouchCode(campaign);
			if (string.IsNullOrEmpty(touchCode))
			{
				TrackingStatusGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9837A4EC-C2F7-4A9A-A015-1110F017ADA5", "Campaign Summary");
			}
			else
			{
				TrackingStatusGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("5C3A19ED-BD0D-4c62-9A51-C8270DFACDAC", "Campaign Summary ({0})");
				TrackingStatusGroupBox.CaptionResourceString = TrackingStatusGroupBox.CaptionResourceString.Format(touchCode);
			}
			TrackingStatusGroupBox.UpdateCaption();
		}

		void SetOpportunitiesGroupBoxCaption(GlbCompanyCampaign campaign)
		{
			string touchCode = GetTouchCode(campaign);
			if (string.IsNullOrEmpty(touchCode))
			{
				OpportunitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7B2FFEF2-BDC2-4154-A158-A2225DECFB6F", "Linked Opportunities");
			}
			else
			{
				OpportunitiesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("22D88D6D-3D14-4f16-B7C8-53EC6037CEB8", "Linked Opportunities ({0})");
				OpportunitiesGroupBox.CaptionResourceString = OpportunitiesGroupBox.CaptionResourceString.Format(touchCode);
			}
			OpportunitiesGroupBox.UpdateCaption();
		}

		protected void OnCampaignSelected(object sender, CampaignSelectedEventArgs e)
		{
			SetDataBinding(e.GlbCompanyCampaign, "");

			var campaign = e.GlbCompanyCampaign;
			var touchCampaign = campaign != null && campaign.IsMasterCampaign ? campaign?.Horizontals?.FirstOrDefault()?.Campaigns?.FirstOrDefault() : campaign;
			SetTrackingStatusGroupBoxCaption(touchCampaign);
			TrackingStatusControl.SetDataContext(touchCampaign);

			ClickStatControl.SetDataContext(e.GlbCompanyCampaign?.StatModel);
			SetOpportunitiesGroupBoxCaption(e.GlbCompanyCampaign);
			OpportunityCreationControl.SetDataContext(e.GlbCompanyCampaign);
		}

		protected void OnInitialCampaignSelected(object sender, CampaignSelectedEventArgs e)
		{
			SetDataBinding(e.GlbCompanyCampaign, "");

			SetTrackingStatusGroupBoxCaption(e.GlbCompanyCampaign);
			TrackingStatusControl.SetDataContext(e.GlbCompanyCampaign);

			ClickStatControl.SetDataContext(e.GlbCompanyCampaign?.StatModel);
			SetOpportunitiesGroupBoxCaption(null);
			OpportunityCreationControl.SetDataContext(e.GlbCompanyCampaign);
		}

		void OnControlOnBeginLongRefresh(object sender, GlbCompanyCampaign.TransitionProgressEventArgs e)
		{
			BeginLongRefresh?.Invoke(sender, e);
		}

		void OnControlOnLongRefreshProgress(object sender, GlbCompanyCampaign.TransitionProgressEventArgs e)
		{
			LongRefreshProgress?.Invoke(sender, e);
		}

		protected void OnControlOnEndLongRefresh(object sender, EventArgs e)
		{
			TransitionProgressControl.RefreshChart();
			ClickStatControl.RefreshCharts();
			TrackingStatusControl.RefreshCampaignSummary();
			OpportunityCreationControl.RefreshOpportunityCreationChart();
			EndLongRefresh?.Invoke(sender, e);
		}

		#region TrackingStatusControl

		protected TrackingStatusChartUserControl TrackingStatusControl;

		void AddTrackingStatusControl()
		{
			SetupTrackingStatusControl();
			TrackingStatusControl.SuspendLayout();
			TrackingStatusGroupBox.Controls.Add(TrackingStatusControl);
			// 
			// TrackingStatusControl
			// 
			TrackingStatusControl.AllowDrop = true;
			TrackingStatusControl.BackColor = Color.White;
			BindingSource.SetBindingMember(TrackingStatusControl, ".");
			TrackingStatusControl.Dock = DockStyle.Fill;
			TrackingStatusControl.Location = ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			TrackingStatusControl.Name = "TrackingStatusControl";
			TrackingStatusControl.Size = ControlDpiScalingHelper.NewScaledSize(581, 133, true);
			TrackingStatusControl.TabIndex = 0;

			TrackingStatusControl.ResumeLayout(true);
			TrackingStatusControl.PerformLayout();
		}

		protected virtual void SetupTrackingStatusControl()
		{
			TrackingStatusControl = new TrackingStatusChartUserControl();
		}

		#endregion

		protected OpportunityCreationChartUserControl OpportunityCreationControl;

		void AddOpportunityCreationControl()
		{
			if (OpportunityCreationControl != null)
			{
				return;
			}

			SetupOpportunityCreationChartUserControl();
			OpportunitiesGroupBox.Controls.Add(OpportunityCreationControl);

			OpportunityCreationControl.AllowDrop = true;
			OpportunityCreationControl.BackColor = Color.White;
			BindingSource.SetBindingMember(OpportunityCreationControl, "MasterCampaign");
			OpportunityCreationControl.Dock = DockStyle.Fill;
			OpportunityCreationControl.Location = ControlDpiScalingHelper.NewScaledPoint(2, 15);
			OpportunityCreationControl.Name = "OpportunityCreationControl";
			OpportunityCreationControl.Size = ControlDpiScalingHelper.NewScaledSize(380, 133);
			OpportunityCreationControl.TabIndex = 0;
		}

		protected virtual void SetupOpportunityCreationChartUserControl()
		{
			OpportunityCreationControl = new OpportunityCreationChartUserControl();
		}

		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> BeginLongRefresh;
		public event EventHandler<GlbCompanyCampaign.TransitionProgressEventArgs> LongRefreshProgress;
		public event EventHandler EndLongRefresh;
		readonly ITouchSummary touchSummaryControl;
	}
}
