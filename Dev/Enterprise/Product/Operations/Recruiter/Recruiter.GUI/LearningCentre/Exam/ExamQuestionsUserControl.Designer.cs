namespace Enterprise.Recruiter.GUI
{
	partial class ExamQuestionsUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo zDynamicMultilineTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDynamicMultilineTextBoxColumnStyleInfo();
			this.SetupDetailsPanel.SuspendLayout();
			this.QuestionsTabControl.SuspendLayout();
			this.ActiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuestionsGrid)).BeginInit();
			this.InactiveQuestionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InactiveQuestionsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// QuestionsGrid
			// 
			zCheckBoxColumnStyleInfo1.ColumnName = "HY_IsRandomisable";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HY_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDynamicMultilineTextBoxColumnStyleInfo1.ColumnName = "HY_Comment";
			zDynamicMultilineTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamQuestionsUserControl|7b82c0f0-1a0d-4868-a6ac-b561cddc399d", "Comment");
			zDynamicMultilineTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.QuestionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.QuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			this.InactiveQuestionsGrid.ColumnStyles.Add(zDynamicMultilineTextBoxColumnStyleInfo1);
			// 
			// ExamQuestionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ExamQuestionsUserControl";
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
	}
}
