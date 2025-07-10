namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganisationSecurityContainerControl
	{

		#region Auto Generated Code

		public Enterprise.ZArchitecture.GUI.ZPanel SecurityPanel;
		Enterprise.ZArchitecture.ZLabel SecurityDeniedLabel;

		void InitializeComponent()
		{
			this.SecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SecurityDeniedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SecurityPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Controls.Add(this.SecurityDeniedLabel);
			this.SecurityPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecurityPanel.Name = "SecurityPanel";
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 24, true);
			this.SecurityPanel.TabIndex = 13;
			// 
			// SecurityDeniedLabel
			// 
			this.SecurityDeniedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrganisationSecurityContainerControl|d1312e7e-65f4-4a7d-9bcf-d204182c33fc", "You do not have security access to modify some of these settings.");
			this.SecurityDeniedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecurityDeniedLabel.ForeColor = System.Drawing.Color.Red;
			this.SecurityDeniedLabel.IsFontBold = true;
			this.SecurityDeniedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecurityDeniedLabel.Name = "SecurityDeniedLabel";
			this.SecurityDeniedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 24, true);
			this.SecurityDeniedLabel.TabIndex = 2;
			this.SecurityDeniedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OrganisationSecurityContainerControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SecurityPanel);
			this.Name = "OrganisationSecurityContainerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SecurityPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}
