namespace Enterprise.Recruiter.GUI
{
	partial class ScaledTestQuestionsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			this.AssessmentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			this.SetupDetailsPanel.SuspendLayout();
			this.QuestionsTabControl.SuspendLayout();
			this.ActiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).BeginInit();
			this.InactiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AssessmentTabPage.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.SuspendLayout();
			// 
			// SetupDetailsPanel
			// 
			this.SetupDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 29, true);
			this.SetupDetailsPanel.TabIndex = 0;
			// 
			// QuestionsPerPageCalcEdit
			// 
			this.QuestionsPerPageCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.QuestionsPerPageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(822, 6, true);
			this.QuestionsPerPageCalcEdit.TabIndex = 0;
			// 
			// PreviewButton
			// 
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 3, true);
			// 
			// QuestionsTabControl
			// 
			this.QuestionsTabControl.Controls.Add(this.AssessmentTabPage);
			this.QuestionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 599, true);
			this.QuestionsTabControl.TabIndex = 1;
			this.QuestionsTabControl.Controls.SetChildIndex(this.AssessmentTabPage, 0);
			this.QuestionsTabControl.Controls.SetChildIndex(this.InactiveQuestionsTabPage, 0);
			this.QuestionsTabControl.Controls.SetChildIndex(this.ActiveQuestionsTabPage, 0);
			// 
			// ActiveQuestionsTabPage
			// 
			this.ActiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 572, true);
			// 
			// QuestionsGrid
			// 
			zDropEditColumnStyleInfo1.ColumnName = "HY_QuestionCategory";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.ColumnName = "HY_IsRandomisable";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HY_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDynamicMultilineTextBoxColumnStyleInfo2.ColumnName = "HY_Comment";
			zDynamicMultilineTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("HRJobSkillForm|efc46d58-6eb8-4919-849c-f686b3b0a86b", "Comment");
			zDynamicMultilineTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.QuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo2);
			this.QuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 566, true);
			// 
			// InactiveQuestionsTabPage
			// 
			this.InactiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 572, true);
			// 
			// InactiveQuestionsGrid
			// 
			zDropEditColumnStyleInfo2.ColumnName = "HY_QuestionCategory";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.InactiveQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InactiveQuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo2);
			this.InactiveQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 566, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreCampaign);
			// 
			// AssessmentTabPage
			// 
			this.AssessmentTabPage.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ScaledTestQuestionsUserControl|784c81a2-ab75-4011-9e34-bfaaeb24c216", "Assessment");
			this.AssessmentTabPage.Controls.Add(this.splitContainer1);
			this.AssessmentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AssessmentTabPage.Name = "AssessmentTabPage";
			this.AssessmentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 572, true);
			this.AssessmentTabPage.TabIndex = 2;
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
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGroupBox2);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 572, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			this.splitContainer1.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ScaledTestQuestionsUserControl|006468c6-cb71-40a2-9a7b-7e12e59d7469", "Question Categories");
			this.zGroupBox1.Controls.Add(this.zGrid1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 123, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "QuestionCategories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).Description)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.GridId = "13e88566-3cee-4d50-b3d1-581c17784525";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 104, true);
			this.zGrid1.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ScaledTestQuestionsUserControl|c31c7e03-66da-47af-a742-23572668b4a5", "Scale Ranges");
			this.zGroupBox2.Controls.Add(this.zGrid2);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 445, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid2, "QuestionCategories.ScaleRanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).ScaleRanges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.LearningCentreQuestion)(((System.Collections.IList)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).ScaleRanges)).SyncRoot)).HY_Min)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.LearningCentreQuestion)(((System.Collections.IList)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).ScaleRanges)).SyncRoot)).HY_Max)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.LearningCentreQuestion)(((System.Collections.IList)(((Enterprise.Recruiter.Business.QuestionCategory)(((System.Collections.IList)(((Enterprise.Recruiter.Business.LearningCentreCampaign)(null)).QuestionCategories)).SyncRoot)).ScaleRanges)).SyncRoot)).HY_Question)));
			this.zGrid2.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ScaledTestQuestionsUserControl|ad2064be-9a4a-41f3-a67a-128064abd3ad", "Lower");
			zCalcEditColumnStyleInfo1.ColumnName = "HY_Min";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ScaledTestQuestionsUserControl|821ce1c8-5ee8-47fe-91c8-bd3f8fc07943", "Upper");
			zCalcEditColumnStyleInfo2.ColumnName = "HY_Max";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDynamicMultilineTextBoxColumnStyleInfo1.ColumnName = "HY_QuestionMultilingual";
			zDynamicMultilineTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("HRJobSkillForm|EB1F2E85-729D-4925-A536-EE9CA59E2099", " ");
			zDynamicMultilineTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid2.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			this.zGrid2.GridId = "9f7a6ff3-3317-4375-b8bc-d319ad0bc789";
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid2";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 426, true);
			this.zGrid2.TabIndex = 0;
			// 
			// ScaledTestQuestionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ScaledTestQuestionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 628, true);
			this.SetupDetailsPanel.ResumeLayout(false);
			this.SetupDetailsPanel.PerformLayout();
			this.QuestionsTabControl.ResumeLayout(false);
			this.ActiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).EndInit();
			this.InactiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AssessmentTabPage.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGroupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage AssessmentTabPage;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private Enterprise.ZArchitecture.ZGrid zGrid1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.ZGrid zGrid2;
	}
}
