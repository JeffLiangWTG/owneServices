using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	partial class WorkItemStatusControl : ZUserControl
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
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusRowPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.CreatedTimeBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreatedByStaffBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FirstCBThatMissedDefectGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DefectIntroducedInTaskGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DefectIntroducedInWorkItemGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StatusBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaskStatusBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TaskAssignedStaffBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.StatusRowPanel.SuspendLayout();
			this.CreatedByStaffBox.SuspendLayout();
			this.FirstCBThatMissedDefectGuidDropEdit.SuspendLayout();
			this.DefectIntroducedInTaskGuidDropEdit.SuspendLayout();
			this.DefectIntroducedInWorkItemGuidFindBox.SuspendLayout();
			this.StatusBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("d69cd258-2541-4c84-b1c7-6fd38d738a88", "Current Task / State");
			this.StatusGroupBox.Controls.Add(this.StatusRowPanel);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 230, true);
			this.StatusGroupBox.TabIndex = 0;
			this.StatusGroupBox.TabStop = false;
			// 
			// StatusRowPanel
			// 
			this.StatusRowPanel.Alignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.StatusRowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusRowPanel.Controls.Add(this.CreatedTimeBox);
			this.StatusRowPanel.Controls.Add(this.CreatedByStaffBox);
			this.StatusRowPanel.Controls.Add(this.FirstCBThatMissedDefectGuidDropEdit);
			this.StatusRowPanel.Controls.Add(this.DefectIntroducedInTaskGuidDropEdit);
			this.StatusRowPanel.Controls.Add(this.DefectIntroducedInWorkItemGuidFindBox);
			this.StatusRowPanel.Controls.Add(this.StatusBox);
			this.StatusRowPanel.Controls.Add(this.TaskStatusBox);
			this.StatusRowPanel.Controls.Add(this.TaskAssignedStaffBox);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.StatusRowPanel, true);
			this.StatusRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 16, true);
			this.StatusRowPanel.Name = "StatusRowPanel";
			this.StatusRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.StatusRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 206, true);
			this.StatusRowPanel.TabIndex = 9;
			// 
			// CreatedTimeBox
			// 
			this.CreatedTimeBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CreatedTimeBox, "CreatedTimeAsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).CreatedTimeAsText)));
			this.CreatedTimeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.CreatedTimeBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("e9e7eae6-69c3-4fbe-8bb5-51407bc33e07", "Created Time");
			this.CreatedTimeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CreatedTimeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CreatedTimeBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CreatedTimeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.CreatedTimeBox.Name = "CreatedTimeBox";
			this.CreatedTimeBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.CreatedTimeBox, 4);
			this.CreatedTimeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.CreatedTimeBox.TabIndex = 5;
			// 
			// CreatedByStaffBox
			// 
			this.CreatedByStaffBox.AllowDrop = true;
			this.CreatedByStaffBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CreatedByStaffBox, "WKI_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_SystemCreateUser)));
			this.CreatedByStaffBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("3171d29c-dc64-4ce7-b2c9-9a112226aece", "Created By");
			this.CreatedByStaffBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.CreatedByStaffBox.Name = "CreatedByStaffBox";
			this.CreatedByStaffBox.PreBoundMaxLength = 3;
			this.StatusRowPanel.SetRow(this.CreatedByStaffBox, 3);
			this.CreatedByStaffBox.ShouldResize = true;
			this.CreatedByStaffBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
			this.CreatedByStaffBox.TabIndex = 4;
			// 
			// FirstCBThatMissedDefectGuidDropEdit
			// 
			this.FirstCBThatMissedDefectGuidDropEdit.AllowDrop = true;
			this.FirstCBThatMissedDefectGuidDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FirstCBThatMissedDefectGuidDropEdit, "WKI_P9_DefectFirstMissedInTask");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_P9_DefectFirstMissedInTask)));
			this.FirstCBThatMissedDefectGuidDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("47ca758d-a68b-4701-a9fc-5c93aef8201a", "First CB that Missed Defect", "The first Containment Barrier that could have reasonably caught this defect.");
			this.FirstCBThatMissedDefectGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.FirstCBThatMissedDefectGuidDropEdit.MaxItemsToShowInDropDown = 15;
			this.FirstCBThatMissedDefectGuidDropEdit.Name = "FirstCBThatMissedDefectGuidDropEdit";
			this.FirstCBThatMissedDefectGuidDropEdit.PreBoundMaxLength = 8;
			this.StatusRowPanel.SetRow(this.FirstCBThatMissedDefectGuidDropEdit, 7);
			this.FirstCBThatMissedDefectGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
			this.FirstCBThatMissedDefectGuidDropEdit.TabIndex = 8;
			// 
			// DefectIntroducedInTaskGuidDropEdit
			// 
			this.DefectIntroducedInTaskGuidDropEdit.AllowDrop = true;
			this.DefectIntroducedInTaskGuidDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DefectIntroducedInTaskGuidDropEdit, "WKI_P9_DefectCausedByTask");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_P9_DefectCausedByTask)));
			this.DefectIntroducedInTaskGuidDropEdit.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("9b95934d-4faa-48a3-bcd3-89ebb346b35f", "Defect Introduced in Task", "The task which most contributed to the introduction of this defect.");
			this.DefectIntroducedInTaskGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.DefectIntroducedInTaskGuidDropEdit.MaxItemsToShowInDropDown = 15;
			this.DefectIntroducedInTaskGuidDropEdit.Name = "DefectIntroducedInTaskGuidDropEdit";
			this.DefectIntroducedInTaskGuidDropEdit.PreBoundMaxLength = 8;
			this.StatusRowPanel.SetRow(this.DefectIntroducedInTaskGuidDropEdit, 6);
			this.DefectIntroducedInTaskGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
			this.DefectIntroducedInTaskGuidDropEdit.TabIndex = 7;
			// 
			// DefectIntroducedInWorkItemGuidFindBox
			// 
			this.DefectIntroducedInWorkItemGuidFindBox.AllowDrop = true;
			this.DefectIntroducedInWorkItemGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DefectIntroducedInWorkItemGuidFindBox, "DefectCausedByWorkItemPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).DefectCausedByWorkItemPK)));
			this.DefectIntroducedInWorkItemGuidFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("318F4D22-5F2A-4F7E-BEE7-A93D777C400D", "Defect Introduced in WI", "The Work Item which most contributed to the introduction of this defect.");
			this.DefectIntroducedInWorkItemGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.DefectIntroducedInWorkItemGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 127, true);
			this.DefectIntroducedInWorkItemGuidFindBox.Name = "DefectIntroducedInWorkItemGuidFindBox";
			this.StatusRowPanel.SetRow(this.DefectIntroducedInWorkItemGuidFindBox, 5);
			this.DefectIntroducedInWorkItemGuidFindBox.ShouldResize = true;
			this.DefectIntroducedInWorkItemGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
			this.DefectIntroducedInWorkItemGuidFindBox.TabIndex = 6;
			// 
			// StatusBox
			// 
			this.StatusBox.AllowDrop = true;
			this.StatusBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusBox, "WKI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).WKI_Status)));
			this.StatusBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("dd84bdf9-5242-4b53-9a8f-ec41b0dc1e43", "Work Item Status");
			this.StatusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.StatusBox.Name = "StatusBox";
			this.StatusBox.PreBoundMaxLength = 3;
			this.StatusRowPanel.SetRow(this.StatusBox, 2);
			this.StatusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 17, true);
			this.StatusBox.TabIndex = 3;
			// 
			// TaskStatusBox
			// 
			this.TaskStatusBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaskStatusBox, "OverallTaskStatusCodeAndDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).OverallTaskStatusCodeAndDescription)));
			this.TaskStatusBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TaskStatusBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("6e8113c0-27a7-4242-a4a2-df7ea7de0ea6", "Task Status");
			this.TaskStatusBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TaskStatusBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TaskStatusBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TaskStatusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.TaskStatusBox.Name = "TaskStatusBox";
			this.TaskStatusBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.TaskStatusBox, 0);
			this.TaskStatusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.TaskStatusBox.TabIndex = 1;
			// 
			// TaskAssignedStaffBox
			// 
			this.TaskAssignedStaffBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaskAssignedStaffBox, "CurrentOrNextTaskAssignedToCodeAndName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.WorkItem)(null)).CurrentOrNextTaskAssignedToCodeAndName)));
			this.TaskAssignedStaffBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TaskAssignedStaffBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("a0689bf1-b94d-4c22-8b37-b792c21d1430", "Task Assigned");
			this.TaskAssignedStaffBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TaskAssignedStaffBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TaskAssignedStaffBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TaskAssignedStaffBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.TaskAssignedStaffBox.Name = "TaskAssignedStaffBox";
			this.TaskAssignedStaffBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.TaskAssignedStaffBox, 1);
			this.TaskAssignedStaffBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 13, true);
			this.TaskAssignedStaffBox.TabIndex = 2;
			// 
			// WorkItemStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusGroupBox);
			this.Name = "WorkItemStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 230, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.StatusRowPanel.ResumeLayout(false);
			this.StatusRowPanel.PerformLayout();
			this.CreatedByStaffBox.ResumeLayout(true);
			this.CreatedByStaffBox.PerformLayout();
			this.FirstCBThatMissedDefectGuidDropEdit.ResumeLayout(true);
			this.FirstCBThatMissedDefectGuidDropEdit.PerformLayout();
			this.DefectIntroducedInTaskGuidDropEdit.ResumeLayout(true);
			this.DefectIntroducedInTaskGuidDropEdit.PerformLayout();
			this.DefectIntroducedInWorkItemGuidFindBox.ResumeLayout(true);
			this.DefectIntroducedInWorkItemGuidFindBox.PerformLayout();
			this.StatusBox.ResumeLayout(true);
			this.StatusBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private ZArchitecture.ZTextBox TaskAssignedStaffBox;
		public CargoWise.Windows.UI.Layout.RowLayoutPanel StatusRowPanel;
		private ZDropEdit StatusBox;
		private ZArchitecture.ZTextBox TaskStatusBox;
		private ZCodeFindBox CreatedByStaffBox;
		private ZArchitecture.ZTextBox CreatedTimeBox;
		private ZArchitecture.GUI.ZGuidDropEdit DefectIntroducedInTaskGuidDropEdit;
		private ZArchitecture.GUI.ZGuidDropEdit FirstCBThatMissedDefectGuidDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox DefectIntroducedInWorkItemGuidFindBox;
	}
}
