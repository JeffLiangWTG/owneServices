namespace Enterprise.Customs.TW.GUI
{
	partial class BondDetailsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BondDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MontlyReportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UniformInvoicesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GovermentUniformInvoiceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MontlyReportLeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CEI_WHSTradeReferenceNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CEI_WHSMonthTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RelatedBondedPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BondedFactoryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopRightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CEI_BillOfMaterialsPageNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CEI_BillOfMaterialsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CEI_DutyRefundCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CEI_ReasonForDutyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BondedWarehousePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToBondedWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ToWMSCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ToBondedWarehouseVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWMSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWHAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FormBondedWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWMSCodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromBondedWarehouseVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWHAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FromWMSCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondDetailsPanel.SuspendLayout();
			this.MontlyReportGroupBox.SuspendLayout();
			this.UniformInvoicesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GovermentUniformInvoiceGrid)).BeginInit();
			this.GovermentUniformInvoiceGrid.SuspendLayout();
			this.MontlyReportLeftPanel.SuspendLayout();
			this.RelatedBondedPartiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BondedFactoryGrid)).BeginInit();
			this.BondedFactoryGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.TopRightGroupBox.SuspendLayout();
			this.CEI_ReasonForDutyDropEdit.SuspendLayout();
			this.BondedWarehousePanel.SuspendLayout();
			this.ToBondedWarehouseGroupBox.SuspendLayout();
			this.ToWMSCodeTypeDropEdit.SuspendLayout();
			this.ToWHAddressControl.SuspendLayout();
			this.FormBondedWarehouseGroupBox.SuspendLayout();
			this.FromWMSCodeTypeDropEdit.SuspendLayout();
			this.FromWHAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.JobDeclaration);
			// 
			// BondDetailsPanel
			// 
			this.BondDetailsPanel.AutoScroll = true;
			this.BondDetailsPanel.Controls.Add(this.MontlyReportGroupBox);
			this.BondDetailsPanel.Controls.Add(this.RelatedBondedPartiesGroupBox);
			this.BondDetailsPanel.Controls.Add(this.TopPanel);
			this.BondDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BondDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondDetailsPanel.Name = "BondDetailsPanel";
			this.BondDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 560, true);
			this.BondDetailsPanel.TabIndex = 0;
			// 
			// MontlyReportGroupBox
			// 
			this.MontlyReportGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8b737500-b6b2-4f12-ba47-f5e8e3c568bf", "Monthly Report");
			this.MontlyReportGroupBox.Controls.Add(this.UniformInvoicesGroupBox);
			this.MontlyReportGroupBox.Controls.Add(this.MontlyReportLeftPanel);
			this.MontlyReportGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MontlyReportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 395, true);
			this.MontlyReportGroupBox.Name = "MontlyReportGroupBox";
			this.MontlyReportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 165, true);
			this.MontlyReportGroupBox.TabIndex = 2;
			this.MontlyReportGroupBox.TabStop = false;
			// 
			// UniformInvoicesGroupBox
			// 
			this.UniformInvoicesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8fc74060-8fd8-4c52-b21c-c76289a4edf6", "Uniform Invoices");
			this.UniformInvoicesGroupBox.Controls.Add(this.GovermentUniformInvoiceGrid);
			this.UniformInvoicesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UniformInvoicesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 16, true);
			this.UniformInvoicesGroupBox.Name = "UniformInvoicesGroupBox";
			this.UniformInvoicesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 146, true);
			this.UniformInvoicesGroupBox.TabIndex = 2;
			this.UniformInvoicesGroupBox.TabStop = false;
			// 
			// GovermentUniformInvoiceGrid
			// 
			this.GovermentUniformInvoiceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GovermentUniformInvoiceGrid, "GovernmentUniformInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).GovernmentUniformInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.GovernmentUniformInvoiceData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).GovernmentUniformInvoices)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.GovernmentUniformInvoiceData)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).GovernmentUniformInvoices)).SyncRoot)).Amount)));
			this.GovermentUniformInvoiceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.GovermentUniformInvoiceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GovermentUniformInvoiceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.GovermentUniformInvoiceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GovermentUniformInvoiceGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
			this.GovermentUniformInvoiceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GovermentUniformInvoiceGrid.LayoutKey = "GovermentUniformInvoiceGrid";
			this.GovermentUniformInvoiceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.GovermentUniformInvoiceGrid.Name = "GovermentUniformInvoiceGrid";
			this.GovermentUniformInvoiceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 127, true);
			this.GovermentUniformInvoiceGrid.TabIndex = 2;
			// 
			// MontlyReportLeftPanel
			// 
			this.MontlyReportLeftPanel.Controls.Add(this.CEI_WHSTradeReferenceNoTextBox);
			this.MontlyReportLeftPanel.Controls.Add(this.CEI_WHSMonthTextBox);
			this.MontlyReportLeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.MontlyReportLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MontlyReportLeftPanel.Name = "MontlyReportLeftPanel";
			this.MontlyReportLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 146, true);
			this.MontlyReportLeftPanel.TabIndex = 13;
			// 
			// TW_WHSTradeReferenceNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CEI_WHSTradeReferenceNoTextBox, "CustomsEntryInstructions.CEI_WHSTradeReferenceNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_WHSTradeReferenceNo)));
			this.CEI_WHSTradeReferenceNoTextBox.CaptionResourceString = null;
			this.CEI_WHSTradeReferenceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 10, true);
			this.CEI_WHSTradeReferenceNoTextBox.Name = "CEI_WHSTradeReferenceNoTextBox";
			this.CEI_WHSTradeReferenceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.CEI_WHSTradeReferenceNoTextBox.TabIndex = 0;
			// 
			// TW_WHSMonthTextBox
			// 
			this.BindingSource.SetBindingMember(this.CEI_WHSMonthTextBox, "CustomsEntryInstructions.CEI_WHSMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_WHSMonth)));
			this.CEI_WHSMonthTextBox.CaptionResourceString = null;
			this.CEI_WHSMonthTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 36, true);
			this.CEI_WHSMonthTextBox.Name = "CEI_WHSMonthTextBox";
			this.CEI_WHSMonthTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.CEI_WHSMonthTextBox.TabIndex = 1;
			// 
			// RelatedBondedPartiesGroupBox
			// 
			this.RelatedBondedPartiesGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("1de3070a-96cf-4dd5-8833-01b36ef5f4ca", "Related Bonded Parties");
			this.RelatedBondedPartiesGroupBox.Controls.Add(this.BondedFactoryGrid);
			this.RelatedBondedPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RelatedBondedPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 194, true);
			this.RelatedBondedPartiesGroupBox.Name = "RelatedBondedPartiesGroupBox";
			this.RelatedBondedPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 201, true);
			this.RelatedBondedPartiesGroupBox.TabIndex = 1;
			this.RelatedBondedPartiesGroupBox.TabStop = false;
			// 
			// BondedFactoryGrid
			// 
			this.BondedFactoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BondedFactoryGrid, "BondedFactories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Business.BondedFactory)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Business.BondedFactory)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)).SyncRoot)).E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.BondedFactory)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)).SyncRoot)).BondedID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.BondedFactory)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)).SyncRoot)).BondedIDTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.BondedFactory)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).BondedFactories)).SyncRoot)).BondedIDTypeDescription)));
			this.BondedFactoryGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zAddressDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zTextBoxColumnStyleInfo2.ColumnName = "BondedID";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("fad796ff-bba6-4dc9-ad94-d2310a4e9daa", "Bonded ID");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "BondedIDTypeCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("fad796ff-bba6-4dc9-ad94-d2310a4e9daa", "Bonded ID");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "BondedIDTypeDescription";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Customs.TW.GUI.Res.GetData("fad796ff-bba6-4dc9-ad94-d2310a4e9daa", "Bonded ID");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			this.BondedFactoryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BondedFactoryGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.BondedFactoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.BondedFactoryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BondedFactoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BondedFactoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BondedFactoryGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6b12";
			this.BondedFactoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BondedFactoryGrid.LayoutKey = "BondedFactoryGrid";
			this.BondedFactoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BondedFactoryGrid.Name = "BondedFactoryGrid";
			this.BondedFactoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 182, true);
			this.BondedFactoryGrid.TabIndex = 2;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.TopRightGroupBox);
			this.TopPanel.Controls.Add(this.BondedWarehousePanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 194, true);
			this.TopPanel.TabIndex = 0;
			// 
			// TopRightGroupBox
			// 
			this.TopRightGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f61e5695-30d7-492f-ad08-e05ccd3e85a8", "Materials");
			this.TopRightGroupBox.Controls.Add(this.CEI_BillOfMaterialsPageNoCalcEdit);
			this.TopRightGroupBox.Controls.Add(this.CEI_BillOfMaterialsCheckBox);
			this.TopRightGroupBox.Controls.Add(this.CEI_DutyRefundCheckBox);
			this.TopRightGroupBox.Controls.Add(this.CEI_ReasonForDutyDropEdit);
			this.TopRightGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopRightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 0, true);
			this.TopRightGroupBox.Name = "TopRightGroupBox";
			this.TopRightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 194, true);
			this.TopRightGroupBox.TabIndex = 1;
			this.TopRightGroupBox.TabStop = false;
			// 
			// TW_BillOfMaterialsPageNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CEI_BillOfMaterialsPageNoCalcEdit, "CustomsEntryInstructions.CEI_BOMPageCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BOMPageCount)));
			this.CEI_BillOfMaterialsPageNoCalcEdit.CaptionResourceString = null;
			this.CEI_BillOfMaterialsPageNoCalcEdit.DecimalPlaces = 0;
			this.CEI_BillOfMaterialsPageNoCalcEdit.Decimals = 0;
			this.CEI_BillOfMaterialsPageNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 69, true);
			this.CEI_BillOfMaterialsPageNoCalcEdit.Name = "CEI_BillOfMaterialsPageNoCalcEdit";
			this.CEI_BillOfMaterialsPageNoCalcEdit.ShowGroupSeparators = false;
			this.CEI_BillOfMaterialsPageNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 20, true);
			this.CEI_BillOfMaterialsPageNoCalcEdit.TabIndex = 2;
			this.CEI_BillOfMaterialsPageNoCalcEdit.Text = "0";
			this.CEI_BillOfMaterialsPageNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TW_BillOfMaterialsCheckBox
			// 
			this.CEI_BillOfMaterialsCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.CEI_BillOfMaterialsCheckBox, "CustomsEntryInstructions.CEI_BillOfMaterials");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BillOfMaterials)));
			this.CEI_BillOfMaterialsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CEI_BillOfMaterialsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CEI_BillOfMaterialsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 46, true);
			this.CEI_BillOfMaterialsCheckBox.Name = "CEI_BillOfMaterialsCheckBox";
			this.CEI_BillOfMaterialsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 17, true);
			this.CEI_BillOfMaterialsCheckBox.TabIndex = 1;
			this.CEI_BillOfMaterialsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CEI_BillOfMaterialsCheckBox.UseVisualStyleBackColor = true;
			// 
			// TW_DutyRefundCheckBox
			// 
			this.CEI_DutyRefundCheckBox.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.CEI_DutyRefundCheckBox, "CustomsEntryInstructions.CEI_DutyRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DutyRefund)));
			this.CEI_DutyRefundCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CEI_DutyRefundCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CEI_DutyRefundCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 19, true);
			this.CEI_DutyRefundCheckBox.Name = "CEI_DutyRefundCheckBox";
			this.CEI_DutyRefundCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.CEI_DutyRefundCheckBox.TabIndex = 0;
			this.CEI_DutyRefundCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CEI_DutyRefundCheckBox.UseVisualStyleBackColor = true;
			// 
			// TW_ReasonForDutyDropEdit
			// 
			this.CEI_ReasonForDutyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CEI_ReasonForDutyDropEdit, "CustomsEntryInstructions.CEI_ReasonForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_ReasonForDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Lookups.ReasonforDutyList)));
			this.CEI_ReasonForDutyDropEdit.BindToList = "CustomsEntryInstructions.Lookups.ReasonforDutyList";
			this.CEI_ReasonForDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 19, true);
			this.CEI_ReasonForDutyDropEdit.Name = "CEI_ReasonForDutyDropEdit";
			this.CEI_ReasonForDutyDropEdit.PreBoundMaxLength = 3;
			this.CEI_ReasonForDutyDropEdit.ShouldResizeByMaxLength = true;
			this.CEI_ReasonForDutyDropEdit.ShowDescriptionBox = false;
			this.CEI_ReasonForDutyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CEI_ReasonForDutyDropEdit.TabIndex = 3;
			// 
			// BondedWarehousePanel
			// 
			this.BondedWarehousePanel.Controls.Add(this.ToBondedWarehouseGroupBox);
			this.BondedWarehousePanel.Controls.Add(this.FormBondedWarehouseGroupBox);
			this.BondedWarehousePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.BondedWarehousePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondedWarehousePanel.Name = "BondedWarehousePanel";
			this.BondedWarehousePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 194, true);
			this.BondedWarehousePanel.TabIndex = 2;
			// 
			// ToBondedWarehouseGroupBox
			// 
			this.ToBondedWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("cba1f08b-2780-4026-ab20-6eda1ff6e33c", "To Bonded Warehouse");
			this.ToBondedWarehouseGroupBox.Controls.Add(this.ToWMSCodeTypeDropEdit);
			this.ToBondedWarehouseGroupBox.Controls.Add(this.ToBondedWarehouseVATTextBox);
			this.ToBondedWarehouseGroupBox.Controls.Add(this.ToWMSCodeTextBox);
			this.ToBondedWarehouseGroupBox.Controls.Add(this.ToWHAddressControl);
			this.ToBondedWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToBondedWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 97, true);
			this.ToBondedWarehouseGroupBox.Name = "ToBondedWarehouseGroupBox";
			this.ToBondedWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 97, true);
			this.ToBondedWarehouseGroupBox.TabIndex = 1;
			this.ToBondedWarehouseGroupBox.TabStop = false;
			// 
			// ToWMSCodeTypeDropEdit
			// 
			this.ToWMSCodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWMSCodeTypeDropEdit, "CustomsEntryInstructions.ToWarehouseCodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCodeType)));
			this.ToWMSCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 45, true);
			this.ToWMSCodeTypeDropEdit.Name = "ToWMSCodeTypeDropEdit";
			this.ToWMSCodeTypeDropEdit.PreBoundMaxLength = 3;
			this.ToWMSCodeTypeDropEdit.ShouldResizeByMaxLength = true;
			this.ToWMSCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.ToWMSCodeTypeDropEdit.TabIndex = 6;
			// 
			// ToBondedWarehouseVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToBondedWarehouseVATTextBox, "CustomsEntryInstructions.ToWarehouseVATCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseVATCode)));
			this.ToBondedWarehouseVATTextBox.CaptionResourceString = null;
			this.ToBondedWarehouseVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 71, true);
			this.ToBondedWarehouseVATTextBox.Name = "ToBondedWarehouseVATTextBox";
			this.ToBondedWarehouseVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ToBondedWarehouseVATTextBox.TabIndex = 8;
			// 
			// ToWMSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWMSCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWMSCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d012b95a-53fa-449c-a81a-2d52b49332aa", "Bonded ID Code");
			this.ToWMSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 45, true);
			this.ToWMSCodeTextBox.Name = "ToWMSCodeTextBox";
			this.ToWMSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.ToWMSCodeTextBox.TabIndex = 7;
			// 
			// ToWHAddressControl
			// 
			this.ToWHAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWHAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
			this.ToWHAddressControl.BindToOrgList = "CustomsEntryInstructions.Lookups.OrganisationsFindBoxCollection";
			this.ToWHAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 19, true);
			this.ToWHAddressControl.Name = "ToWHAddressControl";
			this.ToWHAddressControl.PopupCaption = "";
			this.ToWHAddressControl.ReadOnly = false;
			this.ToWHAddressControl.ShowAddress = false;
			this.ToWHAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ToWHAddressControl.TabIndex = 4;
			// 
			// FormBondedWarehouseGroupBox
			// 
			this.FormBondedWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("da9a783d-88bf-49f2-a418-9128aa89dcf9", "From Bonded Warehouse");
			this.FormBondedWarehouseGroupBox.Controls.Add(this.FromWMSCodeTypeDropEdit);
			this.FormBondedWarehouseGroupBox.Controls.Add(this.FromBondedWarehouseVATTextBox);
			this.FormBondedWarehouseGroupBox.Controls.Add(this.FromWHAddressControl);
			this.FormBondedWarehouseGroupBox.Controls.Add(this.FromWMSCodeTextBox);
			this.FormBondedWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FormBondedWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FormBondedWarehouseGroupBox.Name = "FormBondedWarehouseGroupBox";
			this.FormBondedWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 97, true);
			this.FormBondedWarehouseGroupBox.TabIndex = 0;
			this.FormBondedWarehouseGroupBox.TabStop = false;
			// 
			// FromWMSCodeTypeDropEdit
			// 
			this.FromWMSCodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWMSCodeTypeDropEdit, "CustomsEntryInstructions.FromWarehouseCodeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCodeType)));
			this.FromWMSCodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 45, true);
			this.FromWMSCodeTypeDropEdit.Name = "FromWMSCodeTypeDropEdit";
			this.FromWMSCodeTypeDropEdit.PreBoundMaxLength = 3;
			this.FromWMSCodeTypeDropEdit.ShouldResizeByMaxLength = true;
			this.FromWMSCodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 20, true);
			this.FromWMSCodeTypeDropEdit.TabIndex = 4;
			// 
			// FromBondedWarehouseVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromBondedWarehouseVATTextBox, "CustomsEntryInstructions.FromWarehouseVATCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseVATCode)));
			this.FromBondedWarehouseVATTextBox.CaptionResourceString = null;
			this.FromBondedWarehouseVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 71, true);
			this.FromBondedWarehouseVATTextBox.Name = "FromBondedWarehouseVATTextBox";
			this.FromBondedWarehouseVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.FromBondedWarehouseVATTextBox.TabIndex = 6;
			// 
			// FromWHAddressControl
			// 
			this.FromWHAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWHAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWHAddressControl.BindToOrgList = "CustomsEntryInstructions.Lookups.OrganisationsFindBoxCollection";
			this.FromWHAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 19, true);
			this.FromWHAddressControl.Name = "FromWHAddressControl";
			this.FromWHAddressControl.PopupCaption = "";
			this.FromWHAddressControl.ReadOnly = false;
			this.FromWHAddressControl.ShowAddress = false;
			this.FromWHAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.FromWHAddressControl.TabIndex = 3;
			// 
			// FromWMSCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWMSCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWMSCodeTextBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("d0a75ad3-124f-4c35-8816-6d384d0a5906", "Bonded ID Code");
			this.FromWMSCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(236, 45, true);
			this.FromWMSCodeTextBox.Name = "FromWMSCodeTextBox";
			this.FromWMSCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.FromWMSCodeTextBox.TabIndex = 5;
			// 
			// BondDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BondDetailsPanel);
			this.Name = "BondDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 560, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondDetailsPanel.ResumeLayout(false);
			this.BondDetailsPanel.PerformLayout();
			this.MontlyReportGroupBox.ResumeLayout(false);
			this.MontlyReportGroupBox.PerformLayout();
			this.UniformInvoicesGroupBox.ResumeLayout(false);
			this.UniformInvoicesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GovermentUniformInvoiceGrid)).EndInit();
			this.GovermentUniformInvoiceGrid.ResumeLayout(false);
			this.GovermentUniformInvoiceGrid.PerformLayout();
			this.MontlyReportLeftPanel.ResumeLayout(false);
			this.MontlyReportLeftPanel.PerformLayout();
			this.RelatedBondedPartiesGroupBox.ResumeLayout(false);
			this.RelatedBondedPartiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BondedFactoryGrid)).EndInit();
			this.BondedFactoryGrid.ResumeLayout(false);
			this.BondedFactoryGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.TopRightGroupBox.ResumeLayout(false);
			this.TopRightGroupBox.PerformLayout();
			this.CEI_ReasonForDutyDropEdit.ResumeLayout(true);
			this.CEI_ReasonForDutyDropEdit.PerformLayout();
			this.BondedWarehousePanel.ResumeLayout(false);
			this.BondedWarehousePanel.PerformLayout();
			this.ToBondedWarehouseGroupBox.ResumeLayout(false);
			this.ToBondedWarehouseGroupBox.PerformLayout();
			this.ToWMSCodeTypeDropEdit.ResumeLayout(true);
			this.ToWMSCodeTypeDropEdit.PerformLayout();
			this.ToWHAddressControl.ResumeLayout(true);
			this.ToWHAddressControl.PerformLayout();
			this.FormBondedWarehouseGroupBox.ResumeLayout(false);
			this.FormBondedWarehouseGroupBox.PerformLayout();
			this.FromWMSCodeTypeDropEdit.ResumeLayout(true);
			this.FromWMSCodeTypeDropEdit.PerformLayout();
			this.FromWHAddressControl.ResumeLayout(true);
			this.FromWHAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BondDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox MontlyReportGroupBox;
		private ZArchitecture.GUI.ZGroupBox RelatedBondedPartiesGroupBox;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZGroupBox TopRightGroupBox;
		private ZArchitecture.GUI.ZGroupBox FormBondedWarehouseGroupBox;
		private ZArchitecture.ZTextBox CEI_WHSMonthTextBox;
		private ZArchitecture.ZTextBox CEI_WHSTradeReferenceNoTextBox;
		private ZArchitecture.GUI.ZGroupBox UniformInvoicesGroupBox;
		private ZArchitecture.GUI.ZPanel MontlyReportLeftPanel;
		public ZArchitecture.ZGrid GovermentUniformInvoiceGrid;
		private ZArchitecture.ZGrid BondedFactoryGrid;
		private ZArchitecture.ZCalcEdit CEI_BillOfMaterialsPageNoCalcEdit;
		private ZArchitecture.GUI.ZCheckBox CEI_BillOfMaterialsCheckBox;
		private ZArchitecture.GUI.ZCheckBox CEI_DutyRefundCheckBox;
		private ZArchitecture.GUI.ZDropEdit CEI_ReasonForDutyDropEdit;
		private ZArchitecture.GUI.ZPanel BondedWarehousePanel;
		private ZArchitecture.GUI.ZGroupBox ToBondedWarehouseGroupBox;
		private ZArchitecture.GUI.ZAddressControl FromWHAddressControl;
		private ZArchitecture.ZTextBox FromWMSCodeTextBox;
		private ZArchitecture.GUI.ZAddressControl ToWHAddressControl;
		private ZArchitecture.ZTextBox ToWMSCodeTextBox;
		private ZArchitecture.ZTextBox ToBondedWarehouseVATTextBox;
		private ZArchitecture.ZTextBox FromBondedWarehouseVATTextBox;
		private ZArchitecture.GUI.ZDropEdit FromWMSCodeTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ToWMSCodeTypeDropEdit;
	}
}
