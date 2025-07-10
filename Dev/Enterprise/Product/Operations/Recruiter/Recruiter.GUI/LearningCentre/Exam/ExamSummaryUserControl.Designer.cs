namespace Enterprise.Recruiter.GUI
{
	partial class ExamSummaryUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ResultsByItemsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.QuestionItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SummaryTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResultItemAnswersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SummaryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ResultsByItemsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByItemsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByItemsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuestionItemsGrid)).BeginInit();
			this.SummaryTabControl.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreCampaign);

			// 
			// ResultsByItemsSplitContainer
			// 
			this.ResultsByItemsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsByItemsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsByItemsSplitContainer.Name = "ResultsByItemsSplitContainer";
			this.ResultsByItemsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ResultsByItemsSplitContainer.Panel1
			// 
			this.ResultsByItemsSplitContainer.Panel1.Controls.Add(this.QuestionItemsGrid);
			// 
			// ResultsByItemsSplitContainer.Panel2
			// 
			this.ResultsByItemsSplitContainer.Panel2.Controls.Add(this.SummaryTabControl);
			this.ResultsByItemsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 537, true);
			this.ResultsByItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			this.ResultsByItemsSplitContainer.TabIndex = 1;
			// 
			// ResultItemsGrid
			// 
			this.QuestionItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuestionItemsGrid, "ActualQuestionsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).ActualOrderForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).HY_Question)));
			this.QuestionItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|f8dd5f74-0edb-4551-9df7-7d0deca84354", "No.");
			zTextBoxColumnStyleInfo1.ColumnName = "ActualOrderForBinding";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.ColumnName = "HY_Question";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.QuestionItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuestionItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuestionItemsGrid.GridId = "9e1f158c-9574-4512-92f8-7aab78c0244e";
			this.QuestionItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuestionItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuestionItemsGrid.IsWholeRowSelectedOnClick = true;
			this.QuestionItemsGrid.LayoutKey = "gridResultQuestions";
			this.QuestionItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuestionItemsGrid.Name = "QuestionItemsGrid";
			this.QuestionItemsGrid.ReadOnly = true;
			this.QuestionItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 275, true);
			this.QuestionItemsGrid.TabIndex = 1;

			// 
			// SummaryTabControl
			// 
			this.SummaryTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SummaryTabControl.Controls.Add(this.SummaryTabPage);
			this.SummaryTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SummaryTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryTabControl.Name = "SummaryTabControl";
			this.SummaryTabControl.SelectedIndex = 0;
			this.SummaryTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 100, true);
			this.SummaryTabControl.TabIndex = 2;

			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|ae76baa6-39c8-4363-a0af-15505d6465d4", "Summaries");
			this.SummaryTabPage.Controls.Add(this.ResultItemAnswersGrid);
			this.SummaryTabPage.Controls.Add(this.SummaryGrid);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 100, true);
			this.SummaryTabPage.TabIndex = 0;
			this.SummaryTabPage.UseVisualStyleBackColor = true;

			//this.ShowSummaryButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SummaryGrid
			// 
			this.SummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SummaryGrid, "ActualQuestionsForBinding.VoteExamSurveySummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).QuestionText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfRecipientAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfRecipientRepliedAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).RecipientAnsweredAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).RecipientSkippedAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).AverageAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).AverageAsPercentageAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySummary)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).VoteExamSurveySummaries)).SyncRoot)).NumberOfAnswersAsText))); this.SummaryGrid.CaptionVisible = false;
			this.SummaryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|6425bf63-f0f5-45e1-b0a2-fa20f550bd2c", "Recipients");
			zTextBoxColumnStyleInfo3.ColumnName = "NumberOfRecipientAsText";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|df79e3c0-937e-4907-b3c3-b7f0a9ca9e44", "Replied");
			zTextBoxColumnStyleInfo4.ColumnName = "NumberOfRecipientRepliedAsText";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|4687e9bc-22e3-4953-b968-c97bde99415e", "Answered");
			zTextBoxColumnStyleInfo5.ColumnName = "RecipientAnsweredAsText";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|c55e386d-ca5a-48f3-b92c-3c99b760ccc3", "Skipped");
			zTextBoxColumnStyleInfo6.ColumnName = "RecipientSkippedAsText";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|b04d0617-8fb5-4cd7-bcd1-da398f74b846", "Average");
			zTextBoxColumnStyleInfo7.ColumnName = "AverageAsText";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|476f9c13-7951-40bc-80a2-001f80216008", "Average %");
			zTextBoxColumnStyleInfo8.ColumnName = "AverageAsPercentageAsText";
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);

			this.SummaryGrid.GridId = "0171a6c1-b789-4b0a-be9f-c0299771fd5e";
			this.SummaryGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.SummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SummaryGrid.LayoutKey = "gridSummary";
			this.SummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 3, true);
			this.SummaryGrid.Name = "SummaryGrid";
			this.SummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 100, true);
			this.SummaryGrid.TabIndex = 2;

			// 
			// ResultItemAnswersGrid
			// 
			this.ResultItemAnswersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResultItemAnswersGrid, "ActualQuestionsForBinding.VoteSurveyAnswerSummaries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.ResultItemAnswersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|c241bf95-ee45-4e19-b3a8-dca0948694f2", "Answered Option");
			zTextBoxColumnStyleInfo9.ColumnName = "OptionNumberText";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|ffca67b3-2e0a-428a-a806-201dd6351a8c", "Answer Text");
			zTextBoxColumnStyleInfo10.ColumnName = "AnswerText";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(700);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSummaryUserControl|3ee19823-f066-4e45-b813-800bd52848a1", "Answered Count");
			zTextBoxColumnStyleInfo11.ColumnName = "AnsweredCount";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);

			this.ResultItemAnswersGrid.ReadOnly = true;
			this.ResultItemAnswersGrid.GridId = "cb0d0209-8254-44ee-84b3-786ea3d1ada6";
			this.ResultItemAnswersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultItemAnswersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResultItemAnswersGrid.LayoutKey = "gridResultItemAnswers";
			this.ResultItemAnswersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 110, true);
			this.ResultItemAnswersGrid.Name = "ResultItemAnswersGrid";
			this.ResultItemAnswersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 150, true);
			this.ResultItemAnswersGrid.TabIndex = 3;
			// 
			// ExamSummaryUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ResultsByItemsSplitContainer);
			this.Name = "ExamSummaryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResultsByItemsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QuestionItemsGrid)).EndInit();
			this.SummaryTabControl.ResumeLayout(false);
			this.SummaryTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SummaryGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer ResultsByItemsSplitContainer;
		protected Enterprise.ZArchitecture.ZGrid QuestionItemsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabControl SummaryTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage SummaryTabPage;
		protected Enterprise.ZArchitecture.ZGrid ResultItemAnswersGrid;
		protected Enterprise.ZArchitecture.ZGrid SummaryGrid;
	}
}
