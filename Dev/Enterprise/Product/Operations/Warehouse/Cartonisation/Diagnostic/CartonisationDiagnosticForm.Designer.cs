using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	partial class CartonisationDiagnosticForm
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
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
			}
			if (saveFileDialog != null)
			{
				saveFileDialog.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CartonsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BackContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TopSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CartonsLoadFromXML = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CartonsSavetoXML = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.ProductLoadFromXML = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProductsSaveToXML = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CartoniseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.ResultsTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.openFileDialog = new Enterprise.ZArchitecture.GUI.ZOpenFileDialog();
			this.saveFileDialog = new Enterprise.ZArchitecture.GUI.ZSaveFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CartonsGrid)).BeginInit();
			this.CartonsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BackContainer)).BeginInit();
			this.BackContainer.Panel1.SuspendLayout();
			this.BackContainer.Panel2.SuspendLayout();
			this.BackContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).BeginInit();
			this.TopSplitContainer.Panel1.SuspendLayout();
			this.TopSplitContainer.Panel2.SuspendLayout();
			this.TopSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).BeginInit();
			this.ProductsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 631, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics);
			// 
			// CartonsGrid
			// 
			this.CartonsGrid.AllowNavigation = false;
			this.CartonsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CartonsGrid, "Cartons");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).CartonName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).DimensionUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).MaxFillPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).MaxNumberOfUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).MaxWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).EmptyWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonDefinition)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).Cartons)).SyncRoot)).Cost)));
			this.CartonsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("6c9a8ca9-be8f-4d1f-ac40-015cd01192e6", "Carton Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CartonName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("7df79ca3-aa24-4295-9207-a62dc715aab7", "Height");
			zCalcEditColumnStyleInfo1.ColumnName = "Height";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("c40bc560-b9be-400a-8c5b-7eb0ec1e746b", "Width");
			zCalcEditColumnStyleInfo2.ColumnName = "Width";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("0ff94481-e509-4338-8593-f54345c47b62", "Length");
			zCalcEditColumnStyleInfo3.ColumnName = "Length";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("80f8b9b3-1825-4f22-8b5c-a13d87141f39", "Dimension UQ");
			zDropEditColumnStyleInfo1.ColumnName = "DimensionUQ";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("9e3951ad-306a-4813-b136-becc1d9bf6ff", "Volume");
			zCalcEditColumnStyleInfo4.ColumnName = "Volume";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("1a2ee994-58c1-48ec-8640-21e80a98e614", "Volume UQ");
			zDropEditColumnStyleInfo2.ColumnName = "VolumeUQ";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("b2897db6-8f9d-482c-8eae-74554749798d", "Max Fill Percent");
			zCalcEditColumnStyleInfo5.ColumnName = "MaxFillPercent";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("b1c07b0d-0366-4abf-8771-fcf9a259f56c", "Max Number Of Units");
			zCalcEditColumnStyleInfo6.ColumnName = "MaxNumberOfUnits";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("10020acd-4dc3-47b3-893d-6a046af838f1", "Max Weight");
			zCalcEditColumnStyleInfo7.ColumnName = "MaxWeight";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("e1a7214b-215f-4b55-b198-44b0ef5567bf", "Empty Weight");
			zCalcEditColumnStyleInfo8.ColumnName = "EmptyWeight";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("a95b1fa9-adb5-42b3-b9c7-3483cde060cf", "Weight UQ");
			zDropEditColumnStyleInfo3.ColumnName = "WeightUQ";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("ab11f16e-5572-46ea-9335-ca429b20ee0f", "Cost");
			zCalcEditColumnStyleInfo9.ColumnName = "Cost";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CartonsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CartonsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CartonsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CartonsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CartonsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CartonsGrid.GridId = "4ae8a6f7-abe3-45d1-8e1a-9e17d7f92890";
			this.CartonsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CartonsGrid.LayoutKey = "Grid";
			this.CartonsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 29, true);
			this.CartonsGrid.Name = "CartonsGrid";
			this.CartonsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 217, true);
			this.CartonsGrid.TabIndex = 3;
			// 
			// BackContainer
			// 
			this.BackContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BackContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BackContainer.Name = "BackContainer";
			this.BackContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// BackContainer.Panel1
			// 
			this.BackContainer.Panel1.Controls.Add(this.TopSplitContainer);
			// 
			// BackContainer.Panel2
			// 
			this.BackContainer.Panel2.Controls.Add(this.CartoniseButton);
			this.BackContainer.Panel2.Controls.Add(this.zLabel2);
			this.BackContainer.Panel2.Controls.Add(this.ResultsTextBox);
			this.BackContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 631, true);
			this.BackContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(482);
			this.BackContainer.TabIndex = 1;
			// 
			// TopSplitContainer
			// 
			this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopSplitContainer.Name = "TopSplitContainer";
			this.TopSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TopSplitContainer.Panel1
			// 
			this.TopSplitContainer.Panel1.Controls.Add(this.CartonsLoadFromXML);
			this.TopSplitContainer.Panel1.Controls.Add(this.CartonsSavetoXML);
			this.TopSplitContainer.Panel1.Controls.Add(this.label1);
			this.TopSplitContainer.Panel1.Controls.Add(this.CartonsGrid);
			// 
			// TopSplitContainer.Panel2
			// 
			this.TopSplitContainer.Panel2.Controls.Add(this.ProductLoadFromXML);
			this.TopSplitContainer.Panel2.Controls.Add(this.ProductsSaveToXML);
			this.TopSplitContainer.Panel2.Controls.Add(this.zLabel1);
			this.TopSplitContainer.Panel2.Controls.Add(this.ProductsGrid);
			this.TopSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 482, true);
			this.TopSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(255);
			this.TopSplitContainer.TabIndex = 2;
			// 
			// CartonsLoadFromXML
			// 
			this.CartonsLoadFromXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CartonsLoadFromXML.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("d628f29d-a138-4298-bfae-681d55dbb70a", "Load from XML");
			this.CartonsLoadFromXML.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 5, true);
			this.CartonsLoadFromXML.Name = "CartonsLoadFromXML";
			this.CartonsLoadFromXML.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 21, true);
			this.CartonsLoadFromXML.TabIndex = 2;
			this.CartonsLoadFromXML.ToolTipCaption = null;
			this.CartonsLoadFromXML.UseVisualStyleBackColor = true;
			this.CartonsLoadFromXML.Click += new System.EventHandler(this.CartonsLoadFromXML_Click);
			// 
			// CartonsSavetoXML
			// 
			this.CartonsSavetoXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CartonsSavetoXML.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("a57be3f9-9cdc-4be9-a4fe-1ab6c02d2a8f", "Save to XML");
			this.CartonsSavetoXML.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 5, true);
			this.CartonsSavetoXML.Name = "CartonsSavetoXML";
			this.CartonsSavetoXML.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 21, true);
			this.CartonsSavetoXML.TabIndex = 1;
			this.CartonsSavetoXML.ToolTipCaption = null;
			this.CartonsSavetoXML.UseVisualStyleBackColor = true;
			this.CartonsSavetoXML.Click += new System.EventHandler(this.CartonsSavetoXML_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("7acbc274-86e2-488f-b27c-4d6c27b2d470", "Carton definitions:");
			this.label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.label1.TabIndex = 2;
			// 
			// ProductLoadFromXML
			// 
			this.ProductLoadFromXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ProductLoadFromXML.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("4e194824-4bc9-4afd-9807-a049659efbf0", "Load from XML");
			this.ProductLoadFromXML.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(879, 5, true);
			this.ProductLoadFromXML.Name = "ProductLoadFromXML";
			this.ProductLoadFromXML.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 21, true);
			this.ProductLoadFromXML.TabIndex = 2;
			this.ProductLoadFromXML.ToolTipCaption = null;
			this.ProductLoadFromXML.UseVisualStyleBackColor = true;
			this.ProductLoadFromXML.Click += new System.EventHandler(this.ProductLoadFromXML_Click);
			// 
			// ProductsSaveToXML
			// 
			this.ProductsSaveToXML.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ProductsSaveToXML.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("7fa87571-ad51-47ba-9120-65404cdff714", "Save to XML");
			this.ProductsSaveToXML.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 5, true);
			this.ProductsSaveToXML.Name = "ProductsSaveToXML";
			this.ProductsSaveToXML.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 21, true);
			this.ProductsSaveToXML.TabIndex = 1;
			this.ProductsSaveToXML.ToolTipCaption = null;
			this.ProductsSaveToXML.UseVisualStyleBackColor = true;
			this.ProductsSaveToXML.Click += new System.EventHandler(this.ProductsSaveToXML_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("3325c3e2-d37f-4f40-8247-7659d27587b3", "Products definitions:");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 13, true);
			this.zLabel1.TabIndex = 3;
			// 
			// ProductsGrid
			// 
			this.ProductsGrid.AllowNavigation = false;
			this.ProductsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProductsGrid, "ItemsToPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.ProductName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.DimensionUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).ItemDefinition.KeepUpright)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Cartonisation.Diagnostic.DummyCartonisableItem)(((System.Collections.IList)(((Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics)(null)).ItemsToPack)).SyncRoot)).Location)));
			this.ProductsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("36bd6f8c-5439-42fa-a7f1-89c358a43da5", "Product Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ItemDefinition+ProductName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("ce9a7267-42c8-43b9-835d-3615878b95b9", "Height");
			zCalcEditColumnStyleInfo10.ColumnName = "ItemDefinition+Height";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("18c8406b-8a3c-4073-afb7-f7ed92a2adb4", "Width");
			zCalcEditColumnStyleInfo11.ColumnName = "ItemDefinition+Width";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("37c06c54-c91a-454c-b4a1-e2aa6ca2b187", "Length");
			zCalcEditColumnStyleInfo12.ColumnName = "ItemDefinition+Length";
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("a8a3fc35-5deb-4bad-8332-84e6fa5b57db", "Dimension UQ");
			zDropEditColumnStyleInfo4.ColumnName = "ItemDefinition+DimensionUQ";
			zDropEditColumnStyleInfo4.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("1cc275e3-be86-4acd-b6e0-9761c7fa24d0", "Volume");
			zCalcEditColumnStyleInfo13.ColumnName = "ItemDefinition+Volume";
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("f8da6ddf-e19d-4712-a9fc-917d5f2af13e", "Volume UQ");
			zDropEditColumnStyleInfo5.ColumnName = "ItemDefinition+VolumeUQ";
			zDropEditColumnStyleInfo5.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("96b58136-9728-4571-bd53-2f56305ac872", "Weight");
			zCalcEditColumnStyleInfo14.ColumnName = "ItemDefinition+Weight";
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("7ee734dc-d43a-43e4-bd99-c5e6b02a7ef1", "Weight UQ");
			zDropEditColumnStyleInfo6.ColumnName = "ItemDefinition+WeightUQ";
			zDropEditColumnStyleInfo6.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("b387fb25-122e-4294-99a5-0b0d6e9e7a05", "Keep Up Right");
			zCheckBoxColumnStyleInfo1.ColumnName = "ItemDefinition+KeepUpright";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("8839e569-36e8-41cb-b3a2-84f6498fd8b0", "Quantity");
			zCalcEditColumnStyleInfo15.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("cebdbae0-17f9-4a2e-8681-29945aec3294", "Location");
			zTextBoxColumnStyleInfo3.ColumnName = "Location";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.ProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ProductsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ProductsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.ProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ProductsGrid.GridId = "edfa6473-b128-4522-8982-e86cfd7c2205";
			this.ProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductsGrid.LayoutKey = "Grid";
			this.ProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 29, true);
			this.ProductsGrid.Name = "ProductsGrid";
			this.ProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 192, true);
			this.ProductsGrid.TabIndex = 3;
			// 
			// CartoniseButton
			// 
			this.CartoniseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CartoniseButton.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("df73375c-edcf-49b0-bcb7-63a138875977", "Cartonize");
			this.CartoniseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 12, true);
			this.CartoniseButton.Name = "CartoniseButton";
			this.CartoniseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 35, true);
			this.CartoniseButton.TabIndex = 3;
			this.CartoniseButton.ToolTipCaption = null;
			this.CartoniseButton.UseVisualStyleBackColor = true;
			this.CartoniseButton.Click += new System.EventHandler(this.CartoniseButton_Click);
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("d2736b06-3655-49c1-8212-321a2514993e", "Algorithm results:");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 36, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.zLabel2.TabIndex = 4;
			// 
			// ResultsTextBox
			// 
			this.ResultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ResultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 51, true);
			this.ResultsTextBox.Name = "ResultsTextBox";
			this.ResultsTextBox.ReadOnly = true;
			this.ResultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 92, true);
			this.ResultsTextBox.TabIndex = 4;
			this.ResultsTextBox.Text = "";
			// 
			// openFileDialog
			// 
			this.openFileDialog.AddExtension = true;
			this.openFileDialog.CheckFileExists = true;
			this.openFileDialog.CheckPathExists = true;
			this.openFileDialog.DefaultExt = "*.xml";
			this.openFileDialog.DereferenceLinks = true;
			this.openFileDialog.Filter = "Xml files|*.xml";
			this.openFileDialog.FilterIndex = 1;
			this.openFileDialog.InitialDirectory = "";
			this.openFileDialog.Multiselect = false;
			this.openFileDialog.ReadOnlyChecked = false;
			this.openFileDialog.RestoreDirectory = false;
			this.openFileDialog.ShowHelp = false;
			this.openFileDialog.SupportMultiDottedExtensions = false;
			this.openFileDialog.Title = "";
			this.openFileDialog.ValidateNames = true;
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.AddExtension = true;
			this.saveFileDialog.CheckFileExists = false;
			this.saveFileDialog.CheckPathExists = true;
			this.saveFileDialog.CreatePrompt = false;
			this.saveFileDialog.DefaultExt = "*.xml";
			this.saveFileDialog.DereferenceLinks = true;
			this.saveFileDialog.Filter = "Xml files|*.xml";
			this.saveFileDialog.FilterIndex = 1;
			this.saveFileDialog.InitialDirectory = "";
			this.saveFileDialog.OverwritePrompt = true;
			this.saveFileDialog.RestoreDirectory = false;
			this.saveFileDialog.ShowHelp = false;
			this.saveFileDialog.SupportMultiDottedExtensions = false;
			this.saveFileDialog.Title = "";
			this.saveFileDialog.ValidateNames = true;
			// 
			// CartonisationDiagnosticForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Cartonisation.Diagnostic.Res.GetData("6d76cacb-7230-4cc3-b021-40449802a81f", "Cartonization Algorithm Diagnostic Tool");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 655, true);
			this.Controls.Add(this.BackContainer);
			this.DataSourceType = typeof(Enterprise.Warehouse.Cartonisation.Diagnostic.CartonisationDiagnostics);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			this.Name = "CartonisationDiagnosticForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BackContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CartonsGrid)).EndInit();
			this.CartonsGrid.ResumeLayout(false);
			this.CartonsGrid.PerformLayout();
			this.BackContainer.Panel1.ResumeLayout(false);
			this.BackContainer.Panel2.ResumeLayout(false);
			this.BackContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BackContainer)).EndInit();
			this.BackContainer.ResumeLayout(false);
			this.BackContainer.PerformLayout();
			this.TopSplitContainer.Panel1.ResumeLayout(false);
			this.TopSplitContainer.Panel1.PerformLayout();
			this.TopSplitContainer.Panel2.ResumeLayout(false);
			this.TopSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).EndInit();
			this.TopSplitContainer.ResumeLayout(false);
			this.TopSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductsGrid)).EndInit();
			this.ProductsGrid.ResumeLayout(false);
			this.ProductsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZGrid CartonsGrid;
		private CargoWise.Windows.UI.KSplitContainer BackContainer;
		private CargoWise.Windows.UI.KSplitContainer TopSplitContainer;
		private ZLabel label1;
		private ZLabel zLabel1;
		private ZGrid ProductsGrid;
		private KRichTextBox ResultsTextBox;
		private ZLabel zLabel2;
		private ZButton CartoniseButton;
		private ZButton CartonsLoadFromXML;
		private ZButton CartonsSavetoXML;
		private ZButton ProductLoadFromXML;
		private ZButton ProductsSaveToXML;
		private ZOpenFileDialog openFileDialog;
		private ZSaveFileDialog saveFileDialog;
	}
}
