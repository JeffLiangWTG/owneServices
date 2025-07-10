namespace Enterprise.MarketingManager.GUI
{
	partial class SurveyQuestionsUserControl
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
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
			this.SetupDetailsPanel.Controls.Add(this.zDropEdit1);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.PreviewButton, 0);
			this.SetupDetailsPanel.Controls.SetChildIndex(this.QuestionsPerPageCalcEdit, 0);
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "G0_DefaultAnswerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaign)(null)).G0_DefaultAnswerType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 6, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.zDropEdit1.TabIndex = 15;
			// 
			// SurveyQuestionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "SurveyQuestionsUserControl";
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

		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
	}
}
