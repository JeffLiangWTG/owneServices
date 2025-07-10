using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class InvoiceLinesUserControl
	{


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.tariffFindBox = new Universal.GUI.TariffFindBox();
			this.JI_Calc_ATVConvertToLocalCurrencyControl1 = new ConvertToLocalCurrencyControl();
			this.JI_Calc_ActualPriceCurrencyControl = new ConvertToLocalCurrencyControl();
			this.MiscTabPage = new ZTabPage();
			this.ImportBOEGroupBox = new ZGroupBox();
			this.DA63UserControl = new DA63UserControl();
			this.classificationDetailsUserControl = new ClassificationDetailsUserControl();
			this.GoodsTypeDropEdit = new ZDropEdit();
			this.DiamondProcessingTabPage = new ZTabPage();
			this.diamondProcessingGroupBox = new ZGroupBox();
			this.diamondLevyValueCalcEdit = new ZArchitecture.ZCalcEdit();
			this.diamondProducerRegistrationZTextBox = new ZArchitecture.ZTextBox();
			this.diamondProducerExemptionZTextBox = new ZArchitecture.ZTextBox();
			this.electionsExemptionsLevyZTextBox = new ZArchitecture.ZTextBox();
			this.kimberleyCertificateZTextBox = new ZArchitecture.ZTextBox();
			this.temporaryBuyersPermitZTextBox = new ZArchitecture.ZTextBox();
			this.temporaryExportExemptionZTextBox = new ZArchitecture.ZTextBox();
			this.diamondDealerLicenseZTextBox = new ZArchitecture.ZTextBox();
			this.diamondBeneficiaryLicenseZTextBox = new ZArchitecture.ZTextBox();
			this.ProductCodeFindBox = new ZCodeFindBox();
			this.jI_CustomsQuantityCalcDropEdit = new ZCalcDropEdit();
			this.JI_ProcedureFindBox = new ZCodeFindBox();
			this.JI_CEIGuidDropEdit = new ZGuidDropEdit();
			this.PermitNumberCodeFindBox = new ZCodeFindBox();
			this.JI_ValuationMarkupCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ROOTypeDropEdit = new ZDropEdit();
			this.PreferenceDropEdit = new ZDropEdit();
			this.TaxOrFeeDropEdit = new ZDropEdit();
			this.RulesOfOriginCertificateTextBox = new ZArchitecture.ZTextBox();
			this.TradeStatisticsCheckBox = new ZCheckBox();
			this.TradeAgreementLabel = new ZArchitecture.ZLabel();
			this.PreviousMRNTextBox = new ZArchitecture.ZTextBox();
			this.ImportBOELineCalcEdit = new ZArchitecture.ZCalcEdit();
			this.CustomsValueOverrideCalcDropEdit = new ZCalcDropEdit();
			this.InvoiceQuantityCalcEdit = new ZArchitecture.ZCalcEdit();
			this.InvoiceQuantityUNE20CodeFindBox = new ZCodeFindBox();
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
			this.tariffFindBox.SuspendLayout();
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.SuspendLayout();
			this.JI_Calc_ActualPriceCurrencyControl.SuspendLayout();
			this.MiscTabPage.SuspendLayout();
			this.ImportBOEGroupBox.SuspendLayout();
			this.DA63UserControl.SuspendLayout();
			this.classificationDetailsUserControl.SuspendLayout();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.DiamondProcessingTabPage.SuspendLayout();
			this.diamondProcessingGroupBox.SuspendLayout();
			this.ProductCodeFindBox.SuspendLayout();
			this.jI_CustomsQuantityCalcDropEdit.SuspendLayout();
			this.JI_ProcedureFindBox.SuspendLayout();
			this.JI_CEIGuidDropEdit.SuspendLayout();
			this.PermitNumberCodeFindBox.SuspendLayout();
			this.ROOTypeDropEdit.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.CustomsValueOverrideCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcEdit.SuspendLayout();
			this.InvoiceQuantityUNE20CodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(935, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 424, true);
			this.InvoiceLinesSummaryGroupBox.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 424, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 605, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.MiscTabPage);
			this.LineDetailTabControl.Controls.Add(this.DiamondProcessingTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 424, true);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.DiamondProcessingTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.MiscTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Controls.Add(this.CustomsValueOverrideCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.ImportBOELineCalcEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.PreviousMRNTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.TradeAgreementLabel);
			this.InvoiceDetailsGroupBox.Controls.Add(this.PermitNumberCodeFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_ValuationMarkupCalcEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.PreferenceDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.ROOTypeDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.TaxOrFeeDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.RulesOfOriginCertificateTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.TradeStatisticsCheckBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.ProductCodeFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.jI_CustomsQuantityCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_ProcedureFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_CEIGuidDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceQuantityCalcEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.InvoiceQuantityUNE20CodeFindBox);
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 229, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.GoodsTypeDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CEIGuidDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_ProcedureFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.jI_CustomsQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.ProductCodeFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TradeStatisticsCheckBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.RulesOfOriginCertificateTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TaxOrFeeDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.ROOTypeDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.PreferenceDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_ValuationMarkupCalcEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.PermitNumberCodeFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TradeAgreementLabel, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.PreviousMRNTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.ImportBOELineCalcEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsValueOverrideCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityUNE20CodeFindBox, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 160, true);
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 8;
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 75, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 16;
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 139, true);
			this.JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 7;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.ClassificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 0, true);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 95, true);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			// 
			// CurrentInvoicePanel
			// 
			this.CurrentInvoicePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.CurrentInvoicePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.CurrentInvoicePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 84, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_ActualPriceCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.JI_Calc_ATVConvertToLocalCurrencyControl1);
			this.LineSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 100, true);
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 321, true);
			this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_ATVConvertToLocalCurrencyControl1, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_ActualPriceCurrencyControl, 0);
			// 
			// PendingApportionmentLabel
			// 
			this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 407, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 378, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 605, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Controls.Add(this.classificationDetailsUserControl);
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.ClassificationPanel, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.InvoiceDetailsGroupBox, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.classificationDetailsUserControl, 0);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 397, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 397, true);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 364, true);
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 28, true);
			this.ClassificationPanel.TabIndex = 1;
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 605, true);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 141, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 5;
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("InvoiceLinesUserControl|F7F0DB89-53D4-49E1-A331-A9F1BE564EBE", "Insurance", "Insurance for current line item");
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 115, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 4;
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 91, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups.CurrencyList";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 24, true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 190, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 7;
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 166, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 6;
			// 
			// JI_Calc_BalanceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// JI_Calc_LinesTotalConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 40, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 4;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 53, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 15;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 11, true);
			this.JI_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.JI_WeightCalcDropEdit.TabIndex = 13;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 97, true);
			this.InvoiceQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.InvoiceQuantityCalcDropEdit.TabIndex = 5;
			// 
			// InvoiceQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceQuantityCalcEdit, "FilteredInvoiceLines.JI_InvoiceQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceQuantity)));
			this.InvoiceQuantityCalcEdit.CaptionResourceString = null;
			this.InvoiceQuantityCalcEdit.DecimalPlaces = 6;
			this.InvoiceQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 97, true);
			this.InvoiceQuantityCalcEdit.Name = "InvoiceQuantityCalcEdit";
			this.InvoiceQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.InvoiceQuantityCalcEdit.TabIndex = 5;
			this.InvoiceQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceUQUNE20CodeFindBox
			//
			this.InvoiceQuantityUNE20CodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceQuantityUNE20CodeFindBox, "FilteredInvoiceLines.JI_InvoiceUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_InvoiceUQ)));
			this.InvoiceQuantityUNE20CodeFindBox.BindToList = "FilteredInvoiceLines.Lookups+InvoiceUQUNE20CodeList";
			this.InvoiceQuantityUNE20CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 97, true);
			this.InvoiceQuantityUNE20CodeFindBox.ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusCodeList;
			this.InvoiceQuantityUNE20CodeFindBox.Name = "InvoiceQuantityUNE20CodeFindBox";
			this.InvoiceQuantityUNE20CodeFindBox.ShowDescriptionBox = true;
			this.InvoiceQuantityUNE20CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.InvoiceQuantityUNE20CodeFindBox.TabIndex = 5;
			this.InvoiceQuantityUNE20CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.InvoiceQuantityUNE20CodeFindBox.ParentType = null;
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 75, true);
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 20, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 4;
			// 
			// oLabel8
			// 
			this.oLabel8.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 10, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// tariffFindBox
			// 
			this.tariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.tariffFindBox.ErrorForUnsupportedCountry = null;
			this.tariffFindBox.GetEffectiveDate = null;
			this.tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 53, true);
			this.tariffFindBox.Name = "tariffFindBox";
			this.tariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.tariffFindBox.ParentType = null;
			this.tariffFindBox.PreBoundMaxLength = 10;
			this.tariffFindBox.SelectNomenclatureModes = null;
			this.tariffFindBox.ShowDescriptionBox = false;
			this.tariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.tariffFindBox.TabIndex = 3;
			this.tariffFindBox.TariffType = null;
			// 
			// JI_Calc_ATVConvertToLocalCurrencyControl1
			// 
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.AllowDrop = true;
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.BindToAmount = "FilteredInvoiceLines.JI_Calc_ATV";
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.BindToList = "FilteredInvoiceLines.Lookups.CurrencyList";
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3621e23b-d6cb-44bd-94a0-a57566321de3", "ATV Amount");
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 214, true);
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.Name = "JI_Calc_ATVConvertToLocalCurrencyControl1";
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.TabIndex = 8;
			// 
			// JI_Calc_ActualPriceCurrencyControl
			// 
			this.JI_Calc_ActualPriceCurrencyControl.AllowDrop = true;
			this.JI_Calc_ActualPriceCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_ActualPrice";
			this.JI_Calc_ActualPriceCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups.CurrencyList";
			this.JI_Calc_ActualPriceCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_ActualPriceCurrencyControl.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f0669970-793d-4714-ad4b-9b26b414c918", "Actual Price");
			this.JI_Calc_ActualPriceCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 45, true);
			this.JI_Calc_ActualPriceCurrencyControl.Name = "JI_Calc_ActualPriceCurrencyControl";
			this.JI_Calc_ActualPriceCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_Calc_ActualPriceCurrencyControl.TabIndex = 2;
			// 
			// MiscTabPage
			// 
			this.MiscTabPage.Controls.Add(this.ImportBOEGroupBox);
			this.MiscTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscTabPage.Name = "MiscTabPage";
			this.MiscTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			this.MiscTabPage.TabIndex = 2;
			this.MiscTabPage.Text = "DA63";
			// 
			// ImportBOEGroupBox
			// 
			this.ImportBOEGroupBox.Controls.Add(this.DA63UserControl);
			this.ImportBOEGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportBOEGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportBOEGroupBox.Name = "ImportBOEGroupBox";
			this.ImportBOEGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			this.ImportBOEGroupBox.TabIndex = 1;
			this.ImportBOEGroupBox.TabStop = false;
			this.ImportBOEGroupBox.Text = "DA63 Import BOE Info";
			// 
			// DA63UserControl
			// 
			this.DA63UserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DA63UserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.DA63UserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DA63UserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DA63UserControl.Name = "DA63UserControl";
			this.DA63UserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 378, true);
			this.DA63UserControl.TabIndex = 2;
			this.DA63UserControl.TabStop = false;
			// 
			// classificationDetailsUserControl
			// 
			this.classificationDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.classificationDetailsUserControl, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobComInvoiceLine)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)))));
			this.classificationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.classificationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 229, true);
			this.classificationDetailsUserControl.Name = "classificationDetailsUserControl";
			this.classificationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 168, true);
			this.classificationDetailsUserControl.TabIndex = 1;
			this.classificationDetailsUserControl.TabStop = false;
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "FilteredInvoiceLines.JI_NewUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NewUsed)));
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 160, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 21;
			// 
			// DiamondProcessingTabPage
			// 
			this.DiamondProcessingTabPage.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("abfb2a07-93e8-4c37-868f-6670a1748685", "Diamond Processing");
			this.DiamondProcessingTabPage.Controls.Add(this.diamondProcessingGroupBox);
			this.DiamondProcessingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DiamondProcessingTabPage.Name = "DiamondProcessingTabPage";
			this.DiamondProcessingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			this.DiamondProcessingTabPage.TabIndex = 3;
			this.DiamondProcessingTabPage.Text = "Diamond Processing";
			// 
			// diamondProcessingGroupBox
			// 
			this.diamondProcessingGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("abfb2a07-93e8-4c37-868f-6670a1748685", "Diamond Processing");
			this.diamondProcessingGroupBox.Controls.Add(this.diamondLevyValueCalcEdit);
			this.diamondProcessingGroupBox.Controls.Add(this.diamondProducerRegistrationZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.diamondProducerExemptionZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.electionsExemptionsLevyZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.kimberleyCertificateZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.temporaryBuyersPermitZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.temporaryExportExemptionZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.diamondDealerLicenseZTextBox);
			this.diamondProcessingGroupBox.Controls.Add(this.diamondBeneficiaryLicenseZTextBox);
			this.diamondProcessingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.diamondProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.diamondProcessingGroupBox.Name = "diamondProcessingGroupBox";
			this.diamondProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 397, true);
			this.diamondProcessingGroupBox.TabIndex = 0;
			this.diamondProcessingGroupBox.TabStop = false;
			// 
			// diamondLevyValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.diamondLevyValueCalcEdit, "FilteredInvoiceLines.JI_DiamondLevyValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DiamondLevyValue)));
			this.diamondLevyValueCalcEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e347e502-dd94-4d4e-bbc9-7e99bd34e54b", "Diamond Levy Value");
			this.diamondLevyValueCalcEdit.DecimalPlaces = 0;
			this.diamondLevyValueCalcEdit.Decimals = 0;
			this.diamondLevyValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 90, true);
			this.diamondLevyValueCalcEdit.Name = "diamondLevyValueCalcEdit";
			this.diamondLevyValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.diamondLevyValueCalcEdit.TabIndex = 4;
			this.diamondLevyValueCalcEdit.Text = "0";
			this.diamondLevyValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// diamondProducerRegistrationZTextBox
			// 
			this.BindingSource.SetBindingMember(this.diamondProducerRegistrationZTextBox, "FilteredInvoiceLines.JI_DiamondProducerRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DiamondProducerRegistration)));
			this.diamondProducerRegistrationZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("d30d609c-e0f0-4577-8785-db54191ed4aa", "Diamond Producer Registration");
			this.diamondProducerRegistrationZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 113, true);
			this.diamondProducerRegistrationZTextBox.Name = "diamondProducerRegistrationZTextBox";
			this.diamondProducerRegistrationZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.diamondProducerRegistrationZTextBox.TabIndex = 5;
			// 
			// diamondProducerExemptionZTextBox
			// 
			this.BindingSource.SetBindingMember(this.diamondProducerExemptionZTextBox, "FilteredInvoiceLines.JI_DiamondProducerExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DiamondProducerExemption)));
			this.diamondProducerExemptionZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f6e9276f-1f42-47d5-8569-59482a21f439", "Diamond Producer Exemption");
			this.diamondProducerExemptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 135, true);
			this.diamondProducerExemptionZTextBox.Name = "diamondProducerExemptionZTextBox";
			this.diamondProducerExemptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.diamondProducerExemptionZTextBox.TabIndex = 6;
			// 
			// electionsExemptionsLevyZTextBox
			// 
			this.BindingSource.SetBindingMember(this.electionsExemptionsLevyZTextBox, "FilteredInvoiceLines.JI_ElectionsExemptionsLevy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ElectionsExemptionsLevy)));
			this.electionsExemptionsLevyZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f4cd5755-0358-453d-b7e0-9c5cbd68ddaa", "Elections Exemptions Levy");
			this.electionsExemptionsLevyZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 158, true);
			this.electionsExemptionsLevyZTextBox.Name = "electionsExemptionsLevyZTextBox";
			this.electionsExemptionsLevyZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.electionsExemptionsLevyZTextBox.TabIndex = 7;
			// 
			// kimberleyCertificateZTextBox
			// 
			this.BindingSource.SetBindingMember(this.kimberleyCertificateZTextBox, "FilteredInvoiceLines.JI_KimberleyCertificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_KimberleyCertificate)));
			this.kimberleyCertificateZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("5102d97f-b222-4dd5-8208-88bc34ff118c", "Kimberley Certificate");
			this.kimberleyCertificateZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 180, true);
			this.kimberleyCertificateZTextBox.Name = "kimberleyCertificateZTextBox";
			this.kimberleyCertificateZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.kimberleyCertificateZTextBox.TabIndex = 8;
			// 
			// temporaryBuyersPermitZTextBox
			// 
			this.BindingSource.SetBindingMember(this.temporaryBuyersPermitZTextBox, "FilteredInvoiceLines.JI_TemporaryBuyersPermit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TemporaryBuyersPermit)));
			this.temporaryBuyersPermitZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2ac0e675-1cd3-4973-93fc-b6ef46a14296", "Temporary Buyers Permit");
			this.temporaryBuyersPermitZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 202, true);
			this.temporaryBuyersPermitZTextBox.Name = "temporaryBuyersPermitZTextBox";
			this.temporaryBuyersPermitZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.temporaryBuyersPermitZTextBox.TabIndex = 9;
			// 
			// temporaryExportExemptionZTextBox
			// 
			this.BindingSource.SetBindingMember(this.temporaryExportExemptionZTextBox, "FilteredInvoiceLines.JI_TemporaryExportExemption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TemporaryExportExemption)));
			this.temporaryExportExemptionZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("e353871a-b84a-4f9a-b188-4a861eee4538", "Temporary Export Exemption");
			this.temporaryExportExemptionZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 68, true);
			this.temporaryExportExemptionZTextBox.Name = "temporaryExportExemptionZTextBox";
			this.temporaryExportExemptionZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.temporaryExportExemptionZTextBox.TabIndex = 3;
			// 
			// diamondDealerLicenseZTextBox
			// 
			this.BindingSource.SetBindingMember(this.diamondDealerLicenseZTextBox, "FilteredInvoiceLines.JI_DiamondDealerLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DiamondDealerLicense)));
			this.diamondDealerLicenseZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("62df369c-e20d-49bc-a56e-10377a18d10d", "Diamond Dealer License");
			this.diamondDealerLicenseZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 46, true);
			this.diamondDealerLicenseZTextBox.Name = "diamondDealerLicenseZTextBox";
			this.diamondDealerLicenseZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.diamondDealerLicenseZTextBox.TabIndex = 2;
			// 
			// diamondBeneficiaryLicenseZTextBox
			// 
			this.BindingSource.SetBindingMember(this.diamondBeneficiaryLicenseZTextBox, "FilteredInvoiceLines.JI_DiamondBeneficiaryLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DiamondBeneficiaryLicense)));
			this.diamondBeneficiaryLicenseZTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("170462ff-c9ff-451e-867f-84f6533cd238", "Diamond Beneficiary License");
			this.diamondBeneficiaryLicenseZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 22, true);
			this.diamondBeneficiaryLicenseZTextBox.Name = "diamondBeneficiaryLicenseZTextBox";
			this.diamondBeneficiaryLicenseZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.diamondBeneficiaryLicenseZTextBox.TabIndex = 1;
			// 
			// ProductCodeFindBox
			// 
			this.ProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductCodeFindBox, "FilteredInvoiceLines.JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			this.ProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 53, true);
			this.ProductCodeFindBox.Name = "ProductCodeFindBox";
			this.ProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProductCodeFindBox.ParentType = null;
			this.ProductCodeFindBox.PreBoundMaxLength = 35;
			this.ProductCodeFindBox.ShowDescriptionBox = false;
			this.ProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ProductCodeFindBox.TabIndex = 2;
			// 
			// jI_CustomsQuantityCalcDropEdit
			// 
			this.jI_CustomsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jI_CustomsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
			this.jI_CustomsQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsQuantity";
			this.jI_CustomsQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups.CustomsUQList";
			this.jI_CustomsQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsUnitQty";
			this.jI_CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 118, true);
			this.jI_CustomsQuantityCalcDropEdit.Name = "jI_CustomsQuantityCalcDropEdit";
			this.jI_CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.jI_CustomsQuantityCalcDropEdit.TabIndex = 6;
			this.jI_CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JI_ProcedureFindBox
			// 
			this.JI_ProcedureFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_ProcedureFindBox, "FilteredInvoiceLines.JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CPCList)));
			this.JI_ProcedureFindBox.BindToList = "FilteredInvoiceLines.Lookups+CPCList";
			this.JI_ProcedureFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 32, true);
			this.JI_ProcedureFindBox.ModuleID = Enterprise.Customs.Common.ModuleRegistration.CustomsModuleIDs.Universal.ZZRefCusProcedure;
			this.JI_ProcedureFindBox.Name = "JI_ProcedureFindBox";
			this.JI_ProcedureFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_ProcedureFindBox.ParentType = null;
			this.JI_ProcedureFindBox.PreBoundMaxLength = 3;
			this.JI_ProcedureFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.JI_ProcedureFindBox.TabIndex = 1;
			// 
			// JI_CEIGuidDropEdit
			// 
			this.JI_CEIGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_CEIGuidDropEdit, "FilteredInvoiceLines.JI_CEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CEI)));
			this.JI_CEIGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 11, true);
			this.JI_CEIGuidDropEdit.Name = "JI_CEIGuidDropEdit";
			this.JI_CEIGuidDropEdit.PreBoundMaxLength = 3;
			this.JI_CEIGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.JI_CEIGuidDropEdit.TabIndex = 0;
			// 
			// PermitNumberCodeFindBox
			// 
			this.PermitNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitNumberCodeFindBox, "FilteredInvoiceLines.JI_PermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PermitNumber)));
			this.PermitNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 32, true);
			this.PermitNumberCodeFindBox.Name = "PermitNumberCodeFindBox";
			this.PermitNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PermitNumberCodeFindBox.ParentType = null;
			this.PermitNumberCodeFindBox.ShowDescriptionBox = false;
			this.PermitNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.PermitNumberCodeFindBox.TabIndex = 14;
			// 
			// JI_ValuationMarkupCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_ValuationMarkupCalcEdit, "FilteredInvoiceLines.JI_ValuationMarkup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ValuationMarkup)));
			this.JI_ValuationMarkupCalcEdit.CaptionResourceString = null;
			this.JI_ValuationMarkupCalcEdit.DecimalPlaces = 2;
			this.JI_ValuationMarkupCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 139, true);
			this.JI_ValuationMarkupCalcEdit.Name = "JI_ValuationMarkupCalcEdit";
			this.JI_ValuationMarkupCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
			this.JI_ValuationMarkupCalcEdit.TabIndex = 19;
			this.JI_ValuationMarkupCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ROOTypeDropEdit
			// 
			this.ROOTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ROOTypeDropEdit, "FilteredInvoiceLines.JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
			this.ROOTypeDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("db58802b-d41e-4a16-81a7-90f94f25d8b6", "ROO Type");
			this.ROOTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 181, true);
			this.ROOTypeDropEdit.Name = "ROOTypeDropEdit";
			this.ROOTypeDropEdit.ShowDescriptionBox = false;
			this.ROOTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ROOTypeDropEdit.TabIndex = 10;
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "FilteredInvoiceLines.JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
			this.PreferenceDropEdit.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("{7240E08E-5BBC-42F3-B8B9-ABA9081C1098}", "Preference");
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 181, true);
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.ShowDescriptionBox = false;
			this.PreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PreferenceDropEdit.TabIndex = 10;
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeDropEdit, "FilteredInvoiceLines.JI_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ZZF_NKTaxType)));
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 181, true);
			this.TaxOrFeeDropEdit.Name = "TaxOrFeeDropEdit";
			this.TaxOrFeeDropEdit.PreBoundMaxLength = 3;
			this.TaxOrFeeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.TaxOrFeeDropEdit.TabIndex = 22;
			// 
			// RulesOfOriginCertificateTextBox
			// 
			this.BindingSource.SetBindingMember(this.RulesOfOriginCertificateTextBox, "FilteredInvoiceLines.JI_ROOCert");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ROOCert)));
			this.RulesOfOriginCertificateTextBox.CaptionResourceString = null;
			this.RulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 201, true);
			this.RulesOfOriginCertificateTextBox.Name = "RulesOfOriginCertificateTextBox";
			this.RulesOfOriginCertificateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.RulesOfOriginCertificateTextBox.TabIndex = 12;
			// 
			// TradeStatisticsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TradeStatisticsCheckBox, "FilteredInvoiceLines.JI_TakeUpInTradeStatistics");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TakeUpInTradeStatistics)));
			this.TradeStatisticsCheckBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("{7FFE7D5C-1584-4582-940A-6795A674D438}", "Trade Statistics");
			this.TradeStatisticsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 139, true);
			this.TradeStatisticsCheckBox.Name = "TradeStatisticsCheckBox";
			this.TradeStatisticsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.TradeStatisticsCheckBox.TabIndex = 20;
			// 
			// TradeAgreementLabel
			// 
			this.BindingSource.SetBindingMember(this.TradeAgreementLabel, "FilteredInvoiceLines.TradeAgreementLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TradeAgreementLabel)));
			this.TradeAgreementLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TradeAgreementLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 181, true);
			this.TradeAgreementLabel.Name = "TradeAgreementLabel";
			this.TradeAgreementLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 18, true);
			this.TradeAgreementLabel.TabIndex = 24;
			// 
			// PreviousMRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousMRNTextBox, "FilteredInvoiceLines.JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryNumber)));
			this.PreviousMRNTextBox.CaptionResourceString = null;
			this.PreviousMRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 96, true);
			this.PreviousMRNTextBox.Name = "PreviousMRNTextBox";
			this.PreviousMRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.PreviousMRNTextBox.TabIndex = 17;
			// 
			// ImportBOELineCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ImportBOELineCalcEdit, "FilteredInvoiceLines.JI_PreviousEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryLineNumber)));
			this.ImportBOELineCalcEdit.CaptionResourceString = null;
			this.ImportBOELineCalcEdit.DecimalPlaces = 0;
			this.ImportBOELineCalcEdit.Decimals = 0;
			this.ImportBOELineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 117, true);
			this.ImportBOELineCalcEdit.Name = "ImportBOELineCalcEdit";
			this.ImportBOELineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.ImportBOELineCalcEdit.TabIndex = 18;
			this.ImportBOELineCalcEdit.Text = "0";
			this.ImportBOELineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsValueOverrideCalcDropEdit
			// 
			this.CustomsValueOverrideCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsValueOverrideCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsValueOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RX_NKCustomsValueCurrencyOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobComInvoiceLine)(((System.Collections.IList)(((JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsValueCurrencyOverrideList)));
			this.CustomsValueOverrideCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsValueOverride";
			this.CustomsValueOverrideCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups.CustomsValueCurrencyOverrideList";
			this.CustomsValueOverrideCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_RX_NKCustomsValueCurrencyOverride";
			this.CustomsValueOverrideCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(634, 203, true);
			this.CustomsValueOverrideCalcDropEdit.Name = "CustomsValueOverrideCalcDropEdit";
			this.CustomsValueOverrideCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.CustomsValueOverrideCalcDropEdit.TabIndex = 23;
			// 
			// InvoiceLinesUserControl
			// 
			this.Name = "InvoiceLinesUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1207, 656, true);
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
			this.tariffFindBox.ResumeLayout(true);
			this.tariffFindBox.PerformLayout();
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.ResumeLayout(true);
			this.JI_Calc_ATVConvertToLocalCurrencyControl1.PerformLayout();
			this.JI_Calc_ActualPriceCurrencyControl.ResumeLayout(true);
			this.JI_Calc_ActualPriceCurrencyControl.PerformLayout();
			this.MiscTabPage.ResumeLayout(false);
			this.MiscTabPage.PerformLayout();
			this.ImportBOEGroupBox.ResumeLayout(false);
			this.ImportBOEGroupBox.PerformLayout();
			this.DA63UserControl.ResumeLayout(true);
			this.DA63UserControl.PerformLayout();
			this.classificationDetailsUserControl.ResumeLayout(true);
			this.classificationDetailsUserControl.PerformLayout();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.DiamondProcessingTabPage.ResumeLayout(false);
			this.DiamondProcessingTabPage.PerformLayout();
			this.diamondProcessingGroupBox.ResumeLayout(false);
			this.diamondProcessingGroupBox.PerformLayout();
			this.ProductCodeFindBox.ResumeLayout(true);
			this.ProductCodeFindBox.PerformLayout();
			this.jI_CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.jI_CustomsQuantityCalcDropEdit.PerformLayout();
			this.JI_ProcedureFindBox.ResumeLayout(true);
			this.JI_ProcedureFindBox.PerformLayout();
			this.JI_CEIGuidDropEdit.ResumeLayout(true);
			this.JI_CEIGuidDropEdit.PerformLayout();
			this.PermitNumberCodeFindBox.ResumeLayout(true);
			this.PermitNumberCodeFindBox.PerformLayout();
			this.ROOTypeDropEdit.ResumeLayout(true);
			this.ROOTypeDropEdit.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.CustomsValueOverrideCalcDropEdit.ResumeLayout(true);
			this.CustomsValueOverrideCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcEdit.PerformLayout();
			this.InvoiceQuantityUNE20CodeFindBox.ResumeLayout(true);
			this.InvoiceQuantityUNE20CodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
