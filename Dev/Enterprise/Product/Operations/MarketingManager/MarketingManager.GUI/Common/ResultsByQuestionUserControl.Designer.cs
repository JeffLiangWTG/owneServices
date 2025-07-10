namespace Enterprise.MarketingManager.GUI
{
	partial class ResultsByQuestionUserControl
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
			this.ResultsByItemsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AnswersTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AnswersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResultItemAnswersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ResultsByItemsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByItemsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByItemsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).BeginInit();
			this.AnswersTabControl.SuspendLayout();
			this.AnswersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
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
			this.ResultsByItemsSplitContainer.Panel1.Controls.Add(this.ResultItemsGrid);
			// 
			// ResultsByItemsSplitContainer.Panel2
			// 
			this.ResultsByItemsSplitContainer.Panel2.Controls.Add(this.AnswersTabControl);
			this.ResultsByItemsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 537, true);
			this.ResultsByItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			this.ResultsByItemsSplitContainer.TabIndex = 1;
			// 
			// ResultItemsGrid
			// 
			this.ResultItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResultItemsGrid, "ActualQuestionsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).ActualOrderForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).HY_Question)));
			this.ResultItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByQuestionUserControl|f8dd5f74-0edb-4551-9df7-7d0deca84354", "No.");
			zTextBoxColumnStyleInfo1.ColumnName = "ActualOrderForBinding";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.ColumnName = "HY_Question";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.ResultItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResultItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ResultItemsGrid.GridId = "9e1f158c-9574-4512-92f8-7aab78c0244e";
			this.ResultItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResultItemsGrid.IsWholeRowSelectedOnClick = true;
			this.ResultItemsGrid.LayoutKey = "gridResultQuestions";
			this.ResultItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultItemsGrid.Name = "ResultItemsGrid";
			this.ResultItemsGrid.ReadOnly = true;
			this.ResultItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 275, true);
			this.ResultItemsGrid.TabIndex = 1;
			// 
			// AnswersTabControl
			// 
			this.AnswersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AnswersTabControl.Controls.Add(this.AnswersTabPage);
			this.AnswersTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnswersTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnswersTabControl.Name = "AnswersTabControl";
			this.AnswersTabControl.SelectedIndex = 0;
			this.AnswersTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 258, true);
			this.AnswersTabControl.TabIndex = 2;
			// 
			// AnswersTabPage
			// 
			this.AnswersTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByQuestionUserControl|ae76baa6-39c8-4363-a0af-15505d6465d4", "Answers");
			this.AnswersTabPage.Controls.Add(this.ResultItemAnswersGrid);
			this.AnswersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AnswersTabPage.Name = "AnswersTabPage";
			this.AnswersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AnswersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 231, true);
			this.AnswersTabPage.TabIndex = 0;
			this.AnswersTabPage.UseVisualStyleBackColor = true;
			// 
			// ResultItemAnswersGrid
			// 
			this.ResultItemAnswersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResultItemAnswersGrid, "ActualQuestionsForBinding.SubmittedAnswers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).SubmittedAnswers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySubmittedAnswer)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).SubmittedAnswers)).SyncRoot)).CampaignItem.ContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySubmittedAnswer)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).ActualQuestionsForBinding)).SyncRoot)).SubmittedAnswers)).SyncRoot)).CampaignItem.EmailAddress)));
			this.ResultItemAnswersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByQuestionUserControl|c241bf95-ee45-4e19-b3a8-dca0948694f2", "Contact Name");
			zTextBoxColumnStyleInfo3.ColumnName = "CampaignItem+ContactName";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByQuestionUserControl|ffca67b3-2e0a-428a-a806-201dd6351a8c", "Email", "Email Address", "");
			zTextBoxColumnStyleInfo4.ColumnName = "CampaignItem+EmailAddress";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ResultItemAnswersGrid.GridId = "1c134ff0-fa62-45d5-8bbf-64ecb99bcd62";
			this.ResultItemAnswersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultItemAnswersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResultItemAnswersGrid.LayoutKey = "gridCampaignItemAnswers";
			this.ResultItemAnswersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ResultItemAnswersGrid.Name = "ResultItemAnswersGrid";
			this.ResultItemAnswersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 225, true);
			this.ResultItemAnswersGrid.TabIndex = 2;
			// 
			// ResultsByQuestionUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ResultsByItemsSplitContainer);
			this.Name = "ResultsByQuestionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResultsByItemsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).EndInit();
			this.AnswersTabControl.ResumeLayout(false);
			this.AnswersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer ResultsByItemsSplitContainer;
		protected Enterprise.ZArchitecture.ZGrid ResultItemsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabControl AnswersTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AnswersTabPage;
		protected Enterprise.ZArchitecture.ZGrid ResultItemAnswersGrid;
	}
}
