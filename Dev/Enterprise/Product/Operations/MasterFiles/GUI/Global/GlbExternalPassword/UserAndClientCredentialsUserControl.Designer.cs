using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	partial class UserAndClientCredentialsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TextBox_Username = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_Password = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_PasswordConfirmation = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_ClientId = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_ClientSecret = new Enterprise.ZArchitecture.ZTextBox();
			this.Label_UserCredentialPasswordStatus = new Enterprise.ZArchitecture.ZLabel();
			this.Label_ClientCredentialPasswordStatus = new Enterprise.ZArchitecture.ZLabel();
			this.TextBox_UserCredentialPasswordStatusReason = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBox_ClientCredentialPasswordStatusReason = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UserAndClientCredentials);
			// 
			// TextBox_Username
			// 
			this.TextBox_Username.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_Username, "Username");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).Username)));
			this.TextBox_Username.CaptionResourceString = null;
			this.TextBox_Username.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_Username.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 12, true);
			this.TextBox_Username.Name = "TextBox_Username";
			this.TextBox_Username.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 15, true);
			this.TextBox_Username.TabIndex = 0;
			// 
			// TextBox_Password
			// 
			this.TextBox_Password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_Password, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).Password)));
			this.TextBox_Password.CaptionResourceString = null;
			this.TextBox_Password.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_Password.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 38, true);
			this.TextBox_Password.Name = "TextBox_Password";
			this.TextBox_Password.PasswordChar = '*';
			this.TextBox_Password.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 15, true);
			this.TextBox_Password.TabIndex = 1;
			// 
			// TextBox_PasswordConfirmation
			// 
			this.TextBox_PasswordConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_PasswordConfirmation, "PasswordConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).PasswordConfirmation)));
			this.TextBox_PasswordConfirmation.CaptionResourceString = null;
			this.TextBox_PasswordConfirmation.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_PasswordConfirmation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 64, true);
			this.TextBox_PasswordConfirmation.Name = "TextBox_PasswordConfirmation";
			this.TextBox_PasswordConfirmation.PasswordChar = '*';
			this.TextBox_PasswordConfirmation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 15, true);
			this.TextBox_PasswordConfirmation.TabIndex = 2;
			// 
			// TextBox_ClientId
			// 
			this.TextBox_ClientId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_ClientId, "ClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).ClientId)));
			this.TextBox_ClientId.CaptionResourceString = null;
			this.TextBox_ClientId.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_ClientId.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 145, true);
			this.TextBox_ClientId.Name = "TextBox_ClientId";
			this.TextBox_ClientId.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 15, true);
			this.TextBox_ClientId.TabIndex = 3;
			// 
			// TextBox_ClientSecret
			// 
			this.TextBox_ClientSecret.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_ClientSecret, "ClientSecret");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).ClientSecret)));
			this.TextBox_ClientSecret.CaptionResourceString = null;
			this.TextBox_ClientSecret.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_ClientSecret.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 171, true);
			this.TextBox_ClientSecret.Name = "TextBox_ClientSecret";
			this.TextBox_ClientSecret.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 15, true);
			this.TextBox_ClientSecret.TabIndex = 4;
			// 
			// Label_UserCredentialPasswordStatus
			// 
			this.Label_UserCredentialPasswordStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Label_UserCredentialPasswordStatus, "UserCredentialPasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).UserCredentialPasswordStatus)));
			this.Label_UserCredentialPasswordStatus.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label_UserCredentialPasswordStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 36, true);
			this.Label_UserCredentialPasswordStatus.Name = "Label_UserCredentialPasswordStatus";
			this.Label_UserCredentialPasswordStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 23, true);
			this.Label_UserCredentialPasswordStatus.TabIndex = 5;
			// 
			// Label_ClientCredentialPasswordStatus
			// 
			this.Label_ClientCredentialPasswordStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Label_ClientCredentialPasswordStatus, "ClientCredentialPasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).ClientCredentialPasswordStatus)));
			this.Label_ClientCredentialPasswordStatus.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label_ClientCredentialPasswordStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(467, 168, true);
			this.Label_ClientCredentialPasswordStatus.Name = "Label_ClientCredentialPasswordStatus";
			this.Label_ClientCredentialPasswordStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 23, true);
			this.Label_ClientCredentialPasswordStatus.TabIndex = 6;
			// 
			// TextBox_UserCredentialPasswordStatusReason
			// 
			this.TextBox_UserCredentialPasswordStatusReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_UserCredentialPasswordStatusReason, "UserCredentialPasswordStatusReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).UserCredentialPasswordStatusReason)));
			this.TextBox_UserCredentialPasswordStatusReason.CaptionResourceString = null;
			this.TextBox_UserCredentialPasswordStatusReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_UserCredentialPasswordStatusReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 90, true);
			this.TextBox_UserCredentialPasswordStatusReason.Multiline = true;
			this.TextBox_UserCredentialPasswordStatusReason.Name = "TextBox_UserCredentialPasswordStatusReason";
			this.TextBox_UserCredentialPasswordStatusReason.ReadOnly = true;
			this.TextBox_UserCredentialPasswordStatusReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBox_UserCredentialPasswordStatusReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 49, true);
			this.TextBox_UserCredentialPasswordStatusReason.TabIndex = 7;
			this.TextBox_UserCredentialPasswordStatusReason.TabStop = false;
			// 
			// TextBox_ClientCredentialPasswordStatusReason
			// 
			this.TextBox_ClientCredentialPasswordStatusReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBox_ClientCredentialPasswordStatusReason, "ClientCredentialPasswordStatusReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UserAndClientCredentials)(null)).ClientCredentialPasswordStatusReason)));
			this.TextBox_ClientCredentialPasswordStatusReason.CaptionResourceString = null;
			this.TextBox_ClientCredentialPasswordStatusReason.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox_ClientCredentialPasswordStatusReason.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 197, true);
			this.TextBox_ClientCredentialPasswordStatusReason.Multiline = true;
			this.TextBox_ClientCredentialPasswordStatusReason.Name = "TextBox_ClientCredentialPasswordStatusReason";
			this.TextBox_ClientCredentialPasswordStatusReason.ReadOnly = true;
			this.TextBox_ClientCredentialPasswordStatusReason.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBox_ClientCredentialPasswordStatusReason.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 49, true);
			this.TextBox_ClientCredentialPasswordStatusReason.TabIndex = 8;
			this.TextBox_ClientCredentialPasswordStatusReason.TabStop = false;
			// 
			// UserAndClientCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TextBox_ClientCredentialPasswordStatusReason);
			this.Controls.Add(this.TextBox_UserCredentialPasswordStatusReason);
			this.Controls.Add(this.Label_ClientCredentialPasswordStatus);
			this.Controls.Add(this.Label_UserCredentialPasswordStatus);
			this.Controls.Add(this.TextBox_ClientSecret);
			this.Controls.Add(this.TextBox_PasswordConfirmation);
			this.Controls.Add(this.TextBox_Password);
			this.Controls.Add(this.TextBox_Username);
			this.Controls.Add(this.TextBox_ClientId);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "UserAndClientCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZTextBox TextBox_Username;
		private ZTextBox TextBox_Password;
		private ZTextBox TextBox_PasswordConfirmation;
		private ZTextBox TextBox_ClientId;
		private ZTextBox TextBox_ClientSecret;
		private ZLabel Label_UserCredentialPasswordStatus;
		private ZLabel Label_ClientCredentialPasswordStatus;
		private ZTextBox TextBox_UserCredentialPasswordStatusReason;
		private ZTextBox TextBox_ClientCredentialPasswordStatusReason;
	}
}
