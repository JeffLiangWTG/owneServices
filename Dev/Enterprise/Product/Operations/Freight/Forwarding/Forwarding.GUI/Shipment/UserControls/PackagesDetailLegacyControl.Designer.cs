namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PackagesDetailLegacyControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TransitWarehouseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransitWarehouseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackLineIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PkgPackageCollectionZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PkgPackageTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PkgPackageTotalQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PkgPackageTotalWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackLineWeightUQLTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.CountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VolumeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PkgPackageTotalVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackLineVolumeUQTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.PackLineTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackLinePackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackLineWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackLineWeightUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackLineVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PackLineVolumeUQTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DiscrepancyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DiscrepancyPackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DiscrepancyWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DiscrepancyVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UpdatePackLineButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.leftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransitWarehouseStatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PkgPackageCollectionZGrid)).BeginInit();
			this.PkgPackageCollectionZGrid.SuspendLayout();
			this.leftPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// TransitWarehouseTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransitWarehouseTextBox, "OuterPackLines.JL_Calc_OriginTransitWarehouse.CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_OriginTransitWarehouse.CompanyName)));
			this.TransitWarehouseTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PkgPackageDetailsTabPage|86d41438-fefc-4541-be87-29e0da37795a", "TW", "Origin TW", "");
			this.TransitWarehouseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TransitWarehouseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 12, true);
			this.TransitWarehouseTextBox.Name = "TransitWarehouseTextBox";
			this.TransitWarehouseTextBox.ReadOnly = true;
			this.TransitWarehouseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.TransitWarehouseTextBox.TabIndex = 0;
			// 
			// TransitWarehouseStatusDropEdit
			// 
			this.TransitWarehouseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitWarehouseStatusDropEdit, "OuterPackLines.JL_OriginTransitWarehouseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OriginTransitWarehouseStatus)));
			this.TransitWarehouseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 35, true);
			this.TransitWarehouseStatusDropEdit.Name = "TransitWarehouseStatusDropEdit";
			this.TransitWarehouseStatusDropEdit.ShouldResizeByMaxLength = true;
			this.TransitWarehouseStatusDropEdit.ReadOnly = true;
			this.TransitWarehouseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.TransitWarehouseStatusDropEdit.TabIndex = 1;
			// 
			// PackLineIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackLineIDTextBox, "OuterPackLines.JL_PackLineId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_PackLineId)));
			this.PackLineIDTextBox.CaptionResourceString = null;
			this.PackLineIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 58, true);
			this.PackLineIDTextBox.Name = "PackLineIDTextBox";
			this.PackLineIDTextBox.ReadOnly = true;
			this.PackLineIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.PackLineIDTextBox.TabIndex = 2;
			// 
			// PkgPackageCollectionZGrid
			// 
			this.PkgPackageCollectionZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PkgPackageCollectionZGrid, "OuterPackLines.PkgPackageCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).PackageIDWithFallback)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_TransportRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_DimensionUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_RequiresTemperatureControl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_RequiresTemperatureControl)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_RequiredTemperatureMinimum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_RequiredTemperatureMaximum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_RequiredTemperatureUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_HSCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Packing.Business.PkgPackage)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection)).SyncRoot)).KP_ExternalReference)));
			this.PkgPackageCollectionZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "PackageIDWithFallback";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "KP_F3_NKPackType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "KP_GoodsDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "KP_Weight";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "KP_WeightUQ";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "KP_Volume";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "KP_VolumeUQ";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "KP_Length";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "KP_Width";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "KP_Height";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.ColumnName = "KP_DimensionUQ";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.ColumnName = "KP_RequiresTemperatureControl";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "KP_RequiredTemperatureMinimum";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "KP_RequiredTemperatureMaximum";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "KP_RequiredTemperatureUnit";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "KP_HSCode";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "KP_MarksAndNumbers";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "KP_ExternalReference";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PkgPackageCollectionZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PkgPackageCollectionZGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PkgPackageCollectionZGrid.GridId = "08c9fab8-0fbc-4b39-9519-9d33e0f2bfa2";
			this.PkgPackageCollectionZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PkgPackageCollectionZGrid.LayoutKey = "PkgPackageCollectionZGrid";
			this.PkgPackageCollectionZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			this.PkgPackageCollectionZGrid.Name = "PkgPackageCollectionZGrid";
			this.PkgPackageCollectionZGrid.ReadOnly = true;
			this.PkgPackageCollectionZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PkgPackageCollectionZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 200, true);
			this.PkgPackageCollectionZGrid.TabIndex = 1;
			this.PkgPackageCollectionZGrid.TabStop = false;
			// 
			// PkgPackageTotalLabel
			// 
			this.PkgPackageTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PkgPackageTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 129, true);
			this.PkgPackageTotalLabel.Name = "PkgPackageTotalLabel";
			this.PkgPackageTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.PkgPackageTotalLabel.TabIndex = 24;
			this.PkgPackageTotalLabel.Text = "Packages Total:";
			this.PkgPackageTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PkgPackageTotalQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PkgPackageTotalQtyCalcEdit, "OuterPackLines.PkgPackageCollection_TotalQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection_TotalQty)));
			this.PkgPackageTotalQtyCalcEdit.CaptionResourceString = null;
			this.PkgPackageTotalQtyCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PkgPackageTotalQtyCalcEdit, false);
			this.PkgPackageTotalQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 130, true);
			this.PkgPackageTotalQtyCalcEdit.Name = "PkgPackageTotalQtyCalcEdit";
			this.PkgPackageTotalQtyCalcEdit.ReadOnly = true;
			this.PkgPackageTotalQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.PkgPackageTotalQtyCalcEdit.TabIndex = 6;
			this.PkgPackageTotalQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PkgPackageTotalWeightCalcEdit
			// 
			this.PkgPackageTotalWeightCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PkgPackageTotalWeightCalcEdit, "OuterPackLines.PkgPackageCollection_TotalWeight");
			this.PkgPackageTotalWeightCalcEdit.BindToDecimalPlaces = "OuterPackLines.PkgPackageCollection_TotalWeight+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection_TotalWeight.DecimalPlaces)));
			this.PkgPackageTotalWeightCalcEdit.BindToDecimalPlaces = "OuterPackLines.PkgPackageCollection_TotalWeight+DecimalPlaces";
			this.PkgPackageTotalWeightCalcEdit.CaptionResourceString = null;
			this.PkgPackageTotalWeightCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PkgPackageTotalWeightCalcEdit, false);
			this.PkgPackageTotalWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 130, true);
			this.PkgPackageTotalWeightCalcEdit.Name = "PkgPackageTotalWeightCalcEdit";
			this.PkgPackageTotalWeightCalcEdit.ReadOnly = true;
			this.PkgPackageTotalWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PkgPackageTotalWeightCalcEdit.TabIndex = 7;
			this.PkgPackageTotalWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackLineWeightUQLTextBox2
			// 
			this.PackLineWeightUQLTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PackLineWeightUQLTextBox2, "OuterPackLines.JL_ActualWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			this.PackLineWeightUQLTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PackLineWeightUQLTextBox2.CaptionResourceString = null;
			this.PackLineWeightUQLTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 111, true);
			this.PackLineWeightUQLTextBox2.Name = "PackLineWeightUQLTextBox2";
			this.PackLineWeightUQLTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.PackLineWeightUQLTextBox2.TabIndex = 30;
			this.PackLineWeightUQLTextBox2.TabStop = false;
			this.PackLineWeightUQLTextBox2.Text = "UNIT";
			// 
			// CountLabel
			// 
			this.CountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 81, true);
			this.CountLabel.Name = "CountLabel";
			this.CountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.CountLabel.TabIndex = 31;
			this.CountLabel.Text = "Count";
			// 
			// WeightLabel
			// 
			this.WeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 81, true);
			this.WeightLabel.Name = "WeightLabel";
			this.WeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.WeightLabel.TabIndex = 33;
			this.WeightLabel.Text = "Weight";
			// 
			// VolumeLabel
			// 
			this.VolumeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VolumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 81, true);
			this.VolumeLabel.Name = "VolumeLabel";
			this.VolumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.VolumeLabel.TabIndex = 35;
			this.VolumeLabel.Text = "Volume";
			// 
			// PkgPackageTotalVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PkgPackageTotalVolumeCalcEdit, "OuterPackLines.PkgPackageCollection_TotalVolume");
			this.PkgPackageTotalVolumeCalcEdit.BindToDecimalPlaces = "OuterPackLines.PkgPackageCollection_TotalVolume+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection_TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PkgPackageCollection_TotalVolume.DecimalPlaces)));
			this.PkgPackageTotalVolumeCalcEdit.CaptionResourceString = null;
			this.PkgPackageTotalVolumeCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PkgPackageTotalVolumeCalcEdit, false);
			this.PkgPackageTotalVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 130, true);
			this.PkgPackageTotalVolumeCalcEdit.Name = "PkgPackageTotalVolumeCalcEdit";
			this.PkgPackageTotalVolumeCalcEdit.ReadOnly = true;
			this.PkgPackageTotalVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PkgPackageTotalVolumeCalcEdit.TabIndex = 8;
			this.PkgPackageTotalVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackLineVolumeUQTextBox2
			// 
			this.PackLineVolumeUQTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PackLineVolumeUQTextBox2, "OuterPackLines.JL_ActualVolumeUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			this.PackLineVolumeUQTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PackLineVolumeUQTextBox2.CaptionResourceString = null;
			this.PackLineVolumeUQTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 111, true);
			this.PackLineVolumeUQTextBox2.Name = "PackLineVolumeUQTextBox2";
			this.PackLineVolumeUQTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.PackLineVolumeUQTextBox2.TabIndex = 37;
			this.PackLineVolumeUQTextBox2.TabStop = false;
			this.PackLineVolumeUQTextBox2.Text = "UNIT";
			// 
			// PackLineTotalLabel
			// 
			this.PackLineTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackLineTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 106, true);
			this.PackLineTotalLabel.Name = "PackLineTotalLabel";
			this.PackLineTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.PackLineTotalLabel.TabIndex = 38;
			this.PackLineTotalLabel.Text = "Packline Total:";
			this.PackLineTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PackLinePackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackLinePackCountCalcEdit, "OuterPackLines.JL_PackageCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_PackageCount)));
			this.PackLinePackCountCalcEdit.CaptionResourceString = null;
			this.PackLinePackCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackLinePackCountCalcEdit, false);
			this.PackLinePackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 107, true);
			this.PackLinePackCountCalcEdit.Name = "PackLinePackCountCalcEdit";
			this.PackLinePackCountCalcEdit.ReadOnly = true;
			this.PackLinePackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.PackLinePackCountCalcEdit.TabIndex = 3;
			this.PackLinePackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackLineWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackLineWeightCalcEdit, "OuterPackLines.JL_ActualWeight");
			this.PackLineWeightCalcEdit.BindToDecimalPlaces = "OuterPackLines.JL_ActualWeight+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeight.DecimalPlaces)));
			this.PackLineWeightCalcEdit.CaptionResourceString = null;
			this.PackLineWeightCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackLineWeightCalcEdit, false);
			this.PackLineWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 107, true);
			this.PackLineWeightCalcEdit.Name = "PackLineWeightCalcEdit";
			this.PackLineWeightCalcEdit.ReadOnly = true;
			this.PackLineWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackLineWeightCalcEdit.TabIndex = 4;
			this.PackLineWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackLineWeightUQTextBox
			// 
			this.PackLineWeightUQTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PackLineWeightUQTextBox, "OuterPackLines.JL_ActualWeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			this.PackLineWeightUQTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PackLineWeightUQTextBox.CaptionResourceString = null;
			this.PackLineWeightUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 134, true);
			this.PackLineWeightUQTextBox.Name = "PackLineWeightUQTextBox";
			this.PackLineWeightUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.PackLineWeightUQTextBox.TabIndex = 39;
			this.PackLineWeightUQTextBox.TabStop = false;
			this.PackLineWeightUQTextBox.Text = "UNIT";
			// 
			// PackLineVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackLineVolumeCalcEdit, "OuterPackLines.JL_ActualVolume");
			this.PackLineVolumeCalcEdit.BindToDecimalPlaces = "OuterPackLines.JL_ActualVolume+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolume.DecimalPlaces)));
			this.PackLineVolumeCalcEdit.CaptionResourceString = null;
			this.PackLineVolumeCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackLineVolumeCalcEdit, false);
			this.PackLineVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 107, true);
			this.PackLineVolumeCalcEdit.Name = "PackLineVolumeCalcEdit";
			this.PackLineVolumeCalcEdit.ReadOnly = true;
			this.PackLineVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.PackLineVolumeCalcEdit.TabIndex = 5;
			this.PackLineVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PackLineVolumeUQTextBox
			// 
			this.PackLineVolumeUQTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PackLineVolumeUQTextBox, "OuterPackLines.JL_ActualVolumeUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			this.PackLineVolumeUQTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PackLineVolumeUQTextBox.CaptionResourceString = null;
			this.PackLineVolumeUQTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 134, true);
			this.PackLineVolumeUQTextBox.Name = "PackLineVolumeUQTextBox";
			this.PackLineVolumeUQTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.PackLineVolumeUQTextBox.TabIndex = 40;
			this.PackLineVolumeUQTextBox.TabStop = false;
			this.PackLineVolumeUQTextBox.Text = "UNIT";
			// 
			// DiscrepancyLabel
			// 
			this.DiscrepancyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DiscrepancyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 152, true);
			this.DiscrepancyLabel.Name = "DiscrepancyLabel";
			this.DiscrepancyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 23, true);
			this.DiscrepancyLabel.TabIndex = 41;
			this.DiscrepancyLabel.Text = "Discrepancy:";
			this.DiscrepancyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DiscrepancyPackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DiscrepancyPackCountCalcEdit, "OuterPackLines.JL_Calc_PacklineToPkgPackage_QtyDiscrepancy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_PacklineToPkgPackage_QtyDiscrepancy)));
			this.DiscrepancyPackCountCalcEdit.CaptionResourceString = null;
			this.DiscrepancyPackCountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DiscrepancyPackCountCalcEdit, false);
			this.DiscrepancyPackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 153, true);
			this.DiscrepancyPackCountCalcEdit.Name = "DiscrepancyPackCountCalcEdit";
			this.DiscrepancyPackCountCalcEdit.ReadOnly = true;
			this.DiscrepancyPackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 20, true);
			this.DiscrepancyPackCountCalcEdit.TabIndex = 9;
			this.DiscrepancyPackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DiscrepancyWeightCalcEdit
			// 
			this.DiscrepancyWeightCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DiscrepancyWeightCalcEdit, "OuterPackLines.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy");
			this.DiscrepancyWeightCalcEdit.BindToDecimalPlaces = "OuterPackLines.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_PacklineToPkgPackage_WeightDiscrepancy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_PacklineToPkgPackage_WeightDiscrepancy.DecimalPlaces)));
			this.DiscrepancyWeightCalcEdit.CaptionResourceString = null;
			this.DiscrepancyWeightCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DiscrepancyWeightCalcEdit, false);
			this.DiscrepancyWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 153, true);
			this.DiscrepancyWeightCalcEdit.Name = "DiscrepancyWeightCalcEdit";
			this.DiscrepancyWeightCalcEdit.ReadOnly = true;
			this.DiscrepancyWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DiscrepancyWeightCalcEdit.TabIndex = 10;
			this.DiscrepancyWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DiscrepancyVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DiscrepancyVolumeCalcEdit, "OuterPackLines.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy");
			this.DiscrepancyVolumeCalcEdit.BindToDecimalPlaces = "OuterPackLines.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy+DecimalPlaces";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((int)(((Enterprise.Freight.Forwarding.Business.ForwardingPackLine)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy.DecimalPlaces)));
			this.DiscrepancyVolumeCalcEdit.CaptionResourceString = null;
			this.DiscrepancyVolumeCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DiscrepancyVolumeCalcEdit, false);
			this.DiscrepancyVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(231, 153, true);
			this.DiscrepancyVolumeCalcEdit.Name = "DiscrepancyVolumeCalcEdit";
			this.DiscrepancyVolumeCalcEdit.ReadOnly = true;
			this.DiscrepancyVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.DiscrepancyVolumeCalcEdit.TabIndex = 11;
			this.DiscrepancyVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UpdatePackLineButton
			// 
			this.UpdatePackLineButton.IsCaptionOverridden = true;
			this.UpdatePackLineButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 176, true);
			this.UpdatePackLineButton.Name = "UpdatePackLineButton";
			this.UpdatePackLineButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdatePackLineButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 23, true);
			this.UpdatePackLineButton.TabIndex = 12;
			this.UpdatePackLineButton.Text = "Update Packline";
			this.UpdatePackLineButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UpdatePackLineButton.ToolTipCaption = null;
			this.UpdatePackLineButton.UseVisualStyleBackColor = true;
			this.UpdatePackLineButton.Click += OnUpdatePackLineButtonClick;
			// 
			// leftPanel
			// 
			this.leftPanel.Controls.Add(this.PackLineIDTextBox);
			this.leftPanel.Controls.Add(this.PkgPackageTotalLabel);
			this.leftPanel.Controls.Add(this.VolumeLabel);
			this.leftPanel.Controls.Add(this.PkgPackageTotalQtyCalcEdit);
			this.leftPanel.Controls.Add(this.WeightLabel);
			this.leftPanel.Controls.Add(this.PkgPackageTotalWeightCalcEdit);
			this.leftPanel.Controls.Add(this.CountLabel);
			this.leftPanel.Controls.Add(this.PackLineWeightUQLTextBox2);
			this.leftPanel.Controls.Add(this.TransitWarehouseStatusDropEdit);
			this.leftPanel.Controls.Add(this.PkgPackageTotalVolumeCalcEdit);
			this.leftPanel.Controls.Add(this.TransitWarehouseTextBox);
			this.leftPanel.Controls.Add(this.PackLineVolumeUQTextBox2);
			this.leftPanel.Controls.Add(this.PackLineTotalLabel);
			this.leftPanel.Controls.Add(this.UpdatePackLineButton);
			this.leftPanel.Controls.Add(this.PackLinePackCountCalcEdit);
			this.leftPanel.Controls.Add(this.DiscrepancyVolumeCalcEdit);
			this.leftPanel.Controls.Add(this.PackLineWeightCalcEdit);
			this.leftPanel.Controls.Add(this.DiscrepancyWeightCalcEdit);
			this.leftPanel.Controls.Add(this.PackLineWeightUQTextBox);
			this.leftPanel.Controls.Add(this.DiscrepancyPackCountCalcEdit);
			this.leftPanel.Controls.Add(this.PackLineVolumeCalcEdit);
			this.leftPanel.Controls.Add(this.DiscrepancyLabel);
			this.leftPanel.Controls.Add(this.PackLineVolumeUQTextBox);
			this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftPanel.Name = "leftPanel";
			this.leftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.leftPanel.TabIndex = 0;
			// 
			// PackagesDetailLegacyControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PkgPackageCollectionZGrid);
			this.Controls.Add(this.leftPanel);
			this.Name = "PackagesDetailLegacyControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransitWarehouseStatusDropEdit.ResumeLayout(true);
			this.TransitWarehouseStatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PkgPackageCollectionZGrid)).EndInit();
			this.PkgPackageCollectionZGrid.ResumeLayout(false);
			this.PkgPackageCollectionZGrid.PerformLayout();
			this.leftPanel.ResumeLayout(false);
			this.leftPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox TransitWarehouseTextBox;
		private ZArchitecture.GUI.ZDropEdit TransitWarehouseStatusDropEdit;
		private ZArchitecture.ZTextBox PackLineIDTextBox;
		private ZArchitecture.ZGrid PkgPackageCollectionZGrid;
		private ZArchitecture.ZLabel PkgPackageTotalLabel;
		private ZArchitecture.ZCalcEdit PkgPackageTotalQtyCalcEdit;
		private ZArchitecture.ZCalcEdit PkgPackageTotalWeightCalcEdit;
		private ZArchitecture.ZTextBox PackLineWeightUQLTextBox2;
		private ZArchitecture.ZLabel CountLabel;
		private ZArchitecture.ZLabel WeightLabel;
		private ZArchitecture.ZLabel VolumeLabel;
		private ZArchitecture.ZCalcEdit PkgPackageTotalVolumeCalcEdit;
		private ZArchitecture.ZTextBox PackLineVolumeUQTextBox2;
		private ZArchitecture.ZLabel PackLineTotalLabel;
		private ZArchitecture.ZCalcEdit PackLinePackCountCalcEdit;
		private ZArchitecture.ZCalcEdit PackLineWeightCalcEdit;
		private ZArchitecture.ZTextBox PackLineWeightUQTextBox;
		private ZArchitecture.ZCalcEdit PackLineVolumeCalcEdit;
		private ZArchitecture.ZTextBox PackLineVolumeUQTextBox;
		private ZArchitecture.ZLabel DiscrepancyLabel;
		private ZArchitecture.ZCalcEdit DiscrepancyPackCountCalcEdit;
		private ZArchitecture.ZCalcEdit DiscrepancyWeightCalcEdit;
		private ZArchitecture.ZCalcEdit DiscrepancyVolumeCalcEdit;
		private ZArchitecture.GUI.ZButton UpdatePackLineButton;
		private ZArchitecture.GUI.ZPanel leftPanel;
	}
}
