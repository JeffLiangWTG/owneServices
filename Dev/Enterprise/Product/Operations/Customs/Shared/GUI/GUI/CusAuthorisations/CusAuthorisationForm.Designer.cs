namespace Enterprise.Customs.GUI
{
	partial class CusAuthorisationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.AuthorisationNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AuthorisationHolderZGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.AuthorisationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AuthorisationDescriptionZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AuthorizationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
            this.AdHocCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.EndDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.StartDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.AuthorisationTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.AuthorisationRuleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AuthorisationRuleGrid = new Enterprise.ZArchitecture.ZGrid();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.LinkedAuthorisationRuleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LinkedAuthorisationRuleGrid = new Enterprise.ZArchitecture.ZGrid();
            this.NumberRangesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.customsNumberViewStmNumsTabPageUserControl = new Enterprise.MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AuthorisationHolderZGuidFindBox.SuspendLayout();
            this.AuthorisationDetailsGroupBox.SuspendLayout();
            this.AuthorizationAddressControl.SuspendLayout();
            this.EndDateZDateEdit.SuspendLayout();
            this.StartDateZDateEdit.SuspendLayout();
            this.AuthorisationTypeZDropEdit.SuspendLayout();
            this.AuthorisationRuleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AuthorisationRuleGrid)).BeginInit();
            this.AuthorisationRuleGrid.SuspendLayout();
            this.zPanel1.SuspendLayout();
            this.LinkedAuthorisationRuleGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedAuthorisationRuleGrid)).BeginInit();
            this.LinkedAuthorisationRuleGrid.SuspendLayout();
            this.NumberRangesTabPage.SuspendLayout();
            this.customsNumberViewStmNumsTabPageUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.NumberRangesTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 573, true);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NumberRangesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.Controls.Add(this.LinkedAuthorisationRuleGroupBox);
            this.MainTabPage.Controls.Add(this.AuthorisationRuleGroupBox);
            this.MainTabPage.Controls.Add(this.zPanel1);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 546, true);
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 546, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 546, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 573, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusAuthorisationHeader);
            // 
            // AuthorisationNumberZTextBox
            // 
            this.BindingSource.SetBindingMember(this.AuthorisationNumberZTextBox, "CPH_Number");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_Number)));
            this.AuthorisationNumberZTextBox.CaptionResourceString = null;
            this.AuthorisationNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 120, true);
            this.AuthorisationNumberZTextBox.Name = "AuthorisationNumberZTextBox";
            this.AuthorisationNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 20, true);
            this.AuthorisationNumberZTextBox.TabIndex = 4;
            // 
            // AuthorisationHolderZGuidFindBox
            // 
            this.AuthorisationHolderZGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorisationHolderZGuidFindBox, "CPH_OH_PermitHolder");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_OH_PermitHolder)));
            this.AuthorisationHolderZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 71, true);
            this.AuthorisationHolderZGuidFindBox.Name = "AuthorisationHolderZGuidFindBox";
            this.AuthorisationHolderZGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.AuthorisationHolderZGuidFindBox.ParentType = null;
            this.AuthorisationHolderZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 20, true);
            this.AuthorisationHolderZGuidFindBox.TabIndex = 2;
            // 
            // AuthorisationDetailsGroupBox
            // 
            this.AuthorisationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b252c430-a640-4b81-85be-ae4cd84f1faf", "Authorization Details");
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AuthorisationDescriptionZTextBox);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AuthorizationAddressControl);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AuthorisationHolderZGuidFindBox);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AuthorisationNumberZTextBox);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AdHocCheckBox);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.EndDateZDateEdit);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.StartDateZDateEdit);
            this.AuthorisationDetailsGroupBox.Controls.Add(this.AuthorisationTypeZDropEdit);
            this.AuthorisationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AuthorisationDetailsGroupBox.Name = "AuthorisationDetailsGroupBox";
            this.AuthorisationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 192, true);
            this.AuthorisationDetailsGroupBox.TabIndex = 2;
            this.AuthorisationDetailsGroupBox.TabStop = false;
            // 
            // AuthorisationDescriptionZTextBox
            // 
            this.BindingSource.SetBindingMember(this.AuthorisationDescriptionZTextBox, "CPH_PermitDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_PermitDescription)));
            this.AuthorisationDescriptionZTextBox.CaptionResourceString = null;
            this.AuthorisationDescriptionZTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.AuthorisationDescriptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 45, true);
            this.AuthorisationDescriptionZTextBox.Name = "AuthorisationDescriptionZTextBox";
            this.AuthorisationDescriptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 20, true);
            this.AuthorisationDescriptionZTextBox.TabIndex = 1;
            // 
            // AuthorizationAddressControl
            // 
            this.AuthorizationAddressControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorizationAddressControl, "CPH_OA_AppliesTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_OA_AppliesTo)));
            this.AuthorizationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 96, true);
            this.AuthorizationAddressControl.Name = "AuthorizationAddressControl";
            this.AuthorizationAddressControl.PopupCaption = "";
            this.AuthorizationAddressControl.ShowAddress = false;
            this.AuthorizationAddressControl.ShowOrganisationName = true;
            this.AuthorizationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
            this.AuthorizationAddressControl.TabIndex = 3;
            // 
            // AdHocCheckBox
            // 
            this.BindingSource.SetBindingMember(this.AdHocCheckBox, "CPH_IsAdHoc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_IsAdHoc)));
            this.AdHocCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("d8a27ee5-458b-4c15-b32d-2732e296befe", "Ad Hoc");
            this.AdHocCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 167, true);
            this.AdHocCheckBox.Name = "AdHocCheckBox";
            this.AdHocCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 24, true);
            this.AdHocCheckBox.TabIndex = 8;
            // 
            // EndDateZDateEdit
            // 
            this.EndDateZDateEdit.AllowDrop = true;
            this.EndDateZDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EndDateZDateEdit, "CPH_EndDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_EndDate)));
            this.EndDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 146, true);
            this.EndDateZDateEdit.Name = "EndDateZDateEdit";
            this.EndDateZDateEdit.TabIndex = 7;
            // 
            // StartDateZDateEdit
            // 
            this.StartDateZDateEdit.AllowDrop = true;
            this.StartDateZDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.StartDateZDateEdit, "CPH_StartDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_StartDate)));
            this.StartDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 146, true);
            this.StartDateZDateEdit.Name = "StartDateZDateEdit";
            this.StartDateZDateEdit.TabIndex = 6;
            // 
            // AuthorisationTypeZDropEdit
            // 
            this.AuthorisationTypeZDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AuthorisationTypeZDropEdit, "CPH_Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CPH_Type)));
            this.AuthorisationTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 20, true);
            this.AuthorisationTypeZDropEdit.Name = "AuthorisationTypeZDropEdit";
            this.AuthorisationTypeZDropEdit.PreBoundMaxLength = 3;
            this.AuthorisationTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 20, true);
            this.AuthorisationTypeZDropEdit.TabIndex = 0;
            // 
            // AuthorisationRuleGroupBox
            // 
            this.AuthorisationRuleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.AuthorisationRuleGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c3bb398a-af50-4dc9-afab-13e1eaf2bcc8", "Authorization Rules");
            this.AuthorisationRuleGroupBox.Controls.Add(this.AuthorisationRuleGrid);
            this.AuthorisationRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 203, true);
            this.AuthorisationRuleGroupBox.Name = "AuthorisationRuleGroupBox";
            this.AuthorisationRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 327, true);
            this.AuthorisationRuleGroupBox.TabIndex = 15;
            this.AuthorisationRuleGroupBox.TabStop = false;
            // 
            // AuthorisationRuleGrid
            // 
            this.AuthorisationRuleGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AuthorisationRuleGrid, "CusAuthorisationRules");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).CPR_RuleCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).CPR_ValueFrom)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).CPR_ValueFromFieldType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).CPR_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).CPR_DescriptionFieldType)));
            this.AuthorisationRuleGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo2.ColumnName = "CPR_RuleCode";
            zDropEditColumnStyleInfo2.IsSortable = false;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
            zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
            zMultiControlColumnStyleInfo2.ColumnName = "CPR_ValueFrom";
            zMultiControlColumnStyleInfo2.FieldTypeColumnName = "CPR_ValueFromFieldType";
            zMultiControlColumnStyleInfo2.IsSortable = false;
            zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
            zMultiControlColumnStyleInfo3.BindToDecimalPlaces = null;
            zMultiControlColumnStyleInfo3.ColumnName = "CPR_Description";
            zMultiControlColumnStyleInfo3.FieldTypeColumnName = "CPR_DescriptionFieldType";
            zMultiControlColumnStyleInfo3.IsSortable = false;
            zMultiControlColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
            this.AuthorisationRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.AuthorisationRuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
            this.AuthorisationRuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo3);
            this.AuthorisationRuleGrid.Dock = System.Windows.Forms.DockStyle.Left;
            this.AuthorisationRuleGrid.GridId = "DCBB7C60-32FC-4D61-B554-4993C0421C32";
            this.AuthorisationRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AuthorisationRuleGrid.LayoutKey = "PermitRuleGrid";
            this.AuthorisationRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.AuthorisationRuleGrid.Name = "AuthorisationRuleGrid";
            this.AuthorisationRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 308, true);
            this.AuthorisationRuleGrid.TabIndex = 16;
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.AuthorisationDetailsGroupBox);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 199, true);
            this.zPanel1.TabIndex = 0;
            // 
            // LinkedAuthorisationRuleGroupBox
            // 
            this.LinkedAuthorisationRuleGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LinkedAuthorisationRuleGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6d249c9a-953a-4c60-aefc-7e103415021a", "Linked Rules");
            this.LinkedAuthorisationRuleGroupBox.Controls.Add(this.LinkedAuthorisationRuleGrid);
            this.LinkedAuthorisationRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(521, 203, true);
            this.LinkedAuthorisationRuleGroupBox.Name = "LinkedAuthorisationRuleGroupBox";
            this.LinkedAuthorisationRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 327, true);
            this.LinkedAuthorisationRuleGroupBox.TabIndex = 17;
            this.LinkedAuthorisationRuleGroupBox.TabStop = false;
            // 
            // LinkedAuthorisationRuleGrid
            // 
            this.LinkedAuthorisationRuleGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LinkedAuthorisationRuleGrid, "CusAuthorisationRules.LinkedCusAuthorisationRules");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).LinkedCusAuthorisationRules)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.LinkedCusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).LinkedCusAuthorisationRules)).SyncRoot)).CPR_RuleCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.LinkedCusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).LinkedCusAuthorisationRules)).SyncRoot)).CPR_ValueFrom)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.LinkedCusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).LinkedCusAuthorisationRules)).SyncRoot)).CPR_ValueFromFieldType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.LinkedCusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CusAuthorisationHeader)(null)).CusAuthorisationRules)).SyncRoot)).LinkedCusAuthorisationRules)).SyncRoot)).CPR_Description)));
            this.LinkedAuthorisationRuleGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "CPR_RuleCode";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
            zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
            zMultiControlColumnStyleInfo1.ColumnName = "CPR_ValueFrom";
            zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CPR_ValueFromFieldType";
            zMultiControlColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
            zTextBoxColumnStyleInfo1.ColumnName = "CPR_Description";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
            this.LinkedAuthorisationRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.LinkedAuthorisationRuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
            this.LinkedAuthorisationRuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LinkedAuthorisationRuleGrid.Dock = System.Windows.Forms.DockStyle.Left;
            this.LinkedAuthorisationRuleGrid.GridId = "9680EC32-BF46-435A-9213-1491C7128E5C";
            this.LinkedAuthorisationRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LinkedAuthorisationRuleGrid.LayoutKey = "LinkedRuleGrid";
            this.LinkedAuthorisationRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.LinkedAuthorisationRuleGrid.Name = "LinkedAuthorisationRuleGrid";
            this.LinkedAuthorisationRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 288, true);
            this.LinkedAuthorisationRuleGrid.TabIndex = 18;
            // 
            // NumberRangesTabPage
            // 
            this.NumberRangesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("682409BC-E156-46E1-86A0-7814AF68C258", "Number Ranges");
            this.NumberRangesTabPage.Controls.Add(this.customsNumberViewStmNumsTabPageUserControl);
            this.NumberRangesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.NumberRangesTabPage.Name = "NumberRangesTabPage";
            this.NumberRangesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.NumberRangesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 546, true);
            this.NumberRangesTabPage.TabIndex = 3;
            this.NumberRangesTabPage.UseVisualStyleBackColor = true;
            // 
            // customsNumberViewStmNumsTabPageUserControl
            // 
            this.customsNumberViewStmNumsTabPageUserControl.AllowDrop = true;
            this.customsNumberViewStmNumsTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customsNumberViewStmNumsTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.customsNumberViewStmNumsTabPageUserControl.Name = "customsNumberViewStmNumsTabPageUserControl";
            this.customsNumberViewStmNumsTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 540, true);
            this.customsNumberViewStmNumsTabPageUserControl.TabIndex = 27;
            this.customsNumberViewStmNumsTabPageUserControl.TabStop = false;
            // 
            // CusAuthorisationForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 629, true);
            this.DataSourceType = typeof(Enterprise.Customs.Business.CusAuthorisationHeader);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 662, true);
            this.Name = "CusAuthorisationForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "Authorization";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AuthorisationHolderZGuidFindBox.ResumeLayout(true);
            this.AuthorisationHolderZGuidFindBox.PerformLayout();
            this.AuthorisationDetailsGroupBox.ResumeLayout(false);
            this.AuthorisationDetailsGroupBox.PerformLayout();
            this.AuthorizationAddressControl.ResumeLayout(true);
            this.AuthorizationAddressControl.PerformLayout();
            this.EndDateZDateEdit.ResumeLayout(true);
            this.EndDateZDateEdit.PerformLayout();
            this.StartDateZDateEdit.ResumeLayout(true);
            this.StartDateZDateEdit.PerformLayout();
            this.AuthorisationTypeZDropEdit.ResumeLayout(true);
            this.AuthorisationTypeZDropEdit.PerformLayout();
            this.AuthorisationRuleGroupBox.ResumeLayout(false);
            this.AuthorisationRuleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AuthorisationRuleGrid)).EndInit();
            this.AuthorisationRuleGrid.ResumeLayout(false);
            this.AuthorisationRuleGrid.PerformLayout();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.LinkedAuthorisationRuleGroupBox.ResumeLayout(false);
            this.LinkedAuthorisationRuleGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedAuthorisationRuleGrid)).EndInit();
            this.LinkedAuthorisationRuleGrid.ResumeLayout(false);
            this.LinkedAuthorisationRuleGrid.PerformLayout();
            this.NumberRangesTabPage.ResumeLayout(false);
            this.NumberRangesTabPage.PerformLayout();
            this.customsNumberViewStmNumsTabPageUserControl.ResumeLayout(true);
            this.customsNumberViewStmNumsTabPageUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGuidFindBox AuthorisationHolderZGuidFindBox;
		internal ZArchitecture.ZTextBox AuthorisationNumberZTextBox;
		protected ZArchitecture.GUI.ZDateEdit StartDateZDateEdit;
		private ZArchitecture.GUI.ZDateEdit EndDateZDateEdit;
		protected ZArchitecture.GUI.ZDropEdit AuthorisationTypeZDropEdit;
		private ZArchitecture.GUI.ZGroupBox AuthorisationRuleGroupBox;
		protected ZArchitecture.ZGrid AuthorisationRuleGrid;
		protected ZArchitecture.GUI.ZGroupBox AuthorisationDetailsGroupBox;
		protected ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZAddressControl AuthorizationAddressControl;
		protected ZArchitecture.ZTextBox AuthorisationDescriptionZTextBox;
		private ZArchitecture.GUI.ZGroupBox LinkedAuthorisationRuleGroupBox;
		private ZArchitecture.ZGrid LinkedAuthorisationRuleGrid;
		internal ZArchitecture.GUI.ZTabPage NumberRangesTabPage;
		internal ZArchitecture.GUI.ZCheckBox AdHocCheckBox;
		private Enterprise.MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl customsNumberViewStmNumsTabPageUserControl;
	}
}
