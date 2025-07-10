using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

public class CommercialInvoiceForm : EU.GUI.CommercialInvoice.CommercialInvoiceForm
{
	public CommercialInvoiceForm(JobComInvoiceHeader invoiceHeader)
		: base(invoiceHeader)
	{
	}

	protected override Customs.GUI.CommonInvoiceHeaderUserControl GetHeaderUserControl() => new InvoiceHeaderUserControl();

	protected override EUInvoiceLineUserControl GetImportInvoiceLineUserControl() => new ImportInvoiceLineUserControl();

	protected override EUInvoiceLineUserControl GetExportInvoiceLineUserControl() => new ExportInvoiceLineUserControl();
}
