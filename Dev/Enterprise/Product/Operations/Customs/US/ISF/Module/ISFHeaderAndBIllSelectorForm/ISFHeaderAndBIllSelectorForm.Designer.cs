using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using System.Windows.Forms;

namespace Enterprise.Customs.US.ISF.Module
{
	partial class ISFHeaderAndBIllSelectorForm
	{
		new void InitializeComponent()
		{
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new ZGuidDropEditColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo4 = new ZGuidDropEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo5 = new ZGuidDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			this.containerGroupBox = new ZGroupBox();
			this.containerGrid = new ZGrid();
			this.headerGroupBox = new ZGroupBox();
			this.billsGrid = new ZGrid();
			this.bottomPanel = new ZPanel();
			this.cancelAndClossButton = new ZButton();
			this.CreateButton = new ZButton();
			this.lineGroupBox = new ZGroupBox();
			this.lineGrid = new ZGrid();
			this.topPanel = new ZPanel();
			this.masterBillGuidDropEdit = new ZGuidDropEdit();
			this.consolPPKGuidFindBox = new ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.containerGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containerGrid)).BeginInit();
			this.headerGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).BeginInit();
			this.bottomPanel.SuspendLayout();
			this.lineGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineGrid)).BeginInit();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 446, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ISFHeaderRow);
			// 
			// ContainerGroupBox
			// 
			this.containerGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|af1f055f-97ee-4aaf-909b-29926477d762", "Containers To Copy");
			this.containerGroupBox.Controls.Add(this.containerGrid);
			this.containerGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.containerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.containerGroupBox.Name = "ContainerGroupBox";
			this.containerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 100, true);
			this.containerGroupBox.TabIndex = 1;
			this.containerGroupBox.TabStop = false;
			// 
			// ContainerGrid
			// 
			this.containerGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.containerGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ISFHeaderRow)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ISFContainerRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Containers)).SyncRoot)).ShouldCopy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFContainerRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Containers)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFContainerRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Containers)).SyncRoot)).ContainerISOType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFContainerRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Containers)).SyncRoot)).ContainerUSCode)));
			this.containerGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldCopy";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ContainerISOType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "ContainerUSCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			this.containerGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.containerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.containerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.containerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.containerGrid.GridId = "74497e1c-490c-41d0-90b0-8c9f6904fa55";
			this.containerGrid.CopySelectedRowsAllowed = false;
			this.containerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containerGrid.LayoutKey = "BillsGrid";
			this.containerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.containerGrid.Name = "ContainerGrid";
			this.containerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 81, true);
			this.containerGrid.TabIndex = 1;
			// 
			// HeaderGroupBox
			// 
			this.headerGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|5d9bd155-d691-4a04-9bc4-3a9aace2c87d", "Job to create shipment for");
			this.headerGroupBox.Controls.Add(this.billsGrid);
			this.headerGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.headerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 135, true);
			this.headerGroupBox.Name = "HeaderGroupBox";
			this.headerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 117, true);
			this.headerGroupBox.TabIndex = 2;
			this.headerGroupBox.TabStop = false;
			// 
			// BillsGrid
			// 
			this.billsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.billsGrid, "Bills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).HouseBillPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).OceanBillPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).BuyingPartyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).SellingPartyPK)));
			this.billsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|57c75687-9c72-4955-a656-77f514cfa9e5", "House Bill");
			zGuidDropEditColumnStyleInfo1.ColumnName = "HouseBillPK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|397427ee-22af-4a70-b91c-a005b0493514", "Ocean Bill");
			zGuidDropEditColumnStyleInfo2.ColumnName = "OceanBillPK";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zGuidDropEditColumnStyleInfo3.ColumnName = "BuyingPartyPK";
			zGuidDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zGuidDropEditColumnStyleInfo4.ColumnName = "SellingPartyPK";
			zGuidDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.billsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.billsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.billsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.billsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo4);
			this.billsGrid.GridId = "797e15d8-7215-4586-b00c-a035f7d2d047";
			this.billsGrid.CopySelectedRowsAllowed = false;
			this.billsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.billsGrid.LayoutKey = "BillsGrid";
			this.billsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.billsGrid.Name = "BillsGrid";
			this.billsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 98, true);
			this.billsGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.bottomPanel.Controls.Add(this.cancelAndClossButton);
			this.bottomPanel.Controls.Add(this.CreateButton);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 416, true);
			this.bottomPanel.Name = "BottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 30, true);
			this.bottomPanel.TabIndex = 4;
			// 
			// CancelAndClossButton
			// 
			this.cancelAndClossButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelAndClossButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelAndClossButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(506, 4, true);
			this.cancelAndClossButton.Name = "CancelAndClossButton";
			this.cancelAndClossButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelAndClossButton.TabIndex = 1;
			this.cancelAndClossButton.Text = "Cancel";
			this.cancelAndClossButton.Click += new EventHandler(this.CancelButton_Click);
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 4, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.CreateButton.TabIndex = 0;
			this.CreateButton.Text = "Create Shipment";
			this.CreateButton.Click += new EventHandler(this.CreateButton_Click);
			// 
			// LineGroupBox
			// 
			this.lineGroupBox.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|1144ab3f-37da-4e5b-9e32-cf14245eb53d", "Lines To Copy");
			this.lineGroupBox.Controls.Add(this.lineGrid);
			this.lineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
			this.lineGroupBox.Name = "LineGroupBox";
			this.lineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 164, true);
			this.lineGroupBox.TabIndex = 3;
			this.lineGroupBox.TabStop = false;
			// 
			// LineGrid
			// 
			this.lineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.lineGrid, "Bills.Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ISFLineRow)(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)).SyncRoot)).ShouldCopy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFLineRow)(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)).SyncRoot)).ContainerPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFLineRow)(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)).SyncRoot)).HarmonisedNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFLineRow)(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)).SyncRoot)).GoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ISFLineRow)(((System.Collections.IList)(((ISFBillRow)(((System.Collections.IList)(((ISFHeaderRow)(null)).Bills)).SyncRoot)).Lines)).SyncRoot)).ManufacturerAddressText)));
			this.lineGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "ShouldCopy";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zGuidDropEditColumnStyleInfo5.ColumnName = "ContainerPK";
			zGuidDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo4.ColumnName = "HarmonisedNum";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.ColumnName = "GoodsOrigin";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zTextBoxColumnStyleInfo6.ColumnName = "ManufacturerAddressText";
			zTextBoxColumnStyleInfo6.Caption = "Manufacturer";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.lineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.lineGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo5);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.lineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.lineGrid.GridId = "5fcc8c4b-e3c5-440e-8d13-5191a239e5f7";
			this.lineGrid.CopySelectedRowsAllowed = true;
			this.lineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.lineGrid.LayoutKey = "LineGrid";
			this.lineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.lineGrid.Name = "LineGrid";
			this.lineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 145, true);
			this.lineGrid.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.topPanel.Controls.Add(this.masterBillGuidDropEdit);
			this.topPanel.Controls.Add(this.consolPPKGuidFindBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "TopPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 35, true);
			this.topPanel.TabIndex = 0;
			// 
			// MasterBillGuidDropEdit
			// 
			this.masterBillGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.masterBillGuidDropEdit, "MasterBillPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ISFHeaderRow)(null)).MasterBillPK)));
			this.masterBillGuidDropEdit.CaptionResourceString = Enterprise.Customs.US.ISF.Module.Res.GetData("ISFHeaderAndBIllSelectorForm|583cf7dc-7028-47f9-8be4-87e24a172e1b", "Master Bill");
			this.masterBillGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 9, true);
			this.masterBillGuidDropEdit.Name = "MasterBillGuidDropEdit";
			this.masterBillGuidDropEdit.PreBoundMaxLength = 24;
			this.masterBillGuidDropEdit.ShowDescriptionBox = false;
			this.masterBillGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.masterBillGuidDropEdit.TabIndex = 0;
			// 
			// ConsolPPKGuidFindBox
			// 
			this.consolPPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consolPPKGuidFindBox, "ConsolPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ISFHeaderRow)(null)).ConsolPK)));
			this.consolPPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 9, true);
			this.consolPPKGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
			this.consolPPKGuidFindBox.Name = "ConsolPPKGuidFindBox";
			this.consolPPKGuidFindBox.ShowDescriptionBox = false;
			this.consolPPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.consolPPKGuidFindBox.TabIndex = 1;
			// 
			// ISFHeaderAndBIllSelectorForm
			// 

			this.CancelButton = this.cancelAndClossButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 468, true);
			this.Controls.Add(this.lineGroupBox);
			this.Controls.Add(this.headerGroupBox);
			this.Controls.Add(this.containerGroupBox);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(ISFHeaderRow);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "ISFHeaderAndBIllSelectorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.topPanel, 0);
			this.Controls.SetChildIndex(this.containerGroupBox, 0);
			this.Controls.SetChildIndex(this.headerGroupBox, 0);
			this.Controls.SetChildIndex(this.lineGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.containerGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containerGrid)).EndInit();
			this.headerGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.billsGrid)).EndInit();
			this.bottomPanel.ResumeLayout(false);
			this.lineGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.lineGrid)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		ZGrid containerGrid;
		ZGroupBox containerGroupBox;
		ZGroupBox headerGroupBox;
		ZGrid billsGrid;
		ZPanel bottomPanel;
		public ZButton CreateButton;
		ZGroupBox lineGroupBox;
		ZGrid lineGrid;
		ZPanel topPanel;
		ZGuidFindBox consolPPKGuidFindBox;
		ZGuidDropEdit masterBillGuidDropEdit;
		ZButton cancelAndClossButton;
	}
}
