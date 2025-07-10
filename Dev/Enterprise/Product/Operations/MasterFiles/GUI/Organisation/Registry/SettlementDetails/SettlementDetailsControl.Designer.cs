namespace Enterprise.MasterFiles.GUI
{
	partial class SettlementDetailsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoJobType = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfoBranch = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfoDepartment = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoDirection = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfoTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TermsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TermsBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TreatDisbursementsAsStandardValueBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CyclesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ARTermsCollection);
			// 
			// TermsGroupBox
			// 
			this.TermsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TermsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|40a9d59f-ab1a-47cb-b017-1e5ff5ae8c5c", "Terms and Term Days");
			this.TermsGroupBox.Controls.Add(this.TermsGrid);
			this.TermsGroupBox.Controls.Add(this.TermsBottomPanel);
			this.TermsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TermsGroupBox.Name = "TermsGroupBox";
			this.TermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 148, true);
			this.TermsGroupBox.TabIndex = 4;
			this.TermsGroupBox.TabStop = false;
			// 
			// TermsGrid
			// 
			this.TermsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TermsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ARTerms)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ARTerms)(null)).BranchPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ARTerms)(null)).DeptPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ARTerms)(null)).Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ARTerms)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ARTerms)(null)).InvoiceClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ARTerms)(null)).InvoiceTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARTerms)(null)).InvoiceDays)));
			this.TermsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfoJobType.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1818cf0c-6694-429e-bb4c-53269d9637ce", "Job Type");
			zDropEditColumnStyleInfoJobType.ColumnName = "JobType";
			zDropEditColumnStyleInfoJobType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfoBranch.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d0c3b08f-371c-4782-8fdc-f5ebb0f0f664", "Branch");
			zGuidFindBoxColumnStyleInfoBranch.ColumnName = "BranchPK";
			zGuidFindBoxColumnStyleInfoBranch.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfoDepartment.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a93e781d-faed-463a-8f6d-e7a99f12830a", "Dept.");
			zGuidFindBoxColumnStyleInfoDepartment.ColumnName = "DeptPK";
			zGuidFindBoxColumnStyleInfoDepartment.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfoDirection.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a79a6947-3f86-4dce-8d7c-30c41963c6c9", "Direction");
			zDropEditColumnStyleInfoDirection.ColumnName = "Direction";
			zDropEditColumnStyleInfoDirection.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfoTransportMode.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b908fa53-7a53-401f-bed5-3ac90fc20a60", "Transport Mode");
			zDropEditColumnStyleInfoTransportMode.ColumnName = "TransportMode";
			zDropEditColumnStyleInfoTransportMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|cadab7ef-6a53-4d8f-b155-fac37a8cf58d", "Invoice Type");
			zDropEditColumnStyleInfo4.ColumnName = "InvoiceClass";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|0d05d426-3f82-42eb-9b24-b259fdc2c120", "Invoice Term");
			zDropEditColumnStyleInfo5.ColumnName = "InvoiceTerm";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|612a9e61-7b0b-48c9-b40d-fe09d62d1a7d", "Days/Months");
			zCalcEditColumnStyleInfo1.ColumnName = "InvoiceDays";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoJobType);
			this.TermsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfoBranch);
			this.TermsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfoDepartment);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoDirection);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfoTransportMode);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TermsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.TermsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TermsGrid.CopySelectedRowsAllowed = true;
			this.TermsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TermsGrid.GridId = "3cd7f00a-5819-469a-82bb-07ef8b9d2549";
			this.TermsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsGrid.LayoutKey = "TermsGrid";
			this.TermsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TermsGrid.Name = "TermsGrid";
			this.TermsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 99, true);
			this.TermsGrid.TabIndex = 0;
			// 
			// TermsBottomPanel
			// 
			this.TermsBottomPanel.Controls.Add(this.TreatDisbursementsAsStandardValueBoundCalcEdit);
			this.TermsBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TermsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 115, true);
			this.TermsBottomPanel.Name = "TermsBottomPanel";
			this.TermsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 30, true);
			this.TermsBottomPanel.TabIndex = 1;
			// 
			// TreatDisbursementsAsStandardValueBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TreatDisbursementsAsStandardValueBoundCalcEdit, "TreatDisbursementsAsStandardValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARTerms)(null)).TreatDisbursementsAsStandardValue)));
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|3ba1e8cf-3024-47f6-bbe8-b3c0876a8cf2", "Treat Disbursements As Standard Under Value");
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.DecimalPlaces = 2;
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 5, true);
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.Name = "TreatDisbursementsAsStandardValueBoundCalcEdit";
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 17, true);
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.TabIndex = 0;
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.Text = "0";
			this.TreatDisbursementsAsStandardValueBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CyclesPanel
			// 
			this.CyclesPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CyclesPanel.Controls.Add(this.ARPaymentCycleGroupBox);
			this.CyclesPanel.Controls.Add(this.ARTermsCycleGroupBox);
			this.CyclesPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CyclesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 148, true);
			this.CyclesPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CyclesPanel.Name = "CyclesPanel";
			this.CyclesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 220, true);
			this.CyclesPanel.TabIndex = 6;
			// 
			// ARPaymentCycleGroupBox
			// 
			this.ARPaymentCycleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|d671159d-c354-4fbc-b075-90a8dc8404ab", "Payment Cycle");
			this.ARPaymentCycleGroupBox.Controls.Add(this.PaymentCycleGrid);
			this.ARPaymentCycleGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ARPaymentCycleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.ARPaymentCycleGroupBox.Name = "ARPaymentCycleGroupBox";
			this.ARPaymentCycleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 110, true);
			this.ARPaymentCycleGroupBox.TabIndex = 6;
			this.ARPaymentCycleGroupBox.TabStop = false;
			this.ARPaymentCycleGroupBox.Enter += new System.EventHandler(this.ARPaymentCycleGroupBox_Enter);
			// 
			// PaymentCycleGrid
			// 
			this.PaymentCycleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PaymentCycleGrid, "ARPaymentCycles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARPaymentCycles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARPaymentCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARPaymentCycles)).SyncRoot)).ToDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARPaymentCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARPaymentCycles)).SyncRoot)).PaymentDay)));
			this.PaymentCycleGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|12f1d3d0-48a5-4497-a727-64c1e02b7f55", "Cycle #");
			zCalcEditColumnStyleInfo2.ColumnName = "ToDay";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|41814f84-3da7-4ad1-8dc6-edd82b35d830", "Payment Day");
			zCalcEditColumnStyleInfo3.ColumnName = "PaymentDay";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PaymentCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PaymentCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PaymentCycleGrid.CopySelectedRowsAllowed = true;
			this.PaymentCycleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PaymentCycleGrid.GridId = "5834f5c7-8af2-45c6-8a8b-7dd3b84b8539";
			this.PaymentCycleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentCycleGrid.LayoutKey = "PaymentCycleGrid";
			this.PaymentCycleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PaymentCycleGrid.Name = "PaymentCycleGrid";
			this.PaymentCycleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 91, true);
			this.PaymentCycleGrid.TabIndex = 0;
			// 
			// ARTermsCycleGroupBox
			// 
			this.ARTermsCycleGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|4b051311-03b4-4789-811d-6ee3f0533e80", "Invoice Cycle");
			this.ARTermsCycleGroupBox.Controls.Add(this.TermsCycleGrid);
			this.ARTermsCycleGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ARTermsCycleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ARTermsCycleGroupBox.Name = "ARTermsCycleGroupBox";
			this.ARTermsCycleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 109, true);
			this.ARTermsCycleGroupBox.TabIndex = 4;
			this.ARTermsCycleGroupBox.TabStop = false;
			// 
			// TermsCycleGrid
			// 
			this.TermsCycleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TermsCycleGrid, "ARTermsCycles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARTermsCycles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARTermsCycles)).SyncRoot)).FromDayCalculated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARTermsCycles)).SyncRoot)).ToDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ARTermsCycle)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ARTerms)(null)).ARTermsCycles)).SyncRoot)).PaymentDay)));
			this.TermsCycleGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|505710a7-6a91-4865-9d7e-4025500ae9dd", "From Day");
			zCalcEditColumnStyleInfo4.ColumnName = "FromDayCalculated";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|6abb0920-88ef-4ff1-bfd0-6040b6746a0a", "To Day");
			zCalcEditColumnStyleInfo5.ColumnName = "ToDay";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SettlementDetailsControl|e1a36e38-3a42-496d-9210-bccce1457b38", "Payment Day");
			zCalcEditColumnStyleInfo6.ColumnName = "PaymentDay";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.TermsCycleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.TermsCycleGrid.CopySelectedRowsAllowed = true;
			this.TermsCycleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TermsCycleGrid.GridId = "781316e7-d653-4dca-bb3e-cddcfb83c883";
			this.TermsCycleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsCycleGrid.LayoutKey = "TermsGrid";
			this.TermsCycleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TermsCycleGrid.Name = "TermsCycleGrid";
			this.TermsCycleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 90, true);
			this.TermsCycleGrid.TabIndex = 0;
			// 
			// SettlementDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CyclesPanel);
			this.Controls.Add(this.TermsGroupBox);
			this.Name = "SettlementDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 368, true);
			this.Load += new System.EventHandler(this.SettlementDetailsControl_Load);
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
		internal ZArchitecture.ZGrid TermsGrid;
		private ZArchitecture.GUI.ZPanel TermsBottomPanel;
		private ZArchitecture.ZCalcEdit TreatDisbursementsAsStandardValueBoundCalcEdit;
		private ZArchitecture.GUI.ZPanel CyclesPanel;
		private ZArchitecture.GUI.ZGroupBox ARPaymentCycleGroupBox;
		private ZArchitecture.ZGrid PaymentCycleGrid;
		private ZArchitecture.GUI.ZGroupBox ARTermsCycleGroupBox;
		private ZArchitecture.ZGrid TermsCycleGrid;

	}
}
