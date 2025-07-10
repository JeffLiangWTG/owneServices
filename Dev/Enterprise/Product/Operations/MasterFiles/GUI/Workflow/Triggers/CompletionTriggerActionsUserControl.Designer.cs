namespace Enterprise.MasterFiles.GUI
{
	partial class CompletionTriggerActionsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo zMacrosFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo zTimeEditExColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZTimeEditExColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CompletionTriggerActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CompletionTriggerActionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompletionTriggerActionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompletionTriggerActionGrid)).BeginInit();
			this.CompletionTriggerActionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.EntityFramework.ActiveBusinessObjectCollection<Enterprise.MasterFiles.Business.ProcessTaskNotification>);
			// 
			// CompletionTriggerActionGroupBox
			// 
			this.CompletionTriggerActionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bbcdac34-0ace-4549-9e38-2556e45663b6", "Completion Trigger Actions");
			this.CompletionTriggerActionGroupBox.Controls.Add(this.CompletionTriggerActionGrid);
			this.CompletionTriggerActionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompletionTriggerActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompletionTriggerActionGroupBox.Name = "CompletionTriggerActionGroupBox";
			this.CompletionTriggerActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 238, true);
			this.CompletionTriggerActionGroupBox.TabIndex = 10;
			this.CompletionTriggerActionGroupBox.TabStop = false;
			// 
			// CompletionTriggerActionGrid
			// 
			this.CompletionTriggerActionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CompletionTriggerActionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_TriggerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_Calc_TriggerParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_TriggerPartyService)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_TriggerParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_OH_Recipient)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_EmailAddr)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_MessagePurpose)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_ECS_MessageDeliveryContextSelector)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_SU_Document)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_FieldValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_EmailTextFallbackToTemplate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_SQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_P0_WorkflowTemplate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).TemplateSourcePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).StaffCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_RelatedEntityId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_Offset)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskNotification)(null)).PQ_ActionReference)));
			this.CompletionTriggerActionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PQ_TriggerType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.ColumnName = "PQ_Calc_TriggerParty";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "PQ_TriggerPartyService";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "PQ_TriggerParty";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "PQ_OH_Recipient";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo1.ColumnName = "PQ_EmailAddr";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.MacroClosingBracket = "*)";
			zTextBoxColumnStyleInfo1.MacroOpeningBracket = "(*";
			zTextBoxColumnStyleInfo1.SupportsMacroTemplates = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "PQ_MessagePurpose";
			zDropEditColumnStyleInfo5.IsMandatory = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PQ_ECS_MessageDeliveryContextSelector";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "PQ_SU_Document";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zMacrosFindBoxColumnStyleInfo1.ColumnName = "PQ_FieldName";
			zMacrosFindBoxColumnStyleInfo1.DataFieldsOnly = true;
			zMacrosFindBoxColumnStyleInfo1.ShowIndex = false;
			zMacrosFindBoxColumnStyleInfo1.IsUsedForExpressions = true;
			zMacrosFindBoxColumnStyleInfo1.UseFieldChangeMacroEvaluatorForPreview = true;
			zMacrosFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMacrosFindBoxColumnStyleInfo2.ColumnName = "PQ_FieldValue";
			zMacrosFindBoxColumnStyleInfo2.IsUsedForExpressions = true;
			zMacrosFindBoxColumnStyleInfo2.UseFieldChangeMacroEvaluatorForPreview = true;
			zMacrosFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "PQ_EmailTextFallbackToTemplate";
			zMultiLineTextBoxColumnInfo1.IsMandatory = true;
			zMultiLineTextBoxColumnInfo1.MacroClosingBracket = "*)";
			zMultiLineTextBoxColumnInfo1.MacroOpeningBracket = "(*";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.SupportsMacroTemplates = true;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "PQ_SQ";
			zGuidFindBoxColumnStyleInfo3.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "PQ_P0_WorkflowTemplate";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "TemplateSourcePK";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "StaffCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "PQ_RelatedEntityId";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTimeEditExColumnStyleInfo1.AllowNegative = true;
			zTimeEditExColumnStyleInfo1.ColumnName = "PQ_Offset";
			zTimeEditExColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("73521A07-C24C-4C4C-95A6-FEF44E54BBD7", "Offset", "Time Offset", "Offset in time between the event time and the time when a DLY event should be generated.");
			zTimeEditExColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "PQ_ActionReference";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("BEB184ED-846E-48DC-BAC3-3F3A0628AE77", "Reference", "Action Reference", "Free text reference that can be used to distinguish between DLY completion trigger actions.");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zMacrosFindBoxColumnStyleInfo2);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zTimeEditExColumnStyleInfo1);
			this.CompletionTriggerActionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CompletionTriggerActionGrid.CopySelectedRowsAllowed = true;
			this.CompletionTriggerActionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompletionTriggerActionGrid.GridId = "d04ca3ab-fd52-41b1-ba05-c2fb243a5ae8";
			this.CompletionTriggerActionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CompletionTriggerActionGrid.LayoutKey = "zGrid1";
			this.CompletionTriggerActionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.CompletionTriggerActionGrid.Name = "CompletionTriggerActionGrid";
			this.CompletionTriggerActionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 217, true);
			this.CompletionTriggerActionGrid.TabIndex = 20;
			// 
			// CompletionTriggerActionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompletionTriggerActionGroupBox);
			this.Name = "CompletionTriggerActionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 238, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompletionTriggerActionGroupBox.ResumeLayout(false);
			this.CompletionTriggerActionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CompletionTriggerActionGrid)).EndInit();
			this.CompletionTriggerActionGrid.ResumeLayout(false);
			this.CompletionTriggerActionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CompletionTriggerActionGroupBox;
		internal ZArchitecture.ZGrid CompletionTriggerActionGrid;
	}
}
