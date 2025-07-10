namespace Enterprise.Workflow.GUI
{
	partial class TemplateTriggersUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo triggerConditionValueColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.TriggerConditionValueColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
            this.TriggersSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.TriggersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.CompletionTriggerActionsControl = new Enterprise.MasterFiles.GUI.CompletionTriggerActionsUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TriggersSplitContainer)).BeginInit();
            this.TriggersSplitContainer.Panel1.SuspendLayout();
            this.TriggersSplitContainer.Panel2.SuspendLayout();
            this.TriggersSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TriggersGrid)).BeginInit();
            this.TriggersGrid.SuspendLayout();
            this.CompletionTriggerActionsControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ProcessTemplateTriggerCollection);
            // 
            // TriggersSplitContainer
            // 
            this.TriggersSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TriggersSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TriggersSplitContainer.Name = "TriggersSplitContainer";
            this.TriggersSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // TriggersSplitContainer.Panel1
            // 
            this.TriggersSplitContainer.Panel1.Controls.Add(this.TriggersGrid);
            // 
            // TriggersSplitContainer.Panel2
            // 
            this.TriggersSplitContainer.Panel2.Controls.Add(this.CompletionTriggerActionsControl);
            this.TriggersSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 529, true);
            this.TriggersSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(277);
            this.TriggersSplitContainer.TabIndex = 0;
            // 
            // TriggersGrid
            // 
            this.TriggersGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.TriggersGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_Sequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_IsActive)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerEventCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerEventDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerFieldName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerCondition)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerConditionValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerConditions.TriggerConditionValueFieldType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TemplateConditions.TemplateCondition1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TemplateConditions.TemplateCondition2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TemplateConditions.TemplateCondition2Value)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TemplateConditions.TemplateCondition2ValueFieldType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_RN_NKOriginCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_RN_NKDestinationCountry)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_TriggerFiredCountdown)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_SystemCreateTimeUtc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_SystemCreateUser)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_SystemLastEditTimeUtc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_SystemLastEditUser)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_IsEstimate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_SuppressDuplicates)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).P9T_DelayDuration)));
            this.TriggersGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "P9T_Sequence";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "P9T_Description";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zCheckBoxColumnStyleInfo1.ColumnName = "P9T_IsActive";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zDropEditColumnStyleInfo1.ColumnName = "TriggerConditions+TriggerEventCode";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.GroupName = Enterprise.Workflow.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc86cbe", "Trigger Conditions");
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zTextBoxColumnStyleInfo2.ColumnName = "TriggerConditions+TriggerEventDescription";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.GroupName = Enterprise.Workflow.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc86cbe", "Trigger Conditions");
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo2.ColumnName = "TriggerConditions+TriggerFieldName";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.GroupName = Enterprise.Workflow.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc86cbe", "Trigger Conditions");
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.ColumnName = "TriggerConditions+TriggerCondition";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.GroupName = Enterprise.Workflow.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc86cbe", "Trigger Conditions");
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            triggerConditionValueColumnStyleInfo1.BindToDecimalPlaces = null;
            triggerConditionValueColumnStyleInfo1.ColumnName = "TriggerConditions+TriggerConditionValue";
            triggerConditionValueColumnStyleInfo1.DefaultCollectionIndex = 0;
            triggerConditionValueColumnStyleInfo1.FieldTypeColumnName = "TriggerConditions.TriggerConditionValueFieldType";
            triggerConditionValueColumnStyleInfo1.GroupName = Enterprise.Workflow.GUI.Res.GetData("444e6e07-992c-43ea-9cca-431bddc86cbe", "Trigger Conditions");
            triggerConditionValueColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zDropEditColumnStyleInfo4.ColumnName = "TemplateConditions+TemplateCondition1";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.GroupName = Enterprise.Workflow.GUI.Res.GetData("80c59312-cef6-48fa-a9ea-bbf719d7709b", "Template Conditions");
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo5.ColumnName = "TemplateConditions+TemplateCondition2";
            zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo5.GroupName = Enterprise.Workflow.GUI.Res.GetData("80c59312-cef6-48fa-a9ea-bbf719d7709b", "Template Conditions");
            zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
            zMultiControlColumnStyleInfo1.Caption = "";
            zMultiControlColumnStyleInfo1.ColumnName = "TemplateConditions+TemplateCondition2Value";
            zMultiControlColumnStyleInfo1.DefaultCollectionIndex = 0;
            zMultiControlColumnStyleInfo1.FieldTypeColumnName = "TemplateConditions.TemplateCondition2ValueFieldType";
            zMultiControlColumnStyleInfo1.GroupName = Enterprise.Workflow.GUI.Res.GetData("80c59312-cef6-48fa-a9ea-bbf719d7709b", "Template Conditions");
            zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "P9T_RN_NKOriginCountry";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
            zCodeFindBoxColumnStyleInfo2.ColumnName = "P9T_RN_NKDestinationCountry";
            zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("9711280c-5a37-4243-8a62-d5716ace086f", "Trigger Remaining Countdown");
            zTextBoxColumnStyleInfo3.ColumnName = "P9T_TriggerFiredCountdown";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo1.ColumnName = "P9T_SystemCreateTimeUtc";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsReadOnly = true;
            zDateEditColumnStyleInfo1.IsVisible = false;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.ColumnName = "P9T_SystemCreateUser";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.IsVisible = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "P9T_SystemLastEditTimeUtc";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.IsVisible = false;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo5.ColumnName = "P9T_SystemLastEditUser";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.IsVisible = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("C3BB14B9-915E-4021-B4CC-AD3A4F4771AB", "Estimate", "Responds to Estimate Events", "Indicates whether the trigger responds to estimate events.");
            zCheckBoxColumnStyleInfo2.ColumnName = "P9T_IsEstimate";
            zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo2.IsVisible = false;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("D7E607E4-0299-468C-974B-2D111E792055", "Prevent Duplicates", "Prevent Firing on Duplicate Events", "When multiple events are raised within delay duration the trigger will only fire " +
        "once.");
            zCheckBoxColumnStyleInfo3.ColumnName = "P9T_SuppressDuplicates";
            zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("82A28F1B-A45A-481D-8155-359B7A77087A", "Delay Duration", "Delay Duration", "Delay processing trigger actions for the specified number of minutes.");
			zTimeEditExColumnStyleInfo1.AllowNegative = false;
            zTimeEditExColumnStyleInfo1.ColumnName = "P9T_DelayDuration";
            zTimeEditExColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.TriggersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TriggersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.TriggersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.TriggersGrid.ColumnStyles.Add(triggerConditionValueColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.TriggersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
            this.TriggersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.TriggersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.TriggersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.TriggersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.TriggersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.TriggersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.TriggersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.TriggersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
            this.TriggersGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
            this.TriggersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TriggersGrid.GridId = "0a3c9cdd-d0dd-4b97-8e5d-efd0ddf627e7";
            this.TriggersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TriggersGrid.LayoutKey = "TriggersGrid";
            this.TriggersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TriggersGrid.Name = "TriggersGrid";
            this.TriggersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 277, true);
            this.TriggersGrid.TabIndex = 0;
            // 
            // CompletionTriggerActionsControl
            // 
            this.CompletionTriggerActionsControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CompletionTriggerActionsControl, "TriggerActions");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.ActiveBusinessObjectCollection<Enterprise.MasterFiles.Business.ProcessTaskNotification>)(((Enterprise.Workflow.Business.ProcessTemplateTrigger)(null)).TriggerActions)));
            this.CompletionTriggerActionsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CompletionTriggerActionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CompletionTriggerActionsControl.Name = "CompletionTriggerActionsControl";
            this.CompletionTriggerActionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 248, true);
            this.CompletionTriggerActionsControl.TabIndex = 0;
            // 
            // TemplateTriggersUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TriggersSplitContainer);
            this.Name = "TemplateTriggersUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 529, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.TriggersSplitContainer.Panel1.ResumeLayout(false);
            this.TriggersSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.TriggersSplitContainer)).EndInit();
            this.TriggersSplitContainer.ResumeLayout(false);
            this.TriggersSplitContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TriggersGrid)).EndInit();
            this.TriggersGrid.ResumeLayout(false);
            this.TriggersGrid.PerformLayout();
            this.CompletionTriggerActionsControl.ResumeLayout(true);
            this.CompletionTriggerActionsControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer TriggersSplitContainer;
		private ZArchitecture.ZGrid TriggersGrid;
		private MasterFiles.GUI.CompletionTriggerActionsUserControl CompletionTriggerActionsControl;
	}
}
