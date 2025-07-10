namespace Enterprise.Customs.UY.Manifest.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.CompanyCredentialsDetailsUserControl = new CompanyCredentialsDetailsUserControl();
			this.CompanyCredentialsDetailsUserControl.SuspendLayout();
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.CompanyCredentialsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyCredentialsDetailsUserControl, ".");
			this.CompanyCredentialsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 76, true);
			this.CompanyCredentialsDetailsUserControl.Name = "CompanyCredentialsDetailsUserControl";

			this.Controls.Add(this.CompanyCredentialsDetailsUserControl);
			this.CaptionRenderingEnabled = true;
			this.CompanyCredentialsDetailsUserControl.ResumeLayout(true);
			this.CompanyCredentialsDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public CompanyCredentialsDetailsUserControl CompanyCredentialsDetailsUserControl;
	}
}
