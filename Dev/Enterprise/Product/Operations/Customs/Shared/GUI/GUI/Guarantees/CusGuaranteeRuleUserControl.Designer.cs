namespace Enterprise.Customs.GUI.Permits
{
	partial class CusGuaranteeRuleUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.PermitRuleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitRuleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitRuleExceptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitRuleExceptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RuleDetailPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PermitRuleCodeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitRuleValueFromText = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitRuleValueFromTextDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitRuleValueFromTextCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PermitRuleValueToZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PermitRuleValueToTextDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitRuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PermitRuleGroupBox.SuspendLayout();
			this.PermitRuleDetailsGroupBox.SuspendLayout();
			this.PermitRuleExceptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitRuleExceptionGrid)).BeginInit();
			this.PermitRuleExceptionGrid.SuspendLayout();
			this.RuleDetailPanel.SuspendLayout();
			this.PermitRuleCodeZDropEdit.SuspendLayout();
			this.PermitRuleValueFromTextDropEdit.SuspendLayout();
			this.PermitRuleValueFromTextCodeFindBox.SuspendLayout();
			this.PermitRuleValueToTextDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitRuleGrid)).BeginInit();
			this.PermitRuleGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			// 
			// zPanel1
			//
			this.zPanel1.Controls.Add(this.PermitRuleGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "PermitRulePanel";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 350, true);
			// 
			// PermitRuleGroupBox
			// 
			this.PermitRuleGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|E0EA47C3 - E5D7 - 4693 - 8D62 - 0E8186220351", "Permit Rule");
			this.PermitRuleGroupBox.Controls.Add(this.PermitRuleDetailsGroupBox);
			this.PermitRuleGroupBox.Controls.Add(this.PermitRuleGrid);
			this.PermitRuleGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitRuleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitRuleGroupBox.Name = "PermitRuleGroupBox";
			this.PermitRuleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 350, true);
			this.PermitRuleGroupBox.TabIndex = 1;
			this.PermitRuleGroupBox.TabStop = false;
			// 
			// PermitRuleDetailsGroupBox
			// 
			this.PermitRuleDetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|670EEBC0 - 4F25 - 4800 - B862 - 788C04A1876C", "Rule Details");
			this.PermitRuleDetailsGroupBox.Controls.Add(this.PermitRuleExceptionGroupBox);
			this.PermitRuleDetailsGroupBox.Controls.Add(this.RuleDetailPanel);
			this.PermitRuleDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitRuleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 0, true);
			this.PermitRuleDetailsGroupBox.Name = "PermitRuleDetailsGroupBox";
			this.PermitRuleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 50, true);
			this.PermitRuleDetailsGroupBox.TabIndex = 2;
			this.PermitRuleDetailsGroupBox.TabStop = false;
			// 
			// PermitRuleExceptionGroupBox
			// 
			this.PermitRuleExceptionGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|C7661D09 - D9FA - 4E14 - B42D - 0111D085954B", "Rule Exception");
			this.PermitRuleExceptionGroupBox.Controls.Add(this.PermitRuleExceptionGrid);
			this.PermitRuleExceptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitRuleExceptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 69, true);
			this.PermitRuleExceptionGroupBox.Name = "PermitRuleExceptionGroupBox";
			this.PermitRuleExceptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.PermitRuleExceptionGroupBox.TabIndex = 2;
			this.PermitRuleExceptionGroupBox.TabStop = false;
			// 
			// PermitRuleExceptionGrid
			// 
			this.PermitRuleExceptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitRuleExceptionGrid, "CusGuaranteeRules.CusPermitRuleExceptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CusPermitRuleExceptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRuleException)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CusPermitRuleExceptions)).SyncRoot)).PermitRule.CPR_RuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRuleException)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CusPermitRuleExceptions)).SyncRoot)).CPE_ValueFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRuleException)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CusPermitRuleExceptions)).SyncRoot)).CPE_ValueFrom_FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRuleException)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CusPermitRuleExceptions)).SyncRoot)).CPE_ValueTo)));
			this.PermitRuleExceptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|0ECE42EF - C602 - 4794 - BCC3 - C88616139BA8", "Rule Code");
			zDropEditColumnStyleInfo1.ColumnName = "PermitRule+CPR_RuleCode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|3DF6F224 - D216 - 4F69 - B291 - C7A59DD8FE77", "Value From");
			zMultiControlColumnStyleInfo1.ColumnName = "CPE_ValueFrom";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CPE_ValueFrom_FieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|EE7114C3 - 812F - 42E5 - 85E7 - 44DB0F6857B4", "Value To");
			zTextBoxColumnStyleInfo1.ColumnName = "CPE_ValueTo";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PermitRuleExceptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PermitRuleExceptionGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.PermitRuleExceptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PermitRuleExceptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitRuleExceptionGrid.GridId = "DCBB7C60-32FC-4D61-B554-4993C0421C32";
			this.PermitRuleExceptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitRuleExceptionGrid.LayoutKey = "PermitRuleExceptionGrid";
			this.PermitRuleExceptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PermitRuleExceptionGrid.Name = "PermitRuleExceptionGrid";
			this.PermitRuleExceptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.PermitRuleExceptionGrid.TabIndex = 1;
			// 
			// RuleDetailPanel
			// 
			this.RuleDetailPanel.Controls.Add(this.PermitRuleCodeZDropEdit);
			this.RuleDetailPanel.Controls.Add(this.PermitRuleValueFromText);
			this.RuleDetailPanel.Controls.Add(this.PermitRuleValueFromTextDropEdit);
			this.RuleDetailPanel.Controls.Add(this.PermitRuleValueFromTextCodeFindBox);
			this.RuleDetailPanel.Controls.Add(this.PermitRuleValueToZTextBox);
			this.RuleDetailPanel.Controls.Add(this.PermitRuleValueToTextDropEdit);
			this.RuleDetailPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RuleDetailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.RuleDetailPanel.Name = "RuleDetailPanel";
			this.RuleDetailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 54, true);
			this.RuleDetailPanel.TabIndex = 1;
			// 
			// PermitRuleCodeZDropEdit
			// 
			this.PermitRuleCodeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitRuleCodeZDropEdit, "CusGuaranteeRules.CPR_RuleCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_RuleCode)));
			this.PermitRuleCodeZDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|0ECE42EF - C602 - 4794 - BCC3 - C88616139BA8", "Rule Code");
			this.PermitRuleCodeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 8, true);
			this.PermitRuleCodeZDropEdit.Name = "PermitRuleCodeZDropEdit";
			this.PermitRuleCodeZDropEdit.PreBoundMaxLength = 3;
			this.PermitRuleCodeZDropEdit.ShouldResizeByMaxLength = true;
			this.PermitRuleCodeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.PermitRuleCodeZDropEdit.TabIndex = 1;
			// 
			// PermitRuleValueFromText
			// 
			this.BindingSource.SetBindingMember(this.PermitRuleValueFromText, "CusGuaranteeRules.CPR_ValueFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueFrom)));
			this.PermitRuleValueFromText.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|3DF6F224 - D216 - 4F69 - B291 - C7A59DD8FE77", "Value From");
			this.PermitRuleValueFromText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 29, true);
			this.PermitRuleValueFromText.Name = "PermitRuleValueFromText";
			this.PermitRuleValueFromText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.PermitRuleValueFromText.TabIndex = 2;
			// 
			// PermitRuleValueFromTextDropEdit
			// 
			this.PermitRuleValueFromTextDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitRuleValueFromTextDropEdit, "CusGuaranteeRules.CPR_ValueFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueFrom)));
			this.PermitRuleValueFromTextDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|3DF6F224 - D216 - 4F69 - B291 - C7A59DD8FE77", "Value From");
			this.PermitRuleValueFromTextDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 29, true);
			this.PermitRuleValueFromTextDropEdit.Name = "PermitRuleValueFromTextDropEdit";
			this.PermitRuleValueFromTextDropEdit.ShouldResizeByMaxLength = false;
			this.PermitRuleValueFromTextDropEdit.ShowDescriptionBox = false;
			this.PermitRuleValueFromTextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.PermitRuleValueFromTextDropEdit.TabIndex = 3;
			// 
			// PermitRuleValueFromTextCodeFindBox
			// 
			this.PermitRuleValueFromTextCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitRuleValueFromTextCodeFindBox, "CusGuaranteeRules.CPR_ValueFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueFrom)));
			this.PermitRuleValueFromTextCodeFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|3DF6F224 - D216 - 4F69 - B291 - C7A59DD8FE77", "Value From");
			this.PermitRuleValueFromTextCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 29, true);
			this.PermitRuleValueFromTextCodeFindBox.Name = "PermitRuleValueFromTextCodeFindBox";
			this.PermitRuleValueFromTextCodeFindBox.ShouldResize = false;
			this.PermitRuleValueFromTextCodeFindBox.ShowDescriptionBox = false;
			this.PermitRuleValueFromTextCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.PermitRuleValueFromTextCodeFindBox.TabIndex = 4;
			// 
			// PermitRuleValueToZTextBox
			// 
			this.BindingSource.SetBindingMember(this.PermitRuleValueToZTextBox, "CusGuaranteeRules.CPR_ValueTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueTo)));
			this.PermitRuleValueToZTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|EE7114C3 - 812F - 42E5 - 85E7 - 44DB0F6857B4", "Value To");
			this.PermitRuleValueToZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 29, true);
			this.PermitRuleValueToZTextBox.Name = "PermitRuleValueToZTextBox";
			this.PermitRuleValueToZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.PermitRuleValueToZTextBox.TabIndex = 5;
			// 
			// PermitRuleValueToTextDropEdit
			// 
			this.PermitRuleValueToTextDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitRuleValueToTextDropEdit, "CusGuaranteeRules.CPR_ValueTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueTo)));
			this.PermitRuleValueToTextDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|EE7114C3 - 812F - 42E5 - 85E7 - 44DB0F6857B4", "Value To");
			this.PermitRuleValueToTextDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 29, true);
			this.PermitRuleValueToTextDropEdit.Name = "PermitRuleValueToTextDropEdit";
			this.PermitRuleValueToTextDropEdit.ShouldResizeByMaxLength = false;
			this.PermitRuleValueToTextDropEdit.ShowDescriptionBox = false;
			this.PermitRuleValueToTextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.PermitRuleValueToTextDropEdit.TabIndex = 6;
			// 
			// PermitRuleGrid
			// 
			this.PermitRuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitRuleGrid, "CusGuaranteeRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_RuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueFrom_FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusPermitRule)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CusGuaranteeRules)).SyncRoot)).CPR_ValueTo)));
			this.PermitRuleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|0ECE42EF - C602 - 4794 - BCC3 - C88616139BA8", "Rule Code");
			zDropEditColumnStyleInfo2.ColumnName = "CPR_RuleCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|3DF6F224 - D216 - 4F69 - B291 - C7A59DD8FE77", "Value From");
			zMultiControlColumnStyleInfo2.ColumnName = "CPR_ValueFrom";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "CPR_ValueFrom_FieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo3.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CusGuaranteeRuleUserControl|EE7114C3 - 812F - 42E5 - 85E7 - 44DB0F6857B4", "Value To");
			zMultiControlColumnStyleInfo3.ColumnName = "CPR_ValueTo";
			zMultiControlColumnStyleInfo3.FieldTypeColumnName = "CPR_ValueTo_FieldType";
			zMultiControlColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PermitRuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PermitRuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.PermitRuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo3);
			this.PermitRuleGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.PermitRuleGrid.GridId = "DCBB7C60-32FC-4D61-B554-4993C0421C32";
			this.PermitRuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitRuleGrid.LayoutKey = "PermitRuleGrid";
			this.PermitRuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitRuleGrid.Name = "PermitRuleGrid";
			this.PermitRuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 50, true);
			this.PermitRuleGrid.TabIndex = 1;
			// 

			// CusGuaranteeRuleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Name = "CusGuaranteeRuleUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 350, true);
			//
			//resume/perform
			//
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PermitRuleGroupBox.ResumeLayout(false);
			this.PermitRuleGroupBox.PerformLayout();
			this.PermitRuleDetailsGroupBox.ResumeLayout(false);
			this.PermitRuleDetailsGroupBox.PerformLayout();
			this.PermitRuleExceptionGroupBox.ResumeLayout(false);
			this.PermitRuleExceptionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitRuleExceptionGrid)).EndInit();
			this.PermitRuleExceptionGrid.ResumeLayout(false);
			this.PermitRuleExceptionGrid.PerformLayout();
			this.RuleDetailPanel.ResumeLayout(false);
			this.RuleDetailPanel.PerformLayout();
			this.PermitRuleCodeZDropEdit.ResumeLayout(true);
			this.PermitRuleCodeZDropEdit.PerformLayout();
			this.PermitRuleValueFromTextDropEdit.ResumeLayout(true);
			this.PermitRuleValueFromTextDropEdit.PerformLayout();
			this.PermitRuleValueFromTextCodeFindBox.ResumeLayout(true);
			this.PermitRuleValueFromTextCodeFindBox.PerformLayout();
			this.PermitRuleValueToTextDropEdit.ResumeLayout(true);
			this.PermitRuleValueToTextDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitRuleGrid)).EndInit();
			this.PermitRuleGrid.ResumeLayout(false);
			this.PermitRuleGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		//PAGE
		private ZArchitecture.GUI.ZGroupBox PermitRuleGroupBox;
		protected ZArchitecture.ZGrid PermitRuleGrid;
		private ZArchitecture.GUI.ZGroupBox PermitRuleDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit PermitRuleCodeZDropEdit;
		private ZArchitecture.ZTextBox PermitRuleValueFromText;
		private ZArchitecture.GUI.ZDropEdit PermitRuleValueFromTextDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox PermitRuleValueFromTextCodeFindBox;
		private ZArchitecture.ZTextBox PermitRuleValueToZTextBox;
		private ZArchitecture.GUI.ZDropEdit PermitRuleValueToTextDropEdit;
		protected ZArchitecture.GUI.ZGroupBox PermitRuleExceptionGroupBox;
		protected ZArchitecture.ZGrid PermitRuleExceptionGrid;
		protected ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZPanel RuleDetailPanel;
	}
}
