using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class USExportSupplierHeaderUserControl : USCustomsSupplierHeaderUserControl
	{
		public USExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(InvoiceHeadersGridColumnNamesInSortOrder);
				ModifyInvoiceHeadersGridVisibility();
			}

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutZIkHUoOCsN/evw2qbJLNpg==";
			this.DDTCCategoryXXIDeterminationNumberTextBox.Visible = ZZCustomsFunctionality.IsAESJurisdictionNumberEffective;

			USPPIEINLabel.BringToFront();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			ECCNCodeFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			ECCNTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);

			if (dataSource != null)
			{
				ECCNCodeFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "Invoices.ECCNCodeFindBoxVisible", false, DataSourceUpdateMode.Never));
				ECCNTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, "Invoices.ECCNTextBoxVisible", false, DataSourceUpdateMode.Never));
			}
		}

		const string IsVisibleForBindingString = "IsVisibleForBinding";

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (CurrentDataItem != null)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				if (declaration != null)
				{
					declaration.US_SoldEnRouteIndicatorInfo.ValueChanged -= new EventHandler(US_SoldEnRouteIndicatorInfo_ValueChanged);
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				var declaration = (JobDeclaration)CurrentDataItem;
				if (declaration != null)
				{
					declaration.US_SoldEnRouteIndicatorInfo.ValueChanged += new EventHandler(US_SoldEnRouteIndicatorInfo_ValueChanged);
					InvoiceTabControl.SelectTab(SEDDetailsTabPage);
				}
			}
		}

		void US_SoldEnRouteIndicatorInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = (JobDeclaration)CurrentDataItem;
			if (declaration != null)
			{
				bool isSoldEnRoute = declaration.US_SoldEnRouteIndicator == YesNoDefaultList.Codes.Yes;
			}
		}

		string[] InvoiceHeadersGridColumnNamesInSortOrder
		{
			get
			{
				if (invoiceHeadersGridColumnNamesInSortOrder == null)
				{
					invoiceHeadersGridColumnNamesInSortOrder = new[]
					{
						JobComInvoiceHeader.Schema.JZ_OH_Supplier,
						JobComInvoiceHeader.Schema.JZ_OH_Buyer,
						JobComInvoiceHeader.Schema.US_UltimateDestinationCountry,
						"SupplierPickupAddress+OrganisationPK",
						JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
						JobComInvoiceHeader.Schema.JZ_IncoTerm,
						JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
						JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
						JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.InvoiceLineTotal,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
						"SupplierPickupAddress+E2_OA_Address",
						"USPPIDocAddress+E2_Contact",
						"USPPIDocAddress+E2_Phone_Formatted",
						"UltimateConsigneeDocAddress+E2_OA_Address",
						"UltimateConsigneeDocAddress+E2_Contact",
						"UltimateConsigneeDocAddress+E2_Phone_Formatted",
						JobComInvoiceHeader.Schema.US_UltimateConsigneeType,
						JobComInvoiceHeader.Schema.JZ_OH_Consignee,
						"IntermediateConsigneeDocAddress+E2_OA_Address",
						"IntermediateConsigneeDocAddress+E2_Contact",
						"IntermediateConsigneeDocAddress+E2_Phone_Formatted",
						JobComInvoiceHeader.Schema.US_StateOfOrigin,
						JobComInvoiceHeader.Schema.US_ForeignTradeZone,
						JobComInvoiceHeader.Schema.US_ECCN,
						JobComInvoiceHeader.Schema.US_RoutedTransaction,
						JobComInvoiceHeader.Schema.US_TransactionsRelated,
						JobComInvoiceHeader.Schema.US_LicenseType,
						JobComInvoiceHeader.Schema.US_LicenseNo,
						JobComInvoiceHeader.Schema.US_InbondType,
						JobComInvoiceHeader.Schema.US_ImportEntryNo,
						JobComInvoiceHeader.Schema.US_HazardousCargo,
						JobComInvoiceHeader.Schema.US_ExportCode,
						JobComInvoiceHeader.Schema.US_EntryNumber,
						JobComInvoiceHeader.Schema.US_XTN,
						JobComInvoiceHeader.Schema.US_DateOfExport
					};
				}
				return invoiceHeadersGridColumnNamesInSortOrder;
			}
		}
		string[] invoiceHeadersGridColumnNamesInSortOrder;

		void ModifyInvoiceHeadersGridVisibility()
		{
			ZGrid invoiceHeadersGrid = JobComInvoiceHeadersBoundGrid.InnerGrid;
			invoiceHeadersGrid.SetColumnVisible(true,
				[
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm,
					JobComInvoiceHeaderSchema.Constants.JZ_IncoTermPlace,
					JobComInvoiceHeader.Schema.InvoiceLineTotal,
					JobComInvoiceHeaderSchema.Constants.JZ_InvoiceDate,
					JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer,
					JobComInvoiceHeader.Schema.US_UltimateDestinationCountry
				]);
			invoiceHeadersGrid.SetColumnCaption(JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier, "USPPI");
			invoiceHeadersGrid.SetColumnGroupName(JobComInvoiceHeaderSchema.Constants.JZ_OH_Supplier, Enterprise.Customs.US.GUI.Res.GetData("ExportSupplierHeader|aabb7e43-b9e4-4daa-aa49-a0c94cb08cad", "USPPI"));
			invoiceHeadersGrid.SetColumnCaption(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer, "Ult. Cnee.");
			invoiceHeadersGrid.SetColumnGroupName(JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer, Enterprise.Customs.US.GUI.Res.GetData("ExportSupplierHeader|b6488fa6-6296-4a7e-ab35-127e12aa805a", "Ult. Cnee."));
			invoiceHeadersGrid.SetColumnCaption("SupplierPickupAddress+OrganisationPK", "Pickup");
			invoiceHeadersGrid.SetColumnGroupName("SupplierPickupAddress+OrganisationPK", Enterprise.Customs.US.GUI.Res.GetData("ExportSupplierHeader|6D2BB834-BF9E-4ABA-BB04-5814A229AC36", "Pickup"));
		}
	}
}
