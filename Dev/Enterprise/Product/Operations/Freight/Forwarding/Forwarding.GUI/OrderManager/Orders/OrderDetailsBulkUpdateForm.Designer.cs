using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderDetailsBulkUpdateForm : ZForm
	{
		ZButton UpdateButton;
		ZButton CloseButton;
		ZGroupBox OrdersToUpdateGroupBox;
		ZArchitecture.ZGrid SelectedOrdersBoundGrid;
		ZGroupBox zGroupBox1;
		ZGroupBox zGroupBox2;
		protected OrderPlanningVesselVoyageAndDatesControl OrderPlanningVesselVoyageAndDates;
		protected OrderTrackingDatesControl OrderTrackingDates;
		ZDropEdit JD_TransportModeBoundDropEdit;
		ZArchitecture.ZTextBox PropertyForEnsuringDataEnteredBoundTextBox;
		ZButton DetachButton;
		ZButton AttachButton;
		ZDropEdit zDropEdit1;
		ZTemplateTabControl OrderTabControl;
		ZTabPage OrdersTabPage;
		ZStmNoteTabPage NotesTabPage;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			this.UpdateButton = new ZButton();
			this.CloseButton = new ZButton();
			this.OrdersToUpdateGroupBox = new ZGroupBox();
			this.DetachButton = new ZButton();
			this.AttachButton = new ZButton();
			this.SelectedOrdersBoundGrid = new ZArchitecture.ZGrid();
			this.OrderPlanningVesselVoyageAndDates = new OrderPlanningVesselVoyageAndDatesControl();
			this.zGroupBox1 = new ZGroupBox();
			this.zGroupBox2 = new ZGroupBox();
			this.OrderTrackingDates = new OrderTrackingDatesControl();
			this.JD_TransportModeBoundDropEdit = new ZDropEdit();
			this.PropertyForEnsuringDataEnteredBoundTextBox = new ZArchitecture.ZTextBox();
			this.zDropEdit1 = new ZDropEdit();
			this.OrderTabControl = new ZTemplateTabControl();
			this.OrdersTabPage = new ZTabPage();
			this.NotesTabPage = new ZStmNoteTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrdersToUpdateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedOrdersBoundGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.OrderTabControl.SuspendLayout();
			this.OrdersTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 584, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(348);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(349);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrderDetailsBulkUpdateBusinessObject);
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|5b8ed774-dadd-46e2-bbe7-a63665403ece", "Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 554, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpdateButton.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|3f70ad4a-51df-4589-9d4f-22d92fe7b744", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(648, 554, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			// 
			// OrdersToUpdateGroupBox
			// 
			this.OrdersToUpdateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.OrdersToUpdateGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|d8695157-6758-4723-b64c-b2c6ed8018f6", "Select the orders to update");
			this.OrdersToUpdateGroupBox.Controls.Add(this.DetachButton);
			this.OrdersToUpdateGroupBox.Controls.Add(this.AttachButton);
			this.OrdersToUpdateGroupBox.Controls.Add(this.SelectedOrdersBoundGrid);
			this.OrdersToUpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 39, true);
			this.OrdersToUpdateGroupBox.Name = "OrdersToUpdateGroupBox";
			this.OrdersToUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 362, true);
			this.OrdersToUpdateGroupBox.TabIndex = 2;
			this.OrdersToUpdateGroupBox.TabStop = false;
			// 
			// DetachButton
			// 
			this.DetachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DetachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|5fe1e815-ff9b-49be-82ad-18de932a47b4", "Detach");
			this.DetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 332, true);
			this.DetachButton.Name = "DetachButton";
			this.DetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.DetachButton.TabIndex = 2;
			this.DetachButton.Click += new EventHandler(this.OnDetachButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|ed0d2d87-9971-4efa-a0d1-0eb7f5b2ae7c", "Attach");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 332, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.AttachButton.TabIndex = 1;
			this.AttachButton.Click += new EventHandler(this.OnAttachButton_Click);
			// 
			// SelectedOrdersBoundGrid
			// 
			this.SelectedOrdersBoundGrid.AllowNavigation = false;
			this.SelectedOrdersBoundGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectedOrdersBoundGrid, "SelectedOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OrderDetailsBulkUpdateBusinessObject)(null)).SelectedOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((OrderToBulkUpdate)(((System.Collections.IList)(((OrderDetailsBulkUpdateBusinessObject)(null)).SelectedOrders)).SyncRoot)).OrderNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((OrderToBulkUpdate)(((System.Collections.IList)(((OrderDetailsBulkUpdateBusinessObject)(null)).SelectedOrders)).SyncRoot)).OrderNumberSplit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((OrderToBulkUpdate)(((System.Collections.IList)(((OrderDetailsBulkUpdateBusinessObject)(null)).SelectedOrders)).SyncRoot)).BuyerFK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((OrderToBulkUpdate)(((System.Collections.IList)(((OrderDetailsBulkUpdateBusinessObject)(null)).SelectedOrders)).SyncRoot)).OrderForBinding.SupplierPK)));
			this.SelectedOrdersBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|ff306d48-8ce4-499b-af07-f104b30dfc98", "Order Number");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OrderNumber";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|8e729401-0f2e-4912-9420-cdd050d034dc", "Split");
			zCalcEditColumnStyleInfo1.ColumnName = "OrderNumberSplit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|de2153a4-68b0-4fc0-a9fa-75a82ab74feb", "Buyer");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "BuyerFK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OrderForBinding+SupplierPK";
			zGuidFindBoxColumnStyleInfo2.IsMandatory = true;
			this.SelectedOrdersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SelectedOrdersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SelectedOrdersBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SelectedOrdersBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.SelectedOrdersBoundGrid.GridId = "d6320b7f-06c8-4869-b2e6-b19ac89baf86";
			this.SelectedOrdersBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedOrdersBoundGrid.LayoutKey = "zGrid1";
			this.SelectedOrdersBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.SelectedOrdersBoundGrid.Name = "SelectedOrdersBoundGrid";
			this.SelectedOrdersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 307, true);
			this.SelectedOrdersBoundGrid.TabIndex = 0;
			// 
			// OrderPlanningVesselVoyageAndDates
			// 
			this.BindingSource.SetBindingMember(this.OrderPlanningVesselVoyageAndDates, ".");
			this.OrderPlanningVesselVoyageAndDates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.OrderPlanningVesselVoyageAndDates.Name = "OrderPlanningVesselVoyageAndDates";
			this.OrderPlanningVesselVoyageAndDates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 87, true);
			this.OrderPlanningVesselVoyageAndDates.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|e5dd7636-84f3-4cf8-8bd3-a118cf99a7fe", "Vessel Information to Update");
			this.zGroupBox1.Controls.Add(this.OrderPlanningVesselVoyageAndDates);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 407, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 106, true);
			this.zGroupBox1.TabIndex = 7;
			this.zGroupBox1.TabStop = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|24032267-fd30-4e60-b8a6-2152ca1c88a9", "Tracking Dates to Update");
			this.zGroupBox2.Controls.Add(this.OrderTrackingDates);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 39, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 326, true);
			this.zGroupBox2.TabIndex = 6;
			this.zGroupBox2.TabStop = false;
			// 
			// OrderTrackingDates
			// 
			this.BindingSource.SetBindingMember(this.OrderTrackingDates, ".");
			this.OrderTrackingDates.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 15, true);
			this.OrderTrackingDates.Name = "OrderTrackingDates";
			this.OrderTrackingDates.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 307, true);
			this.OrderTrackingDates.TabIndex = 0;
			// 
			// JD_TransportModeBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JD_TransportModeBoundDropEdit, "JD_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderDetailsBulkUpdateBusinessObject)(null)).JD_TransportMode)));
			this.JD_TransportModeBoundDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|4998fa1f-5e98-48e9-9eac-87a29d8b15c1", "Trans. Mode");
			this.JD_TransportModeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 9, true);
			this.JD_TransportModeBoundDropEdit.Name = "JD_TransportModeBoundDropEdit";
			this.JD_TransportModeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JD_TransportModeBoundDropEdit.TabIndex = 1;
			// 
			// PropertyForEnsuringDataEnteredBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PropertyForEnsuringDataEnteredBoundTextBox, "PropertyForEnsuringDataEntered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((OrderDetailsBulkUpdateBusinessObject)(null)).PropertyForEnsuringDataEntered)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PropertyForEnsuringDataEnteredBoundTextBox, false);
			this.PropertyForEnsuringDataEnteredBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 8, true);
			this.PropertyForEnsuringDataEnteredBoundTextBox.Name = "PropertyForEnsuringDataEnteredBoundTextBox";
			this.PropertyForEnsuringDataEnteredBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 20, true);
			this.PropertyForEnsuringDataEnteredBoundTextBox.TabIndex = 3;
			this.PropertyForEnsuringDataEnteredBoundTextBox.Text = "ZTEXTBOX1";
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "JD_OrderStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OrderDetailsBulkUpdateBusinessObject)(null)).JD_OrderStatus)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|8ea38fdc-4ce0-428a-9121-b15447b5c6af", "Status");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 9, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit1.TabIndex = 5;
			// 
			// OrderTabControl
			// 
			this.OrderTabControl.Controls.Add(this.OrdersTabPage);
			this.OrderTabControl.Controls.Add(this.NotesTabPage);
			this.OrderTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 19, true);
			this.OrderTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.OrderTabControl.Name = "OrderTabControl";
			this.OrderTabControl.SelectedIndex = 0;
			this.OrderTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 546, true);
			this.OrderTabControl.TabIndex = 0;
			// 
			// OrdersTabPage
			// 
			this.OrdersTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|75a8a850-e402-43e1-a0b3-d5614d9def84", "Order");
			this.OrdersTabPage.Controls.Add(this.PropertyForEnsuringDataEnteredBoundTextBox);
			this.OrdersTabPage.Controls.Add(this.zDropEdit1);
			this.OrdersTabPage.Controls.Add(this.OrdersToUpdateGroupBox);
			this.OrdersTabPage.Controls.Add(this.zGroupBox1);
			this.OrdersTabPage.Controls.Add(this.zGroupBox2);
			this.OrdersTabPage.Controls.Add(this.JD_TransportModeBoundDropEdit);
			this.OrdersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OrdersTabPage.Name = "OrdersTabPage";
			this.OrdersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 519, true);
			this.OrdersTabPage.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 528, true);
			this.NotesTabPage.TabIndex = 4;
			// 
			// OrderDetailsBulkUpdateForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 606, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderDetailsBulkUpdateForm|00e669dd-acc6-4ab6-b0b7-63efaca2a5e3", "Order Bulk Update");
			this.Controls.Add(this.OrderTabControl);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.UpdateButton);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(OrderDetailsBulkUpdateBusinessObject);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Orders.Business.OrderDetailsBulkUpdateBusinessObjec" +
				"t";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 642, true);
			this.Name = "OrderDetailsBulkUpdateForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrderTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrdersToUpdateGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SelectedOrdersBoundGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox2.ResumeLayout(false);
			this.OrderTabControl.ResumeLayout(false);
			this.OrdersTabPage.ResumeLayout(false);
			this.OrdersTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
