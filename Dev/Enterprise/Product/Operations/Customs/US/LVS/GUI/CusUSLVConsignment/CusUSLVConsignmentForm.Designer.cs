using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.US.LVS.GUI
{
	partial class CusUSLVConsignmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabPageCommodities = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageStatus = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageMessages = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.tabPageCommodities);
			this.MainTabControl.Controls.Add(this.tabPageStatus);
			this.MainTabControl.Controls.Add(this.tabPageMessages);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1072, 381, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.tabPageMessages, 0);
			this.MainTabControl.Controls.SetChildIndex(this.tabPageStatus, 0);
			this.MainTabControl.Controls.SetChildIndex(this.tabPageCommodities, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ZTemplateForm|ef0328ba-b3df-4f31-9369-171938fde0b7", "Bill Details");
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 374, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 374, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 374, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1072, 381, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1072, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVConsignment);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_OA_Seller)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerAddress1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerAddress2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerCity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerState)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_SellerPostCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_RN_NKSellerCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_OA_Consignee)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneeName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneeAddress1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneeAddress2)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneeCity)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneeState)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_ConsigneePostCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_RN_NKConsigneeCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_OH_Client)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_OH_Importer)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_TariffFormatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_RN_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_RX_NKCurrEXRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_AntiDumping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).FirstCusUSLVItem.ULI_Countervailing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).HasMultipleItemLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).HasAtLeastOnePGARequirementOnAnyItemLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_EntryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_PortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_PortOfEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_DepartureDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_DischargeDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_CarrierSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_VoyageFlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_VoyageFlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_ConveyanceName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_MasterBillIssuerSCAC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_Calc_USTransportMode)));
			// 
			// tabPageCommodities
			// 
			this.tabPageCommodities.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageCommodities.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("406e1919-fa4b-48e3-9216-5bceee522bce", "Commodities");
			this.tabPageCommodities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageCommodities.Name = "tabPageCommodities";
			this.tabPageCommodities.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageCommodities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 374, true);
			this.tabPageCommodities.TabIndex = 4;
			this.tabPageCommodities.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageCommodities_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_TariffFormatted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RX_NKCurrEXRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_RN_NKCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_AntiDumping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVItem)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).CusUSLVItems)).SyncRoot)).ULI_Countervailing)));
			// 
			// tabPageStatus
			// 
			this.tabPageStatus.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("7bb7b032-e774-45cd-ac38-91f135374c41", "Status");
			this.tabPageStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageStatus.Name = "tabPageStatus";
			this.tabPageStatus.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 374, true);
			this.tabPageStatus.TabIndex = 5;
			this.tabPageStatus.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageStatus_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).ErrorMessageIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).NarrativeMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).StatusDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).ReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).ReleaseOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.ErrorsRecord)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.CusUSLVConsignments)).SyncRoot)).DispositionCodesView)).SyncRoot)).ReleaseOriginDescription)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).LatestMsgStatusDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_IssueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_EntryStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_MessageStatus)));
			// 
			// tabPageMessages
			// 
			this.tabPageMessages.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("42dc337b-bb2a-4e0b-b999-018e3d248903", "Messages");
			this.tabPageMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageMessages.Name = "tabPageMessages";
			this.tabPageMessages.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 354, true);
			this.tabPageMessages.TabIndex = 6;
			this.tabPageMessages.UseVisualStyleBackColor = true;
			this.tabPageMessages.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageMessages_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_User)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.StatusErrorsDataViewCollection)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Messages)).SyncRoot)).StatusesAndErrors)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_HouseBill)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_EquipmentNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_GoodsValue)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_Currency)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_OwnerReferenceNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_HouseBillIssuerSCAC)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_PackType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_NonAMSIndicator)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_RailReferenceNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_OH_Client)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_OH_Importer)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.LVS.Business.CusUSLVClearance)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_EntryNum)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_IssueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).LatestMsgStatusDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_EntryFilerCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_US_NKLocationOfGoods)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_US_NKCentralizedExamSite)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_ContactName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).Shipment.ULH_ContactPhone)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).CE_EntryStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_MessageStatus)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ULB_NumberOfPacks)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).ConsigneeOrgPK)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.LVS.Business.CusUSLVConsignment)(null)).SellerOrgPK)));
			// 
			// CusUSLVConsignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1072, 696, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.LVS.Business.CusUSLVConsignment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 707, true);
			this.Name = "CusUSLVConsignmentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
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

		private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Code generated by Windows Form Designer.")]
		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.tableLayoutPanelBillDetails = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.groupBoxTransportDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBoxConsignmentDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBoxLineDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBoxSummary = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.panelLocationDate = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.dateEditArrival = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxLoadingPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxEntryPort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dateEditDeparture = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxDischargeUNLOCO = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxLoadingUNLOCO = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dateEditDischarge = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.codeFindBoxDischargePort = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.panelTransportDetailGroup = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.codeFindBoxCarrierSCAC = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.textBoxFlightNo = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxVoyageNo = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxTripID = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxJourney = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxVessel = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.masterBillControl = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.textBoxOceanBill = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxMailReference = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxMasterBill = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxIssuerSCAC = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.dropEditContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dropEditModeOfTransport = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.textBoxCalculatedModeOfTransport = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxHouseBill = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxContainer = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeId = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerId = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxGoodsValue = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxCurrency = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxOwnerRef = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxConsignmentIssuerSCAC = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxUOM = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxCentralizedExamSite = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxLocationOfGoods = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.textBoxRailReference = new Enterprise.ZArchitecture.ZTextBox();
			this.orgFindBoxMainTabClient = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.orgFindBoxMainTabImporter = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.textBoxEntryNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.dateEditIssueDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateEditMessageDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.textBoxEntryFilerCode = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxContactName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxContactPhone = new Enterprise.ZArchitecture.ZTextBox();
			this.dropEditMainTabEntryStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dropEditMainTabMessageStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.calcEditQuantity = new Enterprise.ZArchitecture.ZCalcEdit();
			this.groupBoxSeller = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBoxConsignee = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.addressControlSeller = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.textBoxSellerName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerAddress1 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerAddress2 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerCity = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerState = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxSellerPostCode = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxSellerCountry = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.addressControlConsignee = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.textBoxConsigneeName = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeAddress1 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeAddress2 = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeCity = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneeState = new Enterprise.ZArchitecture.ZTextBox();
			this.textBoxConsigneePostCode = new Enterprise.ZArchitecture.ZTextBox();
			this.codeFindBoxConsigneeCountry = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxProduct = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.codeFindBoxTariff = new Enterprise.Customs.GUI.TariffFindBox();
			this.textBoxGoodsDescription = new Enterprise.ZArchitecture.ZTextBox();
			this.calcEditLineValue = new Enterprise.ZArchitecture.ZCalcEdit();
			this.codeFindBoxLineCurrency = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.calcEditExchangeRate = new Enterprise.ZArchitecture.ZCalcEdit();
			this.codeFindBoxCountryOfOrigin = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.checkBoxADD = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBoxCVD = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBoxMultipleLines = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.checkBoxPGARequirements = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabPage.SuspendLayout();
			this.tableLayoutPanelBillDetails.SuspendLayout();
			this.groupBoxTransportDetails.SuspendLayout();
			this.groupBoxConsignmentDetails.SuspendLayout();
			this.groupBoxLineDetails.SuspendLayout();
			this.groupBoxSeller.SuspendLayout();
			this.panelLocationDate.SuspendLayout();
			this.dateEditArrival.SuspendLayout();
			this.codeFindBoxLoadingPort.SuspendLayout();
			this.codeFindBoxEntryPort.SuspendLayout();
			this.dateEditDeparture.SuspendLayout();
			this.codeFindBoxDischargeUNLOCO.SuspendLayout();
			this.codeFindBoxLoadingUNLOCO.SuspendLayout();
			this.dateEditDischarge.SuspendLayout();
			this.codeFindBoxDischargePort.SuspendLayout();
			this.panelTransportDetailGroup.SuspendLayout();
			this.codeFindBoxCarrierSCAC.SuspendLayout();
			this.codeFindBoxVessel.SuspendLayout();
			this.masterBillControl.SuspendLayout();
			this.codeFindBoxIssuerSCAC.SuspendLayout();
			this.dropEditContainerMode.SuspendLayout();
			this.dropEditModeOfTransport.SuspendLayout();
			this.addressControlSeller.SuspendLayout();
			this.codeFindBoxSellerCountry.SuspendLayout();
			this.addressControlConsignee.SuspendLayout();
			this.codeFindBoxConsigneeCountry.SuspendLayout();
			this.groupBoxConsignee.SuspendLayout();
			this.groupBoxSummary.SuspendLayout();
			this.orgFindBoxMainTabClient.SuspendLayout();
			this.codeFindBoxCentralizedExamSite.SuspendLayout();
			this.orgFindBoxMainTabImporter.SuspendLayout();
			this.dateEditIssueDate.SuspendLayout();
			this.dateEditMessageDate.SuspendLayout();
			this.dropEditMainTabEntryStatus.SuspendLayout();
			this.dropEditMainTabMessageStatus.SuspendLayout();
			this.codeFindBoxLocationOfGoods.SuspendLayout();
			this.MainTabPage.Controls.Add(this.tableLayoutPanelBillDetails);
			// 
			// tableLayoutPanelBillDetails
			// 
			this.tableLayoutPanelBillDetails.ColumnCount = 1;
			this.tableLayoutPanelBillDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelBillDetails.Controls.Add(this.groupBoxTransportDetails, 0, 0);
			this.tableLayoutPanelBillDetails.Controls.Add(this.groupBoxSummary, 0, 1);
			this.tableLayoutPanelBillDetails.Controls.Add(this.groupBoxConsignmentDetails, 0, 2);
			this.tableLayoutPanelBillDetails.Controls.Add(this.groupBoxLineDetails, 0, 3);
			this.tableLayoutPanelBillDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanelBillDetails.Name = "tableLayoutPanelBillDetails";
			this.tableLayoutPanelBillDetails.RowCount = 4;
			this.tableLayoutPanelBillDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBillDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBillDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBillDetails.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelBillDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 565, true);
			this.tableLayoutPanelBillDetails.TabIndex = 0;
			// 
			// groupBoxTransportDetails
			// 
			this.groupBoxTransportDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("57632923-254b-4ef0-b5ef-8995cd8bd60f", "Transport Details");
			this.groupBoxTransportDetails.Controls.Add(this.panelLocationDate);
			this.groupBoxTransportDetails.Controls.Add(this.panelTransportDetailGroup);
			this.groupBoxTransportDetails.Controls.Add(this.dropEditContainerMode);
			this.groupBoxTransportDetails.Controls.Add(this.dropEditModeOfTransport);
			this.groupBoxTransportDetails.Controls.Add(this.textBoxCalculatedModeOfTransport);
			this.groupBoxTransportDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxTransportDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxTransportDetails.Name = "groupBoxTransportDetails";
			this.groupBoxTransportDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 150, true);
			this.groupBoxTransportDetails.TabIndex = 0;
			this.groupBoxTransportDetails.TabStop = false;
			// 
			// groupBoxSummary
			// 
			this.groupBoxSummary.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("f33b8080-eb0b-46b2-9485-0372d54e052d", "Summary");
			this.tableLayoutPanelBillDetails.SetColumnSpan(this.groupBoxSummary, 2);
			this.groupBoxSummary.Controls.Add(this.dropEditMainTabMessageStatus);
			this.groupBoxSummary.Controls.Add(this.codeFindBoxLocationOfGoods);
			this.groupBoxSummary.Controls.Add(this.dropEditMainTabEntryStatus);
			this.groupBoxSummary.Controls.Add(this.textBoxContactPhone);
			this.groupBoxSummary.Controls.Add(this.textBoxContactName);
			this.groupBoxSummary.Controls.Add(this.textBoxEntryFilerCode);
			this.groupBoxSummary.Controls.Add(this.dateEditMessageDate);
			this.groupBoxSummary.Controls.Add(this.dateEditIssueDate);
			this.groupBoxSummary.Controls.Add(this.textBoxEntryNumber);
			this.groupBoxSummary.Controls.Add(this.orgFindBoxMainTabClient);
			this.groupBoxSummary.Controls.Add(this.codeFindBoxCentralizedExamSite);
			this.groupBoxSummary.Controls.Add(this.orgFindBoxMainTabImporter);
			this.groupBoxSummary.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxSummary.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 155, true);
			this.groupBoxSummary.Name = "groupBoxSummary";
			this.groupBoxSummary.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 100, true);
			this.groupBoxSummary.TabIndex = 2;
			this.groupBoxSummary.TabStop = false;
			// 
			// groupBoxConsignmentDetails
			// 
			this.groupBoxConsignmentDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("6791d888-d45d-461b-a652-577a5de5be58", "House Bill Details");
			this.groupBoxConsignmentDetails.Controls.Add(this.calcEditQuantity);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxRailReference);
			this.groupBoxConsignmentDetails.Controls.Add(this.codeFindBoxUOM);
			this.groupBoxConsignmentDetails.Controls.Add(this.codeFindBoxConsignmentIssuerSCAC);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxOwnerRef);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxCurrency);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxGoodsValue);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxContainer);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxConsigneeId);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxSellerId);
			this.groupBoxConsignmentDetails.Controls.Add(this.textBoxHouseBill);
			this.groupBoxConsignmentDetails.Controls.Add(this.groupBoxSeller);
			this.groupBoxConsignmentDetails.Controls.Add(this.groupBoxConsignee);
			this.groupBoxConsignmentDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxConsignmentDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 260, true);
			this.groupBoxConsignmentDetails.Name = "groupBoxConsignmentDetails";
			this.groupBoxConsignmentDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 195, true);
			this.groupBoxConsignmentDetails.TabIndex = 0;
			this.groupBoxConsignmentDetails.TabStop = false;
			// 
			// groupBoxLineDetails
			// 
			this.groupBoxLineDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("9cee55c4-a798-4a89-afaf-9cc3a0f2af86", "Commodity Details");
			this.groupBoxLineDetails.Controls.Add(this.codeFindBoxProduct);
			this.groupBoxLineDetails.Controls.Add(this.codeFindBoxTariff);
			this.groupBoxLineDetails.Controls.Add(this.textBoxGoodsDescription);
			this.groupBoxLineDetails.Controls.Add(this.calcEditLineValue);
			this.groupBoxLineDetails.Controls.Add(this.codeFindBoxLineCurrency);
			this.groupBoxLineDetails.Controls.Add(this.calcEditExchangeRate);
			this.groupBoxLineDetails.Controls.Add(this.codeFindBoxCountryOfOrigin);
			this.groupBoxLineDetails.Controls.Add(this.checkBoxADD);
			this.groupBoxLineDetails.Controls.Add(this.checkBoxCVD);
			this.groupBoxLineDetails.Controls.Add(this.checkBoxMultipleLines);
			this.groupBoxLineDetails.Controls.Add(this.checkBoxPGARequirements);
			this.groupBoxLineDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxLineDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 460, true);
			this.groupBoxLineDetails.Name = "groupBoxLineDetails";
			this.groupBoxLineDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 90, true);
			this.groupBoxLineDetails.TabIndex = 0;
			this.groupBoxLineDetails.TabStop = false;
			// 
			// panelLocationDate
			// 
			this.panelLocationDate.Controls.Add(this.dateEditArrival);
			this.panelLocationDate.Controls.Add(this.codeFindBoxLoadingPort);
			this.panelLocationDate.Controls.Add(this.codeFindBoxEntryPort);
			this.panelLocationDate.Controls.Add(this.dateEditDeparture);
			this.panelLocationDate.Controls.Add(this.codeFindBoxDischargeUNLOCO);
			this.panelLocationDate.Controls.Add(this.codeFindBoxLoadingUNLOCO);
			this.panelLocationDate.Controls.Add(this.dateEditDischarge);
			this.panelLocationDate.Controls.Add(this.codeFindBoxDischargePort);
			this.panelLocationDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 38, true);
			this.panelLocationDate.Name = "panelLocationDate";
			this.panelLocationDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 80, true);
			this.panelLocationDate.TabIndex = 4;
			// 
			// dateEditArrival
			// 
			this.dateEditArrival.AllowDrop = true;
			this.dateEditArrival.AutoCompleteMonthThreshold = 1;
			this.dateEditArrival.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditArrival, "Shipment.ULH_EntryDate");
			this.dateEditArrival.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("362b5c78-316f-4a5d-898b-b1eb1c53414a", "Arr.");
			this.dateEditArrival.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 51, true);
			this.dateEditArrival.Name = "dateEditArrival";
			this.dateEditArrival.TabIndex = 21;
			// 
			// codeFindBoxLoadingPort
			// 
			this.codeFindBoxLoadingPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLoadingPort, "Shipment.ULH_PortOfLoading");
			this.codeFindBoxLoadingPort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("94d068f7-ec21-4c3b-82fd-db14636ef50e", "Loading");
			this.codeFindBoxLoadingPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 3, true);
			this.codeFindBoxLoadingPort.Name = "codeFindBoxLoadingPort";
			this.codeFindBoxLoadingPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxLoadingPort.TabIndex = 14;
			// 
			// codeFindBoxEntryPort
			// 
			this.codeFindBoxEntryPort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxEntryPort, "Shipment.ULH_PortOfEntry");
			this.codeFindBoxEntryPort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d2a220c1-096f-4b7f-b62e-9a1b4d5ae0fc", "Entry Port");
			this.codeFindBoxEntryPort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 51, true);
			this.codeFindBoxEntryPort.Name = "codeFindBoxEntryPort";
			this.codeFindBoxEntryPort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxEntryPort.TabIndex = 20;
			// 
			// dateEditDeparture
			// 
			this.dateEditDeparture.AllowDrop = true;
			this.dateEditDeparture.AutoCompleteMonthThreshold = 1;
			this.dateEditDeparture.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditDeparture, "Shipment.ULH_DepartureDate");
			this.dateEditDeparture.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("cb511dbe-564b-4853-9f31-f7cc6f7a45bf", "Dep.");
			this.dateEditDeparture.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 3, true);
			this.dateEditDeparture.Name = "dateEditDeparture";
			this.dateEditDeparture.TabIndex = 15;
			// 
			// codeFindBoxDischargeUNLOCO
			// 
			this.codeFindBoxDischargeUNLOCO.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxDischargeUNLOCO, "Shipment.ULH_RL_NKPortOfDischarge");
			this.codeFindBoxDischargeUNLOCO.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 27, true);
			this.codeFindBoxDischargeUNLOCO.Name = "codeFindBoxDischargeUNLOCO";
			this.codeFindBoxDischargeUNLOCO.PreBoundMaxLength = 5;
			this.codeFindBoxDischargeUNLOCO.ShowDescriptionBox = false;
			this.codeFindBoxDischargeUNLOCO.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.codeFindBoxDischargeUNLOCO.TabIndex = 19;
			// 
			// codeFindBoxLoadingUNLOCO
			// 
			this.codeFindBoxLoadingUNLOCO.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLoadingUNLOCO, "Shipment.ULH_RL_NKPortOfLoading");
			this.codeFindBoxLoadingUNLOCO.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 3, true);
			this.codeFindBoxLoadingUNLOCO.Name = "codeFindBoxLoadingUNLOCO";
			this.codeFindBoxLoadingUNLOCO.PreBoundMaxLength = 5;
			this.codeFindBoxLoadingUNLOCO.ShowDescriptionBox = false;
			this.codeFindBoxLoadingUNLOCO.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.codeFindBoxLoadingUNLOCO.TabIndex = 16;
			// 
			// dateEditDischarge
			// 
			this.dateEditDischarge.AllowDrop = true;
			this.dateEditDischarge.AutoCompleteMonthThreshold = 1;
			this.dateEditDischarge.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditDischarge, "Shipment.ULH_DischargeDate");
			this.dateEditDischarge.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2f4bf783-d92c-4d46-a5d9-b340e8e1537c", "Arr.");
			this.dateEditDischarge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 27, true);
			this.dateEditDischarge.Name = "dateEditDischarge";
			this.dateEditDischarge.TabIndex = 18;
			// 
			// codeFindBoxDischargePort
			// 
			this.codeFindBoxDischargePort.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxDischargePort, "Shipment.ULH_PortOfDischarge");
			this.codeFindBoxDischargePort.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("51692669-2705-42fa-b39d-c91df1c22d41", "Discharge");
			this.codeFindBoxDischargePort.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 27, true);
			this.codeFindBoxDischargePort.Name = "codeFindBoxDischargePort";
			this.codeFindBoxDischargePort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.codeFindBoxDischargePort.TabIndex = 17;
			// 
			// panelTransportDetailGroup
			//
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxCarrierSCAC);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxFlightNo);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxVoyageNo);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxTripID);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxJourney);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxVessel);
			this.panelTransportDetailGroup.Controls.Add(this.masterBillControl);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxOceanBill);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxMailReference);
			this.panelTransportDetailGroup.Controls.Add(this.textBoxMasterBill);
			this.panelTransportDetailGroup.Controls.Add(this.codeFindBoxIssuerSCAC);
			this.panelTransportDetailGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.panelTransportDetailGroup.Name = "panelTransportDetailGroup";
			this.panelTransportDetailGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 81, true);
			this.panelTransportDetailGroup.TabIndex = 3;
			// 
			// codeFindBoxCarrierSCAC
			// 
			this.codeFindBoxCarrierSCAC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxCarrierSCAC, "Shipment.ULH_CarrierSCAC");
			this.codeFindBoxCarrierSCAC.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0ab9722f-3428-4969-a619-db8b83e16ce0", "Carrier SCAC");
			this.codeFindBoxCarrierSCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 51, true);
			this.codeFindBoxCarrierSCAC.Name = "codeFindBoxCarrierSCAC";
			this.codeFindBoxCarrierSCAC.PreBoundMaxLength = 4;
			this.codeFindBoxCarrierSCAC.ShowDescriptionBox = false;
			this.codeFindBoxCarrierSCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.codeFindBoxCarrierSCAC.TabIndex = 13;
			// 
			// textBoxFlightNo
			// 
			this.BindingSource.SetBindingMember(this.textBoxFlightNo, "Shipment.ULH_VoyageFlightNo");
			this.textBoxFlightNo.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3f68f470-d611-404c-8f56-4f8ec84715ba", "Flight");
			this.textBoxFlightNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxFlightNo.Name = "textBoxFlightNo";
			this.textBoxFlightNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxFlightNo.TabIndex = 12;
			// 
			// textBoxVoyageNo
			// 
			this.BindingSource.SetBindingMember(this.textBoxVoyageNo, "Shipment.ULH_VoyageFlightNo");
			this.textBoxVoyageNo.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e8b2be41-321c-4796-861e-6613cfac8002", "Voyage");
			this.textBoxVoyageNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxVoyageNo.Name = "textBoxVoyageNo";
			this.textBoxVoyageNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxVoyageNo.TabIndex = 10;
			// 
			// textBoxTripID
			// 
			this.BindingSource.SetBindingMember(this.textBoxTripID, "Shipment.ULH_VoyageFlightNo");
			this.textBoxTripID.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e79be4a2-be1e-489b-b2d2-66ba5ab4542a", "Trip ID");
			this.textBoxTripID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 51, true);
			this.textBoxTripID.Name = "textBoxTripID";
			this.textBoxTripID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxTripID.TabIndex = 11;
			// 
			// textBoxJourney
			// 
			this.BindingSource.SetBindingMember(this.textBoxJourney, "Shipment.ULH_ConveyanceName");
			this.textBoxJourney.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3a784b14-f68d-4d6f-8d8c-8bf1e74a0d3b", "Journey");
			this.textBoxJourney.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 27, true);
			this.textBoxJourney.Name = "textBoxJourney";
			this.textBoxJourney.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.textBoxJourney.TabIndex = 9;
			// 
			// codeFindBoxVessel
			// 
			this.codeFindBoxVessel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxVessel, "Shipment.ULH_ConveyanceName");
			this.codeFindBoxVessel.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("a59876ad-dbb2-414f-8186-0b6bd0896bf6", "Vessel");
			this.codeFindBoxVessel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 27, true);
			this.codeFindBoxVessel.Name = "codeFindBoxVessel";
			this.codeFindBoxVessel.PreBoundMaxLength = 35;
			this.codeFindBoxVessel.ShowDescriptionBox = false;
			this.codeFindBoxVessel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.codeFindBoxVessel.TabIndex = 8;
			// 
			// masterBillControl
			// 
			this.masterBillControl.AllowAlphaInMAWP = false;
			this.masterBillControl.AllowDrop = true;
			this.masterBillControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.masterBillControl, "Shipment.ULH_MasterBill");
			this.masterBillControl.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("16915439-cf16-4298-adb8-c56d22926bc2", "Master Bill");
			this.masterBillControl.FormattedMasterBill = "";
			this.masterBillControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 3, true);
			this.masterBillControl.Name = "masterBillControl";
			this.masterBillControl.ReadOnly = false;
			this.masterBillControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.masterBillControl.TabIndex = 7;
			// 
			// textBoxOceanBill
			// 
			this.BindingSource.SetBindingMember(this.textBoxOceanBill, "Shipment.ULH_MasterBill");
			this.textBoxOceanBill.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("479ab654-2eb0-4bfa-bb2b-b902cc722195", "Ocean Bill");
			this.textBoxOceanBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 3, true);
			this.textBoxOceanBill.Name = "textBoxOceanBill";
			this.textBoxOceanBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxOceanBill.TabIndex = 6;
			// 
			// textBoxMailReference
			// 
			this.BindingSource.SetBindingMember(this.textBoxMailReference, "Shipment.ULH_MasterBill");
			this.textBoxMailReference.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("defb7a6f-cea9-47b1-815d-d4f6926f05ba", "Mail Reference");
			this.textBoxMailReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 3, true);
			this.textBoxMailReference.Name = "textBoxMailReference";
			this.textBoxMailReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxMailReference.TabIndex = 5;
			// 
			// textBoxMasterBill
			// 
			this.BindingSource.SetBindingMember(this.textBoxMasterBill, "Shipment.ULH_MasterBill");
			this.textBoxMasterBill.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fd3093c7-f754-46c1-a985-803fc6c27924", "Master Bill");
			this.textBoxMasterBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 3, true);
			this.textBoxMasterBill.Name = "textBoxMasterBill";
			this.textBoxMasterBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.textBoxMasterBill.TabIndex = 4;
			// 
			// codeFindBoxIssuerSCAC
			// 
			this.codeFindBoxIssuerSCAC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxIssuerSCAC, "Shipment.ULH_MasterBillIssuerSCAC");
			this.codeFindBoxIssuerSCAC.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("c49e8c82-3d30-4e35-bcaf-330dd3da78b3", "Issuer SCAC");
			this.codeFindBoxIssuerSCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 3, true);
			this.codeFindBoxIssuerSCAC.Name = "codeFindBoxIssuerSCAC";
			this.codeFindBoxIssuerSCAC.ShowDescriptionBox = false;
			this.codeFindBoxIssuerSCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.codeFindBoxIssuerSCAC.TabIndex = 3;
			// 
			// dropEditContainerMode
			// 
			this.dropEditContainerMode.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditContainerMode, "Shipment.ULH_ContainerMode");
			this.dropEditContainerMode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("95a58e51-e805-4c51-b11e-f7ef3cb2ca16", "Container");
			this.dropEditContainerMode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 43, true);
			this.dropEditContainerMode.Name = "dropEditContainerMode";
			this.dropEditContainerMode.PreBoundMaxLength = 3;
			this.dropEditContainerMode.ShouldResizeByMaxLength = true;
			this.dropEditContainerMode.ShowDescriptionBox = false;
			this.dropEditContainerMode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.dropEditContainerMode.TabIndex = 2;
			// 
			// dropEditModeOfTransport
			// 
			this.dropEditModeOfTransport.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditModeOfTransport, "Shipment.ULH_TransportMode");
			this.dropEditModeOfTransport.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ef7d335d-a8ec-408b-9c89-ab38a1d0e08f", "Mode of Transport");
			this.dropEditModeOfTransport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.dropEditModeOfTransport.Name = "dropEditModeOfTransport";
			this.dropEditModeOfTransport.PreBoundMaxLength = 3;
			this.dropEditModeOfTransport.ShouldResizeByMaxLength = true;
			this.dropEditModeOfTransport.ShowDescriptionBox = false;
			this.dropEditModeOfTransport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.dropEditModeOfTransport.TabIndex = 0;
			// 
			// textBoxCalculatedModeOfTransport
			// 
			this.BindingSource.SetBindingMember(this.textBoxCalculatedModeOfTransport, "Shipment.ULH_Calc_USTransportMode");
			this.textBoxCalculatedModeOfTransport.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3880ead4-952a-4007-82fa-b34a39e698c3", "Calculated MOT");
			this.textBoxCalculatedModeOfTransport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 19, true);
			this.textBoxCalculatedModeOfTransport.Name = "textBoxCalculatedModeOfTransport";
			this.textBoxCalculatedModeOfTransport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.textBoxCalculatedModeOfTransport.TabIndex = 1;
			// 
			// textBoxHouseBill
			// 
			this.BindingSource.SetBindingMember(this.textBoxHouseBill, "ULB_HouseBill");
			this.textBoxHouseBill.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fbe24612-7a41-4b5a-af0e-bc179259a262", "House Bill");
			this.textBoxHouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 16, true);
			this.textBoxHouseBill.Name = "textBoxHouseBill";
			this.textBoxHouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.textBoxHouseBill.TabIndex = 0;
			// 
			// calcEditQuantity
			// 
			this.BindingSource.SetBindingMember(this.calcEditQuantity, "ULB_NumberOfPacks");
			this.calcEditQuantity.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("e135521f-e9f3-4249-86aa-29740d2f1fe5", "Quantity");
			this.calcEditQuantity.DecimalPlaces = 2;
			this.calcEditQuantity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 16, true);
			this.calcEditQuantity.Name = "calcEditQuantity";
			this.calcEditQuantity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.calcEditQuantity.TabIndex = 1;
			this.calcEditQuantity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// codeFindBoxUOM
			// 
			this.codeFindBoxUOM.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxUOM, "ULB_PackType");
			this.codeFindBoxUOM.CaptionResourceString = null;
			this.codeFindBoxUOM.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 16, true);
			this.codeFindBoxUOM.Name = "codeFindBoxUOM";
			this.codeFindBoxUOM.PreBoundMaxLength = 3;
			this.codeFindBoxUOM.ShouldResize = false;
			this.codeFindBoxUOM.ShowDescriptionBox = false;
			this.codeFindBoxUOM.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.codeFindBoxUOM.TabIndex = 2;
			// 
			// textBoxGoodsValue
			// 
			this.BindingSource.SetBindingMember(this.textBoxGoodsValue, "ULB_GoodsValue");
			this.textBoxGoodsValue.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("34852975-251e-40d4-9ebb-7da706a35bd5", "Goods Value");
			this.textBoxGoodsValue.DecimalPlaces = 2;
			this.textBoxGoodsValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 42, true);
			this.textBoxGoodsValue.Name = "textBoxGoodsValue";
			this.textBoxGoodsValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.textBoxGoodsValue.TabIndex = 3;
			this.textBoxGoodsValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBoxGoodsValue.TabStop = false;
			// 
			// textBoxCurrency
			// 
			this.BindingSource.SetBindingMember(this.textBoxCurrency, "ULB_Currency");
			this.textBoxCurrency.CaptionResourceString = null;
			this.textBoxCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 42, true);
			this.textBoxCurrency.Name = "textBoxCurrency";
			this.textBoxCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.textBoxCurrency.TabIndex = 4;
			this.textBoxCurrency.TabStop = false;
			// 
			// codeFindBoxConsignmentIssuerSCAC
			// 
			this.codeFindBoxConsignmentIssuerSCAC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxConsignmentIssuerSCAC, "ULB_HouseBillIssuerSCAC");
			this.codeFindBoxConsignmentIssuerSCAC.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ebcd4502-6a36-44b2-93cf-e8750ac0f1e3", "Issuer SCAC");
			this.codeFindBoxConsignmentIssuerSCAC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.codeFindBoxConsignmentIssuerSCAC.Name = "codeFindBoxConsignmentIssuerSCAC";
			this.codeFindBoxConsignmentIssuerSCAC.ShouldResize = false;
			this.codeFindBoxConsignmentIssuerSCAC.ShowDescriptionBox = false;
			this.codeFindBoxConsignmentIssuerSCAC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.codeFindBoxConsignmentIssuerSCAC.TabIndex = 5;
			// 
			// textBoxOwnerRef
			// 
			this.BindingSource.SetBindingMember(this.textBoxOwnerRef, "ULB_OwnerReferenceNumber");
			this.textBoxOwnerRef.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("46e45d59-601c-485c-919e-316b2b941aa6", "Owner Ref");
			this.textBoxOwnerRef.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 68, true);
			this.textBoxOwnerRef.Name = "textBoxOwnerRef";
			this.textBoxOwnerRef.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.textBoxOwnerRef.TabIndex = 6;
			// 
			// textBoxRailReference
			// 
			this.BindingSource.SetBindingMember(this.textBoxRailReference, "CE_RailReferenceNumber");
			this.textBoxRailReference.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1b656d16-1c6d-4005-804e-8c3813815d4f", "Rail Reference");
			this.textBoxRailReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 94, true);
			this.textBoxRailReference.Name = "textBoxRailReference";
			this.textBoxRailReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.textBoxRailReference.TabIndex = 7;
			// 
			// textBoxContainer
			// 
			this.BindingSource.SetBindingMember(this.textBoxContainer, "ULB_EquipmentNumber");
			this.textBoxContainer.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d1e4717a-e0bf-4895-9600-4f8707561749", "Container");
			this.textBoxContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 94, true);
			this.textBoxContainer.Name = "textBoxContainer";
			this.textBoxContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 20, true);
			this.textBoxContainer.TabIndex = 8;
			// 
			// textBoxConsigneeId
			//
			this.BindingSource.SetBindingMember(this.textBoxConsigneeId, "ULB_ConsigneeIdentifier");
			this.textBoxConsigneeId.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("7c5104b5-7119-4f29-b607-a1497fdfcf28", "Consignee Identifier");
			this.textBoxConsigneeId.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 122, true);
			this.textBoxConsigneeId.Name = "textBoxConsigneeId";
			this.textBoxConsigneeId.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.textBoxConsigneeId.TabIndex = 9;
			// 
			// textBoxSellerId
			//
			this.BindingSource.SetBindingMember(this.textBoxSellerId, "ULB_SellerIdentifier");
			this.textBoxSellerId.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("44aee608-cee8-4fe7-a7e6-49dba3feef33", "Seller Identifier");
			this.textBoxSellerId.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 121, true);
			this.textBoxSellerId.Name = "textBoxSellerId";
			this.textBoxSellerId.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.textBoxSellerId.TabIndex = 10;
			// 
			// groupBoxSeller
			// 
			this.groupBoxSeller.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("9b41875a-d233-4be9-8356-805352a8eff2", "Seller");
			this.groupBoxSeller.Controls.Add(this.codeFindBoxSellerCountry);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerPostCode);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerState);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerCity);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerAddress2);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerAddress1);
			this.groupBoxSeller.Controls.Add(this.textBoxSellerName);
			this.groupBoxSeller.Controls.Add(this.addressControlSeller);
			this.groupBoxSeller.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 10, true);
			this.groupBoxSeller.Name = "groupBoxSeller";
			this.groupBoxSeller.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 177, true);
			this.groupBoxSeller.TabIndex = 9;
			this.groupBoxSeller.TabStop = false;
			// 
			// groupBoxConsignee
			// 
			this.groupBoxConsignee.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("8a22388f-e090-493a-b51b-391f145360c8", "Consignee");
			this.groupBoxConsignee.Controls.Add(this.codeFindBoxConsigneeCountry);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneePostCode);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeState);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeCity);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeAddress2);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeAddress1);
			this.groupBoxConsignee.Controls.Add(this.textBoxConsigneeName);
			this.groupBoxConsignee.Controls.Add(this.addressControlConsignee);
			this.groupBoxConsignee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(740, 10, true);
			this.groupBoxConsignee.Name = "groupBoxConsignee";
			this.groupBoxConsignee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 177, true);
			this.groupBoxConsignee.TabIndex = 18;
			this.groupBoxConsignee.TabStop = false;
			// 
			// addressControlSeller
			// 
			this.addressControlSeller.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.addressControlSeller, "ULB_OA_Seller");
			this.addressControlSeller.BindToOrgList = "Lookups+SellerOrgList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.addressControlSeller, false);
			this.addressControlSeller.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.addressControlSeller.Name = "addressControlSeller";
			this.addressControlSeller.PopupCaption = "";
			this.addressControlSeller.ReadOnly = false;
			this.addressControlSeller.ShowAddress = false;
			this.addressControlSeller.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.addressControlSeller.TabIndex = 10;
			// 
			// textBoxSellerName
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerName, "ULB_SellerName");
			this.textBoxSellerName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("a42ade9d-4228-40eb-b7f0-f7722a5e6af5", "Name");
			this.textBoxSellerName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 42, true);
			this.textBoxSellerName.Name = "textBoxSellerName";
			this.textBoxSellerName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerName.TabIndex = 11;
			// 
			// textBoxSellerAddress1
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerAddress1, "ULB_SellerAddress1");
			this.textBoxSellerAddress1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("bf5b32f6-3655-493c-b1a7-1fb08f8e9841", "Addr.");
			this.textBoxSellerAddress1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 68, true);
			this.textBoxSellerAddress1.Name = "textBoxSellerAddress1";
			this.textBoxSellerAddress1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerAddress1.TabIndex = 12;
			// 
			// textBoxSellerAddress2
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerAddress2, "ULB_SellerAddress2");
			this.textBoxSellerAddress2.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBoxSellerAddress2, false);
			this.textBoxSellerAddress2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 94, true);
			this.textBoxSellerAddress2.Name = "textBoxSellerAddress2";
			this.textBoxSellerAddress2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxSellerAddress2.TabIndex = 13;
			// 
			// textBoxSellerCity
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerCity, "ULB_SellerCity");
			this.textBoxSellerCity.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("721baddd-d4eb-41ef-b974-4926e000a7bb", "City");
			this.textBoxSellerCity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 120, true);
			this.textBoxSellerCity.Name = "textBoxSellerCity";
			this.textBoxSellerCity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.textBoxSellerCity.TabIndex = 14;
			// 
			// textBoxSellerState
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerState, "ULB_SellerState");
			this.textBoxSellerState.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("c3581f68-0f9d-4f2a-ba99-43c9d65e90c3", "State");
			this.textBoxSellerState.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 120, true);
			this.textBoxSellerState.Name = "textBoxSellerState";
			this.textBoxSellerState.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.textBoxSellerState.TabIndex = 15;
			// 
			// textBoxSellerPostCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxSellerPostCode, "ULB_SellerPostCode");
			this.textBoxSellerPostCode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ec9d9f0f-9a4a-4914-956c-ca9cd25f1aef", "Zip");
			this.textBoxSellerPostCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 146, true);
			this.textBoxSellerPostCode.Name = "textBoxSellerPostCode";
			this.textBoxSellerPostCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.textBoxSellerPostCode.TabIndex = 16;
			// 
			// codeFindBoxSellerCountry
			// 
			this.codeFindBoxSellerCountry.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxSellerCountry, "ULB_RN_NKSellerCountry");
			this.codeFindBoxSellerCountry.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b7dc85ca-975b-4347-9a86-35006cfa8a05", "Ctry/Rgn.", "Country/Region");
			this.codeFindBoxSellerCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 146, true);
			this.codeFindBoxSellerCountry.Name = "codeFindBoxSellerCountry";
			this.codeFindBoxSellerCountry.PreBoundMaxLength = 2;
			this.codeFindBoxSellerCountry.ShouldResize = true;
			this.codeFindBoxSellerCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.codeFindBoxSellerCountry.TabIndex = 17;
			// 
			// addressControlConsignee
			// 
			this.addressControlConsignee.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.addressControlConsignee, "ULB_OA_Consignee");
			this.addressControlConsignee.BindToOrgList = "Lookups+ConsigneeOrgList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.addressControlConsignee, false);
			this.addressControlConsignee.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.addressControlConsignee.Name = "addressControlConsignee";
			this.addressControlConsignee.PopupCaption = "";
			this.addressControlConsignee.ReadOnly = false;
			this.addressControlConsignee.ShowAddress = false;
			this.addressControlConsignee.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.addressControlConsignee.TabIndex = 19;
			// 
			// textBoxConsigneeName
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeName, "ULB_ConsigneeName");
			this.textBoxConsigneeName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("eb27d1c7-081a-413d-be8b-d5695b8c604f", "Name");
			this.textBoxConsigneeName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 42, true);
			this.textBoxConsigneeName.Name = "textBoxConsigneeName";
			this.textBoxConsigneeName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxConsigneeName.TabIndex = 20;
			// 
			// textBoxConsigneeAddress1
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeAddress1, "ULB_ConsigneeAddress1");
			this.textBoxConsigneeAddress1.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("f933e990-dc07-4c19-b90f-cac3e37eab90", "Addr.");
			this.textBoxConsigneeAddress1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 68, true);
			this.textBoxConsigneeAddress1.Name = "textBoxConsigneeAddress1";
			this.textBoxConsigneeAddress1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxConsigneeAddress1.TabIndex = 21;
			// 
			// textBoxConsigneeAddress2
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeAddress2, "ULB_ConsigneeAddress2");
			this.textBoxConsigneeAddress2.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBoxConsigneeAddress2, false);
			this.textBoxConsigneeAddress2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 94, true);
			this.textBoxConsigneeAddress2.Name = "textBoxConsigneeAddress2";
			this.textBoxConsigneeAddress2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.textBoxConsigneeAddress2.TabIndex = 22;
			// 
			// textBoxConsigneeCity
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeCity, "ULB_ConsigneeCity");
			this.textBoxConsigneeCity.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("f7a47869-2800-4238-a276-3b5384aab7d4", "City");
			this.textBoxConsigneeCity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 120, true);
			this.textBoxConsigneeCity.Name = "textBoxConsigneeCity";
			this.textBoxConsigneeCity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.textBoxConsigneeCity.TabIndex = 23;
			// 
			// textBoxConsigneeState
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneeState, "ULB_ConsigneeState");
			this.textBoxConsigneeState.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0b1a8d12-5734-4c94-8d10-f39b7f16e641", "State");
			this.textBoxConsigneeState.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 120, true);
			this.textBoxConsigneeState.Name = "textBoxConsigneeState";
			this.textBoxConsigneeState.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 20, true);
			this.textBoxConsigneeState.TabIndex = 24;
			// 
			// textBoxConsigneePostCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxConsigneePostCode, "ULB_ConsigneePostCode");
			this.textBoxConsigneePostCode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("c209272b-25fa-4520-9cbf-b442a119b9c0", "Zip");
			this.textBoxConsigneePostCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 146, true);
			this.textBoxConsigneePostCode.Name = "textBoxConsigneePostCode";
			this.textBoxConsigneePostCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.textBoxConsigneePostCode.TabIndex = 25;
			// 
			// codeFindBoxConsigneeCountry
			// 
			this.codeFindBoxConsigneeCountry.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxConsigneeCountry, "ULB_RN_NKConsigneeCountry");
			this.codeFindBoxConsigneeCountry.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("30c2a148-4024-47a7-8846-c4e015eb2aad", "Ctry/Rgn.", "Country/Region");
			this.codeFindBoxConsigneeCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 146, true);
			this.codeFindBoxConsigneeCountry.Name = "codeFindBoxConsigneeCountry";
			this.codeFindBoxConsigneeCountry.PreBoundMaxLength = 2;
			this.codeFindBoxConsigneeCountry.ShouldResize = true;
			this.codeFindBoxConsigneeCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.codeFindBoxConsigneeCountry.TabIndex = 26;
			// 
			// textBoxEntryNumber
			// 
			this.BindingSource.SetBindingMember(this.textBoxEntryNumber, "CE_EntryNum");
			this.textBoxEntryNumber.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0b7695d7-a681-4168-94df-c84553d259cd", "Entry Number");
			this.textBoxEntryNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 16, true);
			this.textBoxEntryNumber.Name = "textBoxEntryNumber";
			this.textBoxEntryNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.textBoxEntryNumber.TabIndex = 0;
			this.textBoxEntryNumber.TabStop = false;
			// 
			// textBoxEntryFilerCode
			// 
			this.BindingSource.SetBindingMember(this.textBoxEntryFilerCode, "Shipment.ULH_EntryFilerCode");
			this.textBoxEntryFilerCode.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("eb07e9a8-c98c-43fd-9633-205cb3de10fd", "Filer");
			this.textBoxEntryFilerCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 16, true);
			this.textBoxEntryFilerCode.Name = "textBoxEntryFilerCode";
			this.textBoxEntryFilerCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.textBoxEntryFilerCode.TabIndex = 2;
			this.textBoxEntryFilerCode.ReadOnly = true;
			this.textBoxEntryFilerCode.TabStop = false;
			// 
			// dateEditIssueDate
			// 
			this.dateEditIssueDate.AllowDrop = true;
			this.dateEditIssueDate.AutoCompleteMonthThreshold = 1;
			this.dateEditIssueDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditIssueDate, "CE_IssueDate");
			this.dateEditIssueDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("7172ba4a-a77d-4832-a864-aa4a9a4b7af6", "Date");
			this.dateEditIssueDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 42, true);
			this.dateEditIssueDate.Name = "dateEditIssueDate";
			this.dateEditIssueDate.TabIndex = 3;
			this.dateEditIssueDate.TabStop = false;
			// 
			// dateEditMessageDate
			// 
			this.dateEditMessageDate.AllowDrop = true;
			this.dateEditMessageDate.AutoCompleteMonthThreshold = 1;
			this.dateEditMessageDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditMessageDate, "LatestMsgStatusDate");
			this.dateEditMessageDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("863d7135-e5da-48e7-a6f2-20b89abe325a", "Date");
			this.dateEditMessageDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 68, true);
			this.dateEditMessageDate.Name = "dateEditMessageDate";
			this.dateEditMessageDate.TabIndex = 4;
			this.dateEditMessageDate.TabStop = false;
			// 
			// codeFindBoxLocationOfGoods
			// 
			this.codeFindBoxLocationOfGoods.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLocationOfGoods, "Shipment.ULH_US_NKLocationOfGoods");
			this.codeFindBoxLocationOfGoods.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("E32D008B-0D2D-4FA2-96AC-ED0F6B61FA6A", "FIRMS Code");
			this.codeFindBoxLocationOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 16, true);
			this.codeFindBoxLocationOfGoods.Name = "codeFindBoxLocationOfGoods";
			this.codeFindBoxLocationOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.codeFindBoxLocationOfGoods.TabIndex = 5;
			this.codeFindBoxLocationOfGoods.TabStop = false;
			// 
			// textBoxContactName
			// 
			this.BindingSource.SetBindingMember(this.textBoxContactName, "Shipment.ULH_ContactName");
			this.textBoxContactName.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("3D775EF5-B563-4CA8-BA6A-8C854675C4EE", "Contact Name");
			this.textBoxContactName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 42, true);
			this.textBoxContactName.Name = "textBoxContactName";
			this.textBoxContactName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.textBoxContactName.TabIndex = 6;
			this.textBoxContactName.ReadOnly = true;
			this.textBoxContactName.TabStop = false;
			// 
			// textBoxContactPhone
			// 
			this.BindingSource.SetBindingMember(this.textBoxContactPhone, "Shipment.ULH_ContactPhone");
			this.textBoxContactPhone.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("69a3422e-0a76-4ff1-b932-1451ade896d1", "Contact Phone");
			this.textBoxContactPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 68, true);
			this.textBoxContactPhone.Name = "textBoxContactPhone";
			this.textBoxContactPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.textBoxContactPhone.TabIndex = 7;
			this.textBoxContactPhone.ReadOnly = true;
			this.textBoxContactPhone.TabStop = false;
			// 
			// codeFindBoxCentralizedExamSite
			// 
			this.codeFindBoxCentralizedExamSite.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxCentralizedExamSite, "Shipment.ULH_US_NKCentralizedExamSite");
			this.codeFindBoxCentralizedExamSite.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("FADF26FC-CF30-4934-A5E4-06037B0D001C", "Centralized Exam Site");
			this.codeFindBoxCentralizedExamSite.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(941, 16, true);
			this.codeFindBoxCentralizedExamSite.Name = "codeFindBoxCentralizedExamSite";
			this.codeFindBoxCentralizedExamSite.ShowDescriptionBox = false;
			this.codeFindBoxCentralizedExamSite.ShouldResize = false;
			this.codeFindBoxCentralizedExamSite.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.codeFindBoxCentralizedExamSite.TabIndex = 10;
			this.codeFindBoxCentralizedExamSite.TabStop = false;
			// 
			// orgFindBoxMainTabClient
			// 
			this.orgFindBoxMainTabClient.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orgFindBoxMainTabClient, "Shipment.ULH_OH_Client");
			this.orgFindBoxMainTabClient.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("349077b1-0880-43dd-86fd-cbd39cba129e", "Local Client");
			this.orgFindBoxMainTabClient.IsPrimaryKeyFromCodeRequired = false;
			this.orgFindBoxMainTabClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(941, 42, true);
			this.orgFindBoxMainTabClient.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.orgFindBoxMainTabClient.Name = "orgFindBoxMainTabClient";
			this.orgFindBoxMainTabClient.ShouldResize = true;
			this.orgFindBoxMainTabClient.ShowDescriptionBox = false;
			this.orgFindBoxMainTabClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.orgFindBoxMainTabClient.TabIndex = 10;
			this.orgFindBoxMainTabClient.TabStop = false;
			// 
			// orgFindBoxMainTabImporter
			// 
			this.orgFindBoxMainTabImporter.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orgFindBoxMainTabImporter, "Shipment.ULH_OH_Importer");
			this.orgFindBoxMainTabImporter.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d5062ae9-9d61-4690-8430-4051b0850271", "Importer of Record");
			this.orgFindBoxMainTabImporter.IsPrimaryKeyFromCodeRequired = false;
			this.orgFindBoxMainTabImporter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(941, 68, true);
			this.orgFindBoxMainTabImporter.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.orgFindBoxMainTabImporter.Name = "orgFindBoxMainTabImporter";
			this.orgFindBoxMainTabImporter.ShouldResize = true;
			this.orgFindBoxMainTabImporter.ShowDescriptionBox = false;
			this.orgFindBoxMainTabImporter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.orgFindBoxMainTabImporter.TabIndex = 11;
			this.orgFindBoxMainTabImporter.TabStop = false;
			// 
			// dropEditMainTabEntryStatus
			// 
			this.dropEditMainTabEntryStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditMainTabEntryStatus, "CE_EntryStatus");
			this.dropEditMainTabEntryStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d25cf1ef-aa8f-458b-b9b8-beedd65dc48a", "Release Status");
			this.dropEditMainTabEntryStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 42, true);
			this.dropEditMainTabEntryStatus.Name = "dropEditMainTabEntryStatus";
			this.dropEditMainTabEntryStatus.PreBoundMaxLength = 3;
			this.dropEditMainTabEntryStatus.ShouldResizeByMaxLength = true;
			this.dropEditMainTabEntryStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.dropEditMainTabEntryStatus.TabIndex = 7;
			this.dropEditMainTabEntryStatus.TabStop = false;
			// 
			// dropEditMainTabMessageStatus
			// 
			this.dropEditMainTabMessageStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditMainTabMessageStatus, "ULB_MessageStatus");
			this.dropEditMainTabMessageStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ba8a383e-cd45-462e-89c7-127c969a8a0b", "Message Status");
			this.dropEditMainTabMessageStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.dropEditMainTabMessageStatus.Name = "dropEditMainTabMessageStatus";
			this.dropEditMainTabMessageStatus.PreBoundMaxLength = 3;
			this.dropEditMainTabMessageStatus.ShouldResizeByMaxLength = true;
			this.dropEditMainTabMessageStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.dropEditMainTabMessageStatus.TabIndex = 8;
			this.dropEditMainTabMessageStatus.TabStop = false;
			// 
			// codeFindBoxProduct
			// 
			this.codeFindBoxProduct.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxProduct, "FirstCusUSLVItemProductCode");
			this.codeFindBoxProduct.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("15ba8a2d-4d78-4b66-a86d-26a042fb0e9a", "Product Code");
			this.codeFindBoxProduct.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 16, true);
			this.codeFindBoxProduct.Name = "codeFindBoxProduct";
			this.codeFindBoxProduct.ShouldResize = false;
			this.codeFindBoxProduct.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.codeFindBoxProduct.TabIndex = 28;
			this.codeFindBoxProduct.ShowDescriptionBox = false;
			// 
			// codeFindBoxTariff
			// 
			this.codeFindBoxTariff.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxTariff, "FirstCusUSLVItemTariff");
			this.codeFindBoxTariff.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("5f4624f3-e523-4a98-b852-cf064c68863c", "Tariff");
			this.codeFindBoxTariff.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 16, true);
			this.codeFindBoxTariff.Name = "codeFindBoxTariff";
			this.codeFindBoxTariff.ShouldResize = false;
			this.codeFindBoxTariff.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.codeFindBoxTariff.TabIndex = 29;
			this.codeFindBoxTariff.ShowDescriptionBox = false;
			// 
			// textBoxGoodsDescription
			// 
			this.BindingSource.SetBindingMember(this.textBoxGoodsDescription, "FirstCusUSLVItemGoodsDescription");
			this.textBoxGoodsDescription.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("85f43b76-7252-4683-b1ae-a93d358e7cce", "Goods Description");
			this.textBoxGoodsDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 16, true);
			this.textBoxGoodsDescription.Name = "textBoxGoodsDescription";
			this.textBoxGoodsDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 20, true);
			this.textBoxGoodsDescription.TabIndex = 30;
			// 
			// calcEditLineValue
			// 
			this.BindingSource.SetBindingMember(this.calcEditLineValue, "FirstCusUSLVItemLineValue");
			this.calcEditLineValue.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("204dbf65-8718-4636-aef2-a33fae83800e", "Line Value");
			this.calcEditLineValue.DecimalPlaces = 2;
			this.calcEditLineValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 42, true);
			this.calcEditLineValue.Name = "calcEditLineValue";
			this.calcEditLineValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.calcEditLineValue.TabIndex = 31;
			this.calcEditLineValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// codeFindBoxLineCurrency
			// 
			this.codeFindBoxLineCurrency.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxLineCurrency, "FirstCusUSLVItemCurrency");
			this.codeFindBoxLineCurrency.CaptionResourceString = null;
			this.codeFindBoxLineCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 42, true);
			this.codeFindBoxLineCurrency.MaxLength = 3;
			this.codeFindBoxLineCurrency.Name = "codeFindBoxLineCurrency";
			this.codeFindBoxLineCurrency.PreBoundMaxLength = 3;
			this.codeFindBoxLineCurrency.ShouldResize = false;
			this.codeFindBoxLineCurrency.ShowDescriptionBox = false;
			this.codeFindBoxLineCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 20, true);
			this.codeFindBoxLineCurrency.TabIndex = 32;
			// 
			// calcEditExchangeRate
			// 
			this.BindingSource.SetBindingMember(this.calcEditExchangeRate, "FirstCusUSLVItemExchangeRate");
			this.calcEditExchangeRate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fe357f30-6e69-44c0-b301-7b2b98690059", "Exchange Rate");
			this.calcEditExchangeRate.DecimalPlaces = 6;
			this.calcEditExchangeRate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 42, true);
			this.calcEditExchangeRate.Name = "calcEditExchangeRate";
			this.calcEditExchangeRate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.calcEditExchangeRate.TabIndex = 33;
			this.calcEditExchangeRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.calcEditExchangeRate.TabStop = false;
			this.calcEditExchangeRate.ReadOnly = true;
			// 
			// codeFindBoxCountryOfOrigin
			// 
			this.codeFindBoxCountryOfOrigin.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.codeFindBoxCountryOfOrigin, "FirstCusUSLVItemCountryOfOrigin");
			this.codeFindBoxCountryOfOrigin.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("536b55c9-6c71-418d-9ef5-54aa65060d33", "Ctry/Rgn. Of Origin", "Country/Region Of Origin");
			this.codeFindBoxCountryOfOrigin.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 42, true);
			this.codeFindBoxCountryOfOrigin.MaxLength = 2;
			this.codeFindBoxCountryOfOrigin.Name = "codeFindBoxCountryOfOrigin";
			this.codeFindBoxCountryOfOrigin.PreBoundMaxLength = 2;
			this.codeFindBoxCountryOfOrigin.ShouldResize = true;
			this.codeFindBoxCountryOfOrigin.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.codeFindBoxCountryOfOrigin.TabIndex = 34;
			this.codeFindBoxCountryOfOrigin.ShowDescriptionBox = false;
			// 
			// checkBoxADD
			// 
			this.checkBoxADD.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BindingSource.SetBindingMember(this.checkBoxADD, "FirstCusUSLVItemAntiDumping");
			this.checkBoxADD.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("20b012b0-8396-4edd-886d-0f6c7469cff3", "ADD N/A");
			this.checkBoxADD.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxADD.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 42, true);
			this.checkBoxADD.Name = "checkBoxADD";
			this.checkBoxADD.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.checkBoxADD.TabIndex = 35;
			// 
			// checkBoxCVD
			// 
			this.checkBoxCVD.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BindingSource.SetBindingMember(this.checkBoxCVD, "FirstCusUSLVItemCountervailing");
			this.checkBoxCVD.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("1696a89b-fd58-4c55-8376-c3e70e713a54", "CVD N/A");
			this.checkBoxCVD.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxCVD.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 42, true);
			this.checkBoxCVD.Name = "checkBoxCVD";
			this.checkBoxCVD.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.checkBoxCVD.TabIndex = 36;
			// 
			// checkBoxMultipleLines
			// 
			this.checkBoxMultipleLines.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BindingSource.SetBindingMember(this.checkBoxMultipleLines, "HasMultipleItemLine");
			this.checkBoxMultipleLines.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fc3f60ae-41b0-4c64-acc6-0b4aa2b8e38d", "Multiple Lines");
			this.checkBoxMultipleLines.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxMultipleLines.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 68, true);
			this.checkBoxMultipleLines.Name = "checkBoxMultipleLines";
			this.checkBoxMultipleLines.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 24, true);
			this.checkBoxMultipleLines.TabIndex = 37;
			this.checkBoxMultipleLines.TabStop = false;
			// 
			// checkBoxPGARequirements
			// 
			this.checkBoxPGARequirements.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.BindingSource.SetBindingMember(this.checkBoxPGARequirements, "HasAtLeastOnePGARequirementOnAnyItemLine");
			this.checkBoxPGARequirements.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fa0e2f3b-f36a-4013-a8c8-b2c4360fd7bd", "PGA Requirements");
			this.checkBoxPGARequirements.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxPGARequirements.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 68, true);
			this.checkBoxPGARequirements.Name = "checkBoxPGARequirements";
			this.checkBoxPGARequirements.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.checkBoxPGARequirements.TabIndex = 38;
			this.checkBoxPGARequirements.TabStop = false;
			this.MainTabPage.PerformLayout();
			this.tableLayoutPanelBillDetails.ResumeLayout(false);
			this.tableLayoutPanelBillDetails.PerformLayout();
			this.groupBoxTransportDetails.ResumeLayout(false);
			this.groupBoxTransportDetails.PerformLayout();
			this.groupBoxConsignmentDetails.ResumeLayout(false);
			this.groupBoxConsignmentDetails.PerformLayout();
			this.groupBoxLineDetails.ResumeLayout(false);
			this.groupBoxLineDetails.PerformLayout();
			this.groupBoxSeller.ResumeLayout(false);
			this.groupBoxSeller.PerformLayout();
			this.groupBoxConsignee.ResumeLayout(false);
			this.groupBoxConsignee.PerformLayout();
			this.panelLocationDate.ResumeLayout(false);
			this.panelLocationDate.PerformLayout();
			this.dateEditArrival.ResumeLayout(true);
			this.dateEditArrival.PerformLayout();
			this.codeFindBoxLoadingPort.ResumeLayout(true);
			this.codeFindBoxLoadingPort.PerformLayout();
			this.codeFindBoxEntryPort.ResumeLayout(true);
			this.codeFindBoxEntryPort.PerformLayout();
			this.dateEditDeparture.ResumeLayout(true);
			this.dateEditDeparture.PerformLayout();
			this.codeFindBoxDischargeUNLOCO.ResumeLayout(true);
			this.codeFindBoxDischargeUNLOCO.PerformLayout();
			this.codeFindBoxLoadingUNLOCO.ResumeLayout(true);
			this.codeFindBoxLoadingUNLOCO.PerformLayout();
			this.dateEditDischarge.ResumeLayout(true);
			this.dateEditDischarge.PerformLayout();
			this.codeFindBoxDischargePort.ResumeLayout(true);
			this.codeFindBoxDischargePort.PerformLayout();
			this.panelTransportDetailGroup.ResumeLayout(false);
			this.panelTransportDetailGroup.PerformLayout();
			this.codeFindBoxCarrierSCAC.ResumeLayout(true);
			this.codeFindBoxCarrierSCAC.PerformLayout();
			this.codeFindBoxVessel.ResumeLayout(true);
			this.codeFindBoxVessel.PerformLayout();
			this.masterBillControl.ResumeLayout(true);
			this.masterBillControl.PerformLayout();
			this.codeFindBoxIssuerSCAC.ResumeLayout(true);
			this.codeFindBoxIssuerSCAC.PerformLayout();
			this.dropEditContainerMode.ResumeLayout(true);
			this.dropEditContainerMode.PerformLayout();
			this.dropEditModeOfTransport.ResumeLayout(true);
			this.dropEditModeOfTransport.PerformLayout();
			this.addressControlSeller.ResumeLayout(true);
			this.addressControlSeller.PerformLayout();
			this.codeFindBoxSellerCountry.ResumeLayout(true);
			this.codeFindBoxSellerCountry.PerformLayout();
			this.addressControlConsignee.ResumeLayout(true);
			this.addressControlConsignee.PerformLayout();
			this.codeFindBoxConsigneeCountry.ResumeLayout(true);
			this.codeFindBoxConsigneeCountry.PerformLayout();
			this.groupBoxSummary.ResumeLayout(false);
			this.groupBoxSummary.PerformLayout();
			this.orgFindBoxMainTabClient.ResumeLayout(true);
			this.orgFindBoxMainTabClient.PerformLayout();
			this.codeFindBoxCentralizedExamSite.ResumeLayout(true);
			this.codeFindBoxCentralizedExamSite.PerformLayout();
			this.orgFindBoxMainTabImporter.ResumeLayout(true);
			this.orgFindBoxMainTabImporter.PerformLayout();
			this.dateEditIssueDate.ResumeLayout(true);
			this.dateEditIssueDate.PerformLayout();
			this.dateEditMessageDate.ResumeLayout(true);
			this.dateEditMessageDate.PerformLayout();
			this.dropEditMainTabEntryStatus.ResumeLayout(true);
			this.dropEditMainTabEntryStatus.PerformLayout();
			this.dropEditMainTabMessageStatus.ResumeLayout(true);
			this.dropEditMainTabMessageStatus.PerformLayout();
			this.codeFindBoxLocationOfGoods.ResumeLayout(true);
			this.codeFindBoxLocationOfGoods.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		private void tabPageCommodities_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.US.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.US.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.splitContainerCommodities = new CargoWise.Windows.UI.KSplitContainer();
			this.groupBoxCommodityDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.pgaRequirementsControl = new Enterprise.Customs.US.LVS.GUI.OGAPGARequirementsControl();
			this.gridCommodityDetails = new Enterprise.ZArchitecture.ZGrid();
			this.tabPageCommodities.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerCommodities)).BeginInit();
			this.splitContainerCommodities.Panel1.SuspendLayout();
			this.splitContainerCommodities.Panel2.SuspendLayout();
			this.splitContainerCommodities.SuspendLayout();
			this.groupBoxCommodityDetails.SuspendLayout();
			this.pgaRequirementsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridCommodityDetails)).BeginInit();
			this.gridCommodityDetails.SuspendLayout();
			this.tabPageCommodities.Controls.Add(this.splitContainerCommodities);
			// 
			// splitContainerCommodities
			// 
			this.splitContainerCommodities.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerCommodities.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainerCommodities.Name = "splitContainerCommodities";
			this.splitContainerCommodities.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerCommodities.Panel1
			// 
			this.splitContainerCommodities.Panel1.Controls.Add(this.groupBoxCommodityDetails);
			// 
			// splitContainerCommodities.Panel2
			// 
			this.splitContainerCommodities.Panel2.Controls.Add(this.pgaRequirementsControl);
			this.splitContainerCommodities.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 368, true);
			this.splitContainerCommodities.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(169);
			this.splitContainerCommodities.TabIndex = 0;
			// 
			// groupBoxCommodityDetails
			// 
			this.groupBoxCommodityDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2a465d14-b4d4-4e61-8a9f-873d5c4a440b", "Commodity Details");
			this.groupBoxCommodityDetails.Controls.Add(this.gridCommodityDetails);
			this.groupBoxCommodityDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxCommodityDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxCommodityDetails.Name = "groupBoxCommodityDetails";
			this.groupBoxCommodityDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 169, true);
			this.groupBoxCommodityDetails.TabIndex = 0;
			this.groupBoxCommodityDetails.TabStop = false;
			// 
			// pgaRequirementsControl
			// 
			this.pgaRequirementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pgaRequirementsControl, "CusUSLVItems");
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
			this.BindingSource.SetBindingMember(this.gridCommodityDetails, "CusUSLVItems");
			this.gridCommodityDetails.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ULI_PartNo";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tariffColumnStyleInfo1.ColumnName = "ULI_TariffFormatted";
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ULI_GoodsDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ULI_GoodsValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ULI_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ULI_RX_NKCurrEXRate";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ULI_RN_NKCountryOfOrigin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "ULI_AntiDumping";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ULI_Countervailing";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridCommodityDetails.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.gridCommodityDetails.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.gridCommodityDetails.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.gridCommodityDetails.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.gridCommodityDetails.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.gridCommodityDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridCommodityDetails.GridId = "92e3f038-be18-47cc-86aa-0f7fa480dbf4";
			this.gridCommodityDetails.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridCommodityDetails.LayoutKey = "gridCommodityDetails";
			this.gridCommodityDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridCommodityDetails.Name = "gridCommodityDetails";
			this.gridCommodityDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1052, 150, true);
			this.gridCommodityDetails.TabIndex = 0;
			this.tabPageCommodities.PerformLayout();
			this.splitContainerCommodities.Panel1.ResumeLayout(false);
			this.splitContainerCommodities.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerCommodities)).EndInit();
			this.splitContainerCommodities.ResumeLayout(false);
			this.splitContainerCommodities.PerformLayout();
			this.groupBoxCommodityDetails.ResumeLayout(false);
			this.groupBoxCommodityDetails.PerformLayout();
			this.pgaRequirementsControl.ResumeLayout(true);
			this.pgaRequirementsControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridCommodityDetails)).EndInit();
			this.gridCommodityDetails.ResumeLayout(false);
			this.gridCommodityDetails.PerformLayout();
			this.tabPageCommodities.ResumeLayout(true);

		}

		private void tabPageStatus_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.groupBoxCargoReleaseStatus = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBoxDisposition = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.gridDisposition = new Enterprise.ZArchitecture.ZGrid();
			this.dateEditMessageStatusDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.tableLayoutPanelStatus = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.dropEditMessageStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.tabPageStatus.SuspendLayout();
			this.groupBoxCargoReleaseStatus.SuspendLayout();
			this.groupBoxDisposition.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridDisposition)).BeginInit();
			this.gridDisposition.SuspendLayout();
			this.dateEditMessageStatusDate.SuspendLayout();
			this.tableLayoutPanelStatus.SuspendLayout();
			this.dropEditMessageStatus.SuspendLayout();
			this.tabPageStatus.Controls.Add(this.tableLayoutPanelStatus);
			// 
			// groupBoxCargoReleaseStatus
			// 
			this.groupBoxCargoReleaseStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("0ba0a799-aab0-4a25-8890-6ff14fb4e5e8", "Cargo Release Status");
			this.groupBoxCargoReleaseStatus.Controls.Add(this.dropEditMessageStatus);
			this.groupBoxCargoReleaseStatus.Controls.Add(this.dateEditMessageStatusDate);
			this.groupBoxCargoReleaseStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxCargoReleaseStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.groupBoxCargoReleaseStatus.Name = "groupBoxCargoReleaseStatus";
			this.groupBoxCargoReleaseStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 178, true);
			this.groupBoxCargoReleaseStatus.TabIndex = 0;
			this.groupBoxCargoReleaseStatus.TabStop = false;
			// 
			// groupBoxDisposition
			// 
			this.groupBoxDisposition.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("2d241c63-4482-4ebe-aee0-b4d4f2536ec3", "Disposition");
			this.groupBoxDisposition.Controls.Add(this.gridDisposition);
			this.groupBoxDisposition.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxDisposition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 187, true);
			this.groupBoxDisposition.Name = "groupBoxDisposition";
			this.groupBoxDisposition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 178, true);
			this.groupBoxDisposition.TabIndex = 1;
			this.groupBoxDisposition.TabStop = false;
			// 
			// gridDisposition
			// 
			this.gridDisposition.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridDisposition, "DispositionCodesView");
			this.gridDisposition.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.Caption = "Code ID";
			zTextBoxColumnStyleInfo5.ColumnName = "ErrorMessageIdentifier";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.Caption = "Narrative";
			zTextBoxColumnStyleInfo6.ColumnName = "NarrativeMessage";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zDateEditColumnStyleInfo1.Caption = "Date";
			zDateEditColumnStyleInfo1.ColumnName = "StatusDate";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "Release Date";
			zDateEditColumnStyleInfo2.ColumnName = "ReleaseDate";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "Release Origin";
			zTextBoxColumnStyleInfo7.ColumnName = "ReleaseOrigin";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.Caption = "Release Origin Description";
			zTextBoxColumnStyleInfo8.ColumnName = "ReleaseOriginDescription";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.gridDisposition.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.gridDisposition.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.gridDisposition.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.gridDisposition.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridDisposition.GridId = "a0071b8b-e8dd-49ba-9cbe-4b0891fb8a32";
			this.gridDisposition.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridDisposition.LayoutKey = "gridDisposition";
			this.gridDisposition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridDisposition.Name = "gridDisposition";
			this.gridDisposition.ReadOnly = true;
			this.gridDisposition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 159, true);
			this.gridDisposition.TabIndex = 0;
			// 
			// dateEditMessageStatusDate
			// 
			this.dateEditMessageStatusDate.AllowDrop = true;
			this.dateEditMessageStatusDate.AutoCompleteMonthThreshold = 1;
			this.dateEditMessageStatusDate.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateEditMessageStatusDate, "LatestMsgStatusDate");
			this.dateEditMessageStatusDate.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("fbb3e111-63e2-4683-aa90-ebb024c2f8fe", "Msg. Status Date");
			this.dateEditMessageStatusDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 36, true);
			this.dateEditMessageStatusDate.Name = "dateEditMessageStatusDate";
			this.dateEditMessageStatusDate.TabIndex = 7;
			// 
			// tableLayoutPanelStatus
			// 
			this.tableLayoutPanelStatus.ColumnCount = 1;
			this.tableLayoutPanelStatus.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelStatus.Controls.Add(this.groupBoxCargoReleaseStatus, 0, 0);
			this.tableLayoutPanelStatus.Controls.Add(this.groupBoxDisposition, 0, 1);
			this.tableLayoutPanelStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.tableLayoutPanelStatus.Name = "tableLayoutPanelStatus";
			this.tableLayoutPanelStatus.RowCount = 2;
			this.tableLayoutPanelStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelStatus.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.tableLayoutPanelStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 368, true);
			this.tableLayoutPanelStatus.TabIndex = 0;
			this.tableLayoutPanelStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// dropEditMessageStatus
			// 
			this.dropEditMessageStatus.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditMessageStatus, "ULB_MessageStatus");
			this.dropEditMessageStatus.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("b22a83fa-641d-450f-8914-8641788bf08a", "Message Status");
			this.dropEditMessageStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 36, true);
			this.dropEditMessageStatus.Name = "dropEditMessageStatus";
			this.dropEditMessageStatus.PreBoundMaxLength = 3;
			this.dropEditMessageStatus.ShouldResizeByMaxLength = true;
			this.dropEditMessageStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.dropEditMessageStatus.TabIndex = 9;
			this.tabPageStatus.PerformLayout();
			this.groupBoxCargoReleaseStatus.ResumeLayout(false);
			this.groupBoxCargoReleaseStatus.PerformLayout();
			this.groupBoxDisposition.ResumeLayout(false);
			this.groupBoxDisposition.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridDisposition)).EndInit();
			this.gridDisposition.ResumeLayout(false);
			this.gridDisposition.PerformLayout();
			this.dateEditMessageStatusDate.ResumeLayout(true);
			this.dateEditMessageStatusDate.PerformLayout();
			this.tableLayoutPanelStatus.ResumeLayout(false);
			this.tableLayoutPanelStatus.PerformLayout();
			this.dropEditMessageStatus.ResumeLayout(true);
			this.dropEditMessageStatus.PerformLayout();
			this.tabPageStatus.ResumeLayout(true);

		}

		private void tabPageMessages_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainerMessage = new CargoWise.Windows.UI.KSplitContainer();
			this.groupBoxMessageHistory = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tabControlMessage = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.tabPageMessageText = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageMessageDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.tabPageStatusErrors = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.gridMessageHistory = new Enterprise.ZArchitecture.ZGrid();
			this.tabPageMessages.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMessage)).BeginInit();
			this.splitContainerMessage.Panel1.SuspendLayout();
			this.splitContainerMessage.Panel2.SuspendLayout();
			this.splitContainerMessage.SuspendLayout();
			this.groupBoxMessageHistory.SuspendLayout();
			this.tabControlMessage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridMessageHistory)).BeginInit();
			this.gridMessageHistory.SuspendLayout();
			this.tabPageMessages.Controls.Add(this.splitContainerMessage);
			// 
			// splitContainerMessage
			// 
			this.splitContainerMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainerMessage.Name = "splitContainerMessage";
			// 
			// splitContainerMessage.Panel1
			// 
			this.splitContainerMessage.Panel1.Controls.Add(this.groupBoxMessageHistory);
			// 
			// splitContainerMessage.Panel2
			// 
			this.splitContainerMessage.Panel2.Controls.Add(this.tabControlMessage);
			this.splitContainerMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1058, 348, true);
			this.splitContainerMessage.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(537);
			this.splitContainerMessage.TabIndex = 0;
			// 
			// groupBoxMessageHistory
			// 
			this.groupBoxMessageHistory.BackColor = System.Drawing.SystemColors.Control;
			this.groupBoxMessageHistory.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("a7d10467-47af-4bbd-af33-293dfe2098cd", "Message History");
			this.groupBoxMessageHistory.Controls.Add(this.gridMessageHistory);
			this.groupBoxMessageHistory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxMessageHistory.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxMessageHistory.Name = "groupBoxMessageHistory";
			this.groupBoxMessageHistory.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(537, 348, true);
			this.groupBoxMessageHistory.TabIndex = 0;
			this.groupBoxMessageHistory.TabStop = false;
			// 
			// tabControlMessage
			// 
			this.tabControlMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.tabControlMessage.Controls.Add(this.tabPageMessageText);
			this.tabControlMessage.Controls.Add(this.tabPageMessageDetails);
			this.tabControlMessage.Controls.Add(this.tabPageStatusErrors);
			this.tabControlMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabControlMessage.Name = "tabControlMessage";
			this.tabControlMessage.SelectedIndex = 0;
			this.tabControlMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 348, true);
			this.tabControlMessage.TabIndex = 0;
			// 
			// tabPageMessageText
			// 
			this.tabPageMessageText.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageMessageText.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("ed8a9d27-a0f3-4ca9-8595-ffac5aac2ac8", "Message Text");
			this.tabPageMessageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageMessageText.Name = "tabPageMessageText";
			this.tabPageMessageText.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageMessageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 341, true);
			this.tabPageMessageText.TabIndex = 0;
			this.tabPageMessageText.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageMessageText_InitializeTab));
			// 
			// tabPageMessageDetails
			// 
			this.tabPageMessageDetails.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageMessageDetails.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("d2b58cae-5999-4dc6-9d12-0c20d21a7594", "Message Details");
			this.tabPageMessageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageMessageDetails.Name = "tabPageMessageDetails";
			this.tabPageMessageDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageMessageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 321, true);
			this.tabPageMessageDetails.TabIndex = 1;
			this.tabPageMessageDetails.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageMessageDetails_InitializeTab));
			// 
			// tabPageStatusErrors
			// 
			this.tabPageStatusErrors.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageStatusErrors.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("cfefc9ee-93bc-43af-b87d-844da8557d45", "Status/Errors");
			this.tabPageStatusErrors.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageStatusErrors.Name = "tabPageStatusErrors";
			this.tabPageStatusErrors.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageStatusErrors.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 341, true);
			this.tabPageStatusErrors.TabIndex = 2;
			this.tabPageStatusErrors.RunWhenBindingOrFirstShown(new System.EventHandler(this.tabPageStatusErrors_InitializeTab));
			// 
			// gridMessageHistory
			// 
			this.gridMessageHistory.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridMessageHistory, "Messages");
			this.gridMessageHistory.CaptionVisible = false;
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.ToolTip = "Message Type";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("96184ad0-9849-4b57-84d1-b1017e66bf5f", "Message Time");
			zDateEditColumnStyleInfo3.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("61aba5e0-d070-455f-95a8-07e0d4e35c60", "Interchange No");
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.ToolTip = "Interchange No in which this message was contained.";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("8c642892-d61d-48fc-bf4f-dcd250f5599a", "Interchange Sent");
			zDateEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo5.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.ToolTip = "It is when messages are actually sent to or received from Customs.";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.US.LVS.GUI.Res.GetData("23d14034-c61f-492e-9a38-220d4796e0d8", "Sender");
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.gridMessageHistory.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.gridMessageHistory.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.gridMessageHistory.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.gridMessageHistory.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.gridMessageHistory.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.gridMessageHistory.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.gridMessageHistory.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.gridMessageHistory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridMessageHistory.GridId = "ed5d2ebe-1551-44ef-a60d-e24b3c55735b";
			this.gridMessageHistory.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridMessageHistory.LayoutKey = "gridMessageHistory";
			this.gridMessageHistory.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.gridMessageHistory.Name = "gridMessageHistory";
			this.gridMessageHistory.ReadOnly = true;
			this.gridMessageHistory.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 329, true);
			this.gridMessageHistory.TabIndex = 0;
			this.tabPageMessages.PerformLayout();
			this.splitContainerMessage.Panel1.ResumeLayout(false);
			this.splitContainerMessage.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMessage)).EndInit();
			this.splitContainerMessage.ResumeLayout(false);
			this.splitContainerMessage.PerformLayout();
			this.groupBoxMessageHistory.ResumeLayout(false);
			this.groupBoxMessageHistory.PerformLayout();
			this.tabControlMessage.ResumeLayout(false);
			this.tabControlMessage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridMessageHistory)).EndInit();
			this.gridMessageHistory.ResumeLayout(false);
			this.gridMessageHistory.PerformLayout();
			this.tabPageMessages.ResumeLayout(true);

		}

		private void tabPageMessageText_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.textBoxMessageText = new Enterprise.ZArchitecture.ZTextBox();
			this.tabPageMessageText.SuspendLayout();
			this.tabPageMessageText.Controls.Add(this.textBoxMessageText);
			// 
			// textBoxMessageText
			// 
			this.BindingSource.SetBindingMember(this.textBoxMessageText, "Messages.EM_FormattedMessageText");
			this.textBoxMessageText.CaptionResourceString = null;
			this.textBoxMessageText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxMessageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.textBoxMessageText.Multiline = true;
			this.textBoxMessageText.Name = "textBoxMessageText";
			this.textBoxMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxMessageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 335, true);
			this.textBoxMessageText.TabIndex = 0;
			this.tabPageMessageText.PerformLayout();
			this.tabPageMessageText.ResumeLayout(true);

		}

		private void tabPageMessageDetails_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.textBoxMessageDetails = new Enterprise.ZArchitecture.ZTextBox();
			this.tabPageMessageDetails.SuspendLayout();
			this.tabPageMessageDetails.Controls.Add(this.textBoxMessageDetails);
			// 
			// textBoxMessageDetails
			// 
			this.BindingSource.SetBindingMember(this.textBoxMessageDetails, "Messages.EM_MessageInterpretation");
			this.textBoxMessageDetails.CaptionResourceString = null;
			this.textBoxMessageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxMessageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.textBoxMessageDetails.Multiline = true;
			this.textBoxMessageDetails.Name = "textBoxMessageDetails";
			this.textBoxMessageDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxMessageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 315, true);
			this.textBoxMessageDetails.TabIndex = 0;
			this.textBoxMessageDetails.WordWrap = false;
			this.tabPageMessageDetails.PerformLayout();
			this.tabPageMessageDetails.ResumeLayout(true);

		}

		private void tabPageStatusErrors_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.panelMessageStatusErrors = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.messagesStatusErrorsUserControl1 = new Enterprise.Customs.US.GUI.MessagesStatusErrorsUserControl();
			this.tabPageStatusErrors.SuspendLayout();
			this.panelMessageStatusErrors.SuspendLayout();
			this.messagesStatusErrorsUserControl1.SuspendLayout();
			this.tabPageStatusErrors.Controls.Add(this.panelMessageStatusErrors);
			// 
			// panelMessageStatusErrors
			// 
			this.panelMessageStatusErrors.AutoScroll = true;
			this.panelMessageStatusErrors.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 330, true);
			this.panelMessageStatusErrors.Controls.Add(this.messagesStatusErrorsUserControl1);
			this.panelMessageStatusErrors.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMessageStatusErrors.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panelMessageStatusErrors.Name = "panelMessageStatusErrors";
			this.panelMessageStatusErrors.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 335, true);
			this.panelMessageStatusErrors.TabIndex = 0;
			// 
			// messagesStatusErrorsUserControl1
			// 
			this.messagesStatusErrorsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesStatusErrorsUserControl1, "Messages.StatusesAndErrors");
			this.messagesStatusErrorsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesStatusErrorsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 330, true);
			this.messagesStatusErrorsUserControl1.Name = "messagesStatusErrorsUserControl1";
			this.messagesStatusErrorsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 335, true);
			this.messagesStatusErrorsUserControl1.TabIndex = 0;
			this.tabPageStatusErrors.PerformLayout();
			this.panelMessageStatusErrors.ResumeLayout(false);
			this.panelMessageStatusErrors.PerformLayout();
			this.messagesStatusErrorsUserControl1.ResumeLayout(true);
			this.messagesStatusErrorsUserControl1.PerformLayout();
			this.tabPageStatusErrors.ResumeLayout(true);

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage tabPageCommodities;
		private ZArchitecture.GUI.ZTabPage tabPageStatus;
		private ZArchitecture.GUI.ZTabPage tabPageMessages;
		private ZArchitecture.GUI.ZGroupBox groupBoxSeller;
		private ZArchitecture.GUI.ZGroupBox groupBoxConsignee;
		private ZArchitecture.GUI.ZAddressControl addressControlSeller;
		private ZArchitecture.ZTextBox textBoxSellerName;
		private ZArchitecture.ZTextBox textBoxSellerAddress1;
		private ZArchitecture.ZTextBox textBoxSellerAddress2;
		private ZArchitecture.ZTextBox textBoxSellerCity;
		private ZArchitecture.ZTextBox textBoxSellerState;
		private ZArchitecture.ZTextBox textBoxSellerPostCode;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxSellerCountry;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanelBillDetails;
		private ZArchitecture.GUI.ZGroupBox groupBoxTransportDetails;
		private ZArchitecture.GUI.ZGroupBox groupBoxConsignmentDetails;
		private ZArchitecture.GUI.ZGroupBox groupBoxSummary;
		private ZArchitecture.GUI.ZGroupBox groupBoxLineDetails;
		private ZArchitecture.ZTextBox textBoxCalculatedModeOfTransport;
		private ZArchitecture.GUI.ZDropEdit dropEditModeOfTransport;
		private ZArchitecture.GUI.ZDropEdit dropEditContainerMode;
		private ZArchitecture.GUI.ZPanel panelTransportDetailGroup;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxIssuerSCAC;
		private ZArchitecture.ZTextBox textBoxMasterBill;
		private ZArchitecture.ZTextBox textBoxMailReference;
		private ZArchitecture.ZTextBox textBoxOceanBill;
		private ZArchitecture.ZMasterBillControl masterBillControl;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxVessel;
		private ZArchitecture.ZTextBox textBoxJourney;
		private ZArchitecture.ZTextBox textBoxTripID;
		private ZArchitecture.ZTextBox textBoxVoyageNo;
		private ZArchitecture.ZTextBox textBoxFlightNo;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxCarrierSCAC;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLoadingPort;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLoadingUNLOCO;
		private ZArchitecture.GUI.ZDateEdit dateEditDeparture;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxDischargePort;
		private ZArchitecture.GUI.ZDateEdit dateEditDischarge;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxDischargeUNLOCO;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxEntryPort;
		private ZArchitecture.GUI.ZDateEdit dateEditArrival;
		private ZArchitecture.GUI.ZPanel panelLocationDate;
		private ZArchitecture.ZTextBox textBoxHouseBill;
		private ZArchitecture.ZTextBox textBoxContainer;
		private ZArchitecture.ZTextBox textBoxConsigneeId;
		private ZArchitecture.ZTextBox textBoxSellerId;
		private ZArchitecture.ZTextBox textBoxGoodsValue;
		private ZArchitecture.ZTextBox textBoxCurrency;
		private ZArchitecture.ZTextBox textBoxOwnerRef;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxConsignmentIssuerSCAC;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxUOM;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxCentralizedExamSite;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLocationOfGoods;
		private ZArchitecture.ZTextBox textBoxRailReference;
		private MasterFiles.GUI.ZOrganisationFindBox orgFindBoxMainTabClient;
		private MasterFiles.GUI.ZOrganisationFindBox orgFindBoxMainTabImporter;
		private ZArchitecture.GUI.ZAddressControl addressControlConsignee;
		private ZArchitecture.ZTextBox textBoxConsigneeName;
		private ZArchitecture.ZTextBox textBoxConsigneeAddress1;
		private ZArchitecture.ZTextBox textBoxConsigneeAddress2;
		private ZArchitecture.ZTextBox textBoxConsigneeCity;
		private ZArchitecture.ZTextBox textBoxConsigneeState;
		private ZArchitecture.ZTextBox textBoxConsigneePostCode;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxConsigneeCountry;
		private ZArchitecture.ZTextBox textBoxEntryNumber;
		private ZArchitecture.GUI.ZDateEdit dateEditIssueDate;
		private ZArchitecture.GUI.ZDateEdit dateEditMessageDate;
		private ZArchitecture.ZTextBox textBoxEntryFilerCode;
		private ZArchitecture.ZTextBox textBoxContactPhone;
		private ZArchitecture.ZTextBox textBoxContactName;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxProduct;
		private Enterprise.Customs.GUI.TariffFindBox codeFindBoxTariff;
		private ZArchitecture.ZTextBox textBoxGoodsDescription;
		private ZArchitecture.ZCalcEdit calcEditLineValue;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxLineCurrency;
		private ZArchitecture.ZCalcEdit calcEditExchangeRate;
		private ZArchitecture.GUI.ZCodeFindBox codeFindBoxCountryOfOrigin;
		private ZArchitecture.GUI.ZCheckBox checkBoxADD;
		private ZArchitecture.GUI.ZCheckBox checkBoxCVD;
		private ZArchitecture.GUI.ZCheckBox checkBoxMultipleLines;
		private ZArchitecture.GUI.ZCheckBox checkBoxPGARequirements;
		private ZArchitecture.GUI.ZGroupBox groupBoxCommodityDetails;
		private Enterprise.Customs.US.LVS.GUI.OGAPGARequirementsControl pgaRequirementsControl;
		private ZArchitecture.ZGrid gridCommodityDetails;
		private CargoWise.Windows.UI.KSplitContainer splitContainerCommodities;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanelStatus;
		private ZArchitecture.GUI.ZGroupBox groupBoxCargoReleaseStatus;
		private ZArchitecture.GUI.ZGroupBox groupBoxDisposition;
		private ZArchitecture.ZGrid gridDisposition;
		private ZArchitecture.GUI.ZDateEdit dateEditMessageStatusDate;
		private CargoWise.Windows.UI.KSplitContainer splitContainerMessage;
		private ZArchitecture.GUI.ZGroupBox groupBoxMessageHistory;
		private ZArchitecture.GUI.ZTabControl tabControlMessage;
		private ZArchitecture.GUI.ZTabPage tabPageMessageText;
		private ZArchitecture.GUI.ZTabPage tabPageMessageDetails;
		private ZArchitecture.GUI.ZTabPage tabPageStatusErrors;
		private ZArchitecture.ZGrid gridMessageHistory;
		private ZArchitecture.ZTextBox textBoxMessageText;
		private ZArchitecture.ZTextBox textBoxMessageDetails;
		private ZArchitecture.GUI.ZPanel panelMessageStatusErrors;
		private US.GUI.MessagesStatusErrorsUserControl messagesStatusErrorsUserControl1;
		private ZArchitecture.GUI.ZDropEdit dropEditMainTabEntryStatus;
		private ZArchitecture.GUI.ZDropEdit dropEditMainTabMessageStatus;
		private ZArchitecture.GUI.ZDropEdit dropEditMessageStatus;
		private ZArchitecture.ZCalcEdit calcEditQuantity;
	}
}
