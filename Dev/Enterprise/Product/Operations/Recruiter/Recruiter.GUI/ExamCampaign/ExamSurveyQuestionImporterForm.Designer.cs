using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.GUI
{
	partial class ExamSurveyQuestionImporterForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownloadTemplateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.ExamSurveyQuestionImporterBizO);
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "ShouldClearExistingQuestions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.ExamSurveyQuestionImporterBizO)(null)).ShouldClearExistingQuestions)));
			this.zCheckBox1.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.zCheckBox1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|9ba15450-9250-4449-8f2c-ddef2f1d9657", "Clear Existing?", "Clear Existing?", "Clear Existing Questions.");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 38, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zCheckBox1.TabIndex = 2;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "StartingRowIndex");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.ExamSurveyQuestionImporterBizO)(null)).StartingRowIndex)));
			this.zCalcEdit1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|feb183c9-be30-4e45-990f-3a3d72fcd17d", "Starting Row Index", "Starting Row Index (Not for XML file).");
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 58, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.zCalcEdit1.TabIndex = 3;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "FileLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.ExamSurveyQuestionImporterBizO)(null)).FileLocation)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|25923fe3-e846-4faf-a36a-514f35a24068", "File Location", "CSV or XML File Location.");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 12, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// BrowseButton
			// 
			this.BrowseButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|89b9d935-19db-4d40-955c-5bfbb659dffc", "Browse");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 10, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 1;
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// DownloadTemplateButton
			// 
			this.DownloadTemplateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DownloadTemplateButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|bb3443e1-9130-45e3-b292-6832af7c644e", "Download Template");
			this.DownloadTemplateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 82, true);
			this.DownloadTemplateButton.Name = "ImportButton";
			this.DownloadTemplateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 23, true);
			this.DownloadTemplateButton.TabIndex = 4;
			this.DownloadTemplateButton.UseVisualStyleBackColor = true;
			this.DownloadTemplateButton.Click += new System.EventHandler(this.DownloadTemplateButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|829d71dd-adec-456a-9385-d5f034791004", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 82, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 5;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("ExamSurveyQuestionImporterForm|e24f4e32-4753-401e-8b07-0f80449fbfc2", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 82, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ExamSurveyQuestionImporterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 134, true);
			this.Controls.Add(this.DownloadTemplateButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.zCalcEdit1);
			this.Controls.Add(this.zCheckBox1);
			this.Controls.Add(this.BrowseButton);
			this.DataSourceAssemblyName = "Enterprise.MarketingManager.Business";
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.ExamSurveyQuestionImporterBizO);
			this.DataSourceTypeName = "Enterprise.MarketingManager.Business.ExamSurveyQuestionImporterBizO";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ExamSurveyQuestionImporterForm";
			this.Text = "Import Questions";
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.zCheckBox1, 0);
			this.Controls.SetChildIndex(this.zCalcEdit1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.DownloadTemplateButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		internal Enterprise.ZArchitecture.GUI.ZButton BrowseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton DownloadTemplateButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ImportButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}
