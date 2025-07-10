using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WhsInvoicePrintTask : InvoicePrintTask
	{
		public WhsInvoicePrintTask(InvoicingBase[] invoices, BusinessObjectFactory factory = null)
			: base(new Configuration(invoices) { Factory = factory })
		{
		}

		#region Extra Documents - Invoice Detail Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void AddExtraDocumentsToPack(InvoicingBase invoice, DocumentPack pack)
		{
			base.AddExtraDocumentsToPack(invoice, pack);

			DocumentCommand invoiceDetailCommand = null;

			if (invoice.Job != null)
			{
				invoiceDetailCommand = invoice.Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Invoice Detail Report"));

				if (invoiceDetailCommand != null)
				{
					pack.AddReportsToPack(invoiceDetailCommand, null, invoice, null);
				}
			}
		}

		#endregion
	}
}
