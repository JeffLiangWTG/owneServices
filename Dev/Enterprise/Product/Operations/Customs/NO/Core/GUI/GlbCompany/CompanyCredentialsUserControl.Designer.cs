namespace Enterprise.Customs.NO.GUI
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
			this.CompanyCredentialsDetailsUserControl = new Enterprise.Customs.NO.GUI.CompanyCredentialsDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompanyCredentialsDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.GlbCompanyWrapper);
			// 
			// CompanyCredentialsDetailsUserControl
			// 
			this.CompanyCredentialsDetailsUserControl.AllowDrop = true;
			this.CompanyCredentialsDetailsUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.CompanyCredentialsDetailsUserControl, ".");
			this.CompanyCredentialsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.CompanyCredentialsDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 100, true);
			this.CompanyCredentialsDetailsUserControl.Name = "CompanyCredentialsDetailsUserControl";
			this.CompanyCredentialsDetailsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
			this.CompanyCredentialsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 182, true);
			this.CompanyCredentialsDetailsUserControl.TabIndex = 0;
			// 
			// CompanyCredentialsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompanyCredentialsDetailsUserControl);
			this.Name = "CompanyCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 175, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompanyCredentialsDetailsUserControl.ResumeLayout(true);
			this.CompanyCredentialsDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CompanyCredentialsDetailsUserControl CompanyCredentialsDetailsUserControl;
	}
}
