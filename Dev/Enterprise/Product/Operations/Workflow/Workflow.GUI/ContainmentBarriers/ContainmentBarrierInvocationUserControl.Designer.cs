namespace Enterprise.Workflow.GUI
{
	partial class ContainmentBarrierInvocationUserControl
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
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.QCBTaskDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
            this.ResponseHintLabel = new Enterprise.ZArchitecture.ZLabel();
            this.OutcomeChoicePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.IterationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.IterationTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.IterateReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.IterateFromTaskDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.IterateFromWorkflowDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
            this.IterationTasksHintLabel = new Enterprise.ZArchitecture.ZLabel();
            this.TaskPreviewGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ShouldCreateWorkflowForIterationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.CreateIterationButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ResourceUnderReviewDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.IterationGroupBox.SuspendLayout();
            this.IterationTableLayoutPanel.SuspendLayout();
            this.IterateReasonDropEdit.SuspendLayout();
            this.IterateFromTaskDropEdit.SuspendLayout();
            this.IterateFromWorkflowDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TaskPreviewGrid)).BeginInit();
            this.TaskPreviewGrid.SuspendLayout();
            this.ResourceUnderReviewDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ContainmentBarrierViewModel);
            // 
            // QCBTaskDetailsLabel
            // 
            this.QCBTaskDetailsLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.QCBTaskDetailsLabel, "TaskDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).TaskDetails)));
            this.QCBTaskDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.QCBTaskDetailsLabel.IsFontBold = true;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QCBTaskDetailsLabel, false);
            this.QCBTaskDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 4, true);
            this.QCBTaskDetailsLabel.Name = "QCBTaskDetailsLabel";
            this.QCBTaskDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.QCBTaskDetailsLabel.TabIndex = 8;
            // 
            // ResponseHintLabel
            // 
            this.ResponseHintLabel.AutoSize = true;
            this.ResponseHintLabel.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("cd8d3a1b-8d73-49fa-8892-a4deb9171d38", "Please select the outcome of this Quality Containment Barrier.");
            this.ResponseHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ResponseHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 50, true);
            this.ResponseHintLabel.Name = "ResponseHintLabel";
            this.ResponseHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 13, true);
            this.ResponseHintLabel.TabIndex = 7;
            // 
            // OutcomeChoicePanel
            // 
            this.OutcomeChoicePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutcomeChoicePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 70, true);
            this.OutcomeChoicePanel.Name = "OutcomeChoicePanel";
            this.OutcomeChoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 87, true);
            this.OutcomeChoicePanel.TabIndex = 6;
            // 
            // IterationGroupBox
            // 
            this.IterationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IterationGroupBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("35deb021-a7fc-44c9-b731-10fee3a8ea54", "Iterate From");
            this.IterationGroupBox.Controls.Add(this.IterationTableLayoutPanel);
            this.IterationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 160, true);
            this.IterationGroupBox.Name = "IterationGroupBox";
            this.IterationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 311, true);
            this.IterationGroupBox.TabIndex = 9;
            this.IterationGroupBox.TabStop = false;
            this.IterationGroupBox.Visible = false;
            // 
            // IterationTableLayoutPanel
            // 
            this.IterationTableLayoutPanel.ColumnCount = 2;
            this.IterationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.IterationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.IterationTableLayoutPanel.Controls.Add(this.IterateReasonDropEdit, 0, 0);
            this.IterationTableLayoutPanel.Controls.Add(this.IterateFromTaskDropEdit, 1, 1);
            this.IterationTableLayoutPanel.Controls.Add(this.IterateFromWorkflowDropEdit, 0, 1);
            this.IterationTableLayoutPanel.Controls.Add(this.IterationTasksHintLabel, 0, 2);
            this.IterationTableLayoutPanel.Controls.Add(this.TaskPreviewGrid, 0, 3);
            this.IterationTableLayoutPanel.Controls.Add(this.ShouldCreateWorkflowForIterationCheckBox, 0, 4);
            this.IterationTableLayoutPanel.Controls.Add(this.CreateIterationButton, 1, 4);
            this.IterationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IterationTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
            this.IterationTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
            this.IterationTableLayoutPanel.Name = "IterationTableLayoutPanel";
            this.IterationTableLayoutPanel.RowCount = 5;
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
            this.IterationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
            this.IterationTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 286, true);
            this.IterationTableLayoutPanel.TabIndex = 100;
            // 
            // IterateReasonDropEdit
            // 
            this.IterateReasonDropEdit.AllowDrop = true;
            this.IterateReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IterateReasonDropEdit, "IterateReasonPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterateReasonPK)));
            this.IterateReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 3, true);
            this.IterateReasonDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(64, 3, 3, 3, true);
            this.IterateReasonDropEdit.Name = "IterateReasonDropEdit";
            this.IterateReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 26, true);
            this.IterateReasonDropEdit.TabIndex = 2;
            // 
            // IterateFromTaskDropEdit
            // 
            this.IterateFromTaskDropEdit.AllowDrop = true;
            this.IterateFromTaskDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IterateFromTaskDropEdit, "IterateFromTaskPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterateFromTaskPK)));
            this.IterateFromTaskDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 33, true);
            this.IterateFromTaskDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(48, 3, 3, 3, true);
            this.IterateFromTaskDropEdit.Name = "IterateFromTaskDropEdit";
            this.IterateFromTaskDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 26, true);
            this.IterateFromTaskDropEdit.TabIndex = 4;
            // 
            // IterateFromWorkflowDropEdit
            // 
            this.IterateFromWorkflowDropEdit.AllowDrop = true;
            this.IterateFromWorkflowDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IterateFromWorkflowDropEdit, "IterateFromWorkflowPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterateFromWorkflowPK)));
            this.IterateFromWorkflowDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 33, true);
            this.IterateFromWorkflowDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(64, 3, 3, 3, true);
            this.IterateFromWorkflowDropEdit.Name = "IterateFromWorkflowDropEdit";
            this.IterateFromWorkflowDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 26, true);
            this.IterateFromWorkflowDropEdit.TabIndex = 3;
            this.IterateFromWorkflowDropEdit.SelectedIndexChanged += new System.EventHandler(this.IterateFromWorkflowDropEdit_SelectedIndexChanged);
            // 
            // IterationTasksHintLabel
            // 
            this.IterationTasksHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IterationTasksHintLabel.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IterationTasksHintLabel, "HintLabelText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).HintLabelText)));
            this.IterationTableLayoutPanel.SetColumnSpan(this.IterationTasksHintLabel, 2);
            this.IterationTasksHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.IterationTasksHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 66, true);
            this.IterationTasksHintLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 6, 3, 0, true);
            this.IterationTasksHintLabel.Name = "IterationTasksHintLabel";
            this.IterationTasksHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.IterationTasksHintLabel.TabIndex = 10;
            // 
            // TaskPreviewGrid
            // 
            this.TaskPreviewGrid.AllowNavigation = false;
            this.TaskPreviewGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.TaskPreviewGrid, "IterationTaskPreviews");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.TaskPreviewCopy)(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)).SyncRoot)).IncludeInIteration)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Workflow.Business.TaskPreviewCopy)(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)).SyncRoot)).Sequence)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.TaskPreviewCopy)(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)).SyncRoot)).TaskName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.TaskPreviewCopy)(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)).SyncRoot)).ResourceName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.TaskPreviewCopy)(((System.Collections.IList)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).IterationTaskPreviews)).SyncRoot)).WorkflowName)));
            this.TaskPreviewGrid.CaptionVisible = false;
            this.IterationTableLayoutPanel.SetColumnSpan(this.TaskPreviewGrid, 2);
            zCheckBoxColumnStyleInfo2.ColumnName = "IncludeInIteration";
            zCheckBoxColumnStyleInfo2.IsMandatory = true;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "Sequence";
            zCalcEditColumnStyleInfo2.Decimals = 0;
            zCalcEditColumnStyleInfo2.IsMandatory = true;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zTextBoxColumnStyleInfo4.ColumnName = "TaskName";
            zTextBoxColumnStyleInfo4.IsMandatory = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo5.ColumnName = "ResourceName";
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo6.ColumnName = "WorkflowName";
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.TaskPreviewGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.TaskPreviewGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.TaskPreviewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.TaskPreviewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.TaskPreviewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.TaskPreviewGrid.GridId = "08bd0a13-2212-4e40-83c9-d4fc4b0389cf";
            this.TaskPreviewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TaskPreviewGrid.LayoutKey = "TaskPreviewGrid";
            this.TaskPreviewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 82, true);
            this.TaskPreviewGrid.Name = "TaskPreviewGrid";
            this.TaskPreviewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 171, true);
            this.TaskPreviewGrid.TabIndex = 5;
            // 
            // ShouldCreateWorkflowForIterationCheckBox
            // 
            this.ShouldCreateWorkflowForIterationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BindingSource.SetBindingMember(this.ShouldCreateWorkflowForIterationCheckBox, "ShouldCreateWorkflowForIteration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).ShouldCreateWorkflowForIteration)));
            this.ShouldCreateWorkflowForIterationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 264, true);
            this.ShouldCreateWorkflowForIterationCheckBox.Name = "ShouldCreateWorkflowForIterationCheckBox";
            this.ShouldCreateWorkflowForIterationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 19, true);
            this.ShouldCreateWorkflowForIterationCheckBox.TabIndex = 11;
            this.ShouldCreateWorkflowForIterationCheckBox.UseVisualStyleBackColor = true;
            // 
            // CreateIterationButton
            // 
            this.CreateIterationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CreateIterationButton.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("7bfedba7-3241-4c0f-9507-dab47d943d61", "Create Iteration");
            this.CreateIterationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(531, 259, true);
            this.CreateIterationButton.Name = "CreateIterationButton";
            this.CreateIterationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 24, true);
            this.CreateIterationButton.TabIndex = 99;
            this.CreateIterationButton.ToolTipCaption = null;
            this.CreateIterationButton.UseVisualStyleBackColor = false;
            this.CreateIterationButton.Click += new System.EventHandler(this.CreateIterationButton_Click);
            // 
            // ResourceUnderReviewDropEdit
            // 
            this.ResourceUnderReviewDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ResourceUnderReviewDropEdit, "ResourceUnderReviewNK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ContainmentBarrierViewModel)(null)).ResourceUnderReviewNK)));
            this.ResourceUnderReviewDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 21, true);
            this.ResourceUnderReviewDropEdit.Name = "ResourceUnderReviewDropEdit";
            this.ResourceUnderReviewDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 26, true);
            this.ResourceUnderReviewDropEdit.TabIndex = 10;
            // 
            // ContainmentBarrierInvocationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ResourceUnderReviewDropEdit);
            this.Controls.Add(this.IterationGroupBox);
            this.Controls.Add(this.QCBTaskDetailsLabel);
            this.Controls.Add(this.ResponseHintLabel);
            this.Controls.Add(this.OutcomeChoicePanel);
            this.Name = "ContainmentBarrierInvocationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 474, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.IterationGroupBox.ResumeLayout(false);
            this.IterationGroupBox.PerformLayout();
            this.IterationTableLayoutPanel.ResumeLayout(false);
            this.IterationTableLayoutPanel.PerformLayout();
            this.IterateReasonDropEdit.ResumeLayout(true);
            this.IterateReasonDropEdit.PerformLayout();
            this.IterateFromTaskDropEdit.ResumeLayout(true);
            this.IterateFromTaskDropEdit.PerformLayout();
            this.IterateFromWorkflowDropEdit.ResumeLayout(true);
            this.IterateFromWorkflowDropEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TaskPreviewGrid)).EndInit();
            this.TaskPreviewGrid.ResumeLayout(false);
            this.TaskPreviewGrid.PerformLayout();
            this.ResourceUnderReviewDropEdit.ResumeLayout(true);
            this.ResourceUnderReviewDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel QCBTaskDetailsLabel;
		private ZArchitecture.ZLabel ResponseHintLabel;
		internal ZArchitecture.GUI.ZPanel OutcomeChoicePanel;
		private ZArchitecture.GUI.ZGroupBox IterationGroupBox;
		private ZArchitecture.GUI.ZButton CreateIterationButton;
		private ZArchitecture.GUI.ZGuidDropEdit IterateReasonDropEdit;
		private ZArchitecture.GUI.ZGuidDropEdit IterateFromWorkflowDropEdit;
		private ZArchitecture.GUI.ZGuidDropEdit IterateFromTaskDropEdit;
		private ZArchitecture.ZGrid TaskPreviewGrid;
		private ZArchitecture.ZLabel IterationTasksHintLabel;
		private ZArchitecture.GUI.ZDropEdit ResourceUnderReviewDropEdit;
		private ZArchitecture.GUI.ZCheckBox ShouldCreateWorkflowForIterationCheckBox;
		private CargoWise.Windows.UI.KTableLayoutPanel IterationTableLayoutPanel;
	}
}
