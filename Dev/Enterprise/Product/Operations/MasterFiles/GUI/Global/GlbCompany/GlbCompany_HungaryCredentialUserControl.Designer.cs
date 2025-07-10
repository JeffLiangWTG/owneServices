using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbCompany_HungaryCredentialUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		private ZTextBox TextBox_Login;
		private ZTextBox TextBox_ReplacementKey;
		private ZTextBox TextBox_PasswordHash;
		private ZTextBox TextBox_SignatureKey;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TextBox_Login = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_ReplacementKey = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_PasswordHash = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_SignatureKey = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI);
			// 
			// TextBox_Login
			// 
			this.TextBox_Login.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_Login, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI)(null)).Login)));
			this.TextBox_Login.CaptionResourceString = null;
			this.TextBox_Login.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_Login.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 12, true);
			this.TextBox_Login.Name = "TextBox_Login";
			this.TextBox_Login.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 20, true);
			this.TextBox_Login.TabIndex = 0;
			// 
			// TextBox_ReplacementKey
			// 
			this.TextBox_ReplacementKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_ReplacementKey, "ReplacementKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI)(null)).ReplacementKey)));
			this.TextBox_ReplacementKey.CaptionResourceString = null;
			this.TextBox_ReplacementKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_ReplacementKey.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 90, true);
			this.TextBox_ReplacementKey.Name = "TextBox_ReplacementKey";
			this.TextBox_ReplacementKey.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 20, true);
			this.TextBox_ReplacementKey.TabIndex = 3;
			this.TextBox_ReplacementKey.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
			// 
			// TextBox_PasswordHash
			// 
			this.TextBox_PasswordHash.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_PasswordHash, "PasswordHash");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI)(null)).PasswordHash)));
			this.TextBox_PasswordHash.CaptionResourceString = null;
			this.TextBox_PasswordHash.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_PasswordHash.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 38, true);
			this.TextBox_PasswordHash.Name = "TextBox_PasswordHash";
			this.TextBox_PasswordHash.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 20, true);
			this.TextBox_PasswordHash.TabIndex = 1;
			this.TextBox_PasswordHash.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
			// 
			// TextBox_SignatureKey
			// 
			this.TextBox_SignatureKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_SignatureKey, "SignatureKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbCompanyExternalPasswordHUI)(null)).SignatureKey)));
			this.TextBox_SignatureKey.CaptionResourceString = null;
			this.TextBox_SignatureKey.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_SignatureKey.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 64, true);
			this.TextBox_SignatureKey.Name = "TextBox_SignatureKey";
			this.TextBox_SignatureKey.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 20, true);
			this.TextBox_SignatureKey.TabIndex = 2;
			this.TextBox_SignatureKey.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
			// 
			// GlbCompany_HungaryCredentialUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TextBox_SignatureKey);
			this.Controls.Add(this.TextBox_PasswordHash);
			this.Controls.Add(this.TextBox_Login);
			this.Controls.Add(this.TextBox_ReplacementKey);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "GlbCompany_HungaryCredentialUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 131, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
