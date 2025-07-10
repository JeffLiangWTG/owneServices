using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class WorkflowExceptionTypeForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DefaultDurationHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UseStartEndToCalculateDuration = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.exceptionTypeGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Code = new Enterprise.ZArchitecture.ZTextBox();
			this.Description = new Enterprise.ZArchitecture.ZTextBox();
			this.Category = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JobType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsSystem = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCauseRequired = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsResolutionRequired = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.causesGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CausesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.resolutionsGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResolutionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.exceptionTypeGroupbox.SuspendLayout();
			this.Category.SuspendLayout();
			this.JobType.SuspendLayout();
			this.causesGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CausesGrid)).BeginInit();
			this.CausesGrid.SuspendLayout();
			this.resolutionsGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResolutionsGrid)).BeginInit();
			this.ResolutionsGrid.SuspendLayout();
			this.DurationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 543, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.exceptionTypeGroupbox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1069, 618, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1069, 618, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1069, 618, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 643, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(585, 5, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1078, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType);
			// 
			// exceptionTypeGroupbox
			// 
			this.exceptionTypeGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2983f1b-5b61-44b6-a47f-e473acaa25d5", "Exception Type");
			this.exceptionTypeGroupbox.Controls.Add(this.Code);
			this.exceptionTypeGroupbox.Controls.Add(this.Description);
			this.exceptionTypeGroupbox.Controls.Add(this.Category);
			this.exceptionTypeGroupbox.Controls.Add(this.JobType);
			this.exceptionTypeGroupbox.Controls.Add(this.IsSystem);
			this.exceptionTypeGroupbox.Controls.Add(this.IsActive);
			this.exceptionTypeGroupbox.Controls.Add(this.IsCauseRequired);
			this.exceptionTypeGroupbox.Controls.Add(this.IsResolutionRequired);
			this.exceptionTypeGroupbox.Controls.Add(this.DurationGroupBox);
			this.exceptionTypeGroupbox.Controls.Add(this.causesGroupbox);
			this.exceptionTypeGroupbox.Controls.Add(this.resolutionsGroupbox);
			this.exceptionTypeGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exceptionTypeGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exceptionTypeGroupbox.Name = "exceptionTypeGroupbox";
			this.exceptionTypeGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1077, 621, true);
			this.exceptionTypeGroupbox.TabIndex = 0;
			this.exceptionTypeGroupbox.TabStop = false;
			// 
			// Code
			// 
			this.BindingSource.SetBindingMember(this.Code, "WET_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_Code)));
			this.Code.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0c0adef7-f827-4529-81a2-3092dea134bc", "Code");
			this.Code.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 22, true);
			this.Code.Name = "Code";
			this.Code.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 16, true);
			this.Code.TabIndex = 0;
			// 
			// Description
			// 
			this.BindingSource.SetBindingMember(this.Description, "WET_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_Description)));
			this.Description.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1a4ddc8b-f7d6-46e7-aba5-a07a46d931d9", "Description");
			this.Description.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Description.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 52, true);
			this.Description.Name = "Description";
			this.Description.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 16, true);
			this.Description.TabIndex = 1;
			// 
			// Category
			// 
			this.Category.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Category, "WET_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Lookups.ExceptionCategories)));
			this.Category.BindToList = "Lookups+ExceptionCategories";
			this.Category.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ad557232-eafd-4b2b-bcfb-91e48b4a10b5", "Category");
			this.Category.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 82, true);
			this.Category.Name = "Category";
			this.Category.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 16, true);
			this.Category.TabIndex = 2;
			// 
			// JobType
			// 
			this.JobType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JobType, "WET_JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Lookups.JobTypes)));
			this.JobType.BindToList = "Lookups+JobTypes";
			this.JobType.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f58ff868-3bff-4aae-af3f-c00a13544453", "Job Type");
			this.JobType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 112, true);
			this.JobType.Name = "JobType";
			this.JobType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 16, true);
			this.JobType.TabIndex = 3;
			// 
			// IsSystem
			// 
			this.BindingSource.SetBindingMember(this.IsSystem, "WET_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_IsSystem)));
			this.IsSystem.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("41c687d6-f146-4623-9948-5ced9cae249e", "System");
			this.IsSystem.Enabled = false;
			this.IsSystem.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 22, true);
			this.IsSystem.Name = "IsSystem";
			this.IsSystem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 24, true);
			this.IsSystem.TabIndex = 4;
			// 
			// IsActive
			// 
			this.BindingSource.SetBindingMember(this.IsActive, "WET_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_IsActive)));
			this.IsActive.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b83febc7-b4d8-4914-a99a-c19d5968dea1", "Active");
			this.IsActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 52, true);
			this.IsActive.Name = "IsActive";
			this.IsActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 24, true);
			this.IsActive.TabIndex = 5;
			// 
			// IsCauseRequired
			// 
			this.BindingSource.SetBindingMember(this.IsCauseRequired, "WET_IsCauseRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_IsCauseRequired)));
			this.IsCauseRequired.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b3b7c311-34cb-42db-8e84-f3c03c17b317", "Cause Required");
			this.IsCauseRequired.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 82, true);
			this.IsCauseRequired.Name = "IsCauseRequired";
			this.IsCauseRequired.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 24, true);
			this.IsCauseRequired.TabIndex = 6;
			// 
			// IsResolutionRequired
			// 
			this.BindingSource.SetBindingMember(this.IsResolutionRequired, "WET_IsResolutionRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_IsResolutionRequired)));
			this.IsResolutionRequired.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b50404f4-9a3b-4203-9d81-00bcb9932da5", "Resolution Required");
			this.IsResolutionRequired.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 112, true);
			this.IsResolutionRequired.Name = "IsResolutionRequired";
			this.IsResolutionRequired.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 24, true);
			this.IsResolutionRequired.TabIndex = 7;
			// 
			// DurationGroupBox
			//
			this.DurationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f3d415f4-9125-42c4-8b2c-e4f21202cdd2", "Duration");
			this.DurationGroupBox.Controls.Add(this.DefaultDurationHoursCalcEdit);
			this.DurationGroupBox.Controls.Add(this.UseStartEndToCalculateDuration);
			this.DurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 138, true);
			this.DurationGroupBox.Name = "DurationGroupBox";
			this.DurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 81, true);
			this.DurationGroupBox.TabIndex = 8;
			this.DurationGroupBox.TabStop = false;
			// 
			// DefaultDurationHoursCalcEdit
			// 
			this.DefaultDurationHoursCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DefaultDurationHoursCalcEdit, "WET_DefaultDurationHours");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_DefaultDurationHours)));
			this.DefaultDurationHoursCalcEdit.CaptionResourceString = null;
			this.DefaultDurationHoursCalcEdit.DecimalPlaces = 2;
			this.DefaultDurationHoursCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("880404f4-9a3a-4203-9d81-11bcb9932dae", "Default Duration (Hours)", "Default Duration (Hours) for exceptions, applicable to shipments, consols and containers.");
			this.DefaultDurationHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 20, true);
			this.DefaultDurationHoursCalcEdit.Name = "DefaultDurationHoursCalcEdit";
			this.DefaultDurationHoursCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.DefaultDurationHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.DefaultDurationHoursCalcEdit.TabIndex = 9;
			this.DefaultDurationHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UseStartEndToCalculateDuration
			// 
			this.BindingSource.SetBindingMember(this.UseStartEndToCalculateDuration, "WET_UseStartEndToCalculateDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).WET_UseStartEndToCalculateDuration)));
			this.UseStartEndToCalculateDuration.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("110404f4-9a3b-4203-9d81-00bcb9932dae", "Use Start/End Dates to calculate Duration", "Use Start/End Dates to calculate Duration, applicable to shipments, consols and containers.");
			this.UseStartEndToCalculateDuration.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UseStartEndToCalculateDuration.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseStartEndToCalculateDuration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 51, true);
			this.UseStartEndToCalculateDuration.Name = "UseStartEndToCalculateDuration";
			this.UseStartEndToCalculateDuration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 19, true);
			this.UseStartEndToCalculateDuration.TabIndex = 11;
			// 
			// causesGroupbox
			// 
			this.causesGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("726C407B-C94B-46CF-9494-FB16EEF41C19", "Causes");
			this.causesGroupbox.Controls.Add(this.CausesGrid);
			this.causesGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 231, true);
			this.causesGroupbox.Name = "causesGroupbox";
			this.causesGroupbox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(15, true);
			this.causesGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 240, true);
			this.causesGroupbox.TabIndex = 12;
			this.causesGroupbox.TabStop = false;
			// 
			// CausesGrid
			// 
			this.CausesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CausesGrid, "Causes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Causes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionCause)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Causes)).SyncRoot)).WEC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionCause)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Causes)).SyncRoot)).WEC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionCause)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Causes)).SyncRoot)).WEC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionCause)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Causes)).SyncRoot)).WEC_IsDefault)));
			this.CausesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C7663F83-3173-4921-BA85-76655A21BA83", "Code");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "WEC_Code";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("02EB8A91-2C91-46A3-8768-538482DC4360", "Description");
			zTextBoxColumnStyleInfo6.ColumnName = "WEC_Description";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("97939916-F8EF-40A4-95A5-7BBCB18366F6", "Active");
			zCheckBoxColumnStyleInfo5.ColumnName = "WEC_IsActive";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5982250E-0D20-4284-A0E2-EDD84B27CA8B", "Default");
			zCheckBoxColumnStyleInfo6.ColumnName = "WEC_IsDefault";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.CausesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.CausesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.CausesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.CausesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.CausesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CausesGrid.GridId = "c06f787f-ad50-4964-9b30-1bd4e8e98718";
			this.CausesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CausesGrid.LayoutKey = "CausesGrid";
			this.CausesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 46, true);
			this.CausesGrid.Name = "CausesGrid";
			this.CausesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 240, true);
			this.CausesGrid.TabIndex = 8;
			// 
			// resolutionsGroupbox
			// 
			this.resolutionsGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DD39CDDC-1877-4D65-9778-9928144A07AA", "Resolutions");
			this.resolutionsGroupbox.Controls.Add(this.ResolutionsGrid);
			this.resolutionsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 231, true);
			this.resolutionsGroupbox.Name = "resolutionsGroupbox";
			this.resolutionsGroupbox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(15, true);
			this.resolutionsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 240, true);
			this.resolutionsGroupbox.TabIndex = 13;
			this.resolutionsGroupbox.TabStop = false;
			// 
			// ResolutionsGrid
			// 
			this.ResolutionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ResolutionsGrid, "Resolutions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Resolutions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionResolution)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Resolutions)).SyncRoot)).WER_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionResolution)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Resolutions)).SyncRoot)).WER_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionResolution)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Resolutions)).SyncRoot)).WER_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionResolution)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType)(null)).Resolutions)).SyncRoot)).WER_IsDefault)));
			this.ResolutionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F8758E54-313E-46C6-AFC6-800D6ADB2331", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WER_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("084F6A4C-0F75-41CC-8467-CA99CF06DFA5", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "WER_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("456710A7-A1B2-4DA9-B02F-587922911081", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "WER_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7AF7D9D3-85CC-4E21-868E-45B13A3FE67C", "Default");
			zCheckBoxColumnStyleInfo2.ColumnName = "WER_IsDefault";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ResolutionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ResolutionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ResolutionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ResolutionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ResolutionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResolutionsGrid.GridId = "ef6b0a3a-bf12-45dd-9de1-3769a9f97c4d";
			this.ResolutionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ResolutionsGrid.LayoutKey = "ResolutionsGrid";
			this.ResolutionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 46, true);
			this.ResolutionsGrid.Name = "ResolutionsGrid";
			this.ResolutionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 240, true);
			this.ResolutionsGrid.TabIndex = 14;
			// 
			// WorkflowExceptionTypeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e43f898a-ba12-11e3-a1b8-1c6f653fb9f3", "Exception Type");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 721, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 760, true);
			this.Name = "WorkflowExceptionTypeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.exceptionTypeGroupbox.ResumeLayout(false);
			this.exceptionTypeGroupbox.PerformLayout();
			this.Category.ResumeLayout(true);
			this.Category.PerformLayout();
			this.JobType.ResumeLayout(true);
			this.JobType.PerformLayout();
			this.causesGroupbox.ResumeLayout(false);
			this.causesGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CausesGrid)).EndInit();
			this.CausesGrid.ResumeLayout(false);
			this.CausesGrid.PerformLayout();
			this.resolutionsGroupbox.ResumeLayout(false);
			this.resolutionsGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ResolutionsGrid)).EndInit();
			this.ResolutionsGrid.ResumeLayout(false);
			this.ResolutionsGrid.PerformLayout();
			this.DurationGroupBox.ResumeLayout(false);
			this.DurationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox exceptionTypeGroupbox;
		private ZArchitecture.GUI.ZGroupBox causesGroupbox;
		private ZArchitecture.GUI.ZGroupBox resolutionsGroupbox;
		private ZArchitecture.ZTextBox Code;
		private ZArchitecture.ZTextBox Description;
		private ZArchitecture.GUI.ZDropEdit Category;
		private ZArchitecture.GUI.ZDropEdit JobType;
		private ZArchitecture.GUI.ZCheckBox IsSystem;
		private ZArchitecture.GUI.ZCheckBox IsActive;
		private ZArchitecture.GUI.ZCheckBox IsCauseRequired;
		private ZArchitecture.GUI.ZCheckBox IsResolutionRequired;
		private ZArchitecture.ZGrid CausesGrid;
		private ZArchitecture.ZGrid ResolutionsGrid;
		private ZArchitecture.GUI.ZGroupBox DurationGroupBox;
		private ZArchitecture.ZCalcEdit DefaultDurationHoursCalcEdit;
		private ZArchitecture.GUI.ZCheckBox UseStartEndToCalculateDuration;
		private ZArchitecture.ZTextBoxColumnStyleInfo CauseCode;
		private ZArchitecture.ZTextBoxColumnStyleInfo CauseDesc;
		private ZArchitecture.ZCheckBoxColumnStyleInfo CauseActive;
		private ZArchitecture.ZCheckBoxColumnStyleInfo CauseDefault;
		private ZArchitecture.ZTextBoxColumnStyleInfo ResolutionCode;
		private ZArchitecture.ZTextBoxColumnStyleInfo ResolutionDesc;
		private ZArchitecture.ZCheckBoxColumnStyleInfo ResolutionActive;
		private ZArchitecture.ZCheckBoxColumnStyleInfo ResolutionDefault;
	}
}
