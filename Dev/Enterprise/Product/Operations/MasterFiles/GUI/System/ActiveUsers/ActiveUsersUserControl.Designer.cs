namespace Enterprise.MasterFiles.GUI
{
	public partial class ActiveUsersUserControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.buttonDetails = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ActiveUsersGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ActiveUsersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject);
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUsersUserControl|0a11699a-f726-45dc-a49f-396b0829fc1b", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 28, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// InstructionsLabel
			// 
			this.InstructionsLabel.AutoSize = true;
			this.InstructionsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUsersUserControl|7042761c-d51e-4588-bd85-a317e61e1f95", "", "The following users are logged in. Note: if a user session is not closed properly, it might take a few minutes until it is removed from this list.");
			this.InstructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.InstructionsLabel.Name = "InstructionsLabel";
			this.InstructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 13, true);
			this.InstructionsLabel.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.buttonDetails);
			this.TopPanel.Controls.Add(this.RefreshButton);
			this.TopPanel.Controls.Add(this.InstructionsLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 59, true);
			this.TopPanel.TabIndex = 0;
			// 
			// buttonDetails
			// 
			this.buttonDetails.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUsersUserControl|053a7556-f2d5-47a6-b8d7-b82e11fd2769", "Details");
			this.buttonDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 28, true);
			this.buttonDetails.Name = "buttonDetails";
			this.buttonDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.buttonDetails.TabIndex = 2;
			this.buttonDetails.Click += new System.EventHandler(this.buttonDetails_Click);
			// 
			// ActiveUsersGrid
			// 
			this.ActiveUsersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ActiveUsersGrid, "ActiveUsers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_Initials)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).Staff.HomeBranch.HomePort.RL_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_UserLocalLoginTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_YourLocalLoginTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_UTCLoginTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_ProcessID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ActiveUser)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ActiveUsersModuleBusinessObject)(null)).ActiveUsers)).SyncRoot)).AU_ComputerName)));
			this.ActiveUsersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControls|AU_Initials", "Initials");
			zTextBoxColumnStyleInfo5.ColumnName = "AU_Initials";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_FullName", "Full Name");
			zTextBoxColumnStyleInfo6.ColumnName = "AU_FullName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|RL_Code", "Home Branch");
			zTextBoxColumnStyleInfo7.ColumnName = "Staff+HomeBranch+HomePort+RL_Code";
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_UserLocalLoginTime", "Home Branch Login Time");
			zDateEditColumnStyleInfo4.ColumnName = "AU_UserLocalLoginTime";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_YourLocalLoginTime", "Local Login Time");
			zDateEditColumnStyleInfo5.ColumnName = "AU_YourLocalLoginTime";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_UTCLoginTime", "UTC Login Time");
			zDateEditColumnStyleInfo6.ColumnName = "AU_UTCLoginTime";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_ProcessID", "Process ID");
			zCalcEditColumnStyleInfo2.ColumnName = "AU_ProcessID";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ActiveUserUserControl|AU_ComputerName", "Computer Name");
			zTextBoxColumnStyleInfo8.ColumnName = "AU_ComputerName";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ActiveUsersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ActiveUsersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ActiveUsersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ActiveUsersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ActiveUsersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ActiveUsersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ActiveUsersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ActiveUsersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ActiveUsersGrid.CopySelectedRowsAllowed = true;
			this.ActiveUsersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ActiveUsersGrid.GridId = "c6a93eb1-6796-4123-9034-0eacf0520461";
			this.ActiveUsersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ActiveUsersGrid.IsWholeRowSelectedOnClick = true;
			this.ActiveUsersGrid.LayoutKey = "ZDisplayGrid1";
			this.ActiveUsersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 59, true);
			this.ActiveUsersGrid.Name = "ActiveUsersGrid";
			this.ActiveUsersGrid.ReadOnly = true;
			this.ActiveUsersGrid.ShouldSetErrorsOnTabPage = false;
			this.ActiveUsersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 346, true);
			this.ActiveUsersGrid.TabIndex = 1;
			this.ActiveUsersGrid.DoubleClick += new System.EventHandler(this.ActiveUsersGrid_DoubleClick);
			this.ActiveUsersGrid.ForceShowExportToExcelMenuItem = true;
			// 
			// ActiveUsersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActiveUsersGrid);
			this.Controls.Add(this.TopPanel);
			this.Name = "ActiveUsersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 405, true);
			this.Load += new System.EventHandler(this.ActiveUsersUserControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ActiveUsersGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton RefreshButton;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		protected Enterprise.ZArchitecture.GUI.ZDisplayGrid ActiveUsersGrid;
		private Enterprise.ZArchitecture.ZLabel InstructionsLabel;
		private Enterprise.ZArchitecture.GUI.ZButton buttonDetails;
	}
}
