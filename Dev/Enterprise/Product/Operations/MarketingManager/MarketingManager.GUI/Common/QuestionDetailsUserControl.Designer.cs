namespace Enterprise.MarketingManager.GUI
{
	partial class QuestionDetailsUserControl
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
			this.rowLayoutPanel1 = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.AnswerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.mainSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.OptionsBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AnswerTypeDropEdit.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitPanel)).BeginInit();
			this.mainSplitPanel.Panel1.SuspendLayout();
			this.mainSplitPanel.Panel2.SuspendLayout();
			this.mainSplitPanel.SuspendLayout();
			this.OptionsBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).BeginInit();
			this.OptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.VoteExamSurveyQuestionSet);
			// 
			// rowLayoutPanel1
			// 
			this.rowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.rowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rowLayoutPanel1.Name = "rowLayoutPanel1";
			this.rowLayoutPanel1.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			this.rowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 102, true);
			this.rowLayoutPanel1.TabIndex = 4;
			// 
			// AnswerTypeDropEdit
			// 
			this.AnswerTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AnswerTypeDropEdit, "HY_AnswerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).HY_AnswerType)));
			this.AnswerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 4, true);
			this.AnswerTypeDropEdit.Name = "AnswerTypeDropEdit";
			this.AnswerTypeDropEdit.PreBoundMaxLength = 3;
			this.AnswerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.AnswerTypeDropEdit.TabIndex = 1;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "HY_Question");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).HY_Question)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.zTextBox1, 0);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 32, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 66, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.DetailsTabPage);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 135, true);
			this.zTabControl1.TabIndex = 5;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("QuestionDetailsUserControl|755898f6-5407-4dea-84bc-0cb879810524", "Details");
			this.DetailsTabPage.Controls.Add(this.mainSplitPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 108, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// mainSplitPanel
			// 
			this.mainSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitPanel.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.mainSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.mainSplitPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 8, true);
			this.mainSplitPanel.Name = "mainSplitPanel";
			// 
			// mainSplitPanel.Panel1
			// 
			this.mainSplitPanel.Panel1.Controls.Add(this.zTextBox1);
			this.mainSplitPanel.Panel1.Controls.Add(this.AnswerTypeDropEdit);
			this.mainSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 102, true);
			this.mainSplitPanel.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			// 
			// mainSplitPanel.Panel2
			// 
			this.mainSplitPanel.Panel2.Controls.Add(this.OptionsBox);
			this.mainSplitPanel.Panel2.Controls.Add(this.rowLayoutPanel1);
			this.mainSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.mainSplitPanel.TabIndex = 0;
			// 
			// OptionsBox
			// 
			this.OptionsBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SurveyQuestionDetailsUserControl|5761BA9F-8F97-4803-9255-428797EC891B", "Options");
			this.OptionsBox.Controls.Add(this.OptionsGrid);
			this.OptionsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 0, true);
			this.OptionsBox.Name = "OptionsBox";
			this.OptionsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 102, true);
			this.OptionsBox.TabIndex = 2;
			this.OptionsBox.TabStop = false;
			this.OptionsBox.Visible = false;
			// 
			// OptionsGrid
			// 
			this.OptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OptionsGrid, "SubQuestions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).SubQuestions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).SubQuestions)).SyncRoot)).HY_SubQuestionOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).SubQuestions)).SyncRoot)).HY_Question)));
			this.OptionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "HY_SubQuestionOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDynamicMultilineTextBoxColumnStyleInfo1.ColumnName = "HY_Question";
			zDynamicMultilineTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.OptionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OptionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			this.OptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionsGrid.GridId = "7601d4ed-0e5c-4ed4-9b13-e89282909afe";
			this.OptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OptionsGrid.LayoutKey = "OptionsGrid";
			this.OptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OptionsGrid.Name = "OptionsGrid";
			this.OptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 83, true);
			this.OptionsGrid.TabIndex = 0;
			// 
			// QuestionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTabControl1);
			this.Name = "QuestionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AnswerTypeDropEdit.ResumeLayout(true);
			this.AnswerTypeDropEdit.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.mainSplitPanel.Panel1.ResumeLayout(false);
			this.mainSplitPanel.Panel1.PerformLayout();
			this.mainSplitPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitPanel)).EndInit();
			this.mainSplitPanel.ResumeLayout(false);
			this.mainSplitPanel.PerformLayout();
			this.OptionsBox.ResumeLayout(false);
			this.OptionsBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).EndInit();
			this.OptionsGrid.ResumeLayout(false);
			this.OptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.Layout.RowLayoutPanel rowLayoutPanel1;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit AnswerTypeDropEdit;
		public Enterprise.ZArchitecture.ZTextBox zTextBox1;
		protected Enterprise.ZArchitecture.GUI.ZTabControl zTabControl1;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected Enterprise.ZArchitecture.ZGrid OptionsGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OptionsBox;
		protected CargoWise.Windows.UI.KSplitContainer mainSplitPanel;
	}
}
