using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DocumentActionReasonDialog
	{
		new void InitializeComponent()
		{
            this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
            this.reasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.reasonTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.reasonCodeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 24, true);
            this.MainStatusBar.Visible = false;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.DocumentActionReasonModel);
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.okButton);
            this.bottomPanel.Controls.Add(this.cancelButton);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 191, true);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 41, true);
            this.bottomPanel.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("34c63aac-b595-4770-8fa4-7ce0174bc10f", "&OK");
            this.okButton.IsCaptionOverridden = false;
            this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 9, true);
            this.okButton.Name = "okButton";
            this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.okButton.TabIndex = 2;
            this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.okButton.ToolTipCaption = null;
            this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += OnSendButtonClick;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a852429d-cbc7-4673-a038-a99ea44ee19b", "&Cancel");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.IsCaptionOverridden = false;
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 9, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.cancelButton.ToolTipCaption = null;
            this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += OnCancelButtonClick;
			// 
			// messageLabel
			// 
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.messageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 61, true);
            this.messageLabel.TabIndex = 0;
            // 
            // reasonCodeDropEdit
            // 
            this.reasonCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.reasonCodeDropEdit, "ReasonCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.DocumentActionReasonModel)(null)).ReasonCode)));
            this.reasonCodeDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7d8592f5-28e5-43e6-b841-69c789ac0c0b", "Reason Code");
            this.reasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 76, true);
            this.reasonCodeDropEdit.MaxItemsToShowInDropDown = 100;
            this.reasonCodeDropEdit.Name = "reasonCodeDropEdit";
            this.reasonCodeDropEdit.ShouldResizeByMaxLength = true;
            this.reasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
            this.reasonCodeDropEdit.TabIndex = 1;
            this.reasonCodeDropEdit.UseFullWidthForCodeBox = true;
            // 
            // reasonTextTextBox
            // 
            this.BindingSource.SetBindingMember(this.reasonTextTextBox, "ReasonText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.DocumentActionReasonModel)(null)).ReasonText)));
            this.reasonTextTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("56db181f-dd5f-4435-947a-55ef74963244", "Reason Text");
            this.reasonTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.reasonTextTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.reasonTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 116, true);
            this.reasonTextTextBox.Multiline = true;
            this.reasonTextTextBox.Name = "reasonTextTextBox";
            this.reasonTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 71, true);
            this.reasonTextTextBox.TabIndex = 2;
            // 
            // DocumentActionReasonDialog
            // 
            this.CancelButton = this.cancelButton;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9163599f-f2a0-429b-ba2b-0f5868b000ca", "Confirmation");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 256, true);
            this.Controls.Add(this.reasonTextTextBox);
            this.Controls.Add(this.reasonCodeDropEdit);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.bottomPanel);
            this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.DocumentActionReasonModel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.Name = "DocumentActionReasonDialog";
            this.RememberFormPosition = false;
            this.RememberFormSize = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.bottomPanel, 0);
            this.Controls.SetChildIndex(this.messageLabel, 0);
            this.Controls.SetChildIndex(this.reasonCodeDropEdit, 0);
            this.Controls.SetChildIndex(this.reasonTextTextBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.reasonCodeDropEdit.ResumeLayout(true);
            this.reasonCodeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}
		private ZArchitecture.GUI.ZPanel bottomPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton okButton;
		private Enterprise.ZArchitecture.ZLabel messageLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit reasonCodeDropEdit;
		private ZArchitecture.ZTextBox reasonTextTextBox;
	}
}
