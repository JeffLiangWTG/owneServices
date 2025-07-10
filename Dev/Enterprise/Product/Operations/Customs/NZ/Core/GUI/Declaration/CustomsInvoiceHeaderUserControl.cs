using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsInvoiceHeaderUserControl : DeclarationInvoiceHeaderUserControl
	{
		public CustomsInvoiceHeaderUserControl()
		{
			InitializeComponent();
			SetupNonDesignerableItems();
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutYmlykF3Qr3jStD2WohDXUQ=="; // SupressCodeSmell Reason = Data Import Wizard GridId.
		}

		public override Customs.Business.BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				if (JobDeclaration != null)
				{
					JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.CountChanged -= new CollectionCountChangedEventHandler(JobComInvoiceGroupHeaders_CountChanged);
				}
				base.JobDeclaration = value;
				if (JobDeclaration != null && JobDeclaration.JobComInvoiceGroupHeaders.Count > 0)
				{
					ChangeDeclarationChargesGridVisibility();
					JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.CountChanged += new CollectionCountChangedEventHandler(JobComInvoiceGroupHeaders_CountChanged);
				}
			}
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();

			bool isExport = JobDeclaration.IsExport;
			bool isImport = JobDeclaration.IsImport;
			var declaration = (JobDeclaration)JobDeclaration;
			bool isNonECIExport = declaration != null && declaration.IsTSWDeclaration && declaration.IsExport && !declaration.IsECIWriteoff;

			JZ_ExchangeRateIndicatorDropEdit.Visible = isExport;
			JZ_RelationshipIndicatorDropEdit.Visible = isImport;
			PreferenceDropEdit.Visible = isImport;
			PreferentialCountryGroupDropEdit.Visible = isImport;
			CountryOfExportCodeFindBox.Visible = isImport;
			SupplierOrganisationControl.Visible = isImport;
			OverseasRegisteredSupplierGroupBox.Visible = isImport;
			ImporterOrganisationControl.Visible = isExport;

			ZeroRatedGroupBox.Visible = !isExport;
			OtherTabPage.TabVisible = !isNonECIExport;
			OriginFindBox.Visible = isNonECIExport;

			OriginRegionTextBoxForTSW.Visible = isNonECIExport;
			OriginRegionTextBox.Visible = !(declaration != null && !declaration.IsTSWDeclaration && isExport);
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			JobDeclaration declaration = (JobDeclaration)JobDeclaration;
			bool isTSWExportDec = declaration.IsTSWDeclaration && declaration.IsExport;

			ZGridWithoutColumnStylesSerialisation invoiceHeadersGrid = JobComInvoiceHeadersBoundGrid.InnerGrid;
			invoiceHeadersGrid.SetAvailability(declaration.IsExport, JobComInvoiceHeader.Schema.JZ_ExchangeRateIndicator);
			invoiceHeadersGrid.SetColumnMandatory(JobComInvoiceHeader.Schema.JZ_InvoiceDate, isTSWExportDec);
			invoiceHeadersGrid.SetColumnMandatory(JobComInvoiceHeader.Schema.JZ_OH_Buyer, isTSWExportDec);

			invoiceHeadersGrid.SetAvailability(JobDeclaration.IsImport,
				[
					JobComInvoiceHeader.Schema.JZ_RelationshipIndicator,
					JobComInvoiceHeader.Schema.JZ_DefaultQualifiesForPreferentialDuty,
					JobComInvoiceHeader.Schema.JZ_RN_NKDefaultExport
				]);

			invoiceHeadersGrid.SetColumnVisible(declaration.IsSimplified, JobComInvoiceHeader.Schema.SupplierName);
		}

		protected override void Dispose(bool disposing)
		{
			if (JobDeclaration != null && JobDeclaration.JobComInvoiceGroupHeaders.Count > 0)
			{
				JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.CountChanged -= new CollectionCountChangedEventHandler(JobComInvoiceGroupHeaders_CountChanged);
			}
			base.Dispose(disposing);
		}

		void JobComInvoiceGroupHeaders_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			ChangeDeclarationChargesGridVisibility();
		}

		void ChangeDeclarationChargesGridVisibility()
		{
			if (JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.Count > 0)
			{
				BaseGroupChargesGrid.Visible = false;
				DeclarationChargesNotAvailableLabel.Visible = true;
			}
			else
			{
				BaseGroupChargesGrid.Visible = true;
				DeclarationChargesNotAvailableLabel.Visible = false;
			}
		}

		void SetupNonDesignerableItems()
		{
			ZGridWithoutColumnStylesSerialisation invoiceHeadersGrid = JobComInvoiceHeadersBoundGrid.InnerGrid;
			invoiceHeadersGrid.ReOrderColumns(
				[
					JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
					JobComInvoiceHeader.Schema.JZ_OH_Supplier,
					JobComInvoiceHeader.Schema.SupplierName,
					JobComInvoiceHeader.Schema.JZ_OH_Buyer
				]);
			ZGridColumnInfo supplierNameColumn = invoiceHeadersGrid.GetColumnStyle(JobComInvoiceHeader.Schema.SupplierName);
			supplierNameColumn.IsReadOnly = false;
			ControlDpiScalingHelper.SetWidth(ref supplierNameColumn, 160, true);
		}
	}
}
