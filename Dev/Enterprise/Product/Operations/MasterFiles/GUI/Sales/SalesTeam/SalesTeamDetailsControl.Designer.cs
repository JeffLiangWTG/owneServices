namespace Enterprise.MasterFiles.GUI
{
	partial class SalesTeamDetailsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.teamGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MembersModuleButtonGrid = new MembersModuleGrid();
			this.coverageAreasGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.coverageAreaSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.coveredUnlocosModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.includedPortsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.coveredCountriesModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.includedCountriesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.isGlobalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GG_GCBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GG_DescBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GG_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GG_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ParentTeamFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.teamGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MembersModuleButtonGrid.InnerGrid)).BeginInit();
			this.MembersModuleButtonGrid.SuspendLayout();
			this.coverageAreasGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.coverageAreaSplitContainer)).BeginInit();
			this.coverageAreaSplitContainer.Panel1.SuspendLayout();
			this.coverageAreaSplitContainer.Panel2.SuspendLayout();
			this.coverageAreaSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.coveredUnlocosModuleButtonGrid.InnerGrid)).BeginInit();
			this.coveredUnlocosModuleButtonGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.coveredCountriesModuleButtonGrid.InnerGrid)).BeginInit();
			this.coveredCountriesModuleButtonGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.GG_GCBoundGuidFindBox.SuspendLayout();
			this.ParentTeamFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SalesTeam);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.teamGroupBox);
			this.mainSplitContainer.Panel1MinSize = 100;
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.coverageAreasGroupBox);
			this.mainSplitContainer.Panel2MinSize = 100;
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 382, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(195);
			this.mainSplitContainer.TabIndex = 1;
			// 
			// teamGroupBox
			// 
			this.teamGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2d8a7ab7-bfcb-4a7a-8e01-ad9c14ff65fa", "Team");
			this.teamGroupBox.Controls.Add(this.MembersModuleButtonGrid);
			this.teamGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teamGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.teamGroupBox.Name = "teamGroupBox";
			this.teamGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 195, true);
			this.teamGroupBox.TabIndex = 0;
			this.teamGroupBox.TabStop = false;
			// 
			// MembersModuleButtonGrid
			// 
			this.MembersModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MembersModuleButtonGrid, "Staff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).Staff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).Lookups.CompleteSalesRepList)));
			this.MembersModuleButtonGrid.BindToFindBoxList = "Lookups.CompleteSalesRepList";
			zTextBoxColumnStyleInfo1.ColumnName = "GS_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "GS_FullName";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e3f1895-31fa-4cc9-abd1-d4902adba92e", "Title");
			zTextBoxColumnStyleInfo3.ColumnName = "GS_Title";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GS_GB_HomeBranch";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "GS_EmailAddress";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "GS_IsActive";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9abad803-8f71-4242-bfce-9f46c7e93890", "Company Code");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "HomeBranch+Company+GC_OH_OrgProxy";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2be913a6-37e0-40f0-9be0-508c43f43a68", "Company Name");
			zTextBoxColumnStyleInfo5.ColumnName = "HomeBranch+Company+OrgProxy+OH_FullName";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MembersModuleButtonGrid.DetachMessage = null;
			this.MembersModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MembersModuleButtonGrid.GridId = "e941caf0-8bd5-498d-a0b4-178e7e9aa2af";
			// 
			// 
			// 
			this.MembersModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.MembersModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MembersModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.MembersModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.MembersModuleButtonGrid.InnerGrid.GridId = null;
			this.MembersModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MembersModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.MembersModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MembersModuleButtonGrid.InnerGrid.Name = "Grid";
			this.MembersModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.MembersModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.MembersModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 138, true);
			this.MembersModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.MembersModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MembersModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SalesRep;
			this.MembersModuleButtonGrid.Name = "MembersModuleButtonGrid";
			this.MembersModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("d7d5bf01-966a-4cd4-9979-7c51899d4e37", "Member");
			this.MembersModuleButtonGrid.ReadOnly = true;
			this.MembersModuleButtonGrid.ShowEditButton = false;
			this.MembersModuleButtonGrid.ShowNewButton = false;
			this.MembersModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 176, true);
			this.MembersModuleButtonGrid.TabIndex = 0;
			// 
			// coverageAreasGroupBox
			// 
			this.coverageAreasGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("554d0b6a-fc8d-4d30-9059-5fcf250e92c5", "Coverage Areas");
			this.coverageAreasGroupBox.Controls.Add(this.coverageAreaSplitContainer);
			this.coverageAreasGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coverageAreasGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.coverageAreasGroupBox.Name = "coverageAreasGroupBox";
			this.coverageAreasGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 183, true);
			this.coverageAreasGroupBox.TabIndex = 0;
			this.coverageAreasGroupBox.TabStop = false;
			// 
			// coverageAreaSplitContainer
			// 
			this.coverageAreaSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coverageAreaSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.coverageAreaSplitContainer.Name = "coverageAreaSplitContainer";
			// 
			// coverageAreaSplitContainer.Panel1
			// 
			this.coverageAreaSplitContainer.Panel1.Controls.Add(this.coveredUnlocosModuleButtonGrid);
			this.coverageAreaSplitContainer.Panel1.Controls.Add(this.includedPortsLabel);
			// 
			// coverageAreaSplitContainer.Panel2
			// 
			this.coverageAreaSplitContainer.Panel2.Controls.Add(this.coveredCountriesModuleButtonGrid);
			this.coverageAreaSplitContainer.Panel2.Controls.Add(this.includedCountriesLabel);
			this.coverageAreaSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 164, true);
			this.coverageAreaSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(372);
			this.coverageAreaSplitContainer.TabIndex = 0;
			// 
			// coveredUnlocosModuleButtonGrid
			// 
			this.coveredUnlocosModuleButtonGrid.AllowAttachDetachWithoutEditSecurity = true;
			this.coveredUnlocosModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.coveredUnlocosModuleButtonGrid, "CoveredUnlocos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CoveredUnlocos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).Lookups.Unlocos)));
			this.coveredUnlocosModuleButtonGrid.BindToFindBoxList = "Lookups.Unlocos";
			zTextBoxColumnStyleInfo6.ColumnName = "RL_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.ColumnName = "RL_PortName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "RL_RN_NKCountryCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.coveredUnlocosModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.coveredUnlocosModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.coveredUnlocosModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.coveredUnlocosModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coveredUnlocosModuleButtonGrid.GridId = "46f6d1e6-ebe8-4e19-8b6d-6b871c83d9e5";
			// 
			// 
			// 
			this.coveredUnlocosModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.GridId = null;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.coveredUnlocosModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.coveredUnlocosModuleButtonGrid.InnerGrid.Name = "Grid";
			this.coveredUnlocosModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.coveredUnlocosModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 103, true);
			this.coveredUnlocosModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.coveredUnlocosModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.coveredUnlocosModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.coveredUnlocosModuleButtonGrid.Name = "coveredUnlocosModuleButtonGrid";
			this.coveredUnlocosModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("320ccc54-8e90-4ca7-8736-989071621df0", "UNLOCO");
			this.coveredUnlocosModuleButtonGrid.ReadOnly = true;
			this.coveredUnlocosModuleButtonGrid.ShowEditButton = false;
			this.coveredUnlocosModuleButtonGrid.ShowNewButton = false;
			this.coveredUnlocosModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 141, true);
			this.coveredUnlocosModuleButtonGrid.TabIndex = 0;
			// 
			// includedPortsLabel
			// 
			this.includedPortsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("930124b7-72b2-46f8-b4b0-1450aaf028a3", "Included Ports");
			this.includedPortsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.includedPortsLabel.IsFontBold = true;
			this.includedPortsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.includedPortsLabel.Name = "includedPortsLabel";
			this.includedPortsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 23, true);
			this.includedPortsLabel.TabIndex = 0;
			// 
			// coveredCountriesModuleButtonGrid
			// 
			this.coveredCountriesModuleButtonGrid.AllowAttachDetachWithoutEditSecurity = true;
			this.coveredCountriesModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.coveredCountriesModuleButtonGrid, "CoveredCountries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).CoveredCountries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).Lookups.Countries)));
			this.coveredCountriesModuleButtonGrid.BindToFindBoxList = "Lookups.Countries";
			zTextBoxColumnStyleInfo9.ColumnName = "RN_Code";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.ColumnName = "RN_DescMultilingual";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.coveredCountriesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.coveredCountriesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.coveredCountriesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coveredCountriesModuleButtonGrid.GridId = "46f6d1e6-ebe8-4e19-8b6d-6b871c83d9e5";
			// 
			// 
			// 
			this.coveredCountriesModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.coveredCountriesModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.coveredCountriesModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.coveredCountriesModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.coveredCountriesModuleButtonGrid.InnerGrid.GridId = null;
			this.coveredCountriesModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.coveredCountriesModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.coveredCountriesModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.coveredCountriesModuleButtonGrid.InnerGrid.Name = "Grid";
			this.coveredCountriesModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.coveredCountriesModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 103, true);
			this.coveredCountriesModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.coveredCountriesModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.coveredCountriesModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.coveredCountriesModuleButtonGrid.Name = "coveredCountriesModuleButtonGrid";
			this.coveredCountriesModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("8d5680df-cdca-4009-87fe-48631f505b73", "Country/Region");
			this.coveredCountriesModuleButtonGrid.ReadOnly = true;
			this.coveredCountriesModuleButtonGrid.ShowEditButton = false;
			this.coveredCountriesModuleButtonGrid.ShowNewButton = false;
			this.coveredCountriesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 141, true);
			this.coveredCountriesModuleButtonGrid.TabIndex = 0;
			// 
			// includedCountriesLabel
			// 
			this.includedCountriesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7922e79b-1666-4d8d-aa09-648a6cf976cb", "Included Countries/Regions");
			this.includedCountriesLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.includedCountriesLabel.IsFontBold = true;
			this.includedCountriesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.includedCountriesLabel.Name = "includedCountriesLabel";
			this.includedCountriesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 23, true);
			this.includedCountriesLabel.TabIndex = 0;
			// 
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.ParentTeamFindBox);
			this.TopPanel.Controls.Add(this.isGlobalCheckBox);
			this.TopPanel.Controls.Add(this.GG_GCBoundGuidFindBox);
			this.TopPanel.Controls.Add(this.GG_DescBoundTextBox);
			this.TopPanel.Controls.Add(this.GG_IsActiveCheckBox);
			this.TopPanel.Controls.Add(this.GG_CodeBoundTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 68, true);
			this.TopPanel.TabIndex = 0;
			// 
			// ParentTeamFindBox
			// 
			this.ParentTeamFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentTeamFindBox, "ParentTeamPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).ParentTeamPk)));
			this.ParentTeamFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(830, 37, true);
			this.ParentTeamFindBox.Name = "ParentTeamFindBox";
			this.ParentTeamFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ParentTeamFindBox.ParentType = null;
			this.ParentTeamFindBox.PreBoundMaxLength = 15;
			this.ParentTeamFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 20, true);
			this.ParentTeamFindBox.TabIndex = 5;
			// 
			// isGlobalCheckBox
			// 
			this.isGlobalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isGlobalCheckBox, "IsGlobal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).IsGlobal)));
			this.isGlobalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isGlobalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isGlobalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 40, true);
			this.isGlobalCheckBox.Name = "isGlobalCheckBox";
			this.isGlobalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isGlobalCheckBox.TabIndex = 3;
			this.isGlobalCheckBox.UseVisualStyleBackColor = true;
			// 
			// GG_GCBoundGuidFindBox
			// 
			this.GG_GCBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GG_GCBoundGuidFindBox, "GG_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).GG_GC)));
			this.GG_GCBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(486, 37, true);
			this.GG_GCBoundGuidFindBox.Name = "GG_GCBoundGuidFindBox";
			this.GG_GCBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.GG_GCBoundGuidFindBox.TabIndex = 4;
			// 
			// GG_DescBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.GG_DescBoundTextBox, "GG_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).GG_Desc)));
			this.GG_DescBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GG_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 37, true);
			this.GG_DescBoundTextBox.Name = "GG_DescBoundTextBox";
			this.GG_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.GG_DescBoundTextBox.TabIndex = 2;
			// 
			// GG_IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GG_IsActiveCheckBox, "GG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).GG_IsActive)));
			this.GG_IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.GG_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GG_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 9, true);
			this.GG_IsActiveCheckBox.Name = "GG_IsActiveCheckBox";
			this.GG_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.GG_IsActiveCheckBox.TabIndex = 1;
			this.GG_IsActiveCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.GG_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// GG_CodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.GG_CodeBoundTextBox, "GG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.SalesTeam)(null)).GG_Code)));
			this.GG_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 11, true);
			this.GG_CodeBoundTextBox.Name = "GG_CodeBoundTextBox";
			this.GG_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.GG_CodeBoundTextBox.TabIndex = 0;

			// 
			// SalesTeamDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.TopPanel);
			this.Name = "SalesTeamDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1150, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.teamGroupBox.ResumeLayout(false);
			this.teamGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MembersModuleButtonGrid.InnerGrid)).EndInit();
			this.MembersModuleButtonGrid.ResumeLayout(true);
			this.MembersModuleButtonGrid.PerformLayout();
			this.coverageAreasGroupBox.ResumeLayout(false);
			this.coverageAreasGroupBox.PerformLayout();
			this.coverageAreaSplitContainer.Panel1.ResumeLayout(false);
			this.coverageAreaSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.coverageAreaSplitContainer)).EndInit();
			this.coverageAreaSplitContainer.ResumeLayout(false);
			this.coverageAreaSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.coveredUnlocosModuleButtonGrid.InnerGrid)).EndInit();
			this.coveredUnlocosModuleButtonGrid.ResumeLayout(true);
			this.coveredUnlocosModuleButtonGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.coveredCountriesModuleButtonGrid.InnerGrid)).EndInit();
			this.coveredCountriesModuleButtonGrid.ResumeLayout(true);
			this.coveredCountriesModuleButtonGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.GG_GCBoundGuidFindBox.ResumeLayout(true);
			this.GG_GCBoundGuidFindBox.PerformLayout();
			this.ParentTeamFindBox.ResumeLayout(true);
			this.ParentTeamFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox teamGroupBox;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.ZTextBox GG_DescBoundTextBox;
		private ZArchitecture.GUI.ZCheckBox GG_IsActiveCheckBox;
		private ZArchitecture.ZTextBox GG_CodeBoundTextBox;
		private MembersModuleGrid MembersModuleButtonGrid;
		private CargoWise.Windows.UI.KSplitContainer coverageAreaSplitContainer;
		private ZArchitecture.GUI.ZModuleButtonGrid coveredUnlocosModuleButtonGrid;
		private ZArchitecture.GUI.ZModuleButtonGrid coveredCountriesModuleButtonGrid;
		private ZArchitecture.GUI.ZGroupBox coverageAreasGroupBox;
		private ZArchitecture.ZLabel includedPortsLabel;
		private ZArchitecture.ZLabel includedCountriesLabel;
		private ZArchitecture.GUI.ZGuidFindBox GG_GCBoundGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox isGlobalCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ParentTeamFindBox;
	}
}
