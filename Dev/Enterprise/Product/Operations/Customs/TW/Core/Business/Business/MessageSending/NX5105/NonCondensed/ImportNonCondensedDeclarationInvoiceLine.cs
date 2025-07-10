using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ImportNonCondensedDeclarationInvoiceLine : NX5105Commodity_InvoiceLine
	{
		public ImportNonCondensedDeclarationInvoiceLine(CusEntryLine entryLine, JobComInvoiceLine invoiceLine) : base(entryLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
			invoiceHeader = invoiceLine.InvoiceHeader;
		}
		readonly JobComInvoiceLine invoiceLine;
		readonly JobComInvoiceHeader invoiceHeader;

		public override ZString CurrencyTypeCode => invoiceHeader?.EffectiveCurrencyCode ?? ZString.Empty;

		public override ZDecimal UnitPriceAmount => invoiceLine.JI_EnteredUnitPrice;
	}
}
