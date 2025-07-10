using Enterprise.Customs.NO.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Registry.GUI;

partial class FTPSettingsEMMADocRegistryItemControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
            this.FtpSettingsEMMADocGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.UrlAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PortTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.FtpSettingsEMMADocGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Registry.FTPSettingsRegistry);
            // 
            // FtpSettingsEMMADocGroupBox
            // 
            this.FtpSettingsEMMADocGroupBox.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("B7A8A095-C318-43AD-9694-AD8DC6599299", "FTP Server settings");
            this.FtpSettingsEMMADocGroupBox.Controls.Add(this.UserNameTextBox);
            this.FtpSettingsEMMADocGroupBox.Controls.Add(this.PasswordTextBox);
            this.FtpSettingsEMMADocGroupBox.Controls.Add(this.ViewButton);
            this.FtpSettingsEMMADocGroupBox.Controls.Add(this.UrlAddressTextBox);
            this.FtpSettingsEMMADocGroupBox.Controls.Add(this.PortTextBox);
            this.FtpSettingsEMMADocGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 13, true);
            this.FtpSettingsEMMADocGroupBox.Name = "FtpSettingsEMMADocGroupBox";
            this.FtpSettingsEMMADocGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 130, true);
            this.FtpSettingsEMMADocGroupBox.TabIndex = 0;
            this.FtpSettingsEMMADocGroupBox.TabStop = false;
            // 
            // UserNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.UserNameTextBox, "Username");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsRegistry)(null)).Username)));
            this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 15, true);
            this.UserNameTextBox.Name = "UserNameTextBox";
            this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 15, true);
            this.UserNameTextBox.TabIndex = 1;
            // 
            // PasswordTextBox
            // 
            this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsRegistry)(null)).Password)));
            this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 36, true);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
            this.PasswordTextBox.TabIndex = 2;
            // 
            // ViewButton
            // 
            this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ViewButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("5D34C7D1-F40E-412F-8B67-351B0DA0DB23", "View");
            this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 36, true);
            this.ViewButton.Name = "ViewButton";
            this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
            this.ViewButton.TabIndex = 3;
            this.ViewButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ViewButton.ToolTipCaption = null;
			this.ViewButton.Click += ViewButton_Click;
			// 
			// UrlAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.UrlAddressTextBox, "Url");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsRegistry)(null)).Url)));
            this.UrlAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UrlAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 61, true);
            this.UrlAddressTextBox.Name = "UrlAddressTextBox";
            this.UrlAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 15, true);
            this.UrlAddressTextBox.TabIndex = 4;
            // 
            // PortTextBox
            // 
            this.BindingSource.SetBindingMember(this.PortTextBox, "Port");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsRegistry)(null)).Port)));
            this.PortTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 83, true);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 15, true);
            this.PortTextBox.TabIndex = 5;
            // 
            // FTPSettingsEMMADocRegistryItemControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.FtpSettingsEMMADocGroupBox);
            this.Name = "FTPSettingsEMMADocRegistryItemControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 164, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.FtpSettingsEMMADocGroupBox.ResumeLayout(false);
            this.FtpSettingsEMMADocGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZGroupBox FtpSettingsEMMADocGroupBox;
	internal ZTextBox UserNameTextBox;
	internal ZTextBox PasswordTextBox;
	internal ZButton ViewButton;
	internal ZTextBox UrlAddressTextBox;
	internal ZTextBox PortTextBox;
}
