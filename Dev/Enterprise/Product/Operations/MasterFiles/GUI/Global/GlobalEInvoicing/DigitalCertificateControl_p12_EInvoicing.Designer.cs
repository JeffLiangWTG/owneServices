using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DigitalCertificateControl_p12_EInvoicing : DigitalCertificateControl_p12
	{
		protected ZArchitecture.GUI.ZButton RegisterButton;

		void InitializeComponent()
		{
			this.RegisterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ViewButton
			// 
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 1, true);
			// 
			// LoadButton
			// 
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 1, true);
			// 
			// UserFeedbackLabel
			// 
			this.UserFeedbackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 6, true);
			// 
			// ClearButton
			// 
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 1, true);
			// 
			// RegisterButton
			// 
			this.RegisterButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c6bab1df-a8f0-4c31-967c-adbc67a3a749", "Register");
			this.RegisterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 1, true);
			this.RegisterButton.Name = "RegisterButton";
			this.RegisterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RegisterButton.TabIndex = 0;
			this.RegisterButton.ToolTipCaption = null;
			this.RegisterButton.Visible = false;
			this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
			// 
			// DigitalCertificateControl_p12_EInvoicing
			// 
			this.Controls.Add(this.RegisterButton);
			this.Name = "DigitalCertificateControl_p12_EInvoicing";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 26, true);
			this.Controls.SetChildIndex(this.UserFeedbackLabel, 0);
			this.Controls.SetChildIndex(this.RegisterButton, 0);
			this.Controls.SetChildIndex(this.ViewButton, 0);
			this.Controls.SetChildIndex(this.LoadButton, 0);
			this.Controls.SetChildIndex(this.ClearButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
