using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	partial class LearningCentreCampaignForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.examSettingsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.groupBoxNoText = new Enterprise.Recruiter.GUI.LearningCentreCampaignForm.ZGroupBoxNoText();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.examSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.groupBoxRelatedJobTests = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			// this.skillTestsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncidentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CampaignNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.groupBoxNoText.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.examSettingsGrid)).BeginInit();
			this.examSettingsGrid.SuspendLayout();
			this.groupBoxRelatedJobTests.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zCodeFindBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 515, true);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 515, true);
			this.MainTabControl.TabIndex = 0;
			//
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.splitContainer1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 493, true);
			//
			// NotesTabPage
			//
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 493, true);
			//
			// LogsTabPage
			//
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 493, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 515, true);
			//
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			//
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 24, true);
			this.MainStatusBar.TabIndex = 0;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreCampaign);
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this.zGroupBox1);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 493, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(133);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this.CommentTextBox);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(295);
			this.splitContainer1.TabIndex = 1;
			//
			// zGroupBox1
			//
			this.zGroupBox1.Controls.Add(this.examSettingsLabel);
			this.zGroupBox1.Controls.Add(this.groupBoxNoText);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.IncidentNumberTextBox);
			this.zGroupBox1.Controls.Add(this.CampaignNameTextBox);
			this.zGroupBox1.Controls.Add(this.zCodeFindBox1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 295, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			//
			// examSettingsLabel
			//
			this.examSettingsLabel.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("d3202335-ed46-404f-9170-a80c5f0cd313", "Exam Settings");
			this.examSettingsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.examSettingsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 95, true);
			this.examSettingsLabel.Name = "examSettingsLabel";
			this.examSettingsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.examSettingsLabel.TabIndex = 0;
			//
			// groupBoxNoText
			//
			this.groupBoxNoText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxNoText.Controls.Add(this.splitContainer2);
			this.groupBoxNoText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 89, true);
			this.groupBoxNoText.Name = "groupBoxNoText";
			this.groupBoxNoText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 202, true);
			this.groupBoxNoText.TabIndex = 0;
			this.groupBoxNoText.TabStop = false;
			//
			// splitContainer2
			//
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.splitContainer2.Name = "splitContainer2";
			//
			// splitContainer2.Panel1
			//
			this.splitContainer2.Panel1.Controls.Add(this.examSettingsGrid);
			//
			// splitContainer2.Panel2
			//
			this.splitContainer2.Panel2.Controls.Add(this.groupBoxRelatedJobTests);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 185, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(760);
			this.splitContainer2.TabIndex = 1;
			//
			// examSettingsGrid
			//
			this.examSettingsGrid.AllowNavigation = false;
			this.examSettingsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.examSettingsGrid, "ExamSettingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_ExamVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_IsDefault)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_ExamExpiryTimeInMinutes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_MaximumAskedQuestionsPerExam)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).EXS_TestResultsExpireAfterHours)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.ExamSetting)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).ExamSettingCollection)).SyncRoot)).TestResultsExpireAfterDays)));
			this.examSettingsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("95130784-f42b-42f9-b4e2-4bdb151fd2c9", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "EXS_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("62e04ffd-50cd-4f51-bc62-68ab2023fa39", "Version");
			zDropEditColumnStyleInfo1.ColumnName = "EXS_ExamVersion";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(44);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("bf4f64c1-8837-4278-bfbf-e7750b481623", "Is Web Default");
			zCheckBoxColumnStyleInfo1.ColumnName = "EXS_IsDefault";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("18a99057-8edd-4486-9631-cec7b68ab47c", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EXS_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("768118bf-589e-41c5-b3d1-634c3d582b43", "Exam Expiry (min)");
			zCalcEditColumnStyleInfo1.ColumnName = "EXS_ExamExpiryTimeInMinutes";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("b359f37c-a2c1-4b48-822f-0a0de712f21e", "Max Questions");
			zCalcEditColumnStyleInfo2.ColumnName = "EXS_MaximumAskedQuestionsPerExam";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("3ae4d2c2-6d9e-4081-8d3b-3893c6230edb", "Results Expiry (hours)");
			zCalcEditColumnStyleInfo3.ColumnName = "EXS_TestResultsExpireAfterHours";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ec68ae0f-bcd3-4c33-9935-0ae4ea0b7915", "Results Expiry (days)");
			zCalcEditColumnStyleInfo4.ColumnName = "TestResultsExpireAfterDays";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.examSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.examSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.examSettingsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.examSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.examSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.examSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.examSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.examSettingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.examSettingsGrid.GridId = "a2184550-1dde-45d2-8006-9d85ecf95d0c";
			this.examSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.examSettingsGrid.LayoutKey = "examSettingsGrid";
			this.examSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.examSettingsGrid.Name = "examSettingsGrid";
			this.examSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 182, true);
			this.examSettingsGrid.TabIndex = 1;
			//
			// groupBoxRelatedJobTests
			//
			this.groupBoxRelatedJobTests.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|7fcba3f6-4059-4ff3-af1b-9e2e563a8c00", "Related Job Exams");
			this.groupBoxRelatedJobTests.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxRelatedJobTests.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxRelatedJobTests.Name = "groupBoxRelatedJobTests";
			this.groupBoxRelatedJobTests.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 185, true);
			this.groupBoxRelatedJobTests.TabIndex = 0;
			this.groupBoxRelatedJobTests.TabStop = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|9515f388-e887-45c4-a625-25c3dc4ecfc1", "Skill Code");
			zTextBoxColumnStyleInfo3.ColumnName = "JobSkillCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|bb7d329c-2688-463d-9637-9ad45fefe9ad", "Skill Description");
			zTextBoxColumnStyleInfo4.ColumnName = "JobSkillDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.ColumnName = "HT_TestName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.ColumnName = "HT_TestDetail";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			//
			// zDropEdit1
			//
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "G0_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).G0_Type)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|91772cba-842f-4cd8-964b-c2eab6e35de8", "Type");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 68, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.zDropEdit1.TabIndex = 3;
			//
			// IncidentNumberTextBox
			//
			this.IncidentNumberTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IncidentNumberTextBox, "G0_CampaignID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).G0_CampaignID)));
			this.IncidentNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.IncidentNumberTextBox.ForeColor = System.Drawing.Color.MediumBlue;
			this.IncidentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 2, true);
			this.IncidentNumberTextBox.Name = "IncidentNumberTextBox";
			this.IncidentNumberTextBox.ReadOnly = true;
			this.IncidentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.IncidentNumberTextBox.TabIndex = 0;
			this.IncidentNumberTextBox.Text = "<CAMPAIGNID>";
			//
			// CampaignNameTextBox
			//
			this.CampaignNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CampaignNameTextBox, "G0_CampaignName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).G0_CampaignName)));
			this.CampaignNameTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|8631c3e2-8b0c-47f0-95b3-44247b8e5ac1", "Name");
			this.CampaignNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CampaignNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 16, true);
			this.CampaignNameTextBox.Name = "CampaignNameTextBox";
			this.CampaignNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 17, true);
			this.CampaignNameTextBox.TabIndex = 1;
			//
			// zCodeFindBox1
			//
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "G0_GS_NKCampaignCoordinator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).G0_GS_NKCampaignCoordinator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).Lookups.CampaignCoordinators)));
			this.zCodeFindBox1.BindToList = "Lookups+CampaignCoordinators";
			this.zCodeFindBox1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|a9bc2d14-2939-4319-be71-76979e290fc0", "Coordinator");
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 42, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.PreBoundMaxLength = 3;
			this.zCodeFindBox1.ShouldResize = true;
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.zCodeFindBox1.TabIndex = 2;
			//
			// CommentTextBox
			//
			this.CommentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommentTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.CommentTextBox, "G0_CampaignComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).G0_CampaignComment)));
			this.CommentTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("LearningCentreCampaignForm|fa01eac4-cc87-4a10-9709-2e51586bef4a", "Additional Note");
			this.CommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.CommentTextBox, 1);
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 9, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 175, true);
			this.CommentTextBox.TabIndex = 7;
			//
			// LearningCentreCampaignForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 571, true);
			this.DataSourceAssemblyName = "Enterprise.Recruiter.Business";
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreCampaign);
			this.DataSourceTypeName = "Enterprise.Recruiter.Business.HRJobSkillExamCampaign";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 609, true);
			this.Name = "LearningCentreCampaignForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "LearningCentreCampaignForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.groupBoxNoText.ResumeLayout(false);
			this.groupBoxNoText.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.examSettingsGrid)).EndInit();
			this.examSettingsGrid.ResumeLayout(false);
			this.examSettingsGrid.PerformLayout();
			this.groupBoxRelatedJobTests.ResumeLayout(false);
			this.groupBoxRelatedJobTests.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zCodeFindBox1.ResumeLayout(true);
			this.zCodeFindBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.ZTextBox IncidentNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox CommentTextBox;
		private Enterprise.ZArchitecture.ZTextBox CampaignNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxRelatedJobTests;
		private ZGroupBoxNoText groupBoxNoText;
		private ZLabel examSettingsLabel;
		private ZGrid examSettingsGrid;
	}
}
