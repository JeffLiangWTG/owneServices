using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class CusUSLVClearanceForm
	{
		private ZPanel panelTop;
		private TransportDetailsUserControl transportDetailsControl;
		private ZGroupBox groupBoxHouseBills;
		private ZArchitecture.ZGrid gridHouseBills;
		private ZTabControl tabControlBottom;
		private ZTabPage tabPageHouseBillDetails;
		private ZTabPage tabPageHouseBillSummary;
		private ZTabPage tabPageHouseBillMessages;
		private System.ComponentModel.IContainer components = null;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox organizationFindBoxClient;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox organizationFindBoxImporter;
		private ZDropEdit dropRegistrationType;
		private ZArchitecture.ZTextBox textBoxRegistrationNum;
		private ZArchitecture.ZTextBox textBoxFiler;
		private ZArchitecture.ZTextBox textBoxContactName;
		private ZArchitecture.ZTextBox textBoxContactPhone;
		private ZArchitecture.ZTextBox textBoxMatchingKey;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox guidFindBoxBranch;
		private ZTabControl RightTabControl;
		private ZPanel CenterPanel;
		private Enterprise.ZArchitecture.GUI.ZTabPage OrganisationsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage miscTabPage;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanelMisc;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxFiler;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxRemoteFiling;
		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBoxFIRMSAndCES;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxCentralizedExamSite;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLocationOfGoods;
		private ZArchitecture.GUI.ZCheckBox checkBoxRemoteLocationFiling;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox codeFindBoxPreparerDistrictPort;
		private ZArchitecture.ZTextBox textBoxPreparerOfficeCode;
		private KTableLayoutPanel tableLayoutPanelSummary;
		private ZGroupBox groupBoxSeller;
		private ZAddressControl addressControlSeller;
		private ZArchitecture.ZTextBox textBoxSellerAddress1;
		private ZArchitecture.ZTextBox textBoxSellerName;
		private ZCodeFindBox codeFindBoxSellerCountry;
		private ZArchitecture.ZTextBox textBoxSellerPostCode;
		private ZArchitecture.ZTextBox textBoxSellerCity;
		private ZArchitecture.ZTextBox textBoxSellerState;
		private ZArchitecture.ZTextBox textBoxSellerAddress2;
		private ZGroupBox groupBoxConsignee;
		private ZCodeFindBox codeFindBoxConsigneeCountry;
		private ZArchitecture.ZTextBox textBoxConsigneePostCode;
		private ZArchitecture.ZTextBox textBoxConsigneeCity;
		private ZArchitecture.ZTextBox textBoxConsigneeState;
		private ZArchitecture.ZTextBox TextBoxConsigneeAddress2;
		private ZArchitecture.ZTextBox textBoxConsigneeAddress1;
		private ZArchitecture.ZTextBox textBoxConsigneeName;
		private ZAddressControl addressControlConsignee;
		private ZGroupBox groupBoxCustomsInfo;
		private ZArchitecture.GUI.ZDropEdit dropEditReleaseStatus;
		private ZArchitecture.GUI.ZDropEdit dropEditMessageStatus;
		private ZArchitecture.GUI.ZDateEdit dateEditReleaseDate;
		private ZArchitecture.GUI.ZDateEdit dateEditSubmittedDate;
		private ZArchitecture.GUI.ZDateEdit dateEditLatestMsgStatusDate;
		private ZGroupBox groupBoxDisposition;
		private ZGrid gridDisposition;
		private US.GUI.ImportMessagesUserControl importMessagesUserControl;
		private ZGroupBox groupBoxCommodityDetails;
		private ZGrid gridCommodityDetails;
		private MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		private Enterprise.Customs.US.LVS.GUI.OGAPGARequirementsControl pgaRequirementsControl;
		private CargoWise.Windows.UI.KSplitContainer splitContainerHouseBillDetails;
		private CargoWise.Windows.UI.KSplitter splitterBottom;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 846, true);
			this.workflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.workflowTabPage_InitializeTab));
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1592, 819, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1592, 819, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1592, 819, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 846, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.workflowTabPage.TabIndex = 3;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_Calc_USTransportMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfDischarge)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfEntry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PortOfLoading)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_CarrierSCAC)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_VoyageFlightNo)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ConveyanceName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ConveyanceName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBillIssuerSCAC)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RemoteLocationFiling)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ContainerMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_TransportMode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_DepartureDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_EntryDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_DischargeDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RL_NKPortOfLoading)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RL_NKPortOfDischarge)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MasterBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_HouseBillIssuerSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemGoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemLineValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemAntiDumping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).FirstCusUSLVItemCountervailing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SubmittedDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_OwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_EquipmentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_NumberOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_NonAMSIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_DISIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ConsigneeOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_OA_Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeQualifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneePostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_RN_NKConsigneeCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).SellerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_OA_Seller)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerCity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerPostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_RN_NKSellerCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_RailReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_EntryLineReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ITNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ITDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_IORType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_OH_Client)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_OH_Importer)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_IORReference)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_EntryFilerCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ContactName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_ContactPhone)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_MatchingKey)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_OA_Seller)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerAddress1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerAddress2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerCity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SellerPostCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_RN_NKSellerCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_RN_NKConsigneeCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneePostCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeCity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeAddress1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeAddress2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_ConsigneeName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_OA_Consignee)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_EntryStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_MessageStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_MessageStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).ULB_SubmittedDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CE_IssueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_TariffFormatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RX_NKCurrEXRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RN_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_AntiDumping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_Countervailing)));
			// 
			// CusUSLVClearanceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 902, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVClearance);
			this.Name = "CusUSLVClearanceForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		private void workflowTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.workflowTabPage.SuspendLayout();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo partNoColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.US.GUI.TariffColumnStyleInfo tariffColumnStyleInfo2 = new Enterprise.Customs.US.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.panelTop = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.transportDetailsControl = new TransportDetailsUserControl();
			this.groupBoxHouseBills = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridHouseBills = new Enterprise.ZArchitecture.ZGrid();
			this.tabControlBottom = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.tabPageHouseBillDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageHouseBillSummary = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageHouseBillMessages = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RightTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CenterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.miscTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitterBottom = new CargoWise.Windows.UI.KSplitter();
			this.groupBoxRemoteFiling = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checkBoxRemoteLocationFiling = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.codeFindBoxPreparerDistrictPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.textBoxPreparerOfficeCode = new Enterprise.ZArchitecture.ZTextBox();
			this.groupBoxFIRMSAndCES = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.codeFindBoxCentralizedExamSite = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxLocationOfGoods = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.groupBoxRemoteFiling.SuspendLayout();
			this.groupBoxFIRMSAndCES.SuspendLayout();
			this.codeFindBoxCentralizedExamSite.SuspendLayout();
			this.codeFindBoxLocationOfGoods.SuspendLayout();
			this.OrganisationsTabPage.SuspendLayout();
			this.miscTabPage.SuspendLayout();
			this.checkBoxRemoteLocationFiling.SuspendLayout();
			this.codeFindBoxPreparerDistrictPort.SuspendLayout();
			this.textBoxPreparerOfficeCode.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.panelTop.SuspendLayout();
			this.transportDetailsControl.SuspendLayout();
			this.groupBoxHouseBills.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridHouseBills)).BeginInit();
			this.gridHouseBills.SuspendLayout();
			this.tabControlBottom.SuspendLayout();
			this.tabPageHouseBillDetails.SuspendLayout();
			this.tabPageHouseBillSummary.SuspendLayout();
			this.tabPageHouseBillMessages.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.groupBoxHouseBills);
			this.MainTabPage.Controls.Add(this.splitterBottom);
			this.MainTabPage.Controls.Add(this.panelTop);
			this.MainTabPage.Controls.Add(this.tabControlBottom);
			// 
			// panelTop
			// 
			this.panelTop.Controls.Add(this.RightTabControl);
			this.panelTop.Controls.Add(this.CenterPanel);
			this.panelTop.Controls.Add(this.transportDetailsControl);
			this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panelTop.Name = "panelTop";
			this.panelTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1592, 216, true);
			this.panelTop.TabIndex = 95;
			// 
			// transportDetailsControl
			// 
			this.BindingSource.SetBindingMember(this.transportDetailsControl, ".");
			this.transportDetailsControl.Name = "transportDetailsControl";
			// 
			// groupBoxHouseBills
			// 
			this.groupBoxHouseBills.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("53fa89fc-1fa5-486b-bafc-a5c464d72e77", "House Bills");
			this.groupBoxHouseBills.Controls.Add(this.gridHouseBills);
			this.groupBoxHouseBills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxHouseBills.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 216, true);
			this.groupBoxHouseBills.Name = "groupBoxHouseBills";
			this.groupBoxHouseBills.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1376, 250, true);
			this.groupBoxHouseBills.TabIndex = 99;
			this.groupBoxHouseBills.TabStop = false;
			// 
			// gridHouseBills
			// 
			this.gridHouseBills.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridHouseBills, "CusUSLVConsignments");
			this.gridHouseBills.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ULB_HouseBillIssuerSCAC";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ULB_HouseBill";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			partNoColumnStyleInfo.ColumnName = "FirstCusUSLVItemProductCode";
			partNoColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo2.ColumnName = "FirstCusUSLVItemTariff";
			tariffColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo27.ColumnName = "FirstCusUSLVItemGoodsDescription";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo27.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo4.ColumnName = "FirstCusUSLVItemLineValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo7.ColumnName = "FirstCusUSLVItemCurrency";
			zCodeFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.ColumnName = "FirstCusUSLVItemExchangeRate";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo8.ColumnName = "FirstCusUSLVItemCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.ColumnName = "FirstCusUSLVItemAntiDumping";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "FirstCusUSLVItemCountervailing";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "ULB_SubmittedDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "CE_IssueDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo2.ColumnName = "CE_EntryStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "ULB_MessageStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "ULB_OwnerReferenceNumber";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "ULB_EquipmentNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ULB_GoodsValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.ColumnName = "ULB_Currency";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ULB_NumberOfPacks";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.ColumnName = "ULB_PackType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.ColumnName = "ULB_NonAMSIndicator";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.ColumnName = "ULB_DISIndicator";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "ConsigneeOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.ColumnName = "ULB_OA_Consignee";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "ULB_ConsigneeQualifier";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "ULB_ConsigneeIdentifier";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "ULB_ConsigneeName";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo11.ColumnName = "ULB_ConsigneeAddress1";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "ULB_ConsigneeAddress2";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "ULB_ConsigneeCity";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ULB_ConsigneePostCode";
			zCodeFindBoxColumnStyleInfo2.IsVisible = false;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ULB_RN_NKConsigneeCountry";
			zCodeFindBoxColumnStyleInfo3.IsVisible = false;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "SellerOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo2.ColumnName = "ULB_OA_Seller";
			zAddressDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "ULB_SellerIdentifier";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.ColumnName = "ULB_SellerName";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo16.ColumnName = "ULB_SellerAddress1";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo17.ColumnName = "ULB_SellerAddress2";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo18.ColumnName = "ULB_SellerCity";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo4.ColumnName = "ULB_SellerPostCode";
			zCodeFindBoxColumnStyleInfo4.IsVisible = false;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo5.ColumnName = "ULB_RN_NKSellerCountry";
			zCodeFindBoxColumnStyleInfo5.IsVisible = false;
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo19.ColumnName = "CE_RailReferenceNumber";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "CE_EntryLineReference";
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo21.ColumnName = "ULB_SellerState";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo22.ColumnName = "ULB_ConsigneeState";
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo23.Caption = "IT Number";
			zTextBoxColumnStyleInfo23.ColumnName = "ITNumber";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "ITDate";
			zDateEditColumnStyleInfo3.Caption = "IT Date";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ULB_EntryType";
			zDropEditColumnStyleInfo2.IsVisible = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(partNoColumnStyleInfo);
			this.gridHouseBills.ColumnStyles.Add(tariffColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.gridHouseBills.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.gridHouseBills.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo8);
			this.gridHouseBills.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.gridHouseBills.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.gridHouseBills.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.gridHouseBills.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.gridHouseBills.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.gridHouseBills.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zAddressDropEditColumnStyleInfo2);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.gridHouseBills.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.gridHouseBills.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.gridHouseBills.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.gridHouseBills.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridHouseBills.GridId = "b2206c31-fd8f-4fda-8628-c5667ab9fab5";
			this.gridHouseBills.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridHouseBills.LayoutKey = "gridHouseBills";
			this.gridHouseBills.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridHouseBills.Name = "gridHouseBills";
			this.gridHouseBills.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1586, 267, true);
			this.gridHouseBills.TabIndex = 0;
			// 
			// tabControlBottom
			// 
			this.tabControlBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControlBottom.Controls.Add(this.tabPageHouseBillDetails);
			this.tabControlBottom.Controls.Add(this.tabPageHouseBillSummary);
			this.tabControlBottom.Controls.Add(this.tabPageHouseBillMessages);
			this.tabControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tabControlBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 502, true);
			this.tabControlBottom.Name = "tabControlBottom";
			this.tabControlBottom.SelectedIndex = 0;
			this.tabControlBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1592, 400, true);
			this.tabControlBottom.TabIndex = 97;
			// 
			// tabPageHouseBillDetails
			// 
			this.tabPageHouseBillDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("11efd4f8-91ad-424f-a771-fb91d71b54d7", "Details");
			this.tabPageHouseBillDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageHouseBillDetails.Name = "tabPageHouseBillDetails";
			this.tabPageHouseBillDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageHouseBillDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1584, 290, true);
			this.tabPageHouseBillDetails.TabIndex = 0;
			this.tabPageHouseBillDetails.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageHouseBillDetails_InitializeTab));
			// 
			// tabPageHouseBillSummary
			// 
			this.tabPageHouseBillSummary.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1498c877-4750-42e1-ae1f-e0a7df75c5fa", "Summary");
			this.tabPageHouseBillSummary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageHouseBillSummary.Name = "tabPageHouseBillSummary";
			this.tabPageHouseBillSummary.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageHouseBillSummary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1584, 290, true);
			this.tabPageHouseBillSummary.TabIndex = 1;
			this.tabPageHouseBillSummary.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageHouseBillSummary_InitializeTab));
			// 
			// tabPageHouseBillMessages
			// 
			this.tabPageHouseBillMessages.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("afe9eddc-13b9-4e9b-b9ef-30a993e51385", "Messages");
			this.tabPageHouseBillMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageHouseBillMessages.Name = "tabPageHouseBillMessages";
			this.tabPageHouseBillMessages.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageHouseBillMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1584, 290, true);
			this.tabPageHouseBillMessages.TabIndex = 2;
			this.tabPageHouseBillMessages.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageHouseBillMessages_InitializeTab));
			// 
			// RightTabControl
			// 
			this.RightTabControl.Controls.Add(this.OrganisationsTabPage);
			this.RightTabControl.Controls.Add(this.miscTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(932, 0, true);
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.SelectedIndex = 0;
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 216, true);
			this.RightTabControl.TabIndex = 99;
			// 
			// CenterPanel
			// 
			this.CenterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 0, true);
			this.CenterPanel.Name = "CenterTabControl";
			this.CenterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 216, true);
			this.CenterPanel.TabIndex = 98;
			this.CenterPanel.Controls.Add(this.groupBoxRemoteFiling);
			this.CenterPanel.Controls.Add(this.groupBoxFIRMSAndCES);
			// 
			// OrganisationsTabPage
			// 
			this.OrganisationsTabPage.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("cf179c01-4369-4a08-94a9-8dd59db3306f", "Organizations");
			this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrganisationsTabPage.Name = "OrganisationsTabPage";
			this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 189, true);
			this.OrganisationsTabPage.TabIndex = 0;
			this.OrganisationsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.OrganisationsTabPage_InitializeTab));
			// 
			// miscTabPage
			// 
			this.miscTabPage.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2e73ba2e-5314-4c17-9fd4-4b2f5b057644", "Misc");
			this.miscTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.miscTabPage.Name = "miscTabPage";
			this.miscTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 189, true);
			this.miscTabPage.TabIndex = 1;
			this.miscTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MiscTabPage_InitializeTab));
			// 
			// splitterBottom
			// 
			this.splitterBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitterBottom.Name = "splitterBottom";
			this.splitterBottom.TabIndex = 22;
			this.splitterBottom.MinExtra = 90;
			this.splitterBottom.SplitPosition = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			this.splitterBottom.TabStop = false;
			this.splitterBottom.MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			this.splitterBottom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 466, true);
			this.splitterBottom.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1584, 3, true);
			// 
			// groupBoxFIRMSAndCES
			//
			this.groupBoxFIRMSAndCES.Controls.Add(this.codeFindBoxCentralizedExamSite);
			this.groupBoxFIRMSAndCES.Controls.Add(this.codeFindBoxLocationOfGoods);
			this.groupBoxFIRMSAndCES.Dock = System.Windows.Forms.DockStyle.None;
			this.groupBoxFIRMSAndCES.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxFIRMSAndCES.Name = "groupBoxFIRMSAndCES";
			this.groupBoxFIRMSAndCES.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 80, true);
			this.groupBoxFIRMSAndCES.TabIndex = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.groupBoxFIRMSAndCES, false);
			// 
			// codeFindBoxLocationOfGoods
			// 
			this.codeFindBoxLocationOfGoods.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLocationOfGoods, "ULH_US_NKLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_US_NKLocationOfGoods)));
			this.codeFindBoxLocationOfGoods.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("6DEE004E-4E1C-4854-A133-C957D5DF5CBD", "FIRMS Code");
			this.codeFindBoxLocationOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 12, true);
			this.codeFindBoxLocationOfGoods.Name = "codeFindBoxLocationOfGoods";
			this.codeFindBoxLocationOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.codeFindBoxLocationOfGoods.TabIndex = 0;
			// 
			// codeFindBoxCentralizedExamSite
			// 
			this.codeFindBoxCentralizedExamSite.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxCentralizedExamSite, "ULH_US_NKCentralizedExamSite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_US_NKCentralizedExamSite)));
			this.codeFindBoxCentralizedExamSite.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("7FF2CF77-6FC2-454A-89B3-4191E70D15AE", "Centralized Exam Site");
			this.codeFindBoxCentralizedExamSite.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 38, true);
			this.codeFindBoxCentralizedExamSite.Name = "codeFindBoxCentralizedExamSite";
			this.codeFindBoxCentralizedExamSite.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.codeFindBoxCentralizedExamSite.TabIndex = 1;
			// 
			// groupBoxRemoteFiling
			// 
			this.groupBoxRemoteFiling.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d8e36ac1-d406-4d01-a1cb-91f6aa74a2e4", "Remote Filing");
			this.groupBoxRemoteFiling.Controls.Add(this.checkBoxRemoteLocationFiling);
			this.groupBoxRemoteFiling.Controls.Add(this.codeFindBoxPreparerDistrictPort);
			this.groupBoxRemoteFiling.Controls.Add(this.textBoxPreparerOfficeCode);
			this.groupBoxRemoteFiling.Dock = System.Windows.Forms.DockStyle.None;
			this.groupBoxRemoteFiling.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.groupBoxRemoteFiling.Name = "groupBoxRemoteFiling";
			this.groupBoxRemoteFiling.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 96, true);
			this.groupBoxRemoteFiling.TabIndex = 1;
			// 
			// checkBoxRemoteLocationFiling
			// 
			this.BindingSource.SetBindingMember(this.checkBoxRemoteLocationFiling, "ULH_RemoteLocationFiling");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_RemoteLocationFiling)));
			this.checkBoxRemoteLocationFiling.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 12, true);
			this.checkBoxRemoteLocationFiling.Name = "checkBoxRemoteLocationFiling";
			this.checkBoxRemoteLocationFiling.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.checkBoxRemoteLocationFiling.TabIndex = 2;
			// 
			// codeFindBoxEntryPort
			// 
			this.codeFindBoxPreparerDistrictPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxPreparerDistrictPort, "ULH_PreparerDistrictPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PreparerDistrictPort)));
			this.codeFindBoxPreparerDistrictPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 38, true);
			this.codeFindBoxPreparerDistrictPort.Name = "codeFindBoxPreparerDistrictPort";
			this.codeFindBoxPreparerDistrictPort.ShouldResize = true;
			this.codeFindBoxPreparerDistrictPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.codeFindBoxPreparerDistrictPort.TabIndex = 3;
			// 
			// textBoxPreparerOfficeCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxPreparerOfficeCode, "ULH_PreparerOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_PreparerOfficeCode)));
			this.textBoxPreparerOfficeCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 64, true);
			this.textBoxPreparerOfficeCode.Name = "textBoxPreparerOfficeCode";
			this.textBoxPreparerOfficeCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.textBoxPreparerOfficeCode.TabIndex = 4;
			this.groupBoxFIRMSAndCES.ResumeLayout(true);
			this.groupBoxFIRMSAndCES.PerformLayout();
			this.codeFindBoxLocationOfGoods.ResumeLayout(true);
			this.codeFindBoxLocationOfGoods.PerformLayout();
			this.codeFindBoxCentralizedExamSite.ResumeLayout(true);
			this.codeFindBoxCentralizedExamSite.PerformLayout();
			this.groupBoxRemoteFiling.ResumeLayout(false);
			this.groupBoxRemoteFiling.PerformLayout();
			this.checkBoxRemoteLocationFiling.ResumeLayout(false);
			this.checkBoxRemoteLocationFiling.PerformLayout();
			this.codeFindBoxPreparerDistrictPort.ResumeLayout(false);
			this.codeFindBoxPreparerDistrictPort.PerformLayout();
			this.textBoxPreparerOfficeCode.ResumeLayout(false);
			this.textBoxPreparerOfficeCode.PerformLayout();
			this.MainTabPage.PerformLayout();
			this.panelTop.ResumeLayout(false);
			this.panelTop.PerformLayout();
			this.transportDetailsControl.ResumeLayout(false);
			this.transportDetailsControl.PerformLayout();
			this.groupBoxHouseBills.ResumeLayout(false);
			this.groupBoxHouseBills.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridHouseBills)).EndInit();
			this.gridHouseBills.ResumeLayout(false);
			this.gridHouseBills.PerformLayout();
			this.tabControlBottom.ResumeLayout(false);
			this.tabControlBottom.PerformLayout();
			this.tabPageHouseBillDetails.ResumeLayout(false);
			this.tabPageHouseBillDetails.PerformLayout();
			this.tabPageHouseBillSummary.ResumeLayout(false);
			this.tabPageHouseBillSummary.PerformLayout();
			this.tabPageHouseBillMessages.ResumeLayout(false);
			this.tabPageHouseBillMessages.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrganisationsTabPage.ResumeLayout(false);
			this.OrganisationsTabPage.PerformLayout();
			this.miscTabPage.ResumeLayout(false);
			this.miscTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void tabPageHouseBillMessages_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.importMessagesUserControl = new Enterprise.Customs.US.GUI.ImportMessagesUserControl();
			this.tabPageHouseBillMessages.Controls.Add(this.importMessagesUserControl);
			this.importMessagesUserControl.SuspendLayout();
			// 
			// importMessagesUserControl
			// 
			this.importMessagesUserControl.AllowDrop = true;
			this.importMessagesUserControl.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVConsignment);
			this.BindingSource.SetBindingMember(this.importMessagesUserControl, "CusUSLVConsignments");
			this.importMessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.importMessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.importMessagesUserControl.Name = "importMessagesUserControl";
			this.importMessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1578, 284, true);
			this.importMessagesUserControl.TabIndex = 0;
			this.importMessagesUserControl.ResumeLayout(true);
			this.importMessagesUserControl.PerformLayout();
			this.importMessagesUserControl.ShowMessageHeaderGrid = false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		private void tabPageHouseBillSummary_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.tableLayoutPanelSummary = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.tabPageHouseBillSummary.Controls.Add(this.tableLayoutPanelSummary);
			this.groupBoxSeller = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.addressControlSeller = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.textBoxSellerName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerAddress1 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerAddress2 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerCity = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerState = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerPostCode = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxSellerCountry = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.groupBoxConsignee = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.codeFindBoxConsigneeCountry = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.textBoxConsigneePostCode = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeCity = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeState = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeAddress1 = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxConsigneeAddress2 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeName = new Enterprise.ZArchitecture.ZTextBox();
			this.addressControlConsignee = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.groupBoxCustomsInfo = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.dropEditReleaseStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dropEditMessageStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dateEditSubmittedDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateEditReleaseDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateEditLatestMsgStatusDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.groupBoxDisposition = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridDisposition = new Enterprise.ZArchitecture.ZGrid();
			this.tableLayoutPanelSummary.SuspendLayout();
			this.groupBoxSeller.SuspendLayout();
			this.addressControlSeller.SuspendLayout();
			this.codeFindBoxSellerCountry.SuspendLayout();
			this.groupBoxConsignee.SuspendLayout();
			this.codeFindBoxConsigneeCountry.SuspendLayout();
			this.addressControlConsignee.SuspendLayout();
			this.groupBoxCustomsInfo.SuspendLayout();
			this.dropEditReleaseStatus.SuspendLayout();
			this.dropEditMessageStatus.SuspendLayout();
			this.dateEditSubmittedDate.SuspendLayout();
			this.dateEditReleaseDate.SuspendLayout();
			this.dateEditLatestMsgStatusDate.SuspendLayout();
			this.groupBoxDisposition.SuspendLayout();
			this.gridDisposition.SuspendLayout();
			zDateEditColumnStyleInfo3.Caption = "Date";
			zDateEditColumnStyleInfo3.ColumnComparer = null;
			zDateEditColumnStyleInfo3.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo3.IsMandatory = true;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo22.Caption = "Code ID";
			zTextBoxColumnStyleInfo22.ColumnComparer = null;
			zTextBoxColumnStyleInfo22.ColumnName = "ErrorMessageIdentifier";
			zTextBoxColumnStyleInfo22.IsMandatory = true;
			zTextBoxColumnStyleInfo22.IsReadOnly = true;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo23.Caption = "Narrative";
			zTextBoxColumnStyleInfo23.ColumnComparer = null;
			zTextBoxColumnStyleInfo23.ColumnName = "NarrativeMessage";
			zTextBoxColumnStyleInfo23.IsMandatory = true;
			zTextBoxColumnStyleInfo23.IsReadOnly = true;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zDateEditColumnStyleInfo4.Caption = "Release Date";
			zDateEditColumnStyleInfo4.ColumnComparer = null;
			zDateEditColumnStyleInfo4.ColumnName = "ReleaseDate";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo24.Caption = "Release Origin";
			zTextBoxColumnStyleInfo24.ColumnComparer = null;
			zTextBoxColumnStyleInfo24.ColumnName = "ReleaseOrigin";
			zTextBoxColumnStyleInfo24.IsReadOnly = true;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo25.Caption = "Release Origin Description";
			zTextBoxColumnStyleInfo25.ColumnComparer = null;
			zTextBoxColumnStyleInfo25.ColumnName = "ReleaseOriginDescription";
			zTextBoxColumnStyleInfo25.IsReadOnly = true;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			// 
			// tableLayoutPanelSummary
			// 
			this.tableLayoutPanelSummary.ColumnCount = 3;
			this.tableLayoutPanelSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSummary.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelSummary.Controls.Add(this.groupBoxConsignee, 0, 0);
			this.tableLayoutPanelSummary.Controls.Add(this.groupBoxSeller, 0, 0);
			this.tableLayoutPanelSummary.Controls.Add(this.groupBoxCustomsInfo, 2, 0);
			this.tableLayoutPanelSummary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanelSummary.Name = "tableLayoutPanelSummary";
			this.tableLayoutPanelSummary.RowCount = 1;
			this.tableLayoutPanelSummary.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelSummary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 203, true);
			this.tableLayoutPanelSummary.TabIndex = 0;
			// 
			// groupBoxSeller
			// 
			this.groupBoxSeller.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("9b6f4627-b335-47d5-b824-51d4f2bb5dea", "Seller");
			this.groupBoxSeller.Controls.Add(this.codeFindBoxSellerCountry);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerPostCode);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerCity);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerState);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerAddress2);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerAddress1);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerName);
			this.groupBoxSeller.Controls.Add(this.addressControlSeller);
			this.groupBoxSeller.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxSeller.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.groupBoxSeller.Name = "groupBoxSeller";
			this.groupBoxSeller.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 177, true);
			this.groupBoxSeller.TabIndex = 0;
			this.groupBoxSeller.TabStop = false;
			// 
			// addressControlSeller
			// 
			this.addressControlSeller.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.addressControlSeller, "CusUSLVConsignments.ULB_OA_Seller");
			this.addressControlSeller.BindToOrgList = "CusUSLVConsignments.Lookups+SellerOrgList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.addressControlSeller, false);
			this.addressControlSeller.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.addressControlSeller.Name = "addressControlSeller";
			this.addressControlSeller.PopupCaption = "";
			this.addressControlSeller.ReadOnly = false;
			this.addressControlSeller.ShowAddress = false;
			this.addressControlSeller.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.addressControlSeller.TabIndex = 0;
			// 
			// textBoxSellerName
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerName, "CusUSLVConsignments.ULB_SellerName");
			this.textBoxSellerName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b263660d-cab4-4799-bce5-79e1e61964b7", "Name");
			this.textBoxSellerName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 42, true);
			this.textBoxSellerName.Name = "textBoxSellerName";
			this.textBoxSellerName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerName.TabIndex = 1;
			// 
			// textBoxSellerAddress1
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerAddress1, "CusUSLVConsignments.ULB_SellerAddress1");
			this.textBoxSellerAddress1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1614562a-70d6-4f85-b7db-25cfdce59b8e", "Addr.");
			this.textBoxSellerAddress1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 68, true);
			this.textBoxSellerAddress1.Name = "textBoxSellerAddress1";
			this.textBoxSellerAddress1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerAddress1.TabIndex = 2;
			// 
			// textBoxSellerAddress2
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerAddress2, "CusUSLVConsignments.ULB_SellerAddress2");
			this.textBoxSellerAddress2.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBoxSellerAddress2, false);
			this.textBoxSellerAddress2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 94, true);
			this.textBoxSellerAddress2.Name = "textBoxSellerAddress2";
			this.textBoxSellerAddress2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerAddress2.TabIndex = 3;
			// 
			// textBoxSellerCity
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerCity, "CusUSLVConsignments.ULB_SellerCity");
			this.textBoxSellerCity.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("140b73cb-071e-42bf-bd31-41001a4af327", "City");
			this.textBoxSellerCity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 120, true);
			this.textBoxSellerCity.Name = "textBoxSellerCity";
			this.textBoxSellerCity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.textBoxSellerCity.TabIndex = 4;
			// 
			// textBoxSellerPostCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerPostCode, "CusUSLVConsignments.ULB_SellerPostCode");
			this.textBoxSellerPostCode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d6338016-6998-4a59-b24d-b905b7f83cec", "Zip");
			this.textBoxSellerPostCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 146, true);
			this.textBoxSellerPostCode.Name = "textBoxSellerPostCode";
			this.textBoxSellerPostCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.textBoxSellerPostCode.TabIndex = 6;
			// 
			// codeFindBoxSellerCountry
			// 
			this.codeFindBoxSellerCountry.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxSellerCountry, "CusUSLVConsignments.ULB_RN_NKSellerCountry");
			this.codeFindBoxSellerCountry.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5f02cda1-af98-4e1e-bb48-49b480c9f169", "Ctry/Rgn.", "Country/Region");
			this.codeFindBoxSellerCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 146, true);
			this.codeFindBoxSellerCountry.Name = "codeFindBoxSellerCountry";
			this.codeFindBoxSellerCountry.PreBoundMaxLength = 2;
			this.codeFindBoxSellerCountry.ShouldResize = true;
			this.codeFindBoxSellerCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.codeFindBoxSellerCountry.TabIndex = 7;
			// 
			// groupBoxConsignee
			// 
			this.groupBoxConsignee.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2229d0c8-82c7-440c-8291-efed6fbfc66a", "Consignee");
			this.groupBoxConsignee.Controls.Add(this.codeFindBoxConsigneeCountry);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneePostCode);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeCity);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeState);
			this.groupBoxConsignee.Controls.Add(this.TextBoxConsigneeAddress2);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeAddress1);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeName);
			this.groupBoxConsignee.Controls.Add(this.addressControlConsignee);
			this.groupBoxConsignee.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxConsignee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 3, true);
			this.groupBoxConsignee.Name = "groupBoxConsignee";
			this.groupBoxConsignee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 177, true);
			this.groupBoxConsignee.TabIndex = 1;
			this.groupBoxConsignee.TabStop = false;
			// 
			// codeFindBoxConsigneeCountry
			// 
			this.codeFindBoxConsigneeCountry.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxConsigneeCountry, "CusUSLVConsignments.ULB_RN_NKConsigneeCountry");
			this.codeFindBoxConsigneeCountry.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5f02cda1-af98-4e1e-bb48-49b480c9f169", "Ctry/Rgn.", "Country/Region");
			this.codeFindBoxConsigneeCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 146, true);
			this.codeFindBoxConsigneeCountry.Name = "codeFindBoxConsigneeCountry";
			this.codeFindBoxConsigneeCountry.PreBoundMaxLength = 2;
			this.codeFindBoxConsigneeCountry.ShouldResize = true;
			this.codeFindBoxConsigneeCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.codeFindBoxConsigneeCountry.TabIndex = 7;
			// 
			// textBoxConsigneePostCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneePostCode, "CusUSLVConsignments.ULB_ConsigneePostCode");
			this.textBoxConsigneePostCode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d6338016-6998-4a59-b24d-b905b7f83cec", "Zip");
			this.textBoxConsigneePostCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 146, true);
			this.textBoxConsigneePostCode.Name = "textBoxConsigneePostCode";
			this.textBoxConsigneePostCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.textBoxConsigneePostCode.TabIndex = 6;
			// 
			// textBoxConsigneeCity
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeCity, "CusUSLVConsignments.ULB_ConsigneeCity");
			this.textBoxConsigneeCity.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("140b73cb-071e-42bf-bd31-41001a4af327", "City");
			this.textBoxConsigneeCity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 120, true);
			this.textBoxConsigneeCity.Name = "textBoxConsigneeCity";
			this.textBoxConsigneeCity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.textBoxConsigneeCity.TabIndex = 4;
			// 
			// textBoxConsigneeAddress1
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeAddress1, "CusUSLVConsignments.ULB_ConsigneeAddress1");
			this.textBoxConsigneeAddress1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1614562a-70d6-4f85-b7db-25cfdce59b8e", "Addr.");
			this.textBoxConsigneeAddress1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 68, true);
			this.textBoxConsigneeAddress1.Name = "textBoxConsigneeAddress1";
			this.textBoxConsigneeAddress1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxConsigneeAddress1.TabIndex = 2;
			// 
			// TextBoxConsigneeAddress2
			// 
			this.BindingSource.SetBindingMember(this.TextBoxConsigneeAddress2, "CusUSLVConsignments.ULB_ConsigneeAddress2");
			this.TextBoxConsigneeAddress2.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TextBoxConsigneeAddress2, false);
			this.TextBoxConsigneeAddress2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 94, true);
			this.TextBoxConsigneeAddress2.Name = "TextBoxConsigneeAddress2";
			this.TextBoxConsigneeAddress2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.TextBoxConsigneeAddress2.TabIndex = 3;
			// 
			// textBoxConsigneeName
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeName, "CusUSLVConsignments.ULB_ConsigneeName");
			this.textBoxConsigneeName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b263660d-cab4-4799-bce5-79e1e61964b7", "Name");
			this.textBoxConsigneeName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 42, true);
			this.textBoxConsigneeName.Name = "textBoxConsigneeName";
			this.textBoxConsigneeName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxConsigneeName.TabIndex = 1;
			// 
			// textBoxSellerState
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerState, "CusUSLVConsignments.ULB_SellerState");
			this.textBoxSellerState.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b16efae0-58da-46a1-9258-328d9845eb32", "State");
			this.textBoxSellerState.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 120, true);
			this.textBoxSellerState.Name = "textBoxSellerState";
			this.textBoxSellerState.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.textBoxSellerState.TabIndex = 5;
			// 
			// textBoxConsigneeState
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeState, "CusUSLVConsignments.ULB_ConsigneeState");
			this.textBoxConsigneeState.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b16efae0-58da-46a1-9258-328d9845eb32", "State");
			this.textBoxConsigneeState.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 120, true);
			this.textBoxConsigneeState.Name = "textBoxConsigneeState";
			this.textBoxConsigneeState.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.textBoxConsigneeState.TabIndex = 5;
			// 
			// addressControlConsignee
			// 
			this.addressControlConsignee.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.addressControlConsignee, "CusUSLVConsignments.ULB_OA_Consignee");
			this.addressControlConsignee.BindToOrgList = "CusUSLVConsignments.Lookups+ConsigneeOrgList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.addressControlConsignee, false);
			this.addressControlConsignee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.addressControlConsignee.Name = "addressControlConsignee";
			this.addressControlConsignee.PopupCaption = "";
			this.addressControlConsignee.ReadOnly = false;
			this.addressControlConsignee.ShowAddress = false;
			this.addressControlConsignee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.addressControlConsignee.TabIndex = 0;
			// 
			// groupBoxCustomsInfo
			// 
			this.groupBoxCustomsInfo.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("7562cb46-fe5e-43db-8b7f-b92fa9b30820", "Customs Info");
			this.groupBoxCustomsInfo.Controls.Add(this.dateEditLatestMsgStatusDate);
			this.groupBoxCustomsInfo.Controls.Add(this.dateEditReleaseDate);
			this.groupBoxCustomsInfo.Controls.Add(this.dateEditSubmittedDate);
			this.groupBoxCustomsInfo.Controls.Add(this.dropEditMessageStatus);
			this.groupBoxCustomsInfo.Controls.Add(this.dropEditReleaseStatus);
			this.groupBoxCustomsInfo.Controls.Add(this.groupBoxDisposition);
			this.groupBoxCustomsInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxCustomsInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 3, true);
			this.groupBoxCustomsInfo.Name = "groupBoxCustomsInfo";
			this.groupBoxCustomsInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 294, true);
			this.groupBoxCustomsInfo.TabIndex = 2;
			this.groupBoxCustomsInfo.TabStop = false;
			// 
			// dropEditReleaseStatus
			// 
			this.dropEditReleaseStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditReleaseStatus, "CusUSLVConsignments.CE_EntryStatus");
			this.dropEditReleaseStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("40ac423d-8573-4655-b9f4-9f858c881600", "Release Status");
			this.dropEditReleaseStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 29, true);
			this.dropEditReleaseStatus.Name = "dropEditCustomsStatus";
			this.dropEditReleaseStatus.PreBoundMaxLength = 3;
			this.dropEditReleaseStatus.ShouldResizeByMaxLength = true;
			this.dropEditReleaseStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.dropEditReleaseStatus.TabIndex = 0;
			// 
			// dropEditMessageStatus
			// 
			this.dropEditMessageStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditMessageStatus, "CusUSLVConsignments.ULB_MessageStatus");
			this.dropEditMessageStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5ced49f6-ca96-436c-8d56-fabb19fecb5e", "Message Status");
			this.dropEditMessageStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 50, true);
			this.dropEditMessageStatus.Name = "dropEditMessageStatus";
			this.dropEditMessageStatus.PreBoundMaxLength = 3;
			this.dropEditMessageStatus.ShouldResizeByMaxLength = true;
			this.dropEditMessageStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.dropEditMessageStatus.TabIndex = 1;
			// 
			// dateEditSubmittedDate
			// 
			this.dateEditSubmittedDate.AllowDrop = true;
			this.dateEditSubmittedDate.AutoCompleteMonthThreshold = 1;
			this.dateEditSubmittedDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditSubmittedDate, "CusUSLVConsignments.ULB_SubmittedDate");
			this.dateEditSubmittedDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("daeb1e85-09f4-4534-8d86-abfd7cee27f0", "Submitted Date");
			this.dateEditSubmittedDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 8, true);
			this.dateEditSubmittedDate.Name = "dateEditSubmittedDate";
			this.dateEditSubmittedDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.dateEditSubmittedDate.TabIndex = 2;
			// 
			// dateEditReleaseDate
			// 
			this.dateEditReleaseDate.AllowDrop = true;
			this.dateEditReleaseDate.AutoCompleteMonthThreshold = 1;
			this.dateEditReleaseDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditReleaseDate, "CusUSLVConsignments.CE_IssueDate");
			this.dateEditReleaseDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("afc38fcc-10af-4c0e-ba4f-62829d3c81c9", "Release Date");
			this.dateEditReleaseDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 29, true);
			this.dateEditReleaseDate.Name = "dateEditReleaseDate";
			this.dateEditReleaseDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.dateEditReleaseDate.TabIndex = 3;
			//
			// dateEditLatestMsgStatusDate
			//
			this.dateEditLatestMsgStatusDate.AllowDrop = true;
			this.dateEditLatestMsgStatusDate.AutoCompleteMonthThreshold = 1;
			this.dateEditLatestMsgStatusDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditLatestMsgStatusDate, "CusUSLVConsignments.LatestMsgStatusDate");
			this.dateEditLatestMsgStatusDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e84d12b5-0673-4eb1-819f-06db38ea3798", "Msg. Status Date");
			this.dateEditLatestMsgStatusDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 50, true);
			this.dateEditLatestMsgStatusDate.Name = "dateEditLatestMsgStatusDate";
			this.dateEditLatestMsgStatusDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.dateEditLatestMsgStatusDate.TabIndex = 4;
			//
			// groupBoxDisposition
			//
			this.groupBoxDisposition.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("643aae9c-ac47-42bb-a134-6a32ff5ce897", "Disposition");
			this.groupBoxDisposition.Controls.Add(this.gridDisposition);
			this.groupBoxDisposition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 70, true);
			this.groupBoxDisposition.Name = "groupBoxDisposition";
			this.groupBoxDisposition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 120, true);
			this.groupBoxDisposition.TabIndex = 9;
			this.groupBoxDisposition.TabStop = false;
			this.groupBoxDisposition.Text = "Disposition";
			// 
			// gridDisposition
			// 
			this.gridDisposition.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridDisposition, "CusUSLVConsignments.DispositionCodesView");
			this.gridDisposition.CaptionVisible = false;
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.gridDisposition.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.gridDisposition.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.gridDisposition.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridDisposition.GridId = "71e70aa9-ff88-4463-adfd-b19754419578";
			this.gridDisposition.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridDisposition.LayoutKey = "DispositionGrid";
			this.gridDisposition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridDisposition.Name = "gridDisposition";
			this.gridDisposition.ReadOnly = true;
			this.gridDisposition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 150, true);
			this.gridDisposition.TabIndex = 0;
			this.tableLayoutPanelSummary.ResumeLayout(false);
			this.tableLayoutPanelSummary.PerformLayout();
			this.groupBoxSeller.ResumeLayout(false);
			this.groupBoxSeller.PerformLayout();
			this.addressControlSeller.ResumeLayout(true);
			this.addressControlSeller.PerformLayout();
			this.codeFindBoxSellerCountry.ResumeLayout(true);
			this.codeFindBoxSellerCountry.PerformLayout();
			this.groupBoxConsignee.ResumeLayout(false);
			this.groupBoxConsignee.PerformLayout();
			this.codeFindBoxConsigneeCountry.ResumeLayout(true);
			this.codeFindBoxConsigneeCountry.PerformLayout();
			this.addressControlConsignee.ResumeLayout(true);
			this.addressControlConsignee.PerformLayout();
			this.groupBoxCustomsInfo.ResumeLayout(false);
			this.groupBoxCustomsInfo.PerformLayout();
			this.gridDisposition.ResumeLayout(true);
			this.gridDisposition.PerformLayout();
			this.groupBoxDisposition.ResumeLayout(true);
			this.groupBoxDisposition.PerformLayout();
			this.dateEditLatestMsgStatusDate.ResumeLayout(true);
			this.dateEditLatestMsgStatusDate.PerformLayout();
			this.dateEditSubmittedDate.ResumeLayout(true);
			this.dateEditSubmittedDate.PerformLayout();
			this.dateEditReleaseDate.ResumeLayout(true);
			this.dateEditReleaseDate.PerformLayout();
			this.dropEditMessageStatus.ResumeLayout(true);
			this.dropEditMessageStatus.PerformLayout();
			this.dropEditReleaseStatus.ResumeLayout(true);
			this.dropEditReleaseStatus.PerformLayout();

		}

		private void tabPageHouseBillDetails_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo partNoColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.US.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.US.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.groupBoxCommodityDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainerHouseBillDetails = new CargoWise.Windows.UI.KSplitContainer();
			this.tabPageHouseBillDetails.Controls.Add(this.splitContainerHouseBillDetails);
			this.gridCommodityDetails = new Enterprise.ZArchitecture.ZGrid();
			this.pgaRequirementsControl = new Enterprise.Customs.US.LVS.GUI.OGAPGARequirementsControl();
			this.groupBoxCommodityDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridCommodityDetails)).BeginInit();
			this.gridCommodityDetails.SuspendLayout();
			this.pgaRequirementsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHouseBillDetails)).BeginInit();
			this.splitContainerHouseBillDetails.Panel1.SuspendLayout();
			this.splitContainerHouseBillDetails.Panel2.SuspendLayout();
			this.splitContainerHouseBillDetails.SuspendLayout();
			// 
			// groupBoxCommodityDetails
			// 
			this.groupBoxCommodityDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("f8fbbd7e-cdd0-408d-9f5f-152b98ced16a", "Commodity Details");
			this.groupBoxCommodityDetails.Controls.Add(this.gridCommodityDetails);
			this.groupBoxCommodityDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxCommodityDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.groupBoxCommodityDetails.Name = "groupBoxCommodityDetails";
			this.groupBoxCommodityDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1368, 185, true);
			this.groupBoxCommodityDetails.TabIndex = 97;
			this.groupBoxCommodityDetails.TabStop = false;
			this.groupBoxCommodityDetails.Text = "Commodity Details";
			// 
			// pgaRequirementsControl
			// 
			this.pgaRequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pgaRequirementsControl, "CusUSLVConsignments.CusUSLVItems");
			this.pgaRequirementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pgaRequirementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.pgaRequirementsControl.Name = "pgaRequirementsControl";
			this.pgaRequirementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.pgaRequirementsControl.TabIndex = 0;
			this.pgaRequirementsControl.TabStop = false;
			// 
			// gridCommodityDetails
			// 
			this.gridCommodityDetails.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridCommodityDetails, "CusUSLVConsignments.CusUSLVItems");
			this.gridCommodityDetails.CaptionVisible = false;
			partNoColumnStyleInfo.ColumnName = "ULI_PartNo";
			partNoColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tariffColumnStyleInfo1.ColumnName = "ULI_TariffFormatted";
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo21.ColumnName = "ULI_GoodsDescription";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo21.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ULI_GoodsValue";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "ULI_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "ULI_RX_NKCurrEXRate";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo7.ColumnName = "ULI_RN_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "ULI_AntiDumping";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "ULI_Countervailing";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridCommodityDetails.ColumnStyles.Add(partNoColumnStyleInfo);
			this.gridCommodityDetails.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.gridCommodityDetails.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.gridCommodityDetails.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.gridCommodityDetails.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.gridCommodityDetails.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.gridCommodityDetails.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.gridCommodityDetails.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.gridCommodityDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridCommodityDetails.GridId = "b2206c31-fd8f-4fda-8628-c5667ab9fab5";
			this.gridCommodityDetails.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridCommodityDetails.LayoutKey = "gridHouseBills";
			this.gridCommodityDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridCommodityDetails.Name = "gridCommodityDetails";
			this.gridCommodityDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(771, 265, true);
			this.gridCommodityDetails.TabIndex = 0;
			// 
			// splitContainerHouseBillDetails
			//
			this.splitContainerHouseBillDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerHouseBillDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.splitContainerHouseBillDetails.Name = "splitContainerHouseBillDetails";
			// 
			// splitContainerHouseBillDetails.Panel1
			// 
			this.splitContainerHouseBillDetails.Panel1.Controls.Add(this.groupBoxCommodityDetails);
			this.splitContainerHouseBillDetails.Panel1MinSize = 74;
			// 
			// splitContainerHouseBillDetails.Panel2
			// 
			this.splitContainerHouseBillDetails.Panel2.Controls.Add(this.pgaRequirementsControl);
			this.splitContainerHouseBillDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1572, 265, true);
			this.splitContainerHouseBillDetails.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.splitContainerHouseBillDetails.TabIndex = 1;
			this.splitContainerHouseBillDetails.Panel2MinSize = 74;
			this.splitContainerHouseBillDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;

			this.groupBoxCommodityDetails.ResumeLayout(false);
			this.groupBoxCommodityDetails.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridCommodityDetails)).EndInit();
			this.gridCommodityDetails.ResumeLayout(false);
			this.gridCommodityDetails.PerformLayout();
			this.splitContainerHouseBillDetails.Panel1.ResumeLayout(false);
			this.splitContainerHouseBillDetails.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerHouseBillDetails)).EndInit();
			this.splitContainerHouseBillDetails.ResumeLayout(false);
			this.splitContainerHouseBillDetails.PerformLayout();
			this.pgaRequirementsControl.ResumeLayout(true);
			this.pgaRequirementsControl.PerformLayout();
		}

		private void OrganisationsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.dropRegistrationType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.organizationFindBoxClient = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.organizationFindBoxImporter = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.textBoxRegistrationNum = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationsTabPage.Controls.Add(this.organizationFindBoxClient);
			this.OrganisationsTabPage.Controls.Add(this.organizationFindBoxImporter);
			this.OrganisationsTabPage.Controls.Add(this.textBoxRegistrationNum);
			this.OrganisationsTabPage.Controls.Add(this.dropRegistrationType);
			this.dropRegistrationType.SuspendLayout();
			this.organizationFindBoxClient.SuspendLayout();
			this.organizationFindBoxImporter.SuspendLayout();
			// 
			// dropRegistrationType
			// 
			this.dropRegistrationType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropRegistrationType, "ULH_IORType");
			this.dropRegistrationType.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5f10fc4d-81a0-47e2-a08b-330434ac1486", "Registration Type");
			this.dropRegistrationType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 58, true);
			this.dropRegistrationType.Name = "dropRegistrationType";
			this.dropRegistrationType.ShowDescriptionBox = false;
			this.dropRegistrationType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.dropRegistrationType.TabIndex = 2;
			// 
			// organizationFindBoxClient
			// 
			this.organizationFindBoxClient.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organizationFindBoxClient, "ULH_OH_Client");
			this.organizationFindBoxClient.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("c1dc390a-daad-4e5b-a85f-21870ea1a4b6", "Client");
			this.organizationFindBoxClient.IsPrimaryKeyFromCodeRequired = false;
			this.organizationFindBoxClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 6, true);
			this.organizationFindBoxClient.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.organizationFindBoxClient.Name = "organizationFindBoxClient";
			this.organizationFindBoxClient.ShouldResize = true;
			this.organizationFindBoxClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.organizationFindBoxClient.TabIndex = 0;
			// 
			// organizationFindBoxImporter
			// 
			this.organizationFindBoxImporter.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.organizationFindBoxImporter, "ULH_OH_Importer");
			this.organizationFindBoxImporter.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1956059b-f620-4a96-abac-2f5f6b773c67", "Importer of Record");
			this.organizationFindBoxImporter.IsPrimaryKeyFromCodeRequired = false;
			this.organizationFindBoxImporter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 32, true);
			this.organizationFindBoxImporter.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.organizationFindBoxImporter.Name = "organizationFindBoxImporter";
			this.organizationFindBoxImporter.ShouldResize = true;
			this.organizationFindBoxImporter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.organizationFindBoxImporter.TabIndex = 1;
			// 
			// textBoxRegistrationNum
			// 
			this.BindingSource.SetBindingMember(this.textBoxRegistrationNum, "ULH_IORReference");
			this.textBoxRegistrationNum.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("9fd8d810-613d-4150-862e-f4a3b941accc", "Registration Number");
			this.textBoxRegistrationNum.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 84, true);
			this.textBoxRegistrationNum.Name = "textBoxRegistrationNum";
			this.textBoxRegistrationNum.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.textBoxRegistrationNum.TabIndex = 3;
			this.dropRegistrationType.ResumeLayout(true);
			this.dropRegistrationType.PerformLayout();
			this.organizationFindBoxClient.ResumeLayout(true);
			this.organizationFindBoxClient.PerformLayout();
			this.organizationFindBoxImporter.ResumeLayout(true);
			this.organizationFindBoxImporter.PerformLayout();

		}

		private void MiscTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.tableLayoutPanelMisc = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.groupBoxFiler = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.textBoxFiler = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxContactName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxContactPhone = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxMatchingKey = new Enterprise.ZArchitecture.ZTextBox();
			this.guidFindBoxBranch = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.miscTabPage.Controls.Add(this.tableLayoutPanelMisc);
			this.miscTabPage.Controls.Add(this.textBoxMatchingKey);
			this.miscTabPage.Controls.Add(this.guidFindBoxBranch);
			this.groupBoxFiler.SuspendLayout();
			this.textBoxFiler.SuspendLayout();
			this.tableLayoutPanelMisc.SuspendLayout();
			this.textBoxContactName.SuspendLayout();
			this.textBoxContactPhone.SuspendLayout();
			this.textBoxMatchingKey.SuspendLayout();
			this.guidFindBoxBranch.SuspendLayout();
			// 
			// tableLayoutPanelMisc
			// 
			this.tableLayoutPanelMisc.ColumnCount = 2;
			this.tableLayoutPanelMisc.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMisc.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMisc.Controls.Add(this.groupBoxFiler);
			this.tableLayoutPanelMisc.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanelMisc.Name = "tableLayoutPanelMisc";
			this.tableLayoutPanelMisc.RowCount = 1;
			this.tableLayoutPanelMisc.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMisc.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 106, true);
			this.tableLayoutPanelMisc.TabIndex = 0;
			// 
			// groupBoxFiler
			// 
			this.groupBoxFiler.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("4b1f0025-23d3-4ab3-9ed5-b0b98c721daf", "Filer");
			this.groupBoxFiler.Controls.Add(this.textBoxFiler);
			this.groupBoxFiler.Controls.Add(this.textBoxContactName);
			this.groupBoxFiler.Controls.Add(this.textBoxContactPhone);
			this.groupBoxFiler.Dock = System.Windows.Forms.DockStyle.None;
			this.groupBoxFiler.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 6, true);
			this.groupBoxFiler.Name = "groupBoxFiler";
			this.groupBoxFiler.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 96, true);
			this.groupBoxFiler.TabIndex = 0;
			this.groupBoxFiler.TabStop = false;
			// 
			// textBoxFiler
			// 
			this.BindingSource.SetBindingMember(this.textBoxFiler, "ULH_EntryFilerCode");
			this.textBoxFiler.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("4b1f0025-23d3-4ab3-9ed5-b0b98c721daf", "Filer");
			this.textBoxFiler.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 12, true);
			this.textBoxFiler.Name = "textBoxFiler";
			this.textBoxFiler.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.textBoxFiler.TabIndex = 0;
			// 
			// textBoxContactName
			// 
			this.BindingSource.SetBindingMember(this.textBoxContactName, "ULH_ContactName");
			this.textBoxContactName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("4e48d231-1898-4714-b71b-4ab100d9e82d", "Contact Name");
			this.textBoxContactName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 38, true);
			this.textBoxContactName.Name = "textBoxContactName";
			this.textBoxContactName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.textBoxContactName.TabIndex = 1;
			// 
			// textBoxContactPhone
			// 
			this.BindingSource.SetBindingMember(this.textBoxContactPhone, "ULH_ContactPhone");
			this.textBoxContactPhone.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("4208575a-b4d8-42f1-80d4-8ef5fa3e81eb", "Contact Phone");
			this.textBoxContactPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.textBoxContactPhone.Name = "textBoxContactPhone";
			this.textBoxContactPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.textBoxContactPhone.TabIndex = 2;
			// 
			// textBoxMatchingKey
			// 
			this.BindingSource.SetBindingMember(this.textBoxMatchingKey, "ULH_MatchingKey");
			this.textBoxMatchingKey.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 108, true);
			this.textBoxMatchingKey.Name = "textBoxMatchingKey";
			this.textBoxMatchingKey.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.textBoxMatchingKey.TabIndex = 1;
			// 
			// guidFindBoxBranch
			//
			this.guidFindBoxBranch.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.guidFindBoxBranch, "ULH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(null)).ULH_GB)));
			this.guidFindBoxBranch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 134, true);
			this.guidFindBoxBranch.Name = "guidFindBoxBranch";
			this.guidFindBoxBranch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.guidFindBoxBranch.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.guidFindBoxBranch.PreBoundMaxLength = 3;
			this.guidFindBoxBranch.TabIndex = 2;
			this.tableLayoutPanelMisc.ResumeLayout(false);
			this.tableLayoutPanelMisc.PerformLayout();
			this.groupBoxFiler.ResumeLayout(false);
			this.groupBoxFiler.PerformLayout();
			this.textBoxFiler.ResumeLayout(false);
			this.textBoxFiler.PerformLayout();
			this.textBoxContactName.ResumeLayout(false);
			this.textBoxContactName.PerformLayout();
			this.textBoxContactPhone.ResumeLayout(false);
			this.textBoxContactPhone.PerformLayout();
			this.textBoxMatchingKey.ResumeLayout(false);
			this.textBoxMatchingKey.PerformLayout();
			this.guidFindBoxBranch.ResumeLayout(false);
			this.guidFindBoxBranch.PerformLayout();
		}
	}
}
