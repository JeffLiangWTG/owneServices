using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ContainersUserControl : ZUserControl
	{
		ForwardingShipment Shipment => (ForwardingShipment)packLinesContol.Grid.DataSource;

		KPanel ConsolsPanel;
		KPanel ContainersPanel;
		KPanel PackLinesPanel;
		KPanel TotalsPanel;
		KPanel TotalsInnerPanel;
		TemperatureControlBlock TemperatureControlBlock;
		ZCalcEdit JS_Calc_TotalWeightBoundTextBox;
		ZCalcEdit JS_Calc_TotalVolumeBoundTextBox;
		ZCalcEdit JS_Calc_TotalPackagesBoundTextBox;
		ZTextBox JS_Calc_TotalVolumeUnitBoundTextBox;
		ZTextBox JS_Calc_TotalVolumeUnitBoundTextBox2;
		ZCalcEdit JS_ActualWeightBoundTextBox;
		ZCalcEdit JS_ActualVolumeBoundTextBox;
		ZCalcEdit JS_OuterPacksBoundTextBox;
		ZTextBox ShipmentWeightUnitTextBox;
		ZTextBox PacklineTotalWeightUnitTextBox;
		ZGroupBox GoodsDetailGroupBox;
		ZTextBox JL_DescriptionTextBox;
		ZGuidFindBox DGContactGuidFindBox;
		ZCodeFindBox JL_OriginCodeFindBox;
		ZCodeFindBox JL_CommodityCodeFindBox;
		ZGroupBox DangerousGoodsGroupBox;
		ZGroupBox TemperatureControlledGroupBox;
		ZPanel DetailsPanel;
		KSplitter splitter1;
		KSplitter splitter2;
		KSplitter splitter3;
		ZLabel ConsolsLabel;
		ZLabel ContainersLabel;
		ZLabel OuterPackagesLabel;
		ZLabel ShipmentTotalsLabel;
		ZLabel OuterPackTotalsLabel;
		ZLabel FlashPointUnitLabel;
		ZCalcEdit FlashPointCalcEdit;
		ZCheckBox DIIsCombustibleCheckBox;
		ZTextBox zTextBox1;
		ZTemplateTabControl PackagesDetailTabControl;
		ZTabPage WeightAndMeasureTabPage;
		ZGroupBox WeightAndMeasureGroupBox;
		ZCalcEdit JL_LengthCalcEdit;
		ZCalcEdit JL_WidthCalcEdit;
		ZGroupBox OutturnGroupBox;
		ZTextBox MarksAndNumbersTextBox;
		ZTextBox JL_UnitOfDimensionOutturnedTextBox;
		ZCalcEdit JL_OutturnedLengthCalcEdit;
		ZCalcEdit JL_OutturnedHeightCalcEdit;
		ZCalcEdit JL_OutturnedWidthCalcEdit;
		ZCalcEdit JL_DamagedCalcEdit;
		ZCalcEdit JL_PillagedCalcEdit;
		ZCalcEdit JL_OutturnCalcEdit;
		ZTextBox JL_OutturnCommentTextBox;
		ZTabPage CustomFieldsTabPage;
		ZTextBox JL_CustomAttrib4TextBox;
		ZTextBox JL_CustomAttrib3TextBox;
		ZTextBox JL_CustomAttrib2TextBox;
		ZTextBox JL_CustomAttrib1TextBox;
		ZDateEdit CustomDate1DateEdit;
		ZCheckBox JL_CustomFlag2CheckBox;
		ZDateEdit CustomDate2DateEdit;
		ZCheckBox JL_CustomFlag1CheckBox;
		ZCalcEdit JL_CustomDecimal2;
		ZCalcEdit JL_CustomDecimal1CalcEdit;
		ZTabPage locationTabPage;
		ZTextBox JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2;
		ZCalcEdit JS_ActualVolume_ImperialBoundTextBox;
		ZTextBox JS_Calc_TotalVolumeUnit_ImperialBoundTextBox;
		ZCalcEdit JS_Calc_TotalVolume_ImperialBoundTextBox;
		ZTextBox ShipmentWeightUnit_ImperialTextBox;
		protected ZCalcEdit JS_ActualWeight_ImperialBoundTextBox;
		ZTextBox PacklineTotalWeightUnit_ImperialTextBox;
		ZCalcEdit JS_Calc_TotalWeight_ImperialBoundTextBox;
		private ZLinkLabel DGLinkLabel;
		private ZGuidFindBox DGGuidFindBox;
		protected ZCalcEdit JL_LoadingMetersCalcDropEdit;
		private CargoWise.Windows.UI.Layout.RowLayoutPanel PackingRowLayoutPanel;
		private ZGrid JobConsolBoundGrid;
		private ZGrid JobContainerGrid;
		protected PackLinesContol packLinesContol;
		Customs.Universal.GUI.TariffFindBox HarmonisedCodeFindBox;
		private ZCalcDropEdit JL_PackageCountCalcDropEdit;
		private ZCalcDropEdit JL_ActualWeightCalcDropEdit;
		private ZCalcDropEdit JL_ActualVolumeCalcDropEdit;
		private ZCalcDropEdit JL_HeightCalcDropEdit;
		private ZCalcDropEdit JL_OutturnedVolumeCalcDropEdit;
		private ZCalcDropEdit JL_OutturnedWeightCalcDropEdit;
		protected PackLineLocationsControl locationsControl;
		private ZDropEdit IMOClassDropEdit;
		private ZLinkLabel DGDetailsLinkLabel;
		internal Customs.Universal.GUI.TariffFindBox HSCodeFindBox;
		internal ZCodeFindBox HSCountryFindBox;
		private ZLinkLabel HSLinkLabel;
		private ZLinkLabel HSDetailsLinkLabel;

		ZTabPage PkgPackageDetailsTabPage;
		ZUserControl PkgPackageDetailControl;
		LastKnownTransitWarehouseControl LastKnownTransitWarehouseControl;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();

			var zTextBoxColumnStyleInfoPkgPackageID = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackagePackType = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageGoodsDesc = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageWeightUQ = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageVolumeUQ = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageDimensionUQ = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageRequiredTempUnit = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageHSCode = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageCommodityCode = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageMarksAndNumbers = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfoPkgPackageExternalRef = new ZTextBoxColumnStyleInfo();

			var zCalcEditColumnStyleInfoPkgPackageWeight = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageVolume = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageLength = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageWidth = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageHeight = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageRequiredTempMin = new ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfoPkgPackageRequiredTempMax = new ZCalcEditColumnStyleInfo();

			var zCheckBoxColumnStyleInfoPkgPackageRequiresTempControl = new ZCheckBoxColumnStyleInfo();

			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContainersUserControl));
			this.PkgPackageDetailsTabPage = new ZTabPage();
			this.PkgPackageDetailControl = GetPackagesDetailControl();
			this.ConsolsPanel = new KPanel();
			this.ConsolsLabel = new ZLabel();
			this.JobConsolBoundGrid = new ZGrid();
			this.ContainersPanel = new KPanel();
			this.ContainersLabel = new ZLabel();
			this.JobContainerGrid = new ZGrid();
			this.PackLinesPanel = new KPanel();
			this.packLinesContol = new PackLinesContol();
			this.OuterPackagesLabel = new ZLabel();
			this.TotalsPanel = new KPanel();
			this.TotalsInnerPanel = new KPanel();
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2 = new ZTextBox();
			this.JS_ActualVolume_ImperialBoundTextBox = new ZCalcEdit();
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox = new ZTextBox();
			this.JS_Calc_TotalVolume_ImperialBoundTextBox = new ZCalcEdit();
			this.ShipmentWeightUnit_ImperialTextBox = new ZTextBox();
			this.JS_ActualWeight_ImperialBoundTextBox = new ZCalcEdit();
			this.PacklineTotalWeightUnit_ImperialTextBox = new ZTextBox();
			this.JS_Calc_TotalWeight_ImperialBoundTextBox = new ZCalcEdit();
			this.ShipmentTotalsLabel = new ZLabel();
			this.OuterPackTotalsLabel = new ZLabel();
			this.JS_Calc_TotalVolumeUnitBoundTextBox2 = new ZTextBox();
			this.ShipmentWeightUnitTextBox = new ZTextBox();
			this.JS_ActualWeightBoundTextBox = new ZCalcEdit();
			this.JS_ActualVolumeBoundTextBox = new ZCalcEdit();
			this.JS_OuterPacksBoundTextBox = new ZCalcEdit();
			this.JS_Calc_TotalVolumeUnitBoundTextBox = new ZTextBox();
			this.PacklineTotalWeightUnitTextBox = new ZTextBox();
			this.JS_Calc_TotalWeightBoundTextBox = new ZCalcEdit();
			this.JS_Calc_TotalVolumeBoundTextBox = new ZCalcEdit();
			this.JS_Calc_TotalPackagesBoundTextBox = new ZCalcEdit();
			this.GoodsDetailGroupBox = new ZGroupBox();
			this.DGDetailsLinkLabel = new ZLinkLabel();
			this.IMOClassDropEdit = new ZDropEdit();
			this.DGGuidFindBox = new ZGuidFindBox();
			this.DGLinkLabel = new ZLinkLabel();
			this.FlashPointCalcEdit = new ZCalcEdit();
			this.FlashPointUnitLabel = new ZLabel();
			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				this.DIIsCombustibleCheckBox = new ZCheckBox();
			}
			this.JL_DescriptionTextBox = new ZTextBox();
			this.DGContactGuidFindBox = new ZGuidFindBox();
			this.JL_OriginCodeFindBox = new ZCodeFindBox();
			this.JL_CommodityCodeFindBox = new ZCodeFindBox();
			this.HarmonisedCodeFindBox = new Customs.Universal.GUI.TariffFindBox();
			this.HSDetailsLinkLabel = new ZLinkLabel();
			this.HSCountryFindBox = new ZCodeFindBox();
			this.HSCodeFindBox = new Customs.Universal.GUI.TariffFindBox();
			this.HSLinkLabel = new ZLinkLabel();
			this.zTextBox1 = new ZTextBox();
			this.DetailsPanel = new ZPanel();
			this.PackagesDetailTabControl = new ZTemplateTabControl();
			this.WeightAndMeasureTabPage = new ZTabPage();
			this.WeightAndMeasureGroupBox = new ZGroupBox();
			this.PackingRowLayoutPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.JL_PackageCountCalcDropEdit = new ZCalcDropEdit();
			this.JL_ActualWeightCalcDropEdit = new ZCalcDropEdit();
			this.JL_ActualVolumeCalcDropEdit = new ZCalcDropEdit();
			this.JL_LoadingMetersCalcDropEdit = new ZCalcEdit();
			this.JL_LengthCalcEdit = new ZCalcEdit();
			this.JL_WidthCalcEdit = new ZCalcEdit();
			this.JL_HeightCalcDropEdit = new ZCalcDropEdit();
			this.OutturnGroupBox = new ZGroupBox();
			this.MarksAndNumbersTextBox = new ZTextBox();
			this.JL_UnitOfDimensionOutturnedTextBox = new ZTextBox();
			this.JL_OutturnedLengthCalcEdit = new ZCalcEdit();
			this.JL_OutturnedHeightCalcEdit = new ZCalcEdit();
			this.JL_OutturnedWidthCalcEdit = new ZCalcEdit();
			this.JL_OutturnedVolumeCalcDropEdit = new ZCalcDropEdit();
			this.JL_OutturnedWeightCalcDropEdit = new ZCalcDropEdit();
			this.JL_DamagedCalcEdit = new ZCalcEdit();
			this.JL_PillagedCalcEdit = new ZCalcEdit();
			this.JL_OutturnCalcEdit = new ZCalcEdit();
			this.JL_OutturnCommentTextBox = new ZTextBox();
			this.locationTabPage = new ZTabPage();
			this.locationsControl = new PackLineLocationsControl();
			this.CustomFieldsTabPage = new ZTabPage();
			this.JL_CustomAttrib4TextBox = new ZTextBox();
			this.JL_CustomAttrib3TextBox = new ZTextBox();
			this.JL_CustomAttrib2TextBox = new ZTextBox();
			this.JL_CustomAttrib1TextBox = new ZTextBox();
			this.CustomDate1DateEdit = new ZDateEdit();
			this.JL_CustomFlag2CheckBox = new ZCheckBox();
			this.CustomDate2DateEdit = new ZDateEdit();
			this.JL_CustomFlag1CheckBox = new ZCheckBox();
			this.JL_CustomDecimal2 = new ZCalcEdit();
			this.JL_CustomDecimal1CalcEdit = new ZCalcEdit();
			this.DangerousGoodsGroupBox = new ZGroupBox();
			this.TemperatureControlledGroupBox = new ZGroupBox();
			this.TemperatureControlBlock = new TemperatureControlBlock();
			this.splitter1 = new KSplitter();
			this.splitter2 = new KSplitter();
			this.splitter3 = new KSplitter();
			this.LastKnownTransitWarehouseControl = new LastKnownTransitWarehouseControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PkgPackageDetailsTabPage.SuspendLayout();
			this.PkgPackageDetailControl.SuspendLayout();
			this.ConsolsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobConsolBoundGrid)).BeginInit();
			this.JobConsolBoundGrid.SuspendLayout();
			this.ContainersPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobContainerGrid)).BeginInit();
			this.JobContainerGrid.SuspendLayout();
			this.PackLinesPanel.SuspendLayout();
			this.packLinesContol.SuspendLayout();
			this.TotalsPanel.SuspendLayout();
			this.TotalsInnerPanel.SuspendLayout();
			this.GoodsDetailGroupBox.SuspendLayout();
			this.DangerousGoodsGroupBox.SuspendLayout();
			this.TemperatureControlledGroupBox.SuspendLayout();
			this.IMOClassDropEdit.SuspendLayout();
			this.DGGuidFindBox.SuspendLayout();
			this.DGContactGuidFindBox.SuspendLayout();
			this.JL_OriginCodeFindBox.SuspendLayout();
			this.JL_CommodityCodeFindBox.SuspendLayout();
			this.HarmonisedCodeFindBox.SuspendLayout();
			this.HSCountryFindBox.SuspendLayout();
			this.HSCodeFindBox.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.PackagesDetailTabControl.SuspendLayout();
			this.WeightAndMeasureTabPage.SuspendLayout();
			this.WeightAndMeasureGroupBox.SuspendLayout();
			this.PackingRowLayoutPanel.SuspendLayout();
			this.JL_PackageCountCalcDropEdit.SuspendLayout();
			this.JL_ActualWeightCalcDropEdit.SuspendLayout();
			this.JL_ActualVolumeCalcDropEdit.SuspendLayout();
			this.JL_HeightCalcDropEdit.SuspendLayout();
			this.OutturnGroupBox.SuspendLayout();
			this.JL_OutturnedVolumeCalcDropEdit.SuspendLayout();
			this.JL_OutturnedWeightCalcDropEdit.SuspendLayout();
			this.locationTabPage.SuspendLayout();
			this.locationsControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CustomDate1DateEdit.SuspendLayout();
			this.CustomDate2DateEdit.SuspendLayout();
			this.LastKnownTransitWarehouseControl.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			//
			// PkgPackageDetailsTabPage
			//
			this.PkgPackageDetailsTabPage.Controls.Add(this.PkgPackageDetailControl);
			this.PkgPackageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PkgPackageDetailsTabPage.Name = "PkgPackageDetailsTabPage";
			this.PkgPackageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 228, true);
			this.PkgPackageDetailsTabPage.TabIndex = 1;
			this.PkgPackageDetailsTabPage.Text = Res.GetString("PkgPackageDetailsTabPage|0f62fd34-bba5-47d9-8523-7e6aa078783d", "Package Details");
			//
			// PkgPackageCollectionZGrid
			//
			this.PkgPackageDetailControl.Dock = DockStyle.Fill;
			this.BindingSource.SetBindingMember(this.PkgPackageDetailControl, ".");
			this.PkgPackageDetailControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.PkgPackageDetailControl.Name = "PkgPackageCollectionZGrid";
			this.PkgPackageDetailControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 190, true);
			this.PkgPackageDetailControl.TabStop = true;
			//
			// ConsolsPanel
			//
			this.ConsolsPanel.Controls.Add(this.ConsolsLabel);
			this.ConsolsPanel.Controls.Add(this.JobConsolBoundGrid);
			this.ConsolsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConsolsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolsPanel.Name = "ConsolsPanel";
			this.ConsolsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 90, true);
			this.ConsolsPanel.TabIndex = 0;
			//
			// ConsolsLabel
			//
			this.ConsolsLabel.AutoSize = true;
			this.ConsolsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ConsolsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsolsLabel, false);
			this.ConsolsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.ConsolsLabel.Name = "ConsolsLabel";
			this.ConsolsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.ConsolsLabel.TabIndex = 0;
			//
			// JobConsolBoundGrid
			//
			this.JobConsolBoundGrid.AllowNavigation = false;
			this.JobConsolBoundGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobConsolBoundGrid, "AllMasterConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).JK_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).JK_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).JK_RL_NKDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).JK_MasterBillNum)));
			this.JobConsolBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|5debb6d3-9131-4c1b-840b-c3aaf6c99046", "Loading");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JK_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|fa765ffb-3289-4cfb-9d9c-b115033098ea", "Discharge");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JK_RL_NKDischargePort";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.JobConsolBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobConsolBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobConsolBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.JobConsolBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobConsolBoundGrid.GridId = "a5006db7-0401-4f6a-852e-cb22da73a79d";
			this.JobConsolBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobConsolBoundGrid.IsWholeRowSelectedOnClick = true;
			this.JobConsolBoundGrid.LayoutKey = "JobConsolBoundGrid";
			this.JobConsolBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JobConsolBoundGrid.Name = "JobConsolBoundGrid";
			this.JobConsolBoundGrid.ReadOnly = true;
			this.JobConsolBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 71, true);
			this.JobConsolBoundGrid.TabIndex = 1;
			//
			// ContainersPanel
			//
			this.ContainersPanel.Controls.Add(this.ContainersLabel);
			this.ContainersPanel.Controls.Add(this.JobContainerGrid);
			this.ContainersPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ContainersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 93, true);
			this.ContainersPanel.Name = "ContainersPanel";
			this.ContainersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 128, true);
			this.ContainersPanel.TabIndex = 1;
			//
			// ContainersLabel
			//
			this.ContainersLabel.AutoSize = true;
			this.ContainersLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ContainersLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContainersLabel, false);
			this.ContainersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.ContainersLabel.Name = "ContainersLabel";
			this.ContainersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.ContainersLabel.TabIndex = 0;
			//
			// JobContainerGrid
			//
			this.JobContainerGrid.AllowNavigation = false;
			this.JobContainerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JobContainerGrid, "AllMasterConsols.Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_AdditionalSealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Additional2SealNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_RC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).DeliveryModeForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_SealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_AdditionalSealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Additional2SealParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Calc_ContainerCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_TareWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_VolumeCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_VolumeCapacityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_WeightCapacity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_WeightCapacityUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Calc_TotalPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Calc_TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_Calc_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_TrainWagonNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingContainer)(((System.Collections.IList)(((ForwardingConsol)(((System.Collections.IList)(((ForwardingShipment)(null)).AllMasterConsols)).SyncRoot)).Containers)).SyncRoot)).JC_TempRecorderSerialNo)));
			this.JobContainerGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo3.ToolTip = "Container Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JC_ContainerCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.ColumnName = "JC_SealNum";
			zTextBoxColumnStyleInfo4.ToolTip = "Seal Number / Rate Class";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "JC_AdditionalSealNum";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.ToolTip = "2nd or Additional Seal Number";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "JC_Additional2SealNum";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.ToolTip = "3rd or Additional Seal Number";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo1.ToolTip = "Container Mode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Container Type";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "DeliveryModeForBinding";
			zDropEditColumnStyleInfo2.ToolTip = "Delivery Mode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("0db0e7bc-fadc-4281-9304-586eba4c8ce6", "Delivery Mode");
			zDropEditColumnStyleInfo3.ColumnName = "JC_SealParty";
			zDropEditColumnStyleInfo3.ToolTip = "Sealed By";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "JC_AdditionalSealParty";
			zDropEditColumnStyleInfo4.ToolTip = "Second Sealed By";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "JC_Additional2SealParty";
			zDropEditColumnStyleInfo5.ToolTip = "Third Sealed By";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|42494148-daaa-4f97-9014-e7cba935ab70", "Capacity (M3)");
			zCalcEditColumnStyleInfo2.ColumnName = "JC_Calc_ContainerCapacity";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JC_TareWeight";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JC_VolumeCapacity";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|48ca3e5b-10b0-4a86-b950-204b250ed57d", "Vol. Capacity");
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "JC_VolumeCapacityUQ";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|48ca3e5b-10b0-4a86-b950-204b250ed57d", "Vol. Capacity");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JC_WeightCapacity";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|903f9e61-9a4b-4728-8ac1-dffaa75c9efe", "Wgt. Capacity");
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "JC_WeightCapacityUQ";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|903f9e61-9a4b-4728-8ac1-dffaa75c9efe", "Wgt. Capacity");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|9fb27d6c-ef96-433e-9e42-69569302d185", "Total Packs");
			zCalcEditColumnStyleInfo6.ColumnName = "JC_Calc_TotalPackages";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|b4335990-3189-46a8-8f8e-92302e87cd62", "Total Volume");
			zCalcEditColumnStyleInfo7.ColumnName = "JC_Calc_TotalVolume";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|bef419c2-8134-4274-b45b-149e8232154d", "Total Weight");
			zCalcEditColumnStyleInfo8.ColumnName = "JC_Calc_TotalWeight";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "JC_TrainWagonNumber";
			zTextBoxColumnStyleInfo9.ToolTip = "Train Wagon Number";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "JC_TempRecorderSerialNo";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.JobContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobContainerGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.JobContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.JobContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.JobContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.JobContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.JobContainerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.JobContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.JobContainerGrid.GridId = "02ed55ec-32e2-4f5c-a839-46ed99b4b908";
			this.JobContainerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobContainerGrid.IsWholeRowSelectedOnClick = true;
			this.JobContainerGrid.LayoutKey = "JobContainerGrid";
			this.JobContainerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.JobContainerGrid.Name = "JobContainerGrid";
			this.JobContainerGrid.ReadOnly = true;
			this.JobContainerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 109, true);
			this.JobContainerGrid.TabIndex = 1;
			//
			// PackLinesPanel
			//
			this.PackLinesPanel.Controls.Add(this.packLinesContol);
			this.PackLinesPanel.Controls.Add(this.OuterPackagesLabel);
			this.PackLinesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 224, true);
			this.PackLinesPanel.Name = "PackLinesPanel";
			this.PackLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 122, true);
			this.PackLinesPanel.TabIndex = 2;
			//
			// packLinesContol
			//
			this.packLinesContol.AllowDrop = true;
			this.packLinesContol.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.packLinesContol, ".");
			this.packLinesContol.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.packLinesContol.Name = "packLinesContol";
			this.packLinesContol.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 122, true);
			this.packLinesContol.TabIndex = 1;
			//
			// OuterPackagesLabel
			//
			this.OuterPackagesLabel.AutoSize = true;
			this.OuterPackagesLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OuterPackagesLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OuterPackagesLabel, false);
			this.OuterPackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.OuterPackagesLabel.Name = "OuterPackagesLabel";
			this.OuterPackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.OuterPackagesLabel.TabIndex = 0;
			//
			// TotalsPanel
			//
			this.TotalsPanel.Controls.Add(this.TotalsInnerPanel);
			this.TotalsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 587, true);
			this.TotalsPanel.Name = "TotalsPanel";
			this.TotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 45, true);
			this.TotalsPanel.TabIndex = 3;
			//
			// TotalsInnerPanel
			//
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2);
			this.TotalsInnerPanel.Controls.Add(this.JS_ActualVolume_ImperialBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolume_ImperialBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.ShipmentWeightUnit_ImperialTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_ActualWeight_ImperialBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.PacklineTotalWeightUnit_ImperialTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalWeight_ImperialBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.ShipmentTotalsLabel);
			this.TotalsInnerPanel.Controls.Add(this.OuterPackTotalsLabel);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolumeUnitBoundTextBox2);
			this.TotalsInnerPanel.Controls.Add(this.ShipmentWeightUnitTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_ActualWeightBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_ActualVolumeBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_OuterPacksBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolumeUnitBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.PacklineTotalWeightUnitTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalWeightBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalVolumeBoundTextBox);
			this.TotalsInnerPanel.Controls.Add(this.JS_Calc_TotalPackagesBoundTextBox);
			this.TotalsInnerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TotalsInnerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TotalsInnerPanel.Name = "TotalsInnerPanel";
			this.TotalsInnerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 45, true);
			this.TotalsInnerPanel.TabIndex = 0;
			//
			// JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2
			//
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(951, 27, true);
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.Name = "JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2";
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.TabIndex = 19;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox2.Text = "CF";
			//
			// JS_ActualVolume_ImperialBoundTextBox
			//
			this.JS_ActualVolume_ImperialBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_ActualVolume_ImperialBoundTextBox, "JS_ActualVolume_Imperial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ActualVolume_Imperial)));
			this.JS_ActualVolume_ImperialBoundTextBox.DecimalPlaces = 2;
			this.JS_ActualVolume_ImperialBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(873, 23, true);
			this.JS_ActualVolume_ImperialBoundTextBox.Name = "JS_ActualVolume_ImperialBoundTextBox";
			this.JS_ActualVolume_ImperialBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_ActualVolume_ImperialBoundTextBox.TabIndex = 18;
			this.JS_ActualVolume_ImperialBoundTextBox.Text = "0.000";
			this.JS_ActualVolume_ImperialBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_Calc_TotalVolumeUnit_ImperialBoundTextBox
			//
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(951, 6, true);
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.Name = "JS_Calc_TotalVolumeUnit_ImperialBoundTextBox";
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.TabIndex = 17;
			this.JS_Calc_TotalVolumeUnit_ImperialBoundTextBox.Text = "CF";
			//
			// JS_Calc_TotalVolume_ImperialBoundTextBox
			//
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalVolume_ImperialBoundTextBox, "TotalOuterPacksVolume_Imperial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).TotalOuterPacksVolume_Imperial)));
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.DecimalPlaces = 2;
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(873, 2, true);
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.Name = "JS_Calc_TotalVolume_ImperialBoundTextBox";
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.TabIndex = 16;
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.Text = "0.000";
			this.JS_Calc_TotalVolume_ImperialBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ShipmentWeightUnit_ImperialTextBox
			//
			this.ShipmentWeightUnit_ImperialTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentWeightUnit_ImperialTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.ShipmentWeightUnit_ImperialTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.ShipmentWeightUnit_ImperialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(771, 27, true);
			this.ShipmentWeightUnit_ImperialTextBox.Name = "ShipmentWeightUnit_ImperialTextBox";
			this.ShipmentWeightUnit_ImperialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.ShipmentWeightUnit_ImperialTextBox.TabIndex = 15;
			this.ShipmentWeightUnit_ImperialTextBox.Text = "LB";
			//
			// JS_ActualWeight_ImperialBoundTextBox
			//
			this.JS_ActualWeight_ImperialBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_ActualWeight_ImperialBoundTextBox, "JS_ActualWeight_Imperial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ActualWeight_Imperial)));
			this.JS_ActualWeight_ImperialBoundTextBox.DecimalPlaces = 2;
			this.JS_ActualWeight_ImperialBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 23, true);
			this.JS_ActualWeight_ImperialBoundTextBox.Name = "JS_ActualWeight_ImperialBoundTextBox";
			this.JS_ActualWeight_ImperialBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_ActualWeight_ImperialBoundTextBox.TabIndex = 14;
			this.JS_ActualWeight_ImperialBoundTextBox.Text = "0.000";
			this.JS_ActualWeight_ImperialBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// PacklineTotalWeightUnit_ImperialTextBox
			//
			this.PacklineTotalWeightUnit_ImperialTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PacklineTotalWeightUnit_ImperialTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.PacklineTotalWeightUnit_ImperialTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PacklineTotalWeightUnit_ImperialTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(771, 6, true);
			this.PacklineTotalWeightUnit_ImperialTextBox.Name = "PacklineTotalWeightUnit_ImperialTextBox";
			this.PacklineTotalWeightUnit_ImperialTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.PacklineTotalWeightUnit_ImperialTextBox.TabIndex = 13;
			this.PacklineTotalWeightUnit_ImperialTextBox.Text = "LB";
			//
			// JS_Calc_TotalWeight_ImperialBoundTextBox
			//
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalWeight_ImperialBoundTextBox, "TotalOuterPacksWeight_Imperial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).TotalOuterPacksWeight_Imperial)));
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.DecimalPlaces = 2;
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 2, true);
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.Name = "JS_Calc_TotalWeight_ImperialBoundTextBox";
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.TabIndex = 12;
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.Text = "0.000";
			this.JS_Calc_TotalWeight_ImperialBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ShipmentTotalsLabel
			//
			this.ShipmentTotalsLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentTotalsLabel.AutoSize = true;
			this.ShipmentTotalsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|8ab5bc0d-930f-47f0-8812-6ecdd1505a9c", "Shipment Totals");
			this.ShipmentTotalsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ShipmentTotalsLabel.IsFontBold = true;
			this.ShipmentTotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 27, true);
			this.ShipmentTotalsLabel.Name = "ShipmentTotalsLabel";
			this.ShipmentTotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 14, true);
			this.ShipmentTotalsLabel.TabIndex = 6;
			this.ShipmentTotalsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// OuterPackTotalsLabel
			//
			this.OuterPackTotalsLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OuterPackTotalsLabel.AutoSize = true;
			this.OuterPackTotalsLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|ecad8baf-d6e0-49cd-a0b0-124f17444a95", "Outer Pack Totals");
			this.OuterPackTotalsLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OuterPackTotalsLabel.IsFontBold = true;
			this.OuterPackTotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.OuterPackTotalsLabel.Name = "OuterPackTotalsLabel";
			this.OuterPackTotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 14, true);
			this.OuterPackTotalsLabel.TabIndex = 0;
			this.OuterPackTotalsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// JS_Calc_TotalVolumeUnitBoundTextBox2
			//
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalVolumeUnitBoundTextBox2, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_Calc_TotalVolumeUnitBoundTextBox2, false);
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 27, true);
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.Name = "JS_Calc_TotalVolumeUnitBoundTextBox2";
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.TabIndex = 11;
			this.JS_Calc_TotalVolumeUnitBoundTextBox2.Text = "UNIT";
			//
			// ShipmentWeightUnitTextBox
			//
			this.ShipmentWeightUnitTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentWeightUnitTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ShipmentWeightUnitTextBox, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.ShipmentWeightUnitTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentWeightUnitTextBox, false);
			this.ShipmentWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 27, true);
			this.ShipmentWeightUnitTextBox.Name = "ShipmentWeightUnitTextBox";
			this.ShipmentWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.ShipmentWeightUnitTextBox.TabIndex = 9;
			this.ShipmentWeightUnitTextBox.Text = "UNIT";
			//
			// JS_ActualWeightBoundTextBox
			//
			this.JS_ActualWeightBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_ActualWeightBoundTextBox, "JS_ActualWeightReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ActualWeightReadOnly)));
			this.JS_ActualWeightBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|4efccb39-cb52-4cec-8387-c28a0638d694", "Weight", "The total weight of outer packages across this shipment.");
			this.JS_ActualWeightBoundTextBox.DecimalPlaces = 2;
			this.JS_ActualWeightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 23, true);
			this.JS_ActualWeightBoundTextBox.Name = "JS_ActualWeightBoundTextBox";
			this.JS_ActualWeightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_ActualWeightBoundTextBox.TabIndex = 8;
			this.JS_ActualWeightBoundTextBox.Text = "0.000";
			this.JS_ActualWeightBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_ActualVolumeBoundTextBox
			//
			this.JS_ActualVolumeBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_ActualVolumeBoundTextBox, "JS_ActualVolumeReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_ActualVolumeReadOnly)));
			this.JS_ActualVolumeBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|44a4860a-993c-48a2-a099-5b37c600ed34", "Volume", "The total volume of outer packages across this shipment.");
			this.JS_ActualVolumeBoundTextBox.DecimalPlaces = 2;
			this.JS_ActualVolumeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 23, true);
			this.JS_ActualVolumeBoundTextBox.Name = "JS_ActualVolumeBoundTextBox";
			this.JS_ActualVolumeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_ActualVolumeBoundTextBox.TabIndex = 10;
			this.JS_ActualVolumeBoundTextBox.Text = "0.000";
			this.JS_ActualVolumeBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_OuterPacksBoundTextBox
			//
			this.JS_OuterPacksBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_OuterPacksBoundTextBox, "JS_OuterPacksReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).JS_OuterPacksReadOnly)));
			this.JS_OuterPacksBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|37569088-d5bb-44e6-b487-df6740e9a71b", "Outer Packages", "The total number of outer packages across this shipment.");
			this.JS_OuterPacksBoundTextBox.DecimalPlaces = 0;
			this.JS_OuterPacksBoundTextBox.Decimals = 0;
			this.JS_OuterPacksBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 23, true);
			this.JS_OuterPacksBoundTextBox.Name = "JS_OuterPacksBoundTextBox";
			this.JS_OuterPacksBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JS_OuterPacksBoundTextBox.TabIndex = 7;
			this.JS_OuterPacksBoundTextBox.Text = "0";
			this.JS_OuterPacksBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_Calc_TotalVolumeUnitBoundTextBox
			//
			this.JS_Calc_TotalVolumeUnitBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.JS_Calc_TotalVolumeUnitBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalVolumeUnitBoundTextBox, "TotalPackLineVolumeUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).TotalPackLineVolumeUnit)));
			this.JS_Calc_TotalVolumeUnitBoundTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JS_Calc_TotalVolumeUnitBoundTextBox, false);
			this.JS_Calc_TotalVolumeUnitBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 6, true);
			this.JS_Calc_TotalVolumeUnitBoundTextBox.Name = "JS_Calc_TotalVolumeUnitBoundTextBox";
			this.JS_Calc_TotalVolumeUnitBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.JS_Calc_TotalVolumeUnitBoundTextBox.TabIndex = 5;
			this.JS_Calc_TotalVolumeUnitBoundTextBox.Text = "UNIT";
			//
			// PacklineTotalWeightUnitTextBox
			//
			this.PacklineTotalWeightUnitTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PacklineTotalWeightUnitTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PacklineTotalWeightUnitTextBox, "TotalPackLineWeightUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingShipment)(null)).TotalPackLineWeightUnit)));
			this.PacklineTotalWeightUnitTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PacklineTotalWeightUnitTextBox, false);
			this.PacklineTotalWeightUnitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 6, true);
			this.PacklineTotalWeightUnitTextBox.Name = "PacklineTotalWeightUnitTextBox";
			this.PacklineTotalWeightUnitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 12, true);
			this.PacklineTotalWeightUnitTextBox.TabIndex = 3;
			this.PacklineTotalWeightUnitTextBox.Text = "UNIT";
			//
			// JS_Calc_TotalWeightBoundTextBox
			//
			this.JS_Calc_TotalWeightBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalWeightBoundTextBox, "TotalOuterPacksWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).TotalOuterPacksWeight)));
			this.JS_Calc_TotalWeightBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|09654f27-3221-4f6e-afb5-537184f0466d", "Weight", "The total weight of outer packages.");
			this.JS_Calc_TotalWeightBoundTextBox.DecimalPlaces = 2;
			this.JS_Calc_TotalWeightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 2, true);
			this.JS_Calc_TotalWeightBoundTextBox.Name = "JS_Calc_TotalWeightBoundTextBox";
			this.JS_Calc_TotalWeightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_Calc_TotalWeightBoundTextBox.TabIndex = 2;
			this.JS_Calc_TotalWeightBoundTextBox.Text = "0.000";
			this.JS_Calc_TotalWeightBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_Calc_TotalVolumeBoundTextBox
			//
			this.JS_Calc_TotalVolumeBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalVolumeBoundTextBox, "TotalOuterPacksVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).TotalOuterPacksVolume)));
			this.JS_Calc_TotalVolumeBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|8b1984de-3e6b-4814-8dbf-ec912caf19d6", "Volume", "The total volume of outer packages.");
			this.JS_Calc_TotalVolumeBoundTextBox.DecimalPlaces = 2;
			this.JS_Calc_TotalVolumeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 2, true);
			this.JS_Calc_TotalVolumeBoundTextBox.Name = "JS_Calc_TotalVolumeBoundTextBox";
			this.JS_Calc_TotalVolumeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 18, true);
			this.JS_Calc_TotalVolumeBoundTextBox.TabIndex = 4;
			this.JS_Calc_TotalVolumeBoundTextBox.Text = "0.000";
			this.JS_Calc_TotalVolumeBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JS_Calc_TotalPackagesBoundTextBox
			//
			this.JS_Calc_TotalPackagesBoundTextBox.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JS_Calc_TotalPackagesBoundTextBox, "TotalOuterPacks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingShipment)(null)).TotalOuterPacks)));
			this.JS_Calc_TotalPackagesBoundTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|41755806-a50a-4f78-b8c8-71b4569c8cf1", "Outer Packages", "The total number of outer packages.");
			this.JS_Calc_TotalPackagesBoundTextBox.DecimalPlaces = 0;
			this.JS_Calc_TotalPackagesBoundTextBox.Decimals = 0;
			this.JS_Calc_TotalPackagesBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 2, true);
			this.JS_Calc_TotalPackagesBoundTextBox.Name = "JS_Calc_TotalPackagesBoundTextBox";
			this.JS_Calc_TotalPackagesBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 18, true);
			this.JS_Calc_TotalPackagesBoundTextBox.TabIndex = 1;
			this.JS_Calc_TotalPackagesBoundTextBox.Text = "0";
			this.JS_Calc_TotalPackagesBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// GoodsDetailGroupBox
			//
			this.GoodsDetailGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.GoodsDetailGroupBox.Controls.Add(this.JL_DescriptionTextBox);
			this.GoodsDetailGroupBox.Controls.Add(this.JL_OriginCodeFindBox);
			this.GoodsDetailGroupBox.Controls.Add(this.JL_CommodityCodeFindBox);
			this.GoodsDetailGroupBox.Controls.Add(this.HarmonisedCodeFindBox);
			this.GoodsDetailGroupBox.Controls.Add(this.HSDetailsLinkLabel);
			this.GoodsDetailGroupBox.Controls.Add(this.HSCountryFindBox);
			this.GoodsDetailGroupBox.Controls.Add(this.HSCodeFindBox);
			this.GoodsDetailGroupBox.Controls.Add(this.HSLinkLabel);
			this.GoodsDetailGroupBox.Controls.Add(this.zTextBox1);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoodsDetailGroupBox, false);
			this.GoodsDetailGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.GoodsDetailGroupBox.Name = "GoodsDetailGroupBox";
			this.GoodsDetailGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 220, true);
			this.GoodsDetailGroupBox.TabIndex = 0;
			this.GoodsDetailGroupBox.TabStop = false;
			//
			// DangerousGoodsGroup Box
			//
			this.DangerousGoodsGroupBox.Controls.Add(this.DGDetailsLinkLabel);
			this.DangerousGoodsGroupBox.Controls.Add(this.IMOClassDropEdit);
			this.DangerousGoodsGroupBox.Controls.Add(this.DGGuidFindBox);
			this.DangerousGoodsGroupBox.Controls.Add(this.DGLinkLabel);
			this.DangerousGoodsGroupBox.Controls.Add(this.FlashPointCalcEdit);
			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				this.DangerousGoodsGroupBox.Controls.Add(this.DIIsCombustibleCheckBox);
			}
			this.DangerousGoodsGroupBox.Controls.Add(this.FlashPointUnitLabel);
			this.DangerousGoodsGroupBox.Controls.Add(this.DGContactGuidFindBox);
			this.DangerousGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 3, true);
			this.DangerousGoodsGroupBox.Name = "DangerousGoodsGroupBox";
			this.DangerousGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 115, true);
			this.DangerousGoodsGroupBox.TabIndex = 0;
			this.DangerousGoodsGroupBox.TabStop = false;
			this.DangerousGoodsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("13f2ac5f-2f48-2abe-4fdb-e85e99a0d831", "Dangerous Goods");
			//
			// DGDetailsLinkLabel
			//
			this.DGDetailsLinkLabel.AutoSize = true;
			this.DGDetailsLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("80CFBEED-906F-44BC-B07C-EF9E56EE2F61", "DG Details", "Dangerous Goods Details", "");
			this.DGDetailsLinkLabel.IsFontBold = false;
			this.DGDetailsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 89, true);
			this.DGDetailsLinkLabel.Name = "DGDetailsLinkLabel";
			this.DGDetailsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 14, true);
			this.DGDetailsLinkLabel.TabIndex = 16;
			this.DGDetailsLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.DGManagementLinkLabel_LinkClicked);
			//
			// IMOClassDropEdit
			//
			this.IMOClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IMOClassDropEdit, "OuterPackLines.UNDGs+FirstItemForBinding.DI_IMOClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UNDGDataItem)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_IMOClass)));
			this.IMOClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 40, true);
			this.IMOClassDropEdit.Name = "IMOClassDropEdit";
			this.IMOClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.IMOClassDropEdit.TabIndex = 7;
			//
			// DGCodeFindBox
			//
			this.DGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGGuidFindBox, "OuterPackLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((UNDGDataItem)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 16, true);
			this.DGGuidFindBox.Name = "DGGuidFindBox";
			this.DGGuidFindBox.ShouldResize = true;
			this.DGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 18, true);
			this.DGGuidFindBox.TabIndex = 6;
			//
			// DGLinkLabel
			//
			this.DGLinkLabel.AutoSize = true;
			this.DGLinkLabel.IsFontBold = false;
			this.DGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 16, true);
			this.DGLinkLabel.Name = "DGLinkLabel";
			this.DGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.DGLinkLabel.TabIndex = 5;
			this.DGLinkLabel.TabStop = false;

			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				//
				// DIIsCombustibleCheckBox
				//
				this.BindingSource.SetBindingMember(this.DIIsCombustibleCheckBox, "OuterPackLines.UNDGs+FirstItemForBinding.DI_IsCombustible");
				// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((UNDGDataItem)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_IsCombustible)));
				this.DIIsCombustibleCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("8BE2E66B-A3CE-43DB-A587-5D9D0EFA2AB9", "Has FP", "Has Flash Point", "");
				this.DIIsCombustibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
				this.DIIsCombustibleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 66, true);
				this.DIIsCombustibleCheckBox.Name = "DIIsCombustibleCheckBox";
				this.DIIsCombustibleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
				this.DIIsCombustibleCheckBox.TabIndex = 10;
			}
			//
			// FlashPointCalcEdit
			//
			this.BindingSource.SetBindingMember(this.FlashPointCalcEdit, "OuterPackLines.UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((UNDGDataItem)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.FlashPointCalcEdit.DecimalPlaces = 1;
			this.FlashPointCalcEdit.Decimals = 1;
			this.FlashPointCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 63, true);
			this.FlashPointCalcEdit.Name = "FlashPointCalcEdit";
			this.FlashPointCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.FlashPointCalcEdit.TabIndex = 8;
			this.FlashPointCalcEdit.Text = "0.0";
			this.FlashPointCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// FlashPointUnitTextBox
			//
			this.FlashPointUnitLabel.AutoSize = true;
			this.FlashPointUnitLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f5624eb1-6777-c0af-40b0-1da73e97469b", "(C)");
			this.FlashPointUnitLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FlashPointUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 67, true);
			this.FlashPointUnitLabel.Name = "FlashPointUnitLabel";
			//
			// JL_DescriptionTextBox
			//
			this.BindingSource.SetBindingMember(this.JL_DescriptionTextBox, "OuterPackLines.JL_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Description)));
			this.JL_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 120, true);
			this.JL_DescriptionTextBox.Multiline = true;
			this.JL_DescriptionTextBox.Name = "JL_DescriptionTextBox";
			this.JL_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 40, true);
			this.JL_DescriptionTextBox.TabIndex = 18;
			//
			// DGContactGuidFindBox
			//
			this.DGContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGContactGuidFindBox, "OuterPackLines.UNDGs+FirstItemForBinding.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((UNDGDataItem)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_OC_DGContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).UNDGs.Contacts)));
			this.DGContactGuidFindBox.BindToList = "OuterPackLines.UNDGs+Contacts";
			this.DGContactGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.DGContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 86, true);
			this.DGContactGuidFindBox.Name = "DGContactGuidFindBox";
			this.DGContactGuidFindBox.PreBoundMaxLength = 15;
			this.DGContactGuidFindBox.ShouldResize = true;
			this.DGContactGuidFindBox.ShowDescriptionBox = false;
			this.DGContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.DGContactGuidFindBox.TabIndex = 13;
			//
			// JL_OriginCodeFindBox
			//
			this.JL_OriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_OriginCodeFindBox, "OuterPackLines.JL_RN_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_RN_NKOrigin)));
			this.JL_OriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 96, true);
			this.JL_OriginCodeFindBox.Name = "JL_OriginCodeFindBox";
			this.JL_OriginCodeFindBox.PreBoundMaxLength = 2;
			this.JL_OriginCodeFindBox.ShouldResize = true;
			this.JL_OriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 18, true);
			this.JL_OriginCodeFindBox.TabIndex = 5;
			//
			// JL_CommodityCodeFindBox
			//
			this.JL_CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_CommodityCodeFindBox, "OuterPackLines.JL_RH_NKCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_RH_NKCommodityCode)));
			this.JL_CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 13, true);
			this.JL_CommodityCodeFindBox.Name = "JL_CommodityCodeFindBox";
			this.JL_CommodityCodeFindBox.PreBoundMaxLength = 4;
			this.JL_CommodityCodeFindBox.ShouldResize = true;
			this.JL_CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 18, true);
			this.JL_CommodityCodeFindBox.TabIndex = 0;
			//
			// HarmonisedCodeFindBox
			//
			this.HarmonisedCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HarmonisedCodeFindBox, "OuterPackLines.JL_HarmonisedCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_HarmonisedCode)));
			this.HarmonisedCodeFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("23e450fd-5dac-4db7-b374-558526d8543a", "Harmonized Code");
			this.HarmonisedCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 34, true);
			this.HarmonisedCodeFindBox.Name = "HarmonisedCodeFindBox";
			this.HarmonisedCodeFindBox.ShouldResize = true;
			this.HarmonisedCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 18, true);
			this.HarmonisedCodeFindBox.TabIndex = 1;
			this.HarmonisedCodeFindBox.TariffType = "HSN";
			//
			// HSDetailsLinkLabel
			//
			this.HSDetailsLinkLabel.AutoSize = true;
			this.HSDetailsLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ebd006eb-5c9b-4d39-ba97-90a80595ae47", "HS Details", "Harmonized System Code Details", "");
			this.HSDetailsLinkLabel.IsFontBold = false;
			this.HSDetailsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 76, true);
			this.HSDetailsLinkLabel.Name = "HSDetailsLinkLabel";
			this.HSDetailsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 14, true);
			this.HSDetailsLinkLabel.TabIndex = 4;
			this.HSDetailsLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.HSCodeManagementLinkLabel_LinkClicked);
			//
			// HSCountryFindBox
			//
			this.HSCountryFindBox.AllowDrop = true;
			this.HSCountryFindBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.HSCountryFindBox, "OuterPackLines.HarmonisedCodes+FirstItemForBinding.JLH_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobPackLineHarmonisedCode)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).HarmonisedCodes.FirstItemForBinding)).SyncRoot)).JLH_RN_NKCountry)));
			this.HSCountryFindBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("abf0506c-7429-41af-a9bb-66f331903cd5", "HS Code", "HS Country/Region Code", "Harmonized System Country/Region Code");
			this.HSCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 54, true);
			this.HSCountryFindBox.Name = "HSCountryFindBox";
			this.HSCountryFindBox.PreBoundMaxLength = 4;
			this.HSCountryFindBox.ShouldResize = true;
			this.HSCountryFindBox.ShowDescriptionBox = false;
			this.HSCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 18, true);
			this.HSCountryFindBox.TabIndex = 2;
			//
			// HSCodeFindBox
			//
			this.HSCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HSCodeFindBox, "OuterPackLines.HarmonisedCodes+FirstItemForBinding.JLH_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobPackLineHarmonisedCode)(((System.Collections.IList)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).HarmonisedCodes.FirstItemForBinding)).SyncRoot)).JLH_Code)));
			this.HSCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 54, true);
			this.HSCodeFindBox.PreBoundMaxLength = 4;
			this.HSCodeFindBox.ShouldResize = true;
			this.HSCodeFindBox.ShowDescriptionBox = false;
			this.HSCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 18, true);
			this.HSCodeFindBox.TabIndex = 3;
			this.HSCodeFindBox.TariffType = "HSN";
			//
			// HSLinkLabel
			//
			this.HSLinkLabel.AutoSize = true;
			this.HSLinkLabel.IsFontBold = false;
			this.HSLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 56, true);
			this.HSLinkLabel.Name = "HSLinkLabel";
			this.HSLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.HSLinkLabel.TabIndex = 14;
			this.HSLinkLabel.TabStop = false;
			//
			// zTextBox1
			//
			this.BindingSource.SetBindingMember(this.zTextBox1, "OuterPackLines.JL_RefNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_RefNumber)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 163, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.zTextBox1.TabIndex = 20;
			//
			// DetailsPanel
			//
			this.DetailsPanel.AutoScroll = true;
			this.DetailsPanel.Controls.Add(this.PackagesDetailTabControl);
			this.DetailsPanel.Controls.Add(this.GoodsDetailGroupBox);
			this.DetailsPanel.Controls.Add(this.DangerousGoodsGroupBox);
			this.DetailsPanel.Controls.Add(this.TemperatureControlledGroupBox);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 350, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 228, true);
			this.DetailsPanel.TabIndex = 4;
			//
			// PackagesDetailTabControl
			//
			this.PackagesDetailTabControl.Controls.Add(this.WeightAndMeasureTabPage);
			this.PackagesDetailTabControl.Controls.Add(this.PkgPackageDetailsTabPage);
			this.PackagesDetailTabControl.Controls.Add(this.locationTabPage);
			this.PackagesDetailTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.PackagesDetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 3, true);
			this.PackagesDetailTabControl.Name = "PackagesDetailTabControl";
			this.PackagesDetailTabControl.SelectedIndex = 0;
			this.PackagesDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 228, true);
			this.PackagesDetailTabControl.TabIndex = 1;
			//
			// WeightAndMeasureTabPage
			//
			this.WeightAndMeasureTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|49d39aca-53b6-4a02-bcce-1287d154e8ca", "Packing");
			this.WeightAndMeasureTabPage.Controls.Add(this.WeightAndMeasureGroupBox);
			this.WeightAndMeasureTabPage.Controls.Add(this.OutturnGroupBox);
			this.WeightAndMeasureTabPage.Controls.Add(this.LastKnownTransitWarehouseControl);
			this.WeightAndMeasureTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.WeightAndMeasureTabPage.Name = "WeightAndMeasureTabPage";
			this.WeightAndMeasureTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 212, true);
			this.WeightAndMeasureTabPage.TabIndex = 0;
			//
			// WeightAndMeasureGroupBox
			//
			this.WeightAndMeasureGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | System.Windows.Forms.AnchorStyles.Left)));
			this.WeightAndMeasureGroupBox.Controls.Add(this.PackingRowLayoutPanel);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WeightAndMeasureGroupBox, false);
			this.WeightAndMeasureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.WeightAndMeasureGroupBox.Name = "WeightAndMeasureGroupBox";
			this.WeightAndMeasureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 196, true);
			this.WeightAndMeasureGroupBox.TabIndex = 0;
			this.WeightAndMeasureGroupBox.TabStop = false;
			//
			// PackingRowLayoutPanel
			//
			this.PackingRowLayoutPanel.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top) | System.Windows.Forms.AnchorStyles.Left)));
			this.PackingRowLayoutPanel.Controls.Add(this.JL_PackageCountCalcDropEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_ActualWeightCalcDropEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_ActualVolumeCalcDropEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_LoadingMetersCalcDropEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_LengthCalcEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_WidthCalcEdit);
			this.PackingRowLayoutPanel.Controls.Add(this.JL_HeightCalcDropEdit);
			this.PackingRowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 10, true);
			this.PackingRowLayoutPanel.Name = "PackingRowLayoutPanel";
			this.PackingRowLayoutPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(27);
			this.PackingRowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 183, true);
			this.PackingRowLayoutPanel.TabIndex = 7;
			//
			// JL_PackageCountCalcDropEdit
			//
			this.JL_PackageCountCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_PackageCountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_F3_NKPackType)));
			this.JL_PackageCountCalcDropEdit.BindToAmount = "OuterPackLines.JL_PackageCount";
			this.JL_PackageCountCalcDropEdit.BindToUnit = "OuterPackLines.JL_F3_NKPackType";
			this.JL_PackageCountCalcDropEdit.Decimals = 0;
			this.JL_PackageCountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JL_PackageCountCalcDropEdit.MaxValue = new decimal(new int[] {
			1000000,
			0,
			0,
			0 });
			this.JL_PackageCountCalcDropEdit.Name = "JL_PackageCountCalcDropEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_PackageCountCalcDropEdit, 0);
			this.JL_PackageCountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.JL_PackageCountCalcDropEdit.TabIndex = 1;
			this.JL_PackageCountCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// JL_ActualWeightCalcDropEdit
			//
			this.JL_ActualWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_ActualWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			this.JL_ActualWeightCalcDropEdit.BindToAmount = "OuterPackLines.JL_ActualWeight";
			this.JL_ActualWeightCalcDropEdit.BindToUnit = "OuterPackLines.JL_ActualWeightUQ";
			this.JL_ActualWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.JL_ActualWeightCalcDropEdit.Name = "JL_ActualWeightCalcDropEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_ActualWeightCalcDropEdit, 1);
			this.JL_ActualWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.JL_ActualWeightCalcDropEdit.TabIndex = 2;
			this.JL_ActualWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JL_ActualVolumeCalcDropEdit
			//
			this.JL_ActualVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_ActualVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			this.JL_ActualVolumeCalcDropEdit.BindToAmount = "OuterPackLines.JL_ActualVolume";
			this.JL_ActualVolumeCalcDropEdit.BindToUnit = "OuterPackLines.JL_ActualVolumeUQ";
			this.JL_ActualVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 43, true);
			this.JL_ActualVolumeCalcDropEdit.Name = "JL_ActualVolumeCalcDropEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_ActualVolumeCalcDropEdit, 2);
			this.JL_ActualVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.JL_ActualVolumeCalcDropEdit.TabIndex = 3;
			this.JL_ActualVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JL_LoadingMetersCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.JL_LoadingMetersCalcDropEdit, "OuterPackLines.JL_LoadingMeters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_LoadingMeters)));
			this.JL_LoadingMetersCalcDropEdit.DecimalPlaces = 3;
			this.JL_LoadingMetersCalcDropEdit.Decimals = 3;
			this.JL_LoadingMetersCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.JL_LoadingMetersCalcDropEdit.Name = "JL_LoadingMetersCalcDropEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_LoadingMetersCalcDropEdit, 3);
			this.JL_LoadingMetersCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.JL_LoadingMetersCalcDropEdit.TabIndex = 4;
			this.JL_LoadingMetersCalcDropEdit.Text = "0.000";
			this.JL_LoadingMetersCalcDropEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_LengthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_LengthCalcEdit, "OuterPackLines.JL_Length");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Length)));
			this.JL_LengthCalcEdit.DecimalPlaces = 2;
			this.JL_LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 86, true);
			this.JL_LengthCalcEdit.Name = "JL_LengthCalcEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_LengthCalcEdit, 4);
			this.JL_LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.JL_LengthCalcEdit.TabIndex = 5;
			this.JL_LengthCalcEdit.Text = "0.000";
			this.JL_LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_WidthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_WidthCalcEdit, "OuterPackLines.JL_Width");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Width)));
			this.JL_WidthCalcEdit.DecimalPlaces = 2;
			this.JL_WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
			this.JL_WidthCalcEdit.Name = "JL_WidthCalcEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_WidthCalcEdit, 5);
			this.JL_WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 18, true);
			this.JL_WidthCalcEdit.TabIndex = 6;
			this.JL_WidthCalcEdit.Text = "0.000";
			this.JL_WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_HeightCalcDropEdit
			//
			this.JL_HeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_HeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_UnitOfDimension)));
			this.JL_HeightCalcDropEdit.BindToAmount = "OuterPackLines.JL_Height";
			this.JL_HeightCalcDropEdit.BindToUnit = "OuterPackLines.JL_UnitOfDimension";
			this.JL_HeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.JL_HeightCalcDropEdit.Name = "JL_HeightCalcDropEdit";
			this.PackingRowLayoutPanel.SetRow(this.JL_HeightCalcDropEdit, 6);
			this.JL_HeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 18, true);
			this.JL_HeightCalcDropEdit.TabIndex = 7;
			this.JL_HeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// OutturnGroupBox
			//
			this.OutturnGroupBox.Controls.Add(this.MarksAndNumbersTextBox);
			this.OutturnGroupBox.Controls.Add(this.JL_UnitOfDimensionOutturnedTextBox);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnedLengthCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnedHeightCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnedWidthCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnedVolumeCalcDropEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnedWeightCalcDropEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_DamagedCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_PillagedCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnCalcEdit);
			this.OutturnGroupBox.Controls.Add(this.JL_OutturnCommentTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutturnGroupBox, false);
			this.OutturnGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 64, true);
			this.OutturnGroupBox.Name = "OutturnGroupBox";
			this.OutturnGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 134, true);
			this.OutturnGroupBox.TabIndex = 1;
			this.OutturnGroupBox.TabStop = false;
			//
			// MarksAndNumbersTextBox
			//
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "OuterPackLines.JL_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_MarksAndNumbers)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.MarksAndNumbersTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 95, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 34, true);
			this.MarksAndNumbersTextBox.TabIndex = 10;
			//
			// JL_UnitOfDimensionOutturnedTextBox
			//
			this.BindingSource.SetBindingMember(this.JL_UnitOfDimensionOutturnedTextBox, "OuterPackLines.JL_OutturnUD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnUD)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JL_UnitOfDimensionOutturnedTextBox, false);
			this.JL_UnitOfDimensionOutturnedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 57, true);
			this.JL_UnitOfDimensionOutturnedTextBox.Name = "JL_UnitOfDimensionOutturnedTextBox";
			this.JL_UnitOfDimensionOutturnedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 18, true);
			this.JL_UnitOfDimensionOutturnedTextBox.TabIndex = 8;
			//
			// JL_OutturnedLengthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_OutturnedLengthCalcEdit, "OuterPackLines.JL_OutturnedLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnedLength)));
			this.JL_OutturnedLengthCalcEdit.DecimalPlaces = 2;
			this.JL_OutturnedLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 57, true);
			this.JL_OutturnedLengthCalcEdit.Name = "JL_OutturnedLengthCalcEdit";
			this.JL_OutturnedLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_OutturnedLengthCalcEdit.TabIndex = 5;
			this.JL_OutturnedLengthCalcEdit.Text = "0.000";
			this.JL_OutturnedLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_OutturnedHeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_OutturnedHeightCalcEdit, "OuterPackLines.JL_OutturnedHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnedHeight)));
			this.JL_OutturnedHeightCalcEdit.DecimalPlaces = 2;
			this.JL_OutturnedHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 57, true);
			this.JL_OutturnedHeightCalcEdit.Name = "JL_OutturnedHeightCalcEdit";
			this.JL_OutturnedHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_OutturnedHeightCalcEdit.TabIndex = 7;
			this.JL_OutturnedHeightCalcEdit.Text = "0.000";
			this.JL_OutturnedHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_OutturnedWidthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_OutturnedWidthCalcEdit, "OuterPackLines.JL_OutturnedWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnedWidth)));
			this.JL_OutturnedWidthCalcEdit.DecimalPlaces = 2;
			this.JL_OutturnedWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 57, true);
			this.JL_OutturnedWidthCalcEdit.Name = "JL_OutturnedWidthCalcEdit";
			this.JL_OutturnedWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_OutturnedWidthCalcEdit.TabIndex = 6;
			this.JL_OutturnedWidthCalcEdit.Text = "0.000";
			this.JL_OutturnedWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_OutturnedVolumeCalcDropEdit
			//
			this.JL_OutturnedVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_OutturnedVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PackLineVolumeUnit)));
			this.JL_OutturnedVolumeCalcDropEdit.BindToAmount = "OuterPackLines.JL_OutturnedVolume";
			this.JL_OutturnedVolumeCalcDropEdit.BindToUnit = "OuterPackLines.PackLineVolumeUnit";
			this.JL_OutturnedVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 35, true);
			this.JL_OutturnedVolumeCalcDropEdit.Name = "JL_OutturnedVolumeCalcDropEdit";
			this.JL_OutturnedVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.JL_OutturnedVolumeCalcDropEdit.TabIndex = 4;
			this.JL_OutturnedVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JL_OutturnedWeightCalcDropEdit
			//
			this.JL_OutturnedWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JL_OutturnedWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).PackLineWeightUnit)));
			this.JL_OutturnedWeightCalcDropEdit.BindToAmount = "OuterPackLines.JL_OutturnedWeight";
			this.JL_OutturnedWeightCalcDropEdit.BindToUnit = "OuterPackLines.PackLineWeightUnit";
			this.JL_OutturnedWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 35, true);
			this.JL_OutturnedWeightCalcDropEdit.Name = "JL_OutturnedWeightCalcDropEdit";
			this.JL_OutturnedWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.JL_OutturnedWeightCalcDropEdit.TabIndex = 3;
			this.JL_OutturnedWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JL_DamagedCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_DamagedCalcEdit, "OuterPackLines.JL_Damaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Damaged)));
			this.JL_DamagedCalcEdit.DecimalPlaces = 0;
			this.JL_DamagedCalcEdit.Decimals = 0;
			this.JL_DamagedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 14, true);
			this.JL_DamagedCalcEdit.Name = "JL_DamagedCalcEdit";
			this.JL_DamagedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_DamagedCalcEdit.TabIndex = 1;
			this.JL_DamagedCalcEdit.Text = "0";
			this.JL_DamagedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_PillagedCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_PillagedCalcEdit, "OuterPackLines.JL_Pillaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Pillaged)));
			this.JL_PillagedCalcEdit.DecimalPlaces = 0;
			this.JL_PillagedCalcEdit.Decimals = 0;
			this.JL_PillagedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 14, true);
			this.JL_PillagedCalcEdit.Name = "JL_PillagedCalcEdit";
			this.JL_PillagedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_PillagedCalcEdit.TabIndex = 2;
			this.JL_PillagedCalcEdit.Text = "0";
			this.JL_PillagedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_OutturnCalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_OutturnCalcEdit, "OuterPackLines.JL_Outturn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_Outturn)));
			this.JL_OutturnCalcEdit.DecimalPlaces = 0;
			this.JL_OutturnCalcEdit.Decimals = 0;
			this.JL_OutturnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 14, true);
			this.JL_OutturnCalcEdit.Name = "JL_OutturnCalcEdit";
			this.JL_OutturnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 18, true);
			this.JL_OutturnCalcEdit.TabIndex = 0;
			this.JL_OutturnCalcEdit.Text = "0";
			this.JL_OutturnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_OutturnCommentTextBox
			//
			this.JL_OutturnCommentTextBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.JL_OutturnCommentTextBox, "OuterPackLines.JL_OutturnComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_OutturnComment)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JL_OutturnCommentTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JL_OutturnCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 95, true);
			this.JL_OutturnCommentTextBox.Multiline = true;
			this.JL_OutturnCommentTextBox.Name = "JL_OutturnCommentTextBox";
			this.JL_OutturnCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 34, true);
			this.JL_OutturnCommentTextBox.TabIndex = 9;
			//
			// LastKnownTransitWarehouseControl
			//
			this.LastKnownTransitWarehouseControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 4, true);
			this.LastKnownTransitWarehouseControl.Name = "LastKnownTransitWarehouseControl";
			this.LastKnownTransitWarehouseControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 59, true);
			this.LastKnownTransitWarehouseControl.TabIndex = 1;
			//
			// locationTabPage
			//
			this.locationTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|ec0d9a47-7196-4971-a245-51c40c947b0b", "Location");
			this.locationTabPage.Controls.Add(this.locationsControl);
			this.locationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.locationTabPage.Name = "locationTabPage";
			this.locationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.locationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 212, true);
			this.locationTabPage.TabIndex = 3;
			//
			// locationsControl
			//
			this.locationsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.locationsControl, "OuterPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.locationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.locationsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.locationsControl.Name = "locationsControl";
			this.locationsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 206, true);
			this.locationsControl.TabIndex = 0;
			//
			// CustomFieldsTabPage
			//
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ContainersUserControl|b176a406-718d-43e3-ad28-0e9b4b0f3510", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomAttrib4TextBox);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomAttrib3TextBox);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomAttrib2TextBox);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomAttrib1TextBox);
			this.CustomFieldsTabPage.Controls.Add(this.CustomDate1DateEdit);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomFlag2CheckBox);
			this.CustomFieldsTabPage.Controls.Add(this.CustomDate2DateEdit);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomFlag1CheckBox);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomDecimal2);
			this.CustomFieldsTabPage.Controls.Add(this.JL_CustomDecimal1CalcEdit);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 212, true);
			this.CustomFieldsTabPage.TabIndex = 2;
			//
			// JL_CustomAttrib4TextBox
			//
			this.BindingSource.SetBindingMember(this.JL_CustomAttrib4TextBox, "OuterPackLines.JL_CustomAttrib4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomAttrib4)));
			this.JL_CustomAttrib4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 73, true);
			this.JL_CustomAttrib4TextBox.Name = "JL_CustomAttrib4TextBox";
			this.JL_CustomAttrib4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JL_CustomAttrib4TextBox.TabIndex = 3;
			//
			// JL_CustomAttrib3TextBox
			//
			this.BindingSource.SetBindingMember(this.JL_CustomAttrib3TextBox, "OuterPackLines.JL_CustomAttrib3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomAttrib3)));
			this.JL_CustomAttrib3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 51, true);
			this.JL_CustomAttrib3TextBox.Name = "JL_CustomAttrib3TextBox";
			this.JL_CustomAttrib3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JL_CustomAttrib3TextBox.TabIndex = 2;
			//
			// JL_CustomAttrib2TextBox
			//
			this.BindingSource.SetBindingMember(this.JL_CustomAttrib2TextBox, "OuterPackLines.JL_CustomAttrib2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomAttrib2)));
			this.JL_CustomAttrib2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 30, true);
			this.JL_CustomAttrib2TextBox.Name = "JL_CustomAttrib2TextBox";
			this.JL_CustomAttrib2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JL_CustomAttrib2TextBox.TabIndex = 1;
			//
			// JL_CustomAttrib1TextBox
			//
			this.JL_CustomAttrib1TextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.JL_CustomAttrib1TextBox, "OuterPackLines.JL_CustomAttrib1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomAttrib1)));
			this.JL_CustomAttrib1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.JL_CustomAttrib1TextBox.Name = "JL_CustomAttrib1TextBox";
			this.JL_CustomAttrib1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.JL_CustomAttrib1TextBox.TabIndex = 0;
			//
			// CustomDate1DateEdit
			//
			this.CustomDate1DateEdit.AllowDrop = true;
			this.CustomDate1DateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomDate1DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomDate1DateEdit, "OuterPackLines.JL_CustomDate1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomDate1)));
			this.CustomDate1DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 8, true);
			this.CustomDate1DateEdit.Name = "CustomDate1DateEdit";
			this.CustomDate1DateEdit.TabIndex = 4;
			//
			// JL_CustomFlag2CheckBox
			//
			this.BindingSource.SetBindingMember(this.JL_CustomFlag2CheckBox, "OuterPackLines.JL_CustomFlag2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomFlag2)));
			this.JL_CustomFlag2CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JL_CustomFlag2CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 32, true);
			this.JL_CustomFlag2CheckBox.Name = "JL_CustomFlag2CheckBox";
			this.JL_CustomFlag2CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.JL_CustomFlag2CheckBox.TabIndex = 9;
			//
			// CustomDate2DateEdit
			//
			this.CustomDate2DateEdit.AllowDrop = true;
			this.CustomDate2DateEdit.AutoCompleteMonthThreshold = 1;
			this.CustomDate2DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CustomDate2DateEdit, "OuterPackLines.JL_CustomDate2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomDate2)));
			this.CustomDate2DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 30, true);
			this.CustomDate2DateEdit.Name = "CustomDate2DateEdit";
			this.CustomDate2DateEdit.TabIndex = 5;
			//
			// JL_CustomFlag1CheckBox
			//
			this.BindingSource.SetBindingMember(this.JL_CustomFlag1CheckBox, "OuterPackLines.JL_CustomFlag1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomFlag1)));
			this.JL_CustomFlag1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JL_CustomFlag1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 10, true);
			this.JL_CustomFlag1CheckBox.Name = "JL_CustomFlag1CheckBox";
			this.JL_CustomFlag1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.JL_CustomFlag1CheckBox.TabIndex = 8;
			//
			// JL_CustomDecimal2
			//
			this.BindingSource.SetBindingMember(this.JL_CustomDecimal2, "OuterPackLines.JL_CustomDecimal2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomDecimal2)));
			this.JL_CustomDecimal2.DecimalPlaces = 2;
			this.JL_CustomDecimal2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 73, true);
			this.JL_CustomDecimal2.Name = "JL_CustomDecimal2";
			this.JL_CustomDecimal2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 18, true);
			this.JL_CustomDecimal2.TabIndex = 7;
			this.JL_CustomDecimal2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JL_CustomDecimal1CalcEdit
			//
			this.BindingSource.SetBindingMember(this.JL_CustomDecimal1CalcEdit, "OuterPackLines.JL_CustomDecimal1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((ForwardingPackLine)(((System.Collections.IList)(((ForwardingShipment)(null)).OuterPackLines)).SyncRoot)).JL_CustomDecimal1)));
			this.JL_CustomDecimal1CalcEdit.DecimalPlaces = 2;
			this.JL_CustomDecimal1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 51, true);
			this.JL_CustomDecimal1CalcEdit.Name = "JL_CustomDecimal1CalcEdit";
			this.JL_CustomDecimal1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 18, true);
			this.JL_CustomDecimal1CalcEdit.TabIndex = 6;
			this.JL_CustomDecimal1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// Temperature Controlled Group Box
			//
			this.TemperatureControlledGroupBox.Controls.Add(this.TemperatureControlBlock);
			this.TemperatureControlledGroupBox.Name = "TemperatureControlledBlock";
			this.TemperatureControlledGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("948e04d7-c950-c5b9-4c4f-f9cb7d7529b4", "Temperature Controlled");
			this.TemperatureControlledGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 120, true);
			this.TemperatureControlledGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 103, true);
			//
			// Temperature Control Block
			//
			this.BindingSource.SetBindingMember(this.TemperatureControlBlock, "OuterPackLines");
			this.TemperatureControlBlock.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.TemperatureControlBlock.Configure(new PacklineTemperatureControlConfiguration());
			//
			// splitter1
			//
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 346, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 3, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			//
			// splitter2
			//
			this.splitter2.BackColor = System.Drawing.SystemColors.ControlDark;
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 3, true);
			this.splitter2.TabIndex = 2;
			this.splitter2.TabStop = false;
			//
			// splitter3
			//
			this.splitter3.BackColor = System.Drawing.SystemColors.ControlDark;
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 3, true);
			this.splitter3.TabIndex = 1;
			this.splitter3.TabStop = false;
			//
			// ContainersUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackLinesPanel);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.ContainersPanel);
			this.Controls.Add(this.splitter3);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.DetailsPanel);
			this.Controls.Add(this.TotalsPanel);
			this.Controls.Add(this.ConsolsPanel);
			this.Name = "ContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 632, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PkgPackageDetailsTabPage.ResumeLayout(false);
			this.PkgPackageDetailsTabPage.PerformLayout();
			this.PkgPackageDetailControl.ResumeLayout(false);
			this.PkgPackageDetailControl.PerformLayout();
			this.ConsolsPanel.ResumeLayout(false);
			this.ConsolsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobConsolBoundGrid)).EndInit();
			this.JobConsolBoundGrid.ResumeLayout(false);
			this.JobConsolBoundGrid.PerformLayout();
			this.ContainersPanel.ResumeLayout(false);
			this.ContainersPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobContainerGrid)).EndInit();
			this.JobContainerGrid.ResumeLayout(false);
			this.JobContainerGrid.PerformLayout();
			this.PackLinesPanel.ResumeLayout(false);
			this.PackLinesPanel.PerformLayout();
			this.packLinesContol.ResumeLayout(true);
			this.packLinesContol.PerformLayout();
			this.TotalsPanel.ResumeLayout(false);
			this.TotalsPanel.PerformLayout();
			this.TotalsInnerPanel.ResumeLayout(false);
			this.TotalsInnerPanel.PerformLayout();
			this.GoodsDetailGroupBox.ResumeLayout(false);
			this.GoodsDetailGroupBox.PerformLayout();
			this.IMOClassDropEdit.ResumeLayout(true);
			this.IMOClassDropEdit.PerformLayout();
			this.DGGuidFindBox.ResumeLayout(true);
			this.DGGuidFindBox.PerformLayout();
			this.DGContactGuidFindBox.ResumeLayout(true);
			this.DGContactGuidFindBox.PerformLayout();
			this.JL_OriginCodeFindBox.ResumeLayout(true);
			this.JL_OriginCodeFindBox.PerformLayout();
			this.JL_CommodityCodeFindBox.ResumeLayout(true);
			this.JL_CommodityCodeFindBox.PerformLayout();
			this.HarmonisedCodeFindBox.ResumeLayout(true);
			this.HarmonisedCodeFindBox.PerformLayout();
			this.HSCountryFindBox.ResumeLayout(true);
			this.HSCountryFindBox.PerformLayout();
			this.HSCodeFindBox.ResumeLayout(true);
			this.HSCodeFindBox.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.PackagesDetailTabControl.ResumeLayout(false);
			this.PackagesDetailTabControl.PerformLayout();
			this.WeightAndMeasureTabPage.ResumeLayout(false);
			this.WeightAndMeasureTabPage.PerformLayout();
			this.WeightAndMeasureGroupBox.ResumeLayout(false);
			this.WeightAndMeasureGroupBox.PerformLayout();
			this.PackingRowLayoutPanel.ResumeLayout(false);
			this.PackingRowLayoutPanel.PerformLayout();
			this.JL_PackageCountCalcDropEdit.ResumeLayout(true);
			this.JL_PackageCountCalcDropEdit.PerformLayout();
			this.JL_ActualWeightCalcDropEdit.ResumeLayout(true);
			this.JL_ActualWeightCalcDropEdit.PerformLayout();
			this.JL_ActualVolumeCalcDropEdit.ResumeLayout(true);
			this.JL_ActualVolumeCalcDropEdit.PerformLayout();
			this.JL_HeightCalcDropEdit.ResumeLayout(true);
			this.JL_HeightCalcDropEdit.PerformLayout();
			this.OutturnGroupBox.ResumeLayout(false);
			this.OutturnGroupBox.PerformLayout();
			this.DangerousGoodsGroupBox.ResumeLayout(false);
			this.DangerousGoodsGroupBox.PerformLayout();
			this.TemperatureControlledGroupBox.ResumeLayout(false);
			this.TemperatureControlledGroupBox.PerformLayout();
			this.JL_OutturnedVolumeCalcDropEdit.ResumeLayout(true);
			this.JL_OutturnedVolumeCalcDropEdit.PerformLayout();
			this.JL_OutturnedWeightCalcDropEdit.ResumeLayout(true);
			this.JL_OutturnedWeightCalcDropEdit.PerformLayout();
			this.locationTabPage.ResumeLayout(false);
			this.locationTabPage.PerformLayout();
			this.locationsControl.ResumeLayout(true);
			this.locationsControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.CustomDate1DateEdit.ResumeLayout(true);
			this.CustomDate1DateEdit.PerformLayout();
			this.CustomDate2DateEdit.ResumeLayout(true);
			this.CustomDate2DateEdit.PerformLayout();
			this.LastKnownTransitWarehouseControl.ResumeLayout(false);
			this.LastKnownTransitWarehouseControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region Dispose

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
    }
}
