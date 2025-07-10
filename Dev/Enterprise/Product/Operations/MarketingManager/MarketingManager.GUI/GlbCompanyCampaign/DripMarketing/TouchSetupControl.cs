using System.Linq;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TouchSetupControl : ZUserControl
	{
		public TouchSetupControl()
		{
			InitializeComponent();
			AddSendCampaignControl();
			AddTouchSetupScheduleUserControl();
		}

		#region DataBinding

		protected new GlbCompanyCampaign DataSource
		{
			get { return (GlbCompanyCampaign)base.DataSource; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				UpdateBindingForSelectedPivotChange();
			}
		}

		#endregion

		#region TransitionRulesGrid

		internal ZGrid TransitionRulesGrid
		{
			get { return transitionRulesGrid; }
		}

		void TouchSetupTransitionRulesGrid_AfterBind(object sender, System.EventArgs e)
		{
			transitionRulesGrid.ListManager.PositionChanged += delegate
			{ UpdateBindingForSelectedPivotChange(); };
			transitionRulesGrid.Validated += delegate
			{ SetSources(); };
			UpdateBindingForSelectedPivotChange();
		}

		#endregion

		#region SendCampaignsControl

		protected internal SendCampaignsControl SendCampaignControl;

		void AddSendCampaignControl()
		{
			SetupSendCampaignControl();
			SendCampaignControl.SuspendLayout();
			transistionRulesGroupBox.Controls.Add(SendCampaignControl);

			// 
			// SendCampaignsControl
			// 
			SendCampaignControl.AllowDrop = true;
			BindingSource.SetBindingMember(SendCampaignControl, ".");
			SendCampaignControl.Dock = System.Windows.Forms.DockStyle.Fill;
			SendCampaignControl.IsDripMarketingMode = true;
			SendCampaignControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15);
			SendCampaignControl.Name = "SendCampaignsControl";
			SendCampaignControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 452);
			SendCampaignControl.TabIndex = 0;
			SendCampaignControl.ResumeLayout(true);
			SendCampaignControl.PerformLayout();
		}

		protected virtual void SetupSendCampaignControl()
		{
			SendCampaignControl = new SendCampaignsControl();
		}

		void UpdateBindingForSelectedPivotChange()
		{
			var rule = transitionRulesGrid.ListManager.GetCurrent() as GlbCompanyCampaignDripMarketing;
			var filter = rule != null ? rule.FilterRule : null;

			if (filter != null)
			{
				using (filter.SuspendSettingHasChanges())
				{
					SendCampaignControl.SetReadOnly(false);
					SetSources(rule);

					if (SendCampaignControl.FilterStripControl != null)
					{
						SendCampaignControl.FilterStripControl.IsLoadingForLayoutActivation = true;
					}

					SendCampaignControl.SetFilteredGridFilter(filter);

					if (SendCampaignControl.FilterStripControl != null)
					{
						SendCampaignControl.FilterStripControl.IsLoadingForLayoutActivation = false;
					}
				}
			}
			else
			{
				SendCampaignControl.SetReadOnly(true);
			}
		}

		internal void SetSources()
		{
			SetSources(transitionRulesGrid?.ListManager?.GetCurrent() as GlbCompanyCampaignDripMarketing);
		}

		void SetSources(GlbCompanyCampaignDripMarketing rule)
		{
			if (DataSource != null)
			{
				DataSource.TouchSourceCampaignPKs = rule?.ParentHorizontalsTouches != null ? rule.ParentHorizontalsTouches.Select(t => t.PK).ToArray() : Enumerable.Empty<ZGuid>().ToArray();
			}
		}

		#endregion

		#region TouchSetupScheduleUserControl

		public TouchScheduleUserControl TouchSetupScheduleUserControl;

		void AddTouchSetupScheduleUserControl()
		{
			SetupTouchSetupScheduleUserControl();
			TouchSetupScheduleUserControl.SuspendLayout();
			// 
			// touchSetupScheduleUserControl
			// 
			TouchSetupScheduleUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(TouchSetupScheduleUserControl, "SendSettings");
			TouchSetupScheduleUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			TouchSetupScheduleUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0);
			TouchSetupScheduleUserControl.Name = "touchSetupScheduleUserControl";
			TouchSetupScheduleUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 599);
			TouchSetupScheduleUserControl.TabIndex = 5;
			mainSplitContainer.Panel2.Controls.Add(TouchSetupScheduleUserControl);
			TouchSetupScheduleUserControl.ResumeLayout(true);
			TouchSetupScheduleUserControl.PerformLayout();
		}

		protected virtual void SetupTouchSetupScheduleUserControl()
		{
			TouchSetupScheduleUserControl = new TouchScheduleUserControl();
		}

		#endregion
	}
}
