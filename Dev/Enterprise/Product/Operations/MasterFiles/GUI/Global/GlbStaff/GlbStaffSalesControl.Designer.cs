using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffSalesControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.teamAllocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.teamAllocationModuleButtonGrid = new ZModuleButtonGrid();
			this.commissionEntitlementRulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.commissionEntitlementRulesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ShowDisabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShowExpiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommissionEntitlementRulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.rightCommissionEntitlementRulesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.commissionRulePreviewPane = new Enterprise.MasterFiles.GUI.OverallStaffCommissionRulePreviewPane();
			this.commissionRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.commissionRuleRatesControl = new Enterprise.MasterFiles.GUI.AccCommissionRuleRatesControl();
			this.preferredPaymentCompanyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UseTransactionCompanyAsPreferredPaymentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreferredPaymentCompanyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GS_GC_PreferredPaymentCompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.teamAllocationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.teamAllocationModuleButtonGrid.InnerGrid)).BeginInit();
			this.teamAllocationModuleButtonGrid.SuspendLayout();
			this.commissionEntitlementRulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.commissionEntitlementRulesSplitContainer)).BeginInit();
			this.commissionEntitlementRulesSplitContainer.Panel1.SuspendLayout();
			this.commissionEntitlementRulesSplitContainer.Panel2.SuspendLayout();
			this.commissionEntitlementRulesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommissionEntitlementRulesGrid)).BeginInit();
			this.CommissionEntitlementRulesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rightCommissionEntitlementRulesSplitContainer)).BeginInit();
			this.rightCommissionEntitlementRulesSplitContainer.Panel1.SuspendLayout();
			this.rightCommissionEntitlementRulesSplitContainer.Panel2.SuspendLayout();
			this.rightCommissionEntitlementRulesSplitContainer.SuspendLayout();
			this.commissionRulePreviewPane.SuspendLayout();
			this.commissionRatesGroupBox.SuspendLayout();
			this.commissionRuleRatesControl.SuspendLayout();
			this.preferredPaymentCompanyGroupBox.SuspendLayout();
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaff);
			// 
			// teamAllocationGroupBox
			// 
			this.teamAllocationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7bbddfd5-72db-46da-ab75-d5c82a8a650c", "Team Allocation");
			this.teamAllocationGroupBox.Controls.Add(this.teamAllocationModuleButtonGrid);
			this.teamAllocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.teamAllocationGroupBox.Name = "teamAllocationGroupBox";
			this.teamAllocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 252, true);
			this.teamAllocationGroupBox.TabIndex = 0;
			this.teamAllocationGroupBox.TabStop = false;
			// 
			// teamAllocationModuleButtonGrid
			// 
			this.teamAllocationModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.teamAllocationModuleButtonGrid, "SalesTeams");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).SalesTeams)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Lookups.CompleteSalesTeamList)));
			this.teamAllocationModuleButtonGrid.BindToFindBoxList = "Lookups.CompleteSalesTeamList";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e1c47218-d085-43f9-84d8-d5fc792c2fdc", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "GG_Code";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f2b345aa-0ffc-4999-8d8a-e4a353d61b99", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "GG_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.teamAllocationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.teamAllocationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.teamAllocationModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teamAllocationModuleButtonGrid.GridId = "908e0717-2229-43d2-b54c-7c1285d2f005";
			// 
			// 
			// 
			this.teamAllocationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.teamAllocationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.teamAllocationModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.teamAllocationModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.teamAllocationModuleButtonGrid.InnerGrid.GridId = null;
			this.teamAllocationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.teamAllocationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.teamAllocationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.teamAllocationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.teamAllocationModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.teamAllocationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 195, true);
			this.teamAllocationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.teamAllocationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.teamAllocationModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SalesTeam;
			this.teamAllocationModuleButtonGrid.Name = "teamAllocationModuleButtonGrid";
			this.teamAllocationModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("ce459940-fed8-45ce-95f6-075a7a4d000c", "Sales Team");
			this.teamAllocationModuleButtonGrid.ReadOnly = true;
			this.teamAllocationModuleButtonGrid.ShowEditButton = false;
			this.teamAllocationModuleButtonGrid.ShowNewButton = false;
			this.teamAllocationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 233, true);
			this.teamAllocationModuleButtonGrid.TabIndex = 0;
			// 
			// commissionEntitlementRulesGroupBox
			// 
			this.commissionEntitlementRulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.commissionEntitlementRulesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("666bd311-15f1-43ad-a991-61a933bafc1c", "Commission Entitlement Rules");
			this.commissionEntitlementRulesGroupBox.Controls.Add(this.commissionEntitlementRulesSplitContainer);
			this.commissionEntitlementRulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 0, true);
			this.commissionEntitlementRulesGroupBox.Name = "commissionEntitlementRulesGroupBox";
			this.commissionEntitlementRulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 550, true);
			this.commissionEntitlementRulesGroupBox.TabIndex = 1;
			this.commissionEntitlementRulesGroupBox.TabStop = false;
			// 
			// commissionEntitlementRulesSplitContainer
			// 
			this.commissionEntitlementRulesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionEntitlementRulesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.commissionEntitlementRulesSplitContainer.IsSplitterFixed = true;
			this.commissionEntitlementRulesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.commissionEntitlementRulesSplitContainer.Name = "commissionEntitlementRulesSplitContainer";
			// 
			// commissionEntitlementRulesSplitContainer.Panel1
			// 
			this.commissionEntitlementRulesSplitContainer.Panel1.Controls.Add(this.ShowDisabledCheckBox);
			this.commissionEntitlementRulesSplitContainer.Panel1.Controls.Add(this.ShowExpiredCheckBox);
			this.commissionEntitlementRulesSplitContainer.Panel1.Controls.Add(this.CommissionEntitlementRulesGrid);
			// 
			// commissionEntitlementRulesSplitContainer.Panel2
			// 
			this.commissionEntitlementRulesSplitContainer.Panel2.Controls.Add(this.rightCommissionEntitlementRulesSplitContainer);
			this.commissionEntitlementRulesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 531, true);
			this.commissionEntitlementRulesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(270);
			this.commissionEntitlementRulesSplitContainer.TabIndex = 0;
			// 
			// ShowDisabledCheckBox
			// 
			this.ShowDisabledCheckBox.AutoSize = true;
			this.ShowDisabledCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("43f82864-0fe8-47ae-b866-453351bcae87", "Show Disabled");
			this.ShowDisabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowDisabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 3, true);
			this.ShowDisabledCheckBox.Name = "ShowDisabledCheckBox";
			this.ShowDisabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.ShowDisabledCheckBox.TabIndex = 1;
			this.ShowDisabledCheckBox.UseVisualStyleBackColor = true;
			this.ShowDisabledCheckBox.CheckedChanged += new System.EventHandler(this.ShowDisabledCheckBox_CheckedChanged);
			// 
			// ShowExpiredCheckBox
			// 
			this.ShowExpiredCheckBox.AutoSize = true;
			this.ShowExpiredCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("83b4defc-f6ba-4a78-b213-f7956a07403d", "Show Expired");
			this.ShowExpiredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowExpiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShowExpiredCheckBox.Name = "ShowExpiredCheckBox";
			this.ShowExpiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.ShowExpiredCheckBox.TabIndex = 0;
			this.ShowExpiredCheckBox.UseVisualStyleBackColor = true;
			this.ShowExpiredCheckBox.CheckedChanged += new System.EventHandler(this.ShowExpiredCheckBox_CheckedChanged);
			// 
			// CommissionEntitlementRulesGrid
			// 
			this.CommissionEntitlementRulesGrid.AllowNavigation = false;
			this.CommissionEntitlementRulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommissionEntitlementRulesGrid, "OverallCommissionRulesView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).GroupPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).CompanyPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Service)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).SubModule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).CommissionBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).CommissionTriggerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).EndDate)));
			this.CommissionEntitlementRulesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "GroupPk";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CompanyPk";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.ColumnName = "Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.ColumnName = "Product";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo3.ColumnName = "Service";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo4.ColumnName = "SubModule";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo7.ColumnName = "Mode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("42934AAC-852E-4B03-A0C0-6C2E408762A6", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Origin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3B9B623D-D71F-48A1-9D0D-7D622C29DBB6", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Destination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("19969ebe-0d1a-4f05-901c-303dce940be3", "Basis", "Commission Basis", "");
			zDropEditColumnStyleInfo5.ColumnName = "CommissionBasis";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("71c37c22-26fa-44dc-b37f-c2f847e65cb3", "Trigger", "Effective Trigger", "");
			zDropEditColumnStyleInfo6.ColumnName = "CommissionTriggerType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDateEditColumnStyleInfo2.ColumnName = "EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.CommissionEntitlementRulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.CommissionEntitlementRulesGrid.CopySelectedRowsAllowed = true;
			this.CommissionEntitlementRulesGrid.GridId = "b6a784f4-24ef-4bdb-9a99-4ea5e487d184";
			this.CommissionEntitlementRulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommissionEntitlementRulesGrid.LayoutKey = "commissionEntitlementRulesGrid";
			this.CommissionEntitlementRulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.CommissionEntitlementRulesGrid.Name = "CommissionEntitlementRulesGrid";
			this.CommissionEntitlementRulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 505, true);
			this.CommissionEntitlementRulesGrid.TabIndex = 2;
			// 
			// rightCommissionEntitlementRulesSplitContainer
			// 
			this.rightCommissionEntitlementRulesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightCommissionEntitlementRulesSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.rightCommissionEntitlementRulesSplitContainer.IsSplitterFixed = true;
			this.rightCommissionEntitlementRulesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rightCommissionEntitlementRulesSplitContainer.Name = "rightCommissionEntitlementRulesSplitContainer";
			this.rightCommissionEntitlementRulesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// rightCommissionEntitlementRulesSplitContainer.Panel1
			// 
			this.rightCommissionEntitlementRulesSplitContainer.Panel1.Controls.Add(this.commissionRulePreviewPane);
			// 
			// rightCommissionEntitlementRulesSplitContainer.Panel2
			// 
			this.rightCommissionEntitlementRulesSplitContainer.Panel2.Controls.Add(this.commissionRatesGroupBox);
			this.rightCommissionEntitlementRulesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 531, true);
			this.rightCommissionEntitlementRulesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(293);
			this.rightCommissionEntitlementRulesSplitContainer.TabIndex = 1;
			// 
			// commissionRulePreviewPane
			// 
			this.commissionRulePreviewPane.AllowDrop = true;
			this.commissionRulePreviewPane.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.commissionRulePreviewPane, "OverallCommissionRulesView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)))));
			this.commissionRulePreviewPane.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRulePreviewPane.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRulePreviewPane.Name = "commissionRulePreviewPane";
			this.commissionRulePreviewPane.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 293, true);
			this.commissionRulePreviewPane.TabIndex = 0;
			// 
			// commissionRatesGroupBox
			// 
			this.commissionRatesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b81aea57-3244-4894-8f87-a33faaef8c65", "Commission Rates");
			this.commissionRatesGroupBox.Controls.Add(this.commissionRuleRatesControl);
			this.commissionRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionRatesGroupBox.Name = "commissionRatesGroupBox";
			this.commissionRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 234, true);
			this.commissionRatesGroupBox.TabIndex = 0;
			this.commissionRatesGroupBox.TabStop = false;
			// 
			// commissionRuleRatesControl
			// 
			this.commissionRuleRatesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.commissionRuleRatesControl, "OverallCommissionRulesView.RatesAsCollectionForBinding.Inner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccCommissionRuleRateCollection)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule.RatesWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).OverallCommissionRulesView)).SyncRoot)).RatesAsCollectionForBinding)).SyncRoot)).Inner)));
			this.commissionRuleRatesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionRuleRatesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.commissionRuleRatesControl.Name = "commissionRuleRatesControl";
			this.commissionRuleRatesControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 5, 3, true);
			this.commissionRuleRatesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 215, true);
			this.commissionRuleRatesControl.TabIndex = 0;
			// 
			// preferredPaymentCompanyGroupBox
			// 
			this.preferredPaymentCompanyGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("164adc74-7dfe-4617-a25d-30117dddaeda", "Preferred Payment");
			this.preferredPaymentCompanyGroupBox.Controls.Add(this.UseTransactionCompanyAsPreferredPaymentCheckBox);
			this.preferredPaymentCompanyGroupBox.Controls.Add(this.PreferredPaymentCompanyNameTextBox);
			this.preferredPaymentCompanyGroupBox.Controls.Add(this.GS_GC_PreferredPaymentCompanyGuidFindBox);
			this.preferredPaymentCompanyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 258, true);
			this.preferredPaymentCompanyGroupBox.Name = "preferredPaymentCompanyGroupBox";
			this.preferredPaymentCompanyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 133, true);
			this.preferredPaymentCompanyGroupBox.TabIndex = 2;
			this.preferredPaymentCompanyGroupBox.TabStop = false;
			// 
			// UseTransactionCompanyAsPreferredPaymentCheckBox
			// 
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseTransactionCompanyAsPreferredPaymentCheckBox, "UseTransactionCompanyAsPreferredPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).UseTransactionCompanyAsPreferredPayment)));
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Name = "UseTransactionCompanyAsPreferredPaymentCheckBox";
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.TabIndex = 0;
			this.UseTransactionCompanyAsPreferredPaymentCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreferredPaymentCompanyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreferredPaymentCompanyNameTextBox, "PreferredPaymentCompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).PreferredPaymentCompanyName)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PreferredPaymentCompanyNameTextBox, false);
			this.PreferredPaymentCompanyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 89, true);
			this.PreferredPaymentCompanyNameTextBox.Name = "PreferredPaymentCompanyNameTextBox";
			this.PreferredPaymentCompanyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.PreferredPaymentCompanyNameTextBox.TabIndex = 2;
			// 
			// GS_GC_PreferredPaymentCompanyGuidFindBox
			// 
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GS_GC_PreferredPaymentCompanyGuidFindBox, "GS_GC_PreferredPaymentCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_GC_PreferredPaymentCompany)));
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("94c383e9-c616-4747-9a17-ee35c49c759b", "Specific");
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 66, true);
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.Name = "GS_GC_PreferredPaymentCompanyGuidFindBox";
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.ShowDescriptionBox = false;
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.TabIndex = 1;
			// 
			// GlbStaffSalesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.preferredPaymentCompanyGroupBox);
			this.Controls.Add(this.commissionEntitlementRulesGroupBox);
			this.Controls.Add(this.teamAllocationGroupBox);
			this.Name = "GlbStaffSalesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 550, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.teamAllocationGroupBox.ResumeLayout(false);
			this.teamAllocationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.teamAllocationModuleButtonGrid.InnerGrid)).EndInit();
			this.teamAllocationModuleButtonGrid.ResumeLayout(true);
			this.teamAllocationModuleButtonGrid.PerformLayout();
			this.commissionEntitlementRulesGroupBox.ResumeLayout(false);
			this.commissionEntitlementRulesGroupBox.PerformLayout();
			this.commissionEntitlementRulesSplitContainer.Panel1.ResumeLayout(false);
			this.commissionEntitlementRulesSplitContainer.Panel1.PerformLayout();
			this.commissionEntitlementRulesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.commissionEntitlementRulesSplitContainer)).EndInit();
			this.commissionEntitlementRulesSplitContainer.ResumeLayout(false);
			this.commissionEntitlementRulesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommissionEntitlementRulesGrid)).EndInit();
			this.CommissionEntitlementRulesGrid.ResumeLayout(false);
			this.CommissionEntitlementRulesGrid.PerformLayout();
			this.rightCommissionEntitlementRulesSplitContainer.Panel1.ResumeLayout(false);
			this.rightCommissionEntitlementRulesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rightCommissionEntitlementRulesSplitContainer)).EndInit();
			this.rightCommissionEntitlementRulesSplitContainer.ResumeLayout(false);
			this.rightCommissionEntitlementRulesSplitContainer.PerformLayout();
			this.commissionRulePreviewPane.ResumeLayout(true);
			this.commissionRulePreviewPane.PerformLayout();
			this.commissionRatesGroupBox.ResumeLayout(false);
			this.commissionRatesGroupBox.PerformLayout();
			this.commissionRuleRatesControl.ResumeLayout(true);
			this.commissionRuleRatesControl.PerformLayout();
			this.preferredPaymentCompanyGroupBox.ResumeLayout(false);
			this.preferredPaymentCompanyGroupBox.PerformLayout();
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.ResumeLayout(true);
			this.GS_GC_PreferredPaymentCompanyGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox teamAllocationGroupBox;
		private ZModuleButtonGrid teamAllocationModuleButtonGrid;
		private ZArchitecture.GUI.ZGroupBox commissionEntitlementRulesGroupBox;
		internal ZArchitecture.ZGrid CommissionEntitlementRulesGrid;
		private CargoWise.Windows.UI.KSplitContainer commissionEntitlementRulesSplitContainer;
		private OverallStaffCommissionRulePreviewPane commissionRulePreviewPane;
		internal ZArchitecture.GUI.ZCheckBox ShowExpiredCheckBox;
		internal ZArchitecture.GUI.ZCheckBox ShowDisabledCheckBox;
		private CargoWise.Windows.UI.KSplitContainer rightCommissionEntitlementRulesSplitContainer;
		private ZArchitecture.GUI.ZGroupBox commissionRatesGroupBox;
		private AccCommissionRuleRatesControl commissionRuleRatesControl;
		private ZArchitecture.GUI.ZGroupBox preferredPaymentCompanyGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox GS_GC_PreferredPaymentCompanyGuidFindBox;
		private ZArchitecture.ZTextBox PreferredPaymentCompanyNameTextBox;
		private ZArchitecture.GUI.ZCheckBox UseTransactionCompanyAsPreferredPaymentCheckBox;
	}
}
