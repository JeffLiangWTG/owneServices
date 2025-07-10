namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables
{
	partial class TermsUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TermsBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CyclesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ARTermsInstallmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsInstallmentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ARPaymentCycleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PaymentCycleGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ARTermsCycleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsCycleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TermsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsGrid)).BeginInit();
			this.TermsGrid.SuspendLayout();
			this.TermsBottomPanel.SuspendLayout();
			this.CyclesPanel.SuspendLayout();
			this.ARTermsInstallmentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsInstallmentGrid)).BeginInit();
			this.TermsInstallmentGrid.SuspendLayout();
			this.ARPaymentCycleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PaymentCycleGrid)).BeginInit();
			this.PaymentCycleGrid.SuspendLayout();
			this.ARTermsCycleGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsCycleGrid)).BeginInit();
			this.TermsCycleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCompanyData);
			// 
			// TermsGroupBox
			// 
			this.TermsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TermsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CreditControlAndSettlementControl|c2ed70c7-1c88-4d2a-8d7f-a902a939847b", "Terms and Term Days");
			this.TermsGroupBox.Controls.Add(this.TermsGrid);
			this.TermsGroupBox.Controls.Add(this.TermsBottomPanel);
			this.TermsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TermsGroupBox.Name = "TermsGroupBox";
			this.TermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 146, true);
			this.TermsGroupBox.TabIndex = 4;
			this.TermsGroupBox.TabStop = false;
			// 
			// TermsGrid
			// 
			this.TermsGrid.AllowNavigation = false;
			this.TermsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TermsGrid, "ARTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_GB_Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_GE_Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_InvoiceClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_InvoiceTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_InvoiceDays)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).PY_AgreedPaymentMethod)));
			this.TermsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9dbc500e-c20c-4cec-9385-eeb3f7119889", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "PY_JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fedc5fdc-13b2-41c9-bdf6-90357dadf676", "Branch");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PY_GB_Branch";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("545fd10a-3b55-436b-8b94-63b91c7a50ed", "Department");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "PY_GE_Department";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7cfbaafd-6826-4e6f-8c69-97b705ff4e60", "Direction");
			zDropEditColumnStyleInfo2.ColumnName = "PY_Direction";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a09372eb-dca0-43e4-85ca-f7c105b4a6f0", "Transport Mode");
			zDropEditColumnStyleInfo3.ColumnName = "PY_TransportMode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aa9e188b-4065-4474-90c1-c70c2f9dec77", "Invoice Type");
			zDropEditColumnStyleInfo4.ColumnName = "PY_InvoiceClass";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("912afca7-3ee8-49ba-ac22-05d5b1509a62", "Invoice Term");
			zDropEditColumnStyleInfo5.ColumnName = "PY_InvoiceTerm";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PY_InvoiceDays";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8e2d0a3e-1f1e-4fb4-beeb-0e2a02376c0f", "Agreed Payment Method");
			zDropEditColumnStyleInfo6.ColumnName = "PY_AgreedPaymentMethod";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TermsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TermsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TermsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.TermsGrid.GridId = "15948171-cc21-460d-b4a9-873ae6eeae80";
			this.TermsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsGrid.LayoutKey = "TermsGrid";
			this.TermsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.TermsGrid.Name = "TermsGrid";
			this.TermsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 83, true);
			this.TermsGrid.TabIndex = 0;
			// 
			// TermsBottomPanel
			// 
			this.TermsBottomPanel.Controls.Add(this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit);
			this.TermsBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TermsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.TermsBottomPanel.Name = "TermsBottomPanel";
			this.TermsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 40, true);
			this.TermsBottomPanel.TabIndex = 1;
			// 
			// OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit
			// 
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit, "Header.MiscServ.OM_ARTreatDisbursementsAsStandardValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).Header.MiscServ.OM_ARTreatDisbursementsAsStandardValue)));
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.CaptionResourceString = null;
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.DecimalPlaces = 2;
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 10, true);
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Name = "OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit";
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.TabIndex = 0;
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Text = "0";
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CyclesPanel
			// 
			this.CyclesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CyclesPanel.Controls.Add(this.ARTermsCycleGroupBox);
			this.CyclesPanel.Controls.Add(this.ARTermsInstallmentGroupBox);
			this.CyclesPanel.Controls.Add(this.ARPaymentCycleGroupBox);
			this.CyclesPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CyclesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 172, true);
			this.CyclesPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CyclesPanel.Name = "CyclesPanel";
			this.CyclesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 176, true);
			this.CyclesPanel.TabIndex = 6;
			// 
			// ARTermsInstallmentGroupBox
			//
			this.ARTermsInstallmentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ARTermsInstallmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dc667289-c511-49cf-b29f-865d9619509c", "Multiple Installments");
			this.ARTermsInstallmentGroupBox.Controls.Add(this.TermsInstallmentGrid);
			this.ARTermsInstallmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			this.ARTermsInstallmentGroupBox.Name = "ARTermsInstallmentGroupBox";
			this.ARTermsInstallmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 88, true);
			this.ARTermsInstallmentGroupBox.TabIndex = 5;
			this.ARTermsInstallmentGroupBox.TabStop = false;
			// 
			// TermsInstallmentGrid
			// 
			this.TermsInstallmentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TermsInstallmentGrid, "ARTerms.ARTermsInstallments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsInstallments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsInstallment)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsInstallments)).SyncRoot)).ML_SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsInstallment)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsInstallments)).SyncRoot)).ML_DaysFromInvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsInstallment)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsInstallments)).SyncRoot)).ML_SplitPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgARTermsInstallment)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsInstallments)).SyncRoot)).ML_AgreedPaymentMethod)));
			this.TermsInstallmentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("57b4155b-fc34-4367-bfce-bc064d1c252f", "Installment #");
			zCalcEditColumnStyleInfo2.ColumnName = "ML_SequenceNumber";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ac7a0f6c-bc2f-4067-bfac-328023bdca1b", "Days from Invoice Date");
			zCalcEditColumnStyleInfo3.ColumnName = "ML_DaysFromInvoiceDate";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a4c899aa-2e36-4ebb-982a-40c409808710", "% of Invoice Total Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "ML_SplitPercentage";
			zCalcEditColumnStyleInfo4.MaxValue = 100;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aa561206-f009-4e68-9c3d-2323e3ad3f89", "Agreed Payment Method");
			zDropEditColumnStyleInfo7.ColumnName = "ML_AgreedPaymentMethod";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsInstallmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TermsInstallmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TermsInstallmentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TermsInstallmentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.TermsInstallmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TermsInstallmentGrid.GridId = "96c5cdd9-61b8-43fc-ae34-10f4a513f724";
			this.TermsInstallmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsInstallmentGrid.LayoutKey = "TermsGrid";
			this.TermsInstallmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TermsInstallmentGrid.Name = "TermsInstallmentGrid";
			this.TermsInstallmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 69, true);
			this.TermsInstallmentGrid.TabIndex = 0;
			// 
			// ARPaymentCycleGroupBox
			// 
			this.ARPaymentCycleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("9c31534e-0d77-4e62-9bf1-acade01dd9b0", "Payment Cycle");
			this.ARPaymentCycleGroupBox.Controls.Add(this.PaymentCycleGrid);
			this.ARPaymentCycleGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ARPaymentCycleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.ARPaymentCycleGroupBox.Name = "ARPaymentCycleGroupBox";
			this.ARPaymentCycleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 88, true);
			this.ARPaymentCycleGroupBox.TabIndex = 6;
			this.ARPaymentCycleGroupBox.TabStop = false;
			// 
			// PaymentCycleGrid
			// 
			this.PaymentCycleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PaymentCycleGrid, "ARTerms.ARPaymentCycles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARPaymentCycles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARPaymentCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARPaymentCycles)).SyncRoot)).P5_Cycle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARPaymentCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARPaymentCycles)).SyncRoot)).P5_PaymentDay)));
			this.PaymentCycleGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "P5_Cycle";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "P5_PaymentDay";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			this.PaymentCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PaymentCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PaymentCycleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PaymentCycleGrid.GridId = "96c5cdd9-61b8-43fc-ae34-10f4a513f724";
			this.PaymentCycleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentCycleGrid.LayoutKey = "PaymentCycleGrid";
			this.PaymentCycleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PaymentCycleGrid.Name = "PaymentCycleGrid";
			this.PaymentCycleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 69, true);
			this.PaymentCycleGrid.TabIndex = 0;
			// 
			// ARTermsCycleGroupBox
			// 
			this.ARTermsCycleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CreditControlAndSettlementControl|c5bb8477-a400-4b84-82a3-5ea0436e8418", "Invoice Cycle");
			this.ARTermsCycleGroupBox.Controls.Add(this.TermsCycleGrid);
			this.ARTermsCycleGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ARTermsCycleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ARTermsCycleGroupBox.Name = "ARTermsCycleGroupBox";
			this.ARTermsCycleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 88, true);
			this.ARTermsCycleGroupBox.TabIndex = 4;
			this.ARTermsCycleGroupBox.TabStop = false;
			// 
			// TermsCycleGrid
			// 
			this.TermsCycleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TermsCycleGrid, "ARTerms.ARTermsCycles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsCycles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsCycles)).SyncRoot)).P5_FromDayCalculated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsCycles)).SyncRoot)).P5_ToDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgARTerms)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCompanyData)(null)).ARTerms)).SyncRoot)).ARTermsCycles)).SyncRoot)).P5_PaymentDay)));
			this.TermsCycleGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "P5_FromDayCalculated";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "P5_ToDay";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "P5_PaymentDay";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.TermsCycleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TermsCycleGrid.GridId = "96c5cdd9-61b8-43fc-ae34-10f4a513f724";
			this.TermsCycleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsCycleGrid.LayoutKey = "TermsGrid";
			this.TermsCycleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TermsCycleGrid.Name = "TermsCycleGrid";
			this.TermsCycleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(329, 69, true);
			this.TermsCycleGrid.TabIndex = 0;
			// 
			// TermsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CyclesPanel);
			this.Controls.Add(this.TermsGroupBox);
			this.Name = "TermsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 348, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TermsGroupBox.ResumeLayout(false);
			this.TermsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsGrid)).EndInit();
			this.TermsGrid.ResumeLayout(false);
			this.TermsGrid.PerformLayout();
			this.TermsBottomPanel.ResumeLayout(false);
			this.TermsBottomPanel.PerformLayout();
			this.CyclesPanel.ResumeLayout(false);
			this.CyclesPanel.PerformLayout();
			this.ARTermsInstallmentGroupBox.ResumeLayout(false);
			this.ARTermsInstallmentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsInstallmentGrid)).EndInit();
			this.TermsInstallmentGrid.ResumeLayout(false);
			this.TermsInstallmentGrid.PerformLayout();
			this.ARPaymentCycleGroupBox.ResumeLayout(false);
			this.ARPaymentCycleGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PaymentCycleGrid)).EndInit();
			this.PaymentCycleGrid.ResumeLayout(false);
			this.PaymentCycleGrid.PerformLayout();
			this.ARTermsCycleGroupBox.ResumeLayout(false);
			this.ARTermsCycleGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TermsCycleGrid)).EndInit();
			this.TermsCycleGrid.ResumeLayout(false);
			this.TermsCycleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TermsGroupBox;
		private ZArchitecture.ZGrid TermsGrid;
		private ZArchitecture.GUI.ZPanel TermsBottomPanel;
		private ZArchitecture.ZCalcEdit OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit;
		private ZArchitecture.GUI.ZPanel CyclesPanel;
		private ZArchitecture.GUI.ZGroupBox ARPaymentCycleGroupBox;
		private ZArchitecture.ZGrid PaymentCycleGrid;
		private ZArchitecture.GUI.ZGroupBox ARTermsCycleGroupBox;
		private ZArchitecture.ZGrid TermsCycleGrid;
		private ZArchitecture.GUI.ZGroupBox ARTermsInstallmentGroupBox;
		private ZArchitecture.ZGrid TermsInstallmentGrid;
	}
}
