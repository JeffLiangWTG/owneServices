namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class SecurityDeclarationUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.overrideValuesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.securityStatusPartyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.issuingCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.expiryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.approvalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.receivedFromGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.receivedFromGrid = new Enterprise.ZArchitecture.ZGrid();
			this.screeningMethodGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.screeningMethodsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.groundsForExemptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groundsForExemptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.securityStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.personScreeningTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.otherScreeningMethodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.dateTimeOfScreeningDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.additionalSecurityInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.personScreeningFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dateTimeOfScheduledArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.chooseAdditionalSecurityInformationStatementDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.securityStatusPartyGroupBox.SuspendLayout();
			this.issuingCountryCodeFindBox.SuspendLayout();
			this.expiryDateDateEdit.SuspendLayout();
			this.receivedFromGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.receivedFromGrid)).BeginInit();
			this.receivedFromGrid.SuspendLayout();
			this.screeningMethodGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.screeningMethodsGrid)).BeginInit();
			this.screeningMethodsGrid.SuspendLayout();
			this.groundsForExemptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.groundsForExemptionGrid)).BeginInit();
			this.groundsForExemptionGrid.SuspendLayout();
			this.dateTimeOfScreeningDateEdit.SuspendLayout();
			this.personScreeningFindBox.SuspendLayout();
			this.dateTimeOfScheduledArrivalDateEdit.SuspendLayout();
			this.chooseAdditionalSecurityInformationStatementDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ForwardingConsol);
			// 
			// overrideValuesCheckBox
			// 
			this.overrideValuesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.overrideValuesCheckBox, "IsCSDValuesOverriddenProperty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).IsCSDValuesOverriddenProperty)));
			this.overrideValuesCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2a917560-4684-4d00-9951-d07c5fbcbca3", "Check if you want to override default values");
			this.overrideValuesCheckBox.Click += new System.EventHandler(this.OverrideValuesCheckBox_Click);
			this.overrideValuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.overrideValuesCheckBox.Name = "overrideValuesCheckBox";
			this.overrideValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 17, true);
			this.overrideValuesCheckBox.TabIndex = 0;
			this.overrideValuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// securityStatusPartyGroupBox
			// 
			this.securityStatusPartyGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("45d682e6-514e-4a72-a211-daccf8afaffa", "Party Issuing the Security Status");
			this.securityStatusPartyGroupBox.Controls.Add(this.issuingCountryCodeFindBox);
			this.securityStatusPartyGroupBox.Controls.Add(this.expiryDateDateEdit);
			this.securityStatusPartyGroupBox.Controls.Add(this.approvalNumberTextBox);
			this.securityStatusPartyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
			this.securityStatusPartyGroupBox.Name = "securityStatusPartyGroupBox";
			this.securityStatusPartyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 71, true);
			this.securityStatusPartyGroupBox.TabIndex = 1;
			this.securityStatusPartyGroupBox.TabStop = false;
			// 
			// issuingCountryCodeFindBox
			// 
			this.issuingCountryCodeFindBox.AllowDrop = true;
			this.issuingCountryCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.issuingCountryCodeFindBox, "AWBHeaderManager.EH_RN_NKAgentApprovalCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_RN_NKAgentApprovalCountryCode)));
			this.issuingCountryCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9b04bf80-af53-4ee0-81e5-e7f9933347ff", "Issuing Country/Region Code");
			this.issuingCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 31, true);
			this.issuingCountryCodeFindBox.Name = "issuingCountryCodeFindBox";
			this.issuingCountryCodeFindBox.ShowDescriptionBox = false;
			this.issuingCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.issuingCountryCodeFindBox.TabIndex = 4;
			// 
			// expiryDateDateEdit
			// 
			this.expiryDateDateEdit.AllowDrop = true;
			this.expiryDateDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.expiryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.expiryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.expiryDateDateEdit, "AWBHeaderManager.EH_AgentApprovalExpiryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_AgentApprovalExpiryDate)));
			this.expiryDateDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("9fb61e77-373c-4555-9b13-56bec4ec7e6f", "Expiry Date");
			this.expiryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 31, true);
			this.expiryDateDateEdit.Name = "expiryDateDateEdit";
			this.expiryDateDateEdit.TabIndex = 3;
			// 
			// approvalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.approvalNumberTextBox, "AWBHeaderManager.EH_AgentApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_AgentApprovalNumber)));
			this.approvalNumberTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4bbdb544-882f-48db-8803-64e7e614d32f", "Regulated Agent Identifier");
			this.approvalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 31, true);
			this.approvalNumberTextBox.Name = "approvalNumberTextBox";
			this.approvalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 18, true);
			this.approvalNumberTextBox.TabIndex = 2;
			// 
			// receivedFromGroupBox
			// 
			this.receivedFromGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("3282f59f-3e40-4b6a-b62d-97855b473d57", "Received From");
			this.receivedFromGroupBox.Controls.Add(this.receivedFromGrid);
			this.receivedFromGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 136, true);
			this.receivedFromGroupBox.Name = "receivedFromGroupBox";
			this.receivedFromGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 273, true);
			this.receivedFromGroupBox.TabIndex = 2;
			this.receivedFromGroupBox.TabStop = false;
			// 
			// receivedFromGrid
			// 
			this.receivedFromGrid.AllowBeginDrag = false;
			this.receivedFromGrid.AllowCopyToNewRowMenuItem = false;
			this.receivedFromGrid.AllowDragDropWithChanges = false;
			this.receivedFromGrid.AllowNavigation = false;
			this.receivedFromGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.receivedFromGrid, "AWBHeaderManager.CargoSecurityKnownShippers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityKnownShippers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityKnownShippers)).SyncRoot)).EAS_ApprovalCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityKnownShippers)).SyncRoot)).EAS_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityKnownShippers)).SyncRoot)).EAS_ApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityKnownShippers)).SyncRoot)).EAS_ApprovalExpiryDate)));
			this.receivedFromGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("bf7376ab-b6cb-4eb1-8906-3c3bf98e1ca3", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "EAS_ApprovalCategory";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ffcbe00d-1d84-46ac-92ce-cb2c30dd67aa", "Country/Region", "Approval Country/Region", "The issuing country/region for the approval");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "EAS_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4258054e-fbda-42d5-8886-5b4796658e88", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "EAS_ApprovalNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("092a0c29-7b50-4502-ba4a-6da5b0a361ef", "Expiry", "Expiry Date", "The expiry date of the approval.");
			zDateEditColumnStyleInfo1.ColumnName = "EAS_ApprovalExpiryDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.receivedFromGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.receivedFromGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.receivedFromGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.receivedFromGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.receivedFromGrid.CopyColumnCaptionsToBoundFields = false;
			this.receivedFromGrid.CopySelectedRowsAllowed = false;
			this.receivedFromGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.receivedFromGrid.GridId = "5b547f8b-123c-4c61-9133-7a29a94cca61";
			this.receivedFromGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.receivedFromGrid.IsCustomiseMenuVisible = false;
			this.receivedFromGrid.LayoutKey = "receivedFromGrid";
			this.receivedFromGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.receivedFromGrid.Name = "receivedFromGrid";
			this.receivedFromGrid.RowHeadersVisible = false;
			this.receivedFromGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 256, true);
			this.receivedFromGrid.TabIndex = 0;
			// 
			// screeningMethodGroupBox
			// 
			this.screeningMethodGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("fcba28e6-5c34-4c3f-bba3-b8d90b6f61b9", "Screening Method");
			this.screeningMethodGroupBox.Controls.Add(this.screeningMethodsGrid);
			this.screeningMethodGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 136, true);
			this.screeningMethodGroupBox.Name = "screeningMethodGroupBox";
			this.screeningMethodGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 276, true);
			this.screeningMethodGroupBox.TabIndex = 3;
			this.screeningMethodGroupBox.TabStop = false;
			// 
			// screeningMethodsGrid
			// 
			this.screeningMethodsGrid.AllowBeginDrag = false;
			this.screeningMethodsGrid.AllowCopyToNewRowMenuItem = false;
			this.screeningMethodsGrid.AllowDragDropWithChanges = false;
			this.screeningMethodsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.screeningMethodsGrid, "AWBHeaderManager.CargoSecurityScreeningMethods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityScreeningMethods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityScreeningMethods)).SyncRoot)).EAS_ScreeningMethod)));
			this.screeningMethodsGrid.CaptionVisible = false;
			this.screeningMethodsGrid.ColumnHeadersVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("2944bc30-623d-476b-9d52-7e16e31abb75", "Screening Method");
			zTextBoxColumnStyleInfo2.ColumnName = "EAS_ScreeningMethod";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.screeningMethodsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.screeningMethodsGrid.CopyColumnCaptionsToBoundFields = false;
			this.screeningMethodsGrid.CopySelectedRowsAllowed = false;
			this.screeningMethodsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.screeningMethodsGrid.GridId = "cf48e0dd-2c6b-4f24-8e0d-ed5d36076a74";
			this.screeningMethodsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.screeningMethodsGrid.LayoutKey = "screeningMethodsGrid";
			this.screeningMethodsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.screeningMethodsGrid.Name = "screeningMethodsGrid";
			this.screeningMethodsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 259, true);
			this.screeningMethodsGrid.TabIndex = 0;
			// 
			// groundsForExemptionGroupBox
			// 
			this.groundsForExemptionGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("68c33c5b-6e6c-4587-bc5c-9e2481a04bab", "Grounds for Exemption");
			this.groundsForExemptionGroupBox.Controls.Add(this.groundsForExemptionGrid);
			this.groundsForExemptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 133, true);
			this.groundsForExemptionGroupBox.Name = "groundsForExemptionGroupBox";
			this.groundsForExemptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 279, true);
			this.groundsForExemptionGroupBox.TabIndex = 4;
			this.groundsForExemptionGroupBox.TabStop = false;
			// 
			// groundsForExemptionGrid
			// 
			this.groundsForExemptionGrid.AllowBeginDrag = false;
			this.groundsForExemptionGrid.AllowCopyToNewRowMenuItem = false;
			this.groundsForExemptionGrid.AllowDragDropWithChanges = false;
			this.groundsForExemptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.groundsForExemptionGrid, "AWBHeaderManager.CargoSecurityExemptionGrounds");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityExemptionGrounds)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.AWB.Business.ExportAWBSecurityStatusLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).CargoSecurityExemptionGrounds)).SyncRoot)).EAS_ExemptionGround)));
			this.groundsForExemptionGrid.CaptionVisible = false;
			this.groundsForExemptionGrid.ColumnHeadersVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("cee2ed74-5c3c-4037-af1e-84ceef38167a", "Ground for Exemption");
			zTextBoxColumnStyleInfo3.ColumnName = "EAS_ExemptionGround";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.groundsForExemptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.groundsForExemptionGrid.CopyColumnCaptionsToBoundFields = false;
			this.groundsForExemptionGrid.CopySelectedRowsAllowed = false;
			this.groundsForExemptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groundsForExemptionGrid.GridId = "ae6edfdc-c92a-4faa-986e-0a8bd05a1d07";
			this.groundsForExemptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.groundsForExemptionGrid.LayoutKey = "zGrid1";
			this.groundsForExemptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.groundsForExemptionGrid.Name = "groundsForExemptionGrid";
			this.groundsForExemptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 262, true);
			this.groundsForExemptionGrid.TabIndex = 0;
			// 
			// securityStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.securityStatusTextBox, "AWBHeaderManager.EH_SecurityStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_SecurityStatus)));
			this.securityStatusTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e197ff89-3464-4ce3-b135-f1c8565978b9", "Security Status");
			this.securityStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 441, true);
			this.securityStatusTextBox.Name = "securityStatusTextBox";
			this.securityStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 18, true);
			this.securityStatusTextBox.TabIndex = 5;
			// 
			// personScreeningTextBox
			// 
			this.BindingSource.SetBindingMember(this.personScreeningTextBox, "AWBHeaderManager.EH_SecurityStatusIssuedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_SecurityStatusIssuedBy)));
			this.personScreeningTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.personScreeningTextBox, false);
			this.personScreeningTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 469, true);
			this.personScreeningTextBox.Name = "personScreeningTextBox";
			this.personScreeningTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 18, true);
			this.personScreeningTextBox.TabIndex = 7;
			// 
			// otherScreeningMethodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.otherScreeningMethodsTextBox, "AWBHeaderManager.EH_AdditionalScreeningMethods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_AdditionalScreeningMethods)));
			this.otherScreeningMethodsTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("7a99d656-a47e-4bcc-bccb-15c38140f74e", "Other Screening Methods");
			this.LabelCaptionRenderProvider.SetLabelTop(this.otherScreeningMethodsTextBox, 0);
			this.otherScreeningMethodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 441, true);
			this.otherScreeningMethodsTextBox.Multiline = true;
			this.otherScreeningMethodsTextBox.Name = "otherScreeningMethodsTextBox";
			this.otherScreeningMethodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 96, true);
			this.otherScreeningMethodsTextBox.TabIndex = 8;
			// 
			// dateTimeOfScreeningDateEdit
			// 
			this.dateTimeOfScreeningDateEdit.AllowDrop = true;
			this.dateTimeOfScreeningDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateTimeOfScreeningDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateTimeOfScreeningDateEdit, "AWBHeaderManager.EH_SecurityStatusIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_SecurityStatusIssueDate)));
			this.dateTimeOfScreeningDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("15d0c23a-b866-4fe9-9c68-34db3a035a5d", "Screening Date/Time");
			this.dateTimeOfScreeningDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.dateTimeOfScreeningDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 496, true);
			this.dateTimeOfScreeningDateEdit.Name = "dateTimeOfScreeningDateEdit";
			this.dateTimeOfScreeningDateEdit.TabIndex = 9;
			// 
			// additionalSecurityInfoTextBox
			// 
			this.BindingSource.SetBindingMember(this.additionalSecurityInfoTextBox, "AWBHeaderManager.EH_AdditionalSecurityInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_AdditionalSecurityInformation)));
			this.additionalSecurityInfoTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("91b65d82-a356-4777-adb1-ab0c8cfcb2d4", "Additional Security Information");
			this.additionalSecurityInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 547, true);
			this.additionalSecurityInfoTextBox.Multiline = true;
			this.additionalSecurityInfoTextBox.Name = "additionalSecurityInfoTextBox";
			this.additionalSecurityInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 101, true);
			this.additionalSecurityInfoTextBox.TabIndex = 10;
			// 
			// personScreeningFindBox
			// 
			this.personScreeningFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.personScreeningFindBox, "AWBHeaderManager.EH_GS_NKSecurityStatusIssuedByCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_GS_NKSecurityStatusIssuedByCode)));
			this.personScreeningFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e9b85f4b-dcbf-404c-8a4d-cbc440b7a70f", "Person Screening");
			this.personScreeningFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 469, true);
			this.personScreeningFindBox.Name = "personScreeningFindBox";
			this.personScreeningFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.personScreeningFindBox.ParentType = null;
			this.personScreeningFindBox.ShowDescriptionBox = false;
			this.personScreeningFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.personScreeningFindBox.TabIndex = 6;
			// 
			// dateTimeOfScheduledArrivalDateEdit
			// 
			this.dateTimeOfScheduledArrivalDateEdit.AllowDrop = true;
			this.dateTimeOfScheduledArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateTimeOfScheduledArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateTimeOfScheduledArrivalDateEdit, "AWBHeaderManager.EH_ScheduledArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_ScheduledArrivalDate)));
			this.dateTimeOfScheduledArrivalDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("18049068-3920-4c40-ad42-525b1ac205a4", "Scheduled Arrival Date at CTO");
			this.dateTimeOfScheduledArrivalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.dateTimeOfScheduledArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 660, true);
			this.dateTimeOfScheduledArrivalDateEdit.Name = "dateTimeOfScheduledArrivalDateEdit";
			this.dateTimeOfScheduledArrivalDateEdit.TabIndex = 11;
			// 
			// chooseAdditionalSecurityInformationStatementDropEdit
			// 
			this.BindingSource.SetBindingMember(this.chooseAdditionalSecurityInformationStatementDropEdit, "AWBHeaderManager.EH_AdditionalSecurityInformationStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).AWBHeaderManager)).SyncRoot)).EH_AdditionalSecurityInformationStatement)));
			this.chooseAdditionalSecurityInformationStatementDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b8ee90a1-9956-413c-8925-b44b92b6472d", "Security Statement");
			this.chooseAdditionalSecurityInformationStatementDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 660, true);
			this.chooseAdditionalSecurityInformationStatementDropEdit.Name = "chooseAdditionalSecurityInformationStatementDropEdit";
			this.chooseAdditionalSecurityInformationStatementDropEdit.ShowDescriptionBox = false;
			this.chooseAdditionalSecurityInformationStatementDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.chooseAdditionalSecurityInformationStatementDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.chooseAdditionalSecurityInformationStatementDropEdit.PreBoundMaxLength = 100;
			this.chooseAdditionalSecurityInformationStatementDropEdit.TabIndex = 12;
			// 
			// SecurityDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.additionalSecurityInfoTextBox);
			this.Controls.Add(this.dateTimeOfScreeningDateEdit);
			this.Controls.Add(this.otherScreeningMethodsTextBox);
			this.Controls.Add(this.personScreeningFindBox);
			this.Controls.Add(this.personScreeningTextBox);
			this.Controls.Add(this.securityStatusTextBox);
			this.Controls.Add(this.groundsForExemptionGroupBox);
			this.Controls.Add(this.screeningMethodGroupBox);
			this.Controls.Add(this.receivedFromGroupBox);
			this.Controls.Add(this.securityStatusPartyGroupBox);
			this.Controls.Add(this.overrideValuesCheckBox);
			this.Controls.Add(this.dateTimeOfScheduledArrivalDateEdit);
			this.Controls.Add(this.chooseAdditionalSecurityInformationStatementDropEdit);
			this.Name = "SecurityDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(769, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.securityStatusPartyGroupBox.ResumeLayout(false);
			this.securityStatusPartyGroupBox.PerformLayout();
			this.issuingCountryCodeFindBox.ResumeLayout(true);
			this.issuingCountryCodeFindBox.PerformLayout();
			this.expiryDateDateEdit.ResumeLayout(true);
			this.expiryDateDateEdit.PerformLayout();
			this.receivedFromGroupBox.ResumeLayout(false);
			this.receivedFromGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.receivedFromGrid)).EndInit();
			this.receivedFromGrid.ResumeLayout(false);
			this.receivedFromGrid.PerformLayout();
			this.screeningMethodGroupBox.ResumeLayout(false);
			this.screeningMethodGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.screeningMethodsGrid)).EndInit();
			this.screeningMethodsGrid.ResumeLayout(false);
			this.screeningMethodsGrid.PerformLayout();
			this.groundsForExemptionGroupBox.ResumeLayout(false);
			this.groundsForExemptionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.groundsForExemptionGrid)).EndInit();
			this.groundsForExemptionGrid.ResumeLayout(false);
			this.groundsForExemptionGrid.PerformLayout();
			this.dateTimeOfScreeningDateEdit.ResumeLayout(true);
			this.dateTimeOfScreeningDateEdit.PerformLayout();
			this.personScreeningFindBox.ResumeLayout(true);
			this.personScreeningFindBox.PerformLayout();
			this.dateTimeOfScheduledArrivalDateEdit.ResumeLayout(true);
			this.dateTimeOfScheduledArrivalDateEdit.PerformLayout();
			this.chooseAdditionalSecurityInformationStatementDropEdit.ResumeLayout(true);
			this.chooseAdditionalSecurityInformationStatementDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox overrideValuesCheckBox;
		private ZArchitecture.GUI.ZGroupBox securityStatusPartyGroupBox;
		private ZArchitecture.ZTextBox approvalNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit expiryDateDateEdit;
		private ZArchitecture.GUI.ZCodeFindBox issuingCountryCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox receivedFromGroupBox;
		private ZArchitecture.GUI.ZGroupBox screeningMethodGroupBox;
		private ZArchitecture.GUI.ZGroupBox groundsForExemptionGroupBox;
		private ZArchitecture.ZTextBox securityStatusTextBox;
		private ZArchitecture.ZTextBox personScreeningTextBox;
		private ZArchitecture.ZTextBox otherScreeningMethodsTextBox;
		private ZArchitecture.GUI.ZDateEdit dateTimeOfScreeningDateEdit;
		private ZArchitecture.ZGrid receivedFromGrid;
		private ZArchitecture.ZGrid screeningMethodsGrid;
		private ZArchitecture.ZGrid groundsForExemptionGrid;
		private ZArchitecture.ZTextBox additionalSecurityInfoTextBox;
		private ZArchitecture.GUI.ZCodeFindBox personScreeningFindBox;
		private ZArchitecture.GUI.ZDateEdit dateTimeOfScheduledArrivalDateEdit;
		private ZArchitecture.GUI.ZDropEdit chooseAdditionalSecurityInformationStatementDropEdit;
	}
}
