using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class DynamicWorkOrderEntryUserControl
	{
		#region Auto

		private RelatedJobsTabPage RelatedJobsTabPage;
		private ZTabPage LinesTabPage;
		public DynamicWorkOrderLinesUserControl workOrderDocketLinesGridUserControl2;
		private ZTabPage DetailTabPage;
		public DynamicWorkOrderLinesUserControl DocketLinesGridControl;
		private ZPanel DetailTopPanel;
		private ZCheckBox InwardProcessingCheckBox;
		private ZGuidFindBox WarehouseFindBox;
		private MasterFiles.GUI.ZOrganisationControl OrgWhsFindControl;
		private ZGroupBox OrderDetailsGroupBox;
		private ZCalcDropEdit WD_TotalCubicCalcDropEdit;
		private ZCalcDropEdit WD_TotalWeightCalcDropEdit;
		private ZCheckBox FinalizeReceiveCheckBox;
		private ZCalcEdit TotalUnitsCalcEdit;
		private ZCalcEdit OrderNoSplitCalcEdit;
		private ZDateEdit RequiredDateEdit;
		private ZDropEdit PickOptionDropEdit;
		private ZTextBox StatusTextBox;
		private ZTextBox PickNoTextBox;
		private ZDropEdit SubTypeDropEdit;
		private ZTextBox WD_ExternalReferenceTextBox;
		private ZTemplateTabControl DetailTabControl;

		private System.ComponentModel.IContainer components;

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RelatedJobsTabPage = new Enterprise.ZArchitecture.GUI.RelatedJobsTabPage();
			this.LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.workOrderDocketLinesGridUserControl2 = new Enterprise.Warehouse.Transactions.GUI.DynamicWorkOrderLinesUserControl();
			this.DetailTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocketLinesGridControl = new Enterprise.Warehouse.Transactions.GUI.DynamicWorkOrderLinesUserControl();
			this.DetailTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrgWhsFindControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.WarehouseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OrderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WD_ExternalReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrderNoSplitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PickNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PickOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequiredDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalUnitsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WD_TotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WD_TotalCubicCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.FinalizeReceiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InwardProcessingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DetailTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RelatedJobsTabPage.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			this.workOrderDocketLinesGridUserControl2.SuspendLayout();
			this.DetailTabPage.SuspendLayout();
			this.DocketLinesGridControl.SuspendLayout();
			this.DetailTopPanel.SuspendLayout();
			this.OrgWhsFindControl.SuspendLayout();
			this.WarehouseFindBox.SuspendLayout();
			this.OrderDetailsGroupBox.SuspendLayout();
			this.SubTypeDropEdit.SuspendLayout();
			this.PickOptionDropEdit.SuspendLayout();
			this.RequiredDateEdit.SuspendLayout();
			this.WD_TotalWeightCalcDropEdit.SuspendLayout();
			this.WD_TotalCubicCalcDropEdit.SuspendLayout();
			this.DetailTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder);
			// 
			// RelatedJobsTabPage
			// 
			this.RelatedJobsTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("be2e7662-ee3f-421a-80c4-9fe19ab3a2ec", "Related Jobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.RelatedJobCollection)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).RelatedJobs)));
			this.RelatedJobsTabPage.ExcludeFromBindingOnSave = true;
			this.RelatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedJobsTabPage.Name = "RelatedJobsTabPage";
			this.RelatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.RelatedJobsTabPage.TabIndex = 2;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|90a34e70-fcc6-47b2-af7f-eea09712ff6a", "Lines");
			this.LinesTabPage.Controls.Add(this.workOrderDocketLinesGridUserControl2);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 486, true);
			this.LinesTabPage.TabIndex = 1;
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
			// DetailTabPage
			// 
			this.DetailTabPage.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|18e7ac2b-759c-41f5-a545-3197ebff1e49", "Work Order");
			this.DetailTabPage.Controls.Add(this.DocketLinesGridControl);
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
			this.DocketLinesGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.DocketLinesGridControl.Name = "DocketLinesGridControl";
			this.DocketLinesGridControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0, true);
			this.DocketLinesGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 305, true);
			this.DocketLinesGridControl.TabIndex = 4;
			// 
			// DetailTopPanel
			// 
			this.DetailTopPanel.Controls.Add(this.OrgWhsFindControl);
			this.DetailTopPanel.Controls.Add(this.WarehouseFindBox);
			this.DetailTopPanel.Controls.Add(this.OrderDetailsGroupBox);
			this.DetailTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTopPanel.Name = "DetailTopPanel";
			this.DetailTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 181, true);
			this.DetailTopPanel.TabIndex = 3;
			// 
			// OrgWhsFindControl
			// 
			this.OrgWhsFindControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgWhsFindControl, "WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_OH_Client)));
			this.OrgWhsFindControl.BindToOrganisations = "Lookups+Clients";
			this.OrgWhsFindControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|5d1aaa62-6f62-4e37-a7a1-8cc2f865768d", "Client");
			this.OrgWhsFindControl.Captions = new string[] {
        "Client"};
			this.OrgWhsFindControl.IsCaptionOverridden = false;
			this.OrgWhsFindControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgWhsFindControl.Name = "OrgWhsFindControl";
			this.OrgWhsFindControl.OrgAddressFormatter = null;
			this.OrgWhsFindControl.PopupCaption = "";
			this.OrgWhsFindControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.OrgWhsFindControl.TabIndex = 1;
			// 
			// WarehouseFindBox
			// 
			this.WarehouseFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseFindBox, "WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_WW_Whs)));
			this.WarehouseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 155, true);
			this.WarehouseFindBox.Name = "WarehouseFindBox";
			this.WarehouseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WarehouseFindBox.ParentType = null;
			this.WarehouseFindBox.PreBoundMaxLength = 3;
			this.WarehouseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.WarehouseFindBox.TabIndex = 2;
			// 
			// OrderDetailsGroupBox
			// 
			this.OrderDetailsGroupBox.Controls.Add(this.SubTypeDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.WD_ExternalReferenceTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.OrderNoSplitCalcEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.StatusTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.PickNoTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.PickOptionDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.RequiredDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.TotalUnitsCalcEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.WD_TotalWeightCalcDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.WD_TotalCubicCalcDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.FinalizeReceiveCheckBox);
			this.OrderDetailsGroupBox.Controls.Add(this.InwardProcessingCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrderDetailsGroupBox, false);
			this.OrderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 0, true);
			this.OrderDetailsGroupBox.Name = "OrderDetailsGroupBox";
			this.OrderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 176, true);
			this.OrderDetailsGroupBox.TabIndex = 3;
			this.OrderDetailsGroupBox.TabStop = false;
			// 
			// SubTypeDropEdit
			// 
			this.SubTypeDropEdit.AllowDrop = true;
			this.SubTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubTypeDropEdit, "WD_DocketSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_DocketSubType)));
			this.SubTypeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|6592c9f1-e4a3-4e1f-8a9c-3cebf4b3e710", "Type");
			this.SubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 16, true);
			this.SubTypeDropEdit.Name = "SubTypeDropEdit";
			this.SubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.SubTypeDropEdit.TabIndex = 1;
			// 
			// WD_ExternalReferenceTextBox
			// 
			this.WD_ExternalReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_ExternalReferenceTextBox, "WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_ExternalReference)));
			this.WD_ExternalReferenceTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|f890b1e1-20e3-4792-8c2b-dbf26aaef5f0", "Work Order No");
			this.WD_ExternalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 40, true);
			this.WD_ExternalReferenceTextBox.Name = "WD_ExternalReferenceTextBox";
			this.WD_ExternalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.WD_ExternalReferenceTextBox.TabIndex = 2;
			// 
			// OrderNoSplitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OrderNoSplitCalcEdit, "WD_ExternalReferenceSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_ExternalReferenceSplit)));
			this.OrderNoSplitCalcEdit.DecimalPlaces = 2;
			this.OrderNoSplitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 40, true);
			this.OrderNoSplitCalcEdit.Name = "OrderNoSplitCalcEdit";
			this.OrderNoSplitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 20, true);
			this.OrderNoSplitCalcEdit.TabIndex = 3;
			this.OrderNoSplitCalcEdit.Text = "0";
			this.OrderNoSplitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.OrderNoSplitCalcEdit.TrackDisposedAccess = true;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusTextBox, "WD_DocketStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_DocketStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|557fff66-d5e9-4e31-b483-91c0b4846680", "Status");
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 64, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.StatusTextBox.TabIndex = 4;
			// 
			// PickNoTextBox
			// 
			this.PickNoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickNoTextBox, "Pick.WP_PickNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).Pick.WP_PickNo)));
			this.PickNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|b10c1543-33aa-43ad-8054-fab43f2dd00c", "Pick No");
			this.PickNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 88, true);
			this.PickNoTextBox.Name = "PickNoTextBox";
			this.PickNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.PickNoTextBox.TabIndex = 5;
			// 
			// PickOptionDropEdit
			// 
			this.PickOptionDropEdit.AllowDrop = true;
			this.PickOptionDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PickOptionDropEdit, "WD_PickOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_PickOption)));
			this.PickOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 112, true);
			this.PickOptionDropEdit.Name = "PickOptionDropEdit";
			this.PickOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.PickOptionDropEdit.TabIndex = 6;
			// 
			// RequiredDateEdit
			// 
			this.RequiredDateEdit.AllowDrop = true;
			this.RequiredDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RequiredDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RequiredDateEdit, "RequiredDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).RequiredDate)));
			this.RequiredDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 136, true);
			this.RequiredDateEdit.Name = "RequiredDateEdit";
			this.RequiredDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.RequiredDateEdit.TabIndex = 7;
			// 
			// TotalUnitsCalcEdit
			// 
			this.TotalUnitsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalUnitsCalcEdit, "WD_TotalUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_TotalUnits)));
			this.TotalUnitsCalcEdit.DecimalPlaces = 0;
			this.TotalUnitsCalcEdit.Decimals = 0;
			this.TotalUnitsCalcEdit.IsCalculatorEnabled = false;
			this.TotalUnitsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 16, true);
			this.TotalUnitsCalcEdit.Name = "TotalUnitsCalcEdit";
			this.TotalUnitsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.TotalUnitsCalcEdit.TabIndex = 8;
			this.TotalUnitsCalcEdit.Text = "0";
			this.TotalUnitsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalUnitsCalcEdit.TrackDisposedAccess = true;
			this.TotalUnitsCalcEdit.WordWrap = false;
			// 
			// WD_TotalWeightCalcDropEdit
			// 
			this.WD_TotalWeightCalcDropEdit.AllowDrop = true;
			this.WD_TotalWeightCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_TotalWeightUnit)));
			this.WD_TotalWeightCalcDropEdit.BindToAmount = "WD_TotalWeight";
			this.WD_TotalWeightCalcDropEdit.BindToUnit = "WD_TotalWeightUnit";
			this.WD_TotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|0d9e3f51-e029-4dcc-ba04-4775de806f43", "Total Weight");
			this.WD_TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 40, true);
			this.WD_TotalWeightCalcDropEdit.Name = "WD_TotalWeightCalcDropEdit";
			this.WD_TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.WD_TotalWeightCalcDropEdit.TabIndex = 9;
			// 
			// WD_TotalCubicCalcDropEdit
			// 
			this.WD_TotalCubicCalcDropEdit.AllowDrop = true;
			this.WD_TotalCubicCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WD_TotalCubicCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_TotalCubic)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_TotalCubicUnit)));
			this.WD_TotalCubicCalcDropEdit.BindToAmount = "WD_TotalCubic";
			this.WD_TotalCubicCalcDropEdit.BindToUnit = "WD_TotalCubicUnit";
			this.WD_TotalCubicCalcDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|52c2805e-5744-482e-a068-59fe9b4d881e", "Total Volume");
			this.WD_TotalCubicCalcDropEdit.Decimals = 3;
			this.WD_TotalCubicCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 64, true);
			this.WD_TotalCubicCalcDropEdit.Name = "WD_TotalCubicCalcDropEdit";
			this.WD_TotalCubicCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.WD_TotalCubicCalcDropEdit.TabIndex = 10;
			// 
			// FinalizeReceiveCheckBox
			// 
			this.FinalizeReceiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FinalizeReceiveCheckBox, "WD_AutoFinaliseBOMIntoInventory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_AutoFinaliseBOMIntoInventory)));
			this.FinalizeReceiveCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DynamicWorkOrderEntryUserControl|25515459-8846-4965-aa15-2dd0247e64ef", "Automatically Finalize into Inventory");
			this.FinalizeReceiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 88, true);
			this.FinalizeReceiveCheckBox.Name = "FinalizeReceiveCheckBox";
			this.FinalizeReceiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.FinalizeReceiveCheckBox.TabIndex = 11;
			this.FinalizeReceiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// InwardProcessingCheckBox
			// 
			this.InwardProcessingCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InwardProcessingCheckBox, "WD_IsInwardsProcessingJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Warehouse.Transactions.Business.WhsDynamicWorkOrder)(null)).WD_IsInwardsProcessingJob)));
			this.InwardProcessingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 112, true);
			this.InwardProcessingCheckBox.Name = "InwardProcessingCheckBox";
			this.InwardProcessingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.InwardProcessingCheckBox.TabIndex = 12;
			this.InwardProcessingCheckBox.UseVisualStyleBackColor = true;
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailTabControl.Controls.Add(this.DetailTabPage);
			this.DetailTabControl.Controls.Add(this.LinesTabPage);
			this.DetailTabControl.Controls.Add(this.RelatedJobsTabPage);
			this.DetailTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 513, true);
			this.DetailTabControl.Name = "DetailTabControl";
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 513, true);
			this.DetailTabControl.TabIndex = 0;
			// 
			// DynamicWorkOrderEntryUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailTabControl);
			this.Name = "DynamicWorkOrderEntryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(815, 513, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RelatedJobsTabPage.ResumeLayout(false);
			this.RelatedJobsTabPage.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			this.workOrderDocketLinesGridUserControl2.ResumeLayout(true);
			this.workOrderDocketLinesGridUserControl2.PerformLayout();
			this.DetailTabPage.ResumeLayout(false);
			this.DetailTabPage.PerformLayout();
			this.DocketLinesGridControl.ResumeLayout(true);
			this.DocketLinesGridControl.PerformLayout();
			this.DetailTopPanel.ResumeLayout(false);
			this.DetailTopPanel.PerformLayout();
			this.OrgWhsFindControl.ResumeLayout(true);
			this.OrgWhsFindControl.PerformLayout();
			this.WarehouseFindBox.ResumeLayout(true);
			this.WarehouseFindBox.PerformLayout();
			this.OrderDetailsGroupBox.ResumeLayout(false);
			this.OrderDetailsGroupBox.PerformLayout();
			this.SubTypeDropEdit.ResumeLayout(true);
			this.SubTypeDropEdit.PerformLayout();
			this.PickOptionDropEdit.ResumeLayout(true);
			this.PickOptionDropEdit.PerformLayout();
			this.RequiredDateEdit.ResumeLayout(true);
			this.RequiredDateEdit.PerformLayout();
			this.WD_TotalWeightCalcDropEdit.ResumeLayout(true);
			this.WD_TotalWeightCalcDropEdit.PerformLayout();
			this.WD_TotalCubicCalcDropEdit.ResumeLayout(true);
			this.WD_TotalCubicCalcDropEdit.PerformLayout();
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#endregion
	}
}
