namespace Enterprise.MasterFiles.GUI
{
	partial class RelationshipDetailsUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGroupBox filterGroupBox;
			Enterprise.ZArchitecture.GUI.ZDropEdit transportModeDropEdit;
			Enterprise.ZArchitecture.GUI.ZDropEdit containerModeDropEdit;
			Enterprise.ZArchitecture.GUI.ZTabPage defaultValuesTabPage;
			Enterprise.ZArchitecture.GUI.ZDropEdit incoDropEdit;
			Enterprise.ZArchitecture.GUI.ZDropEdit incoModeDropEdit;
			Enterprise.ZArchitecture.ZCalcEdit deliveryDaysCalcEdit;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox pickupTransportFindBox;
			Enterprise.ZArchitecture.GUI.ZCheckBox overrideDeliveryDaysCheckBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox deliverTransportFindBox;
			Enterprise.ZArchitecture.ZCalcEdit copyBillsCalcEdit;
			Enterprise.ZArchitecture.ZCalcEdit originalBillsCalcEdit;
			Enterprise.ZArchitecture.GUI.ZCodeFindBox serviceLevelDropEdit;
			Enterprise.ZArchitecture.GUI.ZGuidDropEdit notifyPartyContactDropEdit;
			Enterprise.ZArchitecture.GUI.ZGuidDropEdit pickupContactDropEdit;
			Enterprise.ZArchitecture.GUI.ZCodeFindBox originFindBox;
			Enterprise.ZArchitecture.GUI.ZCodeFindBox destinationFindBox;
			Enterprise.ZArchitecture.GUI.ZGuidDropEdit deliverContactDropEdit;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox carrierFindBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox sendingAgentFindBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox receivingAgentFindBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox controllingCustomerFindBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox customsBrokerFindBox;
			Enterprise.ZArchitecture.GUI.ZTabPage notesTabPage;
			CargoWise.Windows.UI.KSplitContainer splitContainer2;
			Enterprise.ZArchitecture.ZTextBox goodsDescriptionTextBox;
			Enterprise.ZArchitecture.ZTextBox handlingInstructionsTextBox;
			Enterprise.ZArchitecture.ZTextBox incoPlaceTextBox;
			Enterprise.ZArchitecture.GUI.ZTabPage modesTabPage;
			CargoWise.Windows.UI.KSplitContainer splitContainer1;
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo8 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo9 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo10 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo11 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo12 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo13 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo14 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.MasterFiles.GUI.RequiredDocumentsUserControl requiredDocumentsUserControl1;
			Enterprise.ZArchitecture.GUI.ZTabPage documentTrackingTabPage;
			Enterprise.ZArchitecture.GUI.ZTabPage packTypeTabPage;
			Enterprise.ZArchitecture.ZGrid packTypeGrid;
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZTabPage orderLineTolerancesTabPage;
			Enterprise.ZArchitecture.ZGrid orderLineTolerancesGrid;
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			CargoWise.Windows.UI.KPanel detailPanel;
			this.USPortOfUnLadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.USPortOfLadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.relationshipsGrid = new Enterprise.ZArchitecture.ZGrid();
			filterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			transportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			containerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			defaultValuesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			incoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			incoModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			deliveryDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			pickupTransportFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			overrideDeliveryDaysCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			deliverTransportFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			copyBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			originalBillsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			serviceLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			notifyPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			notifyPartyContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			pickupContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			originFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			pickupAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			destinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			deliverContactDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			carrierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			deliverAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			controlledArrivalLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			examSiteDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			sendingAgentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			receivingAgentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			controllingCustomerFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			customsBrokerFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			notesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			goodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			incoPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			handlingInstructionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			modesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			relationshipTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			requiredDocumentsUserControl1 = new Enterprise.MasterFiles.GUI.RequiredDocumentsUserControl();
			documentTrackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			packTypeTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			packTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			orderLineTolerancesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			orderLineTolerancesGrid = new Enterprise.ZArchitecture.ZGrid();
			modesAndTracking = new Enterprise.ZArchitecture.GUI.ZTabControl();
			detailPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			filterGroupBox.SuspendLayout();
			defaultValuesTabPage.SuspendLayout();
			pickupAddressControl.SuspendLayout();
			deliverAddressControl.SuspendLayout();
			notifyPartyAddressControl.SuspendLayout();
			notesTabPage.SuspendLayout();
			splitContainer2.Panel1.SuspendLayout();
			splitContainer2.Panel2.SuspendLayout();
			splitContainer2.SuspendLayout();
			modesTabPage.SuspendLayout();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.relationshipsGrid)).BeginInit();
			relationshipTabControl.SuspendLayout();
			documentTrackingTabPage.SuspendLayout();
			packTypeTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(packTypeGrid)).BeginInit();
			orderLineTolerancesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(orderLineTolerancesGrid)).BeginInit();
			modesAndTracking.SuspendLayout();
			detailPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierBuyerLink);
			// 
			// filterGroupBox
			// 
			filterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|a2b4c85b-8354-4ef9-a24e-be0cd6b85c56", "Filters");
			filterGroupBox.Controls.Add(transportModeDropEdit);
			filterGroupBox.Controls.Add(containerModeDropEdit);
			filterGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			filterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			filterGroupBox.Name = "filterGroupBox";
			filterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 208, true);
			filterGroupBox.TabIndex = 0;
			filterGroupBox.TabStop = false;
			// 
			// transportModeDropEdit
			// 
			transportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(transportModeDropEdit, "OrgSupBuyLinkTrnModes.PF_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_TransportMode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(transportModeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			transportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 32, true);
			transportModeDropEdit.Name = "transportModeDropEdit";
			transportModeDropEdit.PreBoundMaxLength = 3;
			transportModeDropEdit.ShowDescriptionBox = false;
			transportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			transportModeDropEdit.TabIndex = 0;
			// 
			// containerModeDropEdit
			// 
			containerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(containerModeDropEdit, "OrgSupBuyLinkTrnModes.PF_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_ContainerMode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(containerModeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			containerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 72, true);
			containerModeDropEdit.Name = "containerModeDropEdit";
			containerModeDropEdit.PreBoundMaxLength = 3;
			containerModeDropEdit.ShowDescriptionBox = false;
			containerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			containerModeDropEdit.TabIndex = 1;
			// 
			// defaultValuesTabPage
			// 
			defaultValuesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|a2006793-d74f-45c6-b051-2df1f1a25dda", "Default Values");
			defaultValuesTabPage.Controls.Add(detailPanel);
			defaultValuesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			defaultValuesTabPage.Name = "defaultValuesTabPage";
			defaultValuesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			defaultValuesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 181, true);
			defaultValuesTabPage.TabIndex = 0;
			defaultValuesTabPage.UseVisualStyleBackColor = true;
			// 
			// incoDropEdit
			// 
			incoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(incoDropEdit, "OrgSupBuyLinkTrnModes.PF_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTerm)));
			incoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			incoDropEdit.Name = "incoDropEdit";
			incoDropEdit.PreBoundMaxLength = 3;
			incoDropEdit.ShowDescriptionBox = false;
			incoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			incoDropEdit.TabIndex = 0;
			// 
			// incoPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(incoPlaceTextBox, "OrgSupBuyLinkTrnModes.PF_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTermPlace)));
			incoPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 8, true);
			incoPlaceTextBox.Name = "incoPlaceTextBox";
			incoPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			incoPlaceTextBox.TabIndex = 0;
			// 
			// 	incoModeDropEdit
			// 
			incoModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(incoModeDropEdit, "OrgSupBuyLinkTrnModes.PF_IncoTermMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTermMode)));
			incoModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 8, true);
			incoModeDropEdit.Name = "incoModeDropEdit";
			incoModeDropEdit.PreBoundMaxLength = 3;
			incoModeDropEdit.ShowDescriptionBox = false;
			incoModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			incoModeDropEdit.TabIndex = 0;
			// 
			// deliveryDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(deliveryDaysCalcEdit, "OrgSupBuyLinkTrnModes.PF_EstDeliveryDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_EstDeliveryDays)));
			deliveryDaysCalcEdit.DecimalPlaces = 0;
			deliveryDaysCalcEdit.Decimals = 0;
			deliveryDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 152, true);
			deliveryDaysCalcEdit.Name = "deliveryDaysCalcEdit";
			deliveryDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			deliveryDaysCalcEdit.TabIndex = 22;
			deliveryDaysCalcEdit.Text = "0";
			deliveryDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// pickupTransportFindBox
			// 
			pickupTransportFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(pickupTransportFindBox, "OrgSupBuyLinkTrnModes.PF_OH_PickupCartageContractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_PickupCartageContractor)));
			pickupTransportFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 32, true);
			pickupTransportFindBox.Name = "pickupTransportFindBox";
			pickupTransportFindBox.ShowDescriptionBox = false;
			pickupTransportFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			pickupTransportFindBox.TabIndex = 11;
			// 
			// overrideDeliveryDaysCheckBox
			// 
			overrideDeliveryDaysCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(overrideDeliveryDaysCheckBox, "OrgSupBuyLinkTrnModes.PF_OverrideDeliveryDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OverrideDeliveryDays)));
			overrideDeliveryDaysCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			overrideDeliveryDaysCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 152, true);
			overrideDeliveryDaysCheckBox.Name = "overrideDeliveryDaysCheckBox";
			overrideDeliveryDaysCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 17, true);
			overrideDeliveryDaysCheckBox.TabIndex = 21;
			overrideDeliveryDaysCheckBox.UseVisualStyleBackColor = true;
			// 
			// deliverTransportFindBox
			// 
			deliverTransportFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(deliverTransportFindBox, "OrgSupBuyLinkTrnModes.PF_OH_DeliveryCartageContractor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_DeliveryCartageContractor)));
			deliverTransportFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 56, true);
			deliverTransportFindBox.Name = "deliverTransportFindBox";
			deliverTransportFindBox.ShowDescriptionBox = false;
			deliverTransportFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			deliverTransportFindBox.TabIndex = 18;
			// 
			// USPortOfUnLadingCodeFindBox
			// 
			this.USPortOfUnLadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.USPortOfUnLadingCodeFindBox, "OrgSupBuyLinkTrnModes.PF_USPortOfUnLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_USPortOfUnLading)));
			this.USPortOfUnLadingCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|9bd38372-0e9c-4072-8b86-e8b3f7a51579", "US Port Of Unlading", "US Port Of Unlading", "US Port Of Unlading", "The US Port Of Unlading to use when this Buyer/Supplier Trade Relationship is used in a Customs job.");
			this.USPortOfUnLadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 128, true);
			this.USPortOfUnLadingCodeFindBox.Name = "USPortOfUnLadingCodeFindBox";
			this.USPortOfUnLadingCodeFindBox.PreBoundMaxLength = 5;
			this.USPortOfUnLadingCodeFindBox.ShowDescriptionBox = false;
			this.USPortOfUnLadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.USPortOfUnLadingCodeFindBox.TabIndex = 5;
			// 
			// copyBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(copyBillsCalcEdit, "OrgSupBuyLinkTrnModes.PF_NoOfCopyBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_NoOfCopyBills)));
			copyBillsCalcEdit.DecimalPlaces = 2;
			copyBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 152, true);
			copyBillsCalcEdit.Name = "copyBillsCalcEdit";
			copyBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			copyBillsCalcEdit.TabIndex = 20;
			copyBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// originalBillsCalcEdit
			// 
			this.BindingSource.SetBindingMember(originalBillsCalcEdit, "OrgSupBuyLinkTrnModes.PF_NoOfOriginalBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_NoOfOriginalBills)));
			originalBillsCalcEdit.DecimalPlaces = 0;
			originalBillsCalcEdit.Decimals = 0;
			originalBillsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 152, true);
			originalBillsCalcEdit.Name = "originalBillsCalcEdit";
			originalBillsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			originalBillsCalcEdit.TabIndex = 19;
			originalBillsCalcEdit.Text = "0";
			originalBillsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// serviceLevelDropEdit
			// 
			serviceLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(serviceLevelDropEdit, "OrgSupBuyLinkTrnModes.PF_RS_NKDefaultServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RS_NKDefaultServiceLevel)));
			serviceLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 32, true);
			serviceLevelDropEdit.Name = "serviceLevelDropEdit";
			serviceLevelDropEdit.PreBoundMaxLength = 3;
			serviceLevelDropEdit.ShowDescriptionBox = false;
			serviceLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			serviceLevelDropEdit.TabIndex = 1;
			// 
			// notifyPartyAddressControl
			//
			notifyPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(notifyPartyAddressControl, "OrgSupBuyLinkTrnModes.PF_OA_OverrideNotifyPartyAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_OverrideNotifyPartyAddress)));
			notifyPartyAddressControl.BindToOrgList = "OrgSupBuyLinkTrnModes.Lookups.NotifyPartyOrganisations";
			notifyPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 8, true);
			notifyPartyAddressControl.Name = "notifyPartyAddressControl";
			notifyPartyAddressControl.PopupCaption = null;
			notifyPartyAddressControl.ReadOnly = false;
			notifyPartyAddressControl.ShowAddress = false;
			notifyPartyAddressControl.ShowOrganisationName = true;
			notifyPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			notifyPartyAddressControl.TabIndex = 16;
			// 
			// notifyPartyContactDropEdit
			// 
			notifyPartyContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(notifyPartyContactDropEdit, "OrgSupBuyLinkTrnModes.PF_OC_OverrideNotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideNotifyParty)));
			notifyPartyContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 32, true);
			notifyPartyContactDropEdit.Name = "notifyPartyContactDropEdit";
			notifyPartyContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			notifyPartyContactDropEdit.TabIndex = 17;
			// 
			// USPortOfLadingCodeFindBox
			// 
			this.USPortOfLadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.USPortOfLadingCodeFindBox, "OrgSupBuyLinkTrnModes.PF_USPortOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_USPortOfLading)));
			this.USPortOfLadingCodeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|bdf1af36-5261-4b8d-bc7d-e5b95d5c1886", "US Port Of Lading", "US Port Of Lading", "US Port Of Lading", "The US Port Of Lading to use when this Buyer/Supplier Trade Relationship is used in a Customs job.");
			this.USPortOfLadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 104, true);
			this.USPortOfLadingCodeFindBox.Name = "USPortOfLadingCodeFindBox";
			this.USPortOfLadingCodeFindBox.PreBoundMaxLength = 5;
			this.USPortOfLadingCodeFindBox.ShowDescriptionBox = false;
			this.USPortOfLadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.USPortOfLadingCodeFindBox.TabIndex = 4;
			// 
			// pickupContactDropEdit
			// 
			pickupContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(pickupContactDropEdit, "OrgSupBuyLinkTrnModes.PF_OC_OverrideSupplierContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideSupplierContact)));
			pickupContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 128, true);
			pickupContactDropEdit.Name = "pickupContactDropEdit";
			pickupContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			pickupContactDropEdit.TabIndex = 15;
			// 
			// originFindBox
			// 
			originFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(originFindBox, "OrgSupBuyLinkTrnModes.PF_RL_NKPlaceOfReceivalPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKPlaceOfReceivalPort)));
			originFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 56, true);
			originFindBox.Name = "originFindBox";
			originFindBox.PreBoundMaxLength = 5;
			originFindBox.ShowDescriptionBox = false;
			originFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			originFindBox.TabIndex = 2;
			// 
			// pickupAddressControl
			//
			pickupAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(pickupAddressControl, "OrgSupBuyLinkTrnModes.PF_OA_OverridePickupAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_OverridePickupAddress)));
			pickupAddressControl.BindToOrgList = "OrgSupBuyLinkTrnModes.Lookups.PickupOrganisations";
			pickupAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 104, true);
			pickupAddressControl.Name = "pickupAddressControl";
			pickupAddressControl.PopupCaption = null;
			pickupAddressControl.ReadOnly = false;
			pickupAddressControl.ShowAddress = false;
			pickupAddressControl.ShowOrganisationName = true;
			pickupAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			pickupAddressControl.TabIndex = 14;
			// 
			// destinationFindBox
			// 
			destinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(destinationFindBox, "OrgSupBuyLinkTrnModes.PF_RL_NKPlaceOfDeliveryPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKPlaceOfDeliveryPort)));
			destinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 80, true);
			destinationFindBox.Name = "destinationFindBox";
			destinationFindBox.PreBoundMaxLength = 5;
			destinationFindBox.ShowDescriptionBox = false;
			destinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			destinationFindBox.TabIndex = 3;
			// 
			// deliverContactDropEdit
			// 
			deliverContactDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(deliverContactDropEdit, "OrgSupBuyLinkTrnModes.PF_OC_OverrideConsigneeContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideConsigneeContact)));
			deliverContactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 80, true);
			deliverContactDropEdit.Name = "deliverContactDropEdit";
			deliverContactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			deliverContactDropEdit.TabIndex = 13;
			// 
			// carrierFindBox
			// 
			carrierFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(carrierFindBox, "OrgSupBuyLinkTrnModes.PF_OH_CarrierLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_CarrierLine)));
			carrierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 32, true);
			carrierFindBox.Name = "carrierFindBox";
			carrierFindBox.ShowDescriptionBox = false;
			carrierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			carrierFindBox.TabIndex = 6;
			// 
			// deliverAddressControl
			// 
			deliverAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(deliverAddressControl, "OrgSupBuyLinkTrnModes.PF_OA_OverrideDeliveryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_OverrideDeliveryAddress)));
			deliverAddressControl.BindToOrgList = "OrgSupBuyLinkTrnModes.Lookups.DeliveryOrganisations";
			deliverAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 56, true);
			deliverAddressControl.Name = "deliverAddressControl";
			deliverAddressControl.PopupCaption = null;
			deliverAddressControl.ReadOnly = false;
			deliverAddressControl.ShowAddress = false;
			deliverAddressControl.ShowOrganisationName = true;
			deliverAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			deliverAddressControl.TabIndex = 12;
			// 
			// controlledArrivalLocationDropEdit
			// 
			controlledArrivalLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(controlledArrivalLocationDropEdit, "OrgSupBuyLinkTrnModes.PF_OA_CustomsControlledArrivalLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_CustomsControlledArrivalLocation)));
			controlledArrivalLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 80, true);
			controlledArrivalLocationDropEdit.Name = "controlledArrivalLocationDropEdit";
			controlledArrivalLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			controlledArrivalLocationDropEdit.TabIndex = 23;
			// 
			// examSiteDropEdit
			// 
			examSiteDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(examSiteDropEdit, "OrgSupBuyLinkTrnModes.PF_OA_CustomsExamSite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_CustomsExamSite)));
			examSiteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(910, 104, true);
			examSiteDropEdit.Name = "examSiteDropEdit";
			examSiteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			examSiteDropEdit.TabIndex = 24;
			// 
			// sendingAgentFindBox
			// 
			sendingAgentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(sendingAgentFindBox, "OrgSupBuyLinkTrnModes.PF_OH_SendingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_SendingAgent)));
			sendingAgentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 56, true);
			sendingAgentFindBox.Name = "sendingAgentFindBox";
			sendingAgentFindBox.ShowDescriptionBox = false;
			sendingAgentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			sendingAgentFindBox.TabIndex = 7;
			// 
			// receivingAgentFindBox
			// 
			receivingAgentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(receivingAgentFindBox, "OrgSupBuyLinkTrnModes.PF_OH_ReceivingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ReceivingAgent)));
			receivingAgentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 80, true);
			receivingAgentFindBox.Name = "receivingAgentFindBox";
			receivingAgentFindBox.ShowDescriptionBox = false;
			receivingAgentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			receivingAgentFindBox.TabIndex = 8;
			// 
			// controllingCustomerFindBox
			// 
			controllingCustomerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(controllingCustomerFindBox, "OrgSupBuyLinkTrnModes.PF_OH_ControllingCustomer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ControllingCustomer)));
			controllingCustomerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 104, true);
			controllingCustomerFindBox.Name = "controllingCustomerFindBox";
			controllingCustomerFindBox.ShowDescriptionBox = false;
			controllingCustomerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			controllingCustomerFindBox.TabIndex = 9;
			// 
			// customsBrokerFindBox
			// 
			customsBrokerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(customsBrokerFindBox, "OrgSupBuyLinkTrnModes.PF_OH_ImportCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ImportCustomsAgent)));
			customsBrokerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 128, true);
			customsBrokerFindBox.Name = "customsBrokerFindBox";
			customsBrokerFindBox.ShowDescriptionBox = false;
			customsBrokerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			customsBrokerFindBox.TabIndex = 10;
			// 
			// notesTabPage
			// 
			notesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|10bff1ee-40aa-4cde-b578-0d2adae373cc", "Default Note Texts");
			notesTabPage.Controls.Add(splitContainer2);
			notesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			notesTabPage.Name = "notesTabPage";
			notesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			notesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 181, true);
			notesTabPage.TabIndex = 1;
			notesTabPage.UseVisualStyleBackColor = true;
			// 
			// splitContainer2
			// 
			splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			splitContainer2.Panel1.Controls.Add(goodsDescriptionTextBox);
			// 
			// splitContainer2.Panel2
			// 
			splitContainer2.Panel2.Controls.Add(handlingInstructionsTextBox);
			splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 176, true);
			splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			splitContainer2.TabIndex = 0;
			// 
			// goodsDescriptionTextBox
			// 
			goodsDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(goodsDescriptionTextBox, "OrgSupBuyLinkTrnModes.PF_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_GoodsDescription)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(goodsDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			goodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			goodsDescriptionTextBox.Multiline = true;
			goodsDescriptionTextBox.Name = "goodsDescriptionTextBox";
			goodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 156, true);
			goodsDescriptionTextBox.TabIndex = 26;
			// 
			// handlingInstructionsTextBox
			// 
			handlingInstructionsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(handlingInstructionsTextBox, "OrgSupBuyLinkTrnModes.PF_HandlingInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_HandlingInstructions)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(handlingInstructionsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			handlingInstructionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 19, true);
			handlingInstructionsTextBox.Multiline = true;
			handlingInstructionsTextBox.Name = "handlingInstructionsTextBox";
			handlingInstructionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 153, true);
			handlingInstructionsTextBox.TabIndex = 27;
			// 
			// modesTabPage
			// 
			modesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|2038891f-7351-4965-8715-e102c1e04078", "Modes");
			modesTabPage.Controls.Add(splitContainer1);
			modesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			modesTabPage.Name = "modesTabPage";
			modesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			modesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 461, true);
			modesTabPage.TabIndex = 0;
			// 
			// splitContainer1
			// 
			splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			splitContainer1.Panel1.Controls.Add(this.relationshipsGrid);
			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(relationshipTabControl);
			splitContainer1.Panel2.Controls.Add(filterGroupBox);
			splitContainer1.Panel2MinSize = 100;
			splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 455, true);
			splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(243);
			splitContainer1.TabIndex = 0;
			// 
			// relationshipsGrid
			// 
			this.relationshipsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.relationshipsGrid, "OrgSupBuyLinkTrnModes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTermMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_IncoTermPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RS_NKDefaultServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OverrideDeliveryDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_EstDeliveryDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_NoOfOriginalBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_NoOfCopyBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_CarrierLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_SendingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ReceivingAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ControllingCustomer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_ImportCustomsAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKPlaceOfReceivalPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_OverridePickupAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideSupplierContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_PickupCartageContractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_RL_NKPlaceOfDeliveryPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OA_OverrideDeliveryAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideConsigneeContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OH_DeliveryCartageContractor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_OC_OverrideNotifyParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupBuyLinkTrnMode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).OrgSupBuyLinkTrnModes)).SyncRoot)).PF_HandlingInstructions)));
			this.relationshipsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "PF_TransportMode";
			zDropEditColumnStyleInfo6.IsMandatory = true;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "PF_ContainerMode";
			zDropEditColumnStyleInfo7.IsMandatory = true;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "PF_IncoTerm";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo11.ColumnName = "PF_IncoTermMode";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "PF_IncoTermPlace";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "PF_RS_NKDefaultServiceLevel";
			zCheckBoxColumnStyleInfo3.ColumnName = "PF_OverrideDeliveryDays";
			zCheckBoxColumnStyleInfo3.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|532fe88f-513c-4cff-9790-c78b52f44bf2", "Estimated Delivery Days");
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "PF_EstDeliveryDays";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|532fe88f-513c-4cff-9790-c78b52f44bf2", "Estimated Delivery Days");
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "PF_NoOfOriginalBills";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|4397af1f-9890-4d2e-b5c6-9317332a26e1", "No of Bills");
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "PF_NoOfCopyBills";
			zCalcEditColumnStyleInfo10.Decimals = 0;
			zCalcEditColumnStyleInfo10.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|4397af1f-9890-4d2e-b5c6-9317332a26e1", "No of Bills");
			zOrganisationFindBoxColumnStyleInfo8.ColumnName = "PF_OH_CarrierLine";
			zOrganisationFindBoxColumnStyleInfo8.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo9.ColumnName = "PF_OH_SendingAgent";
			zOrganisationFindBoxColumnStyleInfo9.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo10.ColumnName = "PF_OH_ReceivingAgent";
			zOrganisationFindBoxColumnStyleInfo10.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo11.ColumnName = "PF_OH_ControllingCustomer";
			zOrganisationFindBoxColumnStyleInfo11.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo12.ColumnName = "PF_OH_ImportCustomsAgent";
			zOrganisationFindBoxColumnStyleInfo12.IsVisible = false;
			zCodeFindBoxColumnStyleInfo7.ColumnName = "PF_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo8.ColumnName = "PF_RL_NKPlaceOfReceivalPort";
			zGuidDropEditColumnStyleInfo7.ColumnName = "PF_OA_OverridePickupAddress";
			zGuidDropEditColumnStyleInfo8.ColumnName = "PF_OC_OverrideSupplierContact";
			zOrganisationFindBoxColumnStyleInfo13.ColumnName = "PF_OH_PickupCartageContractor";
			zCodeFindBoxColumnStyleInfo9.ColumnName = "PF_RL_NKDischargePort";
			zCodeFindBoxColumnStyleInfo10.ColumnName = "PF_RL_NKPlaceOfDeliveryPort";
			zGuidDropEditColumnStyleInfo9.ColumnName = "PF_OA_OverrideDeliveryAddress";
			zGuidDropEditColumnStyleInfo10.ColumnName = "PF_OC_OverrideConsigneeContact";
			zGuidDropEditColumnStyleInfo13.ColumnName = "PF_OA_CustomsControlledArrivalLocation";
			zGuidDropEditColumnStyleInfo14.ColumnName = "PF_OA_CustomsExamSite";
			zOrganisationFindBoxColumnStyleInfo14.ColumnName = "PF_OH_DeliveryCartageContractor";
			zGuidDropEditColumnStyleInfo11.ColumnName = "PF_OC_OverrideNotifyParty";
			zTextBoxColumnStyleInfo2.ColumnName = "PF_GoodsDescription";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo2.ColumnName = "PF_HandlingInstructions";
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 200;
			this.relationshipsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.relationshipsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.relationshipsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.relationshipsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.relationshipsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.relationshipsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.relationshipsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.relationshipsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.relationshipsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.relationshipsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo8);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo9);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo10);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo11);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo12);
			this.relationshipsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.relationshipsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo8);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo7);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo8);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo13);
			this.relationshipsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo9);
			this.relationshipsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo10);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo9);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo10);
			this.relationshipsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo14);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo11);
			this.relationshipsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.relationshipsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo13);
			this.relationshipsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo14);
			this.relationshipsGrid.GridId = "b609c8aa-8870-4c8f-b6ea-26ff0b505ae1";
			this.relationshipsGrid.CopySelectedRowsAllowed = true;
			this.relationshipsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relationshipsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.relationshipsGrid.LayoutKey = "OrgSupplierBuyerLinkBoundGrid";
			this.relationshipsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.relationshipsGrid.Name = "relationshipsGrid";
			this.relationshipsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 243, true);
			this.relationshipsGrid.TabIndex = 0;
			this.relationshipsGrid.AfterBind += new System.EventHandler(this.relationshipGrid_AfterBind);
			// 
			// relationshipTabControl
			// 
			relationshipTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			relationshipTabControl.Controls.Add(defaultValuesTabPage);
			relationshipTabControl.Controls.Add(notesTabPage);
			relationshipTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			relationshipTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 0, true);
			relationshipTabControl.Name = "relationshipTabControl";
			relationshipTabControl.SelectedIndex = 0;
			relationshipTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 208, true);
			relationshipTabControl.TabIndex = 0;
			// 
			// requiredDocumentsUserControl1
			// 
			requiredDocumentsUserControl1.AllowDrop = true;
			requiredDocumentsUserControl1.AutoScroll = true;
			requiredDocumentsUserControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			requiredDocumentsUserControl1.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(requiredDocumentsUserControl1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.IHaveRequiredDocuments)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)))));
			requiredDocumentsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			requiredDocumentsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			requiredDocumentsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 198, true);
			requiredDocumentsUserControl1.Name = "requiredDocumentsUserControl1";
			requiredDocumentsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 455, true);
			requiredDocumentsUserControl1.TabIndex = 0;
			// 
			// documentTrackingTabPage
			// 
			documentTrackingTabPage.BackColor = System.Drawing.Color.White;
			documentTrackingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|1ec00809-8326-4123-a4ba-572db8ce2f6e", "Document Tracking");
			documentTrackingTabPage.Controls.Add(requiredDocumentsUserControl1);
			documentTrackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			documentTrackingTabPage.Name = "documentTrackingTabPage";
			documentTrackingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			documentTrackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 461, true);
			documentTrackingTabPage.TabIndex = 1;
			// 
			// packTypeTabPage
			// 
			packTypeTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|b6500c12-24e9-4dbb-a347-949b78f5a97b", "Pack Type Dimensions");
			packTypeTabPage.Controls.Add(packTypeGrid);
			packTypeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			packTypeTabPage.Name = "packTypeTabPage";
			packTypeTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			packTypeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 461, true);
			packTypeTabPage.TabIndex = 2;
			// 
			// packTypeGrid
			// 
			packTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(packTypeGrid, "PackPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_F3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_UnitOfDimension)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgBuyerSupplierLinkPackPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).PackPivots)).SyncRoot)).Q0_UnitOfWeight)));
			packTypeGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo12.ColumnName = "Q0_F3";
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "Q0_Height";
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "Q0_Length";
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "Q0_Width";
			zDropEditColumnStyleInfo9.ColumnName = "Q0_UnitOfDimension";
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "Q0_Weight";
			zDropEditColumnStyleInfo10.ColumnName = "Q0_UnitOfWeight";
			packTypeGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo12);
			packTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			packTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			packTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			packTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			packTypeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			packTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			packTypeGrid.GridId = "44184aec-f30f-4741-9cce-1a23cb159500";
			packTypeGrid.CopySelectedRowsAllowed = true;
			packTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			packTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			packTypeGrid.LayoutKey = "zGrid2";
			packTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			packTypeGrid.Name = "packTypeGrid";
			packTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 455, true);
			packTypeGrid.TabIndex = 0;
			// 
			// orderLineTolerancesTabPage
			// 
			orderLineTolerancesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RelationshipDetailsUserControl|a3681456-e79d-486d-b094-9c504f93aa85", "Order Line Tolerances");
			orderLineTolerancesTabPage.Controls.Add(orderLineTolerancesGrid);
			orderLineTolerancesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			orderLineTolerancesTabPage.Name = "orderLineTolerancesTabPage";
			orderLineTolerancesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			orderLineTolerancesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 461, true);
			orderLineTolerancesTabPage.TabIndex = 3;
			// 
			// orderLineTolerancesGrid
			// 
			orderLineTolerancesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(orderLineTolerancesGrid, "Tolerances");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_PartNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_UnderQuantityPercentageLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_OverQuantityPercentageLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_EarlyShipmentLimitDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLinkTolerance)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierBuyerLink)(null)).Tolerances)).SyncRoot)).OLT_LateShipmentLimitDays)));
			orderLineTolerancesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo12.ColumnName = "OLT_TransportMode";
			zDropEditColumnStyleInfo12.IsMandatory = true;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo11.ColumnName = "OLT_PartNumber";
			zCodeFindBoxColumnStyleInfo11.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo11.PopupCaption = "Select the Product";
			zCodeFindBoxColumnStyleInfo11.ToolTip = "Enter the Product Number";
			zCodeFindBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.ColumnName = "OLT_UnderQuantityPercentageLimit";
			zCalcEditColumnStyleInfo15.Decimals = 3;
			zCalcEditColumnStyleInfo15.MaxValue = 99;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.ColumnName = "OLT_OverQuantityPercentageLimit";
			zCalcEditColumnStyleInfo16.Decimals = 3;
			zCalcEditColumnStyleInfo16.MaxValue = 999;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(165);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "OLT_EarlyShipmentLimitDays";
			zCalcEditColumnStyleInfo17.Decimals = 0;
			zCalcEditColumnStyleInfo17.MaxValue = 99;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "OLT_LateShipmentLimitDays";
			zCalcEditColumnStyleInfo18.Decimals = 0;
			zCalcEditColumnStyleInfo18.MaxValue = 99;
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			orderLineTolerancesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			orderLineTolerancesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo11);
			orderLineTolerancesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			orderLineTolerancesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			orderLineTolerancesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			orderLineTolerancesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			orderLineTolerancesGrid.GridId = "2c6a40c6-ed06-4da8-836f-9702168e23dd";
			orderLineTolerancesGrid.CopySelectedRowsAllowed = true;
			orderLineTolerancesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			orderLineTolerancesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			orderLineTolerancesGrid.LayoutKey = "OrgSupplierBuyerLinkTolerancesBoundGrid";
			orderLineTolerancesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			orderLineTolerancesGrid.Name = "orderLineTolerancesGrid";
			orderLineTolerancesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 455, true);
			orderLineTolerancesGrid.TabIndex = 0;
			// 
			// modesAndTracking
			// 
			modesAndTracking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			modesAndTracking.Controls.Add(modesTabPage);
			modesAndTracking.Controls.Add(documentTrackingTabPage);
			modesAndTracking.Controls.Add(packTypeTabPage);
			modesAndTracking.Controls.Add(orderLineTolerancesTabPage);
			modesAndTracking.Dock = System.Windows.Forms.DockStyle.Fill;
			modesAndTracking.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			modesAndTracking.Name = "modesAndTracking";
			modesAndTracking.SelectedIndex = 0;
			modesAndTracking.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 488, true);
			modesAndTracking.TabIndex = 0;
			// 
			// detailPanel
			// 
			detailPanel.AutoScroll = true;
			detailPanel.Controls.Add(incoDropEdit);
			detailPanel.Controls.Add(incoModeDropEdit);
			detailPanel.Controls.Add(incoPlaceTextBox);
			detailPanel.Controls.Add(deliveryDaysCalcEdit);
			detailPanel.Controls.Add(carrierFindBox);
			detailPanel.Controls.Add(overrideDeliveryDaysCheckBox);
			detailPanel.Controls.Add(pickupTransportFindBox);
			detailPanel.Controls.Add(copyBillsCalcEdit);
			detailPanel.Controls.Add(deliverAddressControl);
			detailPanel.Controls.Add(controlledArrivalLocationDropEdit);
			detailPanel.Controls.Add(examSiteDropEdit);
			detailPanel.Controls.Add(originalBillsCalcEdit);
			detailPanel.Controls.Add(serviceLevelDropEdit);
			detailPanel.Controls.Add(deliverTransportFindBox);
			detailPanel.Controls.Add(sendingAgentFindBox);
			detailPanel.Controls.Add(this.USPortOfUnLadingCodeFindBox);
			detailPanel.Controls.Add(deliverContactDropEdit);
			detailPanel.Controls.Add(originFindBox);
			detailPanel.Controls.Add(customsBrokerFindBox);
			detailPanel.Controls.Add(controllingCustomerFindBox);
			detailPanel.Controls.Add(notifyPartyAddressControl);
			detailPanel.Controls.Add(notifyPartyContactDropEdit);
			detailPanel.Controls.Add(receivingAgentFindBox);
			detailPanel.Controls.Add(this.USPortOfLadingCodeFindBox);
			detailPanel.Controls.Add(destinationFindBox);
			detailPanel.Controls.Add(pickupContactDropEdit);
			detailPanel.Controls.Add(pickupAddressControl);
			detailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			detailPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			detailPanel.Name = "detailPanel";
			detailPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 175, true);
			detailPanel.TabIndex = 0;
			// 
			// RelationshipDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(modesAndTracking);
			this.Name = "RelationshipDetailsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 488, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			filterGroupBox.ResumeLayout(false);
			defaultValuesTabPage.ResumeLayout(false);
			pickupAddressControl.ResumeLayout(true);
			pickupAddressControl.PerformLayout();
			deliverAddressControl.ResumeLayout(true);
			deliverAddressControl.PerformLayout();
			notifyPartyAddressControl.ResumeLayout(true);
			notifyPartyAddressControl.PerformLayout();
			notesTabPage.ResumeLayout(false);
			splitContainer2.Panel1.ResumeLayout(false);
			splitContainer2.Panel1.PerformLayout();
			splitContainer2.Panel2.ResumeLayout(false);
			splitContainer2.Panel2.PerformLayout();
			splitContainer2.ResumeLayout(false);
			modesTabPage.ResumeLayout(false);
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.relationshipsGrid)).EndInit();
			relationshipTabControl.ResumeLayout(false);
			documentTrackingTabPage.ResumeLayout(false);
			packTypeTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(packTypeGrid)).EndInit();
			orderLineTolerancesTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(orderLineTolerancesGrid)).EndInit();
			modesAndTracking.ResumeLayout(false);
			detailPanel.ResumeLayout(false);
			detailPanel.PerformLayout();
			this.ResumeLayout(false);

		}
		Enterprise.ZArchitecture.GUI.ZCodeFindBox USPortOfLadingCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox USPortOfUnLadingCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit controlledArrivalLocationDropEdit;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit examSiteDropEdit;
		Enterprise.ZArchitecture.ZGrid relationshipsGrid;
		internal Enterprise.ZArchitecture.GUI.ZTabControl modesAndTracking;
		internal Enterprise.ZArchitecture.GUI.ZTabControl relationshipTabControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl pickupAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl deliverAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl notifyPartyAddressControl;
	}
}
