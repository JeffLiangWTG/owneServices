namespace Enterprise.Customs.NO.GUI;

partial class BaseInvoiceLineUserControl
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
		this.JI_LinePriceInLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.JI_Calc_DutyAmountIncludingWHEstimateControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.TotalOtherChargesInNOKControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.TotalDeductionsInNOKControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.TotalExciseDutiesControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
		this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.SupportingDocumentsUserControl = new Enterprise.Customs.NO.GUI.SupportingDocumentsUserControl();
		this.PackagesPivotTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.PackagesPivotUserControl = new Enterprise.Customs.GUI.BaseLineLevelPackingPivotControl();
		this.InvoiceLinesSummaryGroupBox.SuspendLayout();
		this.BottomPanel.SuspendLayout();
		this.TopPanel.SuspendLayout();
		this.LineDetailTabControl.SuspendLayout();
		this.InvoiceDetailsGroupBox.SuspendLayout();
		this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
		this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
		this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
		this.ClassificationDetailsGroupBox.SuspendLayout();
		this.LineChargesTabPage.SuspendLayout();
		this.CurrentInvoicePanel.SuspendLayout();
		this.LineSummaryPanel.SuspendLayout();
		this.ContainersTabPage.SuspendLayout();
		this.ContainersGroupBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
		this.CusContainerInvoiceLineGrid.SuspendLayout();
		this.LineDetailsTabPage.SuspendLayout();
		this.NewLineDetailsTabPage.SuspendLayout();
		this.InvoiceLineDetailsUserControl.SuspendLayout();
		this.ClassificationPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
		this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
		this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
		this.CustomsQuantityCalcDropEdit.SuspendLayout();
		this.VolumeCalcDropEdit.SuspendLayout();
		this.JI_WeightCalcDropEdit.SuspendLayout();
		this.InvoiceQuantityCalcDropEdit.SuspendLayout();
		this.JI_DescriptionBoundTextBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.JI_LinePriceInLocalCurrencyControl.SuspendLayout();
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.SuspendLayout();
		this.TotalOtherChargesInNOKControl.SuspendLayout();
		this.TotalDeductionsInNOKControl.SuspendLayout();
		this.TotalExciseDutiesControl.SuspendLayout();
		this.SupportingDocumentsTabPage.SuspendLayout();
		this.SupportingDocumentsUserControl.SuspendLayout();
		this.PackagesPivotTabPage.SuspendLayout();
		this.PackagesPivotUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// InvoiceLinesSummaryGroupBox
		// 
		this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(895, 0, true);
		this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 541, true);
		// 
		// BottomPanel
		// 
		this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 180, true);
		this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 381, true);
		this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 541, true);
		// 
		// TopPanel
		// 
		this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
		this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 671, true);
		// 
		// LineDetailTabControl
		// 
		this.LineDetailTabControl.Controls.Add(this.PackagesPivotTabPage);
		this.LineDetailTabControl.Controls.Add(this.SupportingDocumentsTabPage);
		this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 541, true);
		this.LineDetailTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
		this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
		this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
		this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
		this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
		this.LineDetailTabControl.Controls.SetChildIndex(this.PackagesPivotTabPage, 0);
		// 
		// InvoiceDetailsGroupBox
		// 
		this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 420, true);
		this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 96, true);
		// 
		// LineChargesTabPage
		// 
		this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		// 
		// LineSummaryPanel
		// 
		this.LineSummaryPanel.Controls.Add(this.TotalDeductionsInNOKControl);
		this.LineSummaryPanel.Controls.Add(this.TotalExciseDutiesControl);
		this.LineSummaryPanel.Controls.Add(this.TotalOtherChargesInNOKControl);
		this.LineSummaryPanel.Controls.Add(this.JI_Calc_DutyAmountIncludingWHEstimateControl);
		this.LineSummaryPanel.Controls.Add(this.JI_LinePriceInLocalCurrencyControl);
		this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 433, true);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_LinePriceInLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyAmountIncludingWHEstimateControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.TotalOtherChargesInNOKControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.TotalExciseDutiesControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.TotalDeductionsInNOKControl, 0);
		this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
		// 
		// PendingApportionmentLabel
		// 
		this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 520, true);
		// 
		// ContainersTabPage
		// 
		this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		// 
		// ContainersGroupBox
		// 
		this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		// 
		// CusContainerInvoiceLineGrid
		// 
		this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 500, true);
		// 
		// CantCreateInvoiceLinesLabel
		// 
		this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
		this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 671, true);
		// 
		// LineDetailsTabPage
		// 
		this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		// 
		// NewLineDetailsTabPage
		// 
		this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.NewLineDetailsTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1052, 518, true);
		this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		// 
		// InvoiceLineDetailsUserControl
		// 
		this.InvoiceLineDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
		this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 339, true);
		// 
		// CustomsInvoiceLinesBoundGrid
		// 
		this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 671, true);
		// 
		// JI_Calc_CIFConvertToLocalCurrencyControl
		// 
		this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_CIF_InLocalCurrency";
		this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.JI_Calc_CIFConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("D4B9D39B-5B0F-77A4-424B-F95819903362", "CIF Value", "CIF value for current item line. CIF value is invoice line value plus(/minus) cha" +
	"rges. It is value at time of border crossing, and that is the basis for calculat" +
	"ing customs and excise duties.");
		this.JI_Calc_CIFConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
		this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 126, true);
		this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 10;
		// 
		// JI_Calc_InsuranceConvertToLocalCurrencyControl
		// 
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_InsuranceInLocalCurrency";
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("E97C40FA-B8E4-B3AD-4E68-C31E92A07B36", "Insurance", "Insurance charges for current line item. (On invoices is in foreign currencies mi" +
	"nor rounding issues may be seen.)");
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
		// 
		// JI_Calc_FreightConvertToLocalCurrencyControl
		// 
		this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_FreightInLocalCurrency";
		this.JI_Calc_FreightConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.JI_Calc_FreightConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("1D75659B-977B-9189-4962-13785EF8ECE4", "Freight", "Freight Charges for current line item. (On invoices is in foreign currencies mino" +
	"r rounding issues may be seen.)");
		this.JI_Calc_FreightConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
		// 
		// JI_Calc_GSTConvertToLocalCurrencyControl
		// 
		this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 192, true);
		// 
		// Splitter
		// 
		this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
		this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 10, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobDeclaration);
		// 
		// JI_LinePriceInLocalCurrencyControl
		// 
		this.JI_LinePriceInLocalCurrencyControl.AllowDrop = true;
		this.JI_LinePriceInLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_LinePriceInLocalCurrency";
		this.JI_LinePriceInLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.JI_LinePriceInLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.JI_LinePriceInLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 21, true);
		this.JI_LinePriceInLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.JI_LinePriceInLocalCurrencyControl.Name = "JI_LinePriceInLocalCurrencyControl";
		this.JI_LinePriceInLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.JI_LinePriceInLocalCurrencyControl.TabIndex = 3;
		// 
		// JI_Calc_DutyAmountIncludingWHEstimateControl
		// 
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.AllowDrop = true;
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_DutyAmountIncludingWHEstimate";
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 147, true);
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.Name = "JI_Calc_DutyAmountIncludingWHEstimateControl";
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.TabIndex = 11;
		// 
		// TotalOtherChargesInNOKControl
		// 
		this.TotalOtherChargesInNOKControl.AllowDrop = true;
		this.TotalOtherChargesInNOKControl.BindToAmount = "FilteredInvoiceLines.TotalOtherChargesInNOK";
		this.TotalOtherChargesInNOKControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.TotalOtherChargesInNOKControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.TotalOtherChargesInNOKControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 84, true);
		this.TotalOtherChargesInNOKControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.TotalOtherChargesInNOKControl.Name = "TotalOtherChargesInNOKControl";
		this.TotalOtherChargesInNOKControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.TotalOtherChargesInNOKControl.TabIndex = 8;
		// 
		// TotalDeductionsInNOKControl
		// 
		this.TotalDeductionsInNOKControl.AllowDrop = true;
		this.TotalDeductionsInNOKControl.BindToAmount = "FilteredInvoiceLines.TotalDeductionsInNOK";
		this.TotalDeductionsInNOKControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.TotalDeductionsInNOKControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.TotalDeductionsInNOKControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 105, true);
		this.TotalDeductionsInNOKControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.TotalDeductionsInNOKControl.Name = "TotalDeductionsInNOKControl";
		this.TotalDeductionsInNOKControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.TotalDeductionsInNOKControl.TabIndex = 9;
		// 
		// TotalExciseDutiesControl
		// 
		this.TotalExciseDutiesControl.AllowDrop = true;
		this.TotalExciseDutiesControl.BindToAmount = "FilteredInvoiceLines.TotalExciseDuties";
		this.TotalExciseDutiesControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
		this.TotalExciseDutiesControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
		this.TotalExciseDutiesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 170, true);
		this.TotalExciseDutiesControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.TotalExciseDutiesControl.Name = "TotalExciseDutiesControl";
		this.TotalExciseDutiesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
		this.TotalExciseDutiesControl.TabIndex = 12;
		// 
		// SupportingDocumentsTabPage
		// 
		this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("4A5C14CF-9C54-449A-95DA-00AD99399B67", "[44] Supporting Documents");
		this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
		this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
		this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		this.SupportingDocumentsTabPage.TabIndex = 1;
		// 
		// SupportingDocumentsUserControl
		// 
		this.SupportingDocumentsUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingDocumentsUserControl, "FilteredInvoiceLines.SupportingDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.NO.Business.SupportingDocument)(((Enterprise.Customs.NO.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)))));
		this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
		this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		this.SupportingDocumentsUserControl.TabIndex = 0;
		// 
		// PackagesPivotTabPage
		// 
		this.PackagesPivotTabPage.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("7A1A20ED-B4DD-4E2F-9A3F-8C78C518B2FC", "[31] Packages");
		this.PackagesPivotTabPage.Controls.Add(this.PackagesPivotUserControl);
		this.PackagesPivotTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
		this.PackagesPivotTabPage.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 327, true);
		this.PackagesPivotTabPage.Name = "PackagesPivotTabPage";
		this.PackagesPivotTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		this.PackagesPivotTabPage.TabIndex = 3;
		// 
		// PackagesPivotUserControl
		// 
		this.PackagesPivotUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.PackagesPivotUserControl, "FilteredInvoiceLines");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NO.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
		this.PackagesPivotUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.PackagesPivotUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.PackagesPivotUserControl.Name = "PackagesPivotUserControl";
		this.PackagesPivotUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 516, true);
		this.PackagesPivotUserControl.TabIndex = 0;
		// 
		// BaseInvoiceLineUserControl
		// 
		this.Name = "BaseInvoiceLineUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 721, true);
		this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
		this.InvoiceLinesSummaryGroupBox.PerformLayout();
		this.BottomPanel.ResumeLayout(false);
		this.BottomPanel.PerformLayout();
		this.TopPanel.ResumeLayout(false);
		this.TopPanel.PerformLayout();
		this.LineDetailTabControl.ResumeLayout(false);
		this.LineDetailTabControl.PerformLayout();
		this.InvoiceDetailsGroupBox.ResumeLayout(false);
		this.InvoiceDetailsGroupBox.PerformLayout();
		this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
		this.JI_CountryOfOriginBoundFindBox.PerformLayout();
		this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
		this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
		this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
		this.JI_LinePriceBoundCurrencyControl.PerformLayout();
		this.ClassificationDetailsGroupBox.ResumeLayout(false);
		this.ClassificationDetailsGroupBox.PerformLayout();
		this.LineChargesTabPage.ResumeLayout(false);
		this.LineChargesTabPage.PerformLayout();
		this.CurrentInvoicePanel.ResumeLayout(false);
		this.CurrentInvoicePanel.PerformLayout();
		this.LineSummaryPanel.ResumeLayout(false);
		this.LineSummaryPanel.PerformLayout();
		this.ContainersTabPage.ResumeLayout(false);
		this.ContainersTabPage.PerformLayout();
		this.ContainersGroupBox.ResumeLayout(false);
		this.ContainersGroupBox.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
		this.CusContainerInvoiceLineGrid.ResumeLayout(false);
		this.CusContainerInvoiceLineGrid.PerformLayout();
		this.LineDetailsTabPage.ResumeLayout(false);
		this.LineDetailsTabPage.PerformLayout();
		this.NewLineDetailsTabPage.ResumeLayout(false);
		this.NewLineDetailsTabPage.PerformLayout();
		this.InvoiceLineDetailsUserControl.ResumeLayout(true);
		this.InvoiceLineDetailsUserControl.PerformLayout();
		this.ClassificationPanel.ResumeLayout(false);
		this.ClassificationPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
		this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
		this.CustomsInvoiceLinesBoundGrid.PerformLayout();
		this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
		this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
		this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
		this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
		this.CustomsQuantityCalcDropEdit.PerformLayout();
		this.VolumeCalcDropEdit.ResumeLayout(true);
		this.VolumeCalcDropEdit.PerformLayout();
		this.JI_WeightCalcDropEdit.ResumeLayout(true);
		this.JI_WeightCalcDropEdit.PerformLayout();
		this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
		this.InvoiceQuantityCalcDropEdit.PerformLayout();
		this.JI_DescriptionBoundTextBox.ResumeLayout(true);
		this.JI_DescriptionBoundTextBox.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.JI_LinePriceInLocalCurrencyControl.ResumeLayout(true);
		this.JI_LinePriceInLocalCurrencyControl.PerformLayout();
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.ResumeLayout(true);
		this.JI_Calc_DutyAmountIncludingWHEstimateControl.PerformLayout();
		this.TotalOtherChargesInNOKControl.ResumeLayout(true);
		this.TotalOtherChargesInNOKControl.PerformLayout();
		this.TotalDeductionsInNOKControl.ResumeLayout(true);
		this.TotalDeductionsInNOKControl.PerformLayout();
		this.TotalExciseDutiesControl.ResumeLayout(true);
		this.TotalExciseDutiesControl.PerformLayout();
		this.SupportingDocumentsTabPage.ResumeLayout(false);
		this.SupportingDocumentsTabPage.PerformLayout();
		this.SupportingDocumentsUserControl.ResumeLayout(true);
		this.SupportingDocumentsUserControl.PerformLayout();
		this.PackagesPivotTabPage.ResumeLayout(false);
		this.PackagesPivotTabPage.PerformLayout();
		this.PackagesPivotUserControl.ResumeLayout(true);
		this.PackagesPivotUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	SupportingDocumentsUserControl SupportingDocumentsUserControl;
	ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
	ZArchitecture.GUI.ZTabPage PackagesPivotTabPage;
	Customs.GUI.ConvertToLocalCurrencyControl JI_LinePriceInLocalCurrencyControl;
	Customs.GUI.ConvertToLocalCurrencyControl JI_Calc_DutyAmountIncludingWHEstimateControl;
	Customs.GUI.ConvertToLocalCurrencyControl TotalOtherChargesInNOKControl;
	Customs.GUI.ConvertToLocalCurrencyControl TotalDeductionsInNOKControl;
	Customs.GUI.ConvertToLocalCurrencyControl TotalExciseDutiesControl;
	Customs.GUI.BaseLineLevelPackingPivotControl PackagesPivotUserControl;
}

