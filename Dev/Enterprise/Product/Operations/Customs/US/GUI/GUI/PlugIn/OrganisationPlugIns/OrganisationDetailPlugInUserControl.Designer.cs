using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class OrganisationDetailPlugInUserControl
	{

		ZGroupBox fDAGroupBox;
		ZDropEdit producerFirmTypeDropEdit;
		ZGroupBox bondDetailsGroupBox;
		ZArchitecture.ZGrid bondDetailsGrid;
		ZDropEdit submitterFirmTypeDropEdit;
		ZGroupBox reconciliationGroupBox;
		ZCheckBox nAFTAReconIndicatorCheckBox;
		ZDropEdit reconIndicatorDropEdit;
		ZDropEdit mFRRegExemptDropEdit;
		ZCheckBox autoGenerateSDCRCheckBox;
		ZArchitecture.ZTextBox zO_AccountNoTextBox;
		ZDropEdit paymentTypeDropEdit;
		ZDropEdit taxDeferredIndDropEdit;
		ZArchitecture.ZCalcEdit sPDNoOfDaysCalcEdit;
		ZCheckBox fileTheirOwnCheckBox;
		ZDropEdit purchasedDropEdit;
		ZGroupBox eNS7501GroupBox;
		ZCheckBox customAttrib3CheckBox;
		ZCheckBox customAttrib2CheckBox;
		ZCheckBox customAttrib1CheckBox;
		ZCheckBox productCodeCheckBox;
		ZGroupBox miscGroupBox;
		ZGuidFindBox branchGuidFindBox;
		ZDropEdit zO_ReconPaymentTypeDropEdit;
		ZDropEdit zO_ImportSourceDropEdit;
		ZCodeFindBox zO_ReconFilingPortFindBox;
		ZDropEdit payMethodDropEdit;
		ZDropEdit brokerToPayDropEdit;
		ZDropEdit reconBrokerToPayDropEdit;
		ZDropEdit iSEINNumberVerifiedDropEdit;
		ZGroupBox fTZGroupBox;
		ZArchitecture.ZTextBox zoneTextBox;
		ZArchitecture.ZTextBox siteTextBox;
		ZArchitecture.ZTextBox subZoneTextBox;
		ZCodeFindBox locationOfGoodsCodeFindBox;
		ZAddressControl defaultWarehouseAddressControl;
		CargoWise.Windows.UI.KTableLayoutPanel iORBusinessRulesTableLayoutPanel;
		ZGroupBox restrictedSPIsGroupBox;
		ZArchitecture.ZGrid restrictedSPIsGrid;
		ZGroupBox restrictedEntryTypesGroupBox;
		ZArchitecture.ZGrid restrictedEntryTypesGrid;
		ZGroupBox restrictedTariffsGroupBox;
		ZArchitecture.ZGrid restrictedTariffsGrid;
		internal ZTabControl DetailsTabControl;
		ZTabPage details1TabPage;
		internal ZTabPage Details2TabPage;
		CargoWise.Windows.UI.KSplitContainer iORSplitContainer;
		ZArchitecture.ZTextBox notifyPartyRefTextBox;
		ZOrganisationFindBox notifyPartyOrganisationFindBox;
		ZGroupBox iORGroupBox;
		ZDropEdit defDateIndicDropEdit;
		ZDropEdit knownImporterIndicatorDropEdit;
		ZGroupBox fTZAllowsFDAGroupBox;
		ZArchitecture.ZGrid fTZAllowsFDAGrid;
		ZCheckBox zO_DoNotConvertSKUCheckBox;
		ZAddressControl zO_OA_ConsigneeAddressControl;
		ZAddressControl zO_OA_ShipToAddressControl;
		ZPanel bondDetailsGridPanel;
		ZPanel bottomPanel;
		ZGroupBox cBMAGroupox;
		ZArchitecture.ZGrid groupNameGrid;
		ZGroupBox allocationQuantityGroupBox;
		ZArchitecture.ZGrid allocationQuantityGrid;
		ZGroupBox groupNameGroupBox;

		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.bondDetailsGridPanel = new ZPanel();
			this.DetailsTabControl = new ZTabControl();
			this.details1TabPage = new ZTabPage();
			this.iORGroupBox = new ZGroupBox();
			this.iORSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.zO_OA_ConsigneeAddressControl = new ZAddressControl();
			this.zO_OA_ShipToAddressControl = new ZAddressControl();
			this.knownImporterIndicatorDropEdit = new ZDropEdit();
			this.notifyPartyOrganisationFindBox = new ZOrganisationFindBox();
			this.notifyPartyRefTextBox = new ZArchitecture.ZTextBox();
			this.paymentTypeDropEdit = new ZDropEdit();
			this.zO_AccountNoTextBox = new ZArchitecture.ZTextBox();
			this.iSEINNumberVerifiedDropEdit = new ZDropEdit();
			this.taxDeferredIndDropEdit = new ZDropEdit();
			this.brokerToPayDropEdit = new ZDropEdit();
			this.autoGenerateSDCRCheckBox = new ZCheckBox();
			this.payMethodDropEdit = new ZDropEdit();
			this.sPDNoOfDaysCalcEdit = new ZArchitecture.ZCalcEdit();
			this.purchasedDropEdit = new ZDropEdit();
			this.iORBusinessRulesTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.restrictedEntryTypesGroupBox = new ZGroupBox();
			this.restrictedEntryTypesGrid = new ZArchitecture.ZGrid();
			this.restrictedSPIsGroupBox = new ZGroupBox();
			this.restrictedSPIsGrid = new ZArchitecture.ZGrid();
			this.restrictedTariffsGroupBox = new ZGroupBox();
			this.restrictedTariffsGrid = new ZArchitecture.ZGrid();
			this.fTZAllowsFDAGroupBox = new ZGroupBox();
			this.fTZAllowsFDAGrid = new ZArchitecture.ZGrid();
			this.fDAGroupBox = new ZGroupBox();
			this.mFRRegExemptDropEdit = new ZDropEdit();
			this.submitterFirmTypeDropEdit = new ZDropEdit();
			this.producerFirmTypeDropEdit = new ZDropEdit();
			this.bondDetailsGroupBox = new ZGroupBox();
			this.bondDetailsGrid = new ZArchitecture.ZGrid();
			this.Details2TabPage = new ZTabPage();
			this.fTZGroupBox = new ZGroupBox();
			this.locationOfGoodsCodeFindBox = new ZCodeFindBox();
			this.siteTextBox = new ZArchitecture.ZTextBox();
			this.subZoneTextBox = new ZArchitecture.ZTextBox();
			this.zoneTextBox = new ZArchitecture.ZTextBox();
			this.defaultWarehouseAddressControl = new ZAddressControl();
			this.miscGroupBox = new ZGroupBox();
			this.zO_DoNotConvertSKUCheckBox = new ZCheckBox();
			this.defDateIndicDropEdit = new ZDropEdit();
			this.branchGuidFindBox = new ZGuidFindBox();
			this.eNS7501GroupBox = new ZGroupBox();
			this.customAttrib3CheckBox = new ZCheckBox();
			this.customAttrib2CheckBox = new ZCheckBox();
			this.customAttrib1CheckBox = new ZCheckBox();
			this.productCodeCheckBox = new ZCheckBox();
			this.reconciliationGroupBox = new ZGroupBox();
			this.reconBrokerToPayDropEdit = new ZDropEdit();
			this.zO_ReconFilingPortFindBox = new ZCodeFindBox();
			this.zO_ReconPaymentTypeDropEdit = new ZDropEdit();
			this.zO_ImportSourceDropEdit = new ZDropEdit();
			this.fileTheirOwnCheckBox = new ZCheckBox();
			this.reconIndicatorDropEdit = new ZDropEdit();
			this.nAFTAReconIndicatorCheckBox = new ZCheckBox();
			this.bottomPanel = new ZPanel();
			this.cBMAGroupox = new ZGroupBox();
			this.groupNameGrid = new ZArchitecture.ZGrid();
			this.groupNameGroupBox = new ZGroupBox();
			this.allocationQuantityGroupBox = new ZGroupBox();
			this.allocationQuantityGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bondDetailsGridPanel.SuspendLayout();
			this.DetailsTabControl.SuspendLayout();
			this.details1TabPage.SuspendLayout();
			this.iORGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iORSplitContainer)).BeginInit();
			this.iORSplitContainer.Panel1.SuspendLayout();
			this.iORSplitContainer.Panel2.SuspendLayout();
			this.iORSplitContainer.SuspendLayout();
			this.zO_OA_ConsigneeAddressControl.SuspendLayout();
			this.zO_OA_ShipToAddressControl.SuspendLayout();
			this.knownImporterIndicatorDropEdit.SuspendLayout();
			this.notifyPartyOrganisationFindBox.SuspendLayout();
			this.paymentTypeDropEdit.SuspendLayout();
			this.iSEINNumberVerifiedDropEdit.SuspendLayout();
			this.taxDeferredIndDropEdit.SuspendLayout();
			this.brokerToPayDropEdit.SuspendLayout();
			this.payMethodDropEdit.SuspendLayout();
			this.purchasedDropEdit.SuspendLayout();
			this.iORBusinessRulesTableLayoutPanel.SuspendLayout();
			this.restrictedEntryTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedEntryTypesGrid)).BeginInit();
			this.restrictedEntryTypesGrid.SuspendLayout();
			this.restrictedSPIsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedSPIsGrid)).BeginInit();
			this.restrictedSPIsGrid.SuspendLayout();
			this.restrictedTariffsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedTariffsGrid)).BeginInit();
			this.restrictedTariffsGrid.SuspendLayout();
			this.fTZAllowsFDAGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fTZAllowsFDAGrid)).BeginInit();
			this.fTZAllowsFDAGrid.SuspendLayout();
			this.fDAGroupBox.SuspendLayout();
			this.mFRRegExemptDropEdit.SuspendLayout();
			this.submitterFirmTypeDropEdit.SuspendLayout();
			this.producerFirmTypeDropEdit.SuspendLayout();
			this.bondDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bondDetailsGrid)).BeginInit();
			this.bondDetailsGrid.SuspendLayout();
			this.Details2TabPage.SuspendLayout();
			this.fTZGroupBox.SuspendLayout();
			this.locationOfGoodsCodeFindBox.SuspendLayout();
			this.defaultWarehouseAddressControl.SuspendLayout();
			this.miscGroupBox.SuspendLayout();
			this.defDateIndicDropEdit.SuspendLayout();
			this.branchGuidFindBox.SuspendLayout();
			this.eNS7501GroupBox.SuspendLayout();
			this.reconciliationGroupBox.SuspendLayout();
			this.reconBrokerToPayDropEdit.SuspendLayout();
			this.zO_ReconFilingPortFindBox.SuspendLayout();
			this.zO_ReconPaymentTypeDropEdit.SuspendLayout();
			this.zO_ImportSourceDropEdit.SuspendLayout();
			this.reconIndicatorDropEdit.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.cBMAGroupox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.groupNameGrid)).BeginInit();
			this.groupNameGrid.SuspendLayout();
			this.groupNameGroupBox.SuspendLayout();
			this.allocationQuantityGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.allocationQuantityGrid)).BeginInit();
			this.allocationQuantityGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.OrgHeaderWrapper);
			// 
			// BondDetailsGridPanel
			// 
			this.bondDetailsGridPanel.Controls.Add(this.DetailsTabControl);
			this.bondDetailsGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bondDetailsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bondDetailsGridPanel.Name = "BondDetailsGridPanel";
			this.bondDetailsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 609, true);
			this.bondDetailsGridPanel.TabIndex = 0;
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsTabControl.Controls.Add(this.details1TabPage);
			this.DetailsTabControl.Controls.Add(this.Details2TabPage);
			this.DetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsTabControl.Name = "DetailsTabControl";
			this.DetailsTabControl.SelectedIndex = 0;
			this.DetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 609, true);
			this.DetailsTabControl.TabIndex = 10;
			// 
			// Details1TabPage
			// 
			this.details1TabPage.BackColor = System.Drawing.SystemColors.Control;
			this.details1TabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f7fa3bb6-5c1f-4ebf-a71e-423a3b79cb94", "Bond, IOR, FDA");
			this.details1TabPage.Controls.Add(this.iORGroupBox);
			this.details1TabPage.Controls.Add(this.fDAGroupBox);
			this.details1TabPage.Controls.Add(this.bondDetailsGroupBox);
			this.details1TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.details1TabPage.Name = "Details1TabPage";
			this.details1TabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.details1TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 582, true);
			this.details1TabPage.TabIndex = 0;
			// 
			// IORGroupBox
			// 
			this.iORGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.iORGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7e6d92c1-4c28-4ecc-bcd0-1d98df8d8d93", "Importer Of Record");
			this.iORGroupBox.Controls.Add(this.iORSplitContainer);
			this.iORGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.iORGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 156, true);
			this.iORGroupBox.Name = "IORGroupBox";
			this.iORGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 423, true);
			this.iORGroupBox.TabIndex = 3;
			this.iORGroupBox.TabStop = false;
			// 
			// IORSplitContainer
			// 
			this.iORSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.iORSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.iORSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.iORSplitContainer.Name = "IORSplitContainer";
			this.iORSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// IORSplitContainer.Panel1
			// 
			this.iORSplitContainer.Panel1.Controls.Add(this.zO_OA_ConsigneeAddressControl);
			this.iORSplitContainer.Panel1.Controls.Add(this.zO_OA_ShipToAddressControl);
			this.iORSplitContainer.Panel1.Controls.Add(this.knownImporterIndicatorDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.notifyPartyOrganisationFindBox);
			this.iORSplitContainer.Panel1.Controls.Add(this.notifyPartyRefTextBox);
			this.iORSplitContainer.Panel1.Controls.Add(this.paymentTypeDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.zO_AccountNoTextBox);
			this.iORSplitContainer.Panel1.Controls.Add(this.iSEINNumberVerifiedDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.taxDeferredIndDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.brokerToPayDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.autoGenerateSDCRCheckBox);
			this.iORSplitContainer.Panel1.Controls.Add(this.payMethodDropEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.sPDNoOfDaysCalcEdit);
			this.iORSplitContainer.Panel1.Controls.Add(this.purchasedDropEdit);
			// 
			// IORSplitContainer.Panel2
			// 
			this.iORSplitContainer.Panel2.Controls.Add(this.iORBusinessRulesTableLayoutPanel);
			this.iORSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 404, true);
			this.iORSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(125);
			this.iORSplitContainer.TabIndex = 16;
			// 
			// RelatedOrganisationFindBox
			// 
			this.zO_OA_ConsigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zO_OA_ConsigneeAddressControl, "ZO_OA_ConsigneeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.OrgHeaderWrapper)(null)).ZO_OA_ConsigneeAddress)));
			this.zO_OA_ConsigneeAddressControl.BindToOrgList = "ImportAddInfoLookups+Consignees";
			this.zO_OA_ConsigneeAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3A3E32AB-B2CB-4582-9C33-A63C3104AF1E", "Consignee");
			this.zO_OA_ConsigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 95, true);
			this.zO_OA_ConsigneeAddressControl.Name = "ZO_OA_ConsigneeAddress";
			this.zO_OA_ConsigneeAddressControl.ReadOnly = false;
			this.zO_OA_ConsigneeAddressControl.ShowAddress = false;
			this.zO_OA_ConsigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.zO_OA_ConsigneeAddressControl.TabIndex = 17;
			// 
			// ShipToOrganisationFindBox
			// 
			this.zO_OA_ShipToAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zO_OA_ShipToAddressControl, "ZO_OA_ShipToAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.OrgHeaderWrapper)(null)).ZO_OA_ShipToAddress)));
			this.zO_OA_ShipToAddressControl.BindToOrgList = "ImportAddInfoLookups+Consignees";
			this.zO_OA_ShipToAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("A06BFE7C-46A9-443B-8B0E-05D60E942286", "Ship To");
			this.zO_OA_ShipToAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 95, true);
			this.zO_OA_ShipToAddressControl.Name = "ZO_OA_ShipToAddress";
			this.zO_OA_ShipToAddressControl.ReadOnly = false;
			this.zO_OA_ShipToAddressControl.ShowAddress = false;
			this.zO_OA_ShipToAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.zO_OA_ShipToAddressControl.TabIndex = 18;
			// 
			// KnownImporterIndicatorDropEdit
			// 
			this.knownImporterIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.knownImporterIndicatorDropEdit, "ZO_KnwImpInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_KnwImpInd)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).ImportAddInfoLookups.ZO_YesNoList)));
			this.knownImporterIndicatorDropEdit.BindToList = "ImportAddInfoLookups+ZO_YesNoList";
			this.knownImporterIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e243d8d0-d547-4a3b-894b-437a023c6d6a", "Known Importer?");
			this.knownImporterIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 73, true);
			this.knownImporterIndicatorDropEdit.Name = "KnownImporterIndicatorDropEdit";
			this.knownImporterIndicatorDropEdit.PreBoundMaxLength = 1;
			this.knownImporterIndicatorDropEdit.ShowDescriptionBox = false;
			this.knownImporterIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.knownImporterIndicatorDropEdit.TabIndex = 16;
			// 
			// NotifyPartyOrganisationFindBox
			// 
			this.notifyPartyOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.notifyPartyOrganisationFindBox, "ZO_OH_NP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.OrgHeaderWrapper)(null)).ZO_OH_NP)));
			this.notifyPartyOrganisationFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("fbc88300-5b32-41f3-8993-daff2ee1464e", "4811 Party", "CBPF 4811 Notify Party", "");
			this.notifyPartyOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 50, true);
			this.notifyPartyOrganisationFindBox.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.notifyPartyOrganisationFindBox.Name = "NotifyPartyOrganisationFindBox";
			this.notifyPartyOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.notifyPartyOrganisationFindBox.TabIndex = 14;
			// 
			// NotifyPartyRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.notifyPartyRefTextBox, "ZO_NPID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_NPID)));
			this.notifyPartyRefTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("11407c80-f7f2-4eac-8465-20cbc19c758b", "4811 Party ID", "CPBF 4811 Notify Party ID", "");
			this.notifyPartyRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 73, true);
			this.notifyPartyRefTextBox.Name = "NotifyPartyRefTextBox";
			this.notifyPartyRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.notifyPartyRefTextBox.TabIndex = 15;
			// 
			// PaymentTypeDropEdit
			// 
			this.paymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.paymentTypeDropEdit, "ZO_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_PaymentType)));
			this.paymentTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("69c97231-a513-4223-9a1c-6681674af4df", "Payment Type");
			this.paymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 3, true);
			this.paymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.paymentTypeDropEdit.PreBoundMaxLength = 1;
			this.paymentTypeDropEdit.ShowDescriptionBox = false;
			this.paymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.paymentTypeDropEdit.TabIndex = 1;
			// 
			// ZO_AccountNoTextBox
			// 
			this.zO_AccountNoTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zO_AccountNoTextBox, "ZO_AccountNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_AccountNo)));
			this.zO_AccountNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f59774a7-1e23-416b-88f2-a168d90f3e3c", "Payer\'s Unit No");
			this.zO_AccountNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 3, true);
			this.zO_AccountNoTextBox.Name = "ZO_AccountNoTextBox";
			this.zO_AccountNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.zO_AccountNoTextBox.TabIndex = 5;
			// 
			// ISEINNumberVerifiedDropEdit
			// 
			this.iSEINNumberVerifiedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.iSEINNumberVerifiedDropEdit, "ZO_IsEINNumberVerifiedIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_IsEINNumberVerifiedIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).ImportAddInfoLookups.ZO_YesNoList)));
			this.iSEINNumberVerifiedDropEdit.BindToList = "ImportAddInfoLookups+ZO_YesNoList";
			this.iSEINNumberVerifiedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("73c01f3f-7be0-47df-8707-6d5a38e64af6", "EIN/CBP Number Verified?");
			this.iSEINNumberVerifiedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 28, true);
			this.iSEINNumberVerifiedDropEdit.Name = "ISEINNumberVerifiedDropEdit";
			this.iSEINNumberVerifiedDropEdit.PreBoundMaxLength = 1;
			this.iSEINNumberVerifiedDropEdit.ShowDescriptionBox = false;
			this.iSEINNumberVerifiedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.iSEINNumberVerifiedDropEdit.TabIndex = 9;
			// 
			// TaxDeferredIndDropEdit
			// 
			this.taxDeferredIndDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.taxDeferredIndDropEdit, "ZO_TaxDeferredInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_TaxDeferredInd)));
			this.taxDeferredIndDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f315f38d-6865-4668-84e3-ad81288776be", "Tax Deferred Ind.");
			this.taxDeferredIndDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 29, true);
			this.taxDeferredIndDropEdit.Name = "TaxDeferredIndDropEdit";
			this.taxDeferredIndDropEdit.PreBoundMaxLength = 1;
			this.taxDeferredIndDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.taxDeferredIndDropEdit.TabIndex = 11;
			// 
			// BrokerToPayDropEdit
			// 
			this.brokerToPayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.brokerToPayDropEdit, "ZO_BrokerToPay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_BrokerToPay)));
			this.brokerToPayDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("59b3abcb-bb79-4791-ab4c-c5d4b47c170f", "Broker To Pay");
			this.brokerToPayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 3, true);
			this.brokerToPayDropEdit.Name = "BrokerToPayDropEdit";
			this.brokerToPayDropEdit.PreBoundMaxLength = 1;
			this.brokerToPayDropEdit.ShowDescriptionBox = false;
			this.brokerToPayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.brokerToPayDropEdit.TabIndex = 7;
			// 
			// AutoGenerateSDCRCheckBox
			// 
			this.autoGenerateSDCRCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.autoGenerateSDCRCheckBox, "ZO_DoNotAutoGenerateSDCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_DoNotAutoGenerateSDCR)));
			this.autoGenerateSDCRCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f17365d1-ccbc-42e9-a6d5-92c560235052", "Do NOT Auto Generate Statement Date Change Request");
			this.autoGenerateSDCRCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.autoGenerateSDCRCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoGenerateSDCRCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 76, true);
			this.autoGenerateSDCRCheckBox.Name = "AutoGenerateSDCRCheckBox";
			this.autoGenerateSDCRCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.autoGenerateSDCRCheckBox.TabIndex = 13;
			this.autoGenerateSDCRCheckBox.UseVisualStyleBackColor = true;
			// 
			// PayMethodDropEdit
			// 
			this.payMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.payMethodDropEdit, "ZO_PayMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_PayMethod)));
			this.payMethodDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3da0859c-6685-4401-9a5e-7f882e7aa30c", "Pay Method");
			this.payMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 3, true);
			this.payMethodDropEdit.Name = "PayMethodDropEdit";
			this.payMethodDropEdit.PreBoundMaxLength = 3;
			this.payMethodDropEdit.ShowDescriptionBox = false;
			this.payMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.payMethodDropEdit.TabIndex = 3;
			// 
			// SPDNoOfDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.sPDNoOfDaysCalcEdit, "ZO_SPDNumberOfDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.OrgHeaderWrapper)(null)).ZO_SPDNumberOfDays)));
			this.sPDNoOfDaysCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6e91076b-b494-480c-86c4-e94fbcc7bed8", "Statement Print Date working days to be added (override registry)");
			this.sPDNoOfDaysCalcEdit.DecimalPlaces = 0;
			this.sPDNoOfDaysCalcEdit.Decimals = 0;
			this.sPDNoOfDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 50, true);
			this.sPDNoOfDaysCalcEdit.Name = "SPDNoOfDaysCalcEdit";
			this.sPDNoOfDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 20, true);
			this.sPDNoOfDaysCalcEdit.TabIndex = 12;
			this.sPDNoOfDaysCalcEdit.Text = "0";
			this.sPDNoOfDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PurchasedDropEdit
			// 
			this.purchasedDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.purchasedDropEdit, "ZO_Purchased");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_Purchased)));
			this.purchasedDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f4c2b15c-f65f-4264-a7e7-252dd030a4eb", "Purchased");
			this.purchasedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 28, true);
			this.purchasedDropEdit.Name = "PurchasedDropEdit";
			this.purchasedDropEdit.PreBoundMaxLength = 1;
			this.purchasedDropEdit.ShowDescriptionBox = false;
			this.purchasedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.purchasedDropEdit.TabIndex = 10;
			// 
			// IORBusinessRulesTableLayoutPanel
			// 
			this.iORBusinessRulesTableLayoutPanel.ColumnCount = 4;
			this.iORBusinessRulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.iORBusinessRulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.iORBusinessRulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.iORBusinessRulesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
			this.iORBusinessRulesTableLayoutPanel.Controls.Add(this.restrictedEntryTypesGroupBox, 1, 0);
			this.iORBusinessRulesTableLayoutPanel.Controls.Add(this.restrictedSPIsGroupBox, 0, 0);
			this.iORBusinessRulesTableLayoutPanel.Controls.Add(this.restrictedTariffsGroupBox, 2, 0);
			this.iORBusinessRulesTableLayoutPanel.Controls.Add(this.fTZAllowsFDAGroupBox, 3, 0);
			this.iORBusinessRulesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.iORBusinessRulesTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.iORBusinessRulesTableLayoutPanel.Name = "IORBusinessRulesTableLayoutPanel";
			this.iORBusinessRulesTableLayoutPanel.RowCount = 1;
			this.iORBusinessRulesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.iORBusinessRulesTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 275, true);
			this.iORBusinessRulesTableLayoutPanel.TabIndex = 8;
			// 
			// RestrictedEntryTypesGroupBox
			// 
			this.restrictedEntryTypesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c792ea53-d48b-4a3d-8762-d6a77caf9cbc", "Restricted Entry Types");
			this.restrictedEntryTypesGroupBox.Controls.Add(this.restrictedEntryTypesGrid);
			this.restrictedEntryTypesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedEntryTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 3, true);
			this.restrictedEntryTypesGroupBox.Name = "RestrictedEntryTypesGroupBox";
			this.restrictedEntryTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 269, true);
			this.restrictedEntryTypesGroupBox.TabIndex = 1;
			this.restrictedEntryTypesGroupBox.TabStop = false;
			// 
			// RestrictedEntryTypesGrid
			// 
			this.restrictedEntryTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.restrictedEntryTypesGrid, "RestrictedEntryTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedEntryTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.RestrictedCode)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedEntryTypes)).SyncRoot)).CY_Data)));
			this.restrictedEntryTypesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("32025c78-0dcf-4101-ac12-02a84b560225", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.restrictedEntryTypesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.restrictedEntryTypesGrid.CopySelectedRowsAllowed = true;
			this.restrictedEntryTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedEntryTypesGrid.GridId = "194d48c4-7377-4392-8923-3931a1cf7efa";
			this.restrictedEntryTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.restrictedEntryTypesGrid.LayoutKey = "RestrictedEntryTypesGrid";
			this.restrictedEntryTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.restrictedEntryTypesGrid.Name = "RestrictedEntryTypesGrid";
			this.restrictedEntryTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 250, true);
			this.restrictedEntryTypesGrid.TabIndex = 0;
			// 
			// RestrictedSPIsGroupBox
			// 
			this.restrictedSPIsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9536365a-8bf9-461c-a077-d5dec7301135", "Restricted SPIs");
			this.restrictedSPIsGroupBox.Controls.Add(this.restrictedSPIsGrid);
			this.restrictedSPIsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedSPIsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.restrictedSPIsGroupBox.Name = "RestrictedSPIsGroupBox";
			this.restrictedSPIsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 269, true);
			this.restrictedSPIsGroupBox.TabIndex = 0;
			this.restrictedSPIsGroupBox.TabStop = false;
			// 
			// RestrictedSPIsGrid
			// 
			this.restrictedSPIsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.restrictedSPIsGrid, "RestrictedSPIs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedSPIs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.RestrictedCode)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedSPIs)).SyncRoot)).CY_Data)));
			this.restrictedSPIsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a10a2945-c4ce-46d9-ab79-ccd0539d536a", "Code");
			zDropEditColumnStyleInfo2.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.restrictedSPIsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.restrictedSPIsGrid.CopySelectedRowsAllowed = true;
			this.restrictedSPIsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedSPIsGrid.GridId = "81c08c8f-5f7c-4df1-a564-327f168bc08e";
			this.restrictedSPIsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.restrictedSPIsGrid.LayoutKey = "RestrictedSPIsGrid";
			this.restrictedSPIsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.restrictedSPIsGrid.Name = "RestrictedSPIsGrid";
			this.restrictedSPIsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 250, true);
			this.restrictedSPIsGrid.TabIndex = 0;
			// 
			// RestrictedTariffsGroupBox
			// 
			this.restrictedTariffsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("555906f0-a87f-4b88-86bb-d5e30dc81e0c", "Restricted Tariffs");
			this.restrictedTariffsGroupBox.Controls.Add(this.restrictedTariffsGrid);
			this.restrictedTariffsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedTariffsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 3, true);
			this.restrictedTariffsGroupBox.Name = "RestrictedTariffsGroupBox";
			this.restrictedTariffsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 269, true);
			this.restrictedTariffsGroupBox.TabIndex = 2;
			this.restrictedTariffsGroupBox.TabStop = false;
			// 
			// RestrictedTariffsGrid
			// 
			this.restrictedTariffsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.restrictedTariffsGrid, "RestrictedTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedTariffs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.RestrictedCode)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).RestrictedTariffs)).SyncRoot)).CY_Data)));
			this.restrictedTariffsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d68e5eca-3b5c-454c-a940-fd7d953394d8", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.restrictedTariffsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.restrictedTariffsGrid.CopySelectedRowsAllowed = true;
			this.restrictedTariffsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.restrictedTariffsGrid.GridId = "4a1e1bce-4c78-42bb-8d7d-ed55d30b180b";
			this.restrictedTariffsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.restrictedTariffsGrid.LayoutKey = "RestrictedTariffsGrid";
			this.restrictedTariffsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.restrictedTariffsGrid.Name = "RestrictedTariffsGrid";
			this.restrictedTariffsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 250, true);
			this.restrictedTariffsGrid.TabIndex = 0;
			// 
			// FTZAllowsFDAGroupBox
			// 
			this.fTZAllowsFDAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2f065edd-b751-4274-b14c-135d04d51cb3", "FTZ Allows FDA");
			this.fTZAllowsFDAGroupBox.Controls.Add(this.fTZAllowsFDAGrid);
			this.fTZAllowsFDAGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fTZAllowsFDAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(723, 3, true);
			this.fTZAllowsFDAGroupBox.Name = "FTZAllowsFDAGroupBox";
			this.fTZAllowsFDAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 269, true);
			this.fTZAllowsFDAGroupBox.TabIndex = 3;
			this.fTZAllowsFDAGroupBox.TabStop = false;
			// 
			// FTZAllowsFDAGrid
			// 
			this.fTZAllowsFDAGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.fTZAllowsFDAGrid, "FTZAllowsFDAs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).FTZAllowsFDAs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.RestrictedCode)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).FTZAllowsFDAs)).SyncRoot)).CY_Data)));
			this.fTZAllowsFDAGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1cd76402-ab2f-4389-bdaa-48b246565516", "Zone ID");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.MaxLengthOverride = 7;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.fTZAllowsFDAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.fTZAllowsFDAGrid.CopySelectedRowsAllowed = true;
			this.fTZAllowsFDAGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fTZAllowsFDAGrid.GridId = "4a1e1bce-4c78-42bb-8d7d-ed55d30b180b";
			this.fTZAllowsFDAGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.fTZAllowsFDAGrid.LayoutKey = "RestrictedTariffsGrid";
			this.fTZAllowsFDAGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.fTZAllowsFDAGrid.Name = "FTZAllowsFDAGrid";
			this.fTZAllowsFDAGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 250, true);
			this.fTZAllowsFDAGrid.TabIndex = 0;
			// 
			// FDAGroupBox
			// 
			this.fDAGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.fDAGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2f9e03a2-621c-48c8-b0a5-bb99bebe7879", "FDA");
			this.fDAGroupBox.Controls.Add(this.mFRRegExemptDropEdit);
			this.fDAGroupBox.Controls.Add(this.submitterFirmTypeDropEdit);
			this.fDAGroupBox.Controls.Add(this.producerFirmTypeDropEdit);
			this.fDAGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.fDAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 116, true);
			this.fDAGroupBox.Name = "FDAGroupBox";
			this.fDAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 40, true);
			this.fDAGroupBox.TabIndex = 1;
			this.fDAGroupBox.TabStop = false;
			// 
			// MFRRegExemptDropEdit
			// 
			this.mFRRegExemptDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mFRRegExemptDropEdit, "ZO_MFRRegExempt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_MFRRegExempt)));
			this.mFRRegExemptDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ace58d8c-52c5-4ff5-a0f6-3a6e56b52577", "Food Facility Reg. Exempt.");
			this.mFRRegExemptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 13, true);
			this.mFRRegExemptDropEdit.Name = "MFRRegExemptDropEdit";
			this.mFRRegExemptDropEdit.PreBoundMaxLength = 1;
			this.mFRRegExemptDropEdit.ShowDescriptionBox = false;
			this.mFRRegExemptDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.mFRRegExemptDropEdit.TabIndex = 2;
			// 
			// SubmitterFirmTypeDropEdit
			// 
			this.submitterFirmTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.submitterFirmTypeDropEdit, "ZO_SubmitterFirmType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_SubmitterFirmType)));
			this.submitterFirmTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5516f6a4-3e9c-4735-95d1-c6763ac74e3c", "Submitter Firm Type");
			this.submitterFirmTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 13, true);
			this.submitterFirmTypeDropEdit.Name = "SubmitterFirmTypeDropEdit";
			this.submitterFirmTypeDropEdit.PreBoundMaxLength = 1;
			this.submitterFirmTypeDropEdit.ShowDescriptionBox = false;
			this.submitterFirmTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.submitterFirmTypeDropEdit.TabIndex = 1;
			// 
			// ProducerFirmTypeDropEdit
			// 
			this.producerFirmTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.producerFirmTypeDropEdit, "ZO_ProducerFirmType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_ProducerFirmType)));
			this.producerFirmTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("18ed0f9c-e7ed-4975-ab12-c4b350132e6e", "Producer Firm Type");
			this.producerFirmTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 13, true);
			this.producerFirmTypeDropEdit.Name = "ProducerFirmTypeDropEdit";
			this.producerFirmTypeDropEdit.PreBoundMaxLength = 1;
			this.producerFirmTypeDropEdit.ShowDescriptionBox = false;
			this.producerFirmTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.producerFirmTypeDropEdit.TabIndex = 0;
			// 
			// BondDetailsGroupBox
			// 
			this.bondDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c2151cee-4e00-4512-80da-6617c8da7679", "Bond Details");
			this.bondDetailsGroupBox.Controls.Add(this.bondDetailsGrid);
			this.bondDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.bondDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.bondDetailsGroupBox.Name = "BondDetailsGroupBox";
			this.bondDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 113, true);
			this.bondDetailsGroupBox.TabIndex = 0;
			this.bondDetailsGroupBox.TabStop = false;
			// 
			// BondDetailsGrid
			// 
			this.bondDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.bondDetailsGrid, "BondDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_ActivityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).Lookups.ActivityCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).Lookups.BondTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_SuretyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondEffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondFiledPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).Lookups.RegionPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_BondExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusBondDetail)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).BondDetails)).SyncRoot)).PW_StatusDesc)));
			this.bondDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.BindToList = "Lookups+ActivityCodeList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("463a9218-253d-4a39-82ff-2697ac9b7bd1", "Activity Code");
			zDropEditColumnStyleInfo3.ColumnName = "PW_ActivityCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "Lookups+BondTypeList";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1d72a2a4-0e20-4a11-8d11-988e4021e3ea", "Bond Type");
			zDropEditColumnStyleInfo4.ColumnName = "PW_BondType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("39fd0e04-a358-45e3-bcdf-0c48f3aa76bf", "Acct. Number");
			zTextBoxColumnStyleInfo3.ColumnName = "PW_BondNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("42121a23-e9db-4751-b192-61734e2d3caa", "Surety Code");
			zTextBoxColumnStyleInfo4.ColumnName = "PW_SuretyCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4cd73994-cd16-488a-9ab3-94b2bd03233b", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "PW_BondAmount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ebba0512-3a33-483a-a1b7-6df9146d9f40", "Bond Effective Date");
			zDateEditColumnStyleInfo1.ColumnName = "PW_BondEffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+RegionPorts";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2bfb04ec-a819-4558-b606-4d4bdd3383e5", "Bond Filed Port");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PW_BondFiledPort";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5b869f8c-52d6-4457-be10-0c1185846942", "Bond Expiry Date");
			zDateEditColumnStyleInfo2.ColumnName = "PW_BondExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0ea1b936-6f42-4e15-9b6a-45eda4ac06e1", "Status");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "PW_StatusDesc";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.bondDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.bondDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.bondDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.bondDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.bondDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.bondDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.bondDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.bondDetailsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.bondDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.bondDetailsGrid.CopySelectedRowsAllowed = true;
			this.bondDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bondDetailsGrid.GridId = "2fc13cb7-52c9-4021-a5dd-ad69c8dd90b3";
			this.bondDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.bondDetailsGrid.LayoutKey = "BondDetailsGrid";
			this.bondDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.bondDetailsGrid.Name = "BondDetailsGrid";
			this.bondDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 94, true);
			this.bondDetailsGrid.TabIndex = 0;
			// 
			// Details2TabPage
			// 
			this.Details2TabPage.BackColor = System.Drawing.SystemColors.Control;
			this.Details2TabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2a73232f-63d2-466e-b652-f6ad461f4726", "Recon, 7501, FTZ/Whs, Misc");
			this.Details2TabPage.Controls.Add(this.bottomPanel);
			this.Details2TabPage.Controls.Add(this.miscGroupBox);
			this.Details2TabPage.Controls.Add(this.eNS7501GroupBox);
			this.Details2TabPage.Controls.Add(this.reconciliationGroupBox);
			this.Details2TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Details2TabPage.Name = "Details2TabPage";
			this.Details2TabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Details2TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(972, 582, true);
			this.Details2TabPage.TabIndex = 1;
			// 
			// BottomPanel
			// 
			this.bottomPanel.Controls.Add(this.cBMAGroupox);
			this.bottomPanel.Controls.Add(this.fTZGroupBox);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 168, true);
			this.bottomPanel.Name = "BottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 411, true);
			this.bottomPanel.TabIndex = 8;
			// 
			// FTZGroupBox
			// 
			this.fTZGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("55f30b89-50f6-4a3a-993f-00840e79fc81", "FTZ/Whs");
			this.fTZGroupBox.Controls.Add(this.locationOfGoodsCodeFindBox);
			this.fTZGroupBox.Controls.Add(this.siteTextBox);
			this.fTZGroupBox.Controls.Add(this.subZoneTextBox);
			this.fTZGroupBox.Controls.Add(this.zoneTextBox);
			this.fTZGroupBox.Controls.Add(this.defaultWarehouseAddressControl);
			this.fTZGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.fTZGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.fTZGroupBox.Name = "FTZGroupBox";
			this.fTZGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 68, true);
			this.fTZGroupBox.TabIndex = 7;
			this.fTZGroupBox.TabStop = false;
			// 
			// LocationOfGoodsCodeFindBox
			// 
			this.locationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.locationOfGoodsCodeFindBox, "ZO_FIRMS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_FIRMS)));
			this.locationOfGoodsCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f063d2ac-f9f6-4cde-b64e-3541b97ba070", "FIRMS Code");
			this.locationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 13, true);
			this.locationOfGoodsCodeFindBox.Name = "LocationOfGoodsCodeFindBox";
			this.locationOfGoodsCodeFindBox.PopupCaption = null;
			this.locationOfGoodsCodeFindBox.PreBoundMaxLength = 4;
			this.locationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.locationOfGoodsCodeFindBox.TabIndex = 7;
			// 
			// SiteTextBox
			// 
			this.siteTextBox.AcceptsReturn = true;
			this.siteTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.siteTextBox, "ZO_Site");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_Site)));
			this.siteTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a471d70e-61bd-4aa8-a310-8c17ac72d7ac", "Site");
			this.siteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(631, 13, true);
			this.siteTextBox.Name = "SiteTextBox";
			this.siteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 20, true);
			this.siteTextBox.TabIndex = 10;
			// 
			// SubZoneTextBox
			// 
			this.subZoneTextBox.AcceptsReturn = true;
			this.subZoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.subZoneTextBox, "ZO_SubZone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_SubZone)));
			this.subZoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("edaa2cd2-8e66-4bc4-98af-dd7f1af74b1d", "Sub Zone");
			this.subZoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 13, true);
			this.subZoneTextBox.Name = "SubZoneTextBox";
			this.subZoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.subZoneTextBox.TabIndex = 9;
			// 
			// ZoneTextBox
			// 
			this.zoneTextBox.AcceptsReturn = true;
			this.zoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zoneTextBox, "ZO_ZoneID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.OrgHeaderWrapper)(null)).ZO_ZoneID)));
			this.zoneTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("16eeed72-a527-4565-b7ac-5428eceb0446", "Zone ID");
			this.zoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 13, true);
			this.zoneTextBox.Name = "ZoneTextBox";
			this.zoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.zoneTextBox.TabIndex = 8;
			// 
			// DefaultWarehouseAddressControl
			// 
			this.defaultWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defaultWarehouseAddressControl, "OV_OA_WarehouseAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.OrgHeaderWrapper)(null)).OV_OA_WarehouseAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZAddress)(((Business.OrgHeaderWrapper)(null)).OV_OA_WarehouseAddress_ZAddress)));
			this.defaultWarehouseAddressControl.BindToOrgList = "ImportLookups+BondedWarehouseList";
			this.defaultWarehouseAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c445da91-c1e3-4917-a96f-f1f619689385", "FTZ / Warehouse Default");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.defaultWarehouseAddressControl, true);
			this.defaultWarehouseAddressControl.Name = "DefaultWarehouseAddressControl";
			this.defaultWarehouseAddressControl.PopupCaption = "";
			this.defaultWarehouseAddressControl.ReadOnly = false;
			this.defaultWarehouseAddressControl.ShowAddress = false;
			this.defaultWarehouseAddressControl.StackControls = false;
			this.defaultWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.defaultWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 38, true);
			this.defaultWarehouseAddressControl.TabIndex = 11;
			// 
			// MiscGroupBox
			// 
			this.miscGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d0c5bc3e-bffb-4ed0-aa11-6ff8efb69899", "Misc");
			this.miscGroupBox.Controls.Add(this.zO_DoNotConvertSKUCheckBox);
			this.miscGroupBox.Controls.Add(this.defDateIndicDropEdit);
			this.miscGroupBox.Controls.Add(this.branchGuidFindBox);
			this.miscGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.miscGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 99, true);
			this.miscGroupBox.Name = "MiscGroupBox";
			this.miscGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 69, true);
			this.miscGroupBox.TabIndex = 5;
			this.miscGroupBox.TabStop = false;
			// 
			// ZO_DoNotConvertSKUCheckBox
			// 
			this.zO_DoNotConvertSKUCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zO_DoNotConvertSKUCheckBox, "ZO_DoNotConvertSKU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_DoNotConvertSKU)));
			this.zO_DoNotConvertSKUCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("11f9f604-ac1b-4114-80a9-532e70121587", "Retain Product SKU in Invoice UQ (IMP jobs only)", "When Stock Keeping Unit is defaulted to Invoice UQ, do not convert it to Customs UQ");
			this.zO_DoNotConvertSKUCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.zO_DoNotConvertSKUCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zO_DoNotConvertSKUCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(655, 15, true);
			this.zO_DoNotConvertSKUCheckBox.Name = "ZO_DoNotConvertSKUCheckBox";
			this.zO_DoNotConvertSKUCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.zO_DoNotConvertSKUCheckBox.TabIndex = 3;
			this.zO_DoNotConvertSKUCheckBox.UseVisualStyleBackColor = true;
			// 
			// DefDateIndicDropEdit
			// 
			this.defDateIndicDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.defDateIndicDropEdit, "ZO_DefTaxDateCalcOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_DefTaxDateCalcOption)));
			this.defDateIndicDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e0f7a1b2-a731-48e2-9ec3-8e04427c653d", "Deferred Tax Due Date Calc. Option");
			this.defDateIndicDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 38, true);
			this.defDateIndicDropEdit.Name = "DefDateIndicDropEdit";
			this.defDateIndicDropEdit.PreBoundMaxLength = 2;
			this.defDateIndicDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.defDateIndicDropEdit.TabIndex = 2;
			// 
			// BranchGuidFindBox
			// 
			this.branchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.branchGuidFindBox, "ZO_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.OrgHeaderWrapper)(null)).ZO_GB)));
			this.branchGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f62814ba-616a-4bcd-9fe5-c5d8a105c5e6", "BIRD Default Branch");
			this.branchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 12, true);
			this.branchGuidFindBox.Name = "BranchGuidFindBox";
			this.branchGuidFindBox.PreBoundMaxLength = 3;
			this.branchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.branchGuidFindBox.TabIndex = 0;
			// 
			// ENS7501GroupBox
			// 
			this.eNS7501GroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("941f35d4-236b-42a1-aff0-462bbe63b646", "7501 Printing");
			this.eNS7501GroupBox.Controls.Add(this.customAttrib3CheckBox);
			this.eNS7501GroupBox.Controls.Add(this.customAttrib2CheckBox);
			this.eNS7501GroupBox.Controls.Add(this.customAttrib1CheckBox);
			this.eNS7501GroupBox.Controls.Add(this.productCodeCheckBox);
			this.eNS7501GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.eNS7501GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.eNS7501GroupBox.Name = "ENS7501GroupBox";
			this.eNS7501GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 36, true);
			this.eNS7501GroupBox.TabIndex = 4;
			this.eNS7501GroupBox.TabStop = false;
			// 
			// CustomAttrib3CheckBox
			// 
			this.customAttrib3CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.customAttrib3CheckBox, "ZO_ENSPrintCustomAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_ENSPrintCustomAttrib3)));
			this.customAttrib3CheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e29e0b6c-828b-4d70-b686-702bcfa7e63d", "Custom Attribute 3");
			this.customAttrib3CheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.customAttrib3CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.customAttrib3CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(654, 16, true);
			this.customAttrib3CheckBox.Name = "CustomAttrib3CheckBox";
			this.customAttrib3CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.customAttrib3CheckBox.TabIndex = 3;
			this.customAttrib3CheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomAttrib2CheckBox
			// 
			this.customAttrib2CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.customAttrib2CheckBox, "ZO_ENSPrintCustomAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_ENSPrintCustomAttrib2)));
			this.customAttrib2CheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("42edf686-66c3-4a2a-8306-8cabfc6cbe81", "Custom Attribute 2");
			this.customAttrib2CheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.customAttrib2CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.customAttrib2CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(463, 16, true);
			this.customAttrib2CheckBox.Name = "CustomAttrib2CheckBox";
			this.customAttrib2CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.customAttrib2CheckBox.TabIndex = 2;
			this.customAttrib2CheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomAttrib1CheckBox
			// 
			this.customAttrib1CheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.customAttrib1CheckBox, "ZO_ENSPrintCustomAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_ENSPrintCustomAttrib1)));
			this.customAttrib1CheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("00855306-a626-4c53-ab7c-75634a567778", "Custom Attribute 1");
			this.customAttrib1CheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.customAttrib1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.customAttrib1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 16, true);
			this.customAttrib1CheckBox.Name = "CustomAttrib1CheckBox";
			this.customAttrib1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.customAttrib1CheckBox.TabIndex = 1;
			this.customAttrib1CheckBox.UseVisualStyleBackColor = true;
			// 
			// ProductCodeCheckBox
			// 
			this.productCodeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.productCodeCheckBox, "ZO_ENSPrintProduct");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_ENSPrintProduct)));
			this.productCodeCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("bd9dc537-3c13-413b-b661-72009cccb244", "Product Code");
			this.productCodeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.productCodeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.productCodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 16, true);
			this.productCodeCheckBox.Name = "ProductCodeCheckBox";
			this.productCodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.productCodeCheckBox.TabIndex = 0;
			this.productCodeCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReconciliationGroupBox
			// 
			this.reconciliationGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e8a7f605-84bc-4ca3-9585-b65f6a3c0e73", "Reconciliation");
			this.reconciliationGroupBox.Controls.Add(this.reconBrokerToPayDropEdit);
			this.reconciliationGroupBox.Controls.Add(this.zO_ReconFilingPortFindBox);
			this.reconciliationGroupBox.Controls.Add(this.zO_ReconPaymentTypeDropEdit);
			this.reconciliationGroupBox.Controls.Add(this.zO_ImportSourceDropEdit);
			this.reconciliationGroupBox.Controls.Add(this.fileTheirOwnCheckBox);
			this.reconciliationGroupBox.Controls.Add(this.reconIndicatorDropEdit);
			this.reconciliationGroupBox.Controls.Add(this.nAFTAReconIndicatorCheckBox);
			this.reconciliationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.reconciliationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.reconciliationGroupBox.Name = "ReconciliationGroupBox";
			this.reconciliationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 60, true);
			this.reconciliationGroupBox.TabIndex = 2;
			this.reconciliationGroupBox.TabStop = false;
			// 
			// ReconBrokerToPayDropEdit
			// 
			this.reconBrokerToPayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reconBrokerToPayDropEdit, "ZO_ReconBrokerToPay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_ReconBrokerToPay)));
			this.reconBrokerToPayDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("205c1745-9c41-4a25-bfc8-3815e65e1eb8", "Broker To Pay");
			this.reconBrokerToPayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(422, 34, true);
			this.reconBrokerToPayDropEdit.Name = "ReconBrokerToPayDropEdit";
			this.reconBrokerToPayDropEdit.PreBoundMaxLength = 1;
			this.reconBrokerToPayDropEdit.ShowDescriptionBox = false;
			this.reconBrokerToPayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.reconBrokerToPayDropEdit.TabIndex = 9;
			// 
			// ZO_ReconFilingPortDropEdit
			// 
			this.zO_ReconFilingPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zO_ReconFilingPortFindBox, "ZO_ReconFilingPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_ReconFilingPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ImportAddInfoLookups.ReconPorts)));
			this.zO_ReconFilingPortFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b699715b-3194-4f22-b266-31199f4c61db", "Filing Port");
			this.zO_ReconFilingPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 11, true);
			this.zO_ReconFilingPortFindBox.Name = "ZO_ReconFilingPortDropEdit";
			this.zO_ReconFilingPortFindBox.PreBoundMaxLength = 4;
			this.zO_ReconFilingPortFindBox.ShowDescriptionBox = true;
			this.zO_ReconFilingPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.zO_ReconFilingPortFindBox.TabIndex = 4;
			// 
			// ZO_ReconPaymentTypeDropEdit
			// 
			this.zO_ReconPaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zO_ReconPaymentTypeDropEdit, "ZO_ReconPaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_ReconPaymentType)));
			this.zO_ReconPaymentTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("24fe9abf-c106-4eca-a8aa-aca095ccf0bd", "Payment Type");
			this.zO_ReconPaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 34, true);
			this.zO_ReconPaymentTypeDropEdit.Name = "ZO_ReconPaymentTypeDropEdit";
			this.zO_ReconPaymentTypeDropEdit.PreBoundMaxLength = 1;
			this.zO_ReconPaymentTypeDropEdit.ShowDescriptionBox = false;
			this.zO_ReconPaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.zO_ReconPaymentTypeDropEdit.TabIndex = 7;
			// 
			// ZO_ImportSourceDropEdit
			// 
			this.zO_ImportSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zO_ImportSourceDropEdit, "ZO_ImportSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_ImportSource)));
			this.zO_ImportSourceDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("6ca89710-a665-4603-b277-0402b6e31ab7", "Import Source");
			this.zO_ImportSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 34, true);
			this.zO_ImportSourceDropEdit.Name = "ZO_ImportSourceDropEdit";
			this.zO_ImportSourceDropEdit.PreBoundMaxLength = 1;
			this.zO_ImportSourceDropEdit.ShowDescriptionBox = false;
			this.zO_ImportSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.zO_ImportSourceDropEdit.TabIndex = 11;
			// 
			// FileTheirOwnCheckBox
			// 
			this.fileTheirOwnCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.fileTheirOwnCheckBox, "ZO_FileTheirOwnRecon");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_FileTheirOwnRecon)));
			this.fileTheirOwnCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("00d66815-10bc-4e88-99c7-aa89a091a68e", "File Their Own Recon.");
			this.fileTheirOwnCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.fileTheirOwnCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fileTheirOwnCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 37, true);
			this.fileTheirOwnCheckBox.Name = "FileTheirOwnCheckBox";
			this.fileTheirOwnCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.fileTheirOwnCheckBox.TabIndex = 5;
			this.fileTheirOwnCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReconIndicatorDropEdit
			// 
			this.reconIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reconIndicatorDropEdit, "ZO_OtherReconIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.OrgHeaderWrapper)(null)).ZO_OtherReconIndicator)));
			this.reconIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8f8d0e94-a0b1-4bf9-a286-05553b919a06", "Recon. Issue");
			this.reconIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 11, true);
			this.reconIndicatorDropEdit.Name = "ReconIndicatorDropEdit";
			this.reconIndicatorDropEdit.PreBoundMaxLength = 2;
			this.reconIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 20, true);
			this.reconIndicatorDropEdit.TabIndex = 2;
			// 
			// NAFTAReconIndicatorCheckBox
			// 
			this.nAFTAReconIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.nAFTAReconIndicatorCheckBox, "ZO_NAFTAReconIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.OrgHeaderWrapper)(null)).ZO_NAFTAReconIndicator)));
			this.nAFTAReconIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d36d1760-0387-49f8-a313-9a9bc7bd2cd1", "FTA Recon.");
			this.nAFTAReconIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.nAFTAReconIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.nAFTAReconIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 14, true);
			this.nAFTAReconIndicatorCheckBox.Name = "NAFTAReconIndicatorCheckBox";
			this.nAFTAReconIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.nAFTAReconIndicatorCheckBox.TabIndex = 0;
			this.nAFTAReconIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// CBMAGroupox
			// 
			this.cBMAGroupox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7ad2c708-739f-4769-93ce-eb20aec32be8", "CBMA");
			this.cBMAGroupox.Controls.Add(this.allocationQuantityGroupBox);
			this.cBMAGroupox.Controls.Add(this.groupNameGroupBox);
			this.cBMAGroupox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cBMAGroupox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.cBMAGroupox.Name = "CBMAGroupox";
			this.cBMAGroupox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 343, true);
			this.cBMAGroupox.TabIndex = 8;
			this.cBMAGroupox.TabStop = false;
			// 
			// GroupNameGrid
			// 
			this.groupNameGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.groupNameGrid, "ImportersControlledGroupNames");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).ImportersControlledGroupNames)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ImportersControlledGroupName)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).ImportersControlledGroupNames)).SyncRoot)).US_GroupName)));
			this.groupNameGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "US_GroupName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			this.groupNameGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.groupNameGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupNameGrid.GridId = "81c08c8f-5f7c-4df1-a564-327f168bc08e";
			this.groupNameGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.groupNameGrid.LayoutKey = "RestrictedSPIsGrid";
			this.groupNameGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.groupNameGrid.Name = "zGrid1";
			this.groupNameGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 305, true);
			this.groupNameGrid.TabIndex = 0;
			// 
			// GroupNameGroupBox
			// 
			this.groupNameGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ae6bca51-9b46-441d-afa6-1561eb2eff98", "Importer's Controlled Group Name ");
			this.groupNameGroupBox.Controls.Add(this.groupNameGrid);
			this.groupNameGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.groupNameGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.groupNameGroupBox.Name = "GroupNameGroupBox";
			this.groupNameGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 324, true);
			this.groupNameGroupBox.TabIndex = 9;
			this.groupNameGroupBox.TabStop = false;
			// 
			// AllocationQuantityGroupBox
			// 
			this.allocationQuantityGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("35c1471d-9cd1-4e7a-8bdf-e398c1581ac4", "Allocation Quantity per FPI for Importer");
			this.allocationQuantityGroupBox.Controls.Add(this.allocationQuantityGrid);
			this.allocationQuantityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allocationQuantityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 16, true);
			this.allocationQuantityGroupBox.Name = "AllocationQuantityGroupBox";
			this.allocationQuantityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 324, true);
			this.allocationQuantityGroupBox.TabIndex = 10;
			this.allocationQuantityGroupBox.TabStop = false;
			// 
			// AllocationQuantityGrid
			// 
			this.allocationQuantityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.allocationQuantityGrid, "AllocationQuantityPerFPIs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.AllocationQuantityPerFPI)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)).SyncRoot)).ManufacturerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AllocationQuantityPerFPI)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)).SyncRoot)).US_ManufacturerCompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.AllocationQuantityPerFPI)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)).SyncRoot)).US_OA_ManufacturerAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.AllocationQuantityPerFPI)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)).SyncRoot)).US_ForeignProducerIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.AllocationQuantityPerFPI)(((System.Collections.IList)(((Business.OrgHeaderWrapper)(null)).AllocationQuantityPerFPIs)).SyncRoot)).US_AllocationQuantity)));
			this.allocationQuantityGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ManufacturerOrgPK";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("94663d5f-cf99-4e44-8bde-83d0d7c1d24e", "Manufacturer");
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "US_ManufacturerCompanyName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zAddressDropEditColumnStyleInfo1.ColumnName = "US_OA_ManufacturerAddress";
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("94663d5f-cf99-4e44-8bde-83d0d7c1d24e", "Manufacturer");
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "US_ForeignProducerIdentifier";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo5.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "US_AllocationQuantity";
			zCalcEditColumnStyleInfo2.MaxValue = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.allocationQuantityGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.allocationQuantityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.allocationQuantityGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.allocationQuantityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.allocationQuantityGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.allocationQuantityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allocationQuantityGrid.GridId = "81c08c8f-5f7c-4df1-a564-327f168bc08e";
			this.allocationQuantityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.allocationQuantityGrid.LayoutKey = "RestrictedSPIsGrid";
			this.allocationQuantityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.allocationQuantityGrid.Name = "zGrid2";
			this.allocationQuantityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 305, true);
			this.allocationQuantityGrid.TabIndex = 0;
			// 
			// OrganisationDetailPlugInUserControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.bondDetailsGridPanel);
			this.Name = "OrganisationDetailPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 609, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bondDetailsGridPanel.ResumeLayout(false);
			this.bondDetailsGridPanel.PerformLayout();
			this.DetailsTabControl.ResumeLayout(false);
			this.DetailsTabControl.PerformLayout();
			this.details1TabPage.ResumeLayout(false);
			this.details1TabPage.PerformLayout();
			this.iORGroupBox.ResumeLayout(false);
			this.iORGroupBox.PerformLayout();
			this.iORSplitContainer.Panel1.ResumeLayout(false);
			this.iORSplitContainer.Panel1.PerformLayout();
			this.iORSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.iORSplitContainer)).EndInit();
			this.iORSplitContainer.ResumeLayout(false);
			this.iORSplitContainer.PerformLayout();
			this.zO_OA_ConsigneeAddressControl.ResumeLayout(true);
			this.zO_OA_ConsigneeAddressControl.PerformLayout();
			this.zO_OA_ShipToAddressControl.ResumeLayout(true);
			this.zO_OA_ShipToAddressControl.PerformLayout();
			this.knownImporterIndicatorDropEdit.ResumeLayout(true);
			this.knownImporterIndicatorDropEdit.PerformLayout();
			this.notifyPartyOrganisationFindBox.ResumeLayout(true);
			this.notifyPartyOrganisationFindBox.PerformLayout();
			this.paymentTypeDropEdit.ResumeLayout(true);
			this.paymentTypeDropEdit.PerformLayout();
			this.iSEINNumberVerifiedDropEdit.ResumeLayout(true);
			this.iSEINNumberVerifiedDropEdit.PerformLayout();
			this.taxDeferredIndDropEdit.ResumeLayout(true);
			this.taxDeferredIndDropEdit.PerformLayout();
			this.brokerToPayDropEdit.ResumeLayout(true);
			this.brokerToPayDropEdit.PerformLayout();
			this.payMethodDropEdit.ResumeLayout(true);
			this.payMethodDropEdit.PerformLayout();
			this.purchasedDropEdit.ResumeLayout(true);
			this.purchasedDropEdit.PerformLayout();
			this.iORBusinessRulesTableLayoutPanel.ResumeLayout(false);
			this.iORBusinessRulesTableLayoutPanel.PerformLayout();
			this.restrictedEntryTypesGroupBox.ResumeLayout(false);
			this.restrictedEntryTypesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedEntryTypesGrid)).EndInit();
			this.restrictedEntryTypesGrid.ResumeLayout(false);
			this.restrictedEntryTypesGrid.PerformLayout();
			this.restrictedSPIsGroupBox.ResumeLayout(false);
			this.restrictedSPIsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedSPIsGrid)).EndInit();
			this.restrictedSPIsGrid.ResumeLayout(false);
			this.restrictedSPIsGrid.PerformLayout();
			this.restrictedTariffsGroupBox.ResumeLayout(false);
			this.restrictedTariffsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.restrictedTariffsGrid)).EndInit();
			this.restrictedTariffsGrid.ResumeLayout(false);
			this.restrictedTariffsGrid.PerformLayout();
			this.fTZAllowsFDAGroupBox.ResumeLayout(false);
			this.fTZAllowsFDAGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fTZAllowsFDAGrid)).EndInit();
			this.fTZAllowsFDAGrid.ResumeLayout(false);
			this.fTZAllowsFDAGrid.PerformLayout();
			this.fDAGroupBox.ResumeLayout(false);
			this.fDAGroupBox.PerformLayout();
			this.mFRRegExemptDropEdit.ResumeLayout(true);
			this.mFRRegExemptDropEdit.PerformLayout();
			this.submitterFirmTypeDropEdit.ResumeLayout(true);
			this.submitterFirmTypeDropEdit.PerformLayout();
			this.producerFirmTypeDropEdit.ResumeLayout(true);
			this.producerFirmTypeDropEdit.PerformLayout();
			this.bondDetailsGroupBox.ResumeLayout(false);
			this.bondDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.bondDetailsGrid)).EndInit();
			this.bondDetailsGrid.ResumeLayout(false);
			this.bondDetailsGrid.PerformLayout();
			this.Details2TabPage.ResumeLayout(false);
			this.Details2TabPage.PerformLayout();
			this.fTZGroupBox.ResumeLayout(false);
			this.fTZGroupBox.PerformLayout();
			this.locationOfGoodsCodeFindBox.ResumeLayout(true);
			this.locationOfGoodsCodeFindBox.PerformLayout();
			this.defaultWarehouseAddressControl.ResumeLayout(true);
			this.defaultWarehouseAddressControl.PerformLayout();
			this.miscGroupBox.ResumeLayout(false);
			this.miscGroupBox.PerformLayout();
			this.defDateIndicDropEdit.ResumeLayout(true);
			this.defDateIndicDropEdit.PerformLayout();
			this.branchGuidFindBox.ResumeLayout(true);
			this.branchGuidFindBox.PerformLayout();
			this.eNS7501GroupBox.ResumeLayout(false);
			this.eNS7501GroupBox.PerformLayout();
			this.reconciliationGroupBox.ResumeLayout(false);
			this.reconciliationGroupBox.PerformLayout();
			this.reconBrokerToPayDropEdit.ResumeLayout(true);
			this.reconBrokerToPayDropEdit.PerformLayout();
			this.zO_ReconFilingPortFindBox.ResumeLayout(true);
			this.zO_ReconFilingPortFindBox.PerformLayout();
			this.zO_ReconPaymentTypeDropEdit.ResumeLayout(true);
			this.zO_ReconPaymentTypeDropEdit.PerformLayout();
			this.zO_ImportSourceDropEdit.ResumeLayout(true);
			this.zO_ImportSourceDropEdit.PerformLayout();
			this.reconIndicatorDropEdit.ResumeLayout(true);
			this.reconIndicatorDropEdit.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.cBMAGroupox.ResumeLayout(false);
			this.cBMAGroupox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.groupNameGrid)).EndInit();
			this.groupNameGrid.ResumeLayout(false);
			this.groupNameGrid.PerformLayout();
			this.groupNameGroupBox.ResumeLayout(false);
			this.groupNameGroupBox.PerformLayout();
			this.allocationQuantityGroupBox.ResumeLayout(false);
			this.allocationQuantityGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.allocationQuantityGrid)).EndInit();
			this.allocationQuantityGrid.ResumeLayout(false);
			this.allocationQuantityGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
