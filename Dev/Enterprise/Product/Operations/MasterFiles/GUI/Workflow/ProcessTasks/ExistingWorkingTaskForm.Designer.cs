namespace Enterprise.MasterFiles.GUI
{
	public partial class ExistingWorkingTaskForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OpenJobButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContinueExistingWorkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SuspendExistingTaskButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TasksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TasksGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 380, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// HeadingLabel
			// 
			this.HeadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 36, true);
			this.HeadingLabel.TabIndex = 3;
			// 
			// OpenJobButton
			// 
			this.OpenJobButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|38887428-909e-4a33-b40f-e60fcfd147e3", "Open");
			this.OpenJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 244, true);
			this.OpenJobButton.Name = "OpenJobButton";
			this.OpenJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 22, true);
			this.OpenJobButton.TabIndex = 5;
			this.OpenJobButton.UseVisualStyleBackColor = true;
			this.OpenJobButton.Click += new System.EventHandler(this.OperationsJobButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 269, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 65, true);
			this.InstructionsLabel.TabIndex = 6;
			// 
			// ContinueExistingWorkButton
			// 
			this.ContinueExistingWorkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueExistingWorkButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|4f0a164f-2c59-45f3-967e-c72a51b43c60", "Work on Selected Task");
			this.ContinueExistingWorkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ContinueExistingWorkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 350, true);
			this.ContinueExistingWorkButton.Name = "ContinueExistingWorkButton";
			this.ContinueExistingWorkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 21, true);
			this.ContinueExistingWorkButton.TabIndex = 1;
			this.ContinueExistingWorkButton.UseVisualStyleBackColor = true;
			this.ContinueExistingWorkButton.Click += new System.EventHandler(this.ContinueExistingWorkButton_Click);
			// 
			// SuspendExistingTaskButton
			// 
			this.SuspendExistingTaskButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SuspendExistingTaskButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|2dc476cd-f8fd-45f7-9eb0-f55382b82d12", "Continue");
			this.SuspendExistingTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 350, true);
			this.SuspendExistingTaskButton.Name = "SuspendExistingTaskButton";
			this.SuspendExistingTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 21, true);
			this.SuspendExistingTaskButton.TabIndex = 0;
			this.SuspendExistingTaskButton.UseVisualStyleBackColor = true;
			this.SuspendExistingTaskButton.Click += new System.EventHandler(this.SuspendExistingTaskButton_Click);
			// 
			// TasksGrid
			// 
			this.TasksGrid.AllowNavigation = false;
			this.TasksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TasksGrid, "SuspendedAndWorkingTasksForAssignedUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SuspendedAndWorkingTasksForAssignedUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SuspendedAndWorkingTasksForAssignedUser)).SyncRoot)).P9_TaskID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SuspendedAndWorkingTasksForAssignedUser)).SyncRoot)).P9_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SuspendedAndWorkingTasksForAssignedUser)).SyncRoot)).ParentJobDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SuspendedAndWorkingTasksForAssignedUser)).SyncRoot)).StatusDescription)));
			this.TasksGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P9_TaskID";
			zTextBoxColumnStyleInfo2.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|98d9d6fe-232a-4e19-b6e2-39e7e58cdc62", "Job Details");
			zTextBoxColumnStyleInfo3.ColumnName = "ParentJobDetails";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|3964b405-baab-4f2e-8cf5-86da2af1b50d", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TasksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TasksGrid.GridId = "29ba5656-ba13-4ebd-8759-9b40875b0fa4";
			this.TasksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TasksGrid.LayoutKey = "zGrid1";
			this.TasksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 46, true);
			this.TasksGrid.Name = "TasksGrid";
			this.TasksGrid.ReadOnly = true;
			this.TasksGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TasksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 194, true);
			this.TasksGrid.TabIndex = 4;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|1238f67b-a726-480e-8fda-d75c29330f29", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 350, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 21, true);
			this.CancelButtonX.TabIndex = 2;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// ExistingWorkingTaskForm
			// 
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 404, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ExistingWorkingTaskForm|fd6923ef-eac7-454f-be97-b6f326fc72bb", "Existing Working Task");
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.TasksGrid);
			this.Controls.Add(this.SuspendExistingTaskButton);
			this.Controls.Add(this.ContinueExistingWorkButton);
			this.Controls.Add(this.InstructionsLabel);
			this.Controls.Add(this.OpenJobButton);
			this.Controls.Add(this.HeadingLabel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "ExistingWorkingTaskForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HeadingLabel, 0);
			this.Controls.SetChildIndex(this.OpenJobButton, 0);
			this.Controls.SetChildIndex(this.InstructionsLabel, 0);
			this.Controls.SetChildIndex(this.ContinueExistingWorkButton, 0);
			this.Controls.SetChildIndex(this.SuspendExistingTaskButton, 0);
			this.Controls.SetChildIndex(this.TasksGrid, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TasksGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		internal Enterprise.ZArchitecture.ZGrid TasksGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;
		private Enterprise.ZArchitecture.ZLabel HeadingLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton OpenJobButton;
		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		internal Enterprise.ZArchitecture.GUI.ZButton ContinueExistingWorkButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SuspendExistingTaskButton;
	}
}
