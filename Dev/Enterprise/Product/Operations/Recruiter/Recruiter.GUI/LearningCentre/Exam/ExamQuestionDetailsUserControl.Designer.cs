namespace Enterprise.Recruiter.GUI
{
	partial class ExamQuestionDetailsUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CorrectAnswerDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CorrectAnswerCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.rowLayoutPanel1.SuspendLayout();
			this.AnswerTypeDropEdit.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).BeginInit();
			this.OptionsGrid.SuspendLayout();
			this.OptionsBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitPanel)).BeginInit();
			this.mainSplitPanel.Panel1.SuspendLayout();
			this.mainSplitPanel.Panel2.SuspendLayout();
			this.mainSplitPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CorrectAnswerDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// OptionalCheckBox
			// 
			this.OptionalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.rowLayoutPanel1.SetRow(this.OptionalCheckBox, 1);
			this.OptionalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			// 
			// MaxCalcEdit
			// 
			this.MaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 50, true);
			this.rowLayoutPanel1.SetRow(this.MaxCalcEdit, 2);
			this.MaxCalcEdit.TabIndex = 2;
			// 
			// MinCalcEdit
			// 
			this.MinCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.rowLayoutPanel1.SetRow(this.MinCalcEdit, 2);
			this.MinCalcEdit.TabIndex = 1;
			// 
			// rowLayoutPanel1
			// 
			this.rowLayoutPanel1.Controls.Add(this.CorrectAnswerDropEdit);
			this.rowLayoutPanel1.Controls.Add(this.CorrectAnswerCalcEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.rowLayoutPanel1, true);
			this.rowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 84, true);
			this.rowLayoutPanel1.Controls.SetChildIndex(this.OptionalCheckBox, 0);
			this.rowLayoutPanel1.Controls.SetChildIndex(this.MaxCalcEdit, 0);
			this.rowLayoutPanel1.Controls.SetChildIndex(this.MinCalcEdit, 0);
			this.rowLayoutPanel1.Controls.SetChildIndex(this.CorrectAnswerCalcEdit, 0);
			this.rowLayoutPanel1.Controls.SetChildIndex(this.CorrectAnswerDropEdit, 0);
			// 
			// zTextBox1
			// 
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 40, true);
			// 
			// zTabControl1
			// 
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 129, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 102, true);
			// 
			// OptionsGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamQuestionDetailsUserControl|2243f2bf-a222-400a-9d0b-f7782a50e200", "Correct?");
			zCheckBoxColumnStyleInfo1.ColumnName = "CorrectAnswerAsBool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.OptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 77, true);
			// 
			// OptionsBox
			// 
			this.OptionsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 96, true);
			// 
			// mainSplitPanel
			// 
			this.mainSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 96, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.LearningCentreQuestionCollection);
			// 
			// CorrectAnswerDropEdit
			// 
			this.CorrectAnswerDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CorrectAnswerDropEdit, "HY_ExamCorrectAnswer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.LearningCentreQuestion)(null)).HY_ExamCorrectAnswer)));
			this.CorrectAnswerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 75, true);
			this.CorrectAnswerDropEdit.Name = "CorrectAnswerDropEdit";
			this.CorrectAnswerDropEdit.PreBoundMaxLength = 3;
			this.rowLayoutPanel1.SetRow(this.CorrectAnswerDropEdit, 3);
			this.CorrectAnswerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CorrectAnswerDropEdit.TabIndex = 5;
			// 
			// CorrectAnswerCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CorrectAnswerCalcEdit, "CorrectAnswerAsByte");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.LearningCentreQuestion)(null)).CorrectAnswerAsByte)));
			this.CorrectAnswerCalcEdit.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamQuestionDetailsUserControl|56f69350-cd2b-48a0-ae78-47be92be398f", "Correct Answer");
			this.CorrectAnswerCalcEdit.DecimalPlaces = 2;
			this.CorrectAnswerCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.CorrectAnswerCalcEdit.Name = "CorrectAnswerCalcEdit";
			this.rowLayoutPanel1.SetRow(this.CorrectAnswerCalcEdit, 4);
			this.CorrectAnswerCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.CorrectAnswerCalcEdit.TabIndex = 6;
			this.CorrectAnswerCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExamQuestionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExamQuestionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 129, true);
			this.rowLayoutPanel1.ResumeLayout(false);
			this.rowLayoutPanel1.PerformLayout();
			this.AnswerTypeDropEdit.ResumeLayout(true);
			this.AnswerTypeDropEdit.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).EndInit();
			this.OptionsGrid.ResumeLayout(false);
			this.OptionsGrid.PerformLayout();
			this.OptionsBox.ResumeLayout(false);
			this.OptionsBox.PerformLayout();
			this.mainSplitPanel.Panel1.ResumeLayout(false);
			this.mainSplitPanel.Panel1.PerformLayout();
			this.mainSplitPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitPanel)).EndInit();
			this.mainSplitPanel.ResumeLayout(false);
			this.mainSplitPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CorrectAnswerDropEdit.ResumeLayout(true);
			this.CorrectAnswerDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit CorrectAnswerDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit CorrectAnswerCalcEdit;
	}
}
