using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class WhsWorkOrderAssemblyConfirmationUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            this.LinkedComponentsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.AssemblyGrid = new Enterprise.ZArchitecture.ZGrid();
            this.GridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.LinkedComponentsGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AssemblyGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedComponentsGrid)).BeginInit();
            this.LinkedComponentsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AssemblyGrid)).BeginInit();
            this.AssemblyGrid.SuspendLayout();
            this.GridPanel.SuspendLayout();
            this.LinkedComponentsGridGroupBox.SuspendLayout();
            this.AssemblyGridGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsReceive);
            // 
            // LinkedComponentsGrid
            // 
            this.LinkedComponentsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LinkedComponentsGrid, "Lines.BOMComponentLinksForBinding");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_OP)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.SupplierPart.OP_Desc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).UnitsToPick)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.ProductUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.LocationString)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_PalletID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_AdjustmentArrivalDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_AllocationKey)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_BondedEntryKey)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_PartAttrib1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_PartAttrib2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_PartAttrib3)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_SerialNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_PackingDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.ComponentLineForAssembly)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).BOMComponentLinksForBinding)).SyncRoot)).InventoryLine.WE_ExpiryDate)));
            this.LinkedComponentsGrid.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo1.ColumnName = "InventoryLine+WE_OP";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "InventoryLine+SupplierPart+OP_Desc";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "UnitsToPick";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "InventoryLine+ProductUQ";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "InventoryLine+LocationString";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "InventoryLine+WE_PalletID";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "InventoryLine+WE_AdjustmentArrivalDate";
            zDateTimeOffsetEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo5.ColumnName = "InventoryLine+WE_AllocationKey";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "InventoryLine+WE_BondedEntryKey";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "InventoryLine+WE_PartAttrib1";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.ColumnName = "InventoryLine+WE_PartAttrib2";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.ColumnName = "InventoryLine+WE_PartAttrib3";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo10.ColumnName = "InventoryLine+WE_SerialNumber";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "InventoryLine+WE_PackingDate";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "InventoryLine+WE_ExpiryDate";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.LinkedComponentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LinkedComponentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.LinkedComponentsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.LinkedComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.LinkedComponentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.LinkedComponentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.LinkedComponentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinkedComponentsGrid.GridId = "6516412c-85b9-4987-9ecf-a30ce53a10af";
            this.LinkedComponentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LinkedComponentsGrid.LayoutKey = "LinkedComponentsGrid";
            this.LinkedComponentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.LinkedComponentsGrid.Name = "LinkedComponentsGrid";
            this.LinkedComponentsGrid.ReadOnly = true;
            this.LinkedComponentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
            this.LinkedComponentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 225, true);
            this.LinkedComponentsGrid.TabIndex = 1;
            // 
            // AssemblyGrid
            // 
            this.AssemblyGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AssemblyGrid, "Lines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_OP)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).SupplierPart.OP_Desc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_TransactionQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).ProductUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_WL)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_PalletID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_AdjustmentArrivalDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_AllocationKey)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_BondedEntryKey)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_PartAttrib1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_PartAttrib2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_PartAttrib3)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_SerialNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_PackingDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsReceiveLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsReceive)(null)).Lines)).SyncRoot)).WE_ExpiryDate)));
            this.AssemblyGrid.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo2.ColumnName = "WE_OP";
            zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo11.ColumnName = "SupplierPart+OP_Desc";
            zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "WE_TransactionQuantity";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo12.ColumnName = "ProductUQ";
            zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo3.ColumnName = "WE_WL";
            zGuidFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo13.ColumnName = "WE_PalletID";
            zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "WE_AdjustmentArrivalDate";
            zDateTimeOffsetEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo14.ColumnName = "WE_AllocationKey";
            zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo15.ColumnName = "WE_BondedEntryKey";
            zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo16.ColumnName = "WE_PartAttrib1";
            zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo17.ColumnName = "WE_PartAttrib2";
            zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo18.ColumnName = "WE_PartAttrib3";
            zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo19.ColumnName = "WE_SerialNumber";
            zTextBoxColumnStyleInfo19.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo3.ColumnName = "WE_PackingDate";
            zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo4.ColumnName = "WE_ExpiryDate";
            zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.AssemblyGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.AssemblyGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
            this.AssemblyGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
            this.AssemblyGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
            this.AssemblyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
            this.AssemblyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.AssemblyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
            this.AssemblyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AssemblyGrid.GridId = "394189cb-28d9-4337-8ff5-d41d5b6cf448";
            this.AssemblyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AssemblyGrid.LayoutKey = "AssemblyGrid";
            this.AssemblyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.AssemblyGrid.Name = "AssemblyGrid";
            this.AssemblyGrid.ReadOnly = true;
            this.AssemblyGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
            this.AssemblyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 157, true);
            this.AssemblyGrid.TabIndex = 0;
            // 
            // GridPanel
            // 
            this.GridPanel.Controls.Add(this.LinkedComponentsGridGroupBox);
            this.GridPanel.Controls.Add(this.AssemblyGridGroupBox);
            this.GridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GridPanel.Name = "GridPanel";
            this.GridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 412, true);
            this.GridPanel.TabIndex = 4;
            // 
            // LinkedComponentsGridGroupBox
            // 
            this.LinkedComponentsGridGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsWorkOrderAssemblyConfirmationDialog|LinkedComponentsGridGroupBox", "Linked Components");
            this.LinkedComponentsGridGroupBox.Controls.Add(this.LinkedComponentsGrid);
            this.LinkedComponentsGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinkedComponentsGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 172, true);
            this.LinkedComponentsGridGroupBox.Name = "LinkedComponentsGridGroupBox";
            this.LinkedComponentsGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 240, true);
            this.LinkedComponentsGridGroupBox.TabIndex = 0;
            this.LinkedComponentsGridGroupBox.TabStop = false;
            // 
            // AssemblyGridGroupBox
            // 
            this.AssemblyGridGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WhsWorkOrderAssemblyConfirmationDialog|AssemblyGridGroupBox", "Resulting Inventory");
            this.AssemblyGridGroupBox.Controls.Add(this.AssemblyGrid);
            this.AssemblyGridGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.AssemblyGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AssemblyGridGroupBox.Name = "AssemblyGridGroupBox";
            this.AssemblyGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 172, true);
            this.AssemblyGridGroupBox.TabIndex = 1;
            this.AssemblyGridGroupBox.TabStop = false;
            // 
            // WhsWorkOrderAssemblyConfirmationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GridPanel);
            this.Name = "WhsWorkOrderAssemblyConfirmationUserControl";
            this.ShouldSerializeTabPageMethods = false;
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 412, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedComponentsGrid)).EndInit();
            this.LinkedComponentsGrid.ResumeLayout(false);
            this.LinkedComponentsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AssemblyGrid)).EndInit();
            this.AssemblyGrid.ResumeLayout(false);
            this.AssemblyGrid.PerformLayout();
            this.GridPanel.ResumeLayout(false);
            this.GridPanel.PerformLayout();
            this.LinkedComponentsGridGroupBox.ResumeLayout(false);
            this.LinkedComponentsGridGroupBox.PerformLayout();
            this.AssemblyGridGroupBox.ResumeLayout(false);
            this.AssemblyGridGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZPanel GridPanel;
		ZArchitecture.GUI.ZGroupBox AssemblyGridGroupBox;
		Enterprise.ZArchitecture.ZGrid AssemblyGrid;
		ZArchitecture.GUI.ZGroupBox LinkedComponentsGridGroupBox;
		Enterprise.ZArchitecture.ZGrid LinkedComponentsGrid;
	}
}
