using System.Windows.Forms;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class UserEscalationLoginForm
	{
		#region Designer generated code

		void InitializeComponent()
		{
            this.LoginFieldContainer = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.UsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PasswordLabel = new Enterprise.ZArchitecture.ZLabel();
            this.UsernameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CancelXButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.MessageContainer = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LoginFieldContainer.SuspendLayout();
            this.MessageContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoginFieldContainer
            // 
            this.LoginFieldContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LoginFieldContainer.Controls.Add(this.PasswordTextBox);
            this.LoginFieldContainer.Controls.Add(this.UsernameTextBox);
            this.LoginFieldContainer.Controls.Add(this.PasswordLabel);
            this.LoginFieldContainer.Controls.Add(this.UsernameLabel);
            this.LoginFieldContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 95, true);
            this.LoginFieldContainer.Name = "LoginFieldContainer";
            this.LoginFieldContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 81, true);
            this.LoginFieldContainer.TabIndex = 0;
            this.LoginFieldContainer.TabStop = false;
            this.LoginFieldContainer.Text = Res.GetString("9902d784-96bd-4ce0-8d16-07edde292945", "Authorizing User");
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 52, true);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 15, true);
            this.PasswordTextBox.TabIndex = 3;
            // 
            // UsernameTextBox
            // 
            this.UsernameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.UsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 27, true);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 15, true);
            this.UsernameTextBox.TabIndex = 2;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.PasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 51, true);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
            this.PasswordLabel.TabIndex = 1;
            this.PasswordLabel.Text = Res.GetString("97c843dd-b026-4a5e-b700-d22231fc74ba", "Password:");
            this.PasswordLabel.UseMnemonic = false;
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.UsernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 27, true);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
            this.UsernameLabel.TabIndex = 0;
            this.UsernameLabel.Text = Res.GetString("e626a0c6-6498-46eb-89c9-79a02d32926c", "Username:");
            this.UsernameLabel.UseMnemonic = false;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.IsCaptionOverridden = true;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 183, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = Res.GetString("62ce27af-44a6-4757-9f78-35cc71fceb03", "OK");
            this.OKButton.ToolTipCaption = null;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelXButton
            // 
            this.CancelXButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelXButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelXButton.IsCaptionOverridden = true;
            this.CancelXButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 183, true);
            this.CancelXButton.Name = "CancelXButton";
            this.CancelXButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
            this.CancelXButton.TabIndex = 2;
            this.CancelXButton.Text = Res.GetString("8535fa31-f59f-4a6b-9b3f-648188573164", "Cancel");
            this.CancelXButton.ToolTipCaption = null;
            this.CancelXButton.Click += new System.EventHandler(this.CancelXButton_Click);
            // 
            // MessageContainer
            // 
            this.MessageContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageContainer.Controls.Add(this.MessageLabel);
            this.MessageContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
            this.MessageContainer.Name = "MessageContainer";
            this.MessageContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 92, true);
            this.MessageContainer.TabIndex = 10;
            this.MessageContainer.TabStop = false;
            // 
            // MessageLabel
            // 
            this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MessageLabel.BackColor = System.Drawing.SystemColors.Control;
            this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.BorderStyle = BorderStyle.None;
            this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 69, true);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.TabStop = false;
            // 
            // UserEscalationLoginForm
            // 
            this.AcceptButton = this.OKButton;
            this.CancelButton = this.CancelXButton;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 213, true);
            this.Controls.Add(this.MessageContainer);
            this.Controls.Add(this.CancelXButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.LoginFieldContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserEscalationLoginForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = Res.GetString("3946fa17-e0e3-478f-a462-c9ea4d678041", "Security Override Login");
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LoginFieldContainer.ResumeLayout(false);
            this.LoginFieldContainer.PerformLayout();
            this.MessageContainer.ResumeLayout(false);
            this.MessageContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox LoginFieldContainer;
		private Enterprise.ZArchitecture.ZLabel UsernameLabel;
		private Enterprise.ZArchitecture.ZLabel PasswordLabel;
		private Enterprise.ZArchitecture.ZTextBox UsernameTextBox;
		private Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageContainer;
		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CancelXButton;
	}
}
