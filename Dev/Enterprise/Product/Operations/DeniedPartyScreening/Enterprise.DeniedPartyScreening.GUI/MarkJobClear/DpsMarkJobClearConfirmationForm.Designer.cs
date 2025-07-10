namespace Enterprise.DeniedPartyScreening.GUI
{
	partial class DpsMarkJobClearConfirmationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ConfirmationTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ConfirmationInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PreExpectedStringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExpectedStringLabel = new Enterprise.Core.Environment.UserConfirmationStringLabel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConfirmClearText = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			this.JobClearReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PartyJobClearingReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WarningPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarningMessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarningMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WarningPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfirmationTableLayoutPanel.SuspendLayout();
			this.ConfirmationInfoPanel.SuspendLayout();
			this.PartyJobClearingReasonDropEdit.SuspendLayout();
			this.WarningPanel.SuspendLayout();
			this.WarningMessagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.GUI.DpsMarkJobClearConfirmationModel);
			// 
			// ConfirmationTableLayoutPanel
			// 
			this.ConfirmationTableLayoutPanel.ColumnCount = 1;
			this.ConfirmationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ConfirmationTableLayoutPanel.Controls.Add(this.ConfirmationInfoPanel, 0, 1);
			this.ConfirmationTableLayoutPanel.Controls.Add(this.WarningPanel, 0, 0);
			this.ConfirmationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfirmationTableLayoutPanel.Name = "ConfirmationTableLayoutPanel";
			this.ConfirmationTableLayoutPanel.RowCount = 2;
			this.ConfirmationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ConfirmationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160)));
			this.ConfirmationTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 136, true);
			this.ConfirmationTableLayoutPanel.TabIndex = 1;
			// 
			// ConfirmationInfoPanel
			// 
			this.ConfirmationInfoPanel.Controls.Add(this.PreExpectedStringLabel);
			this.ConfirmationInfoPanel.Controls.Add(this.ExpectedStringLabel);
			this.ConfirmationInfoPanel.Controls.Add(this.CancelButton);
			this.ConfirmationInfoPanel.Controls.Add(this.OKButton);
			this.ConfirmationInfoPanel.Controls.Add(this.ConfirmClearText);
			this.ConfirmationInfoPanel.Controls.Add(this.ReasonLabel);
			this.ConfirmationInfoPanel.Controls.Add(this.JobClearReasonTextBox);
			this.ConfirmationInfoPanel.Controls.Add(this.PartyJobClearingReasonDropEdit);
			this.ConfirmationInfoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmationInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, -22, true);
			this.ConfirmationInfoPanel.Name = "ConfirmationInfoPanel";
			this.ConfirmationInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 155, true);
			this.ConfirmationInfoPanel.TabIndex = 3;
			// 
			// PreExpectedStringLabel
			// 
			this.PreExpectedStringLabel.AutoSize = true;
			this.PreExpectedStringLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("014ed051-53fa-4ad4-bb7e-95a40dd81267", "Please type the following to continue:");
			this.PreExpectedStringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PreExpectedStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 98, true);
			this.PreExpectedStringLabel.Name = "PreExpectedStringLabel";
			this.PreExpectedStringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 14, true);
			this.PreExpectedStringLabel.TabIndex = 15;
			// 
			// ExpectedStringLabel
			// 
			this.ExpectedStringLabel.AutoSize = true;
			this.ExpectedStringLabel.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("f02f25b9-33bd-469a-a5e6-f0b9b374b6cc", "I AM AUTHORISED TO CLEAR THIS JOB");
			this.ExpectedStringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExpectedStringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 98, true);
			this.ExpectedStringLabel.Name = "ExpectedStringLabel";
			this.ExpectedStringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 14, true);
			this.ExpectedStringLabel.TabIndex = 14;
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("358d14e4-8bd0-4b00-9783-e66af7234f7e", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 125, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 13;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("11d668f5-b2ff-4ae0-a76f-243014830ae9", "OK");
			this.OKButton.Enabled = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 125, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 12;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ConfirmClearText
			// 
			this.ConfirmClearText.CaptionResourceString = null;
			this.ConfirmClearText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 95, true);
			this.ConfirmClearText.Name = "ConfirmClearText";
			this.ConfirmClearText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 18, true);
			this.ConfirmClearText.TabIndex = 11;
			this.ConfirmClearText.TextChanged += new System.EventHandler(this.ConfirmMergeText_TextChanged);
			this.ConfirmClearText.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ConfirmMergeText_KeyPress);
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 3, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 56, true);
			this.ReasonLabel.TabIndex = 1;
			this.ReasonLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// JobClearReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.JobClearReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.GUI.DpsMarkJobClearConfirmationModel)(null)).Reason)));
			this.JobClearReasonTextBox.CaptionResourceString = null;
			this.JobClearReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JobClearReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 30, true);
			this.JobClearReasonTextBox.Multiline = true;
			this.JobClearReasonTextBox.Name = "JobClearReasonTextBox";
			this.JobClearReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JobClearReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 56, true);
			this.JobClearReasonTextBox.TabIndex = 10;
			// 
			// PartyJobClearingReasonDropEdit
			// 
			this.PartyJobClearingReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartyJobClearingReasonDropEdit, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ComplianceRisk.GUI.DpsMarkJobClearConfirmationModel)(null)).Code)));
			this.PartyJobClearingReasonDropEdit.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("JobClearConfirmationForm|844BEB7A-8592-4446-8902-83736EABA0F8", "Party Job Clearing Reason Drop Edit");
			this.PartyJobClearingReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 3, true);
			this.PartyJobClearingReasonDropEdit.Name = "PartyJobClearingReasonDropEdit";
			this.PartyJobClearingReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 18, true);
			this.PartyJobClearingReasonDropEdit.TabIndex = 9;
			// 
			// WarningPanel
			// 
			this.WarningPanel.AutoSize = true;
			this.WarningPanel.Controls.Add(this.WarningMessagePanel);
			this.WarningPanel.Controls.Add(this.WarningPictureBox);
			this.WarningPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarningPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.WarningPanel.Name = "WarningPanel";
			this.WarningPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 1, true);
			this.WarningPanel.TabIndex = 1;
			// 
			// WarningMessagePanel
			// 
			this.WarningMessagePanel.AutoScroll = true;
			this.WarningMessagePanel.Controls.Add(this.WarningMessageLabel);
			this.WarningMessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 0, true);
			this.WarningMessagePanel.Name = "WarningMessagePanel";
			this.WarningMessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.WarningMessagePanel.TabIndex = 2;
			// 
			// WarningMessageLabel
			// 
			this.WarningMessageLabel.AutoSize = true;
			this.WarningMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 5, true);
			this.WarningMessageLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 10, 3, 0, true);
			this.WarningMessageLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 0, true);
			this.WarningMessageLabel.Name = "WarningMessageLabel";
			this.WarningMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.WarningMessageLabel.TabIndex = 3;
			// 
			// WarningPictureBox
			// 
			this.WarningPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.WarningPictureBox.Name = "WarningPictureBox";
			this.WarningPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.WarningPictureBox.TabIndex = 0;
			this.WarningPictureBox.TabStop = false;
			// 
			// DpsMarkJobClearConfirmationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DeniedPartyScreening.GUI.Res.GetData("d153ca49-3b81-4e1b-9f37-f001d9d97ede", "Job Clear Confirmation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 160, true);
			this.Controls.Add(this.ConfirmationTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.ComplianceRisk.GUI.DpsMarkJobClearConfirmationModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "DpsMarkJobClearConfirmationForm";
			this.Text = "DpsJobMarkClearConfirmationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConfirmationTableLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfirmationTableLayoutPanel.ResumeLayout(false);
			this.ConfirmationTableLayoutPanel.PerformLayout();
			this.ConfirmationInfoPanel.ResumeLayout(false);
			this.ConfirmationInfoPanel.PerformLayout();
			this.PartyJobClearingReasonDropEdit.ResumeLayout(true);
			this.PartyJobClearingReasonDropEdit.PerformLayout();
			this.WarningPanel.ResumeLayout(false);
			this.WarningPanel.PerformLayout();
			this.WarningMessagePanel.ResumeLayout(false);
			this.WarningMessagePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel ConfirmationTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel WarningPanel;
		internal Enterprise.ZArchitecture.GUI.ZPictureBox WarningPictureBox;
		private ZArchitecture.GUI.ZPanel ConfirmationInfoPanel;
		internal ZArchitecture.ZTextBox JobClearReasonTextBox;
		internal ZArchitecture.ZLabel ReasonLabel;
		private ZArchitecture.ZLabel PreExpectedStringLabel;
		private Core.Environment.UserConfirmationStringLabel ExpectedStringLabel;
		private new ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton OKButton;
		internal ZArchitecture.ZTextBox ConfirmClearText;
		internal ZArchitecture.ZLabel WarningMessageLabel;
		private ZArchitecture.GUI.ZPanel WarningMessagePanel;
		private ZArchitecture.GUI.ZDropEdit PartyJobClearingReasonDropEdit;
	}
}
