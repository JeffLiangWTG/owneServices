namespace Enterprise.MarketingManager.GUI
{
	partial class VoteExamSurveyUserControl
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
		void InitializeComponent()
		{
			this.VoteExamSurveyTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.QuestionsSetupTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QuestionsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ResultsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResultsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ResultsByRecipientsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResultsByItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ResultsSummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VoteExamSurveyTabControl.SuspendLayout();
			this.QuestionsSetupTabPage.SuspendLayout();
			this.QuestionsSplitContainer.SuspendLayout();
			this.ResultsTabPage.SuspendLayout();
			this.ResultsTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// VoteExamSurveyTabControl
			// 
			this.VoteExamSurveyTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.VoteExamSurveyTabControl.Controls.Add(this.QuestionsSetupTabPage);
			this.VoteExamSurveyTabControl.Controls.Add(this.ResultsTabPage);
			this.VoteExamSurveyTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VoteExamSurveyTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VoteExamSurveyTabControl.Name = "VoteExamSurveyTabControl";
			this.VoteExamSurveyTabControl.SelectedIndex = 0;
			this.VoteExamSurveyTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 563, true);
			this.VoteExamSurveyTabControl.TabIndex = 2;
			// 
			// QuestionsSetupTabPage
			// 
			this.QuestionsSetupTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteExamSurveyUserControl|5c08c0c7-fc41-4550-8deb-703ad8ecc201", "Setup");
			this.QuestionsSetupTabPage.Controls.Add(this.QuestionsSplitContainer);
			this.QuestionsSetupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.QuestionsSetupTabPage.Name = "QuestionsSetupTabPage";
			this.QuestionsSetupTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.QuestionsSetupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 536, true);
			this.QuestionsSetupTabPage.TabIndex = 0;
			this.QuestionsSetupTabPage.UseVisualStyleBackColor = true;
			// 
			// QuestionsSplitContainer
			// 
			this.QuestionsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuestionsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.QuestionsSplitContainer.Name = "QuestionsSplitContainer";
			this.QuestionsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.QuestionsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 530, true);
			this.QuestionsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(386);
			this.QuestionsSplitContainer.TabIndex = 21;
			// 
			// ResultsTabPage
			// 
			this.ResultsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteExamSurveyUserControl|d47b9063-417d-4111-b9e1-5f3f068dd26f", "Results");
			this.ResultsTabPage.Controls.Add(this.ResultsTabControl);
			this.ResultsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ResultsTabPage.Name = "ResultsTabPage";
			this.ResultsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ResultsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 536, true);
			this.ResultsTabPage.TabIndex = 1;
			this.ResultsTabPage.UseVisualStyleBackColor = true;
			// 
			// ResultsTabControl
			// 
			this.ResultsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ResultsTabControl.Controls.Add(this.ResultsByRecipientsTabPage);
			this.ResultsTabControl.Controls.Add(this.ResultsByItemsTabPage);
			this.ResultsTabControl.Controls.Add(this.ResultsSummaryTabPage);
			this.ResultsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ResultsTabControl.Name = "ResultsTabControl";
			this.ResultsTabControl.SelectedIndex = 0;
			this.ResultsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 530, true);
			this.ResultsTabControl.TabIndex = 0;
			// 
			// ResultsByRecipientsTabPage
			// 
			this.ResultsByRecipientsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteExamSurveyUserControl|84c9aeed-eb1f-4822-a64f-aaf4be5bff1e", "By Recipients");
			this.ResultsByRecipientsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ResultsByRecipientsTabPage.Name = "ResultsByRecipientsTabPage";
			this.ResultsByRecipientsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ResultsByRecipientsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 503, true);
			this.ResultsByRecipientsTabPage.TabIndex = 0;
			this.ResultsByRecipientsTabPage.UseVisualStyleBackColor = true;
			// 
			// ResultsByItemsTabPage
			// 
			this.ResultsByItemsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteExamSurveyUserControl|cdd7c483-05a9-4aae-94c7-fe4a425f4f43", "By Items");
			this.ResultsByItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ResultsByItemsTabPage.Name = "ResultsByItemsTabPage";
			this.ResultsByItemsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ResultsByItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 503, true);
			this.ResultsByItemsTabPage.TabIndex = 1;
			this.ResultsByItemsTabPage.UseVisualStyleBackColor = true;
			// 
			// ResultsSummaryTabPage
			// 
			this.ResultsSummaryTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteExamSurveyUserControl|fe78be1f-c81b-4389-a471-81acc7b809cf", "Summary", "Summary", "Summary", "");
			this.ResultsSummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ResultsSummaryTabPage.Name = "ResultsSummaryTabPage";
			this.ResultsSummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ResultsSummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 503, true);
			this.ResultsSummaryTabPage.TabIndex = 2;
			this.ResultsSummaryTabPage.UseVisualStyleBackColor = true;
			// 
			// VoteExamSurveyUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VoteExamSurveyTabControl);
			this.Name = "VoteExamSurveyUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VoteExamSurveyTabControl.ResumeLayout(false);
			this.QuestionsSetupTabPage.ResumeLayout(false);
			this.QuestionsSplitContainer.ResumeLayout(false);
			this.ResultsTabPage.ResumeLayout(false);
			this.ResultsTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZTabPage ResultsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabControl ResultsTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ResultsByRecipientsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ResultsByItemsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabControl VoteExamSurveyTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage QuestionsSetupTabPage;
		protected CargoWise.Windows.UI.KSplitContainer QuestionsSplitContainer;
		protected ZArchitecture.GUI.ZTabPage ResultsSummaryTabPage;
	}
}
