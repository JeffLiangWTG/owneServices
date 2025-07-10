using CargoWiseOne.ResourceStrings;

namespace Enterprise.Workflow.GUI
{
	partial class ReapplyWorkflowTemplatesConfigurationForm
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
		protected new void InitializeComponent()
		{
			this.tasksAndWorkflowsOptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.milestonesOptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.triggersOptionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.ReapplyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.recalculateReleaseGroupsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tasksAndWorkflowsOptionsDropEdit.SuspendLayout();
			this.milestonesOptionsDropEdit.SuspendLayout();
			this.triggersOptionsDropEdit.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 270, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions);
			// 
			// tasksAndWorkflowsOptionsDropEdit
			// 
			this.tasksAndWorkflowsOptionsDropEdit.AllowDrop = true;
			this.tasksAndWorkflowsOptionsDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.tasksAndWorkflowsOptionsDropEdit, "ReapplyWorkflowAndTasksOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions)(null)).ReapplyWorkflowAndTasksOptions)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.tasksAndWorkflowsOptionsDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.tasksAndWorkflowsOptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 33, true);
			this.tasksAndWorkflowsOptionsDropEdit.Name = "tasksAndWorkflowsOptionsDropEdit";
			this.tasksAndWorkflowsOptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 23, true);
			this.tasksAndWorkflowsOptionsDropEdit.TabIndex = 2;
			// 
			// milestonesOptionsDropEdit
			// 
			this.milestonesOptionsDropEdit.AllowDrop = true;
			this.milestonesOptionsDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.milestonesOptionsDropEdit, "ReapplyMilestonesOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions)(null)).ReapplyMilestonesOptions)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.milestonesOptionsDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.milestonesOptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 73, true);
			this.milestonesOptionsDropEdit.Name = "milestonesOptionsDropEdit";
			this.milestonesOptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 23, true);
			this.milestonesOptionsDropEdit.TabIndex = 3;
			// 
			// triggersOptionsDropEdit
			// 
			this.triggersOptionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.triggersOptionsDropEdit, "ReapplyTriggersOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions)(null)).ReapplyTriggersOptions)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.triggersOptionsDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.triggersOptionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 113, true);
			this.triggersOptionsDropEdit.Name = "triggersOptionsDropEdit";
			this.triggersOptionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.triggersOptionsDropEdit.TabIndex = 4;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 208, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 15, true);
			this.zLabel1.TabIndex = 5;
			this.zLabel1.UseMnemonic = false;
			this.zLabel1.CaptionResourceString = ResourceStringData.Empty;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("15da942e-acb5-4081-bcd5-6ca0d6d8cd7d", "Are you sure you want to proceed?");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 223, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 15, true);
			this.zLabel2.TabIndex = 6;
			this.zLabel2.UseMnemonic = false;
			// 
			// ReapplyButton
			// 
			this.ReapplyButton.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("d8bf9aa2-de06-49a1-bf1b-800f5ed165f5", "Reapply Templates");
			this.ReapplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 241, true);
			this.ReapplyButton.Name = "ReapplyButton";
			this.ReapplyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.ReapplyButton.TabIndex = 7;
			this.ReapplyButton.ToolTipCaption = null;
			this.ReapplyButton.UseVisualStyleBackColor = true;
			this.ReapplyButton.Click += new System.EventHandler(this.ReapplyButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("220e18db-de83-499d-86a7-f2b5cb607da9", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 241, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("cc506996-2cb8-4c4e-9c58-ed382472209f", "Options");
			this.zGroupBox1.Controls.Add(this.tasksAndWorkflowsOptionsDropEdit);
			this.zGroupBox1.Controls.Add(this.triggersOptionsDropEdit);
			this.zGroupBox1.Controls.Add(this.milestonesOptionsDropEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 137, true);
			this.zGroupBox1.TabIndex = 9;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("04ce896b-74bb-4626-98ad-7c3fe7a83d99", "Other Options");
			this.zGroupBox2.Controls.Add(this.recalculateReleaseGroupsCheckBox);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 50, true);
			this.zGroupBox2.TabIndex = 10;
			this.zGroupBox2.TabStop = false;
			// 
			// recalculateReleaseGroupsCheckBox
			// 
			this.recalculateReleaseGroupsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.recalculateReleaseGroupsCheckBox, "ReCalculateReleaseGroups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions)(null)).ReCalculateReleaseGroups)));
			this.recalculateReleaseGroupsCheckBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("7c0b48cb-5737-449d-a63d-ad66e51f15d9", "Recalculate Release Groups");
			this.recalculateReleaseGroupsCheckBox.Checked = true;
			this.recalculateReleaseGroupsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.recalculateReleaseGroupsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 21, true);
			this.recalculateReleaseGroupsCheckBox.Name = "recalculateReleaseGroupsCheckBox";
			this.recalculateReleaseGroupsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.recalculateReleaseGroupsCheckBox.TabIndex = 2;
			this.recalculateReleaseGroupsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReapplyWorkflowTemplatesConfigurationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("96643d14-a294-493a-b287-67cc51e56990", "Reapply Workflow Templates");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 294, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.ReapplyButton);
			this.Controls.Add(this.zLabel1);
			this.DataSourceType = typeof(Enterprise.Workflow.Business.ReapplyWorkflowTemplateUserOptions);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 220, true);
			this.Name = "ReapplyWorkflowTemplatesConfigurationForm";
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.ReapplyButton, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tasksAndWorkflowsOptionsDropEdit.ResumeLayout(true);
			this.tasksAndWorkflowsOptionsDropEdit.PerformLayout();
			this.milestonesOptionsDropEdit.ResumeLayout(true);
			this.milestonesOptionsDropEdit.PerformLayout();
			this.triggersOptionsDropEdit.ResumeLayout(true);
			this.triggersOptionsDropEdit.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit tasksAndWorkflowsOptionsDropEdit;
		public ZArchitecture.GUI.ZDropEdit milestonesOptionsDropEdit;
		public ZArchitecture.GUI.ZDropEdit triggersOptionsDropEdit;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel2;
		public ZArchitecture.GUI.ZButton ReapplyButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		public ZArchitecture.GUI.ZCheckBox recalculateReleaseGroupsCheckBox;
	}
}
