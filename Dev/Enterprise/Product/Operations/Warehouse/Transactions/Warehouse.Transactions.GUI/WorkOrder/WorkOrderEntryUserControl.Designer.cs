using System;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WorkOrderEntryUserControl
	{
		private ZTemplateTabControl DetailTabControl;
		private ZTabPage DetailTabPage;
		private ZPanel DetailTopPanel;
		private ZTemplateTabControl TransportTabControl;
		private ZTabPage zTabPage1;
		private MasterFiles.GUI.ZDocAddressControl TransportDocAddressControl;
		private ZTemplateTabControl CompaniesTabControl;
		private ZTabPage ConsigneeTabPage;
		private MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		private ZGroupBox DetailsBottomGroupBox;
		private ZGroupBox zGroupBox3;
		private ZCalcDropEdit WD_TotalCubicCalcDropEdit;
		private ZCalcDropEdit WD_TotalWeightCalcDropEdit;
		private ZCalcEdit TotalUnitsCalcEdit;
		private ZGroupBox zGroupBox2;
		private ZGroupBox zGroupBox1;
		private ZTextBox StatusTextBox;
		private ZDropEdit SubTypeDropEdit;
		private ZTextBox PickNoTextBox;
		private ZDropEdit PickOptionDropEdit;
		private ZTextBox WD_ExternalReferenceTextBox;
		private ZCheckBox FinalizeReceiveCheckBox;
		private ZDateEdit RequiredDateEdit;
		private ZTextBox zTextBox1;
		private ZCalcEdit TotalPackagesCalcEdit;
		private ZCalcEdit TotalPalletsCalcEdit;
		public WorkOrderDocketLinesGridUserControl DocketLinesGridControl;
		public WorkOrderDocketLinesGridUserControl workOrderDocketLinesGridUserControl2;
		private ZTabPage ReferenceTabPage;
		private DocketReferenceGridUserControl DocketReferenceGridUserControl;
		private RelatedJobsTabPage relatedJobsTabPage1;
		private ZTemplateTabControl ClientTabControl;
		private ZTabPage zTabPage3;
		private ZGuidFindBox zGuidFindBox1;
		private MasterFiles.GUI.ZOrganisationControl OrgWhsFindControl;
		private ZCalcEdit OrderNoSplitCalcEdit;
		private ZCheckBox InwardProcessingCheckBox;
		private ZTabPage LinesTabPage;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DetailTabControl = new ZTemplateTabControl();
			this.DetailTabPage = new ZTabPage();
			this.DocketLinesGridControl = new WorkOrderDocketLinesGridUserControl();
			this.DetailsBottomGroupBox = new ZGroupBox();
			this.zGroupBox3 = new ZGroupBox();
			this.WD_TotalCubicCalcDropEdit = new ZCalcDropEdit();
			this.WD_TotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.TotalUnitsCalcEdit = new ZCalcEdit();
			this.TotalPackagesCalcEdit = new ZCalcEdit();
			this.TotalPalletsCalcEdit = new ZCalcEdit();
			this.zGroupBox2 = new ZGroupBox();
			this.FinalizeReceiveCheckBox = new ZCheckBox();
			this.PickOptionDropEdit = new ZDropEdit();
			this.PickNoTextBox = new ZTextBox();
			this.RequiredDateEdit = new ZDateEdit();
			this.zGroupBox1 = new ZGroupBox();
			this.InwardProcessingCheckBox = new ZCheckBox();
			this.OrderNoSplitCalcEdit = new ZCalcEdit();
			this.zTextBox1 = new ZTextBox();
			this.StatusTextBox = new ZTextBox();
			this.SubTypeDropEdit = new ZDropEdit();
			this.WD_ExternalReferenceTextBox = new ZTextBox();
			this.DetailTopPanel = new ZPanel();
			this.ClientTabControl = new ZTemplateTabControl();
			this.zTabPage3 = new ZTabPage();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.OrgWhsFindControl = new MasterFiles.GUI.ZOrganisationControl();
			this.TransportTabControl = new ZTemplateTabControl();
			this.zTabPage1 = new ZTabPage();
			this.TransportDocAddressControl = new MasterFiles.GUI.ZDocAddressControl();
			this.CompaniesTabControl = new ZTemplateTabControl();
			this.ConsigneeTabPage = new ZTabPage();
			this.ConsigneeDocAddressControl = new MasterFiles.GUI.ZDocAddressControl();
			this.LinesTabPage = new ZTabPage();
			this.workOrderDocketLinesGridUserControl2 = new WorkOrderDocketLinesGridUserControl();
			this.ReferenceTabPage = new ZTabPage();
			this.DocketReferenceGridUserControl = new DocketReferenceGridUserControl();
			this.relatedJobsTabPage1 = new RelatedJobsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailTabControl.SuspendLayout();
			this.DetailTabPage.SuspendLayout();
			this.DocketLinesGridControl.SuspendLayout();
			this.DetailsBottomGroupBox.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.WD_TotalCubicCalcDropEdit.SuspendLayout();
			this.WD_TotalWeightCalcDropEdit.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.PickOptionDropEdit.SuspendLayout();
			this.RequiredDateEdit.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.DetailTopPanel.SuspendLayout();
			this.ClientTabControl.SuspendLayout();
			this.zTabPage3.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.OrgWhsFindControl.SuspendLayout();
			this.TransportTabControl.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.TransportDocAddressControl.SuspendLayout();
			this.CompaniesTabControl.SuspendLayout();
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.workOrderDocketLinesGridUserControl2.SuspendLayout();
			this.ReferenceTabPage.SuspendLayout();
			this.DocketReferenceGridUserControl.SuspendLayout();
			this.relatedJobsTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsWorkOrder);
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailTabPage);
			this.DetailTabControl.Controls.Add(this.LinesTabPage);
			this.DetailTabControl.Controls.Add(this.ReferenceTabPage);
			this.DetailTabControl.Controls.Add(this.relatedJobsTabPage1);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 513, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 513, true);
			this.DetailTabControl.TabIndex = 0;
			// 
			// DetailTabPage
			// 
			this.DetailTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|18e7ac2b-759c-41f5-a545-3197ebff1e49", "Work Order");
			this.DetailTabPage.Controls.Add(this.DocketLinesGridControl);
			this.DetailTabPage.Controls.Add(this.DetailsBottomGroupBox);
			this.DetailTabPage.Controls.Add(this.DetailTopPanel);
			this.DetailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailTabPage.Name = "DetailTabPage";
			this.DetailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.DetailTabPage.TabIndex = 0;
			// 
			// DocketLinesGridControl
			// 
			this.DocketLinesGridControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocketLinesGridControl, ".");
			this.DocketLinesGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocketLinesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 368, true);
			this.DocketLinesGridControl.Name = "DocketLinesGridControl";
			this.DocketLinesGridControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.DocketLinesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 118, true);
			this.DocketLinesGridControl.TabIndex = 3;
			// 
			// DetailsBottomGroupBox
			// 
			this.DetailsBottomGroupBox.Controls.Add(this.zGroupBox3);
			this.DetailsBottomGroupBox.Controls.Add(this.zGroupBox2);
			this.DetailsBottomGroupBox.Controls.Add(this.zGroupBox1);
			this.DetailsBottomGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsBottomGroupBox, false);
			this.DetailsBottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 218, true);
			this.DetailsBottomGroupBox.Name = "DetailsBottomGroupBox";
			this.DetailsBottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 150, true);
			this.DetailsBottomGroupBox.TabIndex = 2;
			this.DetailsBottomGroupBox.TabStop = false;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.WD_TotalCubicCalcDropEdit);
			this.zGroupBox3.Controls.Add(this.WD_TotalWeightCalcDropEdit);
			this.zGroupBox3.Controls.Add(this.TotalUnitsCalcEdit);
			this.zGroupBox3.Controls.Add(this.TotalPackagesCalcEdit);
			this.zGroupBox3.Controls.Add(this.TotalPalletsCalcEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox3, false);
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(556, 8, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 136, true);
			this.zGroupBox3.TabIndex = 2;
			this.zGroupBox3.TabStop = false;
			// 
			// WD_TotalCubicCalcDropEdit
			// 
			this.WD_TotalCubicCalcDropEdit.AllowDrop = true;
			this.WD_TotalCubicCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalCubicCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_TotalCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).WD_TotalCubicUnit)));
			this.WD_TotalCubicCalcDropEdit.BindToAmount = "WD_TotalCubic";
			this.WD_TotalCubicCalcDropEdit.BindToUnit = "WD_TotalCubicUnit";
			this.WD_TotalCubicCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|52c2805e-5744-482e-a068-59fe9b4d881e", "Total Volume");
			this.WD_TotalCubicCalcDropEdit.Decimals = 3;
			this.WD_TotalCubicCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 112, true);
			this.WD_TotalCubicCalcDropEdit.Name = "WD_TotalCubicCalcDropEdit";
			this.WD_TotalCubicCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.WD_TotalCubicCalcDropEdit.TabIndex = 9;
			// 
			// WD_TotalWeightCalcDropEdit
			// 
			this.WD_TotalWeightCalcDropEdit.AllowDrop = true;
			this.WD_TotalWeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).WD_TotalWeightUnit)));
			this.WD_TotalWeightCalcDropEdit.BindToAmount = "WD_TotalWeight";
			this.WD_TotalWeightCalcDropEdit.BindToUnit = "WD_TotalWeightUnit";
			this.WD_TotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|0d9e3f51-e029-4dcc-ba04-4775de806f43", "Total Weight");
			this.WD_TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 88, true);
			this.WD_TotalWeightCalcDropEdit.Name = "WD_TotalWeightCalcDropEdit";
			this.WD_TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.WD_TotalWeightCalcDropEdit.TabIndex = 7;
			// 
			// TotalUnitsCalcEdit
			// 
			this.TotalUnitsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalUnitsCalcEdit, "WD_TotalUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_TotalUnits)));
			this.TotalUnitsCalcEdit.DecimalPlaces = 0;
			this.TotalUnitsCalcEdit.Decimals = 0;
			this.TotalUnitsCalcEdit.IsCalculatorEnabled = false;
			this.TotalUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 16, true);
			this.TotalUnitsCalcEdit.Name = "TotalUnitsCalcEdit";
			this.TotalUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.TotalUnitsCalcEdit.TabIndex = 1;
			this.TotalUnitsCalcEdit.Text = "0";
			this.TotalUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalUnitsCalcEdit.WordWrap = false;
			// 
			// TotalPackagesCalcEdit
			// 
			this.TotalPackagesCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalPackagesCalcEdit, "WD_PackagesSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_PackagesSent)));
			this.TotalPackagesCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|2a6d7651-ccc3-45ae-9788-19256b835a3b", "Total Packages");
			this.TotalPackagesCalcEdit.DecimalPlaces = 0;
			this.TotalPackagesCalcEdit.Decimals = 0;
			this.TotalPackagesCalcEdit.IsCalculatorEnabled = false;
			this.TotalPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 40, true);
			this.TotalPackagesCalcEdit.Name = "TotalPackagesCalcEdit";
			this.TotalPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.TotalPackagesCalcEdit.TabIndex = 3;
			this.TotalPackagesCalcEdit.Text = "0";
			this.TotalPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPackagesCalcEdit.WordWrap = false;
			// 
			// TotalPalletsCalcEdit
			// 
			this.TotalPalletsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalPalletsCalcEdit, "WD_TotalPallets");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_TotalPallets)));
			this.TotalPalletsCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|65acc27e-4c55-4f86-8cf0-05b08b237095", "Total Pallets");
			this.TotalPalletsCalcEdit.DecimalPlaces = 0;
			this.TotalPalletsCalcEdit.Decimals = 0;
			this.TotalPalletsCalcEdit.IsCalculatorEnabled = false;
			this.TotalPalletsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 64, true);
			this.TotalPalletsCalcEdit.Name = "TotalPalletsCalcEdit";
			this.TotalPalletsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.TotalPalletsCalcEdit.TabIndex = 5;
			this.TotalPalletsCalcEdit.Text = "0";
			this.TotalPalletsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPalletsCalcEdit.WordWrap = false;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.FinalizeReceiveCheckBox);
			this.zGroupBox2.Controls.Add(this.PickOptionDropEdit);
			this.zGroupBox2.Controls.Add(this.PickNoTextBox);
			this.zGroupBox2.Controls.Add(this.RequiredDateEdit);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 8, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 136, true);
			this.zGroupBox2.TabIndex = 1;
			this.zGroupBox2.TabStop = false;
			// 
			// FinalizeReceiveCheckBox
			// 
			this.FinalizeReceiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FinalizeReceiveCheckBox, "WD_AutoFinaliseBOMIntoInventory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsWorkOrder)(null)).WD_AutoFinaliseBOMIntoInventory)));
			this.FinalizeReceiveCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|25515459-8846-4965-aa15-2dd0247e64ef", "Automatically Finalize into Inventory");
			this.FinalizeReceiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 110, true);
			this.FinalizeReceiveCheckBox.Name = "FinalizeReceiveCheckBox";
			this.FinalizeReceiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.FinalizeReceiveCheckBox.TabIndex = 6;
			this.FinalizeReceiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// PickOptionDropEdit
			// 
			this.PickOptionDropEdit.AllowDrop = true;
			this.PickOptionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickOptionDropEdit, "WD_PickOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsWorkOrder)(null)).WD_PickOption)));
			this.PickOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 40, true);
			this.PickOptionDropEdit.Name = "PickOptionDropEdit";
			this.PickOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.PickOptionDropEdit.TabIndex = 3;
			// 
			// PickNoTextBox
			// 
			this.PickNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickNoTextBox, "Pick.WP_PickNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).Pick.WP_PickNo)));
			this.PickNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|b10c1543-33aa-43ad-8054-fab43f2dd00c", "Pick No");
			this.PickNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 16, true);
			this.PickNoTextBox.Name = "PickNoTextBox";
			this.PickNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.PickNoTextBox.TabIndex = 1;
			// 
			// RequiredDateEdit
			// 
			this.RequiredDateEdit.AllowDrop = true;
			this.RequiredDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RequiredDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RequiredDateEdit, "RequiredDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsWorkOrder)(null)).RequiredDate)));
			this.RequiredDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 64, true);
			this.RequiredDateEdit.Name = "RequiredDateEdit";
			this.RequiredDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.RequiredDateEdit.TabIndex = 5;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.InwardProcessingCheckBox);
			this.zGroupBox1.Controls.Add(this.OrderNoSplitCalcEdit);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.StatusTextBox);
			this.zGroupBox1.Controls.Add(this.SubTypeDropEdit);
			this.zGroupBox1.Controls.Add(this.WD_ExternalReferenceTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 8, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 136, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// InwardProcessingCheckBox
			// 
			this.InwardProcessingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InwardProcessingCheckBox, "WD_IsInwardsProcessingJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((WhsWorkOrder)(null)).WD_IsInwardsProcessingJob)));
			this.InwardProcessingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 110, true);
			this.InwardProcessingCheckBox.Name = "InwardProcessingCheckBox";
			this.InwardProcessingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.InwardProcessingCheckBox.TabIndex = 11;
			this.InwardProcessingCheckBox.UseVisualStyleBackColor = true;
			// 
			// OrderNoSplitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OrderNoSplitCalcEdit, "WD_ExternalReferenceSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((WhsWorkOrder)(null)).WD_ExternalReferenceSplit)));
			this.OrderNoSplitCalcEdit.DecimalPlaces = 2;
			this.OrderNoSplitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 40, true);
			this.OrderNoSplitCalcEdit.Name = "OrderNoSplitCalcEdit";
			this.OrderNoSplitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 20, true);
			this.OrderNoSplitCalcEdit.TabIndex = 10;
			this.OrderNoSplitCalcEdit.Text = "0";
			this.OrderNoSplitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "WD_ParentOrderNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).WD_ParentOrderNo)));
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|f501fb29-6d33-44f1-9d3d-3442e1018ae7", "Parent Order No");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 88, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.zTextBox1.TabIndex = 9;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WD_DocketStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).WD_DocketStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|557fff66-d5e9-4e31-b483-91c0b4846680", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 64, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.StatusTextBox.TabIndex = 7;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.SubTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "WD_DocketSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsWorkOrder)(null)).WD_DocketSubType)));
			this.SubTypeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|6592c9f1-e4a3-4e1f-8a9c-3cebf4b3e710", "Type");
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 16, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.SubTypeDropEdit.TabIndex = 1;
			// 
			// WD_ExternalReferenceTextBox
			// 
			this.WD_ExternalReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_ExternalReferenceTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((WhsWorkOrder)(null)).WD_ExternalReference)));
			this.WD_ExternalReferenceTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|f890b1e1-20e3-4792-8c2b-dbf26aaef5f0", "Work Order No");
			this.WD_ExternalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 40, true);
			this.WD_ExternalReferenceTextBox.Name = "WD_ExternalReferenceTextBox";
			this.WD_ExternalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.WD_ExternalReferenceTextBox.TabIndex = 3;
			// 
			// DetailTopPanel
			// 
			this.DetailTopPanel.Controls.Add(this.ClientTabControl);
			this.DetailTopPanel.Controls.Add(this.TransportTabControl);
			this.DetailTopPanel.Controls.Add(this.CompaniesTabControl);
			this.DetailTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTopPanel.Name = "DetailTopPanel";
			this.DetailTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 218, true);
			this.DetailTopPanel.TabIndex = 1;
			// 
			// ClientTabControl
			// 
			this.ClientTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ClientTabControl.Controls.Add(this.zTabPage3);
			this.ClientTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 1, true);
			this.ClientTabControl.Name = "ClientTabControl";
			this.ClientTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.ClientTabControl.TabIndex = 1;
			// 
			// zTabPage3
			// 
			this.zTabPage3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|f040445f-27c8-458a-be6f-40877a983487", "Client");
			this.zTabPage3.Controls.Add(this.zGuidFindBox1);
			this.zTabPage3.Controls.Add(this.OrgWhsFindControl);
			this.zTabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage3.Name = "zTabPage3";
			this.zTabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 188, true);
			this.zTabPage3.TabIndex = 0;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsWorkOrder)(null)).WD_WW_Whs)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 159, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox1.ParentType = null;
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.zGuidFindBox1.TabIndex = 4;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((WhsWorkOrder)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|5d1aaa62-6f62-4e37-a7a1-8cc2f865768d", "Client");
			this.OrgWhsFindControl.Captions = new string[] { "Client" };
			this.OrgWhsFindControl.IsCaptionOverridden = false;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.OrgAddressFormatter = null;
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.OrgWhsFindControl.TabIndex = 3;
			// 
			// TransportTabControl
			// 
			this.TransportTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TransportTabControl.Controls.Add(this.zTabPage1);
			this.TransportTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 1, true);
			this.TransportTabControl.Name = "TransportTabControl";
			this.TransportTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.TransportTabControl.TabIndex = 2;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|599e126a-1a8a-44d6-87d0-852e1232b610", "Transport");
			this.zTabPage1.Controls.Add(this.TransportDocAddressControl);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 188, true);
			this.zTabPage1.TabIndex = 0;
			// 
			// TransportDocAddressControl
			// 
			this.TransportDocAddressControl.AddressValidationProcessCmdKey = null;
			this.TransportDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocAddressControl, "TransportCoDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((WhsWorkOrder)(null)).TransportCoDocAddress)));
			this.TransportDocAddressControl.BindToOrganisations = "Lookups+TransportCos";
			this.TransportDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|6eb1036c-160b-465e-b45f-528ae6ef71c3", "Transport");
			this.TransportDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.TransportDocAddressControl.Name = "TransportDocAddressControl";
			this.TransportDocAddressControl.ReadOnly = false;
			this.TransportDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.TransportDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.TransportDocAddressControl.TabIndex = 0;
			this.TransportDocAddressControl.ValidationJustForced = false;
			// 
			// CompaniesTabControl
			// 
			this.CompaniesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CompaniesTabControl.Controls.Add(this.ConsigneeTabPage);
			this.CompaniesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 1, true);
			this.CompaniesTabControl.Name = "CompaniesTabControl";
			this.CompaniesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 215, true);
			this.CompaniesTabControl.TabIndex = 3;
			// 
			// ConsigneeTabPage
			// 
			this.ConsigneeTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|a9c4fe0d-819f-4ba8-b092-008c0f3fa078", "Consignee");
			this.ConsigneeTabPage.Controls.Add(this.ConsigneeDocAddressControl);
			this.ConsigneeTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConsigneeTabPage.Name = "ConsigneeTabPage";
			this.ConsigneeTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 188, true);
			this.ConsigneeTabPage.TabIndex = 0;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "ConsigneeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((WhsWorkOrder)(null)).ConsigneeDocAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups+Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|c3f2911f-d3fb-4a5b-b3a4-00fb8fe3d7bc", "Consignee");
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.ShowResidentialAddressOnOverride = true;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 0;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|90a34e70-fcc6-47b2-af7f-eea09712ff6a", "Lines");
			this.LinesTabPage.Controls.Add(this.workOrderDocketLinesGridUserControl2);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.LinesTabPage.TabIndex = 2;
			// 
			// workOrderDocketLinesGridUserControl2
			// 
			this.workOrderDocketLinesGridUserControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.workOrderDocketLinesGridUserControl2, ".");
			this.workOrderDocketLinesGridUserControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.workOrderDocketLinesGridUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.workOrderDocketLinesGridUserControl2.Name = "workOrderDocketLinesGridUserControl2";
			this.workOrderDocketLinesGridUserControl2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.workOrderDocketLinesGridUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.workOrderDocketLinesGridUserControl2.TabIndex = 4;
			// 
			// ReferenceTabPage
			// 
			this.ReferenceTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("WorkOrderEntryUserControl|3d3e8772-0e4c-4763-9ced-634972db38d1", "References");
			this.ReferenceTabPage.Controls.Add(this.DocketReferenceGridUserControl);
			this.ReferenceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReferenceTabPage.Name = "ReferenceTabPage";
			this.ReferenceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.ReferenceTabPage.TabIndex = 3;
			// 
			// DocketReferenceGridUserControl
			// 
			this.DocketReferenceGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocketReferenceGridUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((WhsDocket)(((WhsWorkOrder)(null)))));
			this.DocketReferenceGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocketReferenceGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocketReferenceGridUserControl.Name = "DocketReferenceGridUserControl";
			this.DocketReferenceGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.DocketReferenceGridUserControl.TabIndex = 1;
			// 
			// relatedJobsTabPage1
			// 
			this.relatedJobsTabPage1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("be2e7662-ee3f-421a-80c4-9fe19ab3a2ec", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.RelatedJobCollection)(((WhsWorkOrder)(null)).RelatedJobs)));
			this.relatedJobsTabPage1.ExcludeFromBindingOnSave = true;
			this.relatedJobsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.relatedJobsTabPage1.Name = "relatedJobsTabPage1";
			this.relatedJobsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.relatedJobsTabPage1.TabIndex = 4;
			// 
			// WorkOrderEntryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailTabControl);
			this.Name = "WorkOrderEntryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 513, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailTabPage.ResumeLayout(false);
			this.DetailTabPage.PerformLayout();
			this.DocketLinesGridControl.ResumeLayout(true);
			this.DocketLinesGridControl.PerformLayout();
			this.DetailsBottomGroupBox.ResumeLayout(false);
			this.DetailsBottomGroupBox.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.WD_TotalCubicCalcDropEdit.ResumeLayout(true);
			this.WD_TotalCubicCalcDropEdit.PerformLayout();
			this.WD_TotalWeightCalcDropEdit.ResumeLayout(true);
			this.WD_TotalWeightCalcDropEdit.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.PickOptionDropEdit.ResumeLayout(true);
			this.PickOptionDropEdit.PerformLayout();
			this.RequiredDateEdit.ResumeLayout(true);
			this.RequiredDateEdit.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.DetailTopPanel.ResumeLayout(false);
			this.DetailTopPanel.PerformLayout();
			this.ClientTabControl.ResumeLayout(false);
			this.ClientTabControl.PerformLayout();
			this.zTabPage3.ResumeLayout(false);
			this.zTabPage3.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.OrgWhsFindControl.ResumeLayout(true);
			this.OrgWhsFindControl.PerformLayout();
			this.TransportTabControl.ResumeLayout(false);
			this.TransportTabControl.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.TransportDocAddressControl.ResumeLayout(true);
			this.TransportDocAddressControl.PerformLayout();
			this.CompaniesTabControl.ResumeLayout(false);
			this.CompaniesTabControl.PerformLayout();
			this.ConsigneeTabPage.ResumeLayout(false);
			this.ConsigneeTabPage.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.workOrderDocketLinesGridUserControl2.ResumeLayout(true);
			this.workOrderDocketLinesGridUserControl2.PerformLayout();
			this.ReferenceTabPage.ResumeLayout(false);
			this.ReferenceTabPage.PerformLayout();
			this.DocketReferenceGridUserControl.ResumeLayout(true);
			this.DocketReferenceGridUserControl.PerformLayout();
			this.relatedJobsTabPage1.ResumeLayout(false);
			this.relatedJobsTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
