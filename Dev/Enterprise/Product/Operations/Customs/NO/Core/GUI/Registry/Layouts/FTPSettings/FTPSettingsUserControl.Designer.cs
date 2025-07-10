using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

partial class FTPSettingsUserControl
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
		this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.UrlAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.PortTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.UserNameTextBox.SuspendLayout();
		this.PasswordTextBox.SuspendLayout();
		this.ViewButton.SuspendLayout();
		this.UrlAddressTextBox.SuspendLayout();
		this.PortTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Registry.FTPSettingsCustomsRegistry);
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
		//this.ViewButton.CaptionResourceString = Res.GetData("162F786A-566D-478D-8398-F0DDAC48647D", "View");
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
		// FTPSettingsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.UserNameTextBox);
		this.Controls.Add(this.PasswordTextBox);
		this.Controls.Add(this.ViewButton);
		this.Controls.Add(this.UrlAddressTextBox);
		this.Controls.Add(this.PortTextBox);
		this.CaptionRenderingEnabled = true;
		this.Name = "FTPSettingsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 202, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.UserNameTextBox.ResumeLayout(false);
		this.UserNameTextBox.PerformLayout();
		this.PasswordTextBox.ResumeLayout(false);
		this.PasswordTextBox.ResumeLayout();
		this.ViewButton.ResumeLayout(false);
		this.ViewButton.PerformLayout();
		this.UrlAddressTextBox.ResumeLayout(false);
		this.UrlAddressTextBox.PerformLayout();
		this.PortTextBox.ResumeLayout(false);
		this.PortTextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZTextBox UserNameTextBox;
	internal ZTextBox PasswordTextBox;
	internal ZButton ViewButton;
	internal ZTextBox UrlAddressTextBox;
	internal ZTextBox PortTextBox;
}
