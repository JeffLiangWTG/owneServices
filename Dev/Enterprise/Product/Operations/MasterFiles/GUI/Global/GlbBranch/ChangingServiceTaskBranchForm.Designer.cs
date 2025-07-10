namespace Enterprise.MasterFiles.GUI
{
	partial class ChangingServiceTaskBranchForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoStm1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoStm2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfoStm1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoStm1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.instructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.gridServiceTasks = new Enterprise.ZArchitecture.ZGrid();
			this.gridStmServiceTasks = new Enterprise.ZArchitecture.ZGrid();
			this.replacementBranch = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.gbServiceTasks = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.gbScheduledReports = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridScheduledReports = new Enterprise.ZArchitecture.ZGrid();
			this.gbStaff = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridStaff = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridServiceTasks)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridStmServiceTasks)).BeginInit();
			this.gridServiceTasks.SuspendLayout();
			this.gridStmServiceTasks.SuspendLayout();
			this.replacementBranch.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.gbServiceTasks.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.gbScheduledReports.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridScheduledReports)).BeginInit();
			this.gridScheduledReports.SuspendLayout();
			this.gbStaff.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridStaff)).BeginInit();
			this.gridStaff.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject);
			// 
			// instructionsLabel
			// 
			this.instructionsLabel.AutoSize = true;
			this.instructionsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e7b21bab-8a9a-4ed0-bf47-039041486514", "The branch you are trying to deactivate is linked with active Service Tasks and/or Scheduled Reports and/or Staff.\r\n\r\nEither select a new branch as replacement, or remove the Is Active flag for the following.");
			this.instructionsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.instructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.instructionsLabel.Name = "instructionsLabel";
			this.instructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 39, true);
			this.instructionsLabel.TabIndex = 0;
			// 
			// gridServiceTasks
			// 
			this.gridServiceTasks.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridServiceTasks, "ServiceTasks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ServiceTasks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.ServiceTaskSchedule)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ServiceTasks)).SyncRoot)).S5_ScheduleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.ServiceManager.Business.ServiceTaskSchedule)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ServiceTasks)).SyncRoot)).S5_ScheduleDescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ServiceManager.Business.ServiceTaskSchedule)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ServiceTasks)).SyncRoot)).S5_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.ServiceTaskSchedule)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ServiceTasks)).SyncRoot)).S5_IsActive)));
			this.gridServiceTasks.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("735128c5-8df9-47de-b11a-e453b543cf1d", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "S5_ScheduleType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("285d5ff9-480d-4f81-be74-ec29d7aa6d66", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "S5_ScheduleDescriptionMultilingual";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7b5fe258-1da1-445d-9e2f-721108679a1c", "Branch");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "S5_GB";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7BA61BA9-3BA9-4F15-9334-FA3BD0913F83", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "S5_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridServiceTasks.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridServiceTasks.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridServiceTasks.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.gridServiceTasks.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.gridServiceTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridServiceTasks.GridId = "d4fe1f40-4065-4914-97ab-41f1452978e7";
			this.gridServiceTasks.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridServiceTasks.LayoutKey = "gridServiceTasks";
			this.gridServiceTasks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridServiceTasks.Name = "gridServiceTasks";
			this.gridServiceTasks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 89, true);
			this.gridServiceTasks.TabIndex = 0;
			// 
			// gridStmServiceTasks
			// 
			this.gridStmServiceTasks.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridStmServiceTasks, "StmServiceTasks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).StmServiceTasks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).StmServiceTasks)).SyncRoot)).SST_ServiceTaskCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.ServiceManager.Business.StmServiceTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).StmServiceTasks)).SyncRoot)).DescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.ServiceManager.Business.StmServiceTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).StmServiceTasks)).SyncRoot)).SST_GB_Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.StmServiceTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).StmServiceTasks)).SyncRoot)).SST_Active)));
			this.gridStmServiceTasks.CaptionVisible = false;
			zTextBoxColumnStyleInfoStm1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("735128c5-8df9-47de-b11a-e453b543cf1d", "Code");
			zTextBoxColumnStyleInfoStm1.ColumnName = "SST_ServiceTaskCode";
			zTextBoxColumnStyleInfoStm1.IsReadOnly = true;
			zTextBoxColumnStyleInfoStm1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfoStm2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("285d5ff9-480d-4f81-be74-ec29d7aa6d66", "Description");
			zTextBoxColumnStyleInfoStm2.ColumnName = "DescriptionMultilingual";
			zTextBoxColumnStyleInfoStm2.IsReadOnly = true;
			zTextBoxColumnStyleInfoStm2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfoStm1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7b5fe258-1da1-445d-9e2f-721108679a1c", "Branch");
			zGuidFindBoxColumnStyleInfoStm1.ColumnName = "SST_GB_Branch";
			zGuidFindBoxColumnStyleInfoStm1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfoStm1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7BA61BA9-3BA9-4F15-9334-FA3BD0913F83", "Is Active");
			zCheckBoxColumnStyleInfoStm1.ColumnName = "SST_Active";
			zCheckBoxColumnStyleInfoStm1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridStmServiceTasks.ColumnStyles.Add(zTextBoxColumnStyleInfoStm1);
			this.gridStmServiceTasks.ColumnStyles.Add(zTextBoxColumnStyleInfoStm2);
			this.gridStmServiceTasks.ColumnStyles.Add(zGuidFindBoxColumnStyleInfoStm1);
			this.gridStmServiceTasks.ColumnStyles.Add(zCheckBoxColumnStyleInfoStm1);
			this.gridStmServiceTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridStmServiceTasks.GridId = "CB089C66-B32F-46A7-8B36-35FE84B130F1";
			this.gridStmServiceTasks.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridStmServiceTasks.LayoutKey = "gridStmServiceTasks";
			this.gridStmServiceTasks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridStmServiceTasks.Name = "gridStmServiceTasks";
			this.gridStmServiceTasks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 89, true);
			this.gridStmServiceTasks.TabIndex = 0;
			// 
			// replacementBranch
			// 
			this.replacementBranch.AllowDrop = true;
			this.replacementBranch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.replacementBranch, "ReplacementBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ReplacementBranch)));
			this.replacementBranch.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e52cc444-d87a-4f01-8c9e-e39b7dba1e7e", "Replacement");
			this.replacementBranch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 69, true);
			this.replacementBranch.Name = "replacementBranch";
			this.replacementBranch.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.replacementBranch.ParentType = null;
			this.replacementBranch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 20, true);
			this.replacementBranch.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("da3779fc-da75-4a22-b97d-dc39bf9400b3", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 447, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fbd858fd-133b-4a3a-9664-8eca45e756a4", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 447, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 3;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 95, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.gbServiceTasks);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 346, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(108);
			this.splitContainer1.SplitterWidth = 8;
			this.splitContainer1.TabIndex = 2;
			// 
			// gbServiceTasks
			// 
			this.gbServiceTasks.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("98d08197-f1d4-4150-afd1-e14f76b57706", "Service Tasks");
			this.gbServiceTasks.Controls.Add(this.gridServiceTasks);
			this.gbServiceTasks.Controls.Add(this.gridStmServiceTasks);
			this.gbServiceTasks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbServiceTasks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbServiceTasks.Name = "gbServiceTasks";
			this.gbServiceTasks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 108, true);
			this.gbServiceTasks.TabIndex = 0;
			this.gbServiceTasks.TabStop = false;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.gbScheduledReports);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.gbStaff);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 216, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(106);
			this.splitContainer2.TabIndex = 6;
			// 
			// gbScheduledReports
			// 
			this.gbScheduledReports.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f50e19c0-76b0-4734-9028-cd1ae1a8aeb1", "Scheduled Reports");
			this.gbScheduledReports.Controls.Add(this.gridScheduledReports);
			this.gbScheduledReports.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbScheduledReports.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbScheduledReports.Name = "gbScheduledReports";
			this.gbScheduledReports.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 106, true);
			this.gbScheduledReports.TabIndex = 0;
			this.gbScheduledReports.TabStop = false;
			// 
			// gridScheduledReports
			// 
			this.gridScheduledReports.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridScheduledReports, "ScheduledReports");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ScheduledReports)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ScheduledReports)).SyncRoot)).S5_ScheduleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ScheduledReports)).SyncRoot)).S5_ScheduleDescriptionMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ScheduledReports)).SyncRoot)).S5_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).ScheduledReports)).SyncRoot)).S5_IsActive)));
			this.gridScheduledReports.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("82a32708-3dd5-46d3-a936-c8080b00af5c", "Code");
			zTextBoxColumnStyleInfo3.ColumnName = "S5_ScheduleType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c80c98a6-c454-4024-80cc-b1bbac341ec0", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "S5_ScheduleDescriptionMultilingual";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("81af5616-f3d5-4a02-9059-ef6079e91455", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "S5_GB";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("04bc584d-e95a-4687-89fb-0bdeb6902c44", "Is Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "S5_IsActive";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridScheduledReports.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.gridScheduledReports.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.gridScheduledReports.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.gridScheduledReports.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.gridScheduledReports.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridScheduledReports.GridId = "d4fe1f40-4065-4914-97ab-41f1452978e7";
			this.gridScheduledReports.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridScheduledReports.LayoutKey = "gridScheduledReports";
			this.gridScheduledReports.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridScheduledReports.Name = "gridScheduledReports";
			this.gridScheduledReports.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 87, true);
			this.gridScheduledReports.TabIndex = 0;
			// 
			// gbStaff
			// 
			this.gbStaff.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2e0b203e-ede7-4810-bfb2-65ab6ed84210", "Staff");
			this.gbStaff.Controls.Add(this.gridStaff);
			this.gbStaff.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbStaff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbStaff.Name = "gbStaff";
			this.gbStaff.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 106, true);
			this.gbStaff.TabIndex = 0;
			this.gbStaff.TabStop = false;
			// 
			// gridStaff
			// 
			this.gridStaff.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridStaff, "Staff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).Staff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).Staff)).SyncRoot)).GS_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).Staff)).SyncRoot)).GS_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).Staff)).SyncRoot)).GS_GB_HomeBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbStaff)(((System.Collections.IList)(((Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject)(null)).Staff)).SyncRoot)).GS_IsActive)));
			this.gridStaff.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E1F05E6A-F98C-4209-B03A-05DB4EE16366", "Staff Code");
			zTextBoxColumnStyleInfo5.ColumnName = "GS_Code";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D5522621-6565-4EA9-A34C-4CEEC2D650AE", "Full Name");
			zTextBoxColumnStyleInfo6.ColumnName = "GS_FullName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1AAC5475-9C27-4ECC-AAC0-184B92A541B3", "Home Branch");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "GS_GB_HomeBranch";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E2E977DA-38B8-424D-91C5-085FD743109A", "Is Active");
			zCheckBoxColumnStyleInfo3.ColumnName = "GS_IsActive";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridStaff.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.gridStaff.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.gridStaff.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.gridStaff.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.gridStaff.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridStaff.GridId = "d4fe1f40-4065-4914-97ab-41f1452978e7";
			this.gridStaff.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridStaff.LayoutKey = "gridStaff";
			this.gridStaff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridStaff.Name = "gridStaff";
			this.gridStaff.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 87, true);
			this.gridStaff.TabIndex = 1;
			// 
			// ChangingServiceTaskBranchForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("407566ac-9665-4395-aa6b-ea0da2eeb308", "Replace Branch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 500, true);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.replacementBranch);
			this.Controls.Add(this.instructionsLabel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.GUI.BranchSwitcherBusinessObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 480, true);
			this.Name = "ChangingServiceTaskBranchForm";
			this.Controls.SetChildIndex(this.instructionsLabel, 0);
			this.Controls.SetChildIndex(this.replacementBranch, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridServiceTasks)).EndInit();
			this.gridServiceTasks.ResumeLayout(false);
			this.gridServiceTasks.PerformLayout();
			this.gridStmServiceTasks.ResumeLayout(false);
			this.gridStmServiceTasks.PerformLayout();
			this.replacementBranch.ResumeLayout(true);
			this.replacementBranch.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.gbServiceTasks.ResumeLayout(false);
			this.gbServiceTasks.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.gbScheduledReports.ResumeLayout(false);
			this.gbScheduledReports.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridScheduledReports)).EndInit();
			this.gridScheduledReports.ResumeLayout(false);
			this.gridScheduledReports.PerformLayout();
			this.gbStaff.ResumeLayout(false);
			this.gbStaff.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridStaff)).EndInit();
			this.gridStaff.ResumeLayout(false);
			this.gridStaff.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel instructionsLabel;
		private ZArchitecture.ZGrid gridServiceTasks;
		private ZArchitecture.ZGrid gridStmServiceTasks;
		private ZArchitecture.GUI.ZGuidFindBox replacementBranch;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton okButton;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox gbServiceTasks;
		private ZArchitecture.GUI.ZGroupBox gbScheduledReports;
		private ZArchitecture.ZGrid gridScheduledReports;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private ZArchitecture.GUI.ZGroupBox gbStaff;
		private ZArchitecture.ZGrid gridStaff;
	}
}
