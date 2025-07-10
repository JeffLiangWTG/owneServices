namespace Enterprise.MarketingManager.GUI
{
	partial class QuestionsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SetupDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.QuestionsPerPageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QuestionsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ActiveQuestionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QuestionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InactiveQuestionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InactiveQuestionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SetupDetailsPanel.SuspendLayout();
			this.QuestionsTabControl.SuspendLayout();
			this.ActiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).BeginInit();
			this.InactiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaign);
			// 
			// SetupDetailsPanel
			// 
			this.SetupDetailsPanel.Controls.Add(this.QuestionsPerPageCalcEdit);
			this.SetupDetailsPanel.Controls.Add(this.PreviewButton);
			this.SetupDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SetupDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SetupDetailsPanel.Name = "SetupDetailsPanel";
			this.SetupDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 29, true);
			this.SetupDetailsPanel.TabIndex = 7;
			// 
			// QuestionsPerPageCalcEdit
			// 
			this.QuestionsPerPageCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuestionsPerPageCalcEdit, "G0_QuestionsPerWebPage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_QuestionsPerWebPage)));
			this.QuestionsPerPageCalcEdit.Decimals = 0;
			this.QuestionsPerPageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 5, true);
			this.QuestionsPerPageCalcEdit.Name = "QuestionsPerPageCalcEdit";
			this.QuestionsPerPageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.QuestionsPerPageCalcEdit.TabIndex = 14;
			this.QuestionsPerPageCalcEdit.Text = "0";
			this.QuestionsPerPageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreviewButton
			// 
			this.PreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|527d6a15-ed79-4f63-9b56-8bda9ed726e1", "Preview");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 3, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PreviewButton.TabIndex = 1;
			this.PreviewButton.UseVisualStyleBackColor = true;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// QuestionsTabControl
			// 
			this.QuestionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.QuestionsTabControl.Controls.Add(this.ActiveQuestionsTabPage);
			this.QuestionsTabControl.Controls.Add(this.InactiveQuestionsTabPage);
			this.QuestionsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuestionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.QuestionsTabControl.Name = "QuestionsTabControl";
			this.QuestionsTabControl.SelectedIndex = 0;
			this.QuestionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 432, true);
			this.QuestionsTabControl.TabIndex = 22;
			this.QuestionsTabControl.SelectedIndexChanged += new System.EventHandler(this.QuestionsTabControl_SelectedIndexChanged);
			// 
			// ActiveQuestionsTabPage
			// 
			this.ActiveQuestionsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|7999dfc0-733b-4365-8026-51cb6163ae17", "Active");
			this.ActiveQuestionsTabPage.Controls.Add(this.QuestionsGrid);
			this.ActiveQuestionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ActiveQuestionsTabPage.Name = "ActiveQuestionsTabPage";
			this.ActiveQuestionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ActiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 405, true);
			this.ActiveQuestionsTabPage.TabIndex = 0;
			this.ActiveQuestionsTabPage.UseVisualStyleBackColor = true;
			// 
			// QuestionsGrid
			// 
			this.QuestionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuestionsGrid, "Questions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).ActualOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).HY_QuestionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).HY_AnswerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).Lookups.AnswerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).IsActiveForBinding)));
			this.QuestionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|e6623e62-f694-424b-b245-793d950ef3fe", "No.");
			zCalcEditColumnStyleInfo1.ColumnName = "ActualOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDynamicMultilineTextBoxColumnStyleInfo1.ColumnName = "HY_QuestionMultilingual";
			zDynamicMultilineTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|F7555D9A-0A04-4AC6-98DB-1678AC6C2FFA", "Text");
			zDynamicMultilineTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+AnswerTypes";
			zDropEditColumnStyleInfo1.ColumnName = "HY_AnswerType";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|6faf6892-47a6-42ce-b450-38a5f05c1e3f", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsActiveForBinding";
			this.QuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.QuestionsGrid.GridId = "ba22d479-9f0c-4195-8d1d-c5d473617aba";
			this.QuestionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuestionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuestionsGrid.LayoutKey = "QuestionsGrid";
			this.QuestionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.QuestionsGrid.Name = "QuestionsGrid";
			this.QuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 399, true);
			this.QuestionsGrid.TabIndex = 6;
			// 
			// InactiveQuestionsTabPage
			// 
			this.InactiveQuestionsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|67a541da-733c-4376-83bd-2df4f981b8bc", "Inactive");
			this.InactiveQuestionsTabPage.Controls.Add(this.InactiveQuestionsGrid);
			this.InactiveQuestionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InactiveQuestionsTabPage.Name = "InactiveQuestionsTabPage";
			this.InactiveQuestionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InactiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 405, true);
			this.InactiveQuestionsTabPage.TabIndex = 1;
			this.InactiveQuestionsTabPage.UseVisualStyleBackColor = true;
			// 
			// InactiveQuestionsGrid
			// 
			this.InactiveQuestionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InactiveQuestionsGrid, "InactiveQuestions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).InactiveQuestions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).InactiveQuestions)).SyncRoot)).HY_Question)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).InactiveQuestions)).SyncRoot)).HY_AnswerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).InactiveQuestions)).SyncRoot)).Lookups.AnswerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).InactiveQuestions)).SyncRoot)).IsActiveForBinding)));
			this.InactiveQuestionsGrid.CaptionVisible = false;
			zDynamicMultilineTextBoxColumnStyleInfo2.ColumnName = "HY_Question";
			zDynamicMultilineTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+AnswerTypes";
			zDropEditColumnStyleInfo2.ColumnName = "HY_AnswerType";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionsUserControl|49f3f1ed-e86a-4d3d-8ec6-4593606af421", "Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsActiveForBinding";
			this.InactiveQuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo2);
			this.InactiveQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.InactiveQuestionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.InactiveQuestionsGrid.GridId = "b44e40b9-fbe1-4233-8f6b-daf691f2fb3b";
			this.InactiveQuestionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InactiveQuestionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InactiveQuestionsGrid.LayoutKey = "InactiveVotingGrid";
			this.InactiveQuestionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InactiveQuestionsGrid.Name = "InactiveQuestionsGrid";
			this.InactiveQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 399, true);
			this.InactiveQuestionsGrid.TabIndex = 4;
			// 
			// QuestionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.QuestionsTabControl);
			this.Controls.Add(this.SetupDetailsPanel);
			this.Name = "QuestionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 461, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SetupDetailsPanel.ResumeLayout(false);
			this.SetupDetailsPanel.PerformLayout();
			this.QuestionsTabControl.ResumeLayout(false);
			this.ActiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).EndInit();
			this.InactiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZPanel SetupDetailsPanel;
		protected Enterprise.ZArchitecture.ZCalcEdit QuestionsPerPageCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton PreviewButton;
		protected Enterprise.ZArchitecture.GUI.ZTabControl QuestionsTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ActiveQuestionsTabPage;
		public Enterprise.ZArchitecture.ZGrid QuestionsGrid;
		protected Enterprise.ZArchitecture.GUI.ZTabPage InactiveQuestionsTabPage;
		protected Enterprise.ZArchitecture.ZGrid InactiveQuestionsGrid;
	}
}
