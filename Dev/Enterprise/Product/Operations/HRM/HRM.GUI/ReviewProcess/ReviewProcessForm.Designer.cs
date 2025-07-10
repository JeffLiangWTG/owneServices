using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.HRM.GUI
{
	partial class ReviewProcessForm
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
		private new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 511, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 478, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 511, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 0, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 32, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.HRM.Common.ReviewProcess);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 544, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_Name)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_Type)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_Status)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_PrimaryHierarchy)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_OverrideHierarchy)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_RX_NKCurrency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_SubmissionDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.HRM.Common.ReviewProcess)(null)).RPR_S9_EmployeesInReview)));
			// 
			// ReviewProcessForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 567, true);
			this.DataSourceType = typeof(Enterprise.HRM.Common.ReviewProcess);
			this.Name = "ReviewProcessForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "ReviewProcessForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrimaryHierarchyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideHierarchyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrencyFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.EffectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubmissionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.EmployeesInReviewFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainTabPage.SuspendLayout();
			this.CurrencyFindBox.SuspendLayout();
			this.EffectiveDateEdit.SuspendLayout();
			this.SubmissionDateEdit.SuspendLayout();
			this.EmployeesInReviewFindBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.NameTextBox);
			this.MainTabPage.Controls.Add(this.TypeTextBox);
			this.MainTabPage.Controls.Add(this.StatusTextBox);
			this.MainTabPage.Controls.Add(this.CurrencyFindBox);
			this.MainTabPage.Controls.Add(this.PrimaryHierarchyTextBox);
			this.MainTabPage.Controls.Add(this.OverrideHierarchyTextBox);
			this.MainTabPage.Controls.Add(this.EffectiveDateEdit);
			this.MainTabPage.Controls.Add(this.SubmissionDateEdit);
			this.MainTabPage.Controls.Add(this.EmployeesInReviewFindBox);
			// 
			// NameTextBox
			// 
			this.NameTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NameTextBox, "RPR_Name");
			this.NameTextBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("94f9261f-a3fd-479a-86d0-4db692780b8f", "Review Name");
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 23, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.ReadOnly = true;
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 31, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// TypeTextBox
			// 
			this.TypeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeTextBox, "RPR_Type");
			this.TypeTextBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("476f545d-1d29-4fac-86f2-8e2b52887f97", "Review Type");
			this.TypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 57, true);
			this.TypeTextBox.Name = "TypeTextBox";
			this.TypeTextBox.ReadOnly = true;
			this.TypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 31, true);
			this.TypeTextBox.TabIndex = 1;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusTextBox, "RPR_Status");
			this.StatusTextBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("e5b2222e-5e70-43ee-9b80-63ae1512f0d7", "Review Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 88, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.ReadOnly = true;
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 31, true);
			this.StatusTextBox.TabIndex = 2;
			// 
			// PrimaryHierarchyTextBox
			// 
			this.PrimaryHierarchyTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrimaryHierarchyTextBox, "RPR_PrimaryHierarchy");
			this.PrimaryHierarchyTextBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("5f452e18-cc1c-4a05-ad52-9da166ce24c9", "Primary Hierarchy");
			this.PrimaryHierarchyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 145, true);
			this.PrimaryHierarchyTextBox.Name = "PrimaryHierarchyTextBox";
			this.PrimaryHierarchyTextBox.ReadOnly = true;
			this.PrimaryHierarchyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 31, true);
			this.PrimaryHierarchyTextBox.TabIndex = 4;
			// 
			// OverrideHierarchyTextBox
			// 
			this.OverrideHierarchyTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideHierarchyTextBox, "RPR_OverrideHierarchy");
			this.OverrideHierarchyTextBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("1fa7dffc-2ced-483f-a7d4-b43d00d02901", "Override Hierarchy");
			this.OverrideHierarchyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 175, true);
			this.OverrideHierarchyTextBox.Name = "OverrideHierarchyTextBox";
			this.OverrideHierarchyTextBox.ReadOnly = true;
			this.OverrideHierarchyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 31, true);
			this.OverrideHierarchyTextBox.TabIndex = 5;
			// 
			// CurrencyFindBox
			// 
			this.CurrencyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyFindBox, "RPR_RX_NKCurrency");
			this.CurrencyFindBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("104b607f-94ba-4b69-8289-587041991cac", "Currency");
			this.CurrencyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 115, true);
			this.CurrencyFindBox.Name = "CurrencyFindBox";
			this.CurrencyFindBox.ReadOnly = true;
			this.CurrencyFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrencyFindBox.ParentType = null;
			this.CurrencyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 31, true);
			this.CurrencyFindBox.TabIndex = 3;
			// 
			// EffectiveDateEdit
			// 
			this.EffectiveDateEdit.AllowDrop = true;
			this.EffectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EffectiveDateEdit, "RPR_EffectiveDate");
			this.EffectiveDateEdit.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("d3afa253-4941-4fa2-a878-5c1eb0eabadf", "Effective Date");
			this.EffectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 205, true);
			this.EffectiveDateEdit.Name = "EffectiveDateEdit";
			this.EffectiveDateEdit.ReadOnly = true;
			this.EffectiveDateEdit.TabIndex = 6;
			// 
			// SubmissionDateEdit
			// 
			this.SubmissionDateEdit.AllowDrop = true;
			this.SubmissionDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SubmissionDateEdit, "RPR_SubmissionDate");
			this.SubmissionDateEdit.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("5a007c1e-689c-45ce-8d35-2a092cdf1f6e", "Submission Date");
			this.SubmissionDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SubmissionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 235, true);
			this.SubmissionDateEdit.Name = "SubmissionDateEdit";
			this.SubmissionDateEdit.ReadOnly = true;
			this.SubmissionDateEdit.TabIndex = 7;
			// 
			// EmployeesInReviewFindBox
			// 
			this.EmployeesInReviewFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EmployeesInReviewFindBox, "RPR_S9_EmployeesInReview");
			this.EmployeesInReviewFindBox.CaptionResourceString = Enterprise.HRM.GUI.Res.GetData("908259cd-af37-45e8-a967-05c955f9cc80", "Employees In Review");
			this.EmployeesInReviewFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 265, true);
			this.EmployeesInReviewFindBox.Name = "EmployeesInReviewFindBox";
			this.EmployeesInReviewFindBox.ReadOnly = true;
			this.EmployeesInReviewFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.EmployeesInReviewFindBox.ParentType = null;
			this.EmployeesInReviewFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 31, true);
			this.EmployeesInReviewFindBox.TabIndex = 8;
			this.MainTabPage.PerformLayout();
			this.CurrencyFindBox.ResumeLayout(true);
			this.CurrencyFindBox.PerformLayout();
			this.EffectiveDateEdit.ResumeLayout(true);
			this.EffectiveDateEdit.PerformLayout();
			this.SubmissionDateEdit.ResumeLayout(true);
			this.SubmissionDateEdit.PerformLayout();
			this.EmployeesInReviewFindBox.ResumeLayout(true);
			this.EmployeesInReviewFindBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		void WorkflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);

		}

		#endregion

		protected ZWorkflowTabPage WorkflowTabPage;
		protected ZTextBox NameTextBox;
		protected ZTextBox TypeTextBox;
		protected ZTextBox StatusTextBox;
		protected ZTextBox PrimaryHierarchyTextBox;
		protected ZTextBox OverrideHierarchyTextBox;
		protected ZCodeFindBox CurrencyFindBox;
		protected ZDateEdit EffectiveDateEdit;
		protected ZDateTimeOffsetEdit SubmissionDateEdit;
		protected ZGuidFindBox EmployeesInReviewFindBox;
	}
}
