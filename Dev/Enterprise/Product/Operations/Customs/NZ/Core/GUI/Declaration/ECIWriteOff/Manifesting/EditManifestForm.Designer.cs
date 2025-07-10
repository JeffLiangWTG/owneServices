namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	partial class EditManifestForm
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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.DeclarationsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeclarationModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.DeclarationsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DeclarationCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessagingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelManifestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SubmitManifestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationsAndMessagingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ManifestDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CarrierOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.BarrierPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BarrierDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ECIWriteOffEntryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EDITransmitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AmountPayableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationsPanel.SuspendLayout();
			this.DeclarationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationModuleButtonGrid.InnerGrid)).BeginInit();
			this.DeclarationModuleButtonGrid.SuspendLayout();
			this.DeclarationsDetailsGroupBox.SuspendLayout();
			this.MessagingPanel.SuspendLayout();
			this.MessageModeDropEdit.SuspendLayout();
			this.DeclarationsAndMessagingPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.ManifestDetailsGroupBox.SuspendLayout();
			this.CarrierOrganisationFindBox.SuspendLayout();
			this.BarrierPortCodeFindBox.SuspendLayout();
			this.BarrierDateEdit.SuspendLayout();
			this.ECIWriteOffEntryDetailsGroupBox.SuspendLayout();
			this.EDITransmitDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 415, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader);
			// 
			// DeclarationsPanel
			// 
			this.DeclarationsPanel.Controls.Add(this.DeclarationsGroupBox);
			this.DeclarationsPanel.Controls.Add(this.MessagingPanel);
			this.DeclarationsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsPanel.Name = "DeclarationsPanel";
			this.DeclarationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 260, true);
			this.DeclarationsPanel.TabIndex = 9;
			// 
			// DeclarationsGroupBox
			// 
			this.DeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d2fb6a61-d750-4b52-9c4d-f5ab32bf28a3", "Attached Declarations");
			this.DeclarationsGroupBox.Controls.Add(this.DeclarationModuleButtonGrid);
			this.DeclarationsGroupBox.Controls.Add(this.DeclarationsDetailsGroupBox);
			this.DeclarationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsGroupBox.Name = "DeclarationsGroupBox";
			this.DeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 228, true);
			this.DeclarationsGroupBox.TabIndex = 0;
			this.DeclarationsGroupBox.TabStop = false;
			// 
			// DeclarationModuleButtonGrid
			// 
			this.DeclarationModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationModuleButtonGrid, "Declarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).Declarations)));
			zTextBoxColumnStyleInfo1.Caption = "Declaration No.";
			zTextBoxColumnStyleInfo1.ColumnName = "JE_DeclarationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo2.Caption = "Entry Status";
			zTextBoxColumnStyleInfo2.ColumnName = "JE_EntryStatusDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(169);
			zTextBoxColumnStyleInfo3.Caption = "House Bill";
			zTextBoxColumnStyleInfo3.ColumnName = "JE_HouseBill";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			zTextBoxColumnStyleInfo4.Caption = "Destination";
			zTextBoxColumnStyleInfo4.ColumnName = "JE_RL_NKFinalDestination";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Packages";
			zCalcEditColumnStyleInfo1.ColumnName = "JE_TotalNoOfPacks";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups.SuppliersList";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Supplier";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JE_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups.ImportersList";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "Importer";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "JE_OH_Importer";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Goods Description";
			zTextBoxColumnStyleInfo5.ColumnName = "JE_GoodsDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Weight";
			zCalcEditColumnStyleInfo2.ColumnName = "JE_TotalWeight";
			zCalcEditColumnStyleInfo2.Decimals = 1;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("EditManifestForm|3e352b74-a7ad-4ffa-b3dd-c008e86cce7a", "Total Weight");
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zTextBoxColumnStyleInfo6.Caption = "Unit";
			zTextBoxColumnStyleInfo6.ColumnName = "JE_TotalWeightUnit";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("EditManifestForm|3e352b74-a7ad-4ffa-b3dd-c008e86cce7a", "Total Weight");
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Value";
			zCalcEditColumnStyleInfo3.ColumnName = "JE_ECI_InvoiceAmount";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("EditManifestForm|9386c500-852c-487e-bfe0-2d61de67088c", "Goods Value");
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.CurrencyList";
			zGuidFindBoxColumnStyleInfo1.Caption = "Curr";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JE_ECI_InvoiceCurrency";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("EditManifestForm|9386c500-852c-487e-bfe0-2d61de67088c", "Goods Value");
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(43);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.DeclarationModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DeclarationModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationModuleButtonGrid.GridId = "38710dae-e53b-48bf-bee0-9c2193e00df0";
			// 
			// 
			// 
			this.DeclarationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DeclarationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DeclarationModuleButtonGrid.InnerGrid.GridId = null;
			this.DeclarationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DeclarationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DeclarationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DeclarationModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DeclarationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 133, true);
			this.DeclarationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DeclarationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.DeclarationModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.NZ.CUSCAR;
			this.DeclarationModuleButtonGrid.Name = "DeclarationModuleButtonGrid";
			this.DeclarationModuleButtonGrid.NameOfAGridElement = Enterprise.Customs.NZ.GUI.Res.GetData("17554FFC-970B-467D-AF72-8C3DFC66A4AC", "Declaration");
			this.DeclarationModuleButtonGrid.ReadOnly = false;
			this.DeclarationModuleButtonGrid.ShowAttachButton = false;
			this.DeclarationModuleButtonGrid.ShowDetachButton = false;
			this.DeclarationModuleButtonGrid.ShowNewButton = false;
			this.DeclarationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 170, true);
			this.DeclarationModuleButtonGrid.TabIndex = 0;
			// 
			// DeclarationsDetailsGroupBox
			// 
			this.DeclarationsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("38fd5c36-0aee-49a6-8552-d2f8e6c2678c", "Declaration Details");
			this.DeclarationsDetailsGroupBox.Controls.Add(this.PackagesCalcEdit);
			this.DeclarationsDetailsGroupBox.Controls.Add(this.DeclarationCountCalcEdit);
			this.DeclarationsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DeclarationsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 184, true);
			this.DeclarationsDetailsGroupBox.Name = "DeclarationsDetailsGroupBox";
			this.DeclarationsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 42, true);
			this.DeclarationsDetailsGroupBox.TabIndex = 1;
			this.DeclarationsDetailsGroupBox.TabStop = false;
			// 
			// PackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackagesCalcEdit, "PackagesCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).PackagesCount)));
			this.PackagesCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A9CFB5CE-316A-4051-A4EA-761B7DD5AE5F", "No. Of Packages");
			this.PackagesCalcEdit.DecimalPlaces = 2;
			this.PackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 17, true);
			this.PackagesCalcEdit.Name = "PackagesCalcEdit";
			this.PackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.PackagesCalcEdit.TabIndex = 1;
			this.PackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DeclarationCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DeclarationCountCalcEdit, "ManifestWrapper.DeclarationCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.DeclarationCount)));
			this.DeclarationCountCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("B5ADE2A6-E90F-4BE8-85E3-DB3D692B3BD5", "No. Of  Declarations");
			this.DeclarationCountCalcEdit.DecimalPlaces = 2;
			this.DeclarationCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 17, true);
			this.DeclarationCountCalcEdit.Name = "DeclarationCountCalcEdit";
			this.DeclarationCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.DeclarationCountCalcEdit.TabIndex = 0;
			this.DeclarationCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MessagingPanel
			// 
			this.MessagingPanel.Controls.Add(this.CloseButton);
			this.MessagingPanel.Controls.Add(this.CancelManifestButton);
			this.MessagingPanel.Controls.Add(this.SubmitManifestButton);
			this.MessagingPanel.Controls.Add(this.MessageModeDropEdit);
			this.MessagingPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MessagingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.MessagingPanel.Name = "MessagingPanel";
			this.MessagingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 32, true);
			this.MessagingPanel.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 5, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("1e935c71-d22c-43d4-bd25-f9b6a57589c6", "Close");
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// CancelManifestButton
			// 
			this.CancelManifestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelManifestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 5, true);
			this.CancelManifestButton.Name = "CancelManifestButton";
			this.CancelManifestButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelManifestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.CancelManifestButton.TabIndex = 2;
			this.CancelManifestButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("15e191b7-3164-48a3-8af9-9da85ce8bcb4", "&Cancel Manifest");
			this.CancelManifestButton.ToolTipCaption = null;
			this.CancelManifestButton.Click += new System.EventHandler(this.CancelManifestButton_Click);
			// 
			// SubmitManifestButton
			// 
			this.SubmitManifestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SubmitManifestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 5, true);
			this.SubmitManifestButton.Name = "SubmitManifestButton";
			this.SubmitManifestButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SubmitManifestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.SubmitManifestButton.TabIndex = 1;
			this.SubmitManifestButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("fbeef84c-feea-47b1-98b6-5437cdb241fb", "&Submit Manifest");
			this.SubmitManifestButton.ToolTipCaption = null;
			this.SubmitManifestButton.Click += new System.EventHandler(this.SubmitManifestButton_Click);
			// 
			// MessageModeDropEdit
			// 
			this.MessageModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageModeDropEdit, "ManifestWrapper.MessageMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.MessageMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).Lookups.MessageModeList)));
			this.MessageModeDropEdit.BindToList = "Lookups.MessageModeList";
			this.MessageModeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("15525411-3178-459f-a74c-d42cb8a3934b", "Msg.", "Messaging", "Messaging Mode", "Select the messaging system to be used for sending of this manifest.");
			this.MessageModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.MessageModeDropEdit.Name = "MessageModeDropEdit";
			this.MessageModeDropEdit.PreBoundMaxLength = 3;
			this.MessageModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 18, true);
			this.MessageModeDropEdit.TabIndex = 0;
			// 
			// DeclarationsAndMessagingPanel
			// 
			this.DeclarationsAndMessagingPanel.Controls.Add(this.DeclarationsPanel);
			this.DeclarationsAndMessagingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsAndMessagingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.DeclarationsAndMessagingPanel.Name = "DeclarationsAndMessagingPanel";
			this.DeclarationsAndMessagingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 260, true);
			this.DeclarationsAndMessagingPanel.TabIndex = 8;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.DeclarationsAndMessagingPanel);
			this.MainPanel.Controls.Add(this.ManifestDetailsGroupBox);
			this.MainPanel.Controls.Add(this.ECIWriteOffEntryDetailsGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 415, true);
			this.MainPanel.TabIndex = 7;
			// 
			// ManifestDetailsGroupBox
			// 
			this.ManifestDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6C2702FF-53C8-4CD7-A81F-5F55EFE89157", "Manifest Details");
			this.ManifestDetailsGroupBox.Controls.Add(this.CarrierOrganisationFindBox);
			this.ManifestDetailsGroupBox.Controls.Add(this.BarrierPortCodeFindBox);
			this.ManifestDetailsGroupBox.Controls.Add(this.BarrierDateEdit);
			this.ManifestDetailsGroupBox.Controls.Add(this.FlightNoTextBox);
			this.ManifestDetailsGroupBox.Controls.Add(this.MasterBillTextBox);
			this.ManifestDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ManifestDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 66, true);
			this.ManifestDetailsGroupBox.Name = "ManifestDetailsGroupBox";
			this.ManifestDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 90, true);
			this.ManifestDetailsGroupBox.TabIndex = 1;
			this.ManifestDetailsGroupBox.TabStop = false;
			// 
			// CarrierOrganisationFindBox
			// 
			this.CarrierOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierOrganisationFindBox, "ManifestWrapper.Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).Declaration.Lookups.AirShippingLineList)));
			this.CarrierOrganisationFindBox.BindToList = "Declaration+Lookups+AirShippingLineList";
			this.CarrierOrganisationFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("B9AC4FDA-424F-415E-9CF0-A9A8945F75D0", "Carrier");
			this.CarrierOrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CarrierOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 61, true);
			this.CarrierOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CarrierOrganisationFindBox.Name = "CarrierOrganisationFindBox";
			this.CarrierOrganisationFindBox.ShouldResize = true;
			this.CarrierOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 18, true);
			this.CarrierOrganisationFindBox.TabIndex = 4;
			// 
			// BarrierPortCodeFindBox
			// 
			this.BarrierPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BarrierPortCodeFindBox, "ManifestWrapper.BarrierPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.BarrierPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).Declaration.Lookups.BarrierPortList)));
			this.BarrierPortCodeFindBox.BindToList = "Declaration+Lookups+BarrierPortList";
			this.BarrierPortCodeFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3DF9681B-B635-4C12-8DB4-95CA0FA64D9D", "Local Transfer Port");
			this.BarrierPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 38, true);
			this.BarrierPortCodeFindBox.Name = "BarrierPortCodeFindBox";
			this.BarrierPortCodeFindBox.PreBoundMaxLength = 5;
			this.BarrierPortCodeFindBox.ShouldResize = true;
			this.BarrierPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 18, true);
			this.BarrierPortCodeFindBox.TabIndex = 3;
			// 
			// BarrierDateEdit
			// 
			this.BarrierDateEdit.AllowDrop = true;
			this.BarrierDateEdit.AutoCompleteMonthThreshold = 1;
			this.BarrierDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BarrierDateEdit, "ManifestWrapper.BarrierDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.BarrierDate)));
			this.BarrierDateEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("DCF96421-EAAE-4A46-A87C-481A6FDDF42B", "Local Transfer Date");
			this.BarrierDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 16, true);
			this.BarrierDateEdit.Name = "BarrierDateEdit";
			this.BarrierDateEdit.TabIndex = 1;
			// 
			// FlightNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNoTextBox, "ManifestWrapper.FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.FlightNo)));
			this.FlightNoTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("1FBA62AE-F573-4F7E-BF45-8EEDCD9DFC82", "Flight Number");
			this.FlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.FlightNoTextBox.Name = "FlightNoTextBox";
			this.FlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.FlightNoTextBox.TabIndex = 0;
			// 
			// MasterBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.MasterBillTextBox, "ManifestWrapper.FormattedMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.FormattedMasterBill)));
			this.MasterBillTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E9438336-C5C1-4FD4-9BCF-DF99B94FBCFD", "Master Bill");
			this.MasterBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.MasterBillTextBox.Name = "MasterBillTextBox";
			this.MasterBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 18, true);
			this.MasterBillTextBox.TabIndex = 2;
			// 
			// ECIWriteOffEntryDetailsGroupBox
			// 
			this.ECIWriteOffEntryDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("096271eb-338d-41d6-b11d-b2d68f21a224", "ECI Write-Off Entry Details");
			this.ECIWriteOffEntryDetailsGroupBox.Controls.Add(this.EDITransmitDateDateEdit);
			this.ECIWriteOffEntryDetailsGroupBox.Controls.Add(this.EntryStatusTextBox);
			this.ECIWriteOffEntryDetailsGroupBox.Controls.Add(this.EntryNoTextBox);
			this.ECIWriteOffEntryDetailsGroupBox.Controls.Add(this.AmountPayableCalcEdit);
			this.ECIWriteOffEntryDetailsGroupBox.Controls.Add(this.EntryTypeTextBox);
			this.ECIWriteOffEntryDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ECIWriteOffEntryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ECIWriteOffEntryDetailsGroupBox.Name = "ECIWriteOffEntryDetailsGroupBox";
			this.ECIWriteOffEntryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 66, true);
			this.ECIWriteOffEntryDetailsGroupBox.TabIndex = 0;
			this.ECIWriteOffEntryDetailsGroupBox.TabStop = false;
			// 
			// EDITransmitDateDateEdit
			// 
			this.EDITransmitDateDateEdit.AllowDrop = true;
			this.EDITransmitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EDITransmitDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EDITransmitDateDateEdit, "ManifestWrapper.EDITransmitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.EDITransmitDate)));
			this.EDITransmitDateDateEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("94B13617-FFCE-4202-8CC5-F57A879213D4", "Transmit Date");
			this.EDITransmitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 38, true);
			this.EDITransmitDateDateEdit.Name = "EDITransmitDateDateEdit";
			this.EDITransmitDateDateEdit.TabIndex = 3;
			// 
			// EntryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryStatusTextBox, "ManifestWrapper.StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.StatusDescription)));
			this.EntryStatusTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("BAACD9B0-707C-46BE-A8BC-540AE125D60B", "Entry Status");
			this.EntryStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EntryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 16, true);
			this.EntryStatusTextBox.Name = "EntryStatusTextBox";
			this.EntryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 18, true);
			this.EntryStatusTextBox.TabIndex = 1;
			// 
			// EntryNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNoTextBox, "ManifestWrapper.EntryNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.EntryNo)));
			this.EntryNoTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A099BF3A-29F3-424E-B269-53E858DDACDC", "Entry Number");
			this.EntryNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.EntryNoTextBox.Name = "EntryNoTextBox";
			this.EntryNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.EntryNoTextBox.TabIndex = 0;
			// 
			// AmountPayableCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountPayableCalcEdit, "ManifestWrapper.AmountPayable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.AmountPayable)));
			this.AmountPayableCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("E061568F-D5B0-4882-8801-CFD49C90CC32", "Amount Payable");
			this.AmountPayableCalcEdit.DecimalPlaces = 2;
			this.AmountPayableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 38, true);
			this.AmountPayableCalcEdit.Name = "AmountPayableCalcEdit";
			this.AmountPayableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.AmountPayableCalcEdit.TabIndex = 4;
			this.AmountPayableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EntryTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryTypeTextBox, "ManifestWrapper.MessageTypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader)(null)).ManifestWrapper.MessageTypeDescription)));
			this.EntryTypeTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("01AE95F6-EF27-49EA-83E5-79CF7C22FFB0", "Entry Type");
			this.EntryTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EntryTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 38, true);
			this.EntryTypeTextBox.Name = "EntryTypeTextBox";
			this.EntryTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.EntryTypeTextBox.TabIndex = 2;
			// 
			// EditManifestForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 439, true);
			this.Controls.Add(this.MainPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 477, true);
			this.Name = "EditManifestForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationsPanel.ResumeLayout(false);
			this.DeclarationsPanel.PerformLayout();
			this.DeclarationsGroupBox.ResumeLayout(false);
			this.DeclarationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationModuleButtonGrid.InnerGrid)).EndInit();
			this.DeclarationModuleButtonGrid.ResumeLayout(true);
			this.DeclarationModuleButtonGrid.PerformLayout();
			this.DeclarationsDetailsGroupBox.ResumeLayout(false);
			this.DeclarationsDetailsGroupBox.PerformLayout();
			this.MessagingPanel.ResumeLayout(false);
			this.MessagingPanel.PerformLayout();
			this.MessageModeDropEdit.ResumeLayout(true);
			this.MessageModeDropEdit.PerformLayout();
			this.DeclarationsAndMessagingPanel.ResumeLayout(false);
			this.DeclarationsAndMessagingPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.ManifestDetailsGroupBox.ResumeLayout(false);
			this.ManifestDetailsGroupBox.PerformLayout();
			this.CarrierOrganisationFindBox.ResumeLayout(true);
			this.CarrierOrganisationFindBox.PerformLayout();
			this.BarrierPortCodeFindBox.ResumeLayout(true);
			this.BarrierPortCodeFindBox.PerformLayout();
			this.BarrierDateEdit.ResumeLayout(true);
			this.BarrierDateEdit.PerformLayout();
			this.ECIWriteOffEntryDetailsGroupBox.ResumeLayout(false);
			this.ECIWriteOffEntryDetailsGroupBox.PerformLayout();
			this.EDITransmitDateDateEdit.ResumeLayout(true);
			this.EDITransmitDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZCalcEdit PackagesCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox EntryStatusTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit DeclarationCountCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit AmountPayableCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid DeclarationModuleButtonGrid;
		private Enterprise.ZArchitecture.ZTextBox EntryNoTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ECIWriteOffEntryDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox EntryTypeTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox BarrierPortCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BarrierDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ManifestDetailsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MasterBillTextBox;
		private Enterprise.ZArchitecture.ZTextBox FlightNoTextBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox CarrierOrganisationFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit EDITransmitDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton SubmitManifestButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelManifestButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit MessageModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DeclarationsAndMessagingPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel MessagingPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DeclarationsPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationsDetailsGroupBox;

		#endregion
	}
}
