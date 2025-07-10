using Enterprise.ZArchitecture.GUI;
namespace Enterprise.ProcessManagement.GUI
{
	partial class ProjectStatusControl : ZUserControl
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
			this.ProjectClosedOrDeferredBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProjectCreatedBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreatedByStaffBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StatusBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TaskStatusBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProjectManagerFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TaskAssignedStaffBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OpportunityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OpportunityGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OpportunitySalesPersonCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.StatusRowPanel.SuspendLayout();
			this.CreatedByStaffBox.SuspendLayout();
			this.StatusBox.SuspendLayout();
			this.ProjectManagerFindBox.SuspendLayout();
			this.OpportunityDetailsGroupBox.SuspendLayout();
			this.OpportunityGuidFindBox.SuspendLayout();
			this.OpportunitySalesPersonCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.Project);
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("d69cd258-2541-4c84-b1c7-6fd38d738a88", "Current Task / State");
			this.StatusGroupBox.Controls.Add(this.StatusRowPanel);
			this.StatusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 220, true);
			this.StatusGroupBox.TabIndex = 0;
			this.StatusGroupBox.TabStop = false;
			// 
			// StatusRowPanel
			// 
			this.StatusRowPanel.Alignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;
			this.StatusRowPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusRowPanel.Controls.Add(this.ProjectClosedOrDeferredBox);
			this.StatusRowPanel.Controls.Add(this.ProjectCreatedBox);
			this.StatusRowPanel.Controls.Add(this.CreatedByStaffBox);
			this.StatusRowPanel.Controls.Add(this.StatusBox);
			this.StatusRowPanel.Controls.Add(this.TaskStatusBox);
			this.StatusRowPanel.Controls.Add(this.ProjectManagerFindBox);
			this.StatusRowPanel.Controls.Add(this.TaskAssignedStaffBox);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.StatusRowPanel, true);
			this.StatusRowPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 16, true);
			this.StatusRowPanel.Name = "StatusRowPanel";
			this.StatusRowPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.StatusRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 178, true);
			this.StatusRowPanel.TabIndex = 9;
			// 
			// ProjectClosedOrDeferredBox
			// 
			this.ProjectClosedOrDeferredBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectClosedOrDeferredBox, "ClosedOrDeferredDateAsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).ClosedOrDeferredDateAsText)));
			this.ProjectClosedOrDeferredBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ProjectClosedOrDeferredBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("6a1b535b-0e36-481d-a2ea-fa5b060e324c", "Project Closed");
			this.ProjectClosedOrDeferredBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProjectClosedOrDeferredBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectClosedOrDeferredBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectClosedOrDeferredBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.ProjectClosedOrDeferredBox.Name = "ProjectClosedOrDeferredBox";
			this.ProjectClosedOrDeferredBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.ProjectClosedOrDeferredBox, 5);
			this.ProjectClosedOrDeferredBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 13, true);
			this.ProjectClosedOrDeferredBox.TabIndex = 6;
			// 
			// ProjectCreatedBox
			// 
			this.ProjectCreatedBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectCreatedBox, "CreatedDateAsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).CreatedDateAsText)));
			this.ProjectCreatedBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ProjectCreatedBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("E6874D9B-2E57-4FE1-B5D1-79AD7929F772", "Project Created");
			this.ProjectCreatedBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProjectCreatedBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ProjectCreatedBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectCreatedBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 106, true);
			this.ProjectCreatedBox.Name = "ProjectCreatedBox";
			this.ProjectCreatedBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.ProjectCreatedBox, 4);
			this.ProjectCreatedBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 13, true);
			this.ProjectCreatedBox.TabIndex = 5;
			// 
			// CreatedByStaffBox
			// 
			this.CreatedByStaffBox.AllowDrop = true;
			this.CreatedByStaffBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CreatedByStaffBox, "WKP_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_SystemCreateUser)));
			this.CreatedByStaffBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("3171d29c-dc64-4ce7-b2c9-9a112226aece", "Created By");
			this.CreatedByStaffBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.CreatedByStaffBox.Name = "CreatedByStaffBox";
			this.CreatedByStaffBox.PreBoundMaxLength = 3;
			this.StatusRowPanel.SetRow(this.CreatedByStaffBox, 3);
			this.CreatedByStaffBox.ShouldResize = true;
			this.CreatedByStaffBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.CreatedByStaffBox.TabIndex = 4;
			// 
			// StatusBox
			// 
			this.StatusBox.AllowDrop = true;
			this.StatusBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusBox, "WKP_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_Status)));
			this.StatusBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("c79c3749-5244-4642-828f-f6f6e45c9602", "Status");
			this.StatusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.StatusBox.Name = "StatusBox";
			this.StatusBox.PreBoundMaxLength = 3;
			this.StatusRowPanel.SetRow(this.StatusBox, 2);
			this.StatusBox.ShouldResizeByMaxLength = true;
			this.StatusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.StatusBox.TabIndex = 3;
			// 
			// TaskStatusBox
			// 
			this.TaskStatusBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaskStatusBox, "OverallTaskStatusCodeAndDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).OverallTaskStatusCodeAndDescription)));
			this.TaskStatusBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TaskStatusBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("6e8113c0-27a7-4242-a4a2-df7ea7de0ea6", "Task Status");
			this.TaskStatusBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TaskStatusBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TaskStatusBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TaskStatusBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.TaskStatusBox.Name = "TaskStatusBox";
			this.TaskStatusBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.TaskStatusBox, 0);
			this.TaskStatusBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 13, true);
			this.TaskStatusBox.TabIndex = 1;
			// 
			// ProjectManagerFindBox
			// 
			this.ProjectManagerFindBox.AllowDrop = true;
			this.ProjectManagerFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProjectManagerFindBox, "WKP_GS_NKProjectManager");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_GS_NKProjectManager)));
			this.ProjectManagerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.ProjectManagerFindBox.Name = "ProjectManagerFindBox";
			this.ProjectManagerFindBox.PreBoundMaxLength = 5;
			this.StatusRowPanel.SetRow(this.ProjectManagerFindBox, 6);
			this.ProjectManagerFindBox.ShouldResize = true;
			this.ProjectManagerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.ProjectManagerFindBox.TabIndex = 7;
			// 
			// TaskAssignedStaffBox
			// 
			this.TaskAssignedStaffBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaskAssignedStaffBox, "CurrentOrNextTaskAssignedToCodeAndName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).CurrentOrNextTaskAssignedToCodeAndName)));
			this.TaskAssignedStaffBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TaskAssignedStaffBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("a0689bf1-b94d-4c22-8b37-b792c21d1430", "Task Assigned");
			this.TaskAssignedStaffBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TaskAssignedStaffBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TaskAssignedStaffBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.TaskAssignedStaffBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.TaskAssignedStaffBox.Name = "TaskAssignedStaffBox";
			this.TaskAssignedStaffBox.ReadOnly = true;
			this.StatusRowPanel.SetRow(this.TaskAssignedStaffBox, 1);
			this.TaskAssignedStaffBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 13, true);
			this.TaskAssignedStaffBox.TabIndex = 2;
			// 
			// OpportunityDetailsGroupBox
			// 
			this.OpportunityDetailsGroupBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("1e7b62f7-ce16-45f0-8763-f740fed4fc14", "Opportunity Details");
			this.OpportunityDetailsGroupBox.Controls.Add(this.OpportunityGuidFindBox);
			this.OpportunityDetailsGroupBox.Controls.Add(this.OpportunitySalesPersonCodeFindBox);
			this.OpportunityDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpportunityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OpportunityDetailsGroupBox.Name = "OpportunityDetailsGroupBox";
			this.OpportunityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 76, true);
			this.OpportunityDetailsGroupBox.TabIndex = 10;
			this.OpportunityDetailsGroupBox.TabStop = false;
			// 
			// OpportunityGuidFindBox
			// 
			this.OpportunityGuidFindBox.AllowDrop = true;
			this.OpportunityGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OpportunityGuidFindBox, "WKP_P8_Opportunity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ProcessManagement.Business.Project)(null)).WKP_P8_Opportunity)));
			this.OpportunityGuidFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("e4a78495-67e5-4d54-ac65-93ac2846cd35", "Parent ID");
			this.OpportunityGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.OpportunityGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 21, true);
			this.OpportunityGuidFindBox.Name = "OpportunityGuidFindBox";
			this.OpportunityGuidFindBox.PreBoundMaxLength = 9;
			this.OpportunityGuidFindBox.ShouldResize = true;
			this.OpportunityGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.OpportunityGuidFindBox.TabIndex = 20;
			// 
			// OpportunitySalesPersonCodeFindBox
			// 
			this.OpportunitySalesPersonCodeFindBox.AllowDrop = true;
			this.OpportunitySalesPersonCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OpportunitySalesPersonCodeFindBox, "Opportunity+P8_GS_NKPrimarySalesPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ProcessManagement.Business.Project)(null)).Opportunity.P8_GS_NKPrimarySalesPerson)));
			this.OpportunitySalesPersonCodeFindBox.CaptionResourceString = Enterprise.ProcessManagement.GUI.Res.GetData("2a37e074-ddc0-4dbf-9524-b668df67ad13", "Sales Person");
			this.OpportunitySalesPersonCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 47, true);
			this.OpportunitySalesPersonCodeFindBox.Name = "OpportunitySalesPersonCodeFindBox";
			this.OpportunitySalesPersonCodeFindBox.ShouldResize = true;
			this.OpportunitySalesPersonCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.OpportunitySalesPersonCodeFindBox.TabIndex = 21;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.StatusGroupBox);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.OpportunityDetailsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 300, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(220);
			this.SplitContainer.TabIndex = 1;
			// 
			// ProjectStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 300, true);
			this.Name = "ProjectStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.StatusRowPanel.ResumeLayout(false);
			this.StatusRowPanel.PerformLayout();
			this.CreatedByStaffBox.ResumeLayout(true);
			this.CreatedByStaffBox.PerformLayout();
			this.StatusBox.ResumeLayout(true);
			this.StatusBox.PerformLayout();
			this.ProjectManagerFindBox.ResumeLayout(true);
			this.ProjectManagerFindBox.PerformLayout();
			this.OpportunityDetailsGroupBox.ResumeLayout(false);
			this.OpportunityDetailsGroupBox.PerformLayout();
			this.OpportunityGuidFindBox.ResumeLayout(true);
			this.OpportunityGuidFindBox.PerformLayout();
			this.OpportunitySalesPersonCodeFindBox.ResumeLayout(true);
			this.OpportunitySalesPersonCodeFindBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		private ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private ZArchitecture.ZTextBox TaskAssignedStaffBox;
		protected CargoWise.Windows.UI.Layout.RowLayoutPanel StatusRowPanel;
		private ZDropEdit StatusBox;
		private ZArchitecture.ZTextBox TaskStatusBox;
		private ZCodeFindBox CreatedByStaffBox;
		private ZArchitecture.ZTextBox ProjectCreatedBox;
		private ZArchitecture.ZTextBox ProjectClosedOrDeferredBox;
		private ZCodeFindBox ProjectManagerFindBox;
		private ZGuidFindBox OpportunityGuidFindBox;
		private ZCodeFindBox OpportunitySalesPersonCodeFindBox;
		private ZGroupBox OpportunityDetailsGroupBox;
		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
