using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ChangePasswordDialog : ZChildForm
	{
		protected internal ZGroupBox LoginGroupBox;
		protected internal ZTextBox UserNameTextBox;
		protected internal ZTextBox OldPasswordTextBox;
		protected internal ZTextBox NewPasswordTextBox;
		protected internal ZTextBox ConfirmPasswordTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		protected internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		internal ZLabel ErrorLabel;

		new void InitializeComponent()
		{
			this.LoginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConfirmPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OldPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoginGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 365, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// LoginGroupBox
			// 
			this.LoginGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LoginGroupBox.BackColor = System.Drawing.Color.Transparent;
			this.LoginGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eeea8850-b6fa-4da3-9c89-a10c22f4da88", "Login Information");
			this.LoginGroupBox.Controls.Add(this.NewPasswordTextBox);
			this.LoginGroupBox.Controls.Add(this.ConfirmPasswordTextBox);
			this.LoginGroupBox.Controls.Add(this.UserNameTextBox);
			this.LoginGroupBox.Controls.Add(this.OldPasswordTextBox);
			this.LoginGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 100, true);
			this.LoginGroupBox.Name = "LoginGroupBox";
			this.LoginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 187, true);
			this.LoginGroupBox.TabIndex = 12;
			this.LoginGroupBox.TabStop = false;
			// 
			// NewPasswordTextBox
			// 
			this.NewPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("06a5a099-a773-41ff-8d8b-4a4c7e86e76f", "New Password");
			this.NewPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NewPasswordTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.NewPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 106, true);
			this.NewPasswordTextBox.Name = "NewPasswordTextBox";
			this.NewPasswordTextBox.PasswordChar = '*';
			this.NewPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 21, true);
			this.NewPasswordTextBox.TabIndex = 6;
			this.NewPasswordTextBox.TextChanged += new System.EventHandler(this.Password_TextChanged);
			// 
			// ConfirmPasswordTextBox
			// 
			this.ConfirmPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("eb8b165a-db5d-4194-ab8a-d69e866c71d3", "Confirm New Password");
			this.ConfirmPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConfirmPasswordTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConfirmPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 146, true);
			this.ConfirmPasswordTextBox.Name = "ConfirmPasswordTextBox";
			this.ConfirmPasswordTextBox.PasswordChar = '*';
			this.ConfirmPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 21, true);
			this.ConfirmPasswordTextBox.TabIndex = 7;
			this.ConfirmPasswordTextBox.TextChanged += new System.EventHandler(this.Password_TextChanged);
			// 
			// UserNameTextBox
			// 
			this.UserNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("00246c07-145d-4824-ac17-7865e4ab5bb8", "Username");
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 26, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.ReadOnly = true;
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 21, true);
			this.UserNameTextBox.TabIndex = 1;
			// 
			// OldPasswordTextBox
			// 
			this.OldPasswordTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d8faf152-5443-4bb6-abf3-bbd0eaf341ac", "Old Password");
			this.OldPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OldPasswordTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OldPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 66, true);
			this.OldPasswordTextBox.Name = "OldPasswordTextBox";
			this.OldPasswordTextBox.PasswordChar = '*';
			this.OldPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 21, true);
			this.OldPasswordTextBox.TabIndex = 3;
			this.OldPasswordTextBox.TextChanged += new System.EventHandler(this.Password_TextChanged);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6ef351af-f51e-4e15-90dd-0686cd301d16", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 354, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.Cancel_Button.TabIndex = 14;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("568d34d1-a2a1-416d-ad96-e8eac0d7121d", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 354, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.OKButton.TabIndex = 13;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ErrorLabel
			// 
			this.ErrorLabel.BackColor = System.Drawing.Color.Transparent;
			this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
			this.ErrorLabel.IsFontBold = true;
			this.ErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 290, true);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 47, true);
			this.ErrorLabel.TabIndex = 15;
			// 
			// ChangePasswordDialog
			// 
			this.AcceptButton = this.OKButton;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CancelButton = this.Cancel_Button;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6eb170ba-1378-4b32-9245-c42f9850f182", "Change Password");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 389, true);
			this.Controls.Add(this.ErrorLabel);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.LoginGroupBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "ChangePasswordDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Load += new System.EventHandler(this.ChangePasswordDialog_Load);
			this.Controls.SetChildIndex(this.LoginGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.ErrorLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoginGroupBox.ResumeLayout(false);
			this.LoginGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
