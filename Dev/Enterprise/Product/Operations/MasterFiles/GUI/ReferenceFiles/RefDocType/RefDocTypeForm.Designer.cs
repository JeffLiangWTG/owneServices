
using System.ComponentModel;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDocTypeForm
	{
		#region Windows Form Designer generated code

		private ZTemplateTabControl DocTypeTabControl;
		private ZTabPage MainTabPage;
		private ZCheckBox ForceUserToReadCheckBox;
		private ZCheckBox SaveCopiesCheckBox;
		private ZCheckBox SaveVersionsCheckbox;
		private ZCheckBox IsPublishUpdatableCheckBox;
		private ZCheckBox IsPublishedCheckBox;
		private ZDropEdit DocTypeDropEdit;
		private ZCheckBox IsSystemCheckBox;
		private ZCheckBox IsActiveCheckBox;
		private ZTranslatableTextControl RT_DescriptionBoundTextBox;
		private ZTextBox RT_CodeBoundTextBox;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zLogsTabPage1;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZDropEdit DocumentDeliveredEventDropEdit;
		private ZDropEdit DocumentParseTypeDropEdit;
		private ZCheckBox MultipleTrackingCheckBox;
		private ZCheckBox IsCompanySpecificCheckBox;
		private ZCheckBox IsDepartmentSpecificCheckBox;
		private ZCheckBox IsBranchSpecificCheckBox;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DocTypeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.logMacroFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.logMacroTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsDepartmentSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsBranchSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCompanySpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MultipleTrackingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DocumentDeliveredEventDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DocumentParseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ForceUserToReadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveCopiesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SaveVersionsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsPublishUpdatableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DocTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RT_DescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.RT_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocTypeTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.DocumentDeliveredEventDropEdit.SuspendLayout();
			this.DocumentParseTypeDropEdit.SuspendLayout();
			this.DocTypeDropEdit.SuspendLayout();
			this.RT_DescriptionBoundTextBox.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 321, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(568);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocType);
			// 
			// DocTypeTabControl
			// 
			this.DocTypeTabControl.Controls.Add(this.MainTabPage);
			this.DocTypeTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.DocTypeTabControl.Controls.Add(this.zLogsTabPage1);
			this.DocTypeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.DocTypeTabControl.Name = "DocTypeTabControl";
			this.DocTypeTabControl.SelectedIndex = 0;
			this.DocTypeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 283, true);
			this.DocTypeTabControl.TabIndex = 1;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocTypeForm|013dccd8-4815-480a-b1b8-522466bf4599", "Doc Type");
			this.MainTabPage.Controls.Add(this.logMacroFormButton);
			this.MainTabPage.Controls.Add(this.logMacroTextBox);
			this.MainTabPage.Controls.Add(this.IsDepartmentSpecificCheckBox);
			this.MainTabPage.Controls.Add(this.IsBranchSpecificCheckBox);
			this.MainTabPage.Controls.Add(this.IsCompanySpecificCheckBox);
			this.MainTabPage.Controls.Add(this.MultipleTrackingCheckBox);
			this.MainTabPage.Controls.Add(this.DocumentDeliveredEventDropEdit);
			this.MainTabPage.Controls.Add(this.DocumentParseTypeDropEdit);
			this.MainTabPage.Controls.Add(this.ForceUserToReadCheckBox);
			this.MainTabPage.Controls.Add(this.SaveCopiesCheckBox);
			this.MainTabPage.Controls.Add(this.SaveVersionsCheckbox);
			this.MainTabPage.Controls.Add(this.IsPublishUpdatableCheckBox);
			this.MainTabPage.Controls.Add(this.IsPublishedCheckBox);
			this.MainTabPage.Controls.Add(this.DocTypeDropEdit);
			this.MainTabPage.Controls.Add(this.IsSystemCheckBox);
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.RT_DescriptionBoundTextBox);
			this.MainTabPage.Controls.Add(this.RT_CodeBoundTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 256, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// logMacroFormButton
			// 
			this.logMacroFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.logMacroFormButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("113ebd0e-8b7e-4020-8100-9d133c86c916", "...");
			this.logMacroFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 109, true);
			this.logMacroFormButton.Name = "logMacroFormButton";
			this.logMacroFormButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.logMacroFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 19, true);
			this.logMacroFormButton.TabIndex = 5;
			this.logMacroFormButton.UseVisualStyleBackColor = true;
			this.logMacroFormButton.Click += new System.EventHandler(this.logMacroFormButton_Click);
			// 
			// logMacroTextBox
			// 
			this.logMacroTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logMacroTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.logMacroTextBox, "RT_LogMacro");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_LogMacro)));
			this.logMacroTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d52ac870-1605-44ee-908e-bfe475b4d68d", "Ref. Macro", "Reference Macro", "Event Reference Macro", "");
			this.logMacroTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.logMacroTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 108, true);
			this.logMacroTextBox.Name = "logMacroTextBox";
			this.logMacroTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.logMacroTextBox.TabIndex = 4;
			// 
			// IsDepartmentSpecificCheckBox
			// 
			this.IsDepartmentSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsDepartmentSpecificCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDepartmentSpecificCheckBox, "RT_IsDepartmentSpecific");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsDepartmentSpecific)));
			this.IsDepartmentSpecificCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsDepartmentSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsDepartmentSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 206, true);
			this.IsDepartmentSpecificCheckBox.Name = "IsDepartmentSpecificCheckBox";
			this.IsDepartmentSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsDepartmentSpecificCheckBox.TabIndex = 17;
			// 
			// IsBranchSpecificCheckBox
			// 
			this.IsBranchSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsBranchSpecificCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsBranchSpecificCheckBox, "RT_IsBranchSpecific");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsBranchSpecific)));
			this.IsBranchSpecificCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsBranchSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsBranchSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 182, true);
			this.IsBranchSpecificCheckBox.Name = "IsBranchSpecificCheckBox";
			this.IsBranchSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsBranchSpecificCheckBox.TabIndex = 16;
			// 
			// IsCompanySpecificCheckBox
			// 
			this.IsCompanySpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsCompanySpecificCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsCompanySpecificCheckBox, "RT_IsCompanySpecific");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsCompanySpecific)));
			this.IsCompanySpecificCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsCompanySpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCompanySpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 158, true);
			this.IsCompanySpecificCheckBox.Name = "IsCompanySpecificCheckBox";
			this.IsCompanySpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsCompanySpecificCheckBox.TabIndex = 15;
			// 
			// MultipleTrackingCheckBox
			// 
			this.MultipleTrackingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MultipleTrackingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MultipleTrackingCheckBox, "RT_AllowMultiplePeriodicDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_AllowMultiplePeriodicDocs)));
			this.MultipleTrackingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.MultipleTrackingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MultipleTrackingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 229, true);
			this.MultipleTrackingCheckBox.Name = "MultipleTrackingCheckBox";
			this.MultipleTrackingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.MultipleTrackingCheckBox.TabIndex = 14;
			// 
			// DocumentDeliveredEventDropEdit
			// 
			this.DocumentDeliveredEventDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentDeliveredEventDropEdit, "RT_SE_NKDocumentReceivedEvent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_SE_NKDocumentReceivedEvent)));
			this.DocumentDeliveredEventDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 82, true);
			this.DocumentDeliveredEventDropEdit.Name = "DocumentDeliveredEventDropEdit";
			this.DocumentDeliveredEventDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.DocumentDeliveredEventDropEdit.TabIndex = 3;
			// 
			// DocumentParseTypeDropEdit
			// 
			this.DocumentParseTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentParseTypeDropEdit, "RT_ParseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_ParseType)));
			this.DocumentParseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 134, true);
			this.DocumentParseTypeDropEdit.Name = "DocumentParseTypeDropEdit";
			this.DocumentParseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.DocumentParseTypeDropEdit.SetDropButtonAvailability(EDocsParsingHelper.IsDocumentParsingEnabled());
			this.DocumentParseTypeDropEdit.Visible = EDocsParsingHelper.IsDocumentParsingEnabled();
			this.DocumentParseTypeDropEdit.TabIndex = 6;
			// 
			// ForceUserToReadCheckBox
			// 
			this.ForceUserToReadCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ForceUserToReadCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ForceUserToReadCheckBox, "RT_ForceUserToRead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_ForceUserToRead)));
			this.ForceUserToReadCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ForceUserToReadCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForceUserToReadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 229, true);
			this.ForceUserToReadCheckBox.Name = "ForceUserToReadCheckBox";
			this.ForceUserToReadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ForceUserToReadCheckBox.TabIndex = 10;
			// 
			// SaveCopiesCheckBox
			// 
			this.SaveCopiesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveCopiesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SaveCopiesCheckBox, "RT_LogSystemCreatedDocsToEDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_LogSystemCreatedDocsToEDocs)));
			this.SaveCopiesCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SaveCopiesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveCopiesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 182, true);
			this.SaveCopiesCheckBox.Name = "SaveCopiesCheckBox";
			this.SaveCopiesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SaveCopiesCheckBox.TabIndex = 12;
			// 
			// SaveVersionsCheckbox
			// 
			this.SaveVersionsCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveVersionsCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SaveVersionsCheckbox, "RT_OverrideVersions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_OverrideVersions)));
			this.SaveVersionsCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocTypeForm|3a1abea9-a5c6-4a2e-85a4-35963ef7f58a", "Keep Latest Version Only", "Specifies that only the most recent copy of this Doc Type is kept on file.");
			this.SaveVersionsCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SaveVersionsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SaveVersionsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 206, true);
			this.SaveVersionsCheckbox.Name = "SaveVersionsCheckbox";
			this.SaveVersionsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SaveVersionsCheckbox.TabIndex = 13;
			// 
			// IsPublishUpdatableCheckBox
			// 
			this.IsPublishUpdatableCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsPublishUpdatableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPublishUpdatableCheckBox, "RT_IsPublishUpdatable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsPublishUpdatable)));
			this.IsPublishUpdatableCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPublishUpdatableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPublishUpdatableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 158, true);
			this.IsPublishUpdatableCheckBox.Name = "IsPublishUpdatableCheckBox";
			this.IsPublishUpdatableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsPublishUpdatableCheckBox.TabIndex = 11;
			// 
			// IsPublishedCheckBox
			// 
			this.IsPublishedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsPublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsPublishedCheckBox, "RT_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsPublished)));
			this.IsPublishedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 206, true);
			this.IsPublishedCheckBox.Name = "IsPublishedCheckBox";
			this.IsPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsPublishedCheckBox.TabIndex = 9;
			// 
			// DocTypeDropEdit
			// 
			this.DocTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocTypeDropEdit, "RT_ReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_ReferenceType)));
			this.DocTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 58, true);
			this.DocTypeDropEdit.Name = "DocTypeDropEdit";
			this.DocTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.DocTypeDropEdit.TabIndex = 2;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "RT_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsSystem)));
			this.IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 158, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSystemCheckBox.TabIndex = 7;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 182, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 8;
			// 
			// RT_DescriptionBoundTextBox
			// 
			this.RT_DescriptionBoundTextBox.AcceptsReturn = false;
			this.RT_DescriptionBoundTextBox.AllowDrop = true;
			this.RT_DescriptionBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.RT_DescriptionBoundTextBox, "RT_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_Desc)));
			this.RT_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RT_DescriptionBoundTextBox.GridCurrent = null;
			this.RT_DescriptionBoundTextBox.GridMember = null;
			this.RT_DescriptionBoundTextBox.IsLanguageEditingEnabled = true;
			this.RT_DescriptionBoundTextBox.IsMultiLine = false;
			this.RT_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 34, true);
			this.RT_DescriptionBoundTextBox.Name = "RT_DescriptionBoundTextBox";
			this.RT_DescriptionBoundTextBox.ReadOnly = false;
			this.RT_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RT_DescriptionBoundTextBox.TabIndex = 1;
			// 
			// RT_CodeBoundTextBox
			// 
			this.RT_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RT_CodeBoundTextBox, "RT_DocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocType)(null)).RT_DocType)));
			this.RT_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 10, true);
			this.RT_CodeBoundTextBox.Name = "RT_CodeBoundTextBox";
			this.RT_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RT_CodeBoundTextBox.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 256, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 256, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(471, 290, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// RefDocTypeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocTypeForm|66faae8c-db39-4f50-8e8d-84a2e2d886c6", "Document Type");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 345, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.DocTypeTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocType);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 308, true);
			this.Name = "RefDocTypeForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DocTypeTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocTypeTabControl.ResumeLayout(false);
			this.DocTypeTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.DocumentDeliveredEventDropEdit.ResumeLayout(true);
			this.DocumentDeliveredEventDropEdit.PerformLayout();
			this.DocTypeDropEdit.ResumeLayout(true);
			this.DocTypeDropEdit.PerformLayout();
			this.RT_DescriptionBoundTextBox.ResumeLayout(true);
			this.RT_DescriptionBoundTextBox.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.DocumentParseTypeDropEdit.ResumeLayout(true);
			this.DocumentParseTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		private ZTextBox logMacroTextBox;
		private ZButton logMacroFormButton;
	}
}
