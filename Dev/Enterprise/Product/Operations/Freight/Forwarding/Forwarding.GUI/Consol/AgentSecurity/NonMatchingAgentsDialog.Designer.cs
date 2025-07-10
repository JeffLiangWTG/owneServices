using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class NonMatchingAgentsDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AuthorizationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoginTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizationGroupBox.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.NonMatchingAgentsSecurity);
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageLabel.TabIndex = 2;
			// 
			// CancelZButton
			// 
			this.CancelZButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|a0155810-2bbf-49c6-b0fa-ababfa755dbd", "Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 10, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelZButton.TabIndex = 1;
			this.CancelZButton.UseVisualStyleBackColor = true;
			this.CancelZButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|37cc57e0-e899-4edf-95ac-03a8e48aa091", "Continue");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 10, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.AttachButton.TabIndex = 0;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// AuthorizationGroupBox
			// 
			this.AuthorizationGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|2d046207-80c6-4ebf-9a9d-ee9ffca3408d", "Authorization");
			this.AuthorizationGroupBox.Controls.Add(this.PasswordTextBox);
			this.AuthorizationGroupBox.Controls.Add(this.LoginTextBox);
			this.AuthorizationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorizationGroupBox.Name = "AuthorizationGroupBox";
			this.AuthorizationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 40, true);
			this.AuthorizationGroupBox.TabIndex = 5;
			this.AuthorizationGroupBox.TabStop = false;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.NonMatchingAgentsSecurity)(null)).Password)));
			this.PasswordTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|1172d1e2-2a54-422c-9bf0-b14eb99fafb7", "Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 12, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PasswordTextBox.TabIndex = 1;
			// 
			// LoginTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoginTextBox, "Login");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.NonMatchingAgentsSecurity)(null)).Login)));
			this.LoginTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|e2ca4404-0db0-48c9-985a-626ba29d8b47", "User Name");
			this.LoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 13, true);
			this.LoginTextBox.Name = "LoginTextBox";
			this.LoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.LoginTextBox.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MessageLabel);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 143, true);
			this.MainPanel.TabIndex = 6;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AuthorizationGroupBox);
			this.BottomPanel.Controls.Add(this.AttachButton);
			this.BottomPanel.Controls.Add(this.CancelZButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 44, true);
			this.BottomPanel.TabIndex = 8;
			// 
			// NonMatchingAgentsDialog
			// 
			this.AcceptButton = this.AttachButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NonMatchingAgentsDialog|802bfa4a-21d5-4562-9149-b78cc8d2a05b", "Non-matching agents");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 211, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.NonMatchingAgentsSecurity);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "NonMatchingAgentsDialog";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "NonMatchingAgentsDialog";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizationGroupBox.ResumeLayout(false);
			this.AuthorizationGroupBox.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected internal ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.GUI.ZButton CancelZButton;
		protected ZArchitecture.GUI.ZButton AttachButton;
		protected ZArchitecture.GUI.ZGroupBox AuthorizationGroupBox;
		private ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.ZTextBox LoginTextBox;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel BottomPanel;
	}
}
