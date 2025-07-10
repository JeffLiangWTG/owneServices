using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.ColumnItemTemplates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CustomsInvoiceColumnProvider : GridColumnProvider
	{
		public CustomsInvoiceColumnProvider(ZPage page)
		{
			this.page = page;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceHeader)null).JZ_InvoiceNumber);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceHeader)null).SupplierName);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceHeader)null).Importer_Effective.OH_FullName);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceHeader)null).JZ_IncoTerm);
			ZBindToChecker.CheckBindTo((ZDecimal)((BaseJobComInvoiceHeader)null).JZ_InvoiceAmount);
			ZBindToChecker.CheckBindTo((ZString)((BaseJobComInvoiceHeader)null).Invoice_Currency.RX_Code);
			ZBindToChecker.CheckBindTo((ZDateTime)((BaseJobComInvoiceHeader)null).JZ_InvoiceDate);
			ZBindToChecker.CheckBindTo((BaseJobComInvoiceLineViewCollection)((BaseJobComInvoiceHeader)null).InvoiceLines);

			var linesColumn = new ZNewRowColumn("InvoiceLines") { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceLines };
			linesColumn.ItemTemplate = new InvoiceLineTemplate(this.Page, linesColumn, UserCountryCode);
			linesColumn.Collapsable = true;

			AddToDictionaryAsDefault(linesColumn);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("cef793ed-a138-4178-b06b-44a3f3617ccd", "Inv.Number"), BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceNumber, ReadOnly = true });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f4cd05c7-c26d-4b78-87db-6948675dcdce", "Supplier"), "SupplierName") { ColumnKey = WebTracker.Grids.CustomsInvoice.SupplierName, ReadOnly = true });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("490b2999-571c-48ed-bf99-4b17aba1d66d", "Importer"), "Importer_Effective+OH_FullName") { ColumnKey = WebTracker.Grids.CustomsInvoice.ImporterName, ReadOnly = true });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2ef89e0e-66d4-484b-93f7-5feeec5151f0", "Inv. Terms"), BaseJobComInvoiceHeader.Schema.JZ_IncoTerm) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceTerms, ReadOnly = true });
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("54e287c2-69b4-4feb-830a-309def6d63ec", "Total Amount"), BaseJobComInvoiceHeader.Schema.JZ_InvoiceAmount) { ColumnKey = WebTracker.Grids.CustomsInvoice.TotalAmount, ReadOnly = true, Decimals = 2 });
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("503ddce8-3542-4d49-8c63-e010929d5102", "Curr"), "Invoice_Currency+RX_Code") { ColumnKey = WebTracker.Grids.CustomsInvoice.Currency, ReadOnly = true });
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("7a0de763-8a5f-4df9-b126-cfe8818b5f96", "Inv.Date"), BaseJobComInvoiceHeader.Schema.JZ_InvoiceDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceDate });
		}

		OrgContactWebUser CurrentUser
		{
			get { return Page != null ? (OrgContactWebUser)Page.SiteUser : null; }
		}

		public ZString UserCountryCode
		{
			get
			{
				if (CurrentUser != null)
				{
					return CurrentUser.GetCountryCode();
				}
				return ZString.Empty;
			}
		}

		public ZPage Page
		{
			get { return page; }
		}
		readonly ZPage page;
	}
}
