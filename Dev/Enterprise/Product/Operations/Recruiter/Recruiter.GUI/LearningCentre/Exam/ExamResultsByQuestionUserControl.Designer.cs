namespace Enterprise.Recruiter.GUI
{
	partial class ExamResultsByQuestionUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ResultsByItemsSplitContainer.Panel1.SuspendLayout();
			this.ResultsByItemsSplitContainer.Panel2.SuspendLayout();
			this.ResultsByItemsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).BeginInit();
			this.AnswersTabControl.SuspendLayout();
			this.AnswersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ResultsByItemsSplitContainer
			// 
			// 
			// ResultItemAnswersGrid
			// 
			this.BindingSource.SetBindingMember(this.ResultItemAnswersGrid, "ActualQuestionsForBinding.LastCompletedSubmittedAnswers");
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamResultsByQuestionUserControl|e8736c16-2f0f-4974-8382-e43297bdcd5b", "Phone");
			zTextBoxColumnStyleInfo1.ColumnName = "CampaignItem+WorkPhone";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamResultsByQuestionUserControl|0f3d30b8-153b-4397-8322-db19cef7db97", "Answer");
			zMultiControlColumnStyleInfo1.ColumnName = "Answer";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "AnswerFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamResultsByQuestionUserControl|c2242482-b7b1-452c-a828-d897565b6eec", "Result");
			zTextBoxColumnStyleInfo2.ColumnName = "ResultAsText";
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ResultItemAnswersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// ExamResultsByQuestionUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ExamResultsByQuestionUserControl";
			this.ResultsByItemsSplitContainer.Panel1.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.Panel2.ResumeLayout(false);
			this.ResultsByItemsSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemsGrid)).EndInit();
			this.AnswersTabControl.ResumeLayout(false);
			this.AnswersTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ResultItemAnswersGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
