using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class JobShipmentPreplanningForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		private ZGuidFindBox zGuidFindBox3;
		private ZGuidFindBox zGuidFindBox2;
		private ZGuidFindBox zGuidFindBox1;
		private ZTextBox HouseBillTextBox;
		private OrderModuleButtonGrid OrdersGrid;
		private ZGroupBox zGroupBox1;
		private ZDropEdit zDropEdit2;
		private ZDropEdit zDropEdit1;
		private ZCalcEdit zCalcEdit2;
		private ZCalcEdit zCalcEdit1;
		private ZGuidFindBox zGuidFindBox5;
		private ZGuidFindBox zGuidFindBox4;
		private ZCodeFindBox zCodeFindBox2;
		private ZCodeFindBox zCodeFindBox1;
		private ZDropEdit zDropEdit3;
		private ZCalcEdit zCalcEdit3;
		private ZGroupBox zGroupBox2;
		private ZWorkflowTabPage WorkflowTabPage;
		private ZTextBox zTextBox1;
		private ZTextBox zMasterBillControl1;
		private ZCheckBox zCheckBox1;
		private ZButton DeliveryWizardButton;
		private ZLabel LockedLabel;
		private ZGrid ContainersGrid;
		private ZGroupBox OrdersGroupBox;
		private ZPanel zPanel1;
		private MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl BuyerZAddressWithContactControl;

		new void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			this.HouseBillTextBox = new ZTextBox();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.zGuidFindBox2 = new ZGuidFindBox();
			this.zGuidFindBox3 = new ZGuidFindBox();
			this.OrdersGrid = new OrderModuleButtonGrid();
			this.zGroupBox1 = new ZGroupBox();
			this.zDropEdit3 = new ZDropEdit();
			this.zCalcEdit3 = new ZCalcEdit();
			this.zDropEdit2 = new ZDropEdit();
			this.zDropEdit1 = new ZDropEdit();
			this.zCalcEdit2 = new ZCalcEdit();
			this.zCalcEdit1 = new ZCalcEdit();
			this.zGuidFindBox5 = new ZGuidFindBox();
			this.zGuidFindBox4 = new ZGuidFindBox();
			this.zCodeFindBox1 = new ZCodeFindBox();
			this.zCodeFindBox2 = new ZCodeFindBox();
			this.zGroupBox2 = new ZGroupBox();
			this.ContainersGrid = new ZGrid();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zMasterBillControl1 = new ZTextBox();
			this.zTextBox1 = new ZTextBox();
			this.zCheckBox1 = new ZCheckBox();
			this.DeliveryWizardButton = new ZButton();
			this.LockedLabel = new ZLabel();
			this.zPanel1 = new ZPanel();
			this.BuyerZAddressWithContactControl = new MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			this.OrdersGroupBox = new ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGuidFindBox1.SuspendLayout();
			this.zGuidFindBox2.SuspendLayout();
			this.zGuidFindBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid.InnerGrid)).BeginInit();
			this.OrdersGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zGuidFindBox5.SuspendLayout();
			this.zGuidFindBox4.SuspendLayout();
			this.zCodeFindBox1.SuspendLayout();
			this.zCodeFindBox2.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.BuyerZAddressWithContactControl.SuspendLayout();
			this.OrdersGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 608, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.OrdersGroupBox);
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 585, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 585, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 585, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 608, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(983);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobShipmentPreplanning);
			// 
			// HouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseBillTextBox, "EF_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_HouseBill)));
			this.HouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 67, true);
			this.HouseBillTextBox.Name = "HouseBillTextBox";
			this.HouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.HouseBillTextBox.TabIndex = 5;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "EF_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(null)).EF_OH_Carrier)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 47, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.zGuidFindBox1.TabIndex = 13;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "EF_OH_SendingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(null)).EF_OH_SendingAgent)));
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 68, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.zGuidFindBox2.TabIndex = 15;
			// 
			// zGuidFindBox3
			// 
			this.zGuidFindBox3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox3, "EF_OH_ReceivingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(null)).EF_OH_ReceivingAgent)));
			this.zGuidFindBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 90, true);
			this.zGuidFindBox3.Name = "zGuidFindBox3";
			this.zGuidFindBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 18, true);
			this.zGuidFindBox3.TabIndex = 17;
			// 
			// OrdersGrid
			// 
			this.OrdersGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrdersGrid, "Orders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Orders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Lookups.Orders)));
			this.OrdersGrid.BindToFindBoxList = "Lookups+Orders";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderModuleButtonGrid|fe43dfe2-fbea-41bc-8bfd-15416903db85", "Order Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.ColumnName = "JD_OrderNumberSplit";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "JD_OrderDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderModuleButtonGrid|a1c75805-e3ec-43e9-8d9e-ed7e0b5e1276", "Buyer");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "BuyerPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderModuleButtonGrid|666441ec-9b86-456f-b4ac-d16a9df6ae8f", "Supplier");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "SupplierPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "JD_TransportMode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JD_ContainerMode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "JD_OrderStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JD_JS";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JD_Calc_LineCount";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JD_Calc_InnerPacks";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JD_Calc_OuterPacks";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JD_Calc_TotalQuantity";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JD_Calc_TotalQuantityInvoiced";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JD_Calc_TotalQuantityReceived";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "JD_Calc_TotalQuantityRemaining";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrdersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OrdersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OrdersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.OrdersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGrid.GridId = "c3f30b8a-ba9a-43cb-baf5-05200c69e933";
			// 
			// 
			// 
			this.OrdersGrid.InnerGrid.AllowNavigation = false;
			this.OrdersGrid.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrdersGrid.InnerGrid.CaptionVisible = false;
			this.OrdersGrid.InnerGrid.GridId = null;
			this.OrdersGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdersGrid.InnerGrid.LayoutKey = "Grid";
			this.OrdersGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OrdersGrid.InnerGrid.Name = "Grid";
			this.OrdersGrid.InnerGrid.ReadOnly = true;
			this.OrdersGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 203, true);
			this.OrdersGrid.InnerGrid.TabIndex = 0;
			this.OrdersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.OrdersGrid.Name = "OrdersGrid";
			this.OrdersGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("FBDDC2ED-6A16-4057-9F6B-0BCD01AD1C1B", "Order");
			this.OrdersGrid.ReadOnly = true;
			this.OrdersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(985, 240, true);
			this.OrdersGrid.TabIndex = 22;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|aa0a923e-5233-484f-88bf-ec0aef1cbf3d", "Shipment Information");
			this.zGroupBox1.Controls.Add(this.zDropEdit3);
			this.zGroupBox1.Controls.Add(this.zCalcEdit3);
			this.zGroupBox1.Controls.Add(this.zDropEdit2);
			this.zGroupBox1.Controls.Add(this.zDropEdit1);
			this.zGroupBox1.Controls.Add(this.zCalcEdit2);
			this.zGroupBox1.Controls.Add(this.zCalcEdit1);
			this.zGroupBox1.Controls.Add(this.zGuidFindBox5);
			this.zGroupBox1.Controls.Add(this.zGuidFindBox4);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 178, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 150, true);
			this.zGroupBox1.TabIndex = 19;
			this.zGroupBox1.TabStop = false;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "EF_F3_NKPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobShipmentPreplanning)(null)).EF_F3_NKPackType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit3, false);
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 121, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 3;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
			this.zDropEdit3.TabIndex = 12;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "EF_Packs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobShipmentPreplanning)(null)).EF_Packs)));
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 121, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.zCalcEdit3.TabIndex = 11;
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "EF_UnitOfVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobShipmentPreplanning)(null)).EF_UnitOfVolume)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit2, false);
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 99, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 3;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
			this.zDropEdit2.TabIndex = 9;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "EF_UnitOfWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobShipmentPreplanning)(null)).EF_UnitOfWeight)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zDropEdit1, false);
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 78, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 18, true);
			this.zDropEdit1.TabIndex = 6;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "EF_ActualVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobShipmentPreplanning)(null)).EF_ActualVolume)));
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 99, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.zCalcEdit2.TabIndex = 8;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "EF_ActualWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((JobShipmentPreplanning)(null)).EF_ActualWeight)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 78, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 18, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox5
			// 
			this.zGuidFindBox5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox5, "EF_JE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(null)).EF_JE)));
			this.zGuidFindBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 41, true);
			this.zGuidFindBox5.Name = "zGuidFindBox5";
			this.zGuidFindBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 18, true);
			this.zGuidFindBox5.TabIndex = 3;
			// 
			// zGuidFindBox4
			// 
			this.zGuidFindBox4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox4, "EF_JS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((JobShipmentPreplanning)(null)).EF_JS)));
			this.zGuidFindBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 19, true);
			this.zGuidFindBox4.Name = "zGuidFindBox4";
			this.zGuidFindBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 18, true);
			this.zGuidFindBox4.TabIndex = 1;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "EF_RL_NKPortLoad");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_RL_NKPortLoad)));
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 90, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.zCodeFindBox1.TabIndex = 9;
			// 
			// zCodeFindBox2
			// 
			this.zCodeFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox2, "EF_RL_NKPortDisch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_RL_NKPortDisch)));
			this.zCodeFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 111, true);
			this.zCodeFindBox2.Name = "zCodeFindBox2";
			this.zCodeFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.zCodeFindBox2.TabIndex = 11;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|eadb06e7-a437-4d66-8520-5532f3f9301c", "Containers");
			this.zGroupBox2.Controls.Add(this.ContainersGrid);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(441, 178, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 150, true);
			this.zGroupBox2.TabIndex = 19;
			this.zGroupBox2.TabStop = false;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_RC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_AdditionalSealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OrderContainer)(((System.Collections.IList)(((JobShipmentPreplanning)(null)).Containers)).SyncRoot)).J1_Additional2SealNum)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "J1_ContainerNumber";
			zTextBoxColumnStyleInfo6.ToolTip = "Enter the Container Number";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "J1_ContainerCount";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.ToolTip = "Enter the container count";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "J1_RC";
			zGuidFindBoxColumnStyleInfo2.ToolTip = "Enter the container type";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.ColumnName = "J1_SealNum";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "J1_AdditionalSealNum";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "J1_Additional2SealNum";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "f9ba46f7-e518-48ab-af38-b740ea8fbec6";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 133, true);
			this.ContainersGrid.TabIndex = 20;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|2ff51c59-7154-4e7c-a816-eecec7ab8ef1", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 553, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// zMasterBillControl1
			// 
			this.BindingSource.SetBindingMember(this.zMasterBillControl1, "EF_MasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_MasterBill)));
			this.zMasterBillControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 46, true);
			this.zMasterBillControl1.Name = "zMasterBillControl1";
			this.zMasterBillControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.zMasterBillControl1.TabIndex = 3;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EF_PreshipID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).EF_PreshipID)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 25, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 18, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "EF_IsCancelled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((JobShipmentPreplanning)(null)).EF_IsCancelled)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(882, 197, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.zCheckBox1.TabIndex = 20;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// DeliveryWizardButton
			// 
			this.DeliveryWizardButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DeliveryWizardButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|9ab00711-8a0d-452b-a865-fbad23fcae96", "Product Delivery Wizard");
			this.DeliveryWizardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 225, true);
			this.DeliveryWizardButton.Name = "DeliveryWizardButton";
			this.DeliveryWizardButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeliveryWizardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.DeliveryWizardButton.TabIndex = 24;
			this.DeliveryWizardButton.UseVisualStyleBackColor = true;
			this.DeliveryWizardButton.Click += new EventHandler(this.DeliveryWizardButton_Click);
			// 
			// LockedLabel
			// 
			this.BindingSource.SetBindingMember(this.LockedLabel, "PreAdviceLockedMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((JobShipmentPreplanning)(null)).PreAdviceLockedMessage)));
			this.LockedLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|2a83e7e9-c3e7-4094-883d-f50f806f1c57", "<Pre Advice Locked Message>");
			this.LockedLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LockedLabel.ForeColor = System.Drawing.Color.Red;
			this.LockedLabel.IsFontBold = true;
			this.LockedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 7, true);
			this.LockedLabel.Name = "LockedLabel";
			this.LockedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 15, true);
			this.LockedLabel.TabIndex = 25;
			this.LockedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.BuyerZAddressWithContactControl);
			this.zPanel1.Controls.Add(this.LockedLabel);
			this.zPanel1.Controls.Add(this.zCheckBox1);
			this.zPanel1.Controls.Add(this.HouseBillTextBox);
			this.zPanel1.Controls.Add(this.zTextBox1);
			this.zPanel1.Controls.Add(this.zMasterBillControl1);
			this.zPanel1.Controls.Add(this.zCodeFindBox1);
			this.zPanel1.Controls.Add(this.zGroupBox2);
			this.zPanel1.Controls.Add(this.zCodeFindBox2);
			this.zPanel1.Controls.Add(this.zGroupBox1);
			this.zPanel1.Controls.Add(this.zGuidFindBox3);
			this.zPanel1.Controls.Add(this.zGuidFindBox2);
			this.zPanel1.Controls.Add(this.zGuidFindBox1);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 328, true);
			this.zPanel1.TabIndex = 26;
			// 
			// BuyerZAddressWithContactControl
			// 
			this.BuyerZAddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerZAddressWithContactControl, "BuyerZAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.ZAddressWithContact)(((JobShipmentPreplanning)(null)).BuyerZAddressWithContact)));
			this.BuyerZAddressWithContactControl.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("e808014b-ccfb-4245-963c-7a4859820bae", "Buyer");
			this.BuyerZAddressWithContactControl.ContactInfoTabVisible = false;
			this.BuyerZAddressWithContactControl.InvoiceContactTabCaption = Enterprise.Freight.Forwarding.GUI.Res.GetData("ad29b20b-6e2d-43c9-9ba0-28ebb23e512d", "Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BuyerZAddressWithContactControl, false);
			this.BuyerZAddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 25, true);
			this.BuyerZAddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.Name = "BuyerZAddressWithContactControl";
			this.BuyerZAddressWithContactControl.OnlyStopOnDebtor = false;
			this.BuyerZAddressWithContactControl.PopupCaption = "";
			this.BuyerZAddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.BuyerZAddressWithContactControl.TabIndex = 18;
			// 
			// OrdersGroupBox
			// 
			this.OrdersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("JobShipmentPreplanningForm|732f4b6f-ff79-428b-8758-1d146ca37520", "Orders");
			this.OrdersGroupBox.Controls.Add(this.DeliveryWizardButton);
			this.OrdersGroupBox.Controls.Add(this.OrdersGrid);
			this.OrdersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.OrdersGroupBox.Name = "OrdersGroupBox";
			this.OrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 257, true);
			this.OrdersGroupBox.TabIndex = 27;
			this.OrdersGroupBox.TabStop = false;
			// 
			// JobShipmentPreplanningForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 664, true);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(JobShipmentPreplanning);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 698, true);
			this.Name = "JobShipmentPreplanningForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.zGuidFindBox2.ResumeLayout(true);
			this.zGuidFindBox2.PerformLayout();
			this.zGuidFindBox3.ResumeLayout(true);
			this.zGuidFindBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrdersGrid.InnerGrid)).EndInit();
			this.OrdersGrid.ResumeLayout(true);
			this.OrdersGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zGuidFindBox5.ResumeLayout(true);
			this.zGuidFindBox5.PerformLayout();
			this.zGuidFindBox4.ResumeLayout(true);
			this.zGuidFindBox4.PerformLayout();
			this.zCodeFindBox1.ResumeLayout(true);
			this.zCodeFindBox1.PerformLayout();
			this.zCodeFindBox2.ResumeLayout(true);
			this.zCodeFindBox2.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.BuyerZAddressWithContactControl.ResumeLayout(true);
			this.BuyerZAddressWithContactControl.PerformLayout();
			this.OrdersGroupBox.ResumeLayout(false);
			this.OrdersGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
