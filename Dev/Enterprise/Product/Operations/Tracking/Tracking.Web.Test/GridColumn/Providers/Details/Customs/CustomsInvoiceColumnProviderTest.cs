using Enterprise.Customs.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.ColumnItemTemplates;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CustomsInvoiceColumnProvider))]
	sealed class CustomsInvoiceColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			var linesColumn = new ZNewRowColumn("InvoiceLines") { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceLines };
			linesColumn.ItemTemplate = new InvoiceLineTemplate(TestProvider.Page, linesColumn, TestProvider.UserCountryCode);
			linesColumn.Collapsable = true;

			AddDefaultsColumn(linesColumn);
			AddDefaultsColumn(new ZTextEditColumn("Inv.Number", BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceNumber, ReadOnly = true });
			AddDefaultsColumn(new ZTextEditColumn("Supplier", "SupplierName") { ColumnKey = WebTracker.Grids.CustomsInvoice.SupplierName, ReadOnly = true }); // This is a column name
			AddDefaultsColumn(new ZTextEditColumn("Importer", "Importer_Effective+OH_FullName") { ColumnKey = WebTracker.Grids.CustomsInvoice.ImporterName, ReadOnly = true });
			AddDefaultsColumn(new ZTextEditColumn("Inv. Terms", BaseJobComInvoiceHeader.Schema.JZ_IncoTerm) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceTerms, ReadOnly = true });
			AddDefaultsColumn(new ZCalcEditColumn("Total Amount", BaseJobComInvoiceHeader.Schema.JZ_InvoiceAmount) { ColumnKey = WebTracker.Grids.CustomsInvoice.TotalAmount, ReadOnly = true, Decimals = 2 });
			AddDefaultsColumn(new ZTextEditColumn("Curr", "Invoice_Currency+RX_Code") { ColumnKey = WebTracker.Grids.CustomsInvoice.Currency, ReadOnly = true });
			AddDefaultsColumn(new ZDateTimeColumn("Inv.Date", BaseJobComInvoiceHeader.Schema.JZ_InvoiceDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CustomsInvoice.InvoiceDate });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new CustomsInvoiceColumnProvider(Page);
		}

		new CustomsInvoiceColumnProvider TestProvider
		{
			get { return (CustomsInvoiceColumnProvider)base.TestProvider; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			Page = new ZTestPage();
			SetupNewProvider();
		}

		ZPage Page
		{
			get { return page; }
			set { page = value; }
		}
		ZPage page;
	}
}
