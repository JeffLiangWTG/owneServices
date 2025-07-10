
namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LicensingCertificateUserControl = new Enterprise.Customs.TW.Manifest.GUI.LicensingCertificateUserControl();
			this.ForwarderCertificateUserControl = new Enterprise.Customs.TW.Manifest.GUI.ForwarderCertificateUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicensingCertificateUserControl.SuspendLayout();
			this.ForwarderCertificateUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.TWGlbCompanyWrapper);
			// 
			// LicensingCertificateUserControl
			// 
			this.LicensingCertificateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicensingCertificateUserControl, "."); 
			this.LicensingCertificateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 0, true);
			this.LicensingCertificateUserControl.Name = "LicensingCertificateUserControl";
			this.LicensingCertificateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 278, true);
			this.LicensingCertificateUserControl.TabIndex = 1;
			// 
			// ForwarderCertificateUserControl
			// 
			this.ForwarderCertificateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderCertificateUserControl, ".");
			this.ForwarderCertificateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ForwarderCertificateUserControl.Name = "ForwarderCertificateUserControl";
			this.ForwarderCertificateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 278, true);
			this.ForwarderCertificateUserControl.TabIndex = 0;
			// 
			// CompanyCredentialsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ForwarderCertificateUserControl);
			this.Controls.Add(this.LicensingCertificateUserControl);
			this.Name = "CompanyCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 278, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicensingCertificateUserControl.ResumeLayout(true);
			this.LicensingCertificateUserControl.PerformLayout();
			this.ForwarderCertificateUserControl.ResumeLayout(true);
			this.ForwarderCertificateUserControl.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ForwarderCertificateUserControl ForwarderCertificateUserControl;
		internal LicensingCertificateUserControl LicensingCertificateUserControl;
	}
}
