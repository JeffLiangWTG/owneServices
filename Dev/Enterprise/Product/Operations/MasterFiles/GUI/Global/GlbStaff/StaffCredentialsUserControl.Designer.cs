using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class StaffCredentialsUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CredentialsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffWrapper);
			// 
			// CredentialsHintLabel
			// 
			this.CredentialsHintLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffForm|673d4aa7-8da0-48fd-a69c-da6157acc17c", "", "You do not have sufficient privilege to access this page. Contact your system administrator for more details.");
			this.CredentialsHintLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CredentialsHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CredentialsHintLabel.ForeColor = System.Drawing.Color.Red;
			this.CredentialsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsHintLabel.Name = "CredentialsHintLabel";
			this.CredentialsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 23, true);
			this.CredentialsHintLabel.TabIndex = 0;
			this.CredentialsHintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StaffCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsHintLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		public Enterprise.ZArchitecture.ZLabel CredentialsHintLabel;

		#endregion
	}
}
