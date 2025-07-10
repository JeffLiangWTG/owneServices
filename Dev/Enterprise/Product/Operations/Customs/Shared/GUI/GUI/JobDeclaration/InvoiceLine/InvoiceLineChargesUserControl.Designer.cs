
namespace Enterprise.Customs.GUI
{
	partial class InvoiceLineChargesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ApportionedChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApportionedChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
			this.ChargesGrid.SuspendLayout();
			this.ApportionedChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IInvoicesProvider);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|040D8AFD-3027-42B5-9E27-269E0E797CC3", "Charges");
			this.ChargesGroupBox.Controls.Add(this.ChargesGrid);
			this.ChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 100, true);
			this.ChargesGroupBox.TabIndex = 5;
			this.ChargesGroupBox.TabStop = false;
			// 
			// ChargesGrid
			// 
			this.ChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargesGrid, "FilteredInvoiceLines.Charges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).NoOfDecimalsForPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_Percentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_IsIncludedInITOT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Charges)).SyncRoot)).J7_Calc_IsIncludedInInvoiceAmount)));
			this.ChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ChargeTypeList";
			zDropEditColumnStyleInfo1.ColumnName = "J7_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|f17021a7-5e2c-4d4a-8130-c4eacd8ae096", "Desc.", "Desc.", "Commercial Invoice Charge Description.");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(46);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "J7_Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.Currencies";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "J7_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zCheckBoxColumnStyleInfo1.ColumnName = "J7_IsDutiable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCheckBoxColumnStyleInfo2.ColumnName = "J7_IsGSTApplicable";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "NoOfDecimalsForPercentage";
			zCalcEditColumnStyleInfo2.ColumnName = "J7_Percentage";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|a75b4656-02ca-4f6e-90ce-e47716a88826", "Included In Line");
			zCheckBoxColumnStyleInfo3.ColumnName = "J7_IsIncludedInITOT";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|5a575cc1-9473-40f0-9097-3849b57769e3", "Included In Invoice", "Included In Invoice Total Amount.");
			zCheckBoxColumnStyleInfo4.ColumnName = "J7_Calc_IsIncludedInInvoiceAmount";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			this.ChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGrid.GridId = "dbffd3a1-3e5c-420d-9776-7dabdffce399";
			this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargesGrid.LayoutKey = "ChargesGrid";
			this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ChargesGrid.Name = "ChargesGrid";
			this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 81, true);
			this.ChargesGrid.TabIndex = 0;
			// 
			// ApportionedChargesGroupBox
			// 
			this.ApportionedChargesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|37D3A91B-D837-40EC-A455-399C7CCE5B21", "Apportioned Charges");
			this.ApportionedChargesGroupBox.Controls.Add(this.ApportionedChargesGrid);
			this.ApportionedChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionedChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApportionedChargesGroupBox.Name = "ApportionedChargesGroupBox";
			this.ApportionedChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 98, true);
			this.ApportionedChargesGroupBox.TabIndex = 7;
			this.ApportionedChargesGroupBox.TabStop = false;
			// 
			// ApportionedChargesGrid
			// 
			this.ApportionedChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ApportionedChargesGrid, "FilteredInvoiceLines.ApportionedCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).Lookups.ChargeTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).ChargeCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).Lookups.Currencies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_IsDutiable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_IsGSTApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_IsIncludedInITOT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseInvoiceLineApportionedCharge)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).ApportionedCharges)).SyncRoot)).J7_FullOrPartialApportionment)));
			this.ApportionedChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ChargeTypeList";
			zDropEditColumnStyleInfo2.ColumnName = "J7_ChargeType";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|5b5d6cf1-18cd-4028-ad1d-ec0881c2cf35", "Desc.", "Description", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(46);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "J7_Amount";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.Currencies";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "J7_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(44);
			zCheckBoxColumnStyleInfo5.ColumnName = "J7_IsDutiable";
			zCheckBoxColumnStyleInfo5.IsReadOnly = true;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zCheckBoxColumnStyleInfo6.ColumnName = "J7_IsGSTApplicable";
			zCheckBoxColumnStyleInfo6.IsReadOnly = true;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("InvoiceLineChargesUserControl|a4375997-6bda-4333-b29c-83ea270c3a60", "Included In Line");
			zCheckBoxColumnStyleInfo7.ColumnName = "J7_IsIncludedInITOT";
			zCheckBoxColumnStyleInfo7.IsReadOnly = true;
			zCheckBoxColumnStyleInfo7.IsVisible = false;
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			zTextBoxColumnStyleInfo3.ColumnName = "J7_FullOrPartialApportionment";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			this.ApportionedChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.ApportionedChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ApportionedChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApportionedChargesGrid.GridId = "9897fd88-ec01-4fb9-b3c7-f0e79001b057";
			this.ApportionedChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApportionedChargesGrid.LayoutKey = "ApportionedChargesGrid";
			this.ApportionedChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ApportionedChargesGrid.Name = "ApportionedChargesGrid";
			this.ApportionedChargesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ApportionedChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 79, true);
			this.ApportionedChargesGrid.TabIndex = 0;
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ChargesGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 202, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ApportionedChargesGroupBox);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.SplitContainer.TabIndex = 8;
			// 
			// InvoiceLineChargesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "InvoiceLineChargesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 202, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
			this.ChargesGrid.ResumeLayout(false);
			this.ChargesGrid.PerformLayout();
			this.ApportionedChargesGroupBox.ResumeLayout(false);
			this.ApportionedChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox ChargesGroupBox;
		public Enterprise.ZArchitecture.ZGrid ChargesGrid;
		public Enterprise.ZArchitecture.GUI.ZGroupBox ApportionedChargesGroupBox;
		public Enterprise.ZArchitecture.ZGrid ApportionedChargesGrid;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
	}
}
