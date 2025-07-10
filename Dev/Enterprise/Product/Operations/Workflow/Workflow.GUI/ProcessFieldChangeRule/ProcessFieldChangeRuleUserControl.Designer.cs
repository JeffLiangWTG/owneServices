using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.GUI
{
	partial class ProcessFieldChangeRuleUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.workflowTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.referenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.groupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eventDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.extraInfoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.fieldsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.referenceDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.workflowTypeDropEdit.SuspendLayout();
			this.eventDropEdit.SuspendLayout();
			this.fieldsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fieldsGrid)).BeginInit();
			this.fieldsGrid.SuspendLayout();
			this.groupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ProcessFieldChangeRule);
			// 
			// isActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "PFR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_IsActive)));
			this.isActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 16, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 24, true);
			this.isActiveCheckBox.TabIndex = 1;
			this.isActiveCheckBox.UseVisualStyleBackColor = false;
			// 
			// workflowTypeDropEdit
			// 
			this.workflowTypeDropEdit.AccessibleDescription = "";
			this.workflowTypeDropEdit.AccessibleName = "";
			this.workflowTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workflowTypeDropEdit, "PFR_ProcessType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_ProcessType)));
			this.workflowTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 40, true);
			this.workflowTypeDropEdit.Name = "workflowTypeDropEdit";
			this.workflowTypeDropEdit.ShouldResizeByMaxLength = false;
			this.workflowTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.workflowTypeDropEdit.TabIndex = 2;
			// 
			// referenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.referenceTextBox, "PFR_Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_Reference)));
			this.referenceTextBox.CaptionResourceString = null;
			this.referenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.referenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 90, true);
			this.referenceTextBox.Name = "referenceTextBox";
			this.referenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.referenceTextBox.TabIndex = 4;
			// 
			// groupTextBox
			// 
			this.BindingSource.SetBindingMember(this.groupTextBox, "PFR_GroupName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_GroupName)));
			this.groupTextBox.CaptionResourceString = null;
			this.groupTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.groupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 16, true);
			this.groupTextBox.Name = "groupTextBox";
			this.groupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
			this.groupTextBox.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.AcceptsReturn = true;
			this.descriptionTextBox.AcceptsTab = true;
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "PFR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_Description)));
			this.descriptionTextBox.CaptionResourceString = null;
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.IsDynamicMultiline = true;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 149, true);
			this.descriptionTextBox.Multiline = true;
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 40, true);
			this.descriptionTextBox.TabIndex = 6;
			// 
			// eventDropEdit
			// 
			this.eventDropEdit.AccessibleDescription = "";
			this.eventDropEdit.AccessibleName = "";
			this.eventDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eventDropEdit, "PFR_SE_NKEvent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).PFR_SE_NKEvent)));
			this.eventDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 65, true);
			this.eventDropEdit.Name = "eventDropEdit";
			this.eventDropEdit.ShouldResizeByMaxLength = false;
			this.eventDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 20, true);
			this.eventDropEdit.TabIndex = 3;
			// 
			// extraInfoButton
			// 
			this.extraInfoButton.IsCaptionOverridden = true;
			this.extraInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 90, true);
			this.extraInfoButton.Name = "extraInfoButton";
			this.extraInfoButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.extraInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.extraInfoButton.TabIndex = 5;
			this.extraInfoButton.Text = "...";
			this.extraInfoButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.extraInfoButton.ToolTipCaption = null;
			this.extraInfoButton.UseVisualStyleBackColor = true;
			this.extraInfoButton.Click += new System.EventHandler(this.ExtraInfoButton_Click);
			// 
			// fieldsGroupBox
			// 
			this.fieldsGroupBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("0e27f9c9-8625-4386-bb9d-29447c643635", "Fields");
			this.fieldsGroupBox.Controls.Add(this.fieldsGrid);
			this.fieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fieldsGroupBox.Name = "fieldsGroupBox";
			this.fieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 696, true);
			this.fieldsGroupBox.TabIndex = 7;
			this.fieldsGroupBox.TabStop = false;
			// 
			// fieldsGrid
			// 
			this.fieldsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.fieldsGrid, "FieldsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).FieldsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRuleField)(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).FieldsForBinding)).SyncRoot)).PFL_FieldName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRuleField)(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).FieldsForBinding)).SyncRoot)).FieldDisplayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessFieldChangeRuleField)(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessFieldChangeRule)(null)).FieldsForBinding)).SyncRoot)).TableDisplayName)));
			this.fieldsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("36ab39aa-a889-4e89-b3aa-09edfe7e2c8b", "Field");
			zDropEditColumnStyleInfo1.ColumnName = "PFL_FieldName";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(225);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("6083AD9F-2244-4328-B0DE-9810672A67CF", "Field Name");
			zTextBoxColumnStyleInfo1.ColumnName = "FieldDisplayName";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("139F75CD-6399-46FF-B09F-0FCA899B8D65", "Table");
			zTextBoxColumnStyleInfo2.ColumnName = "TableDisplayName";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.fieldsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.fieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.fieldsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.fieldsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fieldsGrid.GridId = "9fc097e5-9f6a-4922-91cb-e983fec7dc3e";
			this.fieldsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.fieldsGrid.LayoutKey = "fieldsGrid";
			this.fieldsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.fieldsGrid.Name = "fieldsGrid";
			this.fieldsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 677, true);
			this.fieldsGrid.TabIndex = 5;
			// 
			// groupBox
			// 
			this.groupBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("663196E3-32C7-46BD-9B61-5719A18569D6", "Field Change Event");
			this.groupBox.Controls.Add(this.eventDropEdit);
			this.groupBox.Controls.Add(this.descriptionTextBox);
			this.groupBox.Controls.Add(this.groupTextBox);
			this.groupBox.Controls.Add(this.workflowTypeDropEdit);
			this.groupBox.Controls.Add(this.isActiveCheckBox);
			this.groupBox.Controls.Add(this.referenceTextBox);
			this.groupBox.Controls.Add(this.extraInfoButton);
			this.groupBox.Controls.Add(this.referenceDescriptionLabel);
			this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 300, true);
			this.groupBox.TabIndex = 5;
			this.groupBox.TabStop = false;
			// 
			// referenceDescriptionLabel
			// 
			this.referenceDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.referenceDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 113, true);
			this.referenceDescriptionLabel.Name = "referenceDescriptionLabel";
			this.referenceDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 31, true);
			this.referenceDescriptionLabel.TabIndex = 6;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.groupBox);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 1000, true);
			this.splitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.fieldsGroupBox);
			this.splitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(300);
			this.splitContainer.TabIndex = 0;
			// 
			// ProcessFieldChangeRuleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitContainer);
			this.Name = "ProcessFieldChangeRuleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 1000, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.workflowTypeDropEdit.ResumeLayout(true);
			this.workflowTypeDropEdit.PerformLayout();
			this.eventDropEdit.ResumeLayout(true);
			this.eventDropEdit.PerformLayout();
			this.fieldsGroupBox.ResumeLayout(false);
			this.fieldsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fieldsGrid)).EndInit();
			this.fieldsGrid.ResumeLayout(false);
			this.fieldsGrid.PerformLayout();
			this.groupBox.ResumeLayout(false);
			this.groupBox.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.ZTextBox groupTextBox;
		ZArchitecture.GUI.ZDropEdit workflowTypeDropEdit;
		ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		ZArchitecture.ZTextBox referenceTextBox;
		ZArchitecture.ZTextBox descriptionTextBox;
		ZArchitecture.GUI.ZDropEdit eventDropEdit;
		ZArchitecture.GUI.ZButton extraInfoButton;
		ZArchitecture.GUI.ZGroupBox fieldsGroupBox;
		ZArchitecture.GUI.ZGroupBox groupBox;
		ZArchitecture.ZGrid fieldsGrid;
		ZArchitecture.ZLabel referenceDescriptionLabel;
		CargoWise.Windows.UI.KSplitContainer splitContainer;
	}
}
