using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbDepartmentForm : ZForm
	{
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox GE_GEBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage AttributesTabPage;
		private Enterprise.ZArchitecture.ZTranslatableTextControl GE_DescBoundTextbox;
		private Enterprise.ZArchitecture.ZTextBox GE_CodeBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel ActivityLabel;
		private Enterprise.ZArchitecture.ZLabel ActivityBoundLabel;
		private Enterprise.ZArchitecture.ZLabel DirectionBoundLabel;
		private Enterprise.ZArchitecture.ZLabel DirectionLabel;
		private Enterprise.ZArchitecture.ZLabel ModeBoundLabel;
		private Enterprise.ZArchitecture.ZLabel ModeLabel;
		private Enterprise.ZArchitecture.ZLabel SystemLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckbox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsSystemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsCostCentreCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage DepartmentCharges;
		private Enterprise.ZArchitecture.ZGrid DeptChargesGrid;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl DepartmentTabControl;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private ZTabPage WorkingHoursTabPage;
		private GlbWorkTimeControl glbWorkTime;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.GE_DescBoundTextbox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.GE_GEBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GE_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartmentTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.AttributesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ModeBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ModeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DirectionBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DirectionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActivityBoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ActivityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WorkingHoursTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.glbWorkTime = new Enterprise.MasterFiles.GUI.GlbWorkTimeControl();
			this.DepartmentCharges = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeptChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.SystemLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsActiveCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCostCentreCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DepartmentTabControl.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			this.WorkingHoursTabPage.SuspendLayout();
			this.DepartmentCharges.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeptChargesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(298);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(299);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbDepartment);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 422, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// GE_DescBoundTextbox
			// 
			this.BindingSource.SetBindingMember(this.GE_DescBoundTextbox, "GE_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_Desc)));
			this.GE_DescBoundTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GE_DescBoundTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 32, true);
			this.GE_DescBoundTextbox.Name = "GE_DescBoundTextbox";
			this.GE_DescBoundTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.GE_DescBoundTextbox.TabIndex = 1;
			// 
			// GE_GEBoundGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.GE_GEBoundGuidFindBox, "GE_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_GE)));
			this.GE_GEBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 56, true);
			this.GE_GEBoundGuidFindBox.Name = "GE_GEBoundGuidFindBox";
			this.GE_GEBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.GE_GEBoundGuidFindBox.TabIndex = 2;
			// 
			// GE_CodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.GE_CodeBoundTextBox, "GE_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_Code)));
			this.GE_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 8, true);
			this.GE_CodeBoundTextBox.Name = "GE_CodeBoundTextBox";
			this.GE_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.GE_CodeBoundTextBox.TabIndex = 0;
			// 
			// DepartmentTabControl
			// 
			this.DepartmentTabControl.Controls.Add(this.AttributesTabPage);
			this.DepartmentTabControl.Controls.Add(this.WorkingHoursTabPage);
			this.DepartmentTabControl.Controls.Add(this.DepartmentCharges);
			this.DepartmentTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.DepartmentTabControl.Controls.Add(this.zLogsTabPage1);
			this.DepartmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 116, true);
			this.DepartmentTabControl.Name = "DepartmentTabControl";
			this.DepartmentTabControl.SelectedIndex = 0;
			this.DepartmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 300, true);
			this.DepartmentTabControl.TabIndex = 5;
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|8a345efe-4549-459e-8a8c-063b9e895bee", "Attributes");
			this.AttributesTabPage.Controls.Add(this.ModeBoundLabel);
			this.AttributesTabPage.Controls.Add(this.ModeLabel);
			this.AttributesTabPage.Controls.Add(this.DirectionBoundLabel);
			this.AttributesTabPage.Controls.Add(this.DirectionLabel);
			this.AttributesTabPage.Controls.Add(this.ActivityBoundLabel);
			this.AttributesTabPage.Controls.Add(this.ActivityLabel);
			this.AttributesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttributesTabPage.Name = "AttributesTabPage";
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.AttributesTabPage.TabIndex = 0;
			// 
			// ModeBoundLabel
			// 
			this.ModeBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ModeBoundLabel, "GE_Mode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_Mode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ModeBoundLabel, false);
			this.ModeBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 56, true);
			this.ModeBoundLabel.Name = "ModeBoundLabel";
			this.ModeBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.ModeBoundLabel.TabIndex = 8;
			this.ModeBoundLabel.Text = "bla";
			// 
			// ModeLabel
			// 
			this.ModeLabel.AutoSize = true;
			this.ModeLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|356f7627-5571-40ae-99f5-caebe753b0db", "Mode:");
			this.ModeLabel.IsFontBold = true;
			this.ModeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.ModeLabel.Name = "ModeLabel";
			this.ModeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.ModeLabel.TabIndex = 7;
			// 
			// DirectionBoundLabel
			// 
			this.DirectionBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DirectionBoundLabel, "GE_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_Direction)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DirectionBoundLabel, false);
			this.DirectionBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.DirectionBoundLabel.Name = "DirectionBoundLabel";
			this.DirectionBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.DirectionBoundLabel.TabIndex = 6;
			this.DirectionBoundLabel.Text = "bla";
			// 
			// DirectionLabel
			// 
			this.DirectionLabel.AutoSize = true;
			this.DirectionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|6cbefc2e-4e75-4ea2-8fc6-823a7798fc25", "Direction:");
			this.DirectionLabel.IsFontBold = true;
			this.DirectionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.DirectionLabel.Name = "DirectionLabel";
			this.DirectionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.DirectionLabel.TabIndex = 5;
			// 
			// ActivityBoundLabel
			// 
			this.ActivityBoundLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActivityBoundLabel, "GE_Activity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_Activity)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ActivityBoundLabel, false);
			this.ActivityBoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ActivityBoundLabel.Name = "ActivityBoundLabel";
			this.ActivityBoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.ActivityBoundLabel.TabIndex = 4;
			this.ActivityBoundLabel.Text = "bla";
			// 
			// ActivityLabel
			// 
			this.ActivityLabel.AutoSize = true;
			this.ActivityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|54001875-412b-4e66-9429-4fec07440f98", "Activity:");
			this.ActivityLabel.IsFontBold = true;
			this.ActivityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ActivityLabel.Name = "ActivityLabel";
			this.ActivityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 13, true);
			this.ActivityLabel.TabIndex = 3;
			// 
			// WorkingHoursTabPage
			// 
			this.WorkingHoursTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|1eca5eda-4e7e-42ff-b9c9-90fb32bd5ea3", "Working Hours");
			this.WorkingHoursTabPage.Controls.Add(this.glbWorkTime);
			this.WorkingHoursTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkingHoursTabPage.Name = "WorkingHoursTabPage";
			this.WorkingHoursTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.WorkingHoursTabPage.TabIndex = 1;
			// 
			// glbWorkTime
			// 
			this.glbWorkTime.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.glbWorkTime, "WorkTimeViewModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbWorkTimeViewModel)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).WorkTimeViewModel)));
			this.glbWorkTime.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glbWorkTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.glbWorkTime.Name = "glbWorkTime";
			this.glbWorkTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.glbWorkTime.TabIndex = 137;
			// 
			// DepartmentCharges
			// 
			this.DepartmentCharges.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|5d41218a-c41f-4753-b1cd-cc666e7455f8", "Department Charges");
			this.DepartmentCharges.Controls.Add(this.DeptChargesGrid);
			this.DepartmentCharges.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DepartmentCharges.Name = "DepartmentCharges";
			this.DepartmentCharges.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.DepartmentCharges.TabIndex = 2;
			// 
			// DeptChargesGrid
			// 
			this.DeptChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeptChargesGrid, "DeptCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).DeptCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbDeptCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).DeptCharges)).SyncRoot)).GD_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDeptCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).DeptCharges)).SyncRoot)).AccChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GlbDeptCharges)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).DeptCharges)).SyncRoot)).GD_SequenceNumber)));
			this.DeptChargesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GD_AC";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|dc1cd753-e800-4ce8-a43d-89a868f3e62f", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AccChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(210);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "GD_SequenceNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			this.DeptChargesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DeptChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeptChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DeptChargesGrid.GridId = "92648871-8d67-4115-a734-f48d4a4b1cf2";
			this.DeptChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeptChargesGrid.LayoutKey = "DeptChargesGrid";
			this.DeptChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.DeptChargesGrid.Name = "DeptChargesGrid";
			this.DeptChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 200, true);
			this.DeptChargesGrid.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.zStmNoteTabPage1.TabIndex = 3;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 273, true);
			this.zLogsTabPage1.TabIndex = 4;
			// 
			// SystemLabel
			// 
			this.BindingSource.SetBindingMember(this.SystemLabel, "SystemDept");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).SystemDept)));
			this.SystemLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|35412491-f9e1-4682-bd0d-91daf0f2ea1d", "(System Department)");
			this.SystemLabel.IsFontBold = true;
			this.SystemLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 12, true);
			this.SystemLabel.AutoSize = true;
			this.SystemLabel.Name = "SystemLabel";
			this.SystemLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 16, true);
			this.SystemLabel.TabIndex = 34;
			// 
			// IsActiveCheckbox
			// 
			this.IsActiveCheckbox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IsActiveCheckbox, "GE_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_IsActive)));
			this.IsActiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 80, true);
			this.IsActiveCheckbox.Name = "IsActiveCheckbox";
			this.IsActiveCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 24, true);
			this.IsActiveCheckbox.TabIndex = 3;
			this.IsActiveCheckbox.UseVisualStyleBackColor = false;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "GE_SystemCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_SystemCode)));
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 80, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsSystemCheckBox.TabIndex = 4;
			this.IsSystemCheckBox.UseVisualStyleBackColor = false;
			// 
			// IsCostCentreCheckBox
			// 
			this.IsCostCentreCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IsCostCentreCheckBox, "GE_IsCostCentre");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbDepartment)(null)).GE_IsCostCentre)));
			this.IsCostCentreCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCostCentreCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 80, true);
			this.IsCostCentreCheckBox.Name = "IsCostCentreCheckBox";
			this.IsCostCentreCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsCostCentreCheckBox.TabIndex = 5;
			this.IsCostCentreCheckBox.UseVisualStyleBackColor = false;
			// 
			// GlbDepartmentForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 474, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbDepartmentForm|f183947c-808a-4585-90b0-43b062602ffa", "Department");
			this.Controls.Add(this.IsCostCentreCheckBox);
			this.Controls.Add(this.IsSystemCheckBox);
			this.Controls.Add(this.IsActiveCheckbox);
			this.Controls.Add(this.SystemLabel);
			this.Controls.Add(this.DepartmentTabControl);
			this.Controls.Add(this.GE_CodeBoundTextBox);
			this.Controls.Add(this.GE_GEBoundGuidFindBox);
			this.Controls.Add(this.GE_DescBoundTextbox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbDepartment);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 500, true);
			this.Name = "GlbDepartmentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.GE_DescBoundTextbox, 0);
			this.Controls.SetChildIndex(this.GE_GEBoundGuidFindBox, 0);
			this.Controls.SetChildIndex(this.GE_CodeBoundTextBox, 0);
			this.Controls.SetChildIndex(this.DepartmentTabControl, 0);
			this.Controls.SetChildIndex(this.SystemLabel, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckbox, 0);
			this.Controls.SetChildIndex(this.IsSystemCheckBox, 0);
			this.Controls.SetChildIndex(this.IsCostCentreCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DepartmentTabControl.ResumeLayout(false);
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.WorkingHoursTabPage.ResumeLayout(false);
			this.DepartmentCharges.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DeptChargesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private IContainer components;
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
	}
}
