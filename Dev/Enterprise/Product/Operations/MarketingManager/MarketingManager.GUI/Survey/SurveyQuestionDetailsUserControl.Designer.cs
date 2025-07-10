namespace Enterprise.MarketingManager.GUI
{
	partial class SurveyQuestionDetailsUserControl
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
			this.OptionalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MinCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
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
			this.SuspendLayout();
			// 
			// rowLayoutPanel1
			// 
			this.rowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.rowLayoutPanel1.Controls.Add(this.MinCalcEdit);
			this.rowLayoutPanel1.Controls.Add(this.MaxCalcEdit);
			this.rowLayoutPanel1.Controls.Add(this.OptionalCheckBox);
			this.rowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.None;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.rowLayoutPanel1, true);
			this.rowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 0, true);
			this.rowLayoutPanel1.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			// 
			// OptionsGrid
			// 
			this.OptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 83, true);
			// 
			// OptionsBox
			// 
			this.OptionsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 102, true);
			// 
			// mainSplitPanel
			// 
			// 
			// OptionalCheckBox
			// 
			this.OptionalCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OptionalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OptionalCheckBox, "HY_IsOptional");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).HY_IsOptional)));
			this.OptionalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OptionalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.OptionalCheckBox.Name = "OptionalCheckBox";
			this.rowLayoutPanel1.SetRow(this.OptionalCheckBox, 0);
			this.OptionalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.OptionalCheckBox.TabIndex = 1;
			this.OptionalCheckBox.UseVisualStyleBackColor = true;
			// 
			// MinCalcEdit
			// 
			this.MinCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.BindingSource.SetBindingMember(this.MinCalcEdit, "HY_Min");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).HY_Min)));
			this.MinCalcEdit.DecimalPlaces = 2;
			this.MinCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.MinCalcEdit.Name = "MinCalcEdit";
			this.rowLayoutPanel1.SetRow(this.MinCalcEdit, 1);
			this.MinCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.MinCalcEdit.TabIndex = 2;
			this.MinCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxCalcEdit, "HY_Max");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.VoteExamSurveyQuestion)(null)).HY_Max)));
			this.MaxCalcEdit.DecimalPlaces = 2;
			this.MaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 25, true);
			this.MaxCalcEdit.Name = "MaxCalcEdit";
			this.rowLayoutPanel1.SetRow(this.MaxCalcEdit, 1);
			this.MaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.MaxCalcEdit.TabIndex = 3;
			this.MaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SurveyQuestionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SurveyQuestionDetailsUserControl";
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZCheckBox OptionalCheckBox;
		protected Enterprise.ZArchitecture.ZCalcEdit MaxCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit MinCalcEdit;
	}
}
