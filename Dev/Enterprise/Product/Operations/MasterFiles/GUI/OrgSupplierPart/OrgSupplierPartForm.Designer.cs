using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSupplierPartForm : ZTemplateForm
	{
		public readonly OrgSupplierPart Part;

		public Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.GUI.ZGroupBox DimensionsWeightGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit OP_MeasureUQBoundDropDownEdit;
		Enterprise.ZArchitecture.ZCalcEdit OP_WidthBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OP_HeightBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OP_DepthBoundCalcEdit;
		public Enterprise.ZArchitecture.ZLabel DimensionLabel;
		Enterprise.ZArchitecture.GUI.ZDropEdit OP_WeightUQBoundDropDownEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit OP_CubicUQBoundDropDownEdit;
		Enterprise.ZArchitecture.ZCalcEdit OP_WeightBoundCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OP_CubicBoundCalcEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox ProductDetailsGroupBox;
		protected ZDropEdit OP_StockKeepingUnitBoundDropEdit;
		Enterprise.ZArchitecture.ZTextBox OP_PartNumBoundTextBox;
		public Enterprise.ZArchitecture.ZTextBox OP_DescBoundTextBox;
		Enterprise.ZArchitecture.ZLabel CubePerLabel;
		Enterprise.ZArchitecture.ZLabel WeightPerLabel;
		Enterprise.ZArchitecture.ZLabel CubeUnitLabel;
		Enterprise.ZArchitecture.ZLabel WeightUnitLabel;
		Enterprise.ZArchitecture.ZCalcEdit OP_CountDecimalPlacesCalcEdit;
		protected ZTabControl BottomTabControl;
		ZTabPage UNDGTabPage;
		protected ZTabPage AdditionalDetailsTabPage;
		ZTabPage RelatedOrgsTabPage;
		ZTabPage ProductUnitsTabPage;
		CargoWise.Windows.UI.KPanel TopPanel;
		CargoWise.Windows.UI.KPanel AdditionalDetailsPanel;
		ZGroupBox ClientDefinedFieldsGroupBox;
		ZDropEdit OP_OrderMultipleUnitBoundDropEdit;
		ZDropEdit OP_F3_NKPackTypeBoundDropEdit;
		ZTextBox OP_DepartmentBoundTextBox;
		ZCalcEdit OP_VendorPackQtyBoundCalcEdit;
		ZTextBox OP_DivisionBoundTextBox;
		ZCalcEdit OP_OrderMultipleQtyBoundCalcEdit;
		ZGroupBox StockControlGroupBox;
		ZCalcEdit OP_LastCostBoundCalcEdit;
		ZCalcEdit OP_QtyInStockBoundCalcEdit;
		ZCodeFindBox CommodityCodeFindBox;
		ZLabel PerPalletLabel;
		ZCalcEdit OP_StockKeepingUnitPerPalletCalcEdit;
		ZCheckBox OP_IsActiveCheckBox;
		ZCalcEdit OP_NetWeightBoundCalcEdit;
		protected ZTabPage NMFCTabPage;
		ZGroupBox zGroupBox5;
		ZCodeFindBox FN_CodeCodeFindBox;
		ZGroupBox ArticleGroupBox;
		ZTextBox ItemNoTextBox;
		ZTextBox FN_ClassTextBox;
		ZTextBox FN_DescriptionTextBox;
		ZGroupBox AdditionalProductDetailsGroupBox;
		public ZTextBox OP_ModelTextBox;
		public ZTextBox OP_BrandTextBox;
		protected ZTabPage ProductBarcodesTabPage;
		protected ZTabPage BOMTabPage;
		CargoWise.Windows.UI.KPanel BOMTopPanel;
		ZCheckBox CanResellCheckBox;
		ZCheckBox CanDisassembleKitCheckBox;
		ZCheckBox AutoPrintAssemblyInstructionsCheckBox;
		ZCheckBox IsComponentPickedOnSalesOrder;
		ZCheckBox IsAutoReplenish;
		ZCodeFindBox CostCurrencyFindBox;
		ZCalcEdit OP_WeightedCostCalcEdit;
		protected ZGrid UNDGsGrid;
		ZGrid BillOfMaterialsGrid;
		ZGrid BillOfMaterialsViewGrid;
		internal RelatedOrganizationUserControl relatedOrganizationsControl1;
		protected ZGrid PartUnitsGrid;
		protected ZGrid PartBarcodesGrid;
		ZWorkflowTabPage WorkflowTabPage;
		ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl1;
		ZCheckBox OP_IsBarcodedCheckBox;
		ZCheckBox OP_KeepUprightCheckBox;
		ZGroupBox SecondaryProductsGroupBox;
		ZGrid SecondaryProductsGrid;
		ZGrid ComponentUsageGrid;
		CargoWise.Windows.UI.KSplitter Splitter1;
		CargoWise.Windows.UI.KSplitter Splitter2;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.UNDGTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UNDGsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.processTemplateCustomFieldsControl1 = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			this.AdditionalDetailsPanel = new CargoWise.Windows.UI.KPanel();
			this.AdditionalProductDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OP_ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OP_BrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StockControlGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OP_WeightedCostCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CostCurrencyFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OP_LastCostBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_QtyInStockBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ClientDefinedFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OP_OrderMultipleUnitBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OP_F3_NKPackTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OP_DepartmentBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OP_VendorPackQtyBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_DivisionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OP_OrderMultipleQtyBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BottomTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.RelatedOrgsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.relatedOrganizationsControl1 = new Enterprise.MasterFiles.GUI.RelatedOrganizationUserControl();
			this.ProductUnitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PartUnitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductBarcodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PartBarcodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DimensionsWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OP_KeepUprightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OP_NetWeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PerPalletLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OP_StockKeepingUnitPerPalletCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_MeasureUQBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CubePerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightPerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CubeUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WeightUnitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OP_WidthBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_HeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_DepthBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DimensionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OP_WeightUQBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OP_CubicUQBoundDropDownEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OP_WeightBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_CubicBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ProductDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OP_IsBarcodedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OP_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommodityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OP_CountDecimalPlacesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OP_StockKeepingUnitBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OP_PartNumBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OP_DescBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel = new CargoWise.Windows.UI.KPanel();
			this.NMFCTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGroupBox5 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FN_CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ArticleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FN_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FN_ClassTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BOMTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillOfMaterialsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.BillOfMaterialsViewGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Splitter2 = new CargoWise.Windows.UI.KSplitter();
			this.SecondaryProductsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecondaryProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ComponentUsageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BOMTopPanel = new CargoWise.Windows.UI.KPanel();
			this.CanResellCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CanDisassembleKitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoPrintAssemblyInstructionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsComponentPickedOnSalesOrder = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsAutoReplenish = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UNDGTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UNDGsGrid)).BeginInit();
			this.UNDGsGrid.SuspendLayout();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.processTemplateCustomFieldsControl1.SuspendLayout();
			this.AdditionalDetailsPanel.SuspendLayout();
			this.AdditionalProductDetailsGroupBox.SuspendLayout();
			this.StockControlGroupBox.SuspendLayout();
			this.CostCurrencyFindBox.SuspendLayout();
			this.ClientDefinedFieldsGroupBox.SuspendLayout();
			this.OP_OrderMultipleUnitBoundDropEdit.SuspendLayout();
			this.OP_F3_NKPackTypeBoundDropEdit.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.RelatedOrgsTabPage.SuspendLayout();
			this.relatedOrganizationsControl1.SuspendLayout();
			this.ProductUnitsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).BeginInit();
			this.PartUnitsGrid.SuspendLayout();
			this.ProductBarcodesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).BeginInit();
			this.PartBarcodesGrid.SuspendLayout();
			this.DimensionsWeightGroupBox.SuspendLayout();
			this.OP_MeasureUQBoundDropDownEdit.SuspendLayout();
			this.OP_WeightUQBoundDropDownEdit.SuspendLayout();
			this.OP_CubicUQBoundDropDownEdit.SuspendLayout();
			this.ProductDetailsGroupBox.SuspendLayout();
			this.CommodityCodeFindBox.SuspendLayout();
			this.OP_StockKeepingUnitBoundDropEdit.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.NMFCTabPage.SuspendLayout();
			this.zGroupBox5.SuspendLayout();
			this.FN_CodeCodeFindBox.SuspendLayout();
			this.ArticleGroupBox.SuspendLayout();
			this.BOMTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillOfMaterialsGrid)).BeginInit();
			this.BillOfMaterialsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillOfMaterialsViewGrid)).BeginInit();
			this.BillOfMaterialsViewGrid.SuspendLayout();
			this.SecondaryProductsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryProductsGrid)).BeginInit();
			this.SecondaryProductsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentUsageGrid)).BeginInit();
			this.ComponentUsageGrid.SuspendLayout();
			this.BOMTopPanel.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.NMFCTabPage);
			this.MainTabControl.Controls.Add(this.UNDGTabPage);
			this.MainTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.MainTabControl.Controls.Add(this.BOMTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 609, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.BOMTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AdditionalDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.UNDGTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NMFCTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.BottomTabControl);
			this.MainTabPage.Controls.Add(this.TopPanel);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 609, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(690);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierPart);
			// 
			// UNDGTabPage
			// 
			this.UNDGTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|fd187da6-8666-4a97-85f1-0a0322fea800", "UNDG");
			this.UNDGTabPage.Controls.Add(this.UNDGsGrid);
			this.UNDGTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.UNDGTabPage.Name = "UNDGTabPage";
			this.UNDGTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			this.UNDGTabPage.TabIndex = 4;
			// 
			// UNDGsGrid
			// 
			this.UNDGsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UNDGsGrid, "UNDGs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).SubstancePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).Subs.DG_PSN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).Subs.DG_PG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).Subs.DG_SubLabel1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).Subs.DG_SubLabel2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).Subs.DG_Class)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_DGFlashPoint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_OC_DGContact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_DGWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_UnitOfWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_DGVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_UnitOfVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_TechnicalName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).UNDGs)).SyncRoot)).DI_MPMarinePollutant)));
			this.UNDGsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SubstancePK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Subs+DG_PSN";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "Subs+DG_PG";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "Subs+DG_SubLabel1";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "Subs+DG_SubLabel2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.ColumnName = "Subs+DG_Class";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DI_DGFlashPoint";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DI_OC_DGContact";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "DI_DGWeight";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.ColumnName = "DI_UnitOfWeight";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "DI_DGVolume";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo2.ColumnName = "DI_UnitOfVolume";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo6.ColumnName = "DI_TechnicalName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo3.ColumnName = "DI_MPMarinePollutant";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UNDGsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.UNDGsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UNDGsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.UNDGsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.UNDGsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.UNDGsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.UNDGsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.UNDGsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UNDGsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.UNDGsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UNDGsGrid.GridId = "fbdd5e65-406d-42b5-802f-87de4b7f3a5d";
			this.UNDGsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UNDGsGrid.LayoutKey = "zGrid1";
			this.UNDGsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UNDGsGrid.Name = "UNDGsGrid";
			this.UNDGsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			this.UNDGsGrid.TabIndex = 0;
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|7994c12f-6bff-4610-9b33-d9c940bb0419", "Additional Details");
			this.AdditionalDetailsTabPage.Controls.Add(this.processTemplateCustomFieldsControl1);
			this.AdditionalDetailsTabPage.Controls.Add(this.AdditionalDetailsPanel);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			this.AdditionalDetailsTabPage.TabIndex = 5;
			// 
			// processTemplateCustomFieldsControl1
			// 
			this.processTemplateCustomFieldsControl1.AllowDrop = true;
			this.processTemplateCustomFieldsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 211, true);
			this.processTemplateCustomFieldsControl1.Name = "processTemplateCustomFieldsControl1";
			this.processTemplateCustomFieldsControl1.NothingSetupMessageLabelText = "To make use of this tab, please setup custom fields in Workflow Manager.";
			this.processTemplateCustomFieldsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 375, true);
			this.processTemplateCustomFieldsControl1.TabIndex = 1;
			// 
			// AdditionalDetailsPanel
			// 
			this.AdditionalDetailsPanel.Controls.Add(this.AdditionalProductDetailsGroupBox);
			this.AdditionalDetailsPanel.Controls.Add(this.StockControlGroupBox);
			this.AdditionalDetailsPanel.Controls.Add(this.ClientDefinedFieldsGroupBox);
			this.AdditionalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDetailsPanel.Name = "AdditionalDetailsPanel";
			this.AdditionalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 211, true);
			this.AdditionalDetailsPanel.TabIndex = 0;
			// 
			// AdditionalProductDetailsGroupBox
			// 
			this.AdditionalProductDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|9764f2b1-ee29-4cba-8cf5-dc9c43171996", "Additional Product Details");
			this.AdditionalProductDetailsGroupBox.Controls.Add(this.OP_ModelTextBox);
			this.AdditionalProductDetailsGroupBox.Controls.Add(this.OP_BrandTextBox);
			this.AdditionalProductDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 122, true);
			this.AdditionalProductDetailsGroupBox.Name = "AdditionalProductDetailsGroupBox";
			this.AdditionalProductDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 81, true);
			this.AdditionalProductDetailsGroupBox.TabIndex = 1;
			this.AdditionalProductDetailsGroupBox.TabStop = false;
			// 
			// OP_ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_ModelTextBox, "OP_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Model)));
			this.OP_ModelTextBox.CaptionResourceString = null;
			this.OP_ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 47, true);
			this.OP_ModelTextBox.Name = "OP_ModelTextBox";
			this.OP_ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 15, true);
			this.OP_ModelTextBox.TabIndex = 1;
			// 
			// OP_BrandTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_BrandTextBox, "OP_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Brand)));
			this.OP_BrandTextBox.CaptionResourceString = null;
			this.OP_BrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 23, true);
			this.OP_BrandTextBox.Name = "OP_BrandTextBox";
			this.OP_BrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 15, true);
			this.OP_BrandTextBox.TabIndex = 0;
			// 
			// StockControlGroupBox
			// 
			this.StockControlGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|49bff3a7-6ecd-4f79-8923-9350622b59f6", "Basic Stock Control");
			this.StockControlGroupBox.Controls.Add(this.OP_WeightedCostCalcEdit);
			this.StockControlGroupBox.Controls.Add(this.CostCurrencyFindBox);
			this.StockControlGroupBox.Controls.Add(this.OP_LastCostBoundCalcEdit);
			this.StockControlGroupBox.Controls.Add(this.OP_QtyInStockBoundCalcEdit);
			this.StockControlGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 4, true);
			this.StockControlGroupBox.Name = "StockControlGroupBox";
			this.StockControlGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 112, true);
			this.StockControlGroupBox.TabIndex = 2;
			this.StockControlGroupBox.TabStop = false;
			// 
			// OP_WeightedCostCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_WeightedCostCalcEdit, "OP_WeightedCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_WeightedCost)));
			this.OP_WeightedCostCalcEdit.DecimalPlaces = 4;
			this.OP_WeightedCostCalcEdit.Decimals = 4;
			this.OP_WeightedCostCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 17, true);
			this.OP_WeightedCostCalcEdit.Name = "OP_WeightedCostCalcEdit";
			this.OP_WeightedCostCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_WeightedCostCalcEdit.TabIndex = 1;
			this.OP_WeightedCostCalcEdit.Text = "0.0000";
			this.OP_WeightedCostCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CostCurrencyFindBox
			// 
			this.CostCurrencyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CostCurrencyFindBox, "OP_RX_NKLastWeightedCostCurr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_RX_NKLastWeightedCostCurr)));
			this.CostCurrencyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 63, true);
			this.CostCurrencyFindBox.Name = "CostCurrencyFindBox";
			this.CostCurrencyFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CostCurrencyFindBox.ParentType = null;
			this.CostCurrencyFindBox.PreBoundMaxLength = 4;
			this.CostCurrencyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 15, true);
			this.CostCurrencyFindBox.TabIndex = 3;
			// 
			// OP_LastCostBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_LastCostBoundCalcEdit, "OP_LastCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_LastCost)));
			this.OP_LastCostBoundCalcEdit.DecimalPlaces = 4;
			this.OP_LastCostBoundCalcEdit.Decimals = 4;
			this.OP_LastCostBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 40, true);
			this.OP_LastCostBoundCalcEdit.Name = "OP_LastCostBoundCalcEdit";
			this.OP_LastCostBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_LastCostBoundCalcEdit.TabIndex = 2;
			this.OP_LastCostBoundCalcEdit.Text = "0.0000";
			this.OP_LastCostBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_QtyInStockBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_QtyInStockBoundCalcEdit, "OP_QtyInStock");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_QtyInStock)));
			this.OP_QtyInStockBoundCalcEdit.DecimalPlaces = 2;
			this.OP_QtyInStockBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 86, true);
			this.OP_QtyInStockBoundCalcEdit.Name = "OP_QtyInStockBoundCalcEdit";
			this.OP_QtyInStockBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_QtyInStockBoundCalcEdit.TabIndex = 4;
			this.OP_QtyInStockBoundCalcEdit.Text = "0.00";
			this.OP_QtyInStockBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClientDefinedFieldsGroupBox
			// 
			this.ClientDefinedFieldsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|057ea578-faa3-435f-8938-139edb391bea", "Client Defined Fields");
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_OrderMultipleUnitBoundDropEdit);
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_F3_NKPackTypeBoundDropEdit);
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_DepartmentBoundTextBox);
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_VendorPackQtyBoundCalcEdit);
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_DivisionBoundTextBox);
			this.ClientDefinedFieldsGroupBox.Controls.Add(this.OP_OrderMultipleQtyBoundCalcEdit);
			this.ClientDefinedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClientDefinedFieldsGroupBox.Name = "ClientDefinedFieldsGroupBox";
			this.ClientDefinedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 113, true);
			this.ClientDefinedFieldsGroupBox.TabIndex = 0;
			this.ClientDefinedFieldsGroupBox.TabStop = false;
			// 
			// OP_OrderMultipleUnitBoundDropEdit
			// 
			this.OP_OrderMultipleUnitBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_OrderMultipleUnitBoundDropEdit, "OP_OrderMultipleUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_OrderMultipleUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OP_OrderMultipleUnitBoundDropEdit, false);
			this.OP_OrderMultipleUnitBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 18, true);
			this.OP_OrderMultipleUnitBoundDropEdit.Name = "OP_OrderMultipleUnitBoundDropEdit";
			this.OP_OrderMultipleUnitBoundDropEdit.PreBoundMaxLength = 2;
			this.OP_OrderMultipleUnitBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 15, true);
			this.OP_OrderMultipleUnitBoundDropEdit.TabIndex = 1;
			// 
			// OP_F3_NKPackTypeBoundDropEdit
			// 
			this.OP_F3_NKPackTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_F3_NKPackTypeBoundDropEdit, "OP_F3_NKPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_F3_NKPackType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OP_F3_NKPackTypeBoundDropEdit, false);
			this.OP_F3_NKPackTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 41, true);
			this.OP_F3_NKPackTypeBoundDropEdit.Name = "OP_F3_NKPackTypeBoundDropEdit";
			this.OP_F3_NKPackTypeBoundDropEdit.PreBoundMaxLength = 2;
			this.OP_F3_NKPackTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 15, true);
			this.OP_F3_NKPackTypeBoundDropEdit.TabIndex = 3;
			// 
			// OP_DepartmentBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_DepartmentBoundTextBox, "OP_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Department)));
			this.OP_DepartmentBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64, true);
			this.OP_DepartmentBoundTextBox.Name = "OP_DepartmentBoundTextBox";
			this.OP_DepartmentBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 15, true);
			this.OP_DepartmentBoundTextBox.TabIndex = 4;
			// 
			// OP_VendorPackQtyBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_VendorPackQtyBoundCalcEdit, "OP_VendorPackQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_VendorPackQty)));
			this.OP_VendorPackQtyBoundCalcEdit.DecimalPlaces = 3;
			this.OP_VendorPackQtyBoundCalcEdit.Decimals = 3;
			this.OP_VendorPackQtyBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 41, true);
			this.OP_VendorPackQtyBoundCalcEdit.Name = "OP_VendorPackQtyBoundCalcEdit";
			this.OP_VendorPackQtyBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.OP_VendorPackQtyBoundCalcEdit.TabIndex = 2;
			this.OP_VendorPackQtyBoundCalcEdit.Text = "0.000";
			this.OP_VendorPackQtyBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_DivisionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_DivisionBoundTextBox, "OP_Division");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Division)));
			this.OP_DivisionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 87, true);
			this.OP_DivisionBoundTextBox.Name = "OP_DivisionBoundTextBox";
			this.OP_DivisionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 15, true);
			this.OP_DivisionBoundTextBox.TabIndex = 5;
			// 
			// OP_OrderMultipleQtyBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_OrderMultipleQtyBoundCalcEdit, "OP_OrderMultipleQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_OrderMultipleQty)));
			this.OP_OrderMultipleQtyBoundCalcEdit.DecimalPlaces = 3;
			this.OP_OrderMultipleQtyBoundCalcEdit.Decimals = 3;
			this.OP_OrderMultipleQtyBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 18, true);
			this.OP_OrderMultipleQtyBoundCalcEdit.Name = "OP_OrderMultipleQtyBoundCalcEdit";
			this.OP_OrderMultipleQtyBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.OP_OrderMultipleQtyBoundCalcEdit.TabIndex = 0;
			this.OP_OrderMultipleQtyBoundCalcEdit.Text = "0.000";
			this.OP_OrderMultipleQtyBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BottomTabControl
			// 
			this.BottomTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BottomTabControl.Controls.Add(this.RelatedOrgsTabPage);
			this.BottomTabControl.Controls.Add(this.ProductUnitsTabPage);
			this.BottomTabControl.Controls.Add(this.ProductBarcodesTabPage);
			this.BottomTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 413, true);
			this.BottomTabControl.TabIndex = 1;
			// 
			// RelatedOrgsTabPage
			// 
			this.RelatedOrgsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|bdf20e22-6643-4d81-9a87-9cab31eb99ff", "Related Organizations");
			this.RelatedOrgsTabPage.Controls.Add(this.relatedOrganizationsControl1);
			this.RelatedOrgsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.RelatedOrgsTabPage.Name = "RelatedOrgsTabPage";
			this.RelatedOrgsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedOrgsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 390, true);
			this.RelatedOrgsTabPage.TabIndex = 0;
			// 
			// relatedOrganizationsControl1
			// 
			this.relatedOrganizationsControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedOrganizationsControl1, ".");
			this.relatedOrganizationsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedOrganizationsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.relatedOrganizationsControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 310, true);
			this.relatedOrganizationsControl1.Name = "relatedOrganizationsControl1";
			this.relatedOrganizationsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 384, true);
			this.relatedOrganizationsControl1.TabIndex = 0;
			// 
			// ProductUnitsTabPage
			// 
			this.ProductUnitsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|02890d9f-1eb6-43dc-9be0-edfce96c5535", "Unit Conversions");
			this.ProductUnitsTabPage.Controls.Add(this.PartUnitsGrid);
			this.ProductUnitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ProductUnitsTabPage.Name = "ProductUnitsTabPage";
			this.ProductUnitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductUnitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 390, true);
			this.ProductUnitsTabPage.TabIndex = 1;
			// 
			// PartUnitsGrid
			// 
			this.PartUnitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PartUnitsGrid, "PartUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_QuantityInParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_ParentPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_Depth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartUnit)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartUnits)).SyncRoot)).OF_Cubic)));
			this.PartUnitsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "OF_QuantityInParent";
			zCalcEditColumnStyleInfo13.Decimals = 6;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|6a2a430c-15c3-49bd-90c7-045355124631", "Package");
			zDropEditColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo9.ColumnName = "OF_PackType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|b6526e67-0cae-4c5c-9a37-1dcdba0e7c23", "Parent Package");
			zDropEditColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo10.ColumnName = "OF_ParentPackType";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "OF_Weight";
			zCalcEditColumnStyleInfo14.Decimals = 3;
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.ColumnName = "OF_Height";
			zCalcEditColumnStyleInfo15.Decimals = 3;
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.ColumnName = "OF_Width";
			zCalcEditColumnStyleInfo16.Decimals = 3;
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zCalcEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo17.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo17.ColumnName = "OF_Depth";
			zCalcEditColumnStyleInfo17.Decimals = 3;
			zCalcEditColumnStyleInfo17.IsVisible = false;
			zCalcEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo18.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo18.ColumnName = "OF_Cubic";
			zCalcEditColumnStyleInfo18.Decimals = 3;
			zCalcEditColumnStyleInfo18.IsVisible = false;
			zCalcEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.PartUnitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.PartUnitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo17);
			this.PartUnitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo18);
			this.PartUnitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartUnitsGrid.GridId = "30fd36f8-c07a-4e61-8a33-72d0908f0713";
			this.PartUnitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartUnitsGrid.LayoutKey = "zGrid1";
			this.PartUnitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PartUnitsGrid.Name = "PartUnitsGrid";
			this.PartUnitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 384, true);
			this.PartUnitsGrid.TabIndex = 3;
			// 
			// ProductBarcodesTabPage
			// 
			this.ProductBarcodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|2fc092ab-561a-43ce-bb77-0e2c2bae544c", "Barcodes");
			this.ProductBarcodesTabPage.Controls.Add(this.PartBarcodesGrid);
			this.ProductBarcodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ProductBarcodesTabPage.Name = "ProductBarcodesTabPage";
			this.ProductBarcodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductBarcodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 390, true);
			this.ProductBarcodesTabPage.TabIndex = 2;
			// 
			// PartBarcodesGrid
			// 
			this.PartBarcodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PartBarcodesGrid, "PartBarcodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartBarcodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPartBarcode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartBarcodes)).SyncRoot)).PH_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPartBarcode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartBarcodes)).SyncRoot)).PH_Barcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPartBarcode)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).PartBarcodes)).SyncRoot)).PH_UseForDocuments)));
			this.PartBarcodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo11.ColumnName = "PH_F3_NKPackType";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.ColumnName = "PH_Barcode";
			zTextBoxColumnStyleInfo17.IsMandatory = true;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "PH_UseForDocuments";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PartBarcodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.PartBarcodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.PartBarcodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.PartBarcodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartBarcodesGrid.GridId = "55314d1a-b4ad-4a9f-82fd-d8ed1a4b042c";
			this.PartBarcodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PartBarcodesGrid.LayoutKey = "zGrid1";
			this.PartBarcodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PartBarcodesGrid.Name = "PartBarcodesGrid";
			this.PartBarcodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1120, 384, true);
			this.PartBarcodesGrid.TabIndex = 4;
			// 
			// DimensionsWeightGroupBox
			// 
			this.DimensionsWeightGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|2944d7d7-566a-4a27-bb46-215c3023d6ee", "Dimensions and Weight");
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_KeepUprightCheckBox);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_NetWeightBoundCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.PerPalletLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_StockKeepingUnitPerPalletCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_MeasureUQBoundDropDownEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.CubePerLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.WeightPerLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.CubeUnitLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.WeightUnitLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_WidthBoundCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_HeightBoundCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_DepthBoundCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.DimensionLabel);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_WeightUQBoundDropDownEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_CubicUQBoundDropDownEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_WeightBoundCalcEdit);
			this.DimensionsWeightGroupBox.Controls.Add(this.OP_CubicBoundCalcEdit);
			this.DimensionsWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 4, true);
			this.DimensionsWeightGroupBox.Name = "DimensionsWeightGroupBox";
			this.DimensionsWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 163, true);
			this.DimensionsWeightGroupBox.TabIndex = 1;
			this.DimensionsWeightGroupBox.TabStop = false;
			// 
			// OP_KeepUprightCheckBox
			// 
			this.OP_KeepUprightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OP_KeepUprightCheckBox, "OP_KeepUpright");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_KeepUpright)));
			this.OP_KeepUprightCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("618f0381-7111-4632-b76a-6f6764d02d8b", "Keep Upright");
			this.OP_KeepUprightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 41, true);
			this.OP_KeepUprightCheckBox.Name = "OP_KeepUprightCheckBox";
			this.OP_KeepUprightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 14, true);
			this.OP_KeepUprightCheckBox.TabIndex = 16;
			this.OP_KeepUprightCheckBox.UseVisualStyleBackColor = true;
			// 
			// OP_NetWeightBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_NetWeightBoundCalcEdit, "OP_NetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_NetWeight)));
			this.OP_NetWeightBoundCalcEdit.DecimalPlaces = 3;
			this.OP_NetWeightBoundCalcEdit.Decimals = 3;
			this.OP_NetWeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 87, true);
			this.OP_NetWeightBoundCalcEdit.Name = "OP_NetWeightBoundCalcEdit";
			this.OP_NetWeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_NetWeightBoundCalcEdit.TabIndex = 10;
			this.OP_NetWeightBoundCalcEdit.Text = "0.000";
			this.OP_NetWeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PerPalletLabel
			// 
			this.BindingSource.SetBindingMember(this.PerPalletLabel, "OP_StockKeepingUnitForPalletProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_StockKeepingUnitForPalletProxy)));
			this.PerPalletLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PerPalletLabel, false);
			this.PerPalletLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 134, true);
			this.PerPalletLabel.Name = "PerPalletLabel";
			this.PerPalletLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.PerPalletLabel.TabIndex = 0;
			// 
			// OP_StockKeepingUnitPerPalletCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_StockKeepingUnitPerPalletCalcEdit, "OP_StockKeepingUnitPerPallet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_StockKeepingUnitPerPallet)));
			this.OP_StockKeepingUnitPerPalletCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|c14ad640-2cf2-4fd0-b263-955ee532047e", "Pallet Size", "Pallet Size", "");
			this.OP_StockKeepingUnitPerPalletCalcEdit.DecimalPlaces = 2;
			this.OP_StockKeepingUnitPerPalletCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 135, true);
			this.OP_StockKeepingUnitPerPalletCalcEdit.Name = "OP_StockKeepingUnitPerPalletCalcEdit";
			this.OP_StockKeepingUnitPerPalletCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
			this.OP_StockKeepingUnitPerPalletCalcEdit.TabIndex = 15;
			this.OP_StockKeepingUnitPerPalletCalcEdit.Text = "0.00";
			this.OP_StockKeepingUnitPerPalletCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_MeasureUQBoundDropDownEdit
			// 
			this.OP_MeasureUQBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_MeasureUQBoundDropDownEdit, "OP_MeasureUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_MeasureUQ)));
			this.OP_MeasureUQBoundDropDownEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|803488f7-04a4-42e1-b9c8-3e6bf5f15863", "Unit", "Dimension unit of quantity.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OP_MeasureUQBoundDropDownEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OP_MeasureUQBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 39, true);
			this.OP_MeasureUQBoundDropDownEdit.Name = "OP_MeasureUQBoundDropDownEdit";
			this.OP_MeasureUQBoundDropDownEdit.PreBoundMaxLength = 2;
			this.OP_MeasureUQBoundDropDownEdit.ShowDescriptionBox = false;
			this.OP_MeasureUQBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
			this.OP_MeasureUQBoundDropDownEdit.TabIndex = 4;
			// 
			// CubePerLabel
			// 
			this.CubePerLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|f7467f81-9e82-4f38-8be6-f5c772411578", "per");
			this.CubePerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CubePerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 113, true);
			this.CubePerLabel.Name = "CubePerLabel";
			this.CubePerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 16, true);
			this.CubePerLabel.TabIndex = 13;
			// 
			// WeightPerLabel
			// 
			this.WeightPerLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|cc7946d1-1b4e-4ea0-9e1f-3e381158a2a7", "per");
			this.WeightPerLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeightPerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 64, true);
			this.WeightPerLabel.Name = "WeightPerLabel";
			this.WeightPerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 16, true);
			this.WeightPerLabel.TabIndex = 8;
			// 
			// CubeUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.CubeUnitLabel, "StockKeepingUnitProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).StockKeepingUnitProxy)));
			this.CubeUnitLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|2e06f46e-a02e-4e55-9e9d-3032674e4677", "Bag");
			this.CubeUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CubeUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 113, true);
			this.CubeUnitLabel.Name = "CubeUnitLabel";
			this.CubeUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.CubeUnitLabel.TabIndex = 14;
			this.CubeUnitLabel.TextChanged += new System.EventHandler(this.CubeUnitLabel_TextChanged);
			// 
			// WeightUnitLabel
			// 
			this.BindingSource.SetBindingMember(this.WeightUnitLabel, "StockKeepingUnitProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).StockKeepingUnitProxy)));
			this.WeightUnitLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|a222379c-db31-4cd1-8da4-cc3e86dc9c85", "Carton");
			this.WeightUnitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WeightUnitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 64, true);
			this.WeightUnitLabel.Name = "WeightUnitLabel";
			this.WeightUnitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.WeightUnitLabel.TabIndex = 9;
			this.WeightUnitLabel.TextChanged += new System.EventHandler(this.WeightUnitLabel_TextChanged);
			// 
			// OP_WidthBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_WidthBoundCalcEdit, "OP_Width");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Width)));
			this.OP_WidthBoundCalcEdit.DecimalPlaces = 3;
			this.OP_WidthBoundCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OP_WidthBoundCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OP_WidthBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 39, true);
			this.OP_WidthBoundCalcEdit.Name = "OP_WidthBoundCalcEdit";
			this.OP_WidthBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
			this.OP_WidthBoundCalcEdit.TabIndex = 2;
			this.OP_WidthBoundCalcEdit.Text = "0.000";
			this.OP_WidthBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_HeightBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_HeightBoundCalcEdit, "OP_Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Height)));
			this.OP_HeightBoundCalcEdit.DecimalPlaces = 3;
			this.OP_HeightBoundCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OP_HeightBoundCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OP_HeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 39, true);
			this.OP_HeightBoundCalcEdit.Name = "OP_HeightBoundCalcEdit";
			this.OP_HeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 15, true);
			this.OP_HeightBoundCalcEdit.TabIndex = 3;
			this.OP_HeightBoundCalcEdit.Text = "0.000";
			this.OP_HeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_DepthBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_DepthBoundCalcEdit, "OP_Depth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Depth)));
			this.OP_DepthBoundCalcEdit.DecimalPlaces = 3;
			this.OP_DepthBoundCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OP_DepthBoundCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OP_DepthBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 39, true);
			this.OP_DepthBoundCalcEdit.Name = "OP_DepthBoundCalcEdit";
			this.OP_DepthBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.OP_DepthBoundCalcEdit.TabIndex = 1;
			this.OP_DepthBoundCalcEdit.Text = "0.000";
			this.OP_DepthBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DimensionLabel
			// 
			this.DimensionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|4fd4cf31-81af-4eba-9e60-e22a860a064b", "Dimensions");
			this.DimensionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DimensionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 41, true);
			this.DimensionLabel.Name = "DimensionLabel";
			this.DimensionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.DimensionLabel.TabIndex = 0;
			this.DimensionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// OP_WeightUQBoundDropDownEdit
			// 
			this.OP_WeightUQBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_WeightUQBoundDropDownEdit, "OP_WeightUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_WeightUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OP_WeightUQBoundDropDownEdit, false);
			this.OP_WeightUQBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 63, true);
			this.OP_WeightUQBoundDropDownEdit.Name = "OP_WeightUQBoundDropDownEdit";
			this.OP_WeightUQBoundDropDownEdit.PreBoundMaxLength = 2;
			this.OP_WeightUQBoundDropDownEdit.ShowDescriptionBox = false;
			this.OP_WeightUQBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
			this.OP_WeightUQBoundDropDownEdit.TabIndex = 7;
			// 
			// OP_CubicUQBoundDropDownEdit
			// 
			this.OP_CubicUQBoundDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_CubicUQBoundDropDownEdit, "OP_CubicUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_CubicUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OP_CubicUQBoundDropDownEdit, false);
			this.OP_CubicUQBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 111, true);
			this.OP_CubicUQBoundDropDownEdit.Name = "OP_CubicUQBoundDropDownEdit";
			this.OP_CubicUQBoundDropDownEdit.PreBoundMaxLength = 2;
			this.OP_CubicUQBoundDropDownEdit.ShowDescriptionBox = false;
			this.OP_CubicUQBoundDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
			this.OP_CubicUQBoundDropDownEdit.TabIndex = 12;
			// 
			// OP_WeightBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_WeightBoundCalcEdit, "OP_Weight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Weight)));
			this.OP_WeightBoundCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|34f83b02-ecf0-4d42-b40a-a488b8d020fc", "Gross Weight", "Weight measurement of this part per Stock Unit. This value will be added to Product Unit tab and used for unit conversions if this product is entered in invoice lines.");
			this.OP_WeightBoundCalcEdit.DecimalPlaces = 3;
			this.OP_WeightBoundCalcEdit.Decimals = 3;
			this.OP_WeightBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 63, true);
			this.OP_WeightBoundCalcEdit.Name = "OP_WeightBoundCalcEdit";
			this.OP_WeightBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_WeightBoundCalcEdit.TabIndex = 6;
			this.OP_WeightBoundCalcEdit.Text = "0.000";
			this.OP_WeightBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_CubicBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_CubicBoundCalcEdit, "OP_Cubic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Cubic)));
			this.OP_CubicBoundCalcEdit.DecimalPlaces = 3;
			this.OP_CubicBoundCalcEdit.Decimals = 3;
			this.OP_CubicBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 111, true);
			this.OP_CubicBoundCalcEdit.Name = "OP_CubicBoundCalcEdit";
			this.OP_CubicBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.OP_CubicBoundCalcEdit.TabIndex = 11;
			this.OP_CubicBoundCalcEdit.Text = "0.000";
			this.OP_CubicBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductDetailsGroupBox
			// 
			this.ProductDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|51f6f390-5a1c-4d5b-b75a-99ec9638e5f9", "Product Details");
			this.ProductDetailsGroupBox.Controls.Add(this.OP_IsBarcodedCheckBox);
			this.ProductDetailsGroupBox.Controls.Add(this.OP_IsActiveCheckBox);
			this.ProductDetailsGroupBox.Controls.Add(this.CommodityCodeFindBox);
			this.ProductDetailsGroupBox.Controls.Add(this.OP_CountDecimalPlacesCalcEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.OP_StockKeepingUnitBoundDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.OP_PartNumBoundTextBox);
			this.ProductDetailsGroupBox.Controls.Add(this.OP_DescBoundTextBox);
			this.ProductDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProductDetailsGroupBox.Name = "ProductDetailsGroupBox";
			this.ProductDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 164, true);
			this.ProductDetailsGroupBox.TabIndex = 0;
			this.ProductDetailsGroupBox.TabStop = false;
			// 
			// OP_IsBarcodedCheckBox
			// 
			this.OP_IsBarcodedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OP_IsBarcodedCheckBox, "OP_IsBarcoded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_IsBarcoded)));
			this.OP_IsBarcodedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 14, true);
			this.OP_IsBarcodedCheckBox.Name = "OP_IsBarcodedCheckBox";
			this.OP_IsBarcodedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 14, true);
			this.OP_IsBarcodedCheckBox.TabIndex = 1;
			this.OP_IsBarcodedCheckBox.UseVisualStyleBackColor = true;
			// 
			// OP_IsActiveCheckBox
			// 
			this.OP_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OP_IsActiveCheckBox, "OP_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_IsActive)));
			this.OP_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 141, true);
			this.OP_IsActiveCheckBox.Name = "OP_IsActiveCheckBox";
			this.OP_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 14, true);
			this.OP_IsActiveCheckBox.TabIndex = 6;
			this.OP_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// CommodityCodeFindBox
			// 
			this.CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeFindBox, "OP_RH_NKCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_RH_NKCommodityCode)));
			this.CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 89, true);
			this.CommodityCodeFindBox.Name = "CommodityCodeFindBox";
			this.CommodityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CommodityCodeFindBox.ParentType = null;
			this.CommodityCodeFindBox.PreBoundMaxLength = 4;
			this.CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 15, true);
			this.CommodityCodeFindBox.TabIndex = 3;
			// 
			// OP_CountDecimalPlacesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OP_CountDecimalPlacesCalcEdit, "OP_CountDecimalPlaces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_CountDecimalPlaces)));
			this.OP_CountDecimalPlacesCalcEdit.DecimalPlaces = 0;
			this.OP_CountDecimalPlacesCalcEdit.Decimals = 0;
			this.OP_CountDecimalPlacesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 137, true);
			this.OP_CountDecimalPlacesCalcEdit.Name = "OP_CountDecimalPlacesCalcEdit";
			this.OP_CountDecimalPlacesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 15, true);
			this.OP_CountDecimalPlacesCalcEdit.TabIndex = 5;
			this.OP_CountDecimalPlacesCalcEdit.Text = "0";
			this.OP_CountDecimalPlacesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OP_StockKeepingUnitBoundDropEdit
			// 
			this.OP_StockKeepingUnitBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OP_StockKeepingUnitBoundDropEdit, "OP_StockKeepingUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_StockKeepingUnit)));
			this.OP_StockKeepingUnitBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|85ad397c-6503-46bb-b054-c0d563228ec4", "Stock Unit", "Stock Keeping Unit.");
			this.OP_StockKeepingUnitBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 113, true);
			this.OP_StockKeepingUnitBoundDropEdit.Name = "OP_StockKeepingUnitBoundDropEdit";
			this.OP_StockKeepingUnitBoundDropEdit.PreBoundMaxLength = 4;
			this.OP_StockKeepingUnitBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 15, true);
			this.OP_StockKeepingUnitBoundDropEdit.TabIndex = 4;
			// 
			// OP_PartNumBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_PartNumBoundTextBox, "OP_PartNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_PartNum)));
			this.OP_PartNumBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|3431d68e-1633-4bf6-b66e-06395f5c7f69", "Code", "Unique part number for an organization.");
			this.OP_PartNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 11, true);
			this.OP_PartNumBoundTextBox.Name = "OP_PartNumBoundTextBox";
			this.OP_PartNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 15, true);
			this.OP_PartNumBoundTextBox.TabIndex = 0;
			// 
			// OP_DescBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OP_DescBoundTextBox, "OP_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_Desc)));
			this.OP_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 35, true);
			this.OP_DescBoundTextBox.Multiline = true;
			this.OP_DescBoundTextBox.Name = "OP_DescBoundTextBox";
			this.OP_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 49, true);
			this.OP_DescBoundTextBox.TabIndex = 2;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ProductDetailsGroupBox);
			this.TopPanel.Controls.Add(this.DimensionsWeightGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 173, true);
			this.TopPanel.TabIndex = 0;
			// 
			// NMFCTabPage
			// 
			this.NMFCTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|d5fca0bc-608e-4e91-9185-d87e4a0cc5d9", "NMFC");
			this.NMFCTabPage.Controls.Add(this.zGroupBox5);
			this.NMFCTabPage.Controls.Add(this.ArticleGroupBox);
			this.NMFCTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NMFCTabPage.Name = "NMFCTabPage";
			this.NMFCTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			this.NMFCTabPage.TabIndex = 6;
			// 
			// zGroupBox5
			// 
			this.zGroupBox5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|2f292bbc-bb7b-4f83-a9d6-39128ed8c96d", "NMFC");
			this.zGroupBox5.Controls.Add(this.FN_CodeCodeFindBox);
			this.zGroupBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox5.Name = "zGroupBox5";
			this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 49, true);
			this.zGroupBox5.TabIndex = 0;
			this.zGroupBox5.TabStop = false;
			// 
			// FN_CodeCodeFindBox
			// 
			this.FN_CodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FN_CodeCodeFindBox, "CommodityCode+RH_FN_NKNMFC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).CommodityCode.RH_FN_NKNMFC)));
			this.FN_CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 18, true);
			this.FN_CodeCodeFindBox.Name = "FN_CodeCodeFindBox";
			this.FN_CodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FN_CodeCodeFindBox.ParentType = null;
			this.FN_CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.FN_CodeCodeFindBox.TabIndex = 1;
			// 
			// ArticleGroupBox
			// 
			this.ArticleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|e61d9da8-0eac-4e26-9541-ff5f1574c28e", "Article Details");
			this.ArticleGroupBox.Controls.Add(this.ItemNoTextBox);
			this.ArticleGroupBox.Controls.Add(this.FN_DescriptionTextBox);
			this.ArticleGroupBox.Controls.Add(this.FN_ClassTextBox);
			this.ArticleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 58, true);
			this.ArticleGroupBox.Name = "ArticleGroupBox";
			this.ArticleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 259, true);
			this.ArticleGroupBox.TabIndex = 1;
			this.ArticleGroupBox.TabStop = false;
			// 
			// ItemNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemNoTextBox, "CommodityCode+NMFC+FN_ItemNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).CommodityCode.NMFC.FN_ItemNo)));
			this.ItemNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 29, true);
			this.ItemNoTextBox.Name = "ItemNoTextBox";
			this.ItemNoTextBox.ReadOnly = true;
			this.ItemNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.ItemNoTextBox.TabIndex = 1;
			// 
			// FN_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_DescriptionTextBox, "CommodityCode+NMFC+FN_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).CommodityCode.NMFC.FN_Description)));
			this.FN_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 81, true);
			this.FN_DescriptionTextBox.Multiline = true;
			this.FN_DescriptionTextBox.Name = "FN_DescriptionTextBox";
			this.FN_DescriptionTextBox.ReadOnly = true;
			this.FN_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 172, true);
			this.FN_DescriptionTextBox.TabIndex = 5;
			// 
			// FN_ClassTextBox
			// 
			this.BindingSource.SetBindingMember(this.FN_ClassTextBox, "CommodityCode+NMFC+FN_Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).CommodityCode.NMFC.FN_Class)));
			this.FN_ClassTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 55, true);
			this.FN_ClassTextBox.Name = "FN_ClassTextBox";
			this.FN_ClassTextBox.ReadOnly = true;
			this.FN_ClassTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.FN_ClassTextBox.TabIndex = 3;
			// 
			// BOMTabPage
			// 
			this.BOMTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|5063a813-1e62-495d-a4d5-9fbef709e85d", "BOM");
			this.BOMTabPage.Controls.Add(this.BillOfMaterialsGrid);
			this.BOMTabPage.Controls.Add(this.Splitter1);
			this.BOMTabPage.Controls.Add(this.BillOfMaterialsViewGrid);
			this.BOMTabPage.Controls.Add(this.Splitter2);
			this.BOMTabPage.Controls.Add(this.SecondaryProductsGroupBox);
			this.BOMTabPage.Controls.Add(this.BOMTopPanel);
			this.BOMTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.BOMTabPage.Name = "BOMTabPage";
			this.BOMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 586, true);
			this.BOMTabPage.TabIndex = 7;
			// 
			// BillOfMaterialsGrid
			// 
			this.BillOfMaterialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillOfMaterialsGrid, "BillOfMaterials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).ChildrenBomIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).OE_OP_Component)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).ComponentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).OE_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).OE_ComponentQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).OE_CanReuse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterials)).SyncRoot)).OE_ExcludeForVirtualWarehouse)));
			this.BillOfMaterialsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|ac1f14de-b8e2-40bb-9cdf-3ca4364c7c68", "Has Children");
			zTextBoxColumnStyleInfo7.ColumnName = "ChildrenBomIndicator";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "OE_OP_Component";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.ColumnName = "ComponentDescription";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zDropEditColumnStyleInfo4.ColumnName = "OE_F3_NKPackType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "OE_ComponentQty";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "OE_CanReuse";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b1a60858-6b36-49b7-8ced-9ad79fd91bfa", "Exclude For Virtual Warehouse");
			zCheckBoxColumnStyleInfo2.ColumnName = "OE_ExcludeForVirtualWarehouse";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BillOfMaterialsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.BillOfMaterialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillOfMaterialsGrid.GridId = "9321fb1c-449c-42f9-be98-5e4e9c7d1f74";
			this.BillOfMaterialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillOfMaterialsGrid.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.BillOfMaterialsGrid.LayoutKey = "zGrid2";
			this.BillOfMaterialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 41, true);
			this.BillOfMaterialsGrid.Name = "BillOfMaterialsGrid";
			this.BillOfMaterialsGrid.ShouldSetErrorsOnTabPage = false;
			this.BillOfMaterialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 249, true);
			this.BillOfMaterialsGrid.TabIndex = 0;
			this.BillOfMaterialsGrid.AfterBind += new System.EventHandler(this.BillOfMaterialsGrid_AfterBind);
			this.BillOfMaterialsGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(this.BillOfMaterialsGrid_RowDeleting);
			// 
			// Splitter1
			// 
			this.Splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.Splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter1.DoNotSaveSplitterLayout = false;
			this.Splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.Splitter1.Name = "Splitter1";
			this.Splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 4, true);
			this.Splitter1.TabIndex = 3;
			this.Splitter1.TabStop = false;
			// 
			// BillOfMaterialsViewGrid
			// 
			this.BillOfMaterialsViewGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillOfMaterialsViewGrid, "BillOfMaterialsView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBomView)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)).SyncRoot)).ComponentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBomView)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)).SyncRoot)).ComponentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartBomView)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)).SyncRoot)).ComponentPack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartBomView)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)).SyncRoot)).ComponentQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartBomView)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).BillOfMaterialsView)).SyncRoot)).BOMLevel)));
			this.BillOfMaterialsViewGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|0500787d-669a-41b0-b415-bbe0a483f0fb", "Component Part");
			zTextBoxColumnStyleInfo10.ColumnName = "ComponentCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|2777a5c0-aacf-4413-b986-4cd040bd4a39", "Description", "Description", "");
			zTextBoxColumnStyleInfo11.ColumnName = "ComponentDescription";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|ab5a326b-6738-4199-bf4c-3c4b7181f644", "Stock Unit", "Stock Unit", "");
			zTextBoxColumnStyleInfo12.ColumnName = "ComponentPack";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|690f5979-3f98-4402-85e5-6d111b4aae22", "Quantity Per", "Quantity Per", "");
			zCalcEditColumnStyleInfo7.ColumnName = "ComponentQty";
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|161a1afb-2fad-412c-bcb8-34d88a552529", "Level", "Level", "");
			zCalcEditColumnStyleInfo8.ColumnName = "BOMLevel";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.BillOfMaterialsViewGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.BillOfMaterialsViewGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.BillOfMaterialsViewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.BillOfMaterialsViewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.BillOfMaterialsViewGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.BillOfMaterialsViewGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BillOfMaterialsViewGrid.GridId = "667102bb-df3e-4f9f-9c94-73883380e9a1";
			this.BillOfMaterialsViewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillOfMaterialsViewGrid.LayoutKey = "BillOfMaterialsViewGrid";
			this.BillOfMaterialsViewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 294, true);
			this.BillOfMaterialsViewGrid.Name = "BillOfMaterialsViewGrid";
			this.BillOfMaterialsViewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 143, true);
			this.BillOfMaterialsViewGrid.TabIndex = 2;
			// 
			// Splitter2
			// 
			this.Splitter2.BackColor = System.Drawing.SystemColors.ControlLight;
			this.Splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter2.DoNotSaveSplitterLayout = false;
			this.Splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 437, true);
			this.Splitter2.Name = "Splitter2";
			this.Splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 4, true);
			this.Splitter2.TabIndex = 3;
			this.Splitter2.TabStop = false;
			// 
			// SecondaryProductsGroupBox
			// 
			this.SecondaryProductsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("46bae28f-bbb4-4e34-9e4c-cab9fe4c2c9f", "Inward Processing Secondary Products");
			this.SecondaryProductsGroupBox.Controls.Add(this.SecondaryProductsGrid);
			this.SecondaryProductsGroupBox.Controls.Add(this.ComponentUsageGrid);
			this.SecondaryProductsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SecondaryProductsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 441, true);
			this.SecondaryProductsGroupBox.Name = "SecondaryProductsGroupBox";
			this.SecondaryProductsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 145, true);
			this.SecondaryProductsGroupBox.TabIndex = 8;
			this.SecondaryProductsGroupBox.TabStop = false;
			// 
			// SecondaryProductsGrid
			// 
			this.SecondaryProductsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SecondaryProductsGrid, "SecondaryParts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).OSB_OP_SecondaryProduct)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).SecondaryProduct.OP_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).OSB_ProductQuantity)));
			this.SecondaryProductsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo5.ColumnName = "OSB_OP_SecondaryProduct";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.ColumnName = "SecondaryProduct+OP_Desc";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "OSB_ProductQuantity";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.SecondaryProductsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.SecondaryProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.SecondaryProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.SecondaryProductsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecondaryProductsGrid.GridId = "9321fb1c-449c-42f9-be98-5e4e9c7d1f74";
			this.SecondaryProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SecondaryProductsGrid.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.SecondaryProductsGrid.LayoutKey = "SecondaryProductsGrid";
			this.SecondaryProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.SecondaryProductsGrid.Name = "SecondaryProductsGrid";
			this.SecondaryProductsGrid.ShouldSetErrorsOnTabPage = false;
			this.SecondaryProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 130, true);
			this.SecondaryProductsGrid.TabIndex = 1;
			// 
			// ComponentUsageGrid
			// 
			this.ComponentUsageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ComponentUsageGrid, "SecondaryParts.ComponentUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).ComponentUsages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOMPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).ComponentUsages)).SyncRoot)).OPP_OE_Component)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOMPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).ComponentUsages)).SyncRoot)).Component.ComponentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOMPivot)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSecondaryPartBOM)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).SecondaryParts)).SyncRoot)).ComponentUsages)).SyncRoot)).OPP_ComponentQuantity)));
			this.ComponentUsageGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "OPP_OE_Component";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo16.ColumnName = "Component+ComponentDescription";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "OPP_ComponentQuantity";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.ComponentUsageGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ComponentUsageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ComponentUsageGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.ComponentUsageGrid.Dock = System.Windows.Forms.DockStyle.Right;
			this.ComponentUsageGrid.GridId = "9321fb1c-449c-42f9-be98-5e4e9c7d1f74";
			this.ComponentUsageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ComponentUsageGrid.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.ComponentUsageGrid.LayoutKey = "ComponentUsageGrid";
			this.ComponentUsageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 14, true);
			this.ComponentUsageGrid.Name = "ComponentUsageGrid";
			this.ComponentUsageGrid.ShouldSetErrorsOnTabPage = false;
			this.ComponentUsageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 130, true);
			this.ComponentUsageGrid.TabIndex = 2;
			// 
			// BOMTopPanel
			// 
			this.BOMTopPanel.Controls.Add(this.CanResellCheckBox);
			this.BOMTopPanel.Controls.Add(this.CanDisassembleKitCheckBox);
			this.BOMTopPanel.Controls.Add(this.AutoPrintAssemblyInstructionsCheckBox);
			this.BOMTopPanel.Controls.Add(this.IsComponentPickedOnSalesOrder);
			this.BOMTopPanel.Controls.Add(this.IsAutoReplenish);
			this.BOMTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BOMTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BOMTopPanel.Name = "BOMTopPanel";
			this.BOMTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1134, 41, true);
			this.BOMTopPanel.TabIndex = 7;
			// 
			// CanResellCheckBox
			// 
			this.CanResellCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CanResellCheckBox, "OP_CanResell");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_CanResell)));
			this.CanResellCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 13, true);
			this.CanResellCheckBox.Name = "CanResellCheckBox";
			this.CanResellCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 14, true);
			this.CanResellCheckBox.TabIndex = 0;
			this.CanResellCheckBox.UseVisualStyleBackColor = true;
			// 
			// CanDisassembleKitCheckBox
			// 
			this.CanDisassembleKitCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CanDisassembleKitCheckBox, "OP_CanDisassembleKit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_CanDisassembleKit)));
			this.CanDisassembleKitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 13, true);
			this.CanDisassembleKitCheckBox.Name = "CanDisassembleKitCheckBox";
			this.CanDisassembleKitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 14, true);
			this.CanDisassembleKitCheckBox.TabIndex = 1;
			this.CanDisassembleKitCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoPrintAssemblyInstructionsCheckBox
			// 
			this.AutoPrintAssemblyInstructionsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintAssemblyInstructionsCheckBox, "OP_AutoPrintAssemblyInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_AutoPrintAssemblyInstructions)));
			this.AutoPrintAssemblyInstructionsCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|a0309685-083d-4b4f-9ea8-c332c3f3be01", "Automatically Print Assembly/Disassembly Instructions");
			this.AutoPrintAssemblyInstructionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 13, true);
			this.AutoPrintAssemblyInstructionsCheckBox.Name = "AutoPrintAssemblyInstructionsCheckBox";
			this.AutoPrintAssemblyInstructionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 14, true);
			this.AutoPrintAssemblyInstructionsCheckBox.TabIndex = 2;
			this.AutoPrintAssemblyInstructionsCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsComponentPickedOnSalesOrder
			// 
			this.IsComponentPickedOnSalesOrder.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsComponentPickedOnSalesOrder, "OP_IsComponentPickedOnSalesOrder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_IsComponentPickedOnSalesOrder)));
			this.IsComponentPickedOnSalesOrder.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|5bd7bc52-4d2d-4838-a855-41e03ecc00f9", "Can Pick without Work Order");
			this.IsComponentPickedOnSalesOrder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 13, true);
			this.IsComponentPickedOnSalesOrder.Name = "IsComponentPickedOnSalesOrder";
			this.IsComponentPickedOnSalesOrder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 14, true);
			this.IsComponentPickedOnSalesOrder.TabIndex = 2;
			this.IsComponentPickedOnSalesOrder.UseVisualStyleBackColor = true;
			// 
			// IsAutoReplenish
			// 
			this.IsAutoReplenish.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsAutoReplenish, "OP_KitIsAutoReplenished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).OP_KitIsAutoReplenished)));
			this.IsAutoReplenish.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSupplierPartForm|6332b57c-64a8-483d-ac96-35d7f495f934", "Auto Replenish");
			this.IsAutoReplenish.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(858, 13, true);
			this.IsAutoReplenish.Name = "IsAutoReplenish";
			this.IsAutoReplenish.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 14, true);
			this.IsAutoReplenish.TabIndex = 2;
			this.IsAutoReplenish.UseVisualStyleBackColor = true;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
			this.WorkflowTabPage.TabIndex = 8;
			// 
			// OrgSupplierPartForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 665, true);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSupplierPart);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.OrgSupplierPart";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1155, 700, true);
			this.Name = "OrgSupplierPartForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Tag = "";
			this.Text = "";
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
			this.UNDGTabPage.ResumeLayout(false);
			this.UNDGTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UNDGsGrid)).EndInit();
			this.UNDGsGrid.ResumeLayout(false);
			this.UNDGsGrid.PerformLayout();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.processTemplateCustomFieldsControl1.ResumeLayout(true);
			this.processTemplateCustomFieldsControl1.PerformLayout();
			this.AdditionalDetailsPanel.ResumeLayout(false);
			this.AdditionalDetailsPanel.PerformLayout();
			this.AdditionalProductDetailsGroupBox.ResumeLayout(false);
			this.AdditionalProductDetailsGroupBox.PerformLayout();
			this.StockControlGroupBox.ResumeLayout(false);
			this.StockControlGroupBox.PerformLayout();
			this.CostCurrencyFindBox.ResumeLayout(true);
			this.CostCurrencyFindBox.PerformLayout();
			this.ClientDefinedFieldsGroupBox.ResumeLayout(false);
			this.ClientDefinedFieldsGroupBox.PerformLayout();
			this.OP_OrderMultipleUnitBoundDropEdit.ResumeLayout(true);
			this.OP_OrderMultipleUnitBoundDropEdit.PerformLayout();
			this.OP_F3_NKPackTypeBoundDropEdit.ResumeLayout(true);
			this.OP_F3_NKPackTypeBoundDropEdit.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.RelatedOrgsTabPage.ResumeLayout(false);
			this.RelatedOrgsTabPage.PerformLayout();
			this.relatedOrganizationsControl1.ResumeLayout(true);
			this.relatedOrganizationsControl1.PerformLayout();
			this.ProductUnitsTabPage.ResumeLayout(false);
			this.ProductUnitsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).EndInit();
			this.PartUnitsGrid.ResumeLayout(false);
			this.PartUnitsGrid.PerformLayout();
			this.ProductBarcodesTabPage.ResumeLayout(false);
			this.ProductBarcodesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).EndInit();
			this.PartBarcodesGrid.ResumeLayout(false);
			this.PartBarcodesGrid.PerformLayout();
			this.DimensionsWeightGroupBox.ResumeLayout(false);
			this.DimensionsWeightGroupBox.PerformLayout();
			this.OP_MeasureUQBoundDropDownEdit.ResumeLayout(true);
			this.OP_MeasureUQBoundDropDownEdit.PerformLayout();
			this.OP_WeightUQBoundDropDownEdit.ResumeLayout(true);
			this.OP_WeightUQBoundDropDownEdit.PerformLayout();
			this.OP_CubicUQBoundDropDownEdit.ResumeLayout(true);
			this.OP_CubicUQBoundDropDownEdit.PerformLayout();
			this.ProductDetailsGroupBox.ResumeLayout(false);
			this.ProductDetailsGroupBox.PerformLayout();
			this.CommodityCodeFindBox.ResumeLayout(true);
			this.CommodityCodeFindBox.PerformLayout();
			this.OP_StockKeepingUnitBoundDropEdit.ResumeLayout(true);
			this.OP_StockKeepingUnitBoundDropEdit.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.NMFCTabPage.ResumeLayout(false);
			this.NMFCTabPage.PerformLayout();
			this.zGroupBox5.ResumeLayout(false);
			this.zGroupBox5.PerformLayout();
			this.FN_CodeCodeFindBox.ResumeLayout(true);
			this.FN_CodeCodeFindBox.PerformLayout();
			this.ArticleGroupBox.ResumeLayout(false);
			this.ArticleGroupBox.PerformLayout();
			this.BOMTabPage.ResumeLayout(false);
			this.BOMTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillOfMaterialsGrid)).EndInit();
			this.BillOfMaterialsGrid.ResumeLayout(false);
			this.BillOfMaterialsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillOfMaterialsViewGrid)).EndInit();
			this.BillOfMaterialsViewGrid.ResumeLayout(false);
			this.BillOfMaterialsViewGrid.PerformLayout();
			this.SecondaryProductsGroupBox.ResumeLayout(false);
			this.SecondaryProductsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SecondaryProductsGrid)).EndInit();
			this.SecondaryProductsGrid.ResumeLayout(false);
			this.SecondaryProductsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ComponentUsageGrid)).EndInit();
			this.ComponentUsageGrid.ResumeLayout(false);
			this.ComponentUsageGrid.PerformLayout();
			this.BOMTopPanel.ResumeLayout(false);
			this.BOMTopPanel.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
