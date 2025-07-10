namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class JobExRateSysConfigForm
	{
		new void InitializeComponent()
		{
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtons.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 576, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 551, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 25, true);
			this.PostingButtons.TabIndex = 6;
			this.PostingButtons.TabStop = true;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.accExRateConfigs.CaptionRenderingEnabled = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, ".");
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 551, true);
			this.accExRateConfigs.TabIndex = 1;
			// 
			// JobExRateSysConfigForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cd00c846-ae13-4bbf-a904-999fdf7a0ece", "Job Billing Exchange Rate Configuration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 600, true);
			this.Controls.Add(this.accExRateConfigs);
			this.Controls.Add(this.PostingButtons);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection);
			this.Name = "JobExRateSysConfigForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtons, 0);
			this.Controls.SetChildIndex(this.accExRateConfigs, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
