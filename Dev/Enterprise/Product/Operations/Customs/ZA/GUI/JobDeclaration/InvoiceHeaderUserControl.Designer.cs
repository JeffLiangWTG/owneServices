using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ZAInvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.conversionFactorCalcEdit = new ZCalcEdit();
			this.vDNTextBox = new ZTextBox();
			this.vBMPercentCalcEdit = new ZCalcEdit();
			this.valuationCodeDropEdit = new ZDropEdit();
			this.jZ_RN_NKDefaultOriginCodeFindBox = new ZCodeFindBox();
			this.rulesOfOriginCertificateTextBox = new ZTextBox();
			this.relatedIndicatorDropEdit = new ZDropEdit();
			this.invDate1 = new ZDateEdit();
			this.rOOTypeDropEdit = new ZDropEdit();
			this.JZ_PaymentTermsDropEdit = new ZDropEdit();
			this.JZ_PaymentTermsDropEdit.SuspendLayout();
			this.JZ_IncoTermBoundDropDownEdit.SuspendLayout();
			this.GroupInvoiceDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.valuationCodeDropEdit.SuspendLayout();
			this.jZ_RN_NKDefaultOriginCodeFindBox.SuspendLayout();
			this.relatedIndicatorDropEdit.SuspendLayout();
			this.invDate1.SuspendLayout();
			this.rOOTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 94, true);
			this.IncoTermExplainButton.TabIndex = 6;
			// 
			// JZ_InvoiceCurrExRateCalcEdit
			// 
			this.JZ_InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 74, true);
			this.JZ_InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			// 
			// JZ_InvoiceNumberBoundTextBox
			// 
			this.JZ_InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			// 
			// JZ_IncoTermBoundDropDownEdit
			// 
			this.JZ_IncoTermBoundDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 95, true);
			this.JZ_IncoTermBoundDropDownEdit.TabIndex = 5;
			// 
			// GroupInvoiceDropEdit
			// 
			this.GroupInvoiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 30, true);
			// 
			// JZ_InvoiceCurrLandedCostExRateCalcEdit
			// 
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 74, true);
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.JZ_InvoiceCurrLandedCostExRateCalcEdit.TabIndex = 4;
			// 
			// NoOfPacksCalcDropEdit
			// 
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 158, true);
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 115, true);
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 7;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 137, true);
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 8;
			// 
			// JZ_InvoiceAmountBoundCurrencyControl
			// 
			this.JZ_InvoiceAmountBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 52, true);
			this.JZ_InvoiceAmountBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			// 
			// JZ_IncoTermPlaceTextBox
			// 
			this.JZ_IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 95, true);
			this.JZ_IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.JZ_IncoTermPlaceTextBox.TabIndex = 6;
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.TabIndex = 1;
			// 
			// ApportionmentPendingLabel
			// 
			this.ApportionmentPendingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 32, true);
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.conversionFactorCalcEdit);
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 32, true);
			this.RightBottomPanel.TabIndex = 2;
			this.RightBottomPanel.Controls.SetChildIndex(this.conversionFactorCalcEdit, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.ApportionmentPendingLabel, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_CIFAmountBoundCurrencyControl, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_Calc_TNIBoundInvoiceCurrencyControl, 0);
			this.RightBottomPanel.Controls.SetChildIndex(this.JZ_FOBAmountBoundCurrencyControl, 0);
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 484, true);
			// 
			// ComInvoiceDetailsTabPage
			// 
			this.ComInvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 457, true);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 239, true);
			this.ChargesGroupBox.TabIndex = 14;
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 220, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 193, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 193, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 212, true);
			this.BaseGroupChargesGroupBox.TabIndex = 15;
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 193, true);
			// 
			// JZ_FOBAmountBoundCurrencyControl
			// 
			this.JZ_FOBAmountBoundCurrencyControl.BindToAmount = "Invoices.JZ_Calc_FOBAmountInLocalCurrency";
			this.JZ_FOBAmountBoundCurrencyControl.BindToUnit = "Invoices.LocalCurrency.PK";
			// 
			// JZ_CIFAmountBoundCurrencyControl
			// 
			this.JZ_CIFAmountBoundCurrencyControl.BindToAmount = "Invoices.JZ_Calc_CIFAmount_InLocalCurrency";
			this.JZ_CIFAmountBoundCurrencyControl.BindToUnit = "Invoices.LocalCurrency.PK";
			// 
			// JobComInvoiceHeadersBoundGrid
			// 
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JZ_RN_NKDefaultOrigin";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JZ_ROOCert";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.GridId = null;
			// 
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = null;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 109, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			this.JobComInvoiceHeadersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 147, true);
			// 
			// InvCustomFieldsDisplayControl
			// 
			this.InvCustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 389, true);
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 389, true);
			// 
			// Splitter
			// 
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 672, true);
			this.Splitter.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(347);
			// 
			// InvDetailLeftPanel
			//
			this.InvDetailLeftPanel.Controls.Add(this.JZ_PaymentTermsDropEdit);
			this.InvDetailLeftPanel.Controls.Add(this.valuationCodeDropEdit);
			this.InvDetailLeftPanel.Controls.Add(this.vDNTextBox);
			this.InvDetailLeftPanel.Controls.Add(this.relatedIndicatorDropEdit);
			this.InvDetailLeftPanel.Controls.Add(this.vBMPercentCalcEdit);
			this.InvDetailLeftPanel.Controls.Add(this.jZ_RN_NKDefaultOriginCodeFindBox);
			this.InvDetailLeftPanel.Controls.Add(this.rulesOfOriginCertificateTextBox);
			this.InvDetailLeftPanel.Controls.Add(this.invDate1);
			this.InvDetailLeftPanel.Controls.Add(this.rOOTypeDropEdit);
			this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 457, true);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NoOfPacksCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrLandedCostExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermPlaceTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_IncoTermBoundDropDownEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.IncoTermExplainButton, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceCurrExRateCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceAmountBoundCurrencyControl, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.GroupInvoiceDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_InvoiceNumberBoundTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.rOOTypeDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.invDate1, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.rulesOfOriginCertificateTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.jZ_RN_NKDefaultOriginCodeFindBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.vBMPercentCalcEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.relatedIndicatorDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.vDNTextBox, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.valuationCodeDropEdit, 0);
			this.InvDetailLeftPanel.Controls.SetChildIndex(this.JZ_PaymentTermsDropEdit, 0);
			// 
			// InvDetailRightPanel
			// 
			this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 457, true);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 457, true);
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(239);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 484, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 32, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// conversionFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.conversionFactorCalcEdit, "Invoices.JZ_Calc_ConversionFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_ConversionFactor);
			this.conversionFactorCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("D9525290-E91F-4F5D-9321-D7D0429173B2", "Conversion Factor");
			this.conversionFactorCalcEdit.DecimalPlaces = 8;
			this.conversionFactorCalcEdit.Decimals = 8;
			this.conversionFactorCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 6, true);
			this.conversionFactorCalcEdit.Name = "conversionFactorCalcEdit";
			this.conversionFactorCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.conversionFactorCalcEdit.TabIndex = 7;
			this.conversionFactorCalcEdit.Text = "0.000000";
			this.conversionFactorCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// vDNTextBox
			// 
			this.BindingSource.SetBindingMember(this.vDNTextBox, "Invoices.JZ_VDN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_VDN);
			this.vDNTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ae026f15-8a38-4253-9023-e2db35a7a6dc", "VDN #");
			this.vDNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 289, true);
			this.vDNTextBox.Name = "vDNTextBox";
			this.vDNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.vDNTextBox.TabIndex = 17;
			// 
			// vBMPercentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.vBMPercentCalcEdit, "Invoices.JZ_ValuationMarkup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationMarkup);
			this.vBMPercentCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("662509b5-7267-443d-9731-d9f2f69d90d9", "Valuation Markup %");
			this.vBMPercentCalcEdit.DecimalPlaces = 2;
			this.vBMPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 289, true);
			this.vBMPercentCalcEdit.Name = "vBMPercentCalcEdit";
			this.vBMPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.vBMPercentCalcEdit.TabIndex = 18;
			this.vBMPercentCalcEdit.Text = "0";
			this.vBMPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// valuationCodeDropEdit
			// 
			this.valuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.valuationCodeDropEdit, "Invoices.JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationCode);
			this.valuationCodeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZAInvoiceHeaderUserControl|7E9620EA-E1F4-4F95-81A2-729DFCC34327", "Valuation Code");
			this.valuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 267, true);
			this.valuationCodeDropEdit.Name = "valuationCodeDropEdit";
			this.valuationCodeDropEdit.PreBoundMaxLength = 1;
			this.valuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.valuationCodeDropEdit.TabIndex = 16;
			// 
			// jZ_RN_NKDefaultOriginCodeFindBox
			// 
			this.jZ_RN_NKDefaultOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jZ_RN_NKDefaultOriginCodeFindBox, "Invoices.JZ_RN_NKDefaultOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RN_NKDefaultOrigin);
			this.jZ_RN_NKDefaultOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 179, true);
			this.jZ_RN_NKDefaultOriginCodeFindBox.Name = "jZ_RN_NKDefaultOriginCodeFindBox";
			this.jZ_RN_NKDefaultOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.jZ_RN_NKDefaultOriginCodeFindBox.ParentType = null;
			this.jZ_RN_NKDefaultOriginCodeFindBox.PreBoundMaxLength = 2;
			this.jZ_RN_NKDefaultOriginCodeFindBox.ShowDescriptionBox = false;
			this.jZ_RN_NKDefaultOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.jZ_RN_NKDefaultOriginCodeFindBox.TabIndex = 11;
			// 
			// rulesOfOriginCertificateTextBox
			// 
			this.BindingSource.SetBindingMember(this.rulesOfOriginCertificateTextBox, "Invoices.JZ_ROOCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ROOCert);
			this.rulesOfOriginCertificateTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("D30B09C7-326A-4467-91D6-00897F07B443", "ROO Certificate", "Rules Of Origin Certificate", "Rules Of Origin Certificate Number");
			this.rulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 201, true);
			this.rulesOfOriginCertificateTextBox.Name = "rulesOfOriginCertificateTextBox";
			this.rulesOfOriginCertificateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.rulesOfOriginCertificateTextBox.TabIndex = 13;
			// 
			// relatedIndicatorDropEdit
			// 
			this.relatedIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.relatedIndicatorDropEdit, "Invoices.JZ_RelatedIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RelatedIndicator);
			this.relatedIndicatorDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ZAInvoiceHeaderUserControl|6FCA83F0-6CD8-412E-9BE0-B2D33AF7C673", "Relationship Indicator");
			this.relatedIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 245, true);
			this.relatedIndicatorDropEdit.Name = "relatedIndicatorDropEdit";
			this.relatedIndicatorDropEdit.PreBoundMaxLength = 1;
			this.relatedIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.relatedIndicatorDropEdit.TabIndex = 15;
			// 
			// invDate1
			// 
			this.invDate1.AllowDrop = true;
			this.invDate1.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.invDate1, "Invoices.JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceDate);
			this.invDate1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a1c5cc83-12a6-4503-85a4-2f649c7886d1", "Inv. Date");
			this.invDate1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 158, true);
			this.invDate1.Name = "invDate1";
			this.invDate1.TabIndex = 10;
			// 
			// rOOTypeDropEdit
			// 
			this.rOOTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rOOTypeDropEdit, "Invoices.JZ_ROOType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceHeader)(((System.Collections.IList)(((JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ROOType);
			this.rOOTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("7466c898-cfeb-4cc9-b0f1-e4cde46a5631", "ROO Type");
			this.rOOTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 201, true);
			this.rOOTypeDropEdit.Name = "rOOTypeDropEdit";
			this.rOOTypeDropEdit.ShowDescriptionBox = false;
			this.rOOTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.rOOTypeDropEdit.TabIndex = 12;
			// 
			// JZ_PaymentTermsDropEdit
			// 
			this.JZ_PaymentTermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JZ_PaymentTermsDropEdit, "Invoices.JZ_PaymentTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_PaymentTerms)));
			this.JZ_PaymentTermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 223, true);
			this.JZ_PaymentTermsDropEdit.Name = "JZ_PaymentTermsDropEdit";
			this.JZ_PaymentTermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.JZ_PaymentTermsDropEdit.TabIndex = 14;
			// 
			// ZAInvoiceHeaderUserControl
			// 
			this.Name = "ZAInvoiceHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 672, true);
			this.JZ_IncoTermBoundDropDownEdit.ResumeLayout(true);
			this.JZ_IncoTermBoundDropDownEdit.PerformLayout();
			this.GroupInvoiceDropEdit.ResumeLayout(true);
			this.GroupInvoiceDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.JZ_InvoiceAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_InvoiceAmountBoundCurrencyControl.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.valuationCodeDropEdit.ResumeLayout(true);
			this.valuationCodeDropEdit.PerformLayout();
			this.jZ_RN_NKDefaultOriginCodeFindBox.ResumeLayout(true);
			this.jZ_RN_NKDefaultOriginCodeFindBox.PerformLayout();
			this.relatedIndicatorDropEdit.ResumeLayout(true);
			this.relatedIndicatorDropEdit.PerformLayout();
			this.invDate1.ResumeLayout(true);
			this.invDate1.PerformLayout();
			this.rOOTypeDropEdit.ResumeLayout(true);
			this.rOOTypeDropEdit.PerformLayout();
			this.JZ_PaymentTermsDropEdit.ResumeLayout(true);
			this.JZ_PaymentTermsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZDropEdit JZ_PaymentTermsDropEdit;
	}
}
