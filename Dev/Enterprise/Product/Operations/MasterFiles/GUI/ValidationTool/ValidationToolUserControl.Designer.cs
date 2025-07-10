using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class ValidationToolUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo ZGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValidationToolUserControl));
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ValidationRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationRulesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ValidationRuleActionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationRulesFilter = new Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl(this.ValidationRulesGrid, Enterprise.MasterFiles.GUI.ValidationToolUserControl.GetValidationRulesFilterBusinessObject());
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).BeginInit();
			this.ValidationRulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesSplitContainer)).BeginInit();
			this.ValidationRulesSplitContainer.Panel1.SuspendLayout();
			this.ValidationRulesSplitContainer.Panel2.SuspendLayout();
			this.ValidationRulesSplitContainer.SuspendLayout();
			this.ValidationRulesFilter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRuleActionsGrid)).BeginInit();
			this.ValidationRuleActionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IValidationToolParent);
			//
			// ValidationRulesFilter
			// 
			this.ValidationRulesFilter.AllowDrop = true;
			this.ValidationRulesFilter.Dock = System.Windows.Forms.DockStyle.Top;
			this.ValidationRulesFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRulesFilter.Name = "ValidationRulesFilter";
			this.ValidationRulesFilter.AutoSize = true;
			this.ValidationRulesFilter.TabIndex = 1;
			this.ValidationRulesFilter.CaptionRenderingEnabled = true;
			this.ValidationRulesFilter.PerformSearch += ProcessTemplateValidationFilterStripControl_PerformSearch;
			// 
			// ValidationRulesGrid
			// 
			this.ValidationRulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValidationRulesGrid, "ProcessTemplateValidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Condition1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Condition2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Condition2Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_GC_Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_ValidationRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Severity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_Message)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_FieldToDisplayValidation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_LogValidationFailEvent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_ContextType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).P0V_RQT_RequestTypeOnFailure)));
			this.ValidationRulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P0V_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "P0V_Condition1";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "P0V_Condition2";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMacrosFindBoxColumnStyleInfo1.ColumnName = "P0V_Condition2Value";
			zMacrosFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zMacrosFindBoxColumnStyleInfo1.MacroType = Enterprise.ZArchitecture.GUI.MacroType.Antlr;
			zMacrosFindBoxColumnStyleInfo1.ShowIndex = true;
			zMacrosFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "P0V_GC_Company";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMacrosFindBoxColumnStyleInfo2.ColumnName = "P0V_ValidationRule";
			zMacrosFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zMacrosFindBoxColumnStyleInfo2.MacroType = Enterprise.ZArchitecture.GUI.MacroType.Antlr;
			zMacrosFindBoxColumnStyleInfo2.ShowIndex = true;
			zMacrosFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "P0V_Severity";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMacrosFindBoxColumnStyleInfo3.ColumnName = "P0V_Message";
			zMacrosFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zMacrosFindBoxColumnStyleInfo3.MacroType = Enterprise.ZArchitecture.GUI.MacroType.Antlr;
			zMacrosFindBoxColumnStyleInfo3.ShowIndex = true;
			zMacrosFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMacrosFindBoxColumnStyleInfo4.ColumnName = "P0V_FieldToDisplayValidation";
			zMacrosFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zMacrosFindBoxColumnStyleInfo4.MacroType = Enterprise.ZArchitecture.GUI.MacroType.Antlr;
			zMacrosFindBoxColumnStyleInfo4.ShowIndex = true;
			zMacrosFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "P0V_LogValidationFailEvent";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo8.ColumnName = "P0V_ContextType";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			ZGuidFindBoxColumnStyleInfo2.ColumnName = "P0V_RQT_RequestTypeOnFailure";
			ZGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			ZGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ValidationRulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ValidationRulesGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo2);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ValidationRulesGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo3);
			this.ValidationRulesGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo4);
			this.ValidationRulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ValidationRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.ValidationRulesGrid.ColumnStyles.Add(ZGuidFindBoxColumnStyleInfo2);
			this.ValidationRulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRulesGrid.GridId = "30f58023-3f00-4946-80ba-73ace9a12365";
			this.ValidationRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValidationRulesGrid.LayoutKey = "ValidationRulesGrid";
			this.ValidationRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRulesGrid.Name = "ValidationRulesGrid";
			this.ValidationRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 280, true);
			this.ValidationRulesGrid.TabIndex = 2;
			// 
			// ValidationRulesSplitContainer
			// 
			this.ValidationRulesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRulesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRulesSplitContainer.Name = "ValidationRulesSplitContainer";
			this.ValidationRulesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ValidationRulesSplitContainer.Panel1
			// 
			this.ValidationRulesSplitContainer.Panel1.Controls.Add(this.ValidationRulesGrid);
			this.ValidationRulesSplitContainer.Panel1.Controls.Add(this.ValidationRulesFilter);
			// 
			// ValidationRulesSplitContainer.Panel2
			// 
			this.ValidationRulesSplitContainer.Panel2.Controls.Add(this.ValidationRuleActionsGrid);
			this.ValidationRulesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 549, true);
			this.ValidationRulesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(280);
			this.ValidationRulesSplitContainer.SplitterWidth = 10;
			this.ValidationRulesSplitContainer.TabIndex = 3;
			// 
			// ValidationRuleActionsGrid
			// 
			this.ValidationRuleActionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValidationRuleActionsGrid, "ProcessTemplateValidations.ProcessTemplateValidationActions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).ProcessTemplateValidationActions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidationAction)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).ProcessTemplateValidationActions)).SyncRoot)).P0A_ActionSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTemplateValidationAction)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTemplateValidation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IValidationToolParent)(null)).ProcessTemplateValidations)).SyncRoot)).ProcessTemplateValidationActions)).SyncRoot)).ActionSourceDescription)));
			this.ValidationRuleActionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "P0A_ActionSource";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "ActionSourceDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.ValidationRuleActionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ValidationRuleActionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ValidationRuleActionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationRuleActionsGrid.GridId = "d7df0a82-c6c1-47ea-9a4e-20c5b1bd8fc5";
			this.ValidationRuleActionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValidationRuleActionsGrid.LayoutKey = "ValidationRuleActionsGrid";
			this.ValidationRuleActionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValidationRuleActionsGrid.Name = "ValidationRuleActionsGrid";
			this.ValidationRuleActionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 265, true);
			this.ValidationRuleActionsGrid.TabIndex = 0;
			// 
			// ValidationToolUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ValidationRulesSplitContainer);
			this.Name = "ValidationToolUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 549, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesGrid)).EndInit();
			this.ValidationRulesGrid.ResumeLayout(false);
			this.ValidationRulesGrid.PerformLayout();
			this.ValidationRulesSplitContainer.Panel1.ResumeLayout(false);
			this.ValidationRulesSplitContainer.Panel1.PerformLayout();
			this.ValidationRulesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ValidationRulesSplitContainer)).EndInit();
			this.ValidationRulesSplitContainer.ResumeLayout(false);
			this.ValidationRulesSplitContainer.PerformLayout();
			this.ValidationRulesFilter.ResumeLayout(true);
			this.ValidationRulesFilter.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ValidationRuleActionsGrid)).EndInit();
			this.ValidationRuleActionsGrid.ResumeLayout(false);
			this.ValidationRuleActionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ValidationRulesGrid;
		internal CargoWise.Windows.UI.KSplitContainer ValidationRulesSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl ValidationRulesFilter;
		internal ZArchitecture.ZGrid ValidationRuleActionsGrid;
	}
}
