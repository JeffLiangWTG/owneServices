namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class DeniedPartySecurityOverrideForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.usernameLabel = new ZArchitecture.ZLabel();
			this.passwordLabel = new ZArchitecture.ZLabel();
			this.usernameText = new ZArchitecture.ZTextBox();
			this.passwordText = new ZArchitecture.ZTextBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 24, true);
			// 
			// usernameLabel
			// 
			this.usernameLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("87c309fb-9440-4d3f-9dda-794218cdc049", "Username:");
			this.usernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 13, true);
			this.usernameLabel.Name = "usernameLabel";
			this.usernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.usernameLabel.TabIndex = 1;
			// 
			// passwordLabel
			// 
			this.passwordLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("c0848e4d-56be-4f08-b2b3-302590c03276", "Password:");
			this.passwordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 39, true);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.passwordLabel.TabIndex = 2;
			// 
			// usernameText
			// 
			this.usernameText.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("F4F3E8AD-F727-4EF6-B95D-53A9F64A4B43", "Username");
			this.usernameText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.usernameText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 15, true);
			this.usernameText.Name = "usernameText";
			this.usernameText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.usernameText.TabIndex = 3;
			// 
			// passwordText
			// 
			this.passwordText.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("561225B0-9EC8-4D19-B6F9-089AA1D84E29", "Password");
			this.passwordText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.passwordText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 41, true);
			this.passwordText.Name = "passwordText";
			this.passwordText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.passwordText.TabIndex = 4;
			this.passwordText.UseSystemPasswordChar = true;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("7d31184e-ea20-4cfe-964e-1f247a7d8454", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 67, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("e76807cd-25f9-485a-84f3-23d69c46d07b", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 67, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 6;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// DeniedPartySecurityOverrideForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("0607709a-9ffc-4ad1-8464-140d4a28d491", "Security Override Login");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 118, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.passwordText);
			this.Controls.Add(this.usernameText);
			this.Controls.Add(this.passwordLabel);
			this.Controls.Add(this.usernameLabel);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 157, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 157, true);
			this.Name = "DeniedPartySecurityOverrideForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.usernameLabel, 0);
			this.Controls.SetChildIndex(this.passwordLabel, 0);
			this.Controls.SetChildIndex(this.usernameText, 0);
			this.Controls.SetChildIndex(this.passwordText, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZLabel usernameLabel;
		ZArchitecture.ZLabel passwordLabel;
		internal ZArchitecture.ZTextBox usernameText;
		internal ZArchitecture.ZTextBox passwordText;
		Enterprise.ZArchitecture.GUI.ZButton okButton;
		Enterprise.ZArchitecture.GUI.ZButton cancelButton;
	}
}
