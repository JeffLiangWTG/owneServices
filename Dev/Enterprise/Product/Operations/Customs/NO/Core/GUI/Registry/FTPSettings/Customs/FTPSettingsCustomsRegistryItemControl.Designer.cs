using Enterprise.Customs.NO.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Registry.GUI;

partial class FTPSettingsCustomsRegistryItemControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.FtpSettingsCustomsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.UrlAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.PortTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.SendToCustomFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ReceiveFromCustomFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.FtpSettingsCustomsGroupBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry);
		// 
		// FtpSettingsCustomsGroupBox
		// 
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.UserNameTextBox);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.PasswordTextBox);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.ViewButton);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.UrlAddressTextBox);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.PortTextBox);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.SendToCustomFolderTextBox);
		this.FtpSettingsCustomsGroupBox.Controls.Add(this.ReceiveFromCustomFolderTextBox);
		this.FtpSettingsCustomsGroupBox.CaptionResourceString = Res.GetData("1382A9A5-9A6F-4AD9-9E70-BED638F54552", "FTP Server settings");
		this.FtpSettingsCustomsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 13, true);
		this.FtpSettingsCustomsGroupBox.Name = "FtpSettingsCustomsGroupBox";
		this.FtpSettingsCustomsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 174, true);
		this.FtpSettingsCustomsGroupBox.TabIndex = 0;
		this.FtpSettingsCustomsGroupBox.TabStop = false;
		// 
		// UserNameTextBox
		// 
		this.BindingSource.SetBindingMember(this.UserNameTextBox, "Username");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).Username)));
		this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 15, true);
		this.UserNameTextBox.Name = "UserNameTextBox";
		this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.UserNameTextBox.TabIndex = 1;
		// 
		// PasswordTextBox
		// 
		this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).Password)));
		this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 36, true);
		this.PasswordTextBox.Name = "PasswordTextBox";
		this.PasswordTextBox.PasswordChar = '*';
		this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 18, true);
		this.PasswordTextBox.TabIndex = 2;
		// 
		// ViewButton
		//
		this.ViewButton.CaptionResourceString = Res.GetData("162F786A-566D-478D-8398-F0DDAC48647D", "View");
		this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
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
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).Url)));
		this.UrlAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.UrlAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 61, true);
		this.UrlAddressTextBox.Name = "UrlAddressTextBox";
		this.UrlAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.UrlAddressTextBox.TabIndex = 4;
		// 
		// PortTextBox
		// 
		this.BindingSource.SetBindingMember(this.PortTextBox, "Port");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).Port)));
		this.PortTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.PortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 83, true);
		this.PortTextBox.Name = "PortTextBox";
		this.PortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.PortTextBox.TabIndex = 5;
		// 
		// SendToCustomFolderTextBox
		// 
		this.BindingSource.SetBindingMember(this.SendToCustomFolderTextBox, "SendToCustomFolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).SendToCustomFolder)));
		this.SendToCustomFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.SendToCustomFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 103, true);
		this.SendToCustomFolderTextBox.Name = "SendToCustomFolderTextBox";
		this.SendToCustomFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.SendToCustomFolderTextBox.TabIndex = 6;
		// 
		// ReceiveFromCustomFolderTextBox
		// 
		this.BindingSource.SetBindingMember(this.ReceiveFromCustomFolderTextBox, "ReceiveFromCustomFolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry)(null)).ReceiveFromCustomFolder)));
		this.ReceiveFromCustomFolderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.ReceiveFromCustomFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 126, true);
		this.ReceiveFromCustomFolderTextBox.Name = "ReceiveFromCustomFolderTextBox";
		this.ReceiveFromCustomFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
		this.ReceiveFromCustomFolderTextBox.TabIndex = 7;
		// 
		// FTPSettingsCustomsRegistryItemControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.FtpSettingsCustomsGroupBox);
		this.Name = "FTPSettingsCustomsRegistryItemControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 202, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.FtpSettingsCustomsGroupBox.ResumeLayout(false);
		this.FtpSettingsCustomsGroupBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZGroupBox FtpSettingsCustomsGroupBox;
	internal ZTextBox UserNameTextBox;
	internal ZTextBox PasswordTextBox;
	internal ZButton ViewButton;
	internal ZTextBox UrlAddressTextBox;
	internal ZTextBox PortTextBox;
	internal ZTextBox SendToCustomFolderTextBox;
	internal ZTextBox ReceiveFromCustomFolderTextBox;
}
