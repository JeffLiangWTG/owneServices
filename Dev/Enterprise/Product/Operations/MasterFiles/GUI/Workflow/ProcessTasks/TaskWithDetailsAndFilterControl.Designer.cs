namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskWithDetailsAndFilterControl
	{
		#region Component Designer generated code

		void InitializeComponent()
		{
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FilterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GroupFindbox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StaffFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FilterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TasksPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TasksControl = new Enterprise.MasterFiles.GUI.TaskWithDetailsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterPanel.SuspendLayout();
			this.FilterGroupBox.SuspendLayout();
			this.GroupFindbox.SuspendLayout();
			this.StaffFindbox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.TasksPanel.SuspendLayout();
			this.TasksControl.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			//
			// FilterPanel
			//
			this.FilterPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterPanel.Controls.Add(this.FilterGroupBox);
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 43, true);
			this.FilterPanel.TabIndex = 0;
			//
			// FilterGroupBox
			//
			this.FilterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|a390ce15-87d1-4a37-9069-054a6a745446", "Filter");
			this.FilterGroupBox.Controls.Add(this.GroupFindbox);
			this.FilterGroupBox.Controls.Add(this.StaffFindbox);
			this.FilterGroupBox.Controls.Add(this.FilterButton);
			this.FilterGroupBox.Controls.Add(this.StatusDropEdit);
			this.FilterGroupBox.Controls.Add(this.ClearButton);
			this.FilterGroupBox.Controls.Add(this.TypeDropEdit);
			this.FilterGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterGroupBox.Name = "FilterGroupBox";
			this.FilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 43, true);
			this.FilterGroupBox.TabIndex = 0;
			this.FilterGroupBox.TabStop = false;
			//
			// GroupFindbox
			//
			this.GroupFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupFindbox, "P9_GG_AssignedGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GG_AssignedGroup)));
			this.GroupFindbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|613ad7e8-b1da-4517-91c3-0b74191fb077", "Group");
			this.GroupFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 16, true);
			this.GroupFindbox.Name = "GroupFindbox";
			this.GroupFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.GroupFindbox.TabIndex = 7;
			//
			// StaffFindbox
			//
			this.StaffFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffFindbox, "P9_GS_NKAssignedStaffMember");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_GS_NKAssignedStaffMember)));
			this.StaffFindbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|0a27b47d-6335-432e-91cc-68e676aeecee", "Staff");
			this.StaffFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 16, true);
			this.StaffFindbox.Name = "StaffFindbox";
			this.StaffFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.StaffFindbox.TabIndex = 5;
			//
			// FilterButton
			//
			this.FilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|ae54b758-3701-4342-bfd1-a16de458992b", "Filter");
			this.FilterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 15, true);
			this.FilterButton.Name = "FilterButton";
			this.FilterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.FilterButton.TabIndex = 8;
			this.FilterButton.Click += new System.EventHandler(this.FilterButton_Click);
			//
			// StatusDropEdit
			//
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "P9_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Status)));
			this.StatusDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|2094e6a3-ebcc-4698-a99a-ff8043269c82", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 16, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.ShowDescriptionBox = false;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.StatusDropEdit.TabIndex = 3;
			//
			// ClearButton
			//
			this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|b9807b12-312a-4d7f-9b70-c6bea4bfc2ff", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(869, 15, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.ClearButton.TabIndex = 9;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			//
			// TypeDropEdit
			//
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "P9_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Type)));
			this.TypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TaskWithDetailsAndFilterControl|cf1c6c57-8ce2-4664-802c-bb6f97dcd08e", "Type");
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 16, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.ShowDescriptionBox = false;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TypeDropEdit.TabIndex = 1;
			//
			// TasksPanel
			//
			this.TasksPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TasksPanel.Controls.Add(this.TasksControl);
			this.TasksPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.TasksPanel.Name = "TasksPanel";
			this.TasksPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 495, true);
			this.TasksPanel.TabIndex = 1;
			//
			// TasksControl
			//
			this.TasksControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TasksControl, ".");
			this.TasksControl.BindTo = "TasksView";
			this.TasksControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TasksControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TasksControl.Name = "TasksControl";
			this.TasksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 495, true);
			this.TasksControl.TabIndex = 0;
			//
			// TaskWithDetailsAndFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TasksPanel);
			this.Controls.Add(this.FilterPanel);
			this.Name = "TaskWithDetailsAndFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 547, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterPanel.ResumeLayout(false);
			this.FilterPanel.PerformLayout();
			this.FilterGroupBox.ResumeLayout(false);
			this.FilterGroupBox.PerformLayout();
			this.GroupFindbox.ResumeLayout(true);
			this.GroupFindbox.PerformLayout();
			this.StaffFindbox.ResumeLayout(true);
			this.StaffFindbox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.TasksPanel.ResumeLayout(false);
			this.TasksPanel.PerformLayout();
			this.TasksControl.ResumeLayout(true);
			this.TasksControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel FilterPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel TasksPanel;
		internal protected Enterprise.MasterFiles.GUI.TaskWithDetailsControl TasksControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox FilterGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZButton FilterButton;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox GroupFindbox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox StaffFindbox;
	}
}
