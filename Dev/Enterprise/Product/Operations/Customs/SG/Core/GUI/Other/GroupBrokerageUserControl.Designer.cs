namespace Enterprise.Customs.SG.V4.GUI
{
	partial class GroupBrokerageUserControl
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
			this.AccessGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccessPasswordGP_PasswordStatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccessPasswordStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccessPasswordUserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessPasswordNextDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessPasswordCurrentDecryptedPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessVisibleOnlyToDeveloperTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AccessGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper);
			// 
			// AccessGroupBox
			// 
			this.AccessGroupBox.Controls.Add(this.AccessPasswordGP_PasswordStatusDescriptionLabel);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordStatusLabel);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordUserIDTextBox);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordNextDecryptedPasswordTextBox);
			this.AccessGroupBox.Controls.Add(this.AccessPasswordCurrentDecryptedPasswordTextBox);
			this.AccessGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 6, true);
			this.AccessGroupBox.Name = "AccessGroupBox";
			this.AccessGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 129, true);
			this.AccessGroupBox.TabIndex = 12;
			this.AccessGroupBox.TabStop = false;
			// 
			// AccessPasswordGP_PasswordStatusDescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordGP_PasswordStatusDescriptionLabel, "AccessPassword+GP_PasswordStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper)(null)).AccessPassword.GP_PasswordStatusDescription)));
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("ce3cd412-f88f-4d15-9b52-64b4489d5530", "Password Status Description");
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.IsFontBold = true;
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 96, true);
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Name = "AccessPasswordGP_PasswordStatusDescriptionLabel";
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.AccessPasswordGP_PasswordStatusDescriptionLabel.TabIndex = 5;
			// 
			// AccessPasswordStatusLabel
			// 
			this.AccessPasswordStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AccessPasswordStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 100, true);
			this.AccessPasswordStatusLabel.Name = "AccessPasswordStatusLabel";
			this.AccessPasswordStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.AccessPasswordStatusLabel.TabIndex = 4;
			this.AccessPasswordStatusLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9FAC1B6A-9E60-472C-A9C0-AF53021C84EB", "Password Status:");
			// 
			// AccessPasswordUserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordUserIDTextBox, "AccessPassword+GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper)(null)).AccessPassword.GP_UserID)));
			this.AccessPasswordUserIDTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("28ddbe82-5f41-4e81-9a8d-78f6a7af5c9a", "ACCESS User ID");
			this.AccessPasswordUserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordUserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 24, true);
			this.AccessPasswordUserIDTextBox.Name = "AccessPasswordUserIDTextBox";
			this.AccessPasswordUserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordUserIDTextBox.TabIndex = 1;
			// 
			// AccessPasswordNextDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordNextDecryptedPasswordTextBox, "AccessPassword+NextDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper)(null)).AccessPassword.NextDecryptedPassword)));
			this.AccessPasswordNextDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7449c8bd-43d0-4858-94a4-f9b50d8ec0ba", "Next Password");
			this.AccessPasswordNextDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordNextDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 72, true);
			this.AccessPasswordNextDecryptedPasswordTextBox.Name = "AccessPasswordNextDecryptedPasswordTextBox";
			this.AccessPasswordNextDecryptedPasswordTextBox.PasswordChar = '*';
			this.AccessPasswordNextDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordNextDecryptedPasswordTextBox.TabIndex = 3;
			// 
			// AccessPasswordCurrentDecryptedPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessPasswordCurrentDecryptedPasswordTextBox, "AccessPassword+CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper)(null)).AccessPassword.CurrentDecryptedPassword)));
			this.AccessPasswordCurrentDecryptedPasswordTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9270b5f2-6ba2-41fa-a673-c8b8a2d9e316", "Current Password");
			this.AccessPasswordCurrentDecryptedPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 48, true);
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Name = "AccessPasswordCurrentDecryptedPasswordTextBox";
			this.AccessPasswordCurrentDecryptedPasswordTextBox.PasswordChar = '*';
			this.AccessPasswordCurrentDecryptedPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.AccessPasswordCurrentDecryptedPasswordTextBox.TabIndex = 2;
			// 
			// AccessVisibleOnlyToDeveloperTextBox
			// 
			this.AccessVisibleOnlyToDeveloperTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AccessVisibleOnlyToDeveloperTextBox, "AccessPassword+CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.GlbGroupForPluginWrapper)(null)).AccessPassword.CurrentDecryptedPassword)));
			this.AccessVisibleOnlyToDeveloperTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessVisibleOnlyToDeveloperTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 141, true);
			this.AccessVisibleOnlyToDeveloperTextBox.Name = "AccessVisibleOnlyToDeveloperTextBox";
			this.AccessVisibleOnlyToDeveloperTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.AccessVisibleOnlyToDeveloperTextBox.TabIndex = 6;
			this.AccessVisibleOnlyToDeveloperTextBox.Text = "Actual Password Visible to Developer Only";
			this.AccessVisibleOnlyToDeveloperTextBox.Visible = false;
			// 
			// GroupBrokerageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccessVisibleOnlyToDeveloperTextBox);
			this.Controls.Add(this.AccessGroupBox);
			this.Name = "GroupBrokerageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 334, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccessGroupBox.ResumeLayout(false);
			this.AccessGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox AccessGroupBox;
		private ZArchitecture.ZLabel AccessPasswordGP_PasswordStatusDescriptionLabel;
		private ZArchitecture.ZLabel AccessPasswordStatusLabel;
		private ZArchitecture.ZTextBox AccessPasswordUserIDTextBox;
		private ZArchitecture.ZTextBox AccessPasswordNextDecryptedPasswordTextBox;
		private ZArchitecture.ZTextBox AccessPasswordCurrentDecryptedPasswordTextBox;
		private ZArchitecture.ZTextBox AccessVisibleOnlyToDeveloperTextBox;
	}
}
