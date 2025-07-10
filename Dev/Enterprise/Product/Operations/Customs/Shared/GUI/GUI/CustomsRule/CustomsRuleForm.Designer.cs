using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI
{
	partial class CustomsRuleForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.startDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.endDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HolderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RuleDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RuleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.startDateEdit.SuspendLayout();
			this.endDateEdit.SuspendLayout();
			this.HolderGuidFindBox.SuspendLayout();
			this.RulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RuleGrid)).BeginInit();
			this.RuleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 402, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.RulesGroupBox);
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 380, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 380, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 380, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 402, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CustomsRule);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|45722e93-4b43-468b-9bf0-479484c7ce7e", "Details");
			this.DetailsGroupBox.Controls.Add(this.startDateEdit);
			this.DetailsGroupBox.Controls.Add(this.endDateEdit);
			this.DetailsGroupBox.Controls.Add(this.HolderGuidFindBox);
			this.DetailsGroupBox.Controls.Add(this.RuleDescriptionTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 159, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// startDateEdit
			// 
			this.startDateEdit.AllowDrop = true;
			this.startDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.startDateEdit, "CPH_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CustomsRule)(null)).CPH_StartDate)));
			this.startDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|B96910D3-4033-4C00-9DF1-D30C238120AE", "Start Date");
			this.startDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 71, true);
			this.startDateEdit.Name = "startDateEdit";
			this.startDateEdit.TabIndex = 2;
			// 
			// endDateEdit
			// 
			this.endDateEdit.AllowDrop = true;
			this.endDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.endDateEdit, "CPH_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CustomsRule)(null)).CPH_EndDate)));
			this.endDateEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|B674B2D4-35EC-4DB0-AC73-E0EBB806E42B", "End Date");
			this.endDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 97, true);
			this.endDateEdit.Name = "endDateEdit";
			this.endDateEdit.TabIndex = 3;
			// 
			// HolderGuidFindBox
			// 
			this.HolderGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HolderGuidFindBox, "CPH_OH_PermitHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CustomsRule)(null)).CPH_OH_PermitHolder)));
			this.HolderGuidFindBox.BindToForDescription = "OrganizationDescription";
			this.HolderGuidFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|FBDDE406-7728-4671-9882-CDF7F7928827", "Organization");
			this.HolderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 19, true);
			this.HolderGuidFindBox.Name = "HolderGuidFindBox";
			this.HolderGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.HolderGuidFindBox.ParentType = null;
			this.HolderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.HolderGuidFindBox.TabIndex = 0;
			// 
			// RuleDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.RuleDescriptionTextBox, "CPH_PermitDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsRule)(null)).CPH_PermitDescription)));
			this.RuleDescriptionTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|26439F7C-A8B1-4FFD-93F0-78CDE2E1DC8F", "Rule Description");
			this.RuleDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 45, true);
			this.RuleDescriptionTextBox.Name = "RuleDescriptionTextBox";
			this.RuleDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.RuleDescriptionTextBox.TabIndex = 1;
			// 
			// RulesGroupBox
			// 
			this.RulesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|2370c5a8-7f44-4c92-8962-a29755037548", "Rules");
			this.RulesGroupBox.Controls.Add(this.RuleGrid);
			this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 165, true);
			this.RulesGroupBox.Name = "RulesGroupBox";
			this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 200, true);
			this.RulesGroupBox.TabIndex = 1;
			this.RulesGroupBox.TabStop = false;
			// 
			// RuleGrid
			// 
			this.RuleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RuleGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsRule)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsRuleRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsRule)(null)).Rules)).SyncRoot)).CPR_RuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsRuleRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsRule)(null)).Rules)).SyncRoot)).CPR_ValueFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsRuleRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsRule)(null)).Rules)).SyncRoot)).CPR_ValueFrom_FieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsRuleRule)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsRule)(null)).Rules)).SyncRoot)).CPR_ValueTo)));
			this.RuleGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CPR_RuleCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "RuleCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.ColumnName = "CPR_ValueFrom";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CPR_ValueFrom_FieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.ColumnName = "CPR_ValueTo";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "CPR_ValueTo_FieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.RuleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RuleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.RuleGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.RuleGrid.GridId = "C3E46FF9-8DE0-49C4-A79E-E1D46E68D289";
			this.RuleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RuleGrid.LayoutKey = "RuleGrid";
			this.RuleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			this.RuleGrid.Name = "RuleGrid";
			this.RuleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 170, true);
			this.RuleGrid.TabIndex = 0;
			// 
			// CustomsRuleForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("CustomsRuleForm|d56eca86-4211-49c4-9344-161b2b4fea1c", "Rule");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 458, true);
			this.DataSourceType = typeof(Enterprise.Customs.Business.CustomsRule);
			this.Name = "CustomsRuleForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.startDateEdit.ResumeLayout(true);
			this.startDateEdit.PerformLayout();
			this.endDateEdit.ResumeLayout(true);
			this.endDateEdit.PerformLayout();
			this.HolderGuidFindBox.ResumeLayout(true);
			this.HolderGuidFindBox.PerformLayout();
			this.RulesGroupBox.ResumeLayout(false);
			this.RulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RuleGrid)).EndInit();
			this.RuleGrid.ResumeLayout(false);
			this.RuleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RulesGroupBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;

		private ZArchitecture.GUI.ZGuidFindBox HolderGuidFindBox;
		private ZArchitecture.ZTextBox RuleDescriptionTextBox;
		private ZArchitecture.ZGrid RuleGrid;
		private ZArchitecture.GUI.ZDateEdit startDateEdit;
		private ZArchitecture.GUI.ZDateEdit endDateEdit;
	}
}
