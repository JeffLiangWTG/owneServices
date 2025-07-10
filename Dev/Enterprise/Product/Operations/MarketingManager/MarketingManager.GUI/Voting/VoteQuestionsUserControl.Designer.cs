namespace Enterprise.MarketingManager.GUI
{
	partial class VoteQuestionsUserControl
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
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.MaxVotesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinVotesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShouldRankCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RandomizeWithinHeaderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SetupDetailsPanel.SuspendLayout();
			this.QuestionsTabControl.SuspendLayout();
			this.ActiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).BeginInit();
			this.InactiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SetupDetailsPanel
			// 
			this.SetupDetailsPanel.Controls.Add(this.zTextBox1);
			this.SetupDetailsPanel.Controls.Add(this.ShouldRankCheckBox);
			this.SetupDetailsPanel.Controls.Add(this.RandomizeWithinHeaderCheckBox);
			this.SetupDetailsPanel.Controls.Add(this.MinVotesCalcEdit);
			this.SetupDetailsPanel.Controls.Add(this.MaxVotesCalcEdit);
			this.SetupDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 83, true);
			this.SetupDetailsPanel.TabIndex = 0;
			this.SetupDetailsPanel.Controls.SetChildIndex(this.MaxVotesCalcEdit, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.QuestionsPerPageCalcEdit, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.MinVotesCalcEdit, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.ShouldRankCheckBox, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.RandomizeWithinHeaderCheckBox, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.PreviewButton, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.zTextBox1, 0);
			// 
			// QuestionsPerPageCalcEdit
			// 
			this.QuestionsPerPageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 31, true);
			this.QuestionsPerPageCalcEdit.TabIndex = 1;
			// 
			// PreviewButton
			// 
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 29, true);
			this.PreviewButton.TabIndex = 6;
			// 
			// QuestionsTabControl
			// 
			this.QuestionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 83, true);
			this.QuestionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 378, true);
			this.QuestionsTabControl.TabIndex = 1;
			// 
			// ActiveQuestionsTabPage
			// 
			this.ActiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 351, true);
			// 
			// QuestionsGrid
			// 
			this.BindingSource.SetBindingMember(this.QuestionsGrid, "Questions.SubQuestions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)).SyncRoot)).ActualOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)).SyncRoot)).HY_Question)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)).SyncRoot)).HY_AnswerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)).SyncRoot)).Lookups.AnswerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).SubQuestions)).SyncRoot)).IsActiveForBinding)));
			this.QuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 345, true);
			// 
			// InactiveQuestionsTabPage
			// 
			this.InactiveQuestionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 351, true);
			// 
			// InactiveQuestionsGrid
			// 
			this.BindingSource.SetBindingMember(this.InactiveQuestionsGrid, "Questions.InactiveSubQuestions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).InactiveSubQuestions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).InactiveSubQuestions)).SyncRoot)).HY_Question)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).InactiveSubQuestions)).SyncRoot)).HY_AnswerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).InactiveSubQuestions)).SyncRoot)).Lookups.AnswerTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).InactiveSubQuestions)).SyncRoot)).IsActiveForBinding)));
			this.InactiveQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 345, true);
			this.InactiveQuestionsGrid.TabIndex = 0;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "Questions.HY_Question");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).HY_Question)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteQuestionsUserControl|7548aac9-4e66-4b20-b428-13e172f7357a", "Vote Header");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 5, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(563, 20, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// MaxVotesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxVotesCalcEdit, "Questions.HY_Max");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).HY_Max)));
			this.MaxVotesCalcEdit.DecimalPlaces = 0;
			this.MaxVotesCalcEdit.Decimals = 0;
			this.MaxVotesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 57, true);
			this.MaxVotesCalcEdit.Name = "MaxVotesCalcEdit";
			this.MaxVotesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.MaxVotesCalcEdit.TabIndex = 3;
			this.MaxVotesCalcEdit.Text = "0";
			this.MaxVotesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinVotesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinVotesCalcEdit, "Questions.HY_Min");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).HY_Min)));
			this.MinVotesCalcEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteQuestionsUserControl|3f26d4bd-b00c-41a9-8190-f421e724e121", "No. of Votes allowed    Min");
			this.MinVotesCalcEdit.DecimalPlaces = 0;
			this.MinVotesCalcEdit.Decimals = 0;
			this.MinVotesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 57, true);
			this.MinVotesCalcEdit.Name = "MinVotesCalcEdit";
			this.MinVotesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.MinVotesCalcEdit.TabIndex = 2;
			this.MinVotesCalcEdit.Text = "0";
			this.MinVotesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShouldRankCheckBox
			// 
			this.ShouldRankCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldRankCheckBox, "Questions.ShouldRankVotingNominees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(((System.Collections.IList)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).Questions)).SyncRoot)).ShouldRankVotingNominees)));
			this.ShouldRankCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteQuestionsUserControl|7c12fd78-2d18-4c79-9f99-bc4fa982c652", "Should Rank Nominees");
			this.ShouldRankCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShouldRankCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 60, true);
			this.ShouldRankCheckBox.Name = "ShouldRankCheckBox";
			this.ShouldRankCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.ShouldRankCheckBox.TabIndex = 4;
			this.ShouldRankCheckBox.UseVisualStyleBackColor = true;
			// 
			// RandomizeWithinHeaderCheckBox
			// 
			this.RandomizeWithinHeaderCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RandomizeWithinHeaderCheckBox, "G0_RandomizeWithinHeader");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_RandomizeWithinHeader)));
			this.RandomizeWithinHeaderCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("VoteQuestionsUserControl|3204AFB5-D14A-4734-AD4D-E86B3709CB02", "Randomize within Header");
			this.RandomizeWithinHeaderCheckBox.Checked = true;
			this.RandomizeWithinHeaderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.RandomizeWithinHeaderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RandomizeWithinHeaderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 60, true);
			this.RandomizeWithinHeaderCheckBox.Name = "RandomizeWithinHeaderCheckBox";
			this.RandomizeWithinHeaderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.RandomizeWithinHeaderCheckBox.TabIndex = 5;
			this.RandomizeWithinHeaderCheckBox.UseVisualStyleBackColor = true;
			// 
			// VoteQuestionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "VoteQuestionsUserControl";
			this.SetupDetailsPanel.ResumeLayout(false);
			this.SetupDetailsPanel.PerformLayout();
			this.QuestionsTabControl.ResumeLayout(false);
			this.ActiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).EndInit();
			this.InactiveQuestionsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShouldRankCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RandomizeWithinHeaderCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit MinVotesCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit MaxVotesCalcEdit;
	}
}
