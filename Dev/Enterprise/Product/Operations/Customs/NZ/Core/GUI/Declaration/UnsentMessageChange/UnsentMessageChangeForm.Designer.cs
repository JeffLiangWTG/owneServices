
namespace Enterprise.Customs.NZ.GUI.Declaration
{
    partial class UnsentMessageChangeForm
    {
		public Enterprise.ZArchitecture.GUI.ZButton ProceedButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProceedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CancelSaveRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LeaveMessageUnchangedRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RecreateMessageRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.MessageTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 267, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 25, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(196);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(197);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo);
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemarksTextBox, "RemarksForLeavingMessageUnchanged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo)(null)).RemarksForLeavingMessageUnchanged)));
			this.RemarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 126, true);
			this.RemarksTextBox.Multiline = true;
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 66, true);
			this.RemarksTextBox.TabIndex = 4;
			this.RemarksTextBox.TextChanged += new System.EventHandler(this.RemarksTextBox_TextChanged);
			// 
			// ProceedButton
			// 
			this.ProceedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProceedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 238, true);
			this.ProceedButton.Name = "ProceedButton";
			this.ProceedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.ProceedButton.TabIndex = 2;
			this.ProceedButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("dac26061-785f-40df-a590-abcff46eafb1", "Proceed");
			this.ProceedButton.Click += new System.EventHandler(this.ProceedButton_Click);
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OptionsGroupBox.Controls.Add(this.zLabel1);
			this.OptionsGroupBox.Controls.Add(this.CancelSaveRadioButton);
			this.OptionsGroupBox.Controls.Add(this.LeaveMessageUnchangedRadioButton);
			this.OptionsGroupBox.Controls.Add(this.RecreateMessageRadioButton);
			this.OptionsGroupBox.Controls.Add(this.RemarksTextBox);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 34, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 198, true);
			this.OptionsGroupBox.TabIndex = 1;
			this.OptionsGroupBox.TabStop = false;
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("45c158f3-e663-4eff-bab6-1cbb57cb035b", "Please choose one of the following options:");
			// 
			// zLabel1
			// 
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 101, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 22, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("69c38637-30ce-4e62-a65e-e976e10d7072", "Remarks (for not re-creating customs message):");
			// 
			// CancelSaveRadioButton
			// 
			this.CancelSaveRadioButton.AutoCheck = false;
			this.CancelSaveRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CancelSaveRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 76, true);
			this.CancelSaveRadioButton.Name = "CancelSaveRadioButton";
			this.CancelSaveRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 22, true);
			this.CancelSaveRadioButton.TabIndex = 2;
			this.CancelSaveRadioButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8938b286-07d4-4530-adb2-664c10aaeb93", " Cancel saving changes");
			this.CancelSaveRadioButton.CheckedChanged += new System.EventHandler(this.MessageRadioButtonGroup_CheckedChanged);
			// 
			// LeaveMessageUnchangedRadioButton
			// 
			this.LeaveMessageUnchangedRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.LeaveMessageUnchangedRadioButton, "ShouldLeaveMessageUnchanged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo)(null)).ShouldLeaveMessageUnchanged)));
			this.LeaveMessageUnchangedRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LeaveMessageUnchangedRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 48, true);
			this.LeaveMessageUnchangedRadioButton.Name = "LeaveMessageUnchangedRadioButton";
			this.LeaveMessageUnchangedRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 22, true);
			this.LeaveMessageUnchangedRadioButton.TabIndex = 1;
			this.LeaveMessageUnchangedRadioButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4361a6da-e6f7-4c6b-945a-da4759db06f8", " Leave unsent message as it is (please provide remarks below)");
			this.LeaveMessageUnchangedRadioButton.CheckedChanged += new System.EventHandler(this.MessageRadioButtonGroup_CheckedChanged);
			// 
			// RecreateMessageRadioButton
			// 
			this.RecreateMessageRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RecreateMessageRadioButton, "ShouldRecreateMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo)(null)).ShouldRecreateMessage)));
			this.RecreateMessageRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RecreateMessageRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.RecreateMessageRadioButton.Name = "RecreateMessageRadioButton";
			this.RecreateMessageRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 23, true);
			this.RecreateMessageRadioButton.TabIndex = 0;
			this.RecreateMessageRadioButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("39184b71-0248-4aac-ad3d-426329811937", " Create new message and cancel existing one");
			this.RecreateMessageRadioButton.CheckedChanged += new System.EventHandler(this.MessageRadioButtonGroup_CheckedChanged);
			// 
			// MessageTypeLabel
			// 
			this.MessageTypeLabel.IsFontBold = true;
			this.MessageTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.MessageTypeLabel.Name = "MessageTypeLabel";
			this.MessageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 22, true);
			this.MessageTypeLabel.TabIndex = 0;
			this.MessageTypeLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("91f2d218-6866-4144-800e-d432f5b0b055", "Current changes affect unsent message.");
			// 
			// UnsentMessageChangeForm
			// 
			
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 292, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageTypeLabel);
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.ProceedButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.HeldMessageSyncInfo";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 330, true);
			this.Name = "UnsentMessageChangeForm";
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("94da6b05-0195-4c5b-9924-6cd94342ac0f", "Current Changes Affect Unsent Customs Message");
			this.Controls.SetChildIndex(this.ProceedButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.MessageTypeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.OptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

        protected Enterprise.ZArchitecture.ZTextBox RemarksTextBox;
		public Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		protected Enterprise.ZArchitecture.ZLabel MessageTypeLabel;
        private ZArchitecture.GUI.ZRadioButton CancelSaveRadioButton;
        private ZArchitecture.GUI.ZRadioButton LeaveMessageUnchangedRadioButton;
        private ZArchitecture.GUI.ZRadioButton RecreateMessageRadioButton;
        protected ZArchitecture.ZLabel zLabel1;
	}
}
