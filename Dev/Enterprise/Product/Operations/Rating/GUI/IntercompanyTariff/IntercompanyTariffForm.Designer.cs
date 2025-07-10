using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class IntercompanyTariffForm
	{
		private ClientInformationUserControl clientInformationUserControl;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private IntercompanyTariffTabControl intercompanyTariffTabControl;
		internal RateEntryFilterStripControl rateEntryFilterStripControl;

		protected new void InitializeComponent()
		{
			this.clientInformationUserControl = new ClientInformationUserControl();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.intercompanyTariffTabControl = new IntercompanyTariffTabControl();
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
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(985);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IntercompanyTariff);
			// 
			// clientInformationUserControl
			// 
			this.clientInformationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientInformationUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.clientInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.clientInformationUserControl.Name = "clientInformationUserControl";
			this.clientInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 41, true);
			this.clientInformationUserControl.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 675, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// intercompanyTariffTabControl
			// 
			this.intercompanyTariffTabControl.AllowDrop = true;
			this.intercompanyTariffTabControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.intercompanyTariffTabControl, ".");
			this.intercompanyTariffTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.intercompanyTariffTabControl.Name = "IntercompanyTariffTabControl";
			this.intercompanyTariffTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 537, true);
			this.intercompanyTariffTabControl.TabIndex = 1;
			// 
			// rateEntryFilterStripControl
			// 
			this.rateEntryFilterStripControl.AllowDrop = true;
			this.rateEntryFilterStripControl.CaptionRenderingEnabled = true;
			this.rateEntryFilterStripControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.rateEntryFilterStripControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.rateEntryFilterStripControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.rateEntryFilterStripControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(314, 0);
			this.rateEntryFilterStripControl.Name = "rateEntryFilterStripControl";
			this.rateEntryFilterStripControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 136);
			this.rateEntryFilterStripControl.TabIndex = 9;
			// 
			// IntercompanyTariffForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Controls.Add(this.rateEntryFilterStripControl);
			this.Controls.Add(this.intercompanyTariffTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.clientInformationUserControl);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(IntercompanyTariff);
			this.DataSourceTypeName = "Enterprise.Rating.Business.IntercompanyTariff";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "IntercompanyTariffForm";
			this.Text = "IntercompanyTariffForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.clientInformationUserControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.intercompanyTariffTabControl, 0);
			this.Controls.SetChildIndex(this.rateEntryFilterStripControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
