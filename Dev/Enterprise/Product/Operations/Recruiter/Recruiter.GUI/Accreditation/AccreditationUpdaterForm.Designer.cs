using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	partial class AccreditationUpdaterForm
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
		protected new void InitializeComponent()
		{
			this.deleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.deleteWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.updateOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accredFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.windowTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.timePeriodRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.firstExamRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.windowOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.endDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.additionalToleranceDaysNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.completionToleranceDaysNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.startDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RunUpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.updateOptionsGroupBox.SuspendLayout();
			this.accredFindBox.SuspendLayout();
			this.windowTypeGroupBox.SuspendLayout();
			this.windowOptionsGroupBox.SuspendLayout();
			this.endDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalToleranceDaysNumericUpDown)).BeginInit();
			this.additionalToleranceDaysNumericUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.completionToleranceDaysNumericUpDown)).BeginInit();
			this.completionToleranceDaysNumericUpDown.SuspendLayout();
			this.startDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 352, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.AccreditationUpdater);
			// 
			// deleteCheckBox
			// 
			this.deleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.deleteCheckBox, "DeleteExistingCertificates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).DeleteExistingCertificates)));
			this.deleteCheckBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|deleteCheckBox", "Delete any automatically-generated certificates for this accreditation/person before running update");
			this.deleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.deleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.deleteCheckBox.Name = "deleteCheckBox";
			this.deleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 16, true);
			this.deleteCheckBox.TabIndex = 0;
			this.deleteCheckBox.UseVisualStyleBackColor = true;
			// 
			// deleteWarningLabel
			// 
			this.deleteWarningLabel.AutoSize = true;
			this.deleteWarningLabel.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|deleteWarningLabel", "Running this function will delete ALL existing attempts for this accreditation/person and repopulate them from exam attempts specified in the date windows below");
			this.deleteWarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.deleteWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.deleteWarningLabel.Name = "deleteWarningLabel";
			this.deleteWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 13, true);
			this.deleteWarningLabel.TabIndex = 0;
			// 
			// updateOptionsGroupBox
			// 
			this.updateOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.updateOptionsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|updateOptionsGroupBox", "Update Options");
			this.updateOptionsGroupBox.Controls.Add(this.deleteCheckBox);
			this.updateOptionsGroupBox.Controls.Add(this.accredFindBox);
			this.updateOptionsGroupBox.Controls.Add(this.windowTypeGroupBox);
			this.updateOptionsGroupBox.Controls.Add(this.windowOptionsGroupBox);
			this.updateOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 24, true);
			this.updateOptionsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 253, true);
			this.updateOptionsGroupBox.Name = "updateOptionsGroupBox";
			this.updateOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 263, true);
			this.updateOptionsGroupBox.TabIndex = 1;
			this.updateOptionsGroupBox.TabStop = false;
			// 
			// accredFindBox
			// 
			this.accredFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accredFindBox, "ParentAccreditationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).ParentAccreditationPK)));
			this.accredFindBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|accredCodeDropEdit", "Re sync a specific accreditation");
			this.accredFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.accredFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 41, true);
			this.accredFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbAccreditation;
			this.accredFindBox.Name = "accredFindBox";
			this.accredFindBox.ShouldResize = true;
			this.accredFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.accredFindBox.TabIndex = 1;
			// 
			// windowTypeGroupBox
			// 
			this.windowTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.windowTypeGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|windowTypeGroupBox", "Accreditation Attempt Exam Consideration Range");
			this.windowTypeGroupBox.Controls.Add(this.timePeriodRadioButton);
			this.windowTypeGroupBox.Controls.Add(this.firstExamRadioButton);
			this.windowTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 65, true);
			this.windowTypeGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 73, true);
			this.windowTypeGroupBox.Name = "windowTypeGroupBox";
			this.windowTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 73, true);
			this.windowTypeGroupBox.TabIndex = 1;
			this.windowTypeGroupBox.TabStop = false;
			// 
			// timePeriodRadioButton
			// 
			this.timePeriodRadioButton.AutoCheck = false;
			this.timePeriodRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.timePeriodRadioButton, "IsTimePeriodType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).IsTimePeriodType)));
			this.timePeriodRadioButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|timePeriodRadioButton", "Date Range - Include all Exam Attempts within specified time period");
			this.timePeriodRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.timePeriodRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.timePeriodRadioButton.Name = "timePeriodRadioButton";
			this.timePeriodRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.timePeriodRadioButton.TabIndex = 1;
			this.timePeriodRadioButton.UseVisualStyleBackColor = true;
			// 
			// firstExamRadioButton
			// 
			this.firstExamRadioButton.AutoCheck = false;
			this.firstExamRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.firstExamRadioButton, "IsFirstExamType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).IsFirstExamType)));
			this.firstExamRadioButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|firstExamRadioButton", "Days - Include all Exam Attempts within number of days from First Exam Attempt");
			this.firstExamRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.firstExamRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.firstExamRadioButton.Name = "firstExamRadioButton";
			this.firstExamRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.firstExamRadioButton.TabIndex = 0;
			this.firstExamRadioButton.UseVisualStyleBackColor = true;
			// 
			// windowOptionsGroupBox
			// 
			this.windowOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.windowOptionsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|windowOptionsGroupBox", "Days/Date Range");
			this.windowOptionsGroupBox.Controls.Add(this.endDateEdit);
			this.windowOptionsGroupBox.Controls.Add(this.additionalToleranceDaysNumericUpDown);
			this.windowOptionsGroupBox.Controls.Add(this.completionToleranceDaysNumericUpDown);
			this.windowOptionsGroupBox.Controls.Add(this.startDateEdit);
			this.windowOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 142, true);
			this.windowOptionsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 119, true);
			this.windowOptionsGroupBox.Name = "windowOptionsGroupBox";
			this.windowOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 119, true);
			this.windowOptionsGroupBox.TabIndex = 2;
			this.windowOptionsGroupBox.TabStop = false;
			// 
			// endDateEdit
			// 
			this.endDateEdit.AllowDrop = true;
			this.endDateEdit.AutoCompleteMonthThreshold = 1;
			this.endDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.endDateEdit, "ToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).ToDate)));
			this.endDateEdit.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|endDateEdit", "End Date");
			this.endDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 44, true);
			this.endDateEdit.Name = "endDateEdit";
			this.endDateEdit.TabIndex = 2;
			// 
			// additionalToleranceDaysNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.additionalToleranceDaysNumericUpDown, "AdditionalCompletionToleranceDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).AdditionalCompletionToleranceDays)));
			this.additionalToleranceDaysNumericUpDown.BindTo = "AdditionalCompletionToleranceDays";
			this.additionalToleranceDaysNumericUpDown.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|additionalToleranceDaysNumericUpDown", "Additional days for Completion after End Date");
			this.additionalToleranceDaysNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 71, true);
			this.additionalToleranceDaysNumericUpDown.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.additionalToleranceDaysNumericUpDown.Name = "additionalToleranceDaysNumericUpDown";
			this.additionalToleranceDaysNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.additionalToleranceDaysNumericUpDown.TabIndex = 3;
			// 
			// completionToleranceDaysNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.completionToleranceDaysNumericUpDown, "CompletionToleranceDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).CompletionToleranceDays)));
			this.completionToleranceDaysNumericUpDown.BindTo = "CompletionToleranceDays";
			this.completionToleranceDaysNumericUpDown.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|completionToleranceDaysNumericUpDown", "Completion Date Range Tolerance (days)");
			this.completionToleranceDaysNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 19, true);
			this.completionToleranceDaysNumericUpDown.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
			this.completionToleranceDaysNumericUpDown.Name = "completionToleranceDaysNumericUpDown";
			this.completionToleranceDaysNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.completionToleranceDaysNumericUpDown.TabIndex = 0;
			// 
			// startDateEdit
			// 
			this.startDateEdit.AllowDrop = true;
			this.startDateEdit.AutoCompleteMonthThreshold = 1;
			this.startDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.startDateEdit, "FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.AccreditationUpdater)(null)).FromDate)));
			this.startDateEdit.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|startDateEdit", "Start Date");
			this.startDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 18, true);
			this.startDateEdit.Name = "startDateEdit";
			this.startDateEdit.TabIndex = 1;
			// 
			// RunUpdateButton
			// 
			this.RunUpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RunUpdateButton.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm|RunUpdateButton", "Run Update");
			this.RunUpdateButton.IsCaptionOverridden = false;
			this.RunUpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 323, true);
			this.RunUpdateButton.Name = "RunUpdateButton";
			this.RunUpdateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RunUpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RunUpdateButton.TabIndex = 2;
			this.RunUpdateButton.ToolTipCaption = null;
			this.RunUpdateButton.UseVisualStyleBackColor = true;
			this.RunUpdateButton.Click += new System.EventHandler(this.RunUpdateButton_Click);
			// 
			// AccreditationUpdaterForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("AccreditationUpdaterForm", "Create Accreditation Attempt for Existing Exam Attempts");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 376, true);
			this.Controls.Add(this.deleteWarningLabel);
			this.Controls.Add(this.updateOptionsGroupBox);
			this.Controls.Add(this.RunUpdateButton);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.AccreditationUpdater);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 376, true);
			this.Name = "AccreditationUpdaterForm";
			this.Controls.SetChildIndex(this.RunUpdateButton, 0);
			this.Controls.SetChildIndex(this.updateOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.deleteWarningLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.updateOptionsGroupBox.ResumeLayout(false);
			this.updateOptionsGroupBox.PerformLayout();
			this.accredFindBox.ResumeLayout(true);
			this.accredFindBox.PerformLayout();
			this.windowTypeGroupBox.ResumeLayout(false);
			this.windowTypeGroupBox.PerformLayout();
			this.windowOptionsGroupBox.ResumeLayout(false);
			this.windowOptionsGroupBox.PerformLayout();
			this.endDateEdit.ResumeLayout(true);
			this.endDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalToleranceDaysNumericUpDown)).EndInit();
			this.additionalToleranceDaysNumericUpDown.ResumeLayout(false);
			this.additionalToleranceDaysNumericUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.completionToleranceDaysNumericUpDown)).EndInit();
			this.completionToleranceDaysNumericUpDown.ResumeLayout(false);
			this.completionToleranceDaysNumericUpDown.PerformLayout();
			this.startDateEdit.ResumeLayout(true);
			this.startDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel deleteWarningLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox updateOptionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox deleteCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox windowTypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton firstExamRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton timePeriodRadioButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox windowOptionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown completionToleranceDaysNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZNumericUpDown additionalToleranceDaysNumericUpDown;
		private Enterprise.ZArchitecture.GUI.ZDateEdit startDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit endDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox accredFindBox;
		protected Enterprise.ZArchitecture.GUI.ZButton RunUpdateButton;
	}
}