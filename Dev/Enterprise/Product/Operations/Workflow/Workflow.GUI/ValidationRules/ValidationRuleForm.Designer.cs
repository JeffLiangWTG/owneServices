namespace Enterprise.Workflow.GUI
{
	partial class ValidationRuleForm : Enterprise.ZArchitecture.GUI.ZTemplateForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.Workflow.GUI.ValidationRuleForm.RuleMacroColumnStyleInfo ruleMacroColumnStyleInfo1 = new Enterprise.Workflow.GUI.ValidationRuleForm.RuleMacroColumnStyleInfo();
			Enterprise.Workflow.GUI.ValidationRuleForm.RuleMacroColumnStyleInfo messageLogMacroColumnStyleInfo1 = new Enterprise.Workflow.GUI.ValidationRuleForm.RuleMacroColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.nameBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descBox = new Enterprise.ZArchitecture.ZTextBox();
			this.codeBox = new Enterprise.ZArchitecture.ZTextBox();
			this.contextBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.activeBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.criteriaBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ruleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ruleGridLabel = new Enterprise.ZArchitecture.ZLabel();
			this.criteriaPopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.contextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ruleGrid)).BeginInit();
			this.ruleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 642, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.criteriaPopupButton);
			this.MainTabPage.Controls.Add(this.ruleGridLabel);
			this.MainTabPage.Controls.Add(this.ruleGrid);
			this.MainTabPage.Controls.Add(this.criteriaBox);
			this.MainTabPage.Controls.Add(this.activeBox);
			this.MainTabPage.Controls.Add(this.contextBox);
			this.MainTabPage.Controls.Add(this.codeBox);
			this.MainTabPage.Controls.Add(this.descBox);
			this.MainTabPage.Controls.Add(this.nameBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 613, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 613, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 25, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 613, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 642, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.UniversalValidationRuleSet);
			// 
			// nameBox
			// 
			this.BindingSource.SetBindingMember(this.nameBox, "VRS_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_Name)));
			this.nameBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.nameBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 20, true);
			this.nameBox.Name = "nameBox";
			this.nameBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 22, true);
			this.nameBox.TabIndex = 0;
			// 
			// descBox
			// 
			this.BindingSource.SetBindingMember(this.descBox, "VRS_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_Description)));
			this.descBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 20, true);
			this.descBox.Name = "descBox";
			this.descBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 22, true);
			this.descBox.TabIndex = 1;
			// 
			// codeBox
			// 
			this.BindingSource.SetBindingMember(this.codeBox, "VRS_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_Code)));
			this.codeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.codeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 57, true);
			this.codeBox.Name = "codeBox";
			this.codeBox.ReadOnly = true;
			this.codeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 22, true);
			this.codeBox.TabIndex = 3;
			// 
			// contextBox
			// 
			this.contextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.contextBox, "VRS_DataContext");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_DataContext)));
			this.contextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.contextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 57, true);
			this.contextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 22, true);
			this.contextBox.MaxItemsToShowInDropDown = 25;
			this.contextBox.Name = "contextBox";
			this.contextBox.PreBoundMaxLength = 42;
			this.contextBox.ShouldResizeByMaxLength = false;
			this.contextBox.ShowDescriptionBox = false;
			this.contextBox.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.contextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 22, true);
			this.contextBox.TabIndex = 2;
			// 
			// activeBox
			// 
			this.activeBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.activeBox, "VRS_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_IsActive)));
			this.activeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 58, true);
			this.activeBox.Name = "activeBox";
			this.activeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.activeBox.TabIndex = 4;
			// 
			// criteriaBox
			// 
			this.BindingSource.SetBindingMember(this.criteriaBox, "VRS_Criteria");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).VRS_Criteria)));
			this.criteriaBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.criteriaBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 94, true);
			this.criteriaBox.Name = "criteriaBox";
			this.criteriaBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 22, true);
			this.criteriaBox.SupportsMacroTemplates = true;
			this.criteriaBox.TabIndex = 5;
			this.criteriaBox.ReadOnlyChanged += new System.EventHandler(this.CriteriaBox_ReadOnlyChanged);
			// 
			// ruleGrid
			// 
			this.ruleGrid.AllowNavigation = false;
			this.ruleGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ruleGrid, "Rules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_MessageLog)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_BusinessRule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.UniversalValidationRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.UniversalValidationRuleSet)(null)).Rules)).SyncRoot)).VR_SystemLastEditUser)));
			this.ruleGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "VR_Sequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			messageLogMacroColumnStyleInfo1.ColumnName = "VR_MessageLog";
			messageLogMacroColumnStyleInfo1.ShowIndex = true;
			messageLogMacroColumnStyleInfo1.SupportsMacroTemplates = true;
			messageLogMacroColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			messageLogMacroColumnStyleInfo1.XmlType = typeof(Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment);
			messageLogMacroColumnStyleInfo1.RootTypeFunc = GetBizTypeForCurrentDataContext;
			ruleMacroColumnStyleInfo1.ColumnName = "VR_BusinessRule";
			ruleMacroColumnStyleInfo1.ShowIndex = true;
			ruleMacroColumnStyleInfo1.SupportsMacroTemplates = true;
			ruleMacroColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			ruleMacroColumnStyleInfo1.XmlType = typeof(Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment);
			ruleMacroColumnStyleInfo1.RootTypeFunc = GetBizTypeForCurrentDataContext;
			zDropEditColumnStyleInfo1.ColumnName = "VR_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "VR_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "VR_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "VR_SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "VR_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "VR_SystemLastEditUser";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ruleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(messageLogMacroColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(ruleMacroColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ruleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ruleGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ruleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ruleGrid.GridId = "e3d1eb67-fb24-4661-8be3-223cbd8804fe";
			this.ruleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ruleGrid.LayoutKey = "ruleGrid";
			this.ruleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 149, true);
			this.ruleGrid.Name = "ruleGrid";
			this.ruleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 461, true);
			this.ruleGrid.TabIndex = 9;
			// 
			// ruleGridLabel
			// 
			this.ruleGridLabel.AutoSize = true;
			this.ruleGridLabel.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("78e37bfe-38ca-4e66-9021-456a16231ea1", "Business Rule Set");
			this.ruleGridLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ruleGridLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 130, true);
			this.ruleGridLabel.Name = "ruleGridLabel";
			this.ruleGridLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.ruleGridLabel.TabIndex = 8;
			// 
			// criteriaPopupButton
			// 
			this.criteriaPopupButton.IsCaptionOverridden = true;
			this.criteriaPopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 93, true);
			this.criteriaPopupButton.Name = "criteriaPopupButton";
			this.criteriaPopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 23, true);
			this.criteriaPopupButton.TabIndex = 6;
			this.criteriaPopupButton.Text = "...";
			this.criteriaPopupButton.ToolTipCaption = null;
			this.criteriaPopupButton.UseVisualStyleBackColor = true;
			this.criteriaPopupButton.Click += new System.EventHandler(this.CriteriaPopupButton_Click);
			// 
			// ValidationRuleForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("afb19cf2-11ce-4aaa-94ba-e80cbf56c0ab", "Business Rule Set");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 698, true);
			this.DataSourceType = typeof(Enterprise.Workflow.Business.UniversalValidationRuleSet);
			this.Name = "ValidationRuleForm";
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
			this.contextBox.ResumeLayout(true);
			this.contextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ruleGrid)).EndInit();
			this.ruleGrid.ResumeLayout(false);
			this.ruleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox nameBox;
		private ZArchitecture.ZTextBox descBox;
		private ZArchitecture.ZTextBox codeBox;
		private ZArchitecture.GUI.ZDropEdit contextBox;
		private ZArchitecture.GUI.ZCheckBox activeBox;
		private ZArchitecture.ZTextBox criteriaBox;
		private ZArchitecture.ZGrid ruleGrid;
		private ZArchitecture.ZLabel ruleGridLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton criteriaPopupButton;
	}
}
