using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public sealed partial class GlobalTariffsForm
	{
		private Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl1;
		private GlobalTariffsHeaderUserControl GlobalTariffsHeaderUserControl;
		private GlobalTariffTabControl GlobalTariffsTabControl;
		internal RateEntryFilterStripControl rateEntryFilterStripControl;

		new void InitializeComponent()
		{
			this.oPostingButtonsUserControl1 = new Core.Forms.ZPostingButtonsUserControl();
			this.GlobalTariffsHeaderUserControl = new GlobalTariffsHeaderUserControl();
			this.GlobalTariffsTabControl = new GlobalTariffTabControl();
			this.rateEntryFilterStripControl = new RateEntryFilterStripControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 701, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(492);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(493);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CompanyTariff);
			// 
			// oPostingButtonsUserControl1
			// 
			this.oPostingButtonsUserControl1.AllowDrop = true;
			this.oPostingButtonsUserControl1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.oPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 675, true);
			this.oPostingButtonsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl1.Name = "oPostingButtonsUserControl1";
			this.oPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl1.TabIndex = 2;
			// 
			// GlobalTariffsHeaderUserControl
			// 
			this.GlobalTariffsHeaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GlobalTariffsHeaderUserControl, ".");
			this.GlobalTariffsHeaderUserControl.CompanyTariffDiscountVisible = true;
			this.GlobalTariffsHeaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlobalTariffsHeaderUserControl.Name = "GlobalTariffsHeaderUserControl";
			this.GlobalTariffsHeaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 103, true);
			this.GlobalTariffsHeaderUserControl.TabIndex = 0;
			// 
			// GlobalTariffsTabControl
			// 
			this.GlobalTariffsTabControl.AllowDrop = true;
			this.GlobalTariffsTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.GlobalTariffsTabControl, ".");
			this.GlobalTariffsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.GlobalTariffsTabControl.Name = "GlobalTariffsTabControl";
			this.GlobalTariffsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 537, true);
			this.GlobalTariffsTabControl.TabIndex = 1;
			// 
			// rateEntryFilterStripControl1
			// 
			this.rateEntryFilterStripControl.AllowDrop = true;
			this.rateEntryFilterStripControl.CaptionRenderingEnabled = true;
			this.rateEntryFilterStripControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.rateEntryFilterStripControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 0);
			this.rateEntryFilterStripControl.Name = "rateEntryFilterStripControl";
			this.rateEntryFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136);
			this.rateEntryFilterStripControl.TabIndex = 10;
			// 
			// GlobalTariffsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Controls.Add(this.rateEntryFilterStripControl);
			this.Controls.Add(this.GlobalTariffsTabControl);
			this.Controls.Add(this.GlobalTariffsHeaderUserControl);
			this.Controls.Add(this.oPostingButtonsUserControl1);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(CompanyTariff);
			this.DataSourceTypeName = "Enterprise.Rating.Business.CompanyTariff";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "GlobalTariffsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "GlobalTariffsForm";
			this.Controls.SetChildIndex(this.oPostingButtonsUserControl1, 0);
			this.Controls.SetChildIndex(this.GlobalTariffsHeaderUserControl, 0);
			this.Controls.SetChildIndex(this.GlobalTariffsTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.rateEntryFilterStripControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
