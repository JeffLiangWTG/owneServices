using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class StocktakeEntryForm
	{
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZTextBox WS_StocktakeNumberTextBox;
		private ZTemplateTabControl TabControl;
		private ZTabPage EntryTabPage;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		protected ZTemplateTabControl LinesTabControl;
		private ZTabPage FilterTabPage;
		private ZTabPage LinesTabPage;
		private ZGuidFindBox ClientFilterGuidFindBox;
		private ZGuidDropEdit zGuidDropEdit1;
		private ZGuidDropEdit zGuidDropEdit2;
		private ZDropEdit zDropEdit2;
		private ZTextBox zTextBox1;
		protected ZButton LoadButton;
		private ZDateEdit zDateEdit1;
		protected ZButton CloseLinesButton;
		private ZCodeFindBox CommodityCodeFindBox;
		private ZDropEdit StockTakeCyclezDropEdit;
		private ZGuidFindBox zGuidFindBox1;
		private ZDropEdit ABCCategoryDropEdit;
		private ZGroupBox zGroupBox1;
		protected MenuItem NewCountMenuItem;
		private ZDropEdit StocktakeTypeZDropEdit;
		private ZWorkflowTabPage WorkflowTabPage;
		private Stocktake.WhsStocktakeProductFilterGrid whsStocktakeProductFilterGrid1;
		private ZLabel label1;
		private ZDropEdit CountEmptyLocationsZDropEdit;
		private ZGuidFindBox LocationFindBox;
		protected MenuItem AssignAllLinesToUser;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.WS_StocktakeNumberTextBox = new ZTextBox();
			this.zGroupBox1 = new ZGroupBox();
			this.CountEmptyLocationsZDropEdit = new ZDropEdit();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.zDateEdit1 = new ZDateEdit();
			this.zTextBox1 = new ZTextBox();
			this.ClientFilterGuidFindBox = new ZGuidFindBox();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.TabControl = new ZTemplateTabControl();
			this.EntryTabPage = new ZTabPage();
			this.LinesTabControl = new ZTemplateTabControl();
			this.FilterTabPage = new ZTabPage();
			this.LocationFindBox = new ZGuidFindBox();
			this.label1 = new ZLabel();
			this.whsStocktakeProductFilterGrid1 = new Stocktake.WhsStocktakeProductFilterGrid();
			this.StocktakeTypeZDropEdit = new ZDropEdit();
			this.ABCCategoryDropEdit = new ZDropEdit();
			this.StockTakeCyclezDropEdit = new ZDropEdit();
			this.CommodityCodeFindBox = new ZCodeFindBox();
			this.LoadButton = new ZButton();
			this.zDropEdit2 = new ZDropEdit();
			this.zGuidDropEdit2 = new ZGuidDropEdit();
			this.zGuidDropEdit1 = new ZGuidDropEdit();
			this.LinesTabPage = new ZTabPage();
			this.CloseLinesButton = new ZButton();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.CountEmptyLocationsZDropEdit.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.ClientFilterGuidFindBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.EntryTabPage.SuspendLayout();
			this.LinesTabControl.SuspendLayout();
			this.FilterTabPage.SuspendLayout();
			this.LocationFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.whsStocktakeProductFilterGrid1.InnerGrid)).BeginInit();
			this.whsStocktakeProductFilterGrid1.SuspendLayout();
			this.StocktakeTypeZDropEdit.SuspendLayout();
			this.ABCCategoryDropEdit.SuspendLayout();
			this.StockTakeCyclezDropEdit.SuspendLayout();
			this.CommodityCodeFindBox.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zGuidDropEdit2.SuspendLayout();
			this.zGuidDropEdit1.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 535, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 26, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(817);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsStocktake);
			// 
			// WS_StocktakeNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WS_StocktakeNumberTextBox, "WS_StocktakeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsStocktake)(null)).WS_StocktakeNumber)));
			this.WS_StocktakeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 67, true);
			this.WS_StocktakeNumberTextBox.Name = "WS_StocktakeNumberTextBox";
			this.WS_StocktakeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.WS_StocktakeNumberTextBox.TabIndex = 5;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|3797321e-788e-4258-a8ae-8dfa928bb067", "Details");
			this.zGroupBox1.Controls.Add(this.CountEmptyLocationsZDropEdit);
			this.zGroupBox1.Controls.Add(this.zGuidFindBox1);
			this.zGroupBox1.Controls.Add(this.zDateEdit1);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.WS_StocktakeNumberTextBox);
			this.zGroupBox1.Controls.Add(this.ClientFilterGuidFindBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 126, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// CountEmptyLocationsZDropEdit
			// 
			this.CountEmptyLocationsZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountEmptyLocationsZDropEdit, "WS_CountEmptyLocationsCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_CountEmptyLocationsCategory)));
			this.CountEmptyLocationsZDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|d69f3a9b-62f5-4b9a-9f5c-4110051d1416", "Count Empty Locations");
			this.CountEmptyLocationsZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 93, true);
			this.CountEmptyLocationsZDropEdit.Name = "CountEmptyLocationsZDropEdit";
			this.CountEmptyLocationsZDropEdit.PreBoundMaxLength = 3;
			this.CountEmptyLocationsZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CountEmptyLocationsZDropEdit.TabIndex = 10;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WS_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsStocktake)(null)).WS_WW_Whs)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 18, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.zGuidFindBox1.TabIndex = 1;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "WS_StocktakeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_StocktakeDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|1aeb0748-4dcd-4f6b-96f7-7e6d625c96b1", "Date");
			this.zDateEdit1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 42, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 9;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "StatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsStocktake)(null)).StatusDesc)));
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|d97ee1ef-4488-4d33-879f-ac981ef26881", "Status");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(458, 18, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.zTextBox1.TabIndex = 7;
			// 
			// ClientFilterGuidFindBox
			// 
			this.ClientFilterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientFilterGuidFindBox, "WS_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsStocktake)(null)).WS_OH_Client)));
			this.ClientFilterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 42, true);
			this.ClientFilterGuidFindBox.Name = "ClientFilterGuidFindBox";
			this.ClientFilterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ClientFilterGuidFindBox.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 509, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 3;
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.EntryTabPage);
			this.TabControl.Controls.Add(this.WorkflowTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 6, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 497, true);
			this.TabControl.TabIndex = 0;
			// 
			// EntryTabPage
			// 
			this.EntryTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|8593f8c3-738b-42ac-85b4-58a580d88f0d", "Entry");
			this.EntryTabPage.Controls.Add(this.zGroupBox1);
			this.EntryTabPage.Controls.Add(this.LinesTabControl);
			this.EntryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntryTabPage.Name = "EntryTabPage";
			this.EntryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 470, true);
			this.EntryTabPage.TabIndex = 0;
			// 
			// LinesTabControl
			// 
			this.LinesTabControl.Controls.Add(this.FilterTabPage);
			this.LinesTabControl.Controls.Add(this.LinesTabPage);
			this.LinesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 136, true);
			this.LinesTabControl.Name = "LinesTabControl";
			this.LinesTabControl.SelectedIndex = 0;
			this.LinesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 328, true);
			this.LinesTabControl.TabIndex = 1;
			// 
			// FilterTabPage
			// 
			this.FilterTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|882bd97e-e197-4485-a9f2-94bdf377c669", "Filter");
			this.FilterTabPage.Controls.Add(this.LocationFindBox);
			this.FilterTabPage.Controls.Add(this.label1);
			this.FilterTabPage.Controls.Add(this.whsStocktakeProductFilterGrid1);
			this.FilterTabPage.Controls.Add(this.StocktakeTypeZDropEdit);
			this.FilterTabPage.Controls.Add(this.ABCCategoryDropEdit);
			this.FilterTabPage.Controls.Add(this.StockTakeCyclezDropEdit);
			this.FilterTabPage.Controls.Add(this.CommodityCodeFindBox);
			this.FilterTabPage.Controls.Add(this.LoadButton);
			this.FilterTabPage.Controls.Add(this.zDropEdit2);
			this.FilterTabPage.Controls.Add(this.zGuidDropEdit2);
			this.FilterTabPage.Controls.Add(this.zGuidDropEdit1);
			this.FilterTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FilterTabPage.Name = "FilterTabPage";
			this.FilterTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 333, true);
			this.FilterTabPage.TabIndex = 0;
			// 
			// LocationFindBox
			// 
			this.LocationFindBox.AllowDrop = true;
			this.LocationFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.LocationFindBox, "WS_WL_Location");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsStocktake)(null)).WS_WL_Location)));
			this.LocationFindBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("9e1e7130-90fc-478c-9af4-905eec156f69", "Location");
			this.LocationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 202, true);
			this.LocationFindBox.Name = "LocationFindBox";
			this.LocationFindBox.ShowDescriptionBox = false;
			this.LocationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.LocationFindBox.TabIndex = 9;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ff090c0c-d7f5-40dc-ae47-217d55d4d239", "Products:");
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 22, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.label1.TabIndex = 16;
			// 
			// whsStocktakeProductFilterGrid1
			// 
			this.whsStocktakeProductFilterGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.whsStocktakeProductFilterGrid1, "ProductFilterCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsStocktake)(null)).ProductFilterCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsStocktake)(null)).Lookups.SupplierParts)));
			this.whsStocktakeProductFilterGrid1.BindToFindBoxList = "Lookups.SupplierParts";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("7b938b58-c902-4afd-b1fe-c184674c44a8", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ProductCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("b9b8048f-5a00-476f-bbcc-d0a2734f4a37", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ProductDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.whsStocktakeProductFilterGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.whsStocktakeProductFilterGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.whsStocktakeProductFilterGrid1.GridId = "0d140b44-dc87-4837-8cc1-941e629b5369";
			// 
			// 
			// 
			this.whsStocktakeProductFilterGrid1.InnerGrid.AllowNavigation = false;
			this.whsStocktakeProductFilterGrid1.InnerGrid.CaptionVisible = false;
			this.whsStocktakeProductFilterGrid1.InnerGrid.CopySelectedRowsAllowed = true;
			this.whsStocktakeProductFilterGrid1.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.whsStocktakeProductFilterGrid1.InnerGrid.GridId = null;
			this.whsStocktakeProductFilterGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.whsStocktakeProductFilterGrid1.InnerGrid.LayoutKey = "Grid";
			this.whsStocktakeProductFilterGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.whsStocktakeProductFilterGrid1.InnerGrid.Name = "Grid";
			this.whsStocktakeProductFilterGrid1.InnerGrid.ReadOnly = true;
			this.whsStocktakeProductFilterGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 172, true);
			this.whsStocktakeProductFilterGrid1.InnerGrid.TabIndex = 0;
			this.whsStocktakeProductFilterGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 41, true);
			this.whsStocktakeProductFilterGrid1.Name = "whsStocktakeProductFilterGrid1";
			this.whsStocktakeProductFilterGrid1.NameOfAGridElement = Enterprise.Warehouse.Transactions.GUI.Res.GetData("B41523C1-04A1-42A1-A943-9BEE7711EFC5", "Product");
			this.whsStocktakeProductFilterGrid1.ReadOnly = true;
			this.whsStocktakeProductFilterGrid1.ShowEditButton = false;
			this.whsStocktakeProductFilterGrid1.ShowNewButton = false;
			this.whsStocktakeProductFilterGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 210, true);
			this.whsStocktakeProductFilterGrid1.TabIndex = 15;
			// 
			// StocktakeTypeZDropEdit
			// 
			this.StocktakeTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StocktakeTypeZDropEdit, "WS_StocktakeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_StocktakeType)));
			this.StocktakeTypeZDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("d32ff91d-5489-4fc9-be97-cc532cf7660e", "Type");
			this.StocktakeTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 15, true);
			this.StocktakeTypeZDropEdit.Name = "StocktakeTypeZDropEdit";
			this.StocktakeTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StocktakeTypeZDropEdit.TabIndex = 1;
			// 
			// ABCCategoryDropEdit
			// 
			this.ABCCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ABCCategoryDropEdit, "WS_ABCAnalysisCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_ABCAnalysisCategory)));
			this.ABCCategoryDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("eb376add-e45e-4436-abef-89ae269719af", "ABC Category");
			this.ABCCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 67, true);
			this.ABCCategoryDropEdit.Name = "ABCCategoryDropEdit";
			this.ABCCategoryDropEdit.PreBoundMaxLength = 7;
			this.ABCCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ABCCategoryDropEdit.TabIndex = 4;
			// 
			// StockTakeCyclezDropEdit
			// 
			this.StockTakeCyclezDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StockTakeCyclezDropEdit, "WS_StocktakeCycle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_StocktakeCycle)));
			this.StockTakeCyclezDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 41, true);
			this.StockTakeCyclezDropEdit.Name = "StockTakeCyclezDropEdit";
			this.StockTakeCyclezDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StockTakeCyclezDropEdit.TabIndex = 2;
			// 
			// CommodityCodeFindBox
			// 
			this.CommodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityCodeFindBox, "WS_RH_NKCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsStocktake)(null)).WS_RH_NKCommodityCode)));
			this.CommodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 93, true);
			this.CommodityCodeFindBox.Name = "CommodityCodeFindBox";
			this.CommodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CommodityCodeFindBox.TabIndex = 5;
			// 
			// LoadButton
			// 
			this.LoadButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LoadButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|0b17ff90-3ace-423b-8786-3410ad437206", "Load New Stocktake");
			this.LoadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 301, true);
			this.LoadButton.Name = "LoadButton";
			this.LoadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.LoadButton.TabIndex = 14;
			this.LoadButton.Click += new EventHandler(this.LoadButton_Click);
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "WS_PickMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_PickMethod)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 119, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zDropEdit2.TabIndex = 6;
			// 
			// zGuidDropEdit2
			// 
			this.zGuidDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidDropEdit2, "WS_WA_Area");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_WA_Area)));
			this.zGuidDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 174, true);
			this.zGuidDropEdit2.Name = "zGuidDropEdit2";
			this.zGuidDropEdit2.ShowDescriptionBox = false;
			this.zGuidDropEdit2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.zGuidDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zGuidDropEdit2.TabIndex = 8;
			// 
			// zGuidDropEdit1
			// 
			this.zGuidDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidDropEdit1, "WS_WR_Row");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsStocktake)(null)).WS_WR_Row)));
			this.zGuidDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 146, true);
			this.zGuidDropEdit1.Name = "zGuidDropEdit1";
			this.zGuidDropEdit1.ShowDescriptionBox = false;
			this.zGuidDropEdit1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.zGuidDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.zGuidDropEdit1.TabIndex = 7;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|b5117f37-13e7-481c-aa7d-681492367c0b", "Lines");
			this.LinesTabPage.Controls.Add(this.CloseLinesButton);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 333, true);
			this.LinesTabPage.TabIndex = 1;
			// 
			// CloseLinesButton
			// 
			this.CloseLinesButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseLinesButton.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("StocktakeEntryForm|d69f3a9b-62f5-4b9a-9f5c-4110051d1415", "Close Lines");
			this.CloseLinesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(884, 304, true);
			this.CloseLinesButton.Name = "CloseLinesButton";
			this.CloseLinesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.CloseLinesButton.TabIndex = 10;
			this.CloseLinesButton.Click += new EventHandler(this.CloseLinesButton_Click);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 470, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 470, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 470, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// StocktakeEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 561, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsStocktake);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsStocktake";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 600, true);
			this.Name = "StocktakeEntryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.CountEmptyLocationsZDropEdit.ResumeLayout(true);
			this.CountEmptyLocationsZDropEdit.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.ClientFilterGuidFindBox.ResumeLayout(true);
			this.ClientFilterGuidFindBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.EntryTabPage.ResumeLayout(false);
			this.EntryTabPage.PerformLayout();
			this.LinesTabControl.ResumeLayout(false);
			this.LinesTabControl.PerformLayout();
			this.FilterTabPage.ResumeLayout(false);
			this.FilterTabPage.PerformLayout();
			this.LocationFindBox.ResumeLayout(true);
			this.LocationFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.whsStocktakeProductFilterGrid1.InnerGrid)).EndInit();
			this.whsStocktakeProductFilterGrid1.ResumeLayout(true);
			this.whsStocktakeProductFilterGrid1.PerformLayout();
			this.StocktakeTypeZDropEdit.ResumeLayout(true);
			this.StocktakeTypeZDropEdit.PerformLayout();
			this.ABCCategoryDropEdit.ResumeLayout(true);
			this.ABCCategoryDropEdit.PerformLayout();
			this.StockTakeCyclezDropEdit.ResumeLayout(true);
			this.StockTakeCyclezDropEdit.PerformLayout();
			this.CommodityCodeFindBox.ResumeLayout(true);
			this.CommodityCodeFindBox.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zGuidDropEdit2.ResumeLayout(true);
			this.zGuidDropEdit2.PerformLayout();
			this.zGuidDropEdit1.ResumeLayout(true);
			this.zGuidDropEdit1.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
