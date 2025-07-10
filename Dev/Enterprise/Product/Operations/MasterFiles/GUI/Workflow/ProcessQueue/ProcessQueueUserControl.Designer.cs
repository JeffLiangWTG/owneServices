namespace Enterprise.MasterFiles.GUI
{
	public partial class ProcessQueueUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QueueHistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QueueHistoryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CurrentQueueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.QueueHistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QueueHistoryGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IProcessQueueParent);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.QueueHistoryGroupBox);
			this.MainGroupBox.Controls.Add(this.CurrentQueueGroupBox);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 598, true);
			this.MainGroupBox.TabIndex = 1;
			this.MainGroupBox.TabStop = false;
			// 
			// QueueHistoryGroupBox
			// 
			this.QueueHistoryGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.QueueHistoryGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|e21697ef-eaf0-4b04-9aa3-7f9f93e8831d", "History");
			this.QueueHistoryGroupBox.Controls.Add(this.QueueHistoryGrid);
			this.QueueHistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 112, true);
			this.QueueHistoryGroupBox.Name = "QueueHistoryGroupBox";
			this.QueueHistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 480, true);
			this.QueueHistoryGroupBox.TabIndex = 10;
			this.QueueHistoryGroupBox.TabStop = false;
			// 
			// QueueHistoryGrid
			// 
			this.QueueHistoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QueueHistoryGrid, "ActiveProcessQueueForBinding.QueueLogs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).Queue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).SubStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).Reason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).AssignedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).SL_EventTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessQueueLog)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveProcessQueue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IProcessQueueParent)(null)).ActiveProcessQueueForBinding)).SyncRoot)).QueueLogs)).SyncRoot)).SL_UserNameAndInitials)));
			this.QueueHistoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|60f97b1e-59c8-4d70-80dc-b2c5d6b505fe", "Queue");
			zTextBoxColumnStyleInfo1.ColumnName = "Queue";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|0e6dedf8-6703-4114-bc4d-43fd36d8c152", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "Status";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|679b5146-f650-452e-b752-4899bc4a81c0", "Sub Status");
			zTextBoxColumnStyleInfo3.ColumnName = "SubStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|afe22f4d-ea2e-49ba-acdb-262a93893ee5", "Reason");
			zTextBoxColumnStyleInfo4.ColumnName = "Reason";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|01dd1312-ed1c-4e29-9f73-537b4df309dc", "Assigned To");
			zTextBoxColumnStyleInfo5.ColumnName = "AssignedTo";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|7b7c4ac7-dd2d-4244-8c28-c5a23e076885", "Time");
			zDateEditColumnStyleInfo1.ColumnName = "SL_EventTime";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|c7eb1c7f-f5a0-4106-9c46-9992a8d55656", "User");
			zTextBoxColumnStyleInfo6.ColumnName = "SL_UserNameAndInitials";
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.QueueHistoryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.QueueHistoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.QueueHistoryGrid.GridId = "8c639ec5-ff93-47f5-91cd-739a0b27a374";
			this.QueueHistoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueueHistoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QueueHistoryGrid.LayoutKey = "zGrid1";
			this.QueueHistoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.QueueHistoryGrid.Name = "QueueHistoryGrid";
			this.QueueHistoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 461, true);
			this.QueueHistoryGrid.TabIndex = 0;
			// 
			// CurrentQueueGroupBox
			// 
			this.CurrentQueueGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CurrentQueueGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ProcessQueueUserControl|050ad762-e937-47cb-8205-c2a80db216a2", "Current Queue");
			this.CurrentQueueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.CurrentQueueGroupBox.Name = "CurrentQueueGroupBox";
			this.CurrentQueueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 104, true);
			this.CurrentQueueGroupBox.TabIndex = 9;
			this.CurrentQueueGroupBox.TabStop = false;
			// 
			// ProcessQueueUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "ProcessQueueUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 608, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.QueueHistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.QueueHistoryGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CurrentQueueGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox QueueHistoryGroupBox;
		private Enterprise.ZArchitecture.ZGrid QueueHistoryGrid;
		private readonly Enterprise.MasterFiles.GUI.CurrentQueueUserControl currentQueueUserControl;
	}
}
