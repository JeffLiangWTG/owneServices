namespace Enterprise.MarketingManager.GUI
{
	partial class ResultsByRecipientUserControl
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
			this.ResultsByRecipientsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AnswersTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AnswersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RecipientAnswersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ResultsByRecipientsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByRecipientsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByRecipientsSplitContainer.SuspendLayout();
			this.AnswersTabControl.SuspendLayout();
			this.AnswersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientAnswersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// ResultsByRecipientsSplitContainer
			// 
			this.ResultsByRecipientsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsByRecipientsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsByRecipientsSplitContainer.Name = "ResultsByRecipientsSplitContainer";
			this.ResultsByRecipientsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ResultsByRecipientsSplitContainer.Panel2
			// 
			this.ResultsByRecipientsSplitContainer.Panel2.Controls.Add(this.AnswersTabControl);
			this.ResultsByRecipientsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 426, true);
			this.ResultsByRecipientsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(228);
			this.ResultsByRecipientsSplitContainer.TabIndex = 1;
			// 
			// AnswersTabControl
			// 
			this.AnswersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AnswersTabControl.Controls.Add(this.AnswersTabPage);
			this.AnswersTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnswersTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AnswersTabControl.Name = "AnswersTabControl";
			this.AnswersTabControl.SelectedIndex = 0;
			this.AnswersTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 194, true);
			this.AnswersTabControl.TabIndex = 2;
			// 
			// AnswersTabPage
			// 
			this.AnswersTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("ResultsByRecipientUserControl|5f44e18e-32da-4d99-86c8-b4237ce6babf", "Answers");
			this.AnswersTabPage.Controls.Add(this.RecipientAnswersGrid);
			this.AnswersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AnswersTabPage.Name = "AnswersTabPage";
			this.AnswersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AnswersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 167, true);
			this.AnswersTabPage.TabIndex = 0;
			this.AnswersTabPage.UseVisualStyleBackColor = true;
			// 
			// RecipientAnswersGrid
			// 
			this.RecipientAnswersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecipientAnswersGrid, "CampaignsItemsSentForDisplayOnly.SubmittedAnswers");
			//The line(s) below are a compile-time check for a binding member.Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSentForDisplayOnly)).SyncRoot)).SubmittedAnswers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySubmittedAnswer)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSentForDisplayOnly)).SyncRoot)).SubmittedAnswers)).SyncRoot)).Question.ActualOrderForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveySubmittedAnswer)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignItem)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).CampaignsItemsSentForDisplayOnly)).SyncRoot)).SubmittedAnswers)).SyncRoot)).Question.HY_Question)));
			this.RecipientAnswersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("2c9798f6-6b8e-4916-8964-80b4483c775b", "Question");
			zTextBoxColumnStyleInfo3.ColumnName = "Question+ActualOrderForBinding";
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo4.ColumnName = "Question+HY_Question";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.RecipientAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RecipientAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RecipientAnswersGrid.GridId = "d896412d-1d4d-4fec-81b9-996bc6052611";
			this.RecipientAnswersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientAnswersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientAnswersGrid.LayoutKey = "AnswersGrid";
			this.RecipientAnswersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RecipientAnswersGrid.Name = "RecipientAnswersGrid";
			this.RecipientAnswersGrid.ReadOnly = true;
			this.RecipientAnswersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 161, true);
			this.RecipientAnswersGrid.TabIndex = 2;
			// 
			// ResultsByRecipientUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ResultsByRecipientsSplitContainer);
			this.Name = "ResultsByRecipientUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 426, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResultsByRecipientsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByRecipientsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByRecipientsSplitContainer.ResumeLayout(false);
			this.AnswersTabControl.ResumeLayout(false);
			this.AnswersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RecipientAnswersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer ResultsByRecipientsSplitContainer;
		protected Enterprise.ZArchitecture.GUI.ZTabControl AnswersTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage AnswersTabPage;
		protected Enterprise.ZArchitecture.ZGrid RecipientAnswersGrid;
	}
}
