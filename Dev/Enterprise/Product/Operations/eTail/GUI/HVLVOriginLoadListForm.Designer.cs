namespace Enterprise.eTail.GUI
{
	partial class HVLVOriginLoadListForm
	{
		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		private ZArchitecture.GUI.ZGroupBox hvlvOriginLoadListGroupBox;
		private ZArchitecture.GUI.ZAddressControl originDepotAddressControl;
		private ZArchitecture.GUI.ZAddressControl destinationDepotAddressControl;
		private ZArchitecture.GUI.ZDropEdit transportModeDropEdit;
		private ZArchitecture.GUI.ZAddressControl originCTOAddressControl;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox zOrganisationFindBoxCarrier;
		private ZArchitecture.GUI.ZDropEdit serviceLevelDropEdit;
		private ZArchitecture.GUI.ZDropEdit incoTermDropEdit;
		private ZArchitecture.GUI.ZButton incoTermExplainButton;
		private ZArchitecture.ZLabel paymentTermDisplay;
		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.GUI.ZCheckBox isMasterHouse;
		private ZArchitecture.ZTextBox masterBillNumberTextBox;
		private ZArchitecture.GUI.ZCheckBox isNeutralMaster;
		private ZArchitecture.ZTextBox houseBillNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit eTARVBoundDateEdit;
		private ZArchitecture.GUI.ZDateEdit eTDEPBoundDateEdit;
		private ZArchitecture.ZTextBox voyageFlightTextBox;
		private ZArchitecture.GUI.ZCodeFindBox vesselCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox containerTypeGuidFindBox;
		private ZArchitecture.ZTextBox containerNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox originPort;
		private ZArchitecture.GUI.ZCodeFindBox destinationPort;
		private MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		private ZArchitecture.GUI.ZTabPage ItemsTabPage;
		private ZArchitecture.ZGrid itemsGrid;

		protected new void InitializeComponent()
		{
			this.hvlvOriginLoadListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.originDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.destinationDepotAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.transportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.originCTOAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.zOrganisationFindBoxCarrier = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.serviceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.incoTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.incoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.paymentTermDisplay = new Enterprise.ZArchitecture.ZLabel();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isMasterHouse = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.masterBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.isNeutralMaster = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.houseBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eTARVBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.eTDEPBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.voyageFlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.vesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.containerTypeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.containerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.originPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.destinationPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.ItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.itemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.hvlvOriginLoadListGroupBox.SuspendLayout();
			this.originDepotAddressControl.SuspendLayout();
			this.destinationDepotAddressControl.SuspendLayout();
			this.transportModeDropEdit.SuspendLayout();
			this.originCTOAddressControl.SuspendLayout();
			this.zOrganisationFindBoxCarrier.SuspendLayout();
			this.serviceLevelDropEdit.SuspendLayout();
			this.incoTermDropEdit.SuspendLayout();
			this.incoTermExplainButton.SuspendLayout();
			this.paymentTermDisplay.SuspendLayout();
			this.statusDropEdit.SuspendLayout();
			this.isMasterHouse.SuspendLayout();
			this.isNeutralMaster.SuspendLayout();
			this.eTARVBoundDateEdit.SuspendLayout();
			this.eTDEPBoundDateEdit.SuspendLayout();
			this.vesselCodeFindBox.SuspendLayout();
			this.containerTypeGuidFindBox.SuspendLayout();
			this.originPort.SuspendLayout();
			this.destinationPort.SuspendLayout();
			this.ItemsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ItemsTabPage);
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 330, true);
			this.workflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.workflowTabPage_InitializeTab));
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.hvlvOriginLoadListGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 282, true);
			// 
			// ItemsTabPage
			// 
			this.ItemsTabPage.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("237a95fb-2cc6-4ae8-bfa2-d4a05daabc03", "Items");
			this.ItemsTabPage.Controls.Add(this.itemsGrid);
			this.ItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemsTabPage.Name = "ItemsTabPage";
			this.ItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 303, true);
			this.ItemsTabPage.TabIndex = 4;
			this.ItemsTabPage.Text = "Items";
			// 
			// itemsGrid
			// 
			this.itemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.itemsGrid, "Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).Items)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVItem)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).Items)).SyncRoot)).HVI_CurrentBarcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVItem)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).Items)).SyncRoot)).HVI_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVItem)(((System.Collections.IList)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).Items)).SyncRoot)).HVI_Status)));
			this.itemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "HVI_CurrentBarcode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "HVI_GoodsDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zTextBoxColumnStyleInfo3.ColumnName = "HVI_Status_Description";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "BookingServiceLevel";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "ConsigneeCountry";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.itemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemsGrid.GridId = "a96ba892-1cda-428a-8e4f-70e23d467b6f";
			this.itemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.itemsGrid.LayoutKey = "itemsGrid";
			this.itemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemsGrid.Name = "itemsGrid";
			this.itemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 303, true);
			this.itemsGrid.TabIndex = 0;
			this.itemsGrid.AllowDrop = true;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 282, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 303, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 330, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVOriginLoadList);
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.workflowTabPage.TabIndex = 3;
			// 
			// hvlvOriginLoadListGroupBox
			// 
			this.hvlvOriginLoadListGroupBox.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("f64dd9d4-188c-49cc-8d4b-365e4d2203fd", "Origin Load List");
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.originDepotAddressControl);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.destinationDepotAddressControl);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.transportModeDropEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.originCTOAddressControl);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.zOrganisationFindBoxCarrier);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.serviceLevelDropEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.incoTermDropEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.incoTermExplainButton);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.paymentTermDisplay);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.statusDropEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.isMasterHouse);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.masterBillNumberTextBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.isNeutralMaster);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.houseBillNumberTextBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.eTARVBoundDateEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.eTDEPBoundDateEdit);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.voyageFlightTextBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.vesselCodeFindBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.containerTypeGuidFindBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.containerNumberTextBox);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.originPort);
			this.hvlvOriginLoadListGroupBox.Controls.Add(this.destinationPort);
			this.hvlvOriginLoadListGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hvlvOriginLoadListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.hvlvOriginLoadListGroupBox.Name = "hvlvOriginLoadListGroupBox";
			this.hvlvOriginLoadListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 282, true);
			this.hvlvOriginLoadListGroupBox.TabIndex = 0;
			this.hvlvOriginLoadListGroupBox.TabStop = false;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "HVL_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_Status)));
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.ShouldResizeByMaxLength = true;
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.statusDropEdit.TabIndex = 0;
			// 
			// IsMasterHouse
			// 
			this.BindingSource.SetBindingMember(this.isMasterHouse, "HVL_IsMasterHouse");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_IsMasterHouse)));
			this.isMasterHouse.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 12, true);
			this.isMasterHouse.Name = "IsMasterHouse";
			this.isMasterHouse.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 24, true);
			this.isMasterHouse.UseVisualStyleBackColor = true;
			this.isMasterHouse.TabIndex = 1;
			// 
			// originDepotAddressControl
			//
			this.originDepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originDepotAddressControl, "HVL_OA_OriginDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_OA_OriginDepot)));
			this.originDepotAddressControl.BindToOrgList = "Lookups.HVL_OA_OriginDepot_List";
			this.originDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 15, true);
			this.originDepotAddressControl.Name = "originDepotAddressControl";
			this.originDepotAddressControl.PopupCaption = "";
			this.originDepotAddressControl.ReadOnly = false;
			this.originDepotAddressControl.ShowAddress = false;
			this.originDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.originDepotAddressControl.TabIndex = 2;
			// 
			// transportModeDropEdit
			// 
			this.transportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.transportModeDropEdit, "HVL_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_TransportMode)));
			this.transportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.transportModeDropEdit.Name = "transportModeDropEdit";
			this.transportModeDropEdit.ShouldResizeByMaxLength = true;
			this.transportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.transportModeDropEdit.TabIndex = 3;
			// 
			// originPort
			// 
			this.originPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originPort, "HVL_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_RL_NKOrigin)));
			this.originPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 40, true);
			this.originPort.Name = "originPort";
			this.originPort.ShowDescriptionBox = false;
			this.originPort.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.originPort.CodeBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.originPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.originPort.TabIndex = 4;
			// 
			// masterBillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.masterBillNumberTextBox, "HVL_MasterBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_MasterBillNumber)));
			this.masterBillNumberTextBox.CaptionResourceString = null;
			this.masterBillNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.masterBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 65, true);
			this.masterBillNumberTextBox.Name = "masterBillNumberTextBox";
			this.masterBillNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.masterBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			this.masterBillNumberTextBox.TabIndex = 5;
			// 
			// IsNeutralMAWB
			// 
			this.BindingSource.SetBindingMember(this.isNeutralMaster, "HVL_IsNeutralMaster");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_IsNeutralMaster)));
			this.isNeutralMaster.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 62, true);
			this.isNeutralMaster.Name = "IsNeutralMAWB";
			this.isNeutralMaster.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 24, true);
			this.isNeutralMaster.UseVisualStyleBackColor = true;
			this.isNeutralMaster.TabIndex = 6;
			// 
			// destinationDepotAddressControl
			// 
			this.destinationDepotAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationDepotAddressControl, "HVL_OA_DestinationDepot");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_OA_DestinationDepot)));
			this.destinationDepotAddressControl.BindToOrgList = "Lookups.HVL_OA_DestinationDepot_List";
			this.destinationDepotAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 65, true);
			this.destinationDepotAddressControl.Name = "destinationDepotAddressControl";
			this.destinationDepotAddressControl.PopupCaption = "";
			this.destinationDepotAddressControl.ReadOnly = false;
			this.destinationDepotAddressControl.ShowAddress = false;
			this.destinationDepotAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.destinationDepotAddressControl.TabIndex = 7;
			// 
			// houseBillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.houseBillNumberTextBox, "HVL_HouseBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_HouseBillNumber)));
			this.houseBillNumberTextBox.CaptionResourceString = null;
			this.houseBillNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.houseBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 90, true);
			this.houseBillNumberTextBox.Name = "houseBillNumberTextBox";
			this.houseBillNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.houseBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.houseBillNumberTextBox.TabIndex = 8;
			// 
			// destinationPort
			// 
			this.destinationPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.destinationPort, "HVL_RL_NKDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_RL_NKDestination)));
			this.destinationPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 90, true);
			this.destinationPort.Name = "destinationPort";
			this.destinationPort.CodeBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.destinationPort.CodeBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.destinationPort.ShowDescriptionBox = false;
			this.destinationPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.destinationPort.TabIndex = 9;
			// 
			// serviceLevelDropEdit
			// 
			this.serviceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceLevelDropEdit, "HVL_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_RS_NKServiceLevel)));
			this.serviceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 115, true);
			this.serviceLevelDropEdit.Name = "serviceLevelDropEdit";
			this.serviceLevelDropEdit.ShouldResizeByMaxLength = true;
			this.serviceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.serviceLevelDropEdit.TabIndex = 10;
			// 
			// originCTOAddressControl
			// 
			this.originCTOAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.originCTOAddressControl, "OriginCTO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).OriginCTO)));
			this.originCTOAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 115, true);
			this.originCTOAddressControl.Name = "originCTOAddressControl";
			this.originCTOAddressControl.PopupCaption = "";
			this.originCTOAddressControl.ReadOnly = false;
			this.originCTOAddressControl.ShowAddress = false;
			this.originCTOAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.originCTOAddressControl.TabIndex = 11;
			// 
			// IncotermDropEdit
			//
			this.incoTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.incoTermDropEdit, "HVL_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_INCO)));
			this.incoTermDropEdit.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("E95EDEF4-2A2F-4A70-ACE8-C30F7CDC673C", "Incoterm");
			this.incoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 140, true);
			this.incoTermDropEdit.Name = "incoTermDropEdit";
			this.incoTermDropEdit.PreBoundMaxLength = 3;
			this.incoTermDropEdit.ShouldResizeByMaxLength = true;
			this.incoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.incoTermDropEdit.TabIndex = 12;
			// 
			// zButtonIncoTermExplainButton
			// 
			this.incoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 140, true);
			this.incoTermExplainButton.Name = "incoTermExplainButton";
			this.incoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.incoTermExplainButton.Text = "...";
			this.incoTermExplainButton.Click += new System.EventHandler(IncoTermExplainButton_Click);
			this.incoTermExplainButton.TabIndex = 13;
			//
			// zLabelPaymentTermDisplay
			//
			this.paymentTermDisplay.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("05928710-ebe0-4ab1-8e0a-320b7d2ff975", "Freight Collect");
			this.BindingSource.SetBindingMember(this.paymentTermDisplay, "PaymentTermDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVConsignment)(null)).PaymentTermDisplay)));
			this.paymentTermDisplay.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 140, true);
			this.paymentTermDisplay.Name = "paymentTermDisplay";
			this.paymentTermDisplay.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.paymentTermDisplay.TabIndex = 14;
			// 
			// vesselCodeFindBox
			// 
			this.vesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vesselCodeFindBox, "HVL_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_VesselName)));
			this.vesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 165, true);
			this.vesselCodeFindBox.Name = "vesselCodeFindBox";
			this.vesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.vesselCodeFindBox.ParentType = null;
			this.vesselCodeFindBox.ShowDescriptionBox = false;
			this.vesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.vesselCodeFindBox.TabIndex = 15;
			// 
			// zOrganisationFindBoxCarrier
			// 
			this.zOrganisationFindBoxCarrier.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zOrganisationFindBoxCarrier, "HVL_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_OH_Carrier)));
			this.zOrganisationFindBoxCarrier.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 165, true);
			this.zOrganisationFindBoxCarrier.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBoxCarrier.Name = "zOrganisationFindBoxCarrier";
			this.zOrganisationFindBoxCarrier.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zOrganisationFindBoxCarrier.ParentType = null;
			this.zOrganisationFindBoxCarrier.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			this.zOrganisationFindBoxCarrier.TabIndex = 16;
			// 
			// voyageFlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.voyageFlightTextBox, "HVL_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_VoyageFlight)));
			this.voyageFlightTextBox.CaptionResourceString = null;
			this.voyageFlightTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.voyageFlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 190, true);
			this.voyageFlightTextBox.Name = "voyageFlightTextBox";
			this.voyageFlightTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.voyageFlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.voyageFlightTextBox.TabIndex = 17;
			// 
			// eTDEPBoundDateEdit
			// 
			this.eTDEPBoundDateEdit.AllowDrop = true;
			this.eTDEPBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.eTDEPBoundDateEdit.AutoCompleteYear = false;
			this.BindingSource.SetBindingMember(this.eTDEPBoundDateEdit, "HVL_E_Dep");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_E_Dep)));
			this.eTDEPBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 215, true);
			this.eTDEPBoundDateEdit.Name = "eTDEPBoundDateEdit";
			this.eTDEPBoundDateEdit.TabIndex = 18;
			// 
			// eTARVBoundDateEdit
			// 
			this.eTARVBoundDateEdit.AllowDrop = true;
			this.eTARVBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.eTARVBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.eTARVBoundDateEdit, "HVL_E_Arv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_E_Arv)));
			this.eTARVBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 215, true);
			this.eTARVBoundDateEdit.Name = "eTARVBoundDateEdit";
			this.eTARVBoundDateEdit.TabIndex = 19;
			// 
			// containerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.containerNumberTextBox, "HVL_ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_ContainerNumber)));
			this.containerNumberTextBox.CaptionResourceString = null;
			this.containerNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.containerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 240, true);
			this.containerNumberTextBox.Name = "containerNumberTextBox";
			this.containerNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.containerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.containerNumberTextBox.TabIndex = 20;
			// 
			// containerTypeGuidFindBox
			// 
			this.containerTypeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containerTypeGuidFindBox, "HVL_RC_ContainerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.eTail.Business.HVLVOriginLoadList)(null)).HVL_RC_ContainerType)));
			this.containerTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 240, true);
			this.containerTypeGuidFindBox.Name = "containerTypeGuidFindBox";
			this.containerTypeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.containerTypeGuidFindBox.ParentType = null;
			this.containerTypeGuidFindBox.ShowDescriptionBox = false;
			this.containerTypeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.containerTypeGuidFindBox.TabIndex = 21;
			// 
			// HVLVOriginLoadListForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(834, 386, true);
			this.DataSourceType = typeof(Enterprise.eTail.Business.HVLVOriginLoadList);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 425, true);
			this.Name = "HVLVOriginLoadListForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.hvlvOriginLoadListGroupBox.ResumeLayout(false);
			this.hvlvOriginLoadListGroupBox.PerformLayout();
			this.originDepotAddressControl.ResumeLayout(true);
			this.originDepotAddressControl.PerformLayout();
			this.destinationDepotAddressControl.ResumeLayout(true);
			this.destinationDepotAddressControl.PerformLayout();
			this.transportModeDropEdit.ResumeLayout(true);
			this.transportModeDropEdit.PerformLayout();
			this.originCTOAddressControl.ResumeLayout(true);
			this.originCTOAddressControl.PerformLayout();
			this.zOrganisationFindBoxCarrier.ResumeLayout(true);
			this.zOrganisationFindBoxCarrier.PerformLayout();
			this.serviceLevelDropEdit.ResumeLayout(true);
			this.serviceLevelDropEdit.PerformLayout();
			this.incoTermDropEdit.ResumeLayout(true);
			this.incoTermDropEdit.PerformLayout();
			this.incoTermExplainButton.ResumeLayout(true);
			this.incoTermExplainButton.PerformLayout();
			this.paymentTermDisplay.ResumeLayout(true);
			this.paymentTermDisplay.PerformLayout();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.isMasterHouse.ResumeLayout(true);
			this.isMasterHouse.PerformLayout();
			this.isNeutralMaster.ResumeLayout(true);
			this.isNeutralMaster.PerformLayout();
			this.eTARVBoundDateEdit.ResumeLayout(true);
			this.eTARVBoundDateEdit.PerformLayout();
			this.eTDEPBoundDateEdit.ResumeLayout(true);
			this.eTDEPBoundDateEdit.PerformLayout();
			this.vesselCodeFindBox.ResumeLayout(true);
			this.vesselCodeFindBox.PerformLayout();
			this.containerTypeGuidFindBox.ResumeLayout(true);
			this.containerTypeGuidFindBox.PerformLayout();
			this.originPort.ResumeLayout(true);
			this.originPort.PerformLayout();
			this.destinationPort.ResumeLayout(true);
			this.destinationPort.PerformLayout();
			this.ItemsTabPage.ResumeLayout(false);
			this.ItemsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.itemsGrid)).EndInit();
			this.itemsGrid.ResumeLayout(false);
			this.itemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void workflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.workflowTabPage.SuspendLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();

		}

		#endregion
	}
}
