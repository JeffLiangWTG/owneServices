using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class LoginForm
	{
		private ZArchitecture.ZLabel InstructionsLabel;
		private ZGroupBox LoginGroupBox;
		private ZButton CancelXButton;
		private ZButton OKButton;
		private ZArchitecture.ZTextBox LoginTextBox;
		private ZArchitecture.ZTextBox PasswordTextBox;

		new void InitializeComponent()
		{
			this.InstructionsLabel = new ZArchitecture.ZLabel();
			this.LoginGroupBox = new ZGroupBox();
			this.CancelXButton = new ZButton();
			this.OKButton = new ZButton();
			this.LoginTextBox = new ZArchitecture.ZTextBox();
			this.PasswordTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoginGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(SecurityOverridenLogin);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|fdab7343-ba3a-4592-9f4e-5a1d036a6f42", "", "To complete this operation, a user with higher security rights to must login. Please enter username and password details below.");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 7, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 45, true);
			this.InstructionsLabel.TabIndex = 6;
			// 
			// LoginGroupBox
			// 
			this.LoginGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|0ebe024b-741d-4ac2-b572-fc1b4f0488c5", "Login Information");
			this.LoginGroupBox.Controls.Add(this.CancelXButton);
			this.LoginGroupBox.Controls.Add(this.OKButton);
			this.LoginGroupBox.Controls.Add(this.LoginTextBox);
			this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
			this.LoginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 74, true);
			this.LoginGroupBox.Name = "LoginGroupBox";
			this.LoginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 112, true);
			this.LoginGroupBox.TabIndex = 7;
			this.LoginGroupBox.TabStop = false;
			// 
			// CancelXButton
			// 
			this.CancelXButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|1a6b8668-3fe0-47e4-bedd-62c0c9a4e539", "Cancel");
			this.CancelXButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 82, true);
			this.CancelXButton.Name = "CancelXButton";
			this.CancelXButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelXButton.TabIndex = 11;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|19b60017-bfd7-4934-b4f7-5509dcfed2ce", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 82, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 10;
			// 
			// LoginTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoginTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SecurityOverridenLogin)(null)).Login);
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|ba1338d9-417b-4cc1-a7c1-ae92b39be620", "Username");
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 22, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.LoginTextBox.TabIndex = 7;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((SecurityOverridenLogin)(null)).Password);
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("LoginForm|e2c5ace1-b3dc-4840-8d3f-da81f6bd91e5", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 45, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.PasswordTextBox.TabIndex = 9;
			// 
			// LoginForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 240, true);
			this.Controls.Add(this.LoginGroupBox);
			this.Controls.Add(this.InstructionsLabel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(SecurityOverridenLogin);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.SecurityOverridenLogin";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "LoginForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LoginForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.LoginGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoginGroupBox.ResumeLayout(false);
			this.LoginGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
