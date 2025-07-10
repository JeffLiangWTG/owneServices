using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersAttachUserControl
	{
		#region Component Designer generated code

		ZGroupBox OrdersGroupBox;
		internal OrdersModuleButtonGrid OrdersButtonGrid;
		internal ZTextBox JP_OrderItemsAsStringTextBox;
		internal ZButton OrderItemsEditButton;
		private ZPanel OrderReferencesPanel;
		private ZPanel OrderReferencesTextBoxPanel;
		private ZPanel OrderReferencesButtonPanel;
		System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrdersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrderReferencesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrderReferencesTextBoxPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JP_OrderItemsAsStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrderReferencesButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrderItemsEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrdersButtonGrid = new Enterprise.Freight.Forwarding.Orders.GUI.OrdersModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrdersGroupBox.SuspendLayout();
			this.OrderReferencesPanel.SuspendLayout();
			this.OrderReferencesTextBoxPanel.SuspendLayout();
			this.OrderReferencesButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders);
			// 
			// OrdersGroupBox
			// 
			this.OrdersGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersAttachUserControl|2fe29bae-c88e-4603-9f59-18537e2fad16", "Order Management Links");
			this.OrdersGroupBox.Controls.Add(this.OrderReferencesPanel);
			this.OrdersGroupBox.Controls.Add(this.OrdersButtonGrid);
			this.OrdersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrdersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrdersGroupBox.Name = "OrdersGroupBox";
			this.OrdersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 336, true);
			this.OrdersGroupBox.TabIndex = 16;
			this.OrdersGroupBox.TabStop = false;
			// 
			// OrderReferencesPanel
			// 
			this.OrderReferencesPanel.Controls.Add(this.OrderReferencesTextBoxPanel);
			this.OrderReferencesPanel.Controls.Add(this.OrderReferencesButtonPanel);
			this.OrderReferencesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrderReferencesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrderReferencesPanel.Name = "OrderReferencesPanel";
			this.OrderReferencesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 22, true);
			this.OrderReferencesPanel.TabIndex = 22;
			// 
			// OrderReferencesTextBoxPanel
			// 
			this.OrderReferencesTextBoxPanel.Controls.Add(this.JP_OrderItemsAsStringTextBox);
			this.OrderReferencesTextBoxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderReferencesTextBoxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderReferencesTextBoxPanel.Name = "OrderReferencesTextBoxPanel";
			this.OrderReferencesTextBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 22, true);
			this.OrderReferencesTextBoxPanel.TabIndex = 1;
			// 
			// JP_OrderItemsAsStringTextBox
			// 
			this.JP_OrderItemsAsStringTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JP_OrderItemsAsStringTextBox, "DocsAndCartage.JP_OrderItemsAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders)(null)).DocsAndCartage.JP_OrderItemsAsString)));
			this.JP_OrderItemsAsStringTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JP_OrderItemsAsStringTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersAttachUserControl|d367ad48-b5d4-4b94-b4ef-7623eee7afdf", "Order Refs", "Order References", "Enter the Order References.");
			this.JP_OrderItemsAsStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 1, true);
			this.JP_OrderItemsAsStringTextBox.Name = "JP_OrderItemsAsStringTextBox";
			this.JP_OrderItemsAsStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.JP_OrderItemsAsStringTextBox.TabIndex = 20;
			// 
			// OrderReferencesButtonPanel
			// 
			this.OrderReferencesButtonPanel.Controls.Add(this.OrderItemsEditButton);
			this.OrderReferencesButtonPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.OrderReferencesButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.OrderReferencesButtonPanel.Name = "OrderReferencesButtonPanel";
			this.OrderReferencesButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 22, true);
			this.OrderReferencesButtonPanel.TabIndex = 0;
			// 
			// OrderItemsEditButton
			// 
			this.OrderItemsEditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OrderItemsEditButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrdersAttachUserControl|080893bd-92be-4bec-af36-32fb2e4330f9", "More...");
			this.OrderItemsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.OrderItemsEditButton.Name = "OrderItemsEditButton";
			this.OrderItemsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.OrderItemsEditButton.TabIndex = 21;
			this.OrderItemsEditButton.Click += new System.EventHandler(this.OrderReferencesButton_Click);
			// 
			// OrdersButtonGrid
			// 
			this.OrdersButtonGrid.AllowNewWithoutSaving = true;
			this.OrdersButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrdersButtonGrid, "AttachedOrders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders)(null)).AttachedOrders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Orders.Business.IAttachOrders)(null)).PossibleOrdersForAttachment_List)));
			this.OrdersButtonGrid.BindToFindBoxList = "PossibleOrdersForAttachment_List";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ZModuleButtonGrid|4dc8d8d3-c521-4cbb-85ad-2b4463e712a6", "Order Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDateEditColumnStyleInfo1.ColumnName = "JD_OrderDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo2.ColumnName = "JD_OrderGoodsDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.OrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrdersButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OrdersButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrdersButtonGrid.GridId = "98b8677d-e4c0-4025-9019-1bc4d37d2f49";
			// 
			// 
			// 
			this.OrdersButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrdersButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrdersButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrdersButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrdersButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.OrdersButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrdersButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrdersButtonGrid.InnerGrid.Name = "Grid";
			this.OrdersButtonGrid.InnerGrid.ReadOnly = true;
			this.OrdersButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 251, true);
			this.OrdersButtonGrid.InnerGrid.TabIndex = 0;
			this.OrdersButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.OrdersButtonGrid.Name = "OrdersButtonGrid";
			this.OrdersButtonGrid.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("30C5411A-9C41-41A0-ADD3-3E3AFC5F4C0C", "Order");
			this.OrdersButtonGrid.ReadOnly = true;
			this.OrdersButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 289, true);
			this.OrdersButtonGrid.TabIndex = 10;
			// 
			// OrdersAttachUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrdersGroupBox);
			this.Name = "OrdersAttachUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 336, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrdersGroupBox.ResumeLayout(false);
			this.OrderReferencesPanel.ResumeLayout(false);
			this.OrderReferencesTextBoxPanel.ResumeLayout(false);
			this.OrderReferencesTextBoxPanel.PerformLayout();
			this.OrderReferencesButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
	}
}
