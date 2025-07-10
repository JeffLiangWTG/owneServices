using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class ZAInvoiceHeaderUserControl : DeclarationInvoiceHeaderUserControl
	{
		ZTextBox vDNTextBox;
		ZCalcEdit vBMPercentCalcEdit;
		ZCalcEdit conversionFactorCalcEdit;
		ZTextBox rulesOfOriginCertificateTextBox;
		ZCodeFindBox jZ_RN_NKDefaultOriginCodeFindBox;
		ZDropEdit relatedIndicatorDropEdit;
		ZDateEdit invDate1;
		ZDropEdit rOOTypeDropEdit;
		ZDropEdit valuationCodeDropEdit;

		public ZAInvoiceHeaderUserControl()
		{
			InitializeComponent();
			var invoiceDetailsEnabled = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today);
			JZ_PaymentTermsDropEdit.Visible = invoiceDetailsEnabled;
			AddGridColumns(invoiceDetailsEnabled);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayout7zaGfUuf5rM2V1f8hiQdQg=="; //SupressCodeSmell Reason = Data Import Wizard GridId.

			ApportionmentPendingLabel.AllowOverlap(conversionFactorCalcEdit);
		}

		void AddGridColumns(bool invoiceDetailsEnabled)
		{
			if (invoiceDetailsEnabled)
			{
				var paymentTermsColumnStyleInfo = new ZDropEditColumnStyleInfo();
				paymentTermsColumnStyleInfo.ColumnName = JobComInvoiceHeader.Schema.JZ_PaymentTerms;
				paymentTermsColumnStyleInfo.IsVisible = true;
				paymentTermsColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
				var index = JobComInvoiceHeadersBoundGrid.ColumnStyles.IndexOf(JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentDate));
				index = index < 0 ? JobComInvoiceHeadersBoundGrid.ColumnStyles.Count : index + 1;
				JobComInvoiceHeadersBoundGrid.ColumnStyles.Insert(index, paymentTermsColumnStyleInfo);
			}

			var valuationCodeColumnStyleInfo = new ZDropEditColumnStyleInfo();
			valuationCodeColumnStyleInfo.Caption = "Valuation Code";
			valuationCodeColumnStyleInfo.ColumnName = "JZ_ValuationCode";
			valuationCodeColumnStyleInfo.IsVisible = false;
			valuationCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(valuationCodeColumnStyleInfo);

			var relatIndicColumnStyleInfo = new ZDropEditColumnStyleInfo();
			relatIndicColumnStyleInfo.Caption = "Relationship Indicator";
			relatIndicColumnStyleInfo.ColumnName = "JZ_RelatedIndicator";
			relatIndicColumnStyleInfo.IsVisible = false;
			relatIndicColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(relatIndicColumnStyleInfo);

			var vDNNumberColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			vDNNumberColumnStyleInfo.Caption = "VDN Number";
			vDNNumberColumnStyleInfo.ColumnName = "JZ_VDN";
			vDNNumberColumnStyleInfo.IsVisible = false;
			vDNNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(vDNNumberColumnStyleInfo);

			var markupPercentCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			markupPercentCalcEditColumnStyleInfo.Decimals = 2;
			markupPercentCalcEditColumnStyleInfo.Caption = "Valuation Basis Markup %";
			markupPercentCalcEditColumnStyleInfo.ColumnName = "JZ_ValuationMarkup";
			markupPercentCalcEditColumnStyleInfo.IsVisible = false;
			markupPercentCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(markupPercentCalcEditColumnStyleInfo);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			InvoiceChargesGrid.RemoveFromAvailableColumns(Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			BaseGroupChargesGrid.RemoveFromAvailableColumns(Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			ApportionedChargesGrid.RemoveFromAvailableColumns(Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.JZ_OH_Buyer);
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			bool isImport = JobDeclaration.IsImport;
			var isImportShipmentType = JobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import;
			valuationCodeDropEdit.Visible = isImportShipmentType;
			relatedIndicatorDropEdit.Visible = isImportShipmentType;
			vDNTextBox.Visible = isImportShipmentType;
			vBMPercentCalcEdit.Visible = isImportShipmentType;
			rOOTypeDropEdit.Visible = !isImport;

			if (JobDeclaration.IsExport)
			{
				this.rulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 201, true);
			}
			else
			{
				this.rulesOfOriginCertificateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 201, true);
			}

			InvoiceChargesGrid.SetAvailability(isImport, Customs.Business.BaseJobComInvHeaderCharge.Schema.J7_IsDutiable);
			BaseGroupChargesGrid.SetAvailability(isImport, Customs.Business.BaseJobComInvHeaderCharge.Schema.J7_IsDutiable);
			ApportionedChargesGrid.SetAvailability(isImport, Customs.Business.BaseJobComInvHeaderCharge.Schema.J7_IsDutiable);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.IsImport, JobComInvoiceHeader.Schema.JZ_ValuationCode);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.IsImport, JobComInvoiceHeader.Schema.JZ_RelatedIndicator);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.IsImport, JobComInvoiceHeader.Schema.JZ_VDN);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetAvailability(JobDeclaration.IsImport, JobComInvoiceHeader.Schema.JZ_ValuationMarkup);
			JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, JobComInvoiceHeader.Schema.JZ_PaymentNo);
		}
	}
}

