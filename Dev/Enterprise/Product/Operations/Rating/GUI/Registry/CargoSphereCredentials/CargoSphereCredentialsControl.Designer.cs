using CargoWiseOne.ResourceStrings;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	public partial class CargoSphereCredentialsControl
	{
		ZArchitecture.ZTextBox LoginTextBox;
		ZArchitecture.ZTextBox PasswordTextBox;
		ZArchitecture.ZTextBox SystemCodeTextBox;
		ZArchitecture.GUI.ZButton ViewButton;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		/// 

		private void InitializeComponent()
		{
			this.LoginTextBox = new ZArchitecture.ZTextBox();
			this.PasswordTextBox = new ZArchitecture.ZTextBox();
			this.SystemCodeTextBox = new ZArchitecture.ZTextBox();
			this.ViewButton = new ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoSphereCredentials);
			// 
			// RegistrationCodeTextBox
			// 
			this.LoginTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.LoginTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoSphereCredentials)(null)).Login);
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.CaptionResourceString = Res.GetData("CargoSphereCredentialsControl|896a83c6-8d4b-4b51-af4f-b59a17d010a4", "Login");
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 27, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.LoginTextBox.TabIndex = 1;
			// 
			// AuthenticationCodeTextBox
			// 
			this.PasswordTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoSphereCredentials)(null)).Password);
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.CaptionResourceString = Res.GetData("CargoSphereCredentialsControl|05d5dffd-7472-495d-85fa-44889c9c64d8", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 51, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.PasswordTextBox.TabIndex = 2;
			// 
			// ViewButton
			// 
			this.ViewButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 50, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButton.TabIndex = 3;
			this.ViewButton.Text = "View";
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// CustomHouseUsernameTextBox
			// 
			this.SystemCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SystemCodeTextBox, "SystemCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoSphereCredentials)(null)).SystemCode);
			this.SystemCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SystemCodeTextBox.CaptionResourceString = Res.GetData("CargoSphereCredentialsControl|16d7f362-6628-4a88-913d-6256b782e8b4", "{0}").Format("SystemCode");
			this.SystemCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 75, true);
			this.SystemCodeTextBox.Name = "SystemCodeTextBox";
			this.SystemCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.SystemCodeTextBox.TabIndex = 4;
			// 
			// ENettRegistrationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SystemCodeTextBox);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.LoginTextBox);
			this.Controls.Add(this.ViewButton);
			this.Name = "CargoSphereCredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 157, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
