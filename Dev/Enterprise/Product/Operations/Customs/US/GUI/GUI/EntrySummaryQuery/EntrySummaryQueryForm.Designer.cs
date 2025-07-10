namespace Enterprise.Customs.US.GUI
{
	partial class EntrySummaryQueryForm
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
		private new void InitializeComponent()
		{
			this.FilerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Label = new Enterprise.ZArchitecture.ZLabel();
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CriteriaCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateFromTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.TimeToTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEdit();
			this.EntrySummariesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FTAReconSummariesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OtherReconSummariesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DrawbackSummariesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NAFTADutyDeferralSummariesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.CriteriaCodeDropEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 24, true);
			this.MainStatusBar.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.EntrySummaryQueryBizObj);
			// 
			// FilerTextBox
			// 
			this.BindingSource.SetBindingMember(this.FilerTextBox, "EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).EntryFilerCode)));
			this.FilerTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|f66f8de2-4c4e-4e5d-a55d-de4e45fa5059", "Entry Number");
			this.FilerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 8, true);
			this.FilerTextBox.Name = "FilerTextBox";
			this.FilerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 20, true);
			this.FilerTextBox.TabIndex = 0;
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).EntryNumber)));
			this.EntryNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|eaf7a56d-283e-4271-84ff-db2fb02d9f22", "Entry Number");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryNumberTextBox, false);
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 8, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.EntryNumberTextBox.TabIndex = 2;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 331, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 14;
			this.SendButton.Text = "&Send";
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 331, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 15;
			this.GiveUpButton.Text = "&Cancel";
			this.GiveUpButton.ToolTipCaption = null;
			this.GiveUpButton.UseVisualStyleBackColor = true;
			this.GiveUpButton.Click += new System.EventHandler(this.GiveUpButton_Click);
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Label, false);
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 11, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.Label.TabIndex = 1;
			this.Label.Text = "-";
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).ApplicationCode)));
			this.ApplicationCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|42bbf1f6-55b9-4f4f-8575-8376ef3cc9e8", "Application Code");
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.ShowDescriptionBox = false;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.ApplicationCodeDropEdit.TabIndex = 3;
			// 
			// CriteriaCodeDropEdit
			// 
			this.CriteriaCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CriteriaCodeDropEdit, "CriteriaCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).CriteriaCode)));
			this.CriteriaCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|7c33a894-24c0-4fbe-8212-5817a427f2a9", "Criteria Code");
			this.CriteriaCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 45, true);
			this.CriteriaCodeDropEdit.Name = "CriteriaCodeDropEdit";
			this.CriteriaCodeDropEdit.PreBoundMaxLength = 3;
			this.CriteriaCodeDropEdit.ShowDescriptionBox = false;
			this.CriteriaCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CriteriaCodeDropEdit.TabIndex = 4;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).DateFrom)));
			this.FromDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|baa3543a-1423-4ab7-95e0-30c2b005844b", "Entry Accepted From");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 71, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 5;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).DateTo)));
			this.ToDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|c841fb45-1f8b-4873-85a7-30124d45bf57", "To");
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 97, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 7;
			// 
			// DateFromTimeEdit
			// 
			this.DateFromTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DateFromTimeEdit, "TimeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).TimeFrom)));
			this.DateFromTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 71, true);
			this.DateFromTimeEdit.Name = "DateFromTimeEdit";
			this.DateFromTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.DateFromTimeEdit.TabIndex = 6;
			// 
			// TimeToTimeEdit
			// 
			this.TimeToTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TimeToTimeEdit, "TimeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).TimeTo)));
			this.TimeToTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 97, true);
			this.TimeToTimeEdit.Name = "TimeToTimeEdit";
			this.TimeToTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.TimeToTimeEdit.TabIndex = 8;
			// 
			// EntrySummariesCheckBox
			// 
			this.EntrySummariesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EntrySummariesCheckBox, "ConsumptionEntrySummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).ConsumptionEntrySummaries)));
			this.EntrySummariesCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a663a2b6-60b5-423f-b29e-35885898579c", "Entry Summaries");
			this.EntrySummariesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EntrySummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 127, true);
			this.EntrySummariesCheckBox.Name = "EntrySummariesCheckBox";
			this.EntrySummariesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.EntrySummariesCheckBox.TabIndex = 9;
			// 
			// FTAReconSummariesCheckBox
			// 
			this.FTAReconSummariesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FTAReconSummariesCheckBox, "FTAReconSummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).FTAReconSummaries)));
			this.FTAReconSummariesCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4045113a-5040-4351-8c98-d4eac0d20d05", "FTA Reconciliation Summaries");
			this.FTAReconSummariesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FTAReconSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 153, true);
			this.FTAReconSummariesCheckBox.Name = "FTAReconSummariesCheckBox";
			this.FTAReconSummariesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FTAReconSummariesCheckBox.TabIndex = 10;
			// 
			// OtherReconSummariesCheckBox
			// 
			this.OtherReconSummariesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OtherReconSummariesCheckBox, "OtherReconSummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).OtherReconSummaries)));
			this.OtherReconSummariesCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3ba8a43f-d9d5-4efd-b479-f568fe8184c6", "Other Reconciliation Summaries");
			this.OtherReconSummariesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OtherReconSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 179, true);
			this.OtherReconSummariesCheckBox.Name = "OtherReconSummariesCheckBox";
			this.OtherReconSummariesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OtherReconSummariesCheckBox.TabIndex = 11;
			// 
			// DrawbackSummariesCheckBox
			// 
			this.DrawbackSummariesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DrawbackSummariesCheckBox, "DrawbackSummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).DrawbackSummaries)));
			this.DrawbackSummariesCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d7bc02cf-5c85-419f-af6f-743856dfee25", "Drawback Summaries");
			this.DrawbackSummariesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DrawbackSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 205, true);
			this.DrawbackSummariesCheckBox.Name = "DrawbackSummariesCheckBox";
			this.DrawbackSummariesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.DrawbackSummariesCheckBox.TabIndex = 12;
			// 
			// NAFTADutyDeferralSummariesCheckBox
			// 
			this.NAFTADutyDeferralSummariesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NAFTADutyDeferralSummariesCheckBox, "NAFTADutyDeferralSummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).NAFTADutyDeferralSummaries)));
			this.NAFTADutyDeferralSummariesCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6bca3c10-c748-4e3f-9027-6b672beeec6b", "NAFTA Duty Deferral Summaries");
			this.NAFTADutyDeferralSummariesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NAFTADutyDeferralSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 231, true);
			this.NAFTADutyDeferralSummariesCheckBox.Name = "NAFTADutyDeferralSummariesCheckBox";
			this.NAFTADutyDeferralSummariesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NAFTADutyDeferralSummariesCheckBox.TabIndex = 13;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.ApplicationCodeDropEdit);
			this.MainGroupBox.Controls.Add(this.NAFTADutyDeferralSummariesCheckBox);
			this.MainGroupBox.Controls.Add(this.CriteriaCodeDropEdit);
			this.MainGroupBox.Controls.Add(this.DrawbackSummariesCheckBox);
			this.MainGroupBox.Controls.Add(this.FromDateEdit);
			this.MainGroupBox.Controls.Add(this.OtherReconSummariesCheckBox);
			this.MainGroupBox.Controls.Add(this.DateFromTimeEdit);
			this.MainGroupBox.Controls.Add(this.FTAReconSummariesCheckBox);
			this.MainGroupBox.Controls.Add(this.EntrySummariesCheckBox);
			this.MainGroupBox.Controls.Add(this.ToDateEdit);
			this.MainGroupBox.Controls.Add(this.TimeToTimeEdit);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 34, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 285, true);
			this.MainGroupBox.TabIndex = 16;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Text = "Query a group of entries:";
			// 
			// EntrySummaryQueryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 384, true);
			this.Controls.Add(this.MainGroupBox);
			this.Controls.Add(this.Label);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.GiveUpButton);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.FilerTextBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.EntrySummaryQueryBizObj);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.EntrySummaryQueryBizObj";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 115, true);
			this.Name = "EntrySummaryQueryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Entry Summary Query Form";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FilerTextBox, 0);
			this.Controls.SetChildIndex(this.EntryNumberTextBox, 0);
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.Label, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.CriteriaCodeDropEdit.ResumeLayout(true);
			this.CriteriaCodeDropEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void InitializeNewComponent()
		{
			this.EntryNumberQueryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CollectionBillInformationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryNumBerGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo filerTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo entryNumberTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.EntryNumBerGrid)).BeginInit();
			this.EntryNumberQueryGroupBox.SuspendLayout();
			this.CollectionBillInformationCodeDropEdit.SuspendLayout();
			this.EntryNumBerGrid.SuspendLayout();

			this.FilerTextBox.Visible = false;
			this.Label.Visible = false;
			this.EntryNumberTextBox.Visible = false;
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 19, true);
			this.CriteriaCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 45, true);
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 71, true);
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 97, true);
			this.EntrySummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 123, true);
			this.FTAReconSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 149, true);
			this.OtherReconSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 175, true);
			this.DrawbackSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 201, true);
			this.NAFTADutyDeferralSummariesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 227, true);
			this.DateFromTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 71, true);
			this.TimeToTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 97, true);

			// 
			// EntryNumBerGrid
			// 
			this.EntryNumBerGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryNumBerGrid, "EntryNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).EntryNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).EntryNumbers)).SyncRoot)).EntryFilerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(((System.Collections.IList)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).EntryNumbers)).SyncRoot)).EntryNumber)));
			filerTextBoxColumn.ColumnName = "EntryFilerCode";
			filerTextBoxColumn.Caption = "Entry Filer Code";
			filerTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			filerTextBoxColumn.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			entryNumberTextBoxColumn.ColumnName = "EntryNumber";
			entryNumberTextBoxColumn.Caption = "Entry Number";
			entryNumberTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EntryNumBerGrid.CaptionVisible = false;
			this.EntryNumBerGrid.ColumnStyles.Add(filerTextBoxColumn);
			this.EntryNumBerGrid.ColumnStyles.Add(entryNumberTextBoxColumn);
			this.EntryNumBerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryNumBerGrid.GridId = "3F124E80-E517-4F70-96C6-36DCD1D28E3C";
			this.EntryNumBerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryNumBerGrid.LayoutKey = "EntryNumBerGrid";
			this.EntryNumBerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.EntryNumBerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 265, true);
			this.EntryNumBerGrid.Name = "EntryNumBerGrid";
			this.EntryNumBerGrid.TabIndex = 0;
			// 
			// EntryNumberQueryGroupBox
			// 
			this.EntryNumberQueryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 8, true);
			this.EntryNumberQueryGroupBox.Name = "EntryNumberQueryGroupBox";
			this.EntryNumberQueryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 285, true);
			this.EntryNumberQueryGroupBox.TabIndex = 16;
			this.EntryNumberQueryGroupBox.TabStop = false;
			this.EntryNumberQueryGroupBox.Text = "Entry Number(s) to query:";
			this.EntryNumberQueryGroupBox.Controls.Add(this.EntryNumBerGrid);
			// 
			// CollectionBillInformationCodeDropEdit
			// 
			this.CollectionBillInformationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CollectionBillInformationCodeDropEdit, "CollectionBillInformationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.EntrySummaryQueryBizObj)(null)).CollectionBillInformationCode)));
			this.CollectionBillInformationCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntrySummaryQueryForm|61AD375E-7875-4C2A-BE8C-510D9CA9A228", "Collection/Bill Information Code");
			this.CollectionBillInformationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 257, true);
			this.CollectionBillInformationCodeDropEdit.Name = "CollectionBillInformationCodeDropEdit";
			this.CollectionBillInformationCodeDropEdit.PreBoundMaxLength = 2;
			this.CollectionBillInformationCodeDropEdit.ShowDescriptionBox = false;
			this.CollectionBillInformationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.CollectionBillInformationCodeDropEdit.TabIndex = 3;

			this.MainGroupBox.Controls.Add(this.CollectionBillInformationCodeDropEdit);
			this.MainGroupBox.Text = "Or query a group of entries:";
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 8, true);
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 285, true);

			this.Controls.Add(this.EntryNumberQueryGroupBox);
			this.Controls.SetChildIndex(this.EntryNumberQueryGroupBox, 0);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 384, true);
			this.CollectionBillInformationCodeDropEdit.ResumeLayout(true);
			this.CollectionBillInformationCodeDropEdit.PerformLayout();
			this.EntryNumberQueryGroupBox.ResumeLayout(false);
			this.EntryNumberQueryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryNumBerGrid)).EndInit();
			this.EntryNumBerGrid.ResumeLayout(false);
			this.EntryNumBerGrid.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox FilerTextBox;
		private Enterprise.ZArchitecture.ZTextBox EntryNumberTextBox;
		public Enterprise.ZArchitecture.GUI.ZButton SendButton;
		public Enterprise.ZArchitecture.GUI.ZButton GiveUpButton;
		private Enterprise.ZArchitecture.ZLabel Label;
		private ZArchitecture.GUI.ZDropEdit ApplicationCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CriteriaCodeDropEdit;
		private ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private ZArchitecture.GUI.ZTimeEdit DateFromTimeEdit;
		private ZArchitecture.GUI.ZTimeEdit TimeToTimeEdit;
		private ZArchitecture.GUI.ZCheckBox EntrySummariesCheckBox;
		private ZArchitecture.GUI.ZCheckBox FTAReconSummariesCheckBox;
		private ZArchitecture.GUI.ZCheckBox OtherReconSummariesCheckBox;
		private ZArchitecture.GUI.ZCheckBox DrawbackSummariesCheckBox;
		private ZArchitecture.GUI.ZCheckBox NAFTADutyDeferralSummariesCheckBox;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZDropEdit CollectionBillInformationCodeDropEdit;
		private ZArchitecture.GUI.ZGroupBox EntryNumberQueryGroupBox;
		private Enterprise.ZArchitecture.ZGrid EntryNumBerGrid;
	}
}
