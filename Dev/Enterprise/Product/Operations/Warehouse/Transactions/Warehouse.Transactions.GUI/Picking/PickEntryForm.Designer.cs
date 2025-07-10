using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickEntryForm
	{
		internal ZTabPage PickSlipTabPage;
		ZWorkflowTabPage WorkflowTabPage;
		CargoWise.Windows.UI.KTableLayoutPanel TableLayoutPanel;
		PickLinesUserControl PickLinesUserControl;
		PickHeaderUserControl PickHeaderUserControl2;
		PickHeaderUserControl PickHeaderUserControl1;
		PickOrdersUserControl PickOrdersUserControl;
		ZMenuItem AllocatePackageLabelsMenuItem;
		ZMenuItem AssignAllLinesToUserMenuItem;
		ZMenuItem UnAssignAllLinesMenuItem;
		DocAllPackageLabelsMenuItem PrintAllPackageLabelsMenuItem;
		DocAllPackageLabelsMenuItem PrintAllPackageLabelsWithoutSeparatorLabelsMenuItem;
		ZMenuItem ClearIsAwaitingReplenishment;
		ZTabPage PackingViewTabPage;
		CargoWise.Windows.UI.KSplitContainer PackagesSplitContainer;
		ZGrid PackagesGrid;
		ZGrid PackedGrid;
		ZGroupBox OuterPackagesGroupBox;
		ZGroupBox PackedItemsGroupBox;
		ZMenuItem CancelPackageLabelsMenuItem;
		ZMenuItem ChangeTaskPlanningStatusMenuItem;

		protected override void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.PickSlipTabPage = new ZTabPage();
			this.PickLinesUserControl = new PickLinesUserControl();
			this.PickHeaderUserControl2 = new PickHeaderUserControl();
			this.TableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.PickHeaderUserControl1 = new PickHeaderUserControl();
			this.PickOrdersUserControl = new PickOrdersUserControl();
			this.PackingViewTabPage = new ZTabPage();
			this.PackagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OuterPackagesGroupBox = new ZGroupBox();
			this.PackagesGrid = new ZGrid();
			this.PackedItemsGroupBox = new ZGroupBox();
			this.PackedGrid = new ZGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WorkflowTabPage.SuspendLayout();
			this.PickSlipTabPage.SuspendLayout();
			this.PickLinesUserControl.SuspendLayout();
			this.PickHeaderUserControl2.SuspendLayout();
			this.TableLayoutPanel.SuspendLayout();
			this.PickHeaderUserControl1.SuspendLayout();
			this.PickOrdersUserControl.SuspendLayout();
			this.PackingViewTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).BeginInit();
			this.PackagesSplitContainer.Panel1.SuspendLayout();
			this.PackagesSplitContainer.Panel2.SuspendLayout();
			this.PackagesSplitContainer.SuspendLayout();
			this.OuterPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.PackedItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackedGrid)).BeginInit();
			this.PackedGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.PickSlipTabPage);
			this.MainTabControl.Controls.Add(this.PackingViewTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1235, 670, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackingViewTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PickSlipTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickEntryForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Orders");
			this.MainTabPage.Controls.Add(this.TableLayoutPanel);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1270, 585, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 3, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 28, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1270, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1398);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPick);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 550, true);
			this.WorkflowTabPage.TabIndex = 1;
			// 
			// PickSlipTabPage
			// 
			this.PickSlipTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickEntryForm|1cac0eac-2fc5-466d-86cf-cf69cec0a90c", "Pick Slip");
			this.PickSlipTabPage.Controls.Add(this.PickLinesUserControl);
			this.PickSlipTabPage.Controls.Add(this.PickHeaderUserControl2);
			this.PickSlipTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PickSlipTabPage.Name = "PickSlipTabPage";
			this.PickSlipTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PickSlipTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			this.PickSlipTabPage.TabIndex = 3;
			// 
			// PickLinesUserControl
			// 
			this.PickLinesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickLinesUserControl, ".");
			this.PickLinesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickLinesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 118, true);
			this.PickLinesUserControl.Name = "PickLinesUserControl";
			this.PickLinesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 437, true);
			this.PickLinesUserControl.TabIndex = 1;
			// 
			// PickHeaderUserControl2
			// 
			this.PickHeaderUserControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickHeaderUserControl2, ".");
			this.PickHeaderUserControl2.Dock = System.Windows.Forms.DockStyle.Top;
			this.PickHeaderUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PickHeaderUserControl2.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl2.Name = "PickHeaderUserControl2";
			this.PickHeaderUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl2.TabIndex = 0;
			// 
			// TableLayoutPanel
			// 
			this.TableLayoutPanel.ColumnCount = 1;
			this.TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableLayoutPanel.Controls.Add(this.PickHeaderUserControl1, 0, 0);
			this.TableLayoutPanel.Controls.Add(this.PickOrdersUserControl, 0, 2);
			this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TableLayoutPanel.Name = "TableLayoutPanel";
			this.TableLayoutPanel.RowCount = 3;
			this.TableLayoutPanel.RowStyles.Add(new RowStyle());
			this.TableLayoutPanel.RowStyles.Add(new RowStyle());
			this.TableLayoutPanel.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			this.TableLayoutPanel.TabIndex = 3;
			// 
			// PickHeaderUserControl1
			// 
			this.PickHeaderUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickHeaderUserControl1, ".");
			this.PickHeaderUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickHeaderUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PickHeaderUserControl1.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl1.Name = "PickHeaderUserControl1";
			this.PickHeaderUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickHeaderUserControl1.TabIndex = 0;
			// 
			// PickOrdersUserControl
			// 
			this.PickOrdersUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PickOrdersUserControl, ".");
			this.PickOrdersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickOrdersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 121, true);
			this.PickOrdersUserControl.Name = "PickOrdersUserControl";
			this.PickOrdersUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PickOrdersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(926, 431, true);
			this.PickOrdersUserControl.TabIndex = 2;
			// 
			// PackingViewTabPage
			// 
			this.PackingViewTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("dc7b913a-04dd-482c-884d-5a4daa3aed2c", "Packing View");
			this.PackingViewTabPage.Controls.Add(this.PackagesSplitContainer);
			this.PackingViewTabPage.Cursor = System.Windows.Forms.Cursors.Default;
			this.PackingViewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackingViewTabPage.Name = "PackingViewTabPage";
			this.PackingViewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackingViewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 558, true);
			this.PackingViewTabPage.TabIndex = 4;
			this.PackingViewTabPage.UseVisualStyleBackColor = true;
			// 
			// PackagesSplitContainer
			// 
			this.PackagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackagesSplitContainer.Name = "PackagesSplitContainer";
			// 
			// PackagesSplitContainer.Panel1
			// 
			this.PackagesSplitContainer.Panel1.Controls.Add(this.OuterPackagesGroupBox);
			// 
			// PackagesSplitContainer.Panel2
			// 
			this.PackagesSplitContainer.Panel2.Controls.Add(this.PackedItemsGroupBox);
			this.PackagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 552, true);
			this.PackagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(598);
			this.PackagesSplitContainer.TabIndex = 0;
			// 
			// OuterPackagesGroupBox
			// 
			this.OuterPackagesGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("7a4712bb-1fc2-407a-aabd-1667294d70b1", "Outer Packages");
			this.OuterPackagesGroupBox.Controls.Add(this.PackagesGrid);
			this.OuterPackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OuterPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OuterPackagesGroupBox.Name = "OuterPackagesGroupBox";
			this.OuterPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 552, true);
			this.OuterPackagesGroupBox.TabIndex = 1;
			this.OuterPackagesGroupBox.TabStop = false;
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackagesGrid, "OuterPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_PackageID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).CartonGroupAndSize)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_DimensionUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).KP_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackType.F3_UOMType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackageJob.ParentJob.JobNo)));
			this.PackagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "KP_F3_NKPackType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "KP_PackageID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "CartonGroupAndSize";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "KP_Length";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "KP_Width";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "KP_Height";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.ColumnName = "KP_DimensionUQ";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(23);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "KP_Volume";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo5.ColumnName = "KP_VolumeUQ";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(23);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "KP_Weight";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.ColumnName = "KP_WeightUQ";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(23);
			zTextBoxColumnStyleInfo7.ColumnName = "PackType+F3_UOMType";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("5f569147-28b9-4e9e-a3a0-4f1cff29c834", "Order Number");
			zTextBoxColumnStyleInfo8.ColumnName = "PackageJob+ParentJob+JobNo";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGrid.GridId = "dd66ab3e-e352-4d91-8a42-bbbe37d398e6";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "PackagesGrid";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.ReadOnly = true;
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 533, true);
			this.PackagesGrid.TabIndex = 0;
			// 
			// PackedItemsGroupBox
			// 
			this.PackedItemsGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("dee97212-a6df-4baa-8358-e24e8d6358d0", "Packed Items");
			this.PackedItemsGroupBox.Controls.Add(this.PackedGrid);
			this.PackedItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemsGroupBox.Name = "PackedItemsGroupBox";
			this.PackedItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 552, true);
			this.PackedItemsGroupBox.TabIndex = 1;
			this.PackedItemsGroupBox.TabStop = false;
			// 
			// PackedGrid
			// 
			this.PackedGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackedGrid, "OuterPackages.PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PkgPackageItemDivotsWrapper)(((System.Collections.IList)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackedItems)).SyncRoot)).PackedQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackageItemDivotsWrapper)(((System.Collections.IList)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackedItems)).SyncRoot)).UQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackageItemDivotsWrapper)(((System.Collections.IList)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackedItems)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PkgPackageItemDivotsWrapper)(((System.Collections.IList)(((PkgPackage)(((System.Collections.IList)(((WhsPick)(null)).OuterPackages)).SyncRoot)).PackedItems)).SyncRoot)).DescriptionWithoutSupplement)));
			this.PackedGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "PackedQty";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo9.ColumnName = "UQ";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(27);
			zTextBoxColumnStyleInfo10.ColumnName = "Code";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Res.GetData("7b6b5903-e25f-412d-9292-3f5f19079a52", "Product Code");
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo11.ColumnName = "DescriptionWithoutSupplement";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			this.PackedGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PackedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PackedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.PackedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.PackedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedGrid.GridId = "185d3b6e-029e-48d5-aada-378b2c393eb6";
			this.PackedGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackedGrid.LayoutKey = "PackedGrid";
			this.PackedGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackedGrid.Name = "PackedGrid";
			this.PackedGrid.ReadOnly = true;
			this.PackedGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 533, true);
			this.PackedGrid.TabIndex = 0;
			// 
			// PickEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("c2745585-2242-4118-a7f5-033ee44b3f83", "Pick");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 726, true);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsPick);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPick";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 725, true);
			this.Name = "PickEntryForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.PickSlipTabPage.ResumeLayout(false);
			this.PickSlipTabPage.PerformLayout();
			this.PickLinesUserControl.ResumeLayout(true);
			this.PickLinesUserControl.PerformLayout();
			this.PickHeaderUserControl2.ResumeLayout(true);
			this.PickHeaderUserControl2.PerformLayout();
			this.TableLayoutPanel.ResumeLayout(false);
			this.TableLayoutPanel.PerformLayout();
			this.PickHeaderUserControl1.ResumeLayout(true);
			this.PickHeaderUserControl1.PerformLayout();
			this.PickOrdersUserControl.ResumeLayout(true);
			this.PickOrdersUserControl.PerformLayout();
			this.PackingViewTabPage.ResumeLayout(false);
			this.PackingViewTabPage.PerformLayout();
			this.PackagesSplitContainer.Panel1.ResumeLayout(false);
			this.PackagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).EndInit();
			this.PackagesSplitContainer.ResumeLayout(false);
			this.PackagesSplitContainer.PerformLayout();
			this.OuterPackagesGroupBox.ResumeLayout(false);
			this.OuterPackagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.PackedItemsGroupBox.ResumeLayout(false);
			this.PackedItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackedGrid)).EndInit();
			this.PackedGrid.ResumeLayout(false);
			this.PackedGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
